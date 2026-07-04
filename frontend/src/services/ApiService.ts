import {AuthService} from './AuthService';
import MessageService from './MessageService';

/**
 * 标准化API响应接口
 */
export interface ApiResponse<T> {
    code: number;
    errorCode: number;
    message: string;
    data: T;
}

/**
 * API请求配置
 */
export interface ApiRequestConfig extends Omit<RequestInit, 'body'> {
    url: string;
    requiresAuth?: boolean;
    body?: any;

    showMessage?: boolean;
    showSuccess?: boolean;
    showError?: boolean;
}

// 正在进行的刷新 Promise，用于并发请求去重
let refreshPromise: Promise<void> | null = null;

/**
 * 确保 Token 有效——如果即将过期则主动刷新。
 * 同时通过共享 Promise 实现并发刷新去重。
 */
async function ensureValidToken(): Promise<void> {
    const token = AuthService.getToken();
    if (!token) return;

    const userInfo = AuthService.parseJwtToken(token);
    if (!userInfo?.exp) return;

    const nowSeconds = Math.floor(Date.now() / 1000);
    // Token 在未来 60 秒内过期，主动刷新
    if (userInfo.exp - nowSeconds > 60) return;

    if (!refreshPromise) {
        refreshPromise = AuthService.refreshToken().finally(() => {
            refreshPromise = null;
        });
    }

    await refreshPromise;
}

/**
 * 通用API请求处理函数
 * @param config 请求配置
 * @returns Promise<T> 响应数据
 */
export async function apiRequest<T>(config: ApiRequestConfig): Promise<T> {
    const {url, headers = {}, body, ...rest} = config;

    // 添加默认请求头
    const requestHeaders: Record<string, string> = {
        'Content-Type': 'application/json',
        ...((headers as Record<string, string>) || {})
    };

    // 如果需要认证且有令牌，添加Authorization头前先确保 Token 有效（主动刷新 + 并发去重）
    const token = AuthService.getToken();
    if (token) {
        await ensureValidToken();
        // 刷新后重新读取 Token（可能已被 ensureValidToken 更新）
        const validToken = AuthService.getToken();
        if (validToken) {
            requestHeaders['Authorization'] = `Bearer ${validToken}`;
        }
    }

    // 处理请求体：如果是对象且Content-Type为application/json，则转换为JSON字符串
    let requestBody: BodyInit | null | undefined = body;
    const contentType = requestHeaders['Content-Type'];
    if (body && typeof body === 'object' && contentType === 'application/json') {
        requestBody = JSON.stringify(body);
    }

    // 解析响应
    let apiResponse: ApiResponse<T>;
    let response: Response | null = null;
    
    try {
        response = await fetch(url, {
            headers: requestHeaders,
            body: requestBody,
            ...rest
        });

        // 检查响应是否成功
        if (!response.ok) {
            // 如果是401错误，创建相应的错误响应对象
            apiResponse = {
                code: response.status,
                errorCode: response.status === 401 ? 3001 : 5000,
                message: response.statusText || '请求失败',
                data: null!
            };
        } else {
            apiResponse = await response.json();
        }
    } catch (reason: any) {
        // 网络错误时检查是否可能是401导致的连接问题
        const isNetworkError = reason instanceof TypeError && reason.message.includes('fetch');
        
        apiResponse = {
            code: response?.status || reason.code || 0,
            errorCode: (response?.status === 401 || isNetworkError) ? 3001 : 5000,
            message: reason.message || '网络连接异常',
            data: null!
        };
    }

        if (config.showMessage || config.showError) {
            MessageService.handleResponse(apiResponse, config);
        }

        if (apiResponse.code !== 200) {
        // 根据HTTP状态码处理特殊情况
        // 当code为401或者errorCode为3001时触发令牌刷新逻辑
        if (apiResponse.code === 401 || apiResponse.errorCode === 3001) {
            try {
                // 使用共享 Promise 刷新令牌，避免并发 401 触发多次刷新
                if (!refreshPromise) {
                    refreshPromise = AuthService.refreshToken().finally(() => {
                        refreshPromise = null;
                    });
                }
                await refreshPromise;

                // 更新请求头中的令牌
                const newToken = AuthService.getToken();
                if (newToken) {
                    requestHeaders['Authorization'] = `Bearer ${newToken}`;

                    // 重新发送请求
                    let retryResponse = await fetch(url, {
                        headers: requestHeaders,
                        body: requestBody,
                        ...rest
                    });

                    // 重新解析响应
                    if (!retryResponse.ok) {
                        apiResponse = {
                            code: retryResponse.status,
                            errorCode: retryResponse.status === 401 ? 3001 : 5000,
                            message: retryResponse.statusText || '请求失败',
                            data: null!
                        };
                    } else {
                        apiResponse = await retryResponse.json();
                    }

            // 如果重新请求成功，返回数据
            if (apiResponse.code === 200) {
                if (config.showMessage || config.showSuccess) {
                    MessageService.handleResponse(apiResponse, config);
                }
                return apiResponse.data;
            }
                    
                    // 如果重新请求还是失败，继续执行下面的错误处理逻辑
                }
            } catch (refreshError) {
                // 刷新令牌失败，清除令牌并抛出错误
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            
            // 如果令牌刷新成功但重新请求仍然失败，继续执行下面的错误处理逻辑
        }

        // 根据业务错误码处理
        switch (apiResponse.errorCode) {
            case 1000: // 参数不能为空
            case 1001: // 参数格式错误
            case 1002: // 参数超出范围
            case 1003: // 参数验证失败
                throw new Error(apiResponse.message);
            case 2000: // 资源已存在
            case 2001: // 操作失败
            case 2002: // 数据处理失败
            case 2003: // 状态不允许该操作
                throw new Error(apiResponse.message);
            case 3000: // 未授权访问
            case 3001: // 权限不足
            case 401: // HTTP 401 未授权
                AuthService.clearToken();
                throw new Error(apiResponse.message || '登录已过期，请重新登录');
            case 3002: // 登录已过期
                AuthService.clearToken();
                throw new Error(apiResponse.message);
            case 3003: // 无效的令牌
                AuthService.clearToken();
                throw new Error(apiResponse.message);
            case 4000: // 资源不存在
            case 4001: // 文章不存在
            case 4002: // 分类不存在
            case 4003: // 用户不存在
            case 4004: // 项目不存在
            case 4005: // 文件不存在
                throw new Error(apiResponse.message);
            case 5000: // 内部服务器错误
            case 5001: // 数据库操作失败
            case 5002: // 缓存操作失败
            case 5003: // 网络错误
                throw new Error(apiResponse.message || '服务器内部错误');
            case 6000: // 外部服务调用失败
            case 6001: // 外部服务超时
            case 6002: // 外部服务返回错误
            case 6003: // 外部服务未配置
                throw new Error(apiResponse.message);
            case 7000: // 请求频率过高
                throw new Error(apiResponse.message || '请求频率过高，请稍后再试');
            case 7001: // 无效请求
                throw new Error(apiResponse.message);
            default:
                throw new Error(apiResponse.message || '请求失败');
        }
    }

    if (config.showMessage || config.showSuccess) {
        MessageService.handleResponse(apiResponse, config);
    }

    return apiResponse.data;
}

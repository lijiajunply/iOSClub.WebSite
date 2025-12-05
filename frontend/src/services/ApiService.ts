import {AuthService} from './AuthService';

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
    body?: any; // 允许任何类型的body，在函数内部会处理转换
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

    // 如果需要认证且有令牌，添加Authorization头
    const token = AuthService.getToken();
    if (token) {
        requestHeaders['Authorization'] = `Bearer ${token}`;
    }

    // 处理请求体：如果是对象且Content-Type为application/json，则转换为JSON字符串
    let requestBody: BodyInit | null | undefined = body;
    const contentType = requestHeaders['Content-Type'];
    if (body && typeof body === 'object' && contentType === 'application/json') {
        requestBody = JSON.stringify(body);
    }

    // 解析响应
    let apiResponse: ApiResponse<T>;
    try {
        let response = await fetch(url, {
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
        apiResponse = {
            code: reason.code || 500,
            errorCode: 5000,
            message: reason.message,
            data: null!
        };
    }

    // 处理不同的错误情况
    if (apiResponse.code !== 200) {
        // 根据HTTP状态码处理特殊情况
        if (apiResponse.code === 401 || apiResponse.errorCode === 3001) {
            try {
                // 尝试使用刷新令牌获取新的访问令牌
                await AuthService.refreshToken();

                // 更新请求头中的令牌
                const newToken = AuthService.getToken();
                if (newToken) {
                    requestHeaders['Authorization'] = `Bearer ${newToken}`;

                    // 重新发送请求
                    let response = await fetch(url, {
                        headers: requestHeaders,
                        body: requestBody,
                        ...rest
                    });

                    // 重新解析响应
                    apiResponse = await response.json();

                    // 如果重新请求成功，返回数据
                    if (apiResponse.code === 200) {
                        return apiResponse.data;
                    }
                }
            } catch (refreshError) {
                // 刷新令牌失败，清除令牌并抛出错误
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
        }

        // 如果重新请求失败，根据业务错误码处理
        switch (apiResponse.errorCode) {
            case 1001: // 参数不能为空
            case 1002: // 参数格式错误
            case 1003: // 参数验证失败
                throw new Error(apiResponse.message);
            case 2001: // 业务逻辑错误
                throw new Error(apiResponse.message);
            case 3001: // 未授权
            case 401: // HTTP 401 未授权
                AuthService.clearToken();
                throw new Error(apiResponse.message || '登录已过期，请重新登录');
            case 3002: // 权限不足
                throw new Error(apiResponse.message || '权限不足');
            case 4001: // 资源不存在
                throw new Error(apiResponse.message || '资源不存在');
            case 4002: // 资源已存在
                throw new Error(apiResponse.message || '资源已存在');
            case 5000: // 内部服务器错误
                throw new Error(apiResponse.message || '服务器内部错误');
            case 7000: // 请求频率过高
                throw new Error(apiResponse.message || '请求频率过高，请稍后再试');
            default:
                throw new Error(apiResponse.message || '请求失败');
        }
    }

    return apiResponse.data;
}

import {url} from './Url';
import type {StudentModel, LoginModel} from '../models';
import {apiRequest} from './ApiService';

/**
 * 密码哈希函数
 * @param password 原始密码
 * @returns Promise<string> 哈希后的密码
 */
export const hashPassword = async (password: string) => {
    const encoder = new TextEncoder()
    const data = encoder.encode(password)
    const hashBuffer = await crypto.subtle.digest('SHA-256', data)
    const hashArray = Array.from(new Uint8Array(hashBuffer))
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('')
}

/**
 * 认证服务类 - 处理登录注册相关的API调用
 */
export class AuthService {
    /**
     * 用户登录
     * @param loginModel 登录信息
     * @param clientId 客户端ID
     * @param scope 授权范围
     * @returns Promise<string> JWT令牌
     */
    static async login(loginModel: LoginModel, clientId: string | null | undefined = '', scope: string | null | undefined = ''): Promise<string> {
        return await apiRequest<string>({
            url: `${url}/Auth/login?clientId=${clientId}&scope=${scope}`,
            method: 'POST',
            body: JSON.stringify(loginModel),
            requiresAuth: false
        });
    }

    /**
     * 学生注册
     * @param model 学生注册信息
     * @returns Promise<string> JWT令牌
     */
    static async signup(model: StudentModel): Promise<string> {
        return await apiRequest<string>({
            url: `${url}/Auth/signup`,
            method: 'POST',
            body: JSON.stringify(model),
            requiresAuth: false
        });
    }

    static async logout(userId: string, clientId: string | null | undefined = ''): Promise<boolean> {
        try {
            await apiRequest<boolean>({
                url: `${url}/Auth/logout?userId=${userId}&clientId=${clientId}`,
                method: 'POST'
            });
            return true;
        } catch (error) {
            return false;
        }
    }

    static async validate(userId: string, token: string, clientId: string | null | undefined = ''): Promise<boolean> {
        try {
            await apiRequest<boolean>({
                url: `${url}/Auth/validate?userId=${userId}&clientId=${clientId}`,
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            return true;
        } catch (e) {
            console.error(e);
            return false;
        }
    }

    /**
     * 保存令牌到本地存储
     * @param token JWT令牌
     */
    static saveToken(token: string): void {
        localStorage.setItem('Authorization', token);
    }

    /**
     * 从本地存储获取令牌
     * @returns string | null JWT令牌或null
     */
    static getToken(): string | null {
        return localStorage.getItem('Authorization');
    }

    /**
     * 清除本地存储的令牌
     */
    static clearToken(): void {
        localStorage.removeItem('Authorization');
    }

    /**
     * 检查用户是否已登录
     * @returns boolean 是否已登录
     */
    static isLoggedIn(): boolean {
        return this.getToken() !== null;
    }

    /**
     * 解析JWT令牌并提取用户信息
     * @param token JWT令牌
     * @returns any 解析出的用户信息或null
     */
    static parseJwtToken(token: string | null): any {
        if (!token) return null;

        try {
            // JWT格式: header.payload.signature
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(
                window.atob(base64)
                    .split('')
                    .map(function (c) {
                        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                    })
                    .join('')
            );

            return JSON.parse(jsonPayload);
        } catch (e) {
            console.error('解析JWT令牌失败:', e);
            return null;
        }
    }

    /**
     * 获取当前登录用户信息
     * @returns any 用户信息或null
     */
    static getCurrentUserInfo(): any {
        const token = this.getToken();
        return this.parseJwtToken(token);
    }

    /**
     * 修改用户密码
     * @param userId 用户ID
     * @param oldPassword 旧密码
     * @param newPassword 新密码
     * @returns Promise<boolean> 是否修改成功
     */
    static async changePassword(userId: string, oldPassword: string, newPassword: string): Promise<boolean> {
        return await apiRequest<boolean>({
            url: `${url}/Auth/change-password?userId=${userId}&oldPassword=${oldPassword}&newPassword=${newPassword}`,
            method: 'PUT'
        });
    }

    /**
     * 请求重置密码的验证码
     * @param userId 用户ID
     * @returns Promise<boolean> 是否发送成功
     */
    static async requestPasswordReset(userId: string): Promise<boolean> {
        return await apiRequest<boolean>({
            url: `${url}/Auth/request-password-reset?userId=${userId}`,
            method: 'POST',
            requiresAuth: false
        });
    }

    /**
     * 通过验证码重置密码
     * @param userId 用户ID
     * @param code 验证码
     * @param newPassword 新密码
     * @returns Promise<boolean> 是否重置成功
     */
    static async resetPassword(userId: string, code: string, newPassword: string): Promise<boolean> {
        return await apiRequest<boolean>({
            url: `${url}/Auth/reset-password?userId=${userId}&code=${code}&newPassword=${newPassword}`,
            method: 'POST',
            requiresAuth: false
        });
    }

    /**
     * 从主站JWT获取SSO会话
     * @param clientId 客户端ID
     * @param scope 权限范围
     * @returns Promise<boolean> 是否成功
     */
    static async loginFromMainJwt(clientId: string, scope: string = 'profile openid role'): Promise<string> {
        const token = this.getToken();
        if (!token) {
            throw new Error('未登录或令牌已过期');
        }

        const response = await fetch(`${url}/SSO/from_main_jwt?scope=${encodeURIComponent(scope)}&client_id=${encodeURIComponent(clientId)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
        });

        if (!response.ok) {
            throw new Error('从主站JWT获取SSO会话失败');
        }

        return await response.text();
    }
}

// 组织注册记录类
export class OrgSignRecord {
    constructor(
        public readonly url1: string,
        public readonly url2: string
    ) {
    }
}

// iOS 俱乐部注册信息
export const ios = new OrgSignRecord(
    "mqqapi://card/show_pslcard?src_type=internal&version=1&uin=952954710&card_type=group&source=external",
    "https://qm.qq.com/cgi-bin/qm/qr?authKey=MUNgIj%2B1gnkiI175qAQla6EcR44Fa0APCv%2FLo1a7YIlYgpeg76Q%2BGYMoedb8gGHU&k=HvhhArSc7tzuySOhXsnmZ6RgLcWkzXgu&noverify=0"
);
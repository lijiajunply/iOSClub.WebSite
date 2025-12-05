import {defineStore} from 'pinia';
import {AuthService} from '../services/AuthService';
import type {LoginModel, StudentModel} from '../models';

export const useAuthorizationStore = defineStore('AuthorizationId', {
    state: () => ({
        Authorization: localStorage.getItem('accessToken') || ''
    }),
    getters: {
        getAuthorization: (state) => state.Authorization,
        isAuthenticated: (state) => !!state.Authorization && state.Authorization.length > 0,
        getAuthorizationInfo: (state): any => {
            let strings = state.Authorization.split('.'); // 确保是 JWT 格式
            if (strings.length !== 3) return null; // 简单校验是否为合法 JWT
            try {
                const payload = strings[1].replace(/-/g, '+').replace(/_/g, '/');
                const decodedPayload = atob(payload);
                const jsonPayload = decodeURIComponent(encodeURIComponent(decodedPayload));
                return JSON.parse(jsonPayload);
            } catch (e) {
                console.error('解析 Authorization 失败:', e);
                return null;
            }
        },
        getRole: (state): string | null => {
            let strings = state.Authorization.split('.'); // 确保是 JWT 格式
            if (strings.length !== 3) return null; // 简单校验是否为合法 JWT
            try {
                const payload = strings[1].replace(/-/g, '+').replace(/_/g, '/');
                const decodedPayload = atob(payload);
                const jsonPayload = decodeURIComponent(encodeURIComponent(decodedPayload));
                const json = JSON.parse(jsonPayload);
                return json['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            } catch (e) {
                console.error('解析 Authorization 失败:', e);
                return null;
            }
        },
    },
    actions: {
        async signup(stu: StudentModel): Promise<boolean> {
            const { accessToken } = await AuthService.signup(stu);
            if (!accessToken) return false;
            this.Authorization = accessToken;
            localStorage.setItem('UserId', stu.userId);
            return true;
        },
        async logout(clientId: string | null | undefined = '') {
            this.Authorization = '';
            const id = localStorage.getItem('UserId');
            if (id) {
                await AuthService.logout(id, clientId);
            }
            localStorage.removeItem('UserId');
        },
        async login(user: LoginModel, clientId: string | null | undefined = '', scope: string | null | undefined = ''): Promise<boolean> {
            try {
                const { accessToken } = await AuthService.login(user, clientId, scope);
                if (!accessToken) {
                    return false;
                }
                this.Authorization = accessToken;
                localStorage.setItem('UserId', user.userId);
                return true;
            } catch (e) {
                return false;
            }
        },
        async oauthLogin(user: LoginModel, clientId: string | null | undefined = '', scope: string | null | undefined = ''): Promise<string> {
            const { accessToken } = await AuthService.login(user, clientId, scope);
            this.Authorization = accessToken;
            localStorage.setItem('UserId', user.userId);
            return accessToken;
        },
        async validate(clientId: string | null | undefined = ''): Promise<boolean> {
            const id = localStorage.getItem('UserId');
            if (id === null || id === '') return false;
            return await AuthService.validate(id, this.Authorization, clientId);
        },
        async refreshToken(): Promise<boolean> {
            try {
                const newAccessToken = await AuthService.refreshToken();
                if (newAccessToken) {
                    this.Authorization = newAccessToken;
                    return true;
                }
                return false;
            } catch (e) {
                console.error('刷新令牌失败:', e);
                return false;
            }
        },
        isAdmin() {
            return this.getRole === 'Founder' || this.getRole === 'President' || this.getRole === 'Minister';
        }
    }
});
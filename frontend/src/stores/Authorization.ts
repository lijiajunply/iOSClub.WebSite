import {defineStore} from 'pinia';
import {AuthService} from '../services/AuthService';
import type {LoginModel, StudentModel} from '../models';

export const useAuthorizationStore = defineStore('AuthorizationId', {
    state: () => ({
        Authorization: localStorage.getItem('Authorization') || ''
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
            const result = await AuthService.signup(stu)
            if (result === '') return false;
            this.Authorization = result;
            localStorage.setItem('Authorization', result);
            localStorage.setItem('UserId', stu.userId);
            return true
        },
        logout() {
            this.Authorization = '';
            localStorage.removeItem('Authorization');
            localStorage.removeItem('UserId');
        },
        async login(user: LoginModel): Promise<boolean> {
            try {
                const a = await AuthService.login(user)
                if (!a) {
                    return false;
                }
                this.Authorization = a;
                localStorage.setItem('Authorization', a);
                localStorage.setItem('UserId', user.id);
                return true;
            } catch (e) {
                return false;
            }
        },
        async validate(): Promise<boolean> {
            const id = localStorage.getItem('UserId');
            if (id === null || id === '') return false;
            return await AuthService.validate(id, this.Authorization);
        },
        isAdmin() {
            return this.getRole === 'Admin';
        }
    }
});
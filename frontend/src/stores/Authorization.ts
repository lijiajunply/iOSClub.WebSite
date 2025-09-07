import {defineStore} from 'pinia';
import {LoginService} from "../services/LoginService.ts";

export const useAuthorizationStore = defineStore('AuthorizationId', {
    state: () => ({
        Authorization: localStorage.getItem('Authorization') || ''
    }),
    getters: {
        getAuthorization: (state) => state.Authorization,
        isAuthenticated: (state) => !!state.Authorization && state.Authorization.length > 0
    },
    actions: {
        // 修改token，并将token存入localStorage
        async login(user: any): Promise<boolean> {
            try {
                const result = await LoginService.login(user.username, user.password)
                if (!result || !result.token) {
                    return false;
                }
                this.Authorization = result.token;
                localStorage.setItem('Authorization', result.token);
                return true;
            } catch (e) {
                console.error('登录错误:', e);
                return false;
            }
        },
        logout() {
            this.Authorization = '';
            localStorage.removeItem('Authorization');
        }
    }
});
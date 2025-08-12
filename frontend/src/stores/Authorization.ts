import {defineStore} from 'pinia';
import {LoginService} from "../services/LoginService.ts";

export const useAuthorizationStore = defineStore('AuthorizationId', {
    state: () => ({
        Authorization: localStorage.getItem('Authorization') || ''
    }),
    getters: {
        getAuthorization: (state) => state.Authorization,
        isAuthenticated: (state) => !!state.Authorization
    },
    actions: {
        // 修改token，并将token存入localStorage
        async login(user: any): Promise<boolean> {
            try {
                const a = await LoginService.login(user.username, user.password)
                if (!a) {
                    return false;
                }
                this.Authorization = a.token;
                localStorage.setItem('Authorization', a.token);
                return true;
            } catch (e) {
                return false;
            }
        },
        logout() {
            this.Authorization = '';
            localStorage.removeItem('Authorization');
        }
    }
});
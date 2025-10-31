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
        logout() {
            this.Authorization = '';
            localStorage.removeItem('Authorization');
        },
        setAuthorization(token: string) {
            this.Authorization = token;
            localStorage.setItem('Authorization', token);
        }
    }
});
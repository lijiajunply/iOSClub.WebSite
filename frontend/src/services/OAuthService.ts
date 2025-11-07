import {url} from './Url';

export class OAuthService {
    static async loadClientAppInfo(clientId: string): Promise<any> {
        if (!clientId) return null;

        try {
            const response = await fetch(`${url}/SSO/client-info?clientId=${clientId}`);
            if (response.ok) {
                return await response.json();
            }
            return null;
        } catch (error) {
            console.error('加载客户端应用信息失败:', error);
            return null;
        }
    }

    static async storeSession(state: string, clientId: string, authorization: string): Promise<boolean> {
        try {
            console.log("asdf",authorization)
            const sessionResponse = await fetch(`${url}/SSO/store-session`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${authorization}`
                },
                body: JSON.stringify({
                    state,
                    client_id: clientId
                })
            });

            return sessionResponse.ok;
        } catch (error) {
            console.error('会话存储失败:', error);
            return false;
        }
    }
}
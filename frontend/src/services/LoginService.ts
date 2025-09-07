import {Url} from './Url';

export class LoginService {
    public static async login(username: string, password: string): Promise<any> {
        const response = await fetch(`${Url}/Member/Login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name: username,
                id: password
            })
        });

        if (!response.ok) {
            throw new Error(`登录失败 (${response.status})`);
        }
        const token = await response.text();
        return { token };
    }
}

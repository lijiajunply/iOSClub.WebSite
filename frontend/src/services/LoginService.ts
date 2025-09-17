import {Url} from './Url';

export class LoginService {
    static async login(username: string, studentId: string, password: string): Promise<any> {
        const response = await fetch(`${this.API_BASE_URL}/Member/Login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                name: username,
                id: studentId,
                password: password
            })
        });

        if (!response.ok) {
            throw new Error(`登录失败: ${response.status}`);
        }

        return await response.json();
    }
}

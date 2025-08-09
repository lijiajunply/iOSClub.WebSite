import {Url} from './Url';

export class LoginService {
    public static async login(username: string, password: string): Promise<any> {
        return await fetch(`${Url}/Login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name: username,
                password: password
            })
        }).then(res => res.json())
    }
}
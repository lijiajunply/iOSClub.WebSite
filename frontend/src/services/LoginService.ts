export class LoginService {
    public static async login(username: string, password: string): Promise<any> {
        return await fetch('https://localhost:7257/Login', {
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
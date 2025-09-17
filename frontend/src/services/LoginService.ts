import {Url} from './Url';

export interface SignupModel {
    userName: string;
    userId: string;
    academy: string;
    politicalLandscape: string;
    gender: string;
    className: string;
    phoneNum: string;
    joinTime: string;
    passwordHash: string;
    eMail: string | null;
}

export const hashPassword = async (password) => {
    const encoder = new TextEncoder()
    const data = encoder.encode(password)
    const hashBuffer = await crypto.subtle.digest('SHA-256', data)
    const hashArray = Array.from(new Uint8Array(hashBuffer))
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('')
}

export class OrgSignRecord {
    constructor(
        public readonly url1: string,
        public readonly url2: string
    ) {}
}

export const ios = new OrgSignRecord(
    "mqqapi://card/show_pslcard?src_type=internal&version=1&uin=952954710&card_type=group&source=external",
    "https://qm.qq.com/cgi-bin/qm/qr?authKey=MUNgIj%2B1gnkiI175qAQla6EcR44Fa0APCv%2FLo1a7YIlYgpeg76Q%2BGYMoedb8gGHU&k=HvhhArSc7tzuySOhXsnmZ6RgLcWkzXgu&noverify=0"
);


export class LoginService {
    static async login(username: string, studentId: string, password: string): Promise<string> {
        const response = await fetch(`${Url}/Member/Login`, {
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

        return await response.text();
    }

    static async signup(member: SignupModel): Promise<string> {
        const response = await fetch(`${Url}/Member/SignUp`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(member)
        });

        if (!response.ok) {
            throw new Error(`登录失败: ${response.status}`);
        }

        return await response.text();
    }
}

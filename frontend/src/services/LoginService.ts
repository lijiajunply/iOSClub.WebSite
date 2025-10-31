import { AuthService } from './AuthService';
import { type StudentModel } from './AuthService';

// 组织注册记录类
export class OrgSignRecord {
    constructor(
        public readonly url1: string,
        public readonly url2: string
    ) {}
}

// iOS 俱乐部注册信息
export const ios = new OrgSignRecord(
    "mqqapi://card/show_pslcard?src_type=internal&version=1&uin=952954710&card_type=group&source=external",
    "https://qm.qq.com/cgi-bin/qm/qr?authKey=MUNgIj%2B1gnkiI175qAQla6EcR44Fa0APCv%2FLo1a7YIlYgpeg76Q%2BGYMoedb8gGHU&k=HvhhArSc7tzuySOhXsnmZ6RgLcWkzXgu&noverify=0"
);

// 保留 LoginService 作为兼容层，实际功能委托给 AuthService
export class LoginService {
    // 委托登录功能到 AuthService
    static async login(username: string, studentId: string, password: string): Promise<string> {
        return await AuthService.memberLogin(username, studentId, password);
    }

    // 委托注册功能到 AuthService
    static async signup(member: StudentModel): Promise<string> {
        return await AuthService.memberSignup(member);
    }
}

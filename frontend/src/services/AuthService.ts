import { url } from './Url';
import type { StudentModel, LoginModel } from '../models';

/**
 * 密码哈希函数
 * @param password 原始密码
 * @returns Promise<string> 哈希后的密码
 */
export const hashPassword = async (password: string) => {
  const encoder = new TextEncoder()
  const data = encoder.encode(password)
  const hashBuffer = await crypto.subtle.digest('SHA-256', data)
  const hashArray = Array.from(new Uint8Array(hashBuffer))
  return hashArray.map(b => b.toString(16).padStart(2, '0')).join('')
}

/**
 * 认证服务类 - 处理登录注册相关的API调用
 */
export class AuthService {
  /**
   * 用户登录
   * @param loginModel 登录信息
   * @returns Promise<string> JWT令牌
   */
  static async login(loginModel: LoginModel): Promise<string> {
    const response = await fetch(`${url}/Auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(loginModel),
    });

    if (!response.ok) {
      if (response.status === 404) {
        throw new Error('用户不存在');
      }
      throw new Error('登录失败');
    }

    return await response.text();
  }

  /**
   * 学生注册
   * @param model 学生注册信息
   * @returns Promise<string> JWT令牌
   */
  static async signup(model: StudentModel): Promise<string> {
    const response = await fetch(`${url}/Auth/signup`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(model),
    });

    if (!response.ok) {
      if (response.status === 409) {
        throw new Error('用户ID已存在');
      }
      throw new Error('注册失败');
    }

    return await response.text();
  }

  static async logout(userId: string) {
    const token = AuthService.getToken();
    const response = await fetch(`${url}/Auth/logout?userId=${userId}`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    return response.ok;
  }

  static async validate(userId: string, token: string): Promise<boolean> {
    const response = await fetch(`${url}/Auth/validate?userId=${userId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    return response.ok;
  }

  /**
   * 保存令牌到本地存储
   * @param token JWT令牌
   */
  static saveToken(token: string): void {
    localStorage.setItem('Authorization', token);
  }

  /**
   * 从本地存储获取令牌
   * @returns string | null JWT令牌或null
   */
  static getToken(): string | null {
    return localStorage.getItem('Authorization');
  }

  /**
   * 清除本地存储的令牌
   */
  static clearToken(): void {
    localStorage.removeItem('Authorization');
  }

  /**
   * 检查用户是否已登录
   * @returns boolean 是否已登录
   */
  static isLoggedIn(): boolean {
    return this.getToken() !== null;
  }
}

// 组织注册记录类
export class OrgSignRecord {
  constructor(
    public readonly url1: string,
    public readonly url2: string
  ) {
  }
}

// iOS 俱乐部注册信息
export const ios = new OrgSignRecord(
  "mqqapi://card/show_pslcard?src_type=internal&version=1&uin=952954710&card_type=group&source=external",
  "https://qm.qq.com/cgi-bin/qm/qr?authKey=MUNgIj%2B1gnkiI175qAQla6EcR44Fa0APCv%2FLo1a7YIlYgpeg76Q%2BGYMoedb8gGHU&k=HvhhArSc7tzuySOhXsnmZ6RgLcWkzXgu&noverify=0"
);
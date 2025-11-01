import { url } from './Url';

// 登录模型接口
export interface LoginModel {
  id: string;
  name: string;
}

// 成员模型接口
export interface MemberModel {
  userName: string;
  userId: string;
  identity: string;
}

// 详细注册模型接口
export interface StudentModel {
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
  
  /**
   * 成员详细注册
   * @param member 成员详细信息
   * @returns Promise<string> 响应结果
   */
  static async memberSignup(member: StudentModel): Promise<string> {
    const response = await fetch(`${url}/Member/SignUp`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(member)
    });

    if (!response.ok) {
      throw new Error(`注册失败: ${response.status}`);
    }

    return await response.text();
  }
  
  /**
   * 成员登录（兼容旧接口）
   * @param username 用户名
   * @param studentId 学生ID
   * @param password 密码
   * @returns Promise<string> 响应结果
   */
  static async memberLogin(username: string, studentId: string, password: string): Promise<string> {
    const response = await fetch(`${url}/Member/Login`, {
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

  /**
   * 保存令牌到本地存储
   * @param token JWT令牌
   */
  static saveToken(token: string): void {
    localStorage.setItem('token', token);
  }

  /**
   * 从本地存储获取令牌
   * @returns string | null JWT令牌或null
   */
  static getToken(): string | null {
    return localStorage.getItem('token');
  }

  /**
   * 清除本地存储的令牌
   */
  static clearToken(): void {
    localStorage.removeItem('token');
  }

  /**
   * 检查用户是否已登录
   * @returns boolean 是否已登录
   */
  static isLoggedIn(): boolean {
    return this.getToken() !== null;
  }
}
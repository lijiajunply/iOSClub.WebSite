import { Url } from './Url';
import { AuthService } from './AuthService';

// 用户信息响应接口
export interface UserInfoResponse {
  Tasks?: any[];
  Projects?: any[];
  Resources?: any[];
  Total?: number;
  StaffsCount?: number;
  Departments?: any[];
}

/**
 * 信息服务类 - 处理系统信息相关的API调用
 */
export class InfoService {
  /**
   * 获取所有学院列表
   * @returns Promise<string[]> 学院名称字符串数组
   */
  static async getAcademies(): Promise<string[]> {
    const response = await fetch(`${Url}/Info/academies`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('获取学院列表失败');
    }

    return await response.json();
  }

  /**
   * 获取当前用户信息
   * 根据用户身份返回不同级别的信息
   * @returns Promise<UserInfoResponse> 用户信息对象
   */
  static async getUserInfo(): Promise<UserInfoResponse> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Info/user-info`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.clearToken();
        throw new Error('登录已过期，请重新登录');
      }
      throw new Error('获取用户信息失败');
    }

    // 对于普通成员，返回空对象
    const data = await response.json();
    return data || {};
  }
}
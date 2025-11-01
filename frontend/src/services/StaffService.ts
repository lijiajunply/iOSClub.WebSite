import { url } from './Url';
import { AuthService } from './AuthService';

// 员工模型接口
export interface StaffModel {
  userId: string;
  name: string;
  identity: string;
  department: string;
  // 其他必要的员工信息字段
}

/**
 * 员工服务类 - 处理员工管理相关的API调用
 */
export class StaffService {
  /**
   * 获取所有员工列表
   * @returns Promise<StaffModel[]> 员工列表
   */
  static async getAllStaff(): Promise<StaffModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Staff`, {
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
      if (response.status === 403) {
        throw new Error('权限不足，需要管理员身份');
      }
      throw new Error('获取员工列表失败');
    }

    return await response.json();
  }

  /**
   * 根据用户ID获取员工信息
   * @param userId 用户ID
   * @returns Promise<StaffModel> 员工信息
   */
  static async getStaff(userId: string): Promise<StaffModel> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Staff/${userId}`, {
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
      if (response.status === 403) {
        throw new Error('权限不足，需要管理员身份');
      }
      if (response.status === 404) {
        throw new Error('员工不存在');
      }
      throw new Error('获取员工信息失败');
    }

    return await response.json();
  }

  /**
   * 创建新员工
   * @param staff 员工信息模型
   * @returns Promise<any> 创建结果
   */
  static async createStaff(staff: StaffModel): Promise<any> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Staff/Create`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(staff),
    });

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.clearToken();
        throw new Error('登录已过期，请重新登录');
      }
      if (response.status === 403) {
        throw new Error('权限不足，需要管理员身份');
      }
      throw new Error('创建员工失败');
    }

    return await response.json();
  }
}
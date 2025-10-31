import { Url } from './Url';
import { AuthService } from './AuthService';
import { StudentModel } from './AuthService';
import { MemberModel } from './AuthService';

/**
 * 成员管理服务类 - 处理成员的删除、更新等管理功能
 */
export class MemberManagementService {
  /**
   * 删除学生成员
   * @param id 学生ID
   * @returns Promise<void>
   */
  static async deleteMember(id: string): Promise<void> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/MemberManagement/delete/${id}`, {
      method: 'POST',
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
        throw new Error('成员不存在');
      }
      throw new Error('删除成员失败');
    }
  }

  /**
   * 批量更新或添加学生成员
   * @param list 学生列表
   * @returns Promise<StudentModel[]> 更新后的学生列表
   */
  static async updateManyMembers(list: StudentModel[]): Promise<StudentModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/MemberManagement/update-many`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(list),
    });

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.clearToken();
        throw new Error('登录已过期，请重新登录');
      }
      if (response.status === 403) {
        throw new Error('权限不足，需要管理员身份');
      }
      throw new Error('批量更新成员失败');
    }

    return await response.json();
  }

  /**
   * 更新单个成员信息
   * @param model 成员模型
   * @returns Promise<void>
   */
  static async updateMember(model: MemberModel): Promise<void> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/MemberManagement/update`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(model),
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
        throw new Error('成员不存在');
      }
      throw new Error('更新成员信息失败');
    }
  }
}
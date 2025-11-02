import { url } from './Url';
import { AuthService } from './AuthService';
import {DepartmentModel} from "../models";

/**
 * 部门服务类 - 处理部门相关的API调用
 */
export class DepartmentService {
  /**
   * 获取部门信息
   * @param name 部门名称（可选），不提供则返回当前用户所在部门
   * @returns Promise<DepartmentModel> 部门信息
   */
  static async getDepartment(name?: string): Promise<DepartmentModel> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const departmentUrl = name ? `${url}/Department/${name}` : `${url}/Department`;
    const response = await fetch(departmentUrl, {
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
        throw new Error('权限不足');
      }
      if (response.status === 404) {
        throw new Error(name ? '部门不存在' : '用户未分配部门');
      }
      throw new Error('获取部门信息失败');
    }

    return await response.json();
  }

  /**
   * 获取所有部门（仅管理员）
   * @returns Promise<DepartmentModel[]> 部门列表
   */
  static async getAllDepartments(): Promise<DepartmentModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Department/all`, {
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
      throw new Error('获取所有部门失败');
    }

    return await response.json();
  }

  static async createDepartment(department: DepartmentModel): Promise<void> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Department/Create`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(department),
    });

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.clearToken();
        throw new Error('登录已过期，请重新登录');
      }
      if (response.status === 403) {
        throw new Error('权限不足，需要管理员身份');
      }
      throw new Error('创建部门失败');
    }
  }

  static async updateDepartment(department: DepartmentModel): Promise<void> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Department/Update`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(department),
    });

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.clearToken();
        throw new Error('登录已过期，请重新登录');
      }
      if (response.status === 403) {
        throw new Error('权限不足，需要管理员身份');
      }
      throw new Error('更新部门失败');
    }
  }

  static async deleteDepartment(name: string): Promise<void> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Department/Delete/${name}`, {
      method: 'DELETE',
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
      throw new Error('删除部门失败');
    }
  }
}
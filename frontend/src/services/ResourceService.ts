import { Url } from './Url';
import { AuthService } from './AuthService';

// 资源模型接口
export interface ResourceModel {
  Id: string;
  Name: string;
  Type: string;
  Path: string;
  Description: string;
  CreateTime: Date;
  Tags?: string[];
}

/**
 * 资源服务类 - 处理资源相关的API调用
 */
export class ResourceService {
  /**
   * 获取所有资源（需要社团成员身份）
   * @returns Promise<ResourceModel[]> 资源列表
   */
  static async getAllResources(): Promise<ResourceModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Resource`, {
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
        throw new Error('权限不足，需要社团成员身份');
      }
      throw new Error('获取资源列表失败');
    }

    return await response.json();
  }

  /**
   * 根据ID获取资源（需要社团成员身份）
   * @param id 资源ID
   * @returns Promise<ResourceModel> 资源详情
   */
  static async getResourceById(id: string): Promise<ResourceModel> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Resource/${id}`, {
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
        throw new Error('权限不足，需要社团成员身份');
      }
      if (response.status === 404) {
        throw new Error('资源不存在');
      }
      throw new Error('获取资源失败');
    }

    return await response.json();
  }

  /**
   * 根据标签筛选资源（需要社团成员身份）
   * @param tag 标签名称
   * @returns Promise<ResourceModel[]> 符合条件的资源列表
   */
  static async getResourcesByTag(tag: string): Promise<ResourceModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Resource/tag/${tag}`, {
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
        throw new Error('权限不足，需要社团成员身份');
      }
      throw new Error('获取资源失败');
    }

    return await response.json();
  }
}
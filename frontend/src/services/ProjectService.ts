import { Url } from './Url';
import { AuthService } from './AuthService';

// 项目模型接口
export interface ProjectModel {
  Id: string;
  Name: string;
  Description: string;
  StartTime: Date;
  EndTime: Date;
  Department: string;
  Staffs?: StaffModel[];
  Tasks?: TaskModel[];
}

// 员工模型接口
export interface StaffModel {
  UserId: string;
  Name: string;
  Identity: string;
  Department: string;
}

// 任务模型接口
export interface TaskModel {
  Id: string;
  Name: string;
  Description: string;
  Status: string;
  StartTime: Date;
  EndTime: Date;
  Users?: StaffModel[];
}

// 资源模型接口
export interface ResourceModel {
  Id: string;
  Name: string;
  Type: string;
  Path: string;
  Description: string;
  CreateTime: Date;
}

/**
 * 项目服务类 - 处理项目相关的API调用
 */
export class ProjectService {
  /**
   * 获取所有项目数据（仅管理员）
   * @returns Promise<ProjectModel[]> 项目列表
   */
  static async getAllProjects(): Promise<ProjectModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Project`, {
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
      throw new Error('获取项目列表失败');
    }

    return await response.json();
  }

  /**
   * 获取当前用户的项目列表
   * @returns Promise<ProjectModel[]> 用户参与的项目列表
   */
  static async getYourProjects(): Promise<ProjectModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Project/your-projects`, {
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
      throw new Error('获取用户项目列表失败');
    }

    return await response.json();
  }

  /**
   * 获取当前用户的任务列表
   * @returns Promise<TaskModel[]> 用户的任务列表
   */
  static async getYourTasks(): Promise<TaskModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Project/your-tasks`, {
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
      throw new Error('获取用户任务列表失败');
    }

    return await response.json();
  }

  /**
   * 获取所有资源列表
   * @returns Promise<ResourceModel[]> 资源列表
   */
  static async getResources(): Promise<ResourceModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Project/resources`, {
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
      throw new Error('获取资源列表失败');
    }

    return await response.json();
  }

  /**
   * 创建或更新项目（仅管理员）
   * @param model 项目模型
   * @returns Promise<ProjectModel> 创建或更新后的项目信息
   */
  static async createOrUpdateProject(model: ProjectModel): Promise<ProjectModel> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${Url}/Project`, {
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
      throw new Error('创建或更新项目失败');
    }

    return await response.json();
  }
}
import { url } from './Url';
import { AuthService } from './AuthService';

// 项目模型接口
export interface ProjectModel {
  id: string;
  name: string;
  description: string;
  startTime: Date;
  endTime: Date;
  department: string;
  staffs?: StaffModel[];
  tasks?: TaskModel[];
}

// 员工模型接口
export interface StaffModel {
  userId: string;
  name: string;
  identity: string;
  department: string;
}

// 任务模型接口
export interface TaskModel {
  id: string;
  name: string;
  description: string;
  status: string;
  startTime: Date;
  endTime: Date;
  users?: StaffModel[];
}

// 资源模型接口
export interface ResourceModel {
  id: string;
  name: string;
  type: string;
  path: string;
  description: string;
  createTime: Date;
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

    const response = await fetch(`${url}/Project`, {
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

    const response = await fetch(`${url}/Project/your-projects`, {
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

    const response = await fetch(`${url}/Project/your-tasks`, {
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

    const response = await fetch(`${url}/Project/resources`, {
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

    const response = await fetch(`${url}/Project`, {
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
import {url} from './Url';
import {AuthService} from './AuthService';
import {ProjectModel, ResourceModel, TaskModel} from '../models';

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

        const response = await fetch(`${url}/User/todos`, {
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

        const response = await fetch(`${url}/Resource`, {
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

    /**
     * 删除项目（仅管理员）
     * @param id 项目ID
     * @returns Promise<void>
     */
    static async deleteProject(id: string): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Project/delete/${id}`, {
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
            throw new Error('删除项目失败');
        }
    }

    /**
     * 更改项目成员（仅管理员）
     * @param id 成员ID
     * @param projId 项目ID
     * @returns Promise<void>
     */
    static async changeMember(id: string, projId: string): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Project/change-member/${id}/${projId}`, {
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
            throw new Error('更改项目成员失败');
        }
    }
}
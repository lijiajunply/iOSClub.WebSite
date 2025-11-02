import {url} from './Url';
import {AuthService} from './AuthService';
import {TodoModel, MemberModel} from '../models';

/**
 * 用户服务类 - 处理用户相关的API调用
 */
export class UserService {
    /**
     * 获取当前用户的详细信息
     * @returns Promise<MemberModel> 用户信息对象
     */
    static async getUserData(): Promise<MemberModel> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/User/data`, {
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

        return await response.json();
    }

    /**
     * 获取当前用户的所有待办事项
     * @returns Promise<TodoModel[]> 待办事项列表
     */
    static async getTodos(): Promise<TodoModel[]> {
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
            throw new Error('获取待办事项失败');
        }

        return await response.json();
    }

    /**
     * 添加新的待办事项
     * @param todoModel 待办事项模型
     * @returns Promise<TodoModel> 添加后的待办事项
     */
    static async addTodo(todoModel: Omit<TodoModel, 'Id' | 'CreateTime' | 'LastUpdateTime'>): Promise<TodoModel> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/User/todos`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(todoModel),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            throw new Error('添加待办事项失败');
        }

        return await response.json();
    }

    static async updateTodo(todoModel: TodoModel): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/User/todos`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(todoModel),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            throw new Error('更新待办事项失败');
        }
    }

    static async getTodoById(id: string): Promise<TodoModel> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/User/todos/${id}`, {
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
            throw new Error('获取待办事项失败');
        }

        return await response.json();
    }

    /**
     * 删除指定ID的待办事项
     * @param id 待办事项ID
     * @returns Promise<void>
     */
    static async deleteTodo(id: string): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/User/todos/${id}`, {
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
            if (response.status === 404) {
                throw new Error('待办事项不存在或无权删除');
            }
            throw new Error('删除待办事项失败');
        }
    }

    static async updateProfile(memberModel: MemberModel): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/User/profile`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(memberModel),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            throw new Error('更新用户资料失败');
        }
    }
}
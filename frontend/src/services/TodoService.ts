import { url } from './Url';
import { AuthService } from './AuthService';
import { TodoModel, TodoStatistics } from '../models';

/**
 * 待办事项服务类 - 处理待办事项相关的API调用
 */
export class TodoService {
  /**
   * 获取当前用户的所有待办事项
   * @returns Promise<TodoModel[]> 待办事项列表
   */
  static async getTodos(): Promise<TodoModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Todo`, {
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
   * 获取待办事项统计信息
   * @returns Promise<TodoStatistics> 统计信息对象
   */
  static async getTodoStatistics(): Promise<TodoStatistics> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Todo/statistics`, {
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
      throw new Error('获取待办事项统计失败');
    }

    return await response.json();
  }

  /**
   * 根据ID获取待办事项详情
   * @param id 待办事项ID
   * @returns Promise<TodoModel> 待办事项详情
   */
  static async getTodoById(id: string): Promise<TodoModel> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Todo/${id}`, {
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
        throw new Error('无权访问此待办事项');
      }
      if (response.status === 404) {
        throw new Error('待办事项不存在');
      }
      throw new Error('获取待办事项详情失败');
    }

    return await response.json();
  }

  static async createTodo(todo: TodoModel): Promise<string> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Todo`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(todo),
    });

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.clearToken();
        throw new Error('登录已过期，请重新登录');
      }
      throw new Error('创建待办事项失败');
    }

    return await response.text();
  }

  static async updateTodo(todo: TodoModel): Promise<void> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Todo`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(todo),
    });

    if (!response.ok) {
      if (response.status === 401) {
        AuthService.clearToken();
        throw new Error('登录已过期，请重新登录');
      }
      throw new Error('更新待办事项失败');
    }
  }

  static async deleteTodo(id: string): Promise<void> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Todo/${id}`, {
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
      throw new Error('删除待办事项失败');
    }
  }

  static async getTodosByPage(page: number, pageSize: number): Promise<TodoModel[]> {
    const token = AuthService.getToken();
    if (!token) {
      throw new Error('未登录');
    }

    const response = await fetch(`${url}/Todo/Page/${page}/${pageSize}`, {
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
}
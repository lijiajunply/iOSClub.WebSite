import {url} from './Url';
import {apiRequest} from './ApiService';
import {TodoModel, TodoStatistics} from '../models';

/**
 * 待办事项服务类 - 处理待办事项相关的API调用
 */
export class TodoService {
    /**
     * 获取当前用户的所有待办事项
     * @returns Promise<TodoModel[]> 待办事项列表
     */
    static async getTodos(): Promise<TodoModel[]> {
        return apiRequest<TodoModel[]>({
            url: `${url}/Todo`,
            method: 'GET'
        });
    }

    /**
     * 获取待办事项统计信息
     * @returns Promise<TodoStatistics> 统计信息对象
     */
    static async getTodoStatistics(): Promise<TodoStatistics> {
        return apiRequest<TodoStatistics>({
            url: `${url}/Todo/statistics`,
            method: 'GET'
        });
    }

    /**
     * 根据ID获取待办事项详情
     * @param id 待办事项ID
     * @returns Promise<TodoModel> 待办事项详情
     */
    static async getTodoById(id: string): Promise<TodoModel> {
        return apiRequest<TodoModel>({
            url: `${url}/Todo/${id}`,
            method: 'GET'
        });
    }

    static async createTodo(todo: TodoModel): Promise<string> {
        return apiRequest<string>({
            url: `${url}/Todo`,
            method: 'POST',
            body: todo
        });
    }

    static async updateTodo(todo: TodoModel): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Todo`,
            method: 'PUT',
            body: todo
        });
    }

    static async deleteTodo(id: string): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Todo/${id}`,
            method: 'DELETE'
        });
    }

    static async getTodosByPage(page: number, pageSize: number): Promise<TodoModel[]> {
        return apiRequest<TodoModel[]>({
            url: `${url}/Todo/Page/${page}/${pageSize}`,
            method: 'GET'
        });
    }
}
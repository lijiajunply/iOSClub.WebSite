import {url} from './Url';
import {apiRequest} from './ApiService';
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
        return apiRequest<MemberModel>({
            url: `${url}/User/data`,
            method: 'GET'
        });
    }

    /**
     * 获取当前用户的所有待办事项
     * @returns Promise<TodoModel[]> 待办事项列表
     */
    static async getTodos(): Promise<TodoModel[]> {
        return apiRequest<TodoModel[]>({
            url: `${url}/User/todos`,
            method: 'GET'
        });
    }

    /**
     * 添加新的待办事项
     * @param todoModel 待办事项模型
     * @returns Promise<TodoModel> 添加后的待办事项
     */
    static async addTodo(todoModel: Omit<TodoModel, 'Id' | 'CreateTime' | 'LastUpdateTime'>): Promise<TodoModel> {
        return apiRequest<TodoModel>({
            url: `${url}/User/todos`,
            method: 'POST',
            body: todoModel
        });
    }

    static async updateTodo(todoModel: TodoModel): Promise<void> {
        await apiRequest<void>({
            url: `${url}/User/todos`,
            method: 'PUT',
            body: todoModel
        });
    }

    static async getTodoById(id: string): Promise<TodoModel> {
        return apiRequest<TodoModel>({
            url: `${url}/User/todos/${id}`,
            method: 'GET'
        });
    }

    /**
     * 删除指定ID的待办事项
     * @param id 待办事项ID
     * @returns Promise<void>
     */
    static async deleteTodo(id: string): Promise<void> {
        await apiRequest<void>({
            url: `${url}/User/todos/${id}`,
            method: 'DELETE'
        });
    }

    static async updateProfile(memberModel: MemberModel): Promise<void> {
        await apiRequest<void>({
            url: `${url}/User/profile`,
            method: 'PUT',
            body: memberModel
        });
    }
}
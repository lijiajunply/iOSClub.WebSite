import {url} from './Url';
import {apiRequest} from './ApiService';
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
        return apiRequest<ProjectModel[]>({
            url: `${url}/Project`,
            method: 'GET'
        });
    }

    /**
     * 获取当前用户的项目列表
     * @returns Promise<ProjectModel[]> 用户参与的项目列表
     */
    static async getYourProjects(): Promise<ProjectModel[]> {
        return apiRequest<ProjectModel[]>({
            url: `${url}/Project/your-projects`,
            method: 'GET'
        });
    }

    /**
     * 获取当前用户的任务列表
     * @returns Promise<TaskModel[]> 用户的任务列表
     */
    static async getYourTasks(): Promise<TaskModel[]> {
        return apiRequest<TaskModel[]>({
            url: `${url}/User/todos`,
            method: 'GET'
        });
    }

    /**
     * 获取所有资源列表
     * @returns Promise<ResourceModel[]> 资源列表
     */
    static async getResources(): Promise<ResourceModel[]> {
        return apiRequest<ResourceModel[]>({
            url: `${url}/Resource`,
            method: 'GET'
        });
    }

    /**
     * 创建或更新项目（仅管理员）
     * @param model 项目模型
     * @returns Promise<ProjectModel> 创建或更新后的项目信息
     */
    static async createOrUpdateProject(model: ProjectModel): Promise<ProjectModel> {
        return apiRequest<ProjectModel>({
            url: `${url}/Project`,
            method: 'POST',
            body: model
        });
    }

    /**
     * 删除项目（仅管理员）
     * @param id 项目ID
     * @returns Promise<void>
     */
    static async deleteProject(id: string): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Project/delete/${id}`,
            method: 'POST'
        });
    }

    /**
     * 更改项目成员（仅管理员）
     * @param id 成员ID
     * @param projId 项目ID
     * @returns Promise<void>
     */
    static async changeMember(id: string, projId: string): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Project/change-member/${id}/${projId}`,
            method: 'POST'
        });
    }
}
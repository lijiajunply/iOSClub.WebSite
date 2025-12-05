import {url} from './Url';
import {apiRequest} from './ApiService';
import {ResourceModel} from '../models';

/**
 * 资源服务类 - 处理资源相关的API调用
 */
export class ResourceService {
    /**
     * 获取所有资源（需要社团成员身份）
     * @returns Promise<ResourceModel[]> 资源列表
     */
    static async getAllResources(): Promise<ResourceModel[]> {
        return apiRequest<ResourceModel[]>({
            url: `${url}/Resource`,
            method: 'GET'
        });
    }

    /**
     * 根据ID获取资源（需要社团成员身份）
     * @param id 资源ID
     * @returns Promise<ResourceModel> 资源详情
     */
    static async getResourceById(id: string): Promise<ResourceModel> {
        return apiRequest<ResourceModel>({
            url: `${url}/Resource/${id}`,
            method: 'GET'
        });
    }

    /**
     * 根据标签筛选资源（需要社团成员身份）
     * @param tag 标签名称
     * @returns Promise<ResourceModel[]> 符合条件的资源列表
     */
    static async getResourcesByTag(tag: string): Promise<ResourceModel[]> {
        return apiRequest<ResourceModel[]>({
            url: `${url}/Resource/tag/${tag}`,
            method: 'GET'
        });
    }

    static async createResource(resource: ResourceModel): Promise<ResourceModel> {
        return apiRequest<ResourceModel>({
            url: `${url}/Resource`,
            method: 'POST',
            body: resource
        });
    }

    static async updateResource(resource: ResourceModel): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Resource`,
            method: 'PUT',
            body: resource
        });
    }

    static async deleteResource(id: string): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Resource/${id}`,
            method: 'DELETE'
        });
    }

    static async searchResources(name: string): Promise<ResourceModel[]> {
        return apiRequest<ResourceModel[]>({
            url: `${url}/Resource/search/${name}`,
            method: 'GET'
        });
    }

    static async getResourceTags(): Promise<string[]> {
        return apiRequest<string[]>({
            url: `${url}/Resource/tags`,
            method: 'GET'
        });
    }
}
import {url} from './Url';
import {AuthService} from './AuthService';
import {DepartmentModel} from "../models";
import {apiRequest} from './ApiService';

/**
 * 部门服务类 - 处理部门相关的API调用
 */
export class DepartmentService {
    /**
     * 获取部门信息
     * @param name 部门名称（可选），不提供则返回当前用户所在部门
     * @returns Promise<DepartmentModel> 部门信息
     */
    static async getDepartment(name?: string): Promise<DepartmentModel> {
        const departmentUrl = name ? `${url}/Department/${name}` : `${url}/Department`;
        return await apiRequest<DepartmentModel>({
            url: departmentUrl,
            method: 'GET'
        });
    }

    /**
     * 获取所有部门（仅管理员）
     * @returns Promise<DepartmentModel[]> 部门列表
     */
    static async getAllDepartments(): Promise<DepartmentModel[]> {
        return await apiRequest<DepartmentModel[]>({
            url: `${url}/Department/all`,
            method: 'GET'
        });
    }

    static async createDepartment(department: DepartmentModel): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Department/Create`,
            method: 'POST',
            body: JSON.stringify(department)
        });
    }

    static async updateDepartment(department: DepartmentModel): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Department/Update`,
            method: 'POST',
            body: JSON.stringify(department)
        });
    }

    static async deleteDepartment(name: string): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Department/Delete/${name}`,
            method: 'GET'
        });
    }

    static async exportJson(): Promise<Blob> {
        // 对于文件下载，我们直接使用fetch而不是apiRequest，因为apiRequest只处理JSON响应
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Department/export-json`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
            },
        });

        if (!response.ok) {
            // 尝试解析错误响应
            let errorMessage = `HTTP error! status: ${response.status}`;
            try {
                const errorData = await response.json();
                if (errorData.Message) {
                    errorMessage = errorData.Message;
                }
            } catch {
            }
            throw new Error(errorMessage);
        }

        return await response.blob();
    }
}
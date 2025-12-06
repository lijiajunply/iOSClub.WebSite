import {url} from './Url';
import {apiRequest} from './ApiService';
import {StaffModel, MemberModel} from "../models";

/**
 * 员工服务类 - 处理员工管理相关的API调用
 */
export class StaffService {
    /**
     * 获取所有员工列表
     * @returns Promise<StaffModel[]> 员工列表
     */
    static async getAllStaff(): Promise<MemberModel[]> {
        return apiRequest<MemberModel[]>({
            url: `${url}/Staff/members`,
            method: 'GET'
        });
    }

    /**
     * 根据用户ID获取员工信息
     * @param userId 用户ID
     * @returns Promise<StaffModel> 员工信息
     */
    static async getStaff(userId: string): Promise<StaffModel> {
        return apiRequest<StaffModel>({
            url: `${url}/Staff/${userId}`,
            method: 'GET'
        });
    }

    /**
     * 创建新员工
     * @param staff 员工信息模型
     * @returns Promise<any> 创建结果
     */
    static async createStaff(staff: StaffModel): Promise<any> {
        console.log(staff)
        return apiRequest<any>({
            url: `${url}/Staff/Create`,
            method: 'POST',
            body: staff
        });
    }

    static async updateStaff(userId: string, staff: StaffModel): Promise<any> {
        return apiRequest<any>({
            url: `${url}/Staff/Update/${userId}`,
            method: 'POST',
            body: staff
        });
    }

    static async deleteStaff(userId: string): Promise<boolean> {
        await apiRequest<void>({
            url: `${url}/Staff/Delete/${userId}`,
            method: 'GET'
        });
        return true;
    }

    static async getStaffByIdentity(identity: string): Promise<StaffModel[]> {
        return apiRequest<StaffModel[]>({
            url: `${url}/Staff/by-identity/${identity}`,
            method: 'GET'
        });
    }

    static async changeDepartment(userId: string, departmentName: string): Promise<any> {
        return apiRequest<any>({
            url: `${url}/Staff/change-department/${userId}?departmentName=${departmentName}`,
            method: 'POST'
        });
    }
}
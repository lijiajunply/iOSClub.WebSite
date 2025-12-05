import {url} from './Url';
import {UserInfoResponse} from "../models";
import {apiRequest} from './ApiService';

/**
 * 信息服务类 - 处理系统信息相关的API调用
 */
export class InfoService {
    /**
     * 获取所有学院列表
     * @returns Promise<string[]> 学院名称字符串数组
     */
    static async getAcademies(): Promise<string[]> {
        return await apiRequest<string[]>({
            url: `${url}/Info/academies`,
            method: 'GET',
            requiresAuth: false
        });
    }

    /**
     * 获取当前用户信息
     * 根据用户身份返回不同级别的信息
     * @returns Promise<UserInfoResponse> 用户信息对象
     */
    static async getUserInfo(): Promise<UserInfoResponse> {
        // 对于普通成员，返回空对象
        const data = await apiRequest<UserInfoResponse>({
            url: `${url}/Info/user-info`,
            method: 'GET'
        });
        return data || {};
    }
}
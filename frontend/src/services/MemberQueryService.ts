import {url} from './Url';
import {AuthService} from './AuthService';
import type {MemberModel, PaginatedMemberResponse, StudentModel} from '../models'
import {GZipService} from "./GZipService";

/**
 * 成员查询服务类 - 处理成员数据的查询功能
 */
export class MemberQueryService {
    /**
     * 获取所有成员数据
     * @returns Promise<MemberModel[]> 成员数据列表
     */
    static async getAllData(): Promise<MemberModel[]> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/MemberQuery/all-data`, {
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
            throw new Error('获取成员数据失败');
        }

        // 注意：后端返回的是压缩后的JSON字符串，需要解压处理
        const jsonData = await GZipService.decompressFromString(await response.text());
        return JSON.parse(jsonData);
    }

    /**
     * 分页获取所有成员数据（支持搜索）
     * @param pageNum 页码，默认1
     * @param pageSize 每页大小，默认10
     * @param searchTerm 搜索词
     * @param searchCondition 搜索条件（字段）
     * @returns Promise<PaginatedMemberResponse> 分页后的成员数据
     */
    static async getAllDataByPage(pageNum: number = 1, pageSize: number = 10, searchTerm?: string, searchCondition?: string): Promise<PaginatedMemberResponse> {
        if (pageNum < 1 || pageSize < 1 || pageSize > 100) {
            throw new Error('无效的分页参数');
        }

        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        // 构建查询参数
        const params = new URLSearchParams();
        params.append('pageNum', pageNum.toString());
        params.append('pageSize', pageSize.toString());
        if (searchTerm) {
            params.append('searchTerm', searchTerm);
        }
        if (searchCondition) {
            params.append('searchCondition', searchCondition);
        }

        const response = await fetch(`${url}/MemberQuery/all-data/page?${params.toString()}`, {
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
            throw new Error('获取分页成员数据失败');
        }

        const jsonData = await GZipService.decompressFromString(await response.text());
        return JSON.parse(jsonData);
    }

    public static async search(searchTerm: string, searchCondition: string): Promise<StudentModel[]> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const params = new URLSearchParams();
        params.append('searchTerm', searchTerm);
        params.append('searchCondition', searchCondition);
        const response = await fetch(`${url}/MemberQuery/all-data/search?${params.toString()}`, {
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
            throw new Error('获取分页成员数据失败');
        }

        return await response.json();
    }
}
import {url} from './Url';
import {AuthService} from './AuthService';
import type {MemberModel, PaginatedMemberResponse} from '../models'
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
     * 分页获取所有成员数据
     * @param pageNum 页码，默认1
     * @param pageSize 每页大小，默认10
     * @returns Promise<PaginatedMemberResponse> 分页后的成员数据
     */
    static async getAllDataByPage(pageNum: number = 1, pageSize: number = 10): Promise<PaginatedMemberResponse> {
        if (pageNum < 1 || pageSize < 1 || pageSize > 100) {
            throw new Error('无效的分页参数');
        }

        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/MemberQuery/all-data/page?pageNum=${pageNum}&pageSize=${pageSize}`, {
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
}
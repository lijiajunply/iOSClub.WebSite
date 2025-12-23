import { url } from './Url';
import { apiRequest } from './ApiService';

/**
 * IP黑名单统计信息接口
 */
export interface BlacklistStats {
    totalIps: number;
    totalCidrRanges: number;
    cacheHits: number;
    cacheMisses: number;
    blacklistHits: number;
    lastRefreshTime: string;
}

/**
 * 添加IP请求接口
 */
export interface AddIpRequest {
    ip: string;
    reason?: string;
}

/**
 * 移除IP请求接口
 */
export interface RemoveIpRequest {
    ip: string;
    reason?: string;
}

/**
 * IP检查结果接口
 */
export interface IpCheckResult {
    ip: string;
    isBlacklisted: boolean;
    checkTime: string;
}

/**
 * IP黑名单服务类
 */
export class IpBlacklistService {
    /**
     * 获取黑名单统计信息
     */
    static async getStats(): Promise<BlacklistStats> {
        return apiRequest<BlacklistStats>({
            url: `${url}/IpBlacklist/stats`,
            method: 'GET',
            requiresAuth: true
        });
    }

    /**
     * 添加IP到黑名单
     */
    static async addToBlacklist(request: AddIpRequest): Promise<string> {
        return apiRequest<string>({
            url: `${url}/IpBlacklist/add`,
            method: 'POST',
            body: request,
            requiresAuth: true
        });
    }

    /**
     * 从黑名单移除IP
     */
    static async removeFromBlacklist(request: RemoveIpRequest): Promise<string> {
        return apiRequest<string>({
            url: `${url}/IpBlacklist/remove`,
            method: 'POST',
            body: request,
            requiresAuth: true
        });
    }

    /**
     * 刷新黑名单缓存
     */
    static async refreshBlacklist(): Promise<string> {
        return apiRequest<string>({
            url: `${url}/IpBlacklist/refresh`,
            method: 'POST',
            requiresAuth: true
        });
    }

    /**
     * 检查IP是否在黑名单中
     */
    static async checkIp(ip: string): Promise<IpCheckResult> {
        return apiRequest<IpCheckResult>({
            url: `${url}/IpBlacklist/check/${encodeURIComponent(ip)}`,
            method: 'GET',
            requiresAuth: true
        });
    }
}

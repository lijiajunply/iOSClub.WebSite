import { apiRequest } from './ApiService';
import {url} from './Url';

/**
 * 监控数据服务
 */
export class MonitoringService {
    /**
     * 获取性能监控数据
     * @returns Promise<any> 性能监控数据
     */
    static async getPerformanceMetrics(): Promise<any> {
        return apiRequest({
            url: `${url}/Monitoring/performance`,
            requiresAuth: true
        });
    }

    /**
     * 获取HTTP请求统计数据
     * @returns Promise<any> HTTP请求统计数据
     */
    static async getHttpStats(): Promise<any> {
        return apiRequest({
            url: `${url}/Monitoring/http-stats`,
            requiresAuth: true
        });
    }

    /**
     * 获取数据访问统计
     * @param entityType 实体类型（可选）
     * @param top 返回前N条数据
     * @returns Promise<any> 数据访问统计数据
     */
    static async getDataAccessStats(entityType?: string, top: number = 10): Promise<any> {
        const params = new URLSearchParams();
        if (entityType) {
            params.append('entityType', entityType);
        }
        params.append('top', top.toString());

        return apiRequest({
            url: `${url}/Monitoring/data-access-stats?${params.toString()}`,
            requiresAuth: true
        });
    }

    /**
     * 获取数据变化统计
     * @param entityType 实体类型（可选）
     * @param top 返回前N条数据
     * @returns Promise<any> 数据变化统计数据
     */
    static async getDataChangeStats(entityType?: string, top: number = 10): Promise<any> {
        const params = new URLSearchParams();
        if (entityType) {
            params.append('entityType', entityType);
        }
        params.append('top', top.toString());

        return apiRequest({
            url: `${url}/Monitoring/data-change-stats?${params.toString()}`,
            requiresAuth: true
        });
    }

    /**
     * 重置数据统计
     * @param entityType 实体类型（可选）
     * @returns Promise<any> 重置结果
     */
    static async resetDataStats(entityType?: string): Promise<any> {
        const params = new URLSearchParams();
        if (entityType) {
            params.append('entityType', entityType);
        }

        return apiRequest({
            url: `${url}/Monitoring/reset-data-stats?${params.toString()}`,
            method: 'POST',
            requiresAuth: true
        });
    }
}

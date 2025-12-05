import {url} from './Url';
import {apiRequest} from './ApiService';

/**
 * 日志级别枚举
 */
export enum LogLevel {
    TRACE = 'Trace',
    DEBUG = 'Debug',
    INFORMATION = 'Information',
    WARNING = 'Warning',
    ERROR = 'Error',
    CRITICAL = 'Critical'
}

/**
 * 日志统计结果接口
 */
export interface LogStatistics {
    /**
     * 总日志数量
     */
    totalCount: number;

    /**
     * 各日志级别的数量统计
     */
    levelCounts: Record<string, number>;
}

/**
 * 日志分布数据接口，用于表示按时间分布的日志数量
 */
export interface LogDistribution {
    /**
     * 时间点，格式为"HH:00"或"YYYY-MM-DD"
     */
    timePoint: string;

    /**
     * 该时间点的日志总数
     */
    totalCount: number;

    /**
     * 该时间点的错误日志数量
     */
    errorCount: number;

    /**
     * 该时间点的信息日志数量
     */
    infoCount: number;

    /**
     * 该时间点的警告日志数量
     */
    warningCount: number;
}

/**
 * 日志条目接口，用于表示单条日志记录
 */
export interface LogEntry {
    /**
     * 日志时间戳
     */
    timestamp: string;
    /**
     * 日志级别（如 Information, Warning, Error等）
     */
    level: string;
    /**
     * 异常信息（如果有的话）
     */
    exception?: string | null;
    /**
     * 日志属性字典
     */
    properties?: Record<string, unknown> | null;
    /**
     * 日志消息内容
     */
    message?: string | null;
}

/**
 * 分页响应接口，包含分页信息和数据
 */
export interface PaginatedResponse<T> {
    /**
     * 数据列表
     */
    data: T[];

    /**
     * 总记录数
     */
    totalCount: number;

    /**
     * 当前页码
     */
    pageIndex: number;

    /**
     * 每页记录数
     */
    pageSize: number;

    /**
     * 总页数
     */
    totalPages: number;
}

/**
 * 日志服务类 - 处理日志相关的API调用
 */
export class LogsService {
    /**
     * 获取最近的日志条目，支持分页和多条件搜索
     * @param pageIndex 页码，从1开始，默认为1
     * @param pageSize 每页记录数，默认为10
     * @param searchTerm 搜索关键词，用于在日志消息和属性中搜索
     * @param levelFilter 日志级别过滤
     * @param timeRange 时间范围过滤："today"表示当天，或正整数表示最近几天
     * @returns Promise<PaginatedResponse<LogEntry>> 包含分页信息和日志条目的响应
     */
    static async getRecentLogs(pageIndex: number = 1, pageSize: number = 10, searchTerm?: string, levelFilter?: string, timeRange?: string): Promise<PaginatedResponse<LogEntry>> {
        // 参数验证
        if (pageIndex < 1) {
            throw new Error('页码必须大于等于1');
        }
        if (pageSize < 1 || pageSize > 100) {
            throw new Error('每页记录数必须在1到100之间');
        }

        // 构建查询参数
        const params = new URLSearchParams();
        params.append('pageIndex', pageIndex.toString());
        params.append('pageSize', pageSize.toString());

        if (searchTerm && searchTerm.trim()) {
            params.append('searchTerm', searchTerm.trim());
        }

        if (levelFilter && levelFilter.trim()) {
            params.append('levelFilter', levelFilter.trim());
        }

        if (timeRange && timeRange.trim()) {
            params.append('timeRange', timeRange.trim());
        }

        return apiRequest<PaginatedResponse<LogEntry>>({
            url: `${url}/Logs?${params.toString()}`,
            method: 'GET'
        });
    }

    /**
     * 获取日志统计信息，包括总日志数量和各日志级别的数量
     * @returns Promise<LogStatistics> 日志统计结果
     */
    static async getLogStatistics(): Promise<LogStatistics> {
        return apiRequest<LogStatistics>({
            url: `${url}/Logs/statistics`,
            method: 'GET'
        });
    }

    /**
     * 获取日志分布数据，用于图表展示
     * @param timeRange 时间范围过滤："today"表示当天，或正整数表示最近几天
     * @returns Promise<LogDistribution[]> 按时间分布的日志数据
     */
    static async getLogDistribution(timeRange: string = 'today'): Promise<LogDistribution[]> {
        // 构建查询参数
        const params = new URLSearchParams();
        params.append('timeRange', timeRange);

        return apiRequest<LogDistribution[]>({
            url: `${url}/Logs/distribution?${params.toString()}`,
            method: 'GET'
        });
    }

    /**
     * 手动清理旧日志
     * @param days 要保留的日志天数，默认为7天
     * @returns Promise<{ Message: string }> 清理结果
     */
    static async cleanupOldLogs(days: number = 7): Promise<{ Message: string }> {
        // 参数验证
        if (days <= 0) {
            throw new Error('天数必须大于0');
        }

        // 构建查询参数
        const params = new URLSearchParams();
        params.append('days', days.toString());

        return apiRequest<{ Message: string }>({
            url: `${url}/Logs/cleanup?${params.toString()}`,
            method: 'POST'
        });
    }
}
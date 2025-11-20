import { url } from './Url';
import { AuthService } from './AuthService';

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
 * 日志服务类 - 处理日志相关的API调用
 */
export class LogsService {
  /**
   * 获取最近的日志条目
   * @param count 要返回的日志条目数量，默认为10条
   * @returns Promise<LogEntry[]> 日志条目列表
   */
  static async getRecentLogs(count: number = 10): Promise<LogEntry[]> {
    // 参数验证
    if (count <= 0) {
      throw new Error('日志数量必须大于0');
    }

    try {
      // 获取token（假设查看日志需要认证）
      const token = AuthService.getToken();
      const headers: HeadersInit = {
        'Content-Type': 'application/json'
      };

      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await fetch(`${url}/Logs?count=${count}`, {
        method: 'GET',
        headers
      });

      if (!response.ok) {
        if (response.status === 401) {
          AuthService.clearToken();
          throw new Error('登录已过期，请重新登录');
        }
        if (response.status === 403) {
          throw new Error('权限不足，无法查看日志');
        }
        
        // 尝试获取详细错误信息
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.Error || `获取日志失败: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('获取最近日志时发生错误:', error);
      throw error;
    }
  }

  /**
   * 按级别过滤日志
   * @param level 日志级别 (Information, Warning, Error等)
   * @param count 要返回的日志条目数量，默认为10条
   * @returns Promise<LogEntry[]> 指定级别的日志条目列表
   */
  static async getLogsByLevel(level: string, count: number = 10): Promise<LogEntry[]> {
    // 参数验证
    if (!level || level.trim() === '') {
      throw new Error('日志级别不能为空');
    }
    if (count <= 0) {
      throw new Error('日志数量必须大于0');
    }

    try {
      // 获取token（假设查看日志需要认证）
      const token = AuthService.getToken();
      const headers: HeadersInit = {
        'Content-Type': 'application/json'
      };

      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const response = await fetch(`${url}/Logs/filter?level=${encodeURIComponent(level)}&count=${count}`, {
        method: 'GET',
        headers
      });

      if (!response.ok) {
        if (response.status === 401) {
          AuthService.clearToken();
          throw new Error('登录已过期，请重新登录');
        }
        if (response.status === 403) {
          throw new Error('权限不足，无法查看日志');
        }
        
        // 尝试获取详细错误信息
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.Error || `按级别过滤日志失败: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error(`按级别 ${level} 过滤日志时发生错误:`, error);
      throw error;
    }
  }
}
import { createDiscreteApi } from 'naive-ui'
import type { MessageOptions, MessageReactive } from 'naive-ui'
import type { ApiResponse } from './ApiService'

/**
 * 消息服务配置选项
 */
export interface MessageServiceConfig {
    showSuccess?: boolean;
    showError?: boolean;
    successMessage?: string;
    errorMessage?: string;
}

/**
 * 使用 createDiscreteApi 创建独立的消息 API
 * 允许在非组件代码中调用 Naive UI 的消息系统
 */
const { message } = createDiscreteApi(['message'])

/**
 * 消息服务 - 提供全局的消息显示功能
 * 用于在服务层自动显示 API 响应消息
 */
export class MessageService {
    /**
     * 处理 API 响应并自动显示消息
     * @param response API 响应对象
     * @param config 消息配置选项
     */
    static handleResponse<T>(response: ApiResponse<T>, config: MessageServiceConfig = {}): void {
        const {
            showSuccess = false,
            showError = true,
            successMessage,
            errorMessage
        } = config

        if (response.code === 200 && showSuccess) {
            this.success(successMessage || response.message || '操作成功')
        } else if (response.code !== 200 && showError) {
            this.error(errorMessage || response.message || '操作失败')
        }
    }

    /**
     * 显示成功消息
     * @param content 消息内容
     * @param options 额外配置选项
     */
    static success(content: string, options?: MessageOptions): MessageReactive {
        return message.success(content, {
            duration: 3000,
            ...options
        })
    }

    /**
     * 显示错误消息
     * @param content 消息内容
     * @param options 额外配置选项
     */
    static error(content: string, options?: MessageOptions): MessageReactive {
        return message.error(content, {
            duration: 5000,
            ...options
        })
    }

    /**
     * 显示警告消息
     * @param content 消息内容
     * @param options 额外配置选项
     */
    static warning(content: string, options?: MessageOptions): MessageReactive {
        return message.warning(content, {
            duration: 4000,
            ...options
        })
    }

    /**
     * 显示信息消息
     * @param content 消息内容
     * @param options 额外配置选项
     */
    static info(content: string, options?: MessageOptions): MessageReactive {
        return message.info(content, {
            duration: 3000,
            ...options
        })
    }

    /**
     * 显示加载消息（不自动消失，需手动关闭）
     * @param content 消息内容
     * @param options 额外配置选项
     */
    static loading(content: string, options?: MessageOptions): MessageReactive {
        return message.loading(content, {
            duration: 0,
            ...options
        })
    }

    /**
     * 创建消息（通用方法）
     * @param type 消息类型
     * @param content 消息内容
     * @param options 额外配置选项
     */
    static create(
        type: 'success' | 'error' | 'warning' | 'info' | 'loading',
        content: string,
        options?: MessageOptions
    ): MessageReactive {
        return message[type](content, options)
    }
}

/**
 * 默认导出 MessageService 实例
 * 方便直接导入使用
 */
export default MessageService

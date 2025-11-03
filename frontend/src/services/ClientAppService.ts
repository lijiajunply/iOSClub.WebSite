import { url } from './Url';
import { AuthService } from './AuthService';
import type { ClientApplication, ClientAppResultModel, CreateClientAppModel, UpdateClientAppModel, RegenerateSecretResult } from '../models';

/**
 * 客户端应用服务类 - 处理客户端应用管理相关的API调用
 */
export class ClientAppService {
    /**
     * 获取所有客户端应用
     * @returns Promise<ClientApplication[]> 客户端应用列表
     */
    static async getAllClientApplications(): Promise<ClientApplication[]> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/ClientApp`, {
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
            throw new Error('获取客户端应用列表失败');
        }

        return await response.json();
    }

    /**
     * 根据客户端ID获取应用
     * @param clientId 客户端ID
     * @returns Promise<ClientApplication> 客户端应用
     */
    static async getClientApplication(clientId: string): Promise<ClientApplication> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/ClientApp/${clientId}`, {
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
            if (response.status === 404) {
                throw new Error('客户端应用不存在');
            }
            throw new Error('获取客户端应用失败');
        }

        return await response.json();
    }

    /**
     * 创建新的客户端应用
     * @param clientAppModel 客户端应用信息
     * @returns Promise<ClientAppResultModel> 创建的客户端应用（包含密钥）
     */
    static async createClientApplication(clientAppModel: CreateClientAppModel): Promise<ClientAppResultModel> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/ClientApp`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(clientAppModel),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            if (response.status === 403) {
                throw new Error('权限不足，需要管理员身份');
            }
            throw new Error('创建客户端应用失败');
        }

        return await response.json();
    }

    /**
     * 更新客户端应用
     * @param clientId 客户端ID
     * @param clientAppModel 客户端应用信息
     * @returns Promise<void>
     */
    static async updateClientApplication(clientId: string, clientAppModel: UpdateClientAppModel): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/ClientApp/${clientId}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(clientAppModel),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            if (response.status === 403) {
                throw new Error('权限不足，需要管理员身份');
            }
            if (response.status === 404) {
                throw new Error('客户端应用不存在');
            }
            throw new Error('更新客户端应用失败');
        }
    }

    /**
     * 删除客户端应用
     * @param clientId 客户端ID
     * @returns Promise<void>
     */
    static async deleteClientApplication(clientId: string): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/ClientApp/${clientId}`, {
            method: 'DELETE',
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
            if (response.status === 404) {
                throw new Error('客户端应用不存在');
            }
            throw new Error('删除客户端应用失败');
        }
    }

    /**
     * 重新生成客户端密钥
     * @param clientId 客户端ID
     * @returns Promise<RegenerateSecretResult> 新的客户端密钥
     */
    static async regenerateClientSecret(clientId: string): Promise<RegenerateSecretResult> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/ClientApp/${clientId}/regenerate-secret`, {
            method: 'POST',
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
            if (response.status === 404) {
                throw new Error('客户端应用不存在');
            }
            throw new Error('重新生成密钥失败');
        }

        return await response.json();
    }
}
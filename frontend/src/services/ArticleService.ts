import {url} from './Url';
import {AuthService} from './AuthService';
import type {ArticleModel, ArticleCreateDto, ArticleUpdateDto} from "../models";
import type {ArticleSearchResult} from '../models/ArticleModel'

/**
 * 文章服务类 - 处理文章相关的API调用
 */
export class ArticleService {
    static async getAllArticles(): Promise<ArticleModel[]> {
        const response = await fetch(`${url}/Article`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (!response.ok) {
            throw new Error('获取文章列表失败');
        }

        return await response.json();
    }

    /**
     * 根据路径获取文章（公开访问）
     * @param path 文章路径
     * @returns Promise<ArticleModel> 文章详情
     */
    static async getArticleByPath(path: string): Promise<ArticleModel> {
        if (!path || path.trim() === '') {
            throw new Error('路径不能为空');
        }

        const response = await fetch(`${url}/Article/${path}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (!response.ok) {
            if (response.status === 404) {
                throw new Error(`未找到路径为 '${path}' 的文章`);
            }
            throw new Error('获取文章失败');
        }

        return await response.json();
    }

    /**
     * 创建新文章（需要社团成员身份）
     * @param createDto 文章创建数据
     * @returns Promise<ArticleModel> 创建的文章
     */
    static async createArticle(createDto: ArticleCreateDto): Promise<ArticleModel> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Article`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(createDto),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            if (response.status === 403) {
                throw new Error('权限不足，需要社团成员身份');
            }
            if (response.status === 409) {
                throw new Error(`路径 '${createDto.path}' 已存在`);
            }
            if (response.status === 400) {
                const errors = await response.json();
                const errorMessages = Object.keys(errors).map(key => errors[key]).join(', ');
                throw new Error(`数据验证失败: ${errorMessages}`);
            }
            throw new Error('创建文章失败');
        }

        return await response.json();
    }

    static async updateArticle(path: string, updateDto: ArticleUpdateDto) {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Article/update/${path}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(updateDto),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            if (response.status === 403) {
                throw new Error('权限不足，需要社团成员身份');
            }
            if (response.status === 404) {
                throw new Error(`未找到路径为 '${path}' 的文章`);
            }
            if (response.status === 409) {
                throw new Error(`路径 '${path}' 已存在`);
            }
            if (response.status === 400) {
                const errors = await response.json();
                const errorMessages = Object.keys(errors).map(key => errors[key]).join(', ');
                throw new Error(`数据验证失败: ${errorMessages}`);
            }
            throw new Error('更新文章失败');
        }

        return await response.json();
    }

    static async deleteArticle(path: string): Promise<boolean> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Article/delete/${path}`, {
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
                throw new Error('权限不足，需要社团成员身份');
            }
            if (response.status === 404) {
                throw new Error(`未找到路径为 '${path}' 的文章`);
            }
            throw new Error('删除文章失败');
        }

        return response.ok;
    }

    static async searchArticles(keyword: string): Promise<ArticleSearchResult[]> {
        const response = await fetch(`${url}/Article/search/highlights?keyword=${encodeURIComponent(keyword)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            throw new Error('搜索文章失败');
        }

        return await response.json();
    }

    static async getArticle(path: string): Promise<ArticleModel> {
        return this.getArticleByPath(path);
    }



    /**
     * 获取所有分类的文章（公开访问）
     * @returns Promise<Dictionary<string, ArticleModel[]>> 分类文章列表
     */
    static async getAllCategoryArticles(): Promise<Record<string, ArticleModel[]>> {
        const token = AuthService.getToken();

        const headers: Record<string, string> = {
            'Content-Type': 'application/json',
        };

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(`${url}/Article/category`, {
            method: 'GET',
            headers: headers
        });

        if (!response.ok) {
            throw new Error('获取分类文章失败');
        }

        return await response.json();
    }

    /**
     * 批量更新文章顺序
     * @param articleOrders 文章路径和对应顺序的字典
     * @returns Promise<void>
     */
    static async updateArticleOrders(articleOrders: Record<string, number>): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Article/update-orders`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(articleOrders),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            if (response.status === 403) {
                throw new Error('权限不足，需要管理员身份');
            }
            throw new Error('批量更新文章顺序失败');
        }
    }
}
import {url} from './Url';
import {AuthService} from './AuthService';
import {ArticleModel, CategoryModel} from '../models/ArticleModel';

/**
 * 分类服务类 - 处理文章分类相关的API调用
 */
export class CategoryService {
    /**
     * 获取所有分类
     * @returns Promise<CategoryModel[]> 分类列表
     */
    static async getAllCategories(): Promise<CategoryModel[]> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/all`, {
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
                throw new Error('权限不足');
            }
            throw new Error('获取分类列表失败');
        }

        return await response.json();
    }

    /**
     * 根据名称获取分类
     * @param name 分类名称
     * @returns Promise<CategoryModel> 分类信息
     */
    static async getCategory(name: string): Promise<CategoryModel> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/${name}`, {
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
                throw new Error('权限不足');
            }
            if (response.status === 404) {
                throw new Error('分类不存在');
            }
            throw new Error('获取分类信息失败');
        }

        return await response.json();
    }

    /**
     * 根据Id获取分类
     * @param id 分类Id
     * @returns Promise<CategoryModel> 分类信息
     */
    static async getCategoryById(id: string): Promise<CategoryModel | null> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/byId/${id}`, {
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
                throw new Error('权限不足');
            }
            if (response.status === 404) {
                throw new Error('分类不存在');
            }
            throw new Error('获取分类信息失败');
        }

        return await response.json();
    }

    /**
     * 获取所有分类下的文章（公开访问）
     * @param id 分类名称
     * @returns Promise<ArticleModel[]> 分类下的文章列表
     */
    static async getCategoryArticles(id: string): Promise<ArticleModel[]> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/articles/${id}`, {
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
                throw new Error('权限不足');
            }
            if (response.status === 404) {
                throw new Error('分类不存在');
            }
            throw new Error('获取分类信息失败');
        }

        return await response.json();
    }

    /**
     * 创建或更新分类
     * @param category 分类信息
     * @returns Promise<void>
     */
    static async createOrUpdateCategory(category: CategoryModel): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/CreateOrUpdate`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(category),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            if (response.status === 403) {
                throw new Error('权限不足，需要管理员身份');
            }
            throw new Error('创建或更新分类失败');
        }
    }

    /**
     * 删除分类
     * @param name 分类名称
     * @returns Promise<void>
     */
    static async deleteCategory(name: string): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/Delete/${name}`, {
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
            throw new Error('删除分类失败');
        }
    }

    /**
     * 更新分类顺序
     * @param name 分类名称
     * @param order 新的顺序值
     * @returns Promise<void>
     */
    static async updateCategoryOrder(name: string, order: number): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/UpdateOrder/${name}/${order}`, {
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
            throw new Error('更新分类顺序失败');
        }
    }

    /**
     * 批量更新分类顺序
     * @param categoryOrders 分类名称和对应顺序的字典
     * @returns Promise<void>
     */
    static async updateCategoryOrders(categoryOrders: Record<string, number>): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }

        const response = await fetch(`${url}/Category/UpdateOrders`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(categoryOrders),
        });

        if (!response.ok) {
            if (response.status === 401) {
                AuthService.clearToken();
                throw new Error('登录已过期，请重新登录');
            }
            if (response.status === 403) {
                throw new Error('权限不足，需要管理员身份');
            }
            throw new Error('批量更新分类顺序失败');
        }
    }
}
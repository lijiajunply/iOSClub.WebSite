import {url} from './Url';
import type {ArticleModel, ArticleCreateDto, ArticleUpdateDto} from "../models";
import type {ArticleSearchResult} from '../models'
import { apiRequest } from './ApiService';

/**
 * 文章服务类 - 处理文章相关的API调用
 */
export class ArticleService {
    static async getAllArticles(): Promise<ArticleModel[]> {
        return await apiRequest<ArticleModel[]>({
            url: `${url}/Article`,
            method: 'GET',
            requiresAuth: false
        });
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

        return await apiRequest<ArticleModel>({
            url: `${url}/Article/${path}`,
            method: 'GET',
            requiresAuth: false
        });
    }

    /**
     * 创建新文章（需要社团成员身份）
     * @param createDto 文章创建数据
     * @returns Promise<ArticleModel> 创建的文章
     */
    static async createArticle(createDto: ArticleCreateDto): Promise<ArticleModel> {
        return await apiRequest<ArticleModel>({
            url: `${url}/Article`,
            method: 'POST',
            body: JSON.stringify(createDto)
        });
    }

    static async updateArticle(path: string, updateDto: ArticleUpdateDto): Promise<ArticleModel> {
        return await apiRequest<ArticleModel>({
            url: `${url}/Article/update/${path}`,
            method: 'POST',
            body: JSON.stringify(updateDto)
        });
    }

    static async deleteArticle(path: string): Promise<boolean> {
        return await apiRequest<boolean>({
            url: `${url}/Article/delete/${path}`,
            method: 'POST'
        });
    }

    static async searchArticles(keyword: string): Promise<ArticleSearchResult[]> {
        return await apiRequest<ArticleSearchResult[]>({
            url: `${url}/Article/search/highlights?keyword=${encodeURIComponent(keyword)}`,
            method: 'GET',
            requiresAuth: false
        });
    }

    static async getArticle(path: string): Promise<ArticleModel> {
        return this.getArticleByPath(path);
    }



    /**
     * 获取所有分类的文章（公开访问）
     * @returns Promise<Dictionary<string, ArticleModel[]>> 分类文章列表
     */
    static async getAllCategoryArticles(): Promise<Record<string, ArticleModel[]>> {
        return await apiRequest<Record<string, ArticleModel[]>>({
            url: `${url}/Article/category`,
            method: 'GET',
            requiresAuth: false
        });
    }

    /**
     * 批量更新文章顺序
     * @param articleOrders 文章路径和对应顺序的字典
     * @returns Promise<void>
     */
    static async updateArticleOrders(articleOrders: Record<string, number>): Promise<void> {
        await apiRequest<void>({
            url: `${url}/Article/update-orders`,
            method: 'POST',
            body: JSON.stringify(articleOrders)
        });
    }
}
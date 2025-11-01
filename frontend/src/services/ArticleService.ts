import { url } from './Url';
import { AuthService } from './AuthService';

// 文章模型接口
export interface ArticleModel {
  path: string;
  title: string;
  content: string;
  lastWriteTime: string;
  identity?: string;
  watch?: number;
}

// 文章创建DTO接口
export interface ArticleCreateDto {
  path: string;
  title: string;
  content: string;
}

/**
 * 文章服务类 - 处理文章相关的API调用
 */
export class ArticleService {
  /**
   * 获取所有文章（公开访问）
   * @returns Promise<ArticleModel[]> 文章列表
   */
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

  // 保持向后兼容的方法
  static async getArticles(): Promise<any> {
    return this.getAllArticles();
  }

  static async getArticle(path: string): Promise<ArticleModel> {
    return this.getArticleByPath(path);
  }
}
// 分类模型接口
export interface CategoryModel {
  id: string;
  name: string;
  order: number;
  description?: string;
}

// 文章模型接口
export interface ArticleModel {
  path: string;
  title: string;
  content: string;
  lastWriteTime: string;
  identity?: string;
  categoryId?: string;
  category?: CategoryModel;
  articleOrder?: number;
}

// 文章创建DTO接口
export interface ArticleCreateDto {
  path: string;
  title: string;
  content: string;
  identity?: string;
  categoryId?: string;
  articleOrder?: number;
  category?: string;
}

export interface ArticleUpdateDto{
  title: string;
  content: string;
  identity?: string;
  categoryId?: string;
  articleOrder?: number;
  category?: string;
}

// 文章搜索结果接口
export interface ArticleSearchResult {
  path: string;
  title: string;
  content: string;
  lastWriteTime: string;
  identity?: string;
  categoryId?: string;
  category?: CategoryModel;
  articleOrder?: number;
  highlight?: string;
  highlightedTitle?: string;
  highlightedContent?: string;
}
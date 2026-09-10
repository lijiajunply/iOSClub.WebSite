// 分类模型接口
export interface CategoryModel {
  id: string;
  name: string;
  order: number;
  description?: string;
}

// 文章模型接口 —— 对应后端 VOs/ArticleVO.cs（列表为 ArticleListItemVO.cs）
// 后端返回的是扁平的 categoryId / categoryName，没有嵌套的 category 对象。
export interface ArticleModel {
  path: string;
  title: string;
  content: string;
  lastWriteTime: string;
  identity?: string;
  visibleToDepartment?: string;
  categoryId?: string;
  categoryName?: string;
  articleOrder?: number;
}

// 文章创建请求 —— 对应后端 DTOs/ArticleCreateDTO.cs
// 注意 category 传的是分类**名称**，后端没有 categoryId 字段。
export interface ArticleCreateDto {
  path: string;
  title: string;
  content: string;
  identity?: string;
  visibleToDepartment?: string;
  articleOrder?: number;
  category?: string;
}

// 文章更新请求 —— 对应后端 DTOs/ArticleUpdateDTO.cs（path 走路由，不在 body 里）
export interface ArticleUpdateDto{
  title: string;
  content: string;
  identity?: string;
  visibleToDepartment?: string;
  articleOrder?: number;
  category?: string;
}

// 文章搜索结果接口 —— 对应后端 ArticleRepository.ArticleSearchResult
export interface ArticleSearchResult {
  path: string;
  title: string;
  content: string;
  lastWriteTime: string;
  identity?: string;
  visibleToDepartment?: string;
  categoryId?: string;
  categoryName?: string;
  articleOrder?: number;
  highlight?: string;
  highlightedTitle?: string;
  highlightedContent?: string;
}

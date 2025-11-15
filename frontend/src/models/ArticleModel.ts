// 文章模型接口
export interface ArticleModel {
  path: string;
  title: string;
  content: string;
  lastWriteTime: string;
  identity?: string;
}

// 文章创建DTO接口
export interface ArticleCreateDto {
  path: string;
  title: string;
  content: string;
}

export interface ArticleUpdateDto{
  title: string;
  content: string;
  identity?: string;
}
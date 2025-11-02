import { MemberModel } from './AuthModel';

// 分页响应接口
export interface PaginatedMemberResponse {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  data: MemberModel[];
}
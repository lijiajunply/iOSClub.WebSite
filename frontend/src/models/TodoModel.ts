// 待办事项模型接口
export interface TodoModel {
  id: string;
  studentId: string;
  content: string;
  completed: boolean;
  createTime: Date;
  lastUpdateTime: Date;
}

// 待办事项统计接口
export interface TodoStatistics {
  total: number;
  completed: number;
  pending: number;
  completionRate: number;
}
// 项目模型接口
export interface ProjectModel {
  id: string;
  name: string;
  description: string;
  startTime: Date;
  endTime: Date;
  department: string;
  staffs?: StaffModel[];
  tasks?: TaskModel[];
}

// 员工模型接口
export interface StaffModel {
  userId: string;
  name: string;
  identity: string;
  department: string;
  Projects?: ProjectModel[];
  Tasks?: TaskModel[];
}

// 任务模型接口
export interface TaskModel {
  id: string;
  name: string;
  description: string;
  status: string;
  startTime: Date;
  endTime: Date;
  users?: StaffModel[];
}

// 资源模型接口
export interface ResourceModel {
  id: string;
  name: string;
  description: string;
  tag?: string;
}
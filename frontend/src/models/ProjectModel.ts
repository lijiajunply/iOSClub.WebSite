// 项目模型接口
import {DepartmentModel} from "./DepartmentModel";

export interface ProjectModel {
  id: string;
  title: string;
  description: string;
  startTime: string | null;
  endTime: string | null;
  department: DepartmentModel | null;
  staffs?: StaffModel[];
  tasks?: TaskModel[];
}

// 员工模型接口
export interface StaffModel {
  userId: string;
  name: string;
  identity: string;
  department: DepartmentModel | null;
  projects?: ProjectModel[];
  tasks?: TaskModel[];
}

// 任务模型接口
export interface TaskModel {
  id: string;
  title: string;
  description: string;
  status: boolean;
  startTime: string;
  endTime: string;
  users?: StaffModel[];
}

// 资源模型接口
export interface ResourceModel {
  id: string;
  name: string;
  description: string;
  tag?: string;
}

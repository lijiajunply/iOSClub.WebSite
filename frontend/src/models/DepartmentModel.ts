import {ProjectModel, StaffModel} from "./ProjectModel";

// 部门模型接口
export interface Department {
  id: string;
  name: string;
  description: string;
  ministers?: StaffModel[];
  members?: StaffModel[];
  projects?: Project[];
}

export interface DepartmentModel {
  key: string;
  name: string;
  description: string;
  staffs?: StaffModel[];
  projects?: ProjectModel[];
}

export interface Project {
  id: string;
  title: string;
  description: string;
  department?: {
    id: number;
    name: string;
  };
}

// 身份枚举
export enum Identity {
  Founder = "Founder",      // 创始人
  President = "President",   // 社长,团支书,秘书长
  Minister = "Minister",     // 部长
  Department = "Department"  // 部员成员
}
import { MemberModel } from "./AuthModel";

// 部门模型接口
export interface Department {
  id: number;
  name: string;
  description: string;
  ministers?: MemberModel[];
  members?: MemberModel[];
  projects?: Project[];
}

export interface DepartmentModel {
  key: string;
  name: string;
  description: string;
  staffs?: any[];
  projects?: any[];
}

export interface Project {
  id: number;
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
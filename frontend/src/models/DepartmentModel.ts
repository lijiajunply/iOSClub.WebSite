// 部门模型接口
export interface StaffModel {
  userId: string;
  name: string;
  identity: string;
  department: DepartmentModel | null;
}

export interface Department {
  id: string;
  name: string;
  description: string;
  ministers?: StaffModel[];
  members?: StaffModel[];
}

export interface DepartmentModel {
  key: string;
  name: string;
  description: string;
  staffs?: StaffModel[];
}

// 身份枚举
export enum Identity {
  Founder = "Founder",      // 创始人
  President = "President",   // 社长,团支书,秘书长
  Minister = "Minister",     // 部长
  Department = "Department"  // 部员
}

// 员工模型 —— 对应后端 VOs/StaffVO.cs（请求侧对应 DTOs/StaffCreateDTO.cs）
// 后端不返回嵌套的部门对象，只返回扁平的 departmentName。
export interface StaffModel {
  userId: string;
  name: string;
  identity: string;
  departmentName: string | null;
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

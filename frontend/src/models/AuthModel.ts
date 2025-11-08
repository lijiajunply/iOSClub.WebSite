// 登录模型接口
export interface LoginModel {
  userId: string;
  password: string;
  rememberMe: boolean;
}

export interface MemberModel {
  identity: string;
  userName: string;
  userId: string;
  academy: string;
  politicalLandscape: string;
  gender: string;
  className: string;
  phoneNum: string;
  joinTime: string;
  passwordHash: string;
  eMail: string | null;
}

// 详细注册模型接口
export interface StudentModel {
  userName: string;
  userId: string;
  academy: string;
  politicalLandscape: string;
  gender: string;
  className: string;
  phoneNum: string;
  joinTime: string;
  passwordHash: string;
  eMail: string | null;
}
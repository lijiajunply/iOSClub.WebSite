// 登录模型接口
export interface LoginModel {
  id: string;
  name: string;
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

// 组织注册记录类
export class OrgSignRecord {
  constructor(
    public readonly url1: string,
    public readonly url2: string
  ) {
  }
}
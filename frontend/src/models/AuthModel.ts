/*
 * 本文件中的类型与后端 DTO/VO 一一对应，字段名必须与 C# 属性名保持一致
 * （后端用 System.Text.Json 默认的 camelCase 策略序列化，例如 EMail -> eMail）。
 * 修改任何字段前请先核对后端：
 *   rearend/iOSClub.WebAPI/iOSClub.Data/DTOs/
 *   rearend/iOSClub.WebAPI/iOSClub.Data/VOs/
 */

// 登录请求 —— 对应 DTOs/LoginDTO.cs
export interface LoginModel {
  userId: string;
  password: string;
  rememberMe: boolean;
}

// 成员信息（响应）—— 对应 VOs/MemberVO.cs
// 后端不下发密码哈希，因此这里没有 passwordHash 字段。
export interface MemberVO {
  userId: string;
  userName: string;
  academy: string;
  politicalLandscape: string;
  gender: string;
  className: string;
  phoneNum: string;
  joinTime: string;
  eMail: string | null;
  identity: string;
}

// 学生信息（响应）—— 对应 VOs/StudentVO.cs
export interface StudentVO {
  userId: string;
  userName: string;
  academy: string;
  politicalLandscape: string;
  gender: string;
  className: string;
  phoneNum: string;
  joinTime: string;
  eMail: string | null;
}

// 学生注册请求 —— 对应 DTOs/StudentCreateDTO.cs
// password 是明文密码，由后端用 BCrypt 哈希后存入 PasswordHash；
// joinTime 由服务端设置为当前时间，不接受客户端传入。
export interface StudentCreateDTO {
  userId: string;
  userName: string;
  academy: string;
  politicalLandscape: string;
  gender: string;
  className: string;
  phoneNum: string;
  password: string;
  eMail: string | null;
}

// 学生信息更新请求 —— 对应 DTOs/StudentUpdateDTO.cs
// 除 userId 外均为可选：后端语义是"空值表示不修改该字段"。
export interface StudentUpdateDTO {
  userId: string;
  userName?: string | null;
  academy?: string | null;
  politicalLandscape?: string | null;
  gender?: string | null;
  className?: string | null;
  phoneNum?: string | null;
  eMail?: string | null;
}

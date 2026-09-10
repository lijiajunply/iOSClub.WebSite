/**
 * 业务错误码 —— 与后端 rearend/iOSClub.WebAPI/iOSClub.WebAPI/Common/ErrorCode.cs 一一对应。
 *
 * 后端是唯一真源：改动错误码时请先改后端，再同步这里。
 * 前端通常按区间处理（见 ApiService.ts 的 toError），所以后端新增错误码一般不需要改前端。
 * 区间划分：1000-1999 参数、2000-2999 业务、3000-3999 权限、4000-4999 资源、
 *          5000-5999 系统、6000-6999 外部服务、7000-7999 HTTP 相关。
 */
export const ErrorCode = {
    Success: 0,

    // 参数错误
    ParameterEmpty: 1000,
    ParameterFormatError: 1001,
    ParameterOutOfRange: 1002,
    ParameterValidationFailed: 1003,

    // 业务逻辑错误
    ResourceAlreadyExists: 2000,
    OperationFailed: 2001,
    DataProcessingFailed: 2002,
    InvalidStatusForOperation: 2003,

    // 权限错误
    Unauthorized: 3000,
    InsufficientPermission: 3001,
    LoginExpired: 3002,
    InvalidToken: 3003,

    // 资源错误
    ResourceNotFound: 4000,
    ArticleNotFound: 4001,
    CategoryNotFound: 4002,
    UserNotFound: 4003,
    ProjectNotFound: 4004,
    FileNotFound: 4005,

    // 系统错误
    InternalServerError: 5000,
    DatabaseOperationFailed: 5001,
    CacheOperationFailed: 5002,
    NetworkError: 5003,

    // 外部服务错误
    ExternalServiceFailed: 6000,
    ExternalServiceTimeout: 6001,
    ExternalServiceReturnError: 6002,
    ExternalServiceNotConfigured: 6003,

    // HTTP 状态码相关错误
    TooManyRequests: 7000,
    InvalidRequest: 7001
} as const;

export type ErrorCodeValue = (typeof ErrorCode)[keyof typeof ErrorCode];

/**
 * 客户端应用模型
 */
export interface ClientApplication {
    /**
     * 客户端ID（主键）
     */
    ClientId: string;
    
    /**
     * 客户端密钥
     */
    ClientSecret: string;
    
    /**
     * 应用名称
     */
    ApplicationName: string;
    
    /**
     * 应用描述
     */
    Description: string;
    
    /**
     * 应用主页URL
     */
    HomepageUrl: string;
    
    /**
     * 回调URL白名单（多个URL用分号分隔）
     */
    RedirectUris: string;
    
    /**
     * 应用图标URL
     */
    LogoUrl: string;
    
    /**
     * 是否启用
     */
    IsActive: boolean;
    
    /**
     * 创建时间
     */
    CreatedAt: string;
    
    /**
     * 最后更新时间
     */
    UpdatedAt: string;
}

/**
 * 创建客户端应用模型
 */
export interface CreateClientAppModel {
    /**
     * 应用名称
     */
    ApplicationName: string;
    
    /**
     * 应用描述
     */
    Description: string;
    
    /**
     * 应用主页URL
     */
    HomepageUrl: string;
    
    /**
     * 回调URL列表
     */
    RedirectUris: string[];
    
    /**
     * 应用图标URL
     */
    LogoUrl: string;
}

/**
 * 更新客户端应用模型
 */
export interface UpdateClientAppModel {
    /**
     * 应用名称
     */
    ApplicationName: string;
    
    /**
     * 应用描述
     */
    Description: string;
    
    /**
     * 应用主页URL
     */
    HomepageUrl: string;
    
    /**
     * 回调URL列表
     */
    RedirectUris: string[];
    
    /**
     * 应用图标URL
     */
    LogoUrl: string;
    
    /**
     * 是否启用
     */
    IsActive: boolean;
}

/**
 * 客户端应用结果模型（包含密钥）
 */
export interface ClientAppResultModel {
    /**
     * 客户端ID
     */
    ClientId: string;
    
    /**
     * 客户端密钥
     */
    ClientSecret: string;
    
    /**
     * 应用名称
     */
    ApplicationName: string;
    
    /**
     * 应用描述
     */
    Description: string;
    
    /**
     * 应用主页URL
     */
    HomepageUrl: string;
    
    /**
     * 回调URL列表
     */
    RedirectUris: string[];
    
    /**
     * 应用图标URL
     */
    LogoUrl: string;
    
    /**
     * 是否启用
     */
    IsActive: boolean;
}

/**
 * 重新生成密钥结果
 */
export interface RegenerateSecretResult {
    /**
     * 客户端ID
     */
    ClientId: string;
    
    /**
     * 新的客户端密钥
     */
    NewSecret: string;
}
/**
 * 客户端应用模型
 */
export interface ClientApplication {
    /**
     * 客户端ID（主键）
     */
    clientId: string;
    
    /**
     * 客户端密钥
     */
    clientSecret: string;
    
    /**
     * 应用名称
     */
    applicationName: string;
    
    /**
     * 应用描述
     */
    description: string;
    
    /**
     * 应用主页URL
     */
    homepageUrl: string;
    
    /**
     * 回调URL白名单（多个URL用分号分隔）
     */
    redirectUris: string;
    
    /**
     * 应用图标URL
     */
    logoUrl: string;
    
    /**
     * 是否启用
     */
    isActive: boolean;
    
    /**
     * 创建时间
     */
    createdAt: string;
    
    /**
     * 最后更新时间
     */
    updatedAt: string;

    /**
     * 是否支持PKCE
     */
    supportsPkce: boolean;
}

/**
 * 创建客户端应用模型
 */
export interface CreateClientAppModel {
    /**
     * 应用名称
     */
    applicationName: string;
    
    /**
     * 应用描述
     */
    description: string;
    
    /**
     * 应用主页URL
     */
    homepageUrl: string;
    
    /**
     * 回调URL列表
     */
    redirectUris: string[];
    
    /**
     * 应用图标URL
     */
    logoUrl: string;
    
    /**
     * 是否支持PKCE
     */
    supportsPkce: boolean;
}

/**
 * 更新客户端应用模型
 */
export interface UpdateClientAppModel {
    /**
     * 应用名称
     */
    applicationName: string;
    
    /**
     * 应用描述
     */
    description: string;
    
    /**
     * 应用主页URL
     */
    homepageUrl: string;
    
    /**
     * 回调URL列表
     */
    redirectUris: string[];
    
    /**
     * 应用图标URL
     */
    logoUrl: string;
    
    /**
     * 是否启用
     */
    isActive: boolean;
    
    /**
     * 是否支持PKCE
     */
    supportsPkce: boolean;
}

/**
 * 客户端应用结果模型（包含密钥）
 */
export interface ClientAppResultModel {
    /**
     * 客户端ID
     */
    clientId: string;
    
    /**
     * 客户端密钥
     */
    clientSecret: string;
    
    /**
     * 应用名称
     */
    applicationName: string;
    
    /**
     * 应用描述
     */
    description: string;
    
    /**
     * 应用主页URL
     */
    homepageUrl: string;
    
    /**
     * 回调URL列表
     */
    redirectUris: string[];
    
    /**
     * 应用图标URL
     */
    logoUrl: string;
    
    /**
     * 是否启用
     */
    isActive: boolean;
    
    /**
     * 是否支持PKCE
     */
    supportsPkce: boolean;
}

/**
 * 重新生成密钥结果
 */
export interface RegenerateSecretResult {
    /**
     * 客户端ID
     */
    clientId: string;
    
    /**
     * 新的客户端密钥
     */
    newSecret: string;
}
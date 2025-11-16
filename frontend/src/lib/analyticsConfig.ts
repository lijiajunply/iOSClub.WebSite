// 谷歌分析配置管理工具

/**
 * 谷歌分析配置接口
 */
export interface AnalyticsConfig {
  key?: string;
  enabled?: boolean;
}

/**
 * 全局谷歌分析配置对象
 */
let analyticsConfig: AnalyticsConfig = {
  enabled: true
};

/**
 * 设置谷歌分析配置
 * @param config 配置对象
 */
export function setAnalyticsConfig(config: AnalyticsConfig): void {
  analyticsConfig = { ...analyticsConfig, ...config };
  
  // 如果设置了新的key，可以在这里重新初始化谷歌分析
  if (config.key && typeof window !== 'undefined') {
    // 动态导入谷歌分析服务以避免循环依赖
    import('../services/GoogleAnalyticsService').then(({ googleAnalyticsService }) => {
      googleAnalyticsService.initialize(config.key!);
    }).catch(err => {
      console.error('无法导入谷歌分析服务:', err);
    });
  }
}

/**
 * 获取当前的谷歌分析配置
 */
export function getAnalyticsConfig(): AnalyticsConfig {
  return { ...analyticsConfig };
}

/**
 * 获取当前应该使用的谷歌分析Key
 * 优先级：1. 手动设置的key  2. 环境变量中的key
 */
export function getAnalyticsKey(): string | undefined {
  // 如果手动设置了key，优先使用
  if (analyticsConfig.key) {
    return analyticsConfig.key;
  }
  
  // 否则使用环境变量中的key
  return import.meta.env.VITE_GOOGLE_ANALYTICS_KEY || undefined;
}

/**
 * 检查谷歌分析是否启用
 */
export function isAnalyticsEnabled(): boolean {
  return analyticsConfig.enabled !== false;
}
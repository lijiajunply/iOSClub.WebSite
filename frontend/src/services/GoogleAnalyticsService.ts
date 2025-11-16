// 谷歌分析服务
class GoogleAnalyticsService {
  private analyticsKey: string | null = null;
  private isInitialized: boolean = false;

  /**
   * 初始化谷歌分析
   * @param key 自定义的谷歌分析Key，如果不提供则使用环境变量中的值
   */
  initialize(key?: string): void {
    // 使用提供的key或环境变量中的key
    this.analyticsKey = key || import.meta.env.VITE_GOOGLE_ANALYTICS_KEY;

    if (!this.analyticsKey || this.analyticsKey === 'G-XXXXXXXXXX') {
      console.warn('谷歌分析Key未配置或使用默认值，请在环境变量中设置有效的VITE_GOOGLE_ANALYTICS_KEY');
      return;
    }

    // 避免重复初始化
    if (this.isInitialized) {
      console.warn('谷歌分析已经初始化');
      return;
    }

    // 加载谷歌分析脚本
    this.loadGoogleAnalyticsScript();
    this.isInitialized = true;
  }

  /**
   * 加载谷歌分析脚本
   */
  private loadGoogleAnalyticsScript(): void {
    // 创建gtag函数
    (window as any).dataLayer = (window as any).dataLayer || [];
    (window as any).gtag = function(...args: any[]) {
      (window as any).dataLayer.push(args);
    };
    (window as any).gtag('js', new Date());
    (window as any).gtag('config', this.analyticsKey!);

    // 添加脚本标签
    const script = document.createElement('script');
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${this.analyticsKey}`;
    document.head.appendChild(script);

    console.log('谷歌分析脚本已加载');
  }

  /**
   * 跟踪页面浏览
   * @param path 页面路径
   * @param title 页面标题
   */
  trackPageview(path: string, title?: string): void {
    if (!this.isInitialized) {
      console.warn('谷歌分析未初始化，无法跟踪页面浏览');
      return;
    }

    // 确保window.gtag存在且analyticsKey不为空
    if ((window as any).gtag && this.analyticsKey) {
      (window as any).gtag('config', this.analyticsKey, {
        page_path: path,
        page_title: title || document.title
      });
    }
  }

  /**
   * 跟踪事件
   * @param eventName 事件名称
   * @param params 事件参数
   */
  trackEvent(eventName: string, params?: Record<string, any>): void {
    if (!this.isInitialized) {
      console.warn('谷歌分析未初始化，无法跟踪事件');
      return;
    }

    // 确保window.gtag存在且analyticsKey不为空
    if ((window as any).gtag && this.analyticsKey) {
      (window as any).gtag('event', eventName, params || {});
    }
  }

  /**
   * 获取当前使用的分析Key
   */
  getAnalyticsKey(): string | null {
    return this.analyticsKey;
  }

  /**
   * 检查是否已初始化
   */
  getIsInitialized(): boolean {
    return this.isInitialized;
  }
}

// 导出单例
export const googleAnalyticsService = new GoogleAnalyticsService();
export default GoogleAnalyticsService;
/**
 * RSS服务类，用于获取和处理RSS订阅数据
 */

export interface RssArticle {
  title: string;
  url: string;
  image: string;
}

export interface AtomEntry {
  title: string;
  updated: string;
  link: Array<{ href: string }>;
}

/**
 * 获取RSS文章数据（带超时控制）
 */
export const loadRssArticles = async (): Promise<RssArticle[]> => {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), 10000); // 10秒超时
  
  try {
    const response = await fetch('https://test.xauat.site/feeds/MP_WXS_3226711201.json', { signal: controller.signal });
    if (!response.ok) {
      throw new Error(`HTTP 错误！状态码: ${response.status}`);
    }
    const text = await response.text();
    let data;
    try {
      data = JSON.parse(text);
    } catch (parseError) {
      console.error('JSON 解析失败:', parseError, '原始响应内容:', text);
      throw parseError;
    }
    console.log('RSS 数据:', data);

    if (data && data.items) {
      return data.items.map((item: any) => ({
        title: item.title || '',
        url: item.url || '',
        image: item.image || ''
      }));
    } else {
      console.warn('RSS 数据结构异常，未找到 items 字段');
      return [];
    }
  } catch (error) {
    if (error.name === 'AbortError') {
      console.error('获取 RSS 文章超时！');
    } else {
      console.error('获取 RSS 文章失败:', error);
    }
    return [];
  } finally {
    clearTimeout(timeoutId);
  }
};

/**
 * 获取Atom订阅数据（带超时控制）
 */
export const loadAtomEntries = async (): Promise<AtomEntry[]> => {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), 10000); // 10秒超时
  
  try {
    const response = await fetch('https://test.xauat.site/feeds/all.atom', { signal: controller.signal });
    if (!response.ok) {
      throw new Error(`HTTP 错误！状态码: ${response.status}`);
    }
    const xmlText = await response.text();

    const parser = new DOMParser();
    const xmlDoc = parser.parseFromString(xmlText, 'text/xml');
    const entryElements = xmlDoc.getElementsByTagName('entry');
    const entryList: AtomEntry[] = [];

    for (let i = 0; i < entryElements.length; i++) {
      const entry = entryElements[i];
      const title = entry.getElementsByTagName('title')[0]?.textContent || '';
      const updated = entry.getElementsByTagName('updated')[0]?.textContent || '';
      const linkElements = entry.getElementsByTagName('link');
      const links: Array<{ href: string }> = [];
      
      for (let j = 0; j < linkElements.length; j++) {
        const href = linkElements[j].getAttribute('href');
        if (href) {
          links.push({ href });
        }
      }
      
      entryList.push({ title, updated, link: links });
    }
    
    return entryList;
  } catch (error) {
    if (error.name === 'AbortError') {
      console.error('获取订阅文章超时！');
    } else {
      console.error('获取订阅文章失败:', error);
    }
    return [];
  } finally {
    clearTimeout(timeoutId);
  }
};
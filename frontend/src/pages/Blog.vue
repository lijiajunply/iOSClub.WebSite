<template>
  <meta name="referrer" content="never">

  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <!-- 主要内容区 -->
    <div class="container mx-auto px-4 max-w-7xl">
      <!-- 头部区域 -->
      <PageStart
          :title="isMobile ? 'iOS Club 博客' : 'iOS Club 技术博客'"
          subtitle="记录每一个思维的并发点"
          :img="articleHeaderImg"
          gradient-class="bg-gradient-to-r from-purple-600 to-pink-600"
      />

      <!-- 文章卡片区域 - 使用Naive UI NCard统一样式 -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-8 pb-16 ml-4 mr-4 mt-8">
        <n-card
            v-for="(article, index) in rssArticles"
            :key="index"
            hoverable
            class="group cursor-pointer animate-slide-up dark:bg-neutral-800 dark:text-gray-100"
            :style="`animation-delay: ${index * 100}ms`"
            @click="openArticle(article.url)"
        >
          <!-- 卡片封面（图片区域） -->
          <template #cover>
            <div class="h-48 bg-gradient-to-br from-gray-50 to-gray-100 dark:from-neutral-900 dark:to-neutral-800 flex items-center justify-center overflow-hidden">
              <template v-if="article.image">
                <img
                    :src="article.image"
                    :alt="article.title"
                    class="w-full h-full object-contain group-hover:scale-110 transition-transform duration-500"
                    @error="handleImageError($event, index)"
                    @load="handleImageLoad"
                />
              </template>
              <template v-else>
                <span class="text-gray-500 dark:text-gray-400 text-5xl">📰</span>
              </template>
            </div>
          </template>

          <!-- 卡片内容区 -->
          <div class="p-6 space-y-4">
            <h3 class="text-2xl font-semibold text-gray-900 group-hover:text-purple-600 transition-colors duration-300 text-center">
              {{ article.title }}
            </h3>
            <div class="flex items-center justify-center text-purple-600 font-medium">
              <span>阅读全文</span>
              <svg class="w-5 h-5 ml-2 group-hover:translate-x-2 transition-transform duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
              </svg>
            </div>
          </div>
        </n-card>
      </div>

      <!-- 空状态显示 -->
      <n-empty
          v-if="rssArticles.length === 0 && !loading"
          image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg"
          class="text-center py-16"
      >
        <template #description>
          <span>暂无文章</span>
        </template>
      </n-empty>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <n-spin size="large" />
      </div>

      <!-- 更多订阅 - 优化后的版本 -->
      <div v-if="entries.length > 0" class="pb-16 ml-4 mr-4 mt-8">
        <h2 class="text-3xl font-bold mb-8 text-gray-900 relative inline-block">
          更多订阅
          <span class="absolute -bottom-2 left-0 w-1/2 h-1 bg-gradient-to-r from-purple-500 to-pink-500 rounded-full"></span>
        </h2>
        <div class="space-y-4">
          <div
              v-for="(entry, index) in entries"
              :key="index"
              class="subscription-item animate-slide-up"
              :style="`animation-delay: ${index * 80}ms`"
              @click="openLink(entry.link[0].href)"
          >
            <div class="flex justify-between items-center">
              <span class="text-lg font-medium">
                {{ entry.title }}
              </span>
              <span class="text-gray-500 text-sm bg-white/50 px-3 py-1 rounded-full backdrop-blur-sm">
                {{ formatDate(entry.updated) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 背景装饰 -->
    <div class="fixed top-0 left-0 w-full h-full pointer-events-none overflow-hidden -z-10">
      <div class="absolute -top-40 -right-40 w-80 h-80 bg-purple-100 rounded-full blur-3xl opacity-30 animate-float"></div>
      <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-pink-100 rounded-full blur-3xl opacity-30 animate-float-delayed"></div>
    </div>
  </div>
</template>

<script setup>
// 脚本部分保持不变
import { onMounted, ref } from 'vue';
import { NEmpty, NSpin, NCard } from 'naive-ui';
import PageStart from "../components/PageStart.vue";

// 导入头部图片
import articleHeaderImg from '../assets/Centre/Article.jpg';

const isMobile = ref(window.innerWidth < 640);

// 数据响应式变量
const rssArticles = ref([]);
const entries = ref([]);
const loading = ref(true);

// 日期格式化函数
const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return isNaN(date.getTime()) ? '' : date.toLocaleDateString('zh-CN');
};

// 打开文章链接
const openArticle = (url) => {
  window.open(url, '_blank');
};

// 打开链接
const openLink = (url) => {
  window.open(url, '_blank');
};

// 处理图片加载错误（传入index便于定位）
const handleImageError = (event, index) => {
  console.error(`第${index}篇文章图片加载失败:`, event.target.src);
  // 替换为默认图标
  event.target.src = '';
  event.target.parentElement.innerHTML = '<span class="text-gray-500 text-5xl">📰</span>';
};

// 处理图片加载成功
const handleImageLoad = (event) => {
  console.log('图片加载成功:', event.target.src);
};

// 获取RSS文章数据（带超时控制）
const loadRssArticles = async () => {
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
      rssArticles.value = data.items.map(item => ({
        title: item.title || '',
        url: item.url || '',
        image: item.image || ''
      }));
      console.log('处理后的文章数据:', rssArticles.value);
    } else {
      console.warn('RSS 数据结构异常，未找到 items 字段');
      rssArticles.value = [];
    }
  } catch (error) {
    if (error.name === 'AbortError') {
      console.error('获取 RSS 文章超时！');
    } else {
      console.error('获取 RSS 文章失败:', error);
    }
    rssArticles.value = [];
  } finally {
    clearTimeout(timeoutId);
  }
};

// 获取更多订阅数据（带超时控制）
const loadWebArticles = async () => {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), 10000); // 10秒超时
  try {
    const response = await fetch('https://test.xauat.site/feeds/all.atom', { signal: controller.signal });
    if (!response.ok) {
      throw new Error(`HTTP 错误！状态码: ${response.status}`);
    }
    const xmlText = await response.text();
    console.log('Atom Feed 数据:', xmlText);

    const parser = new DOMParser();
    const xmlDoc = parser.parseFromString(xmlText, 'text/xml');
    const entryElements = xmlDoc.getElementsByTagName('entry');
    const entryList = [];

    for (let i = 0; i < entryElements.length; i++) {
      const entry = entryElements[i];
      const title = entry.getElementsByTagName('title')[0]?.textContent || '';
      const updated = entry.getElementsByTagName('updated')[0]?.textContent || '';
      const linkElements = entry.getElementsByTagName('link');
      const links = [];
      for (let j = 0; j < linkElements.length; j++) {
        const href = linkElements[j].getAttribute('href');
        if (href) {
          links.push({ href });
        }
      }
      entryList.push({ title, updated, link: links });
    }
    entries.value = entryList;
    console.log('处理后的订阅数据:', entries.value);
  } catch (error) {
    if (error.name === 'AbortError') {
      console.error('获取订阅文章超时！');
    } else {
      console.error('获取订阅文章失败:', error);
    }
    entries.value = [];
  } finally {
    clearTimeout(timeoutId);
  }
};

// 页面初始化时加载数据
onMounted(async () => {
  try {
    loading.value = true;
    // 并行加载，带错误捕获
    await Promise.allSettled([
      loadRssArticles(),
      loadWebArticles()
    ]);
  } catch (error) {
    console.error('数据加载过程中发生未捕获错误:', error);
  } finally {
    loading.value = false;
    console.log('加载完成，loading 状态置为 false');
  }
});
</script>

<style scoped>
/* 动画定义（与活动卡片保持一致） */
@keyframes slide-up {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px);
  }
  50% {
    transform: translateY(-20px);
  }
}

.animate-slide-up {
  opacity: 0;
  animation: slide-up 0.8s ease-out forwards;
}

.animate-float {
  animation: float 6s ease-in-out infinite;
}

.animate-float-delayed {
  animation: float 6s ease-in-out 3s infinite;
}

/* NaiveUI 卡片样式统一 */
:deep(.n-card) {
  border: none;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

:deep(.n-card:hover) {
  transform: translateY(-8px);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

:deep(.n-card__cover) {
  background: linear-gradient(135deg, #fafafa 0%, #f3f4f6 100%);
}

/* 优化后的订阅项样式 */
.subscription-item {
  cursor: pointer;
  width: 100%;
  border-radius: 12px;
  padding: 18px 20px;
  margin: 0 0 6px 0;
  font-size: 18px;
  color: #1c1f23;
  /* 半透明背景与背景融合 */
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.7) 0%, rgba(255, 255, 255, 0.9) 100%);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.5);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  overflow: hidden;
  position: relative;
}

/* 添加与背景装饰呼应的微妙效果 */
.subscription-item::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  background: linear-gradient(to bottom, #9333ea, #ec4899);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.subscription-item:hover {
  transform: translateY(-4px) scale(1.005);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.08);
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.9) 0%, rgba(255, 255, 255, 1) 100%);
}

.subscription-item:hover::before {
  opacity: 1;
}
</style>
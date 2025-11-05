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

      <!-- 文章卡片区域 - 使用 TailwindCSS 实现苹果风格 -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-8 pb-16 ml-4 mr-4 mt-8">
        <div
            v-for="(article, index) in rssArticles"
            :key="index"
            class="group cursor-pointer animate-slide-up dark:bg-neutral-800 dark:text-gray-100 bg-white rounded-2xl overflow-hidden shadow-sm hover:shadow-xl transition-all duration-300 ease-out"
            :style="`animation-delay: ${index * 100}ms`"
            @click="openArticle(article.url)"
        >
          <!-- 卡片封面（图片区域） -->
          <div class="h-48 bg-gradient-to-br from-gray-50 to-gray-100 dark:from-neutral-900 dark:to-neutral-800 flex items-center justify-center overflow-hidden">
            <template v-if="article.image">
              <img
                  :src="article.image"
                  :alt="article.title"
                  class="w-full h-full object-contain group-hover:scale-105 transition-transform duration-500"
                  @error="handleImageError($event, index)"
                  @load="handleImageLoad"
              />
            </template>
            <template v-else>
              <span class="text-gray-500 dark:text-gray-400 text-5xl">📰</span>
            </template>
          </div>

          <!-- 卡片内容区 -->
          <div class="p-6 space-y-4">
            <h3 class="text-2xl font-semibold text-gray-700 dark:text-gray-200 group-hover:text-purple-600 transition-colors duration-300 text-center line-clamp-2">
              {{ article.title }}
            </h3>
            <div class="flex items-center justify-center text-purple-600 font-medium">
              <span>阅读全文</span>
              <svg class="w-5 h-5 ml-2 group-hover:translate-x-2 transition-transform duration-300" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
              </svg>
            </div>
          </div>
        </div>
      </div>

      <!-- 空状态显示 -->
      <div
          v-if="rssArticles.length === 0 && !loading"
          class="text-center py-16"
      >
        <div class="flex justify-center mb-4">
          <svg class="w-16 h-16 text-gray-400" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z"></path>
          </svg>
        </div>
        <p class="text-gray-500 dark:text-gray-400 text-lg">暂无文章</p>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-purple-500"></div>
      </div>

      <!-- 更多订阅 - 优化后的版本 -->
      <div v-if="entries.length > 0" class="pb-16 ml-4 mr-4 mt-8">
        <h2 class="text-3xl font-bold mb-8 text-gray-900 dark:text-gray-100 relative inline-block">
          更多订阅
          <span class="absolute -bottom-2 left-0 w-1/2 h-1 bg-gradient-to-r from-purple-500 to-pink-500 rounded-full"></span>
        </h2>
        <div class="space-y-4">
          <div
              v-for="(entry, index) in entries"
              :key="index"
              class="subscription-item animate-slide-up bg-white dark:bg-neutral-800 rounded-xl p-5 shadow-sm hover:shadow-md transition-all duration-300 ease-out"
              :style="`animation-delay: ${index * 80}ms`"
              @click="openLink(entry.link[0].href)"
          >
            <div class="flex justify-between items-center">
              <span class="text-lg font-medium text-gray-800 dark:text-gray-200">
                {{ entry.title }}
              </span>
              <span class="text-gray-500 text-sm px-3 py-1 rounded-full bg-gray-100 dark:bg-neutral-700">
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
import PageStart from "../components/PageStart.vue";
import { loadRssArticles, loadAtomEntries } from '../services/RssService'; // 引入新的RSS服务

// 导入头部图片
import articleHeaderImg from '/assets/Centre/Article.jpg';

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

// 页面初始化时加载数据
onMounted(async () => {
  try {
    loading.value = true;
    // 使用从服务导入的方法并行加载，带错误捕获
    const [rssResult, atomResult] = await Promise.allSettled([
      loadRssArticles(),
      loadAtomEntries()
    ]);
    
    // 处理结果
    if (rssResult.status === 'fulfilled') {
      rssArticles.value = rssResult.value;
    }
    
    if (atomResult.status === 'fulfilled') {
      entries.value = atomResult.value;
    }
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

/* 优化后的订阅项样式 */
.subscription-item {
  cursor: pointer;
  width: 100%;
  transition: all 0.3s ease;
  overflow: hidden;
  position: relative;
}

.subscription-item:hover {
  transform: translateY(-2px);
}

/* 添加线条动画效果 */
.subscription-item::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 0;
  height: 100%;
  background: linear-gradient(to right, rgba(147, 51, 234, 0.1), rgba(236, 72, 153, 0.1));
  transition: width 0.3s ease;
  z-index: 0;
}

.subscription-item:hover::before {
  width: 100%;
}

.subscription-item > * {
  position: relative;
  z-index: 1;
}
</style>
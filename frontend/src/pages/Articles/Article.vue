<template>
  <div class="min-h-screen">
    <div v-if="loading" class="flex justify-center items-center h-screen">
      <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
    </div>
    
    <div v-else-if="article" class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="bg-white dark:bg-neutral-800 rounded-2xl shadow-lg overflow-hidden">
        <MarkdownComponent :content="articleContent" :show-nav="true" />
      </div>
    </div>
    
    <div v-else-if="!loading && !article" class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <div class="bg-white dark:bg-neutral-800 rounded-2xl shadow-lg p-12 text-center">
        <div class="flex justify-center mb-6">
          <svg class="w-16 h-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
          </svg>
        </div>
        <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">文章未找到</h2>
        <p class="text-gray-600 dark:text-gray-400 mb-6">抱歉，您访问的文章不存在或已被删除。</p>
        <button 
          @click="goBack"
          class="px-6 py-3 bg-blue-600 text-white rounded-full hover:bg-blue-700 transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          返回
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArticleService } from '../../services/ArticleService';
import MarkdownComponent from '../../components/MarkdownComponent.vue';

const route = useRoute();
const router = useRouter();

const article = ref(null);
const loading = ref(true);

// 根据权限值获取显示标签
const getIdentityLabel = (identity) => {
  const options = [
    {label: '所有人', value: 'Member'},
    {label: '部员', value: 'Department'},
    {label: '部长', value: 'Minister'},
    {label: '社长', value: 'President'},
    {label: '创始人', value: 'Founder'}
  ];
  const option = options.find(item => item.value === identity);
  return option ? option.label : '未知';
};

// 根据权限值获取标签样式
const getIdentityClass = (identity) => {
  switch (identity) {
    case 'Member':
      return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100';
    case 'Department':
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-100';
    case 'Minister':
      return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-100';
    case 'President':
      return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-100';
    case 'Founder':
      return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-100';
    default:
      return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-100';
  }
};

// 日期格式化函数
const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return isNaN(date.getTime()) ? '' : date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

// 返回
const goBack = () => {
  router.go(-1);
};

// 为 MarkdownComponent 准备内容数据
const articleContent = computed(() => {
  if (!article.value) return null;
  
  return {
    title: article.value.title,
    date: article.value.lastWriteTime,
    content: article.value.content,
    watch: 0 // 观看次数，如果后端提供可以替换
  };
});

// 加载文章详情
const loadArticle = async (path) => {
  try {
    loading.value = true;
    console.log('正在获取文章:', path); // 添加调试日志
    const result = await ArticleService.getArticle(path);
    console.log('获取到的文章数据:', result); // 添加调试日志
    article.value = result;
  } catch (error) {
    console.error('获取文章详情失败:', error);
    article.value = null;
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  const path = route.params.id;
  console.log('路由参数中的文章ID:', path); // 添加调试日志
  if (path) {
    loadArticle(path);
  } else {
    loading.value = false;
    article.value = null;
  }
});
</script>

<style scoped>
/* 添加一些额外的样式来美化内容显示 */
.prose :deep(h1) {
  font-size: 2em;
  font-weight: bold;
  margin-top: 1em;
  margin-bottom: 0.5em;
}

.prose :deep(h2) {
  font-size: 1.5em;
  font-weight: bold;
  margin-top: 1em;
  margin-bottom: 0.5em;
}

.prose :deep(h3) {
  font-size: 1.25em;
  font-weight: bold;
  margin-top: 1em;
  margin-bottom: 0.5em;
}

.prose :deep(p) {
  margin-top: 0.5em;
  margin-bottom: 0.5em;
  line-height: 1.75;
}

.prose :deep(ul), .prose :deep(ol) {
  margin-top: 0.5em;
  margin-bottom: 0.5em;
  padding-left: 1.5em;
}

.prose :deep(li) {
  margin-bottom: 0.25em;
}

.prose :deep(a) {
  color: #3b82f6;
  text-decoration: underline;
}

.prose :deep(blockquote) {
  border-left: 4px solid #94a3b8;
  padding-left: 1rem;
  margin-left: 0;
  margin-right: 0;
  color: #64748b;
}

.prose :deep(code) {
  background-color: #f1f5f9;
  padding: 0.2em 0.4em;
  border-radius: 0.25em;
  font-size: 0.875em;
}

.dark .prose :deep(code) {
  background-color: #334155;
}

.prose :deep(pre) {
  background-color: #f1f5f9;
  padding: 1em;
  border-radius: 0.5em;
  overflow-x: auto;
}

.dark .prose :deep(pre) {
  background-color: #334155;
}
</style>
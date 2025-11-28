<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <!-- 顶部导航栏 -->
    <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <div class="flex items-center">
            <n-button text @click="goBack" class="mr-4">
              <Icon icon="material-symbols:arrow-back" class="w-6 h-6 text-gray-600 dark:text-gray-300"/>
            </n-button>
            <h1 class="ml-2 text-lg font-semibold text-gray-900 dark:text-white">
              分类设置 - {{ category.name || '加载中' }}
            </h1>
          </div>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <!-- 分类信息卡片 -->
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-sm mb-6">
        <div class="p-6">
          <h2 class="text-lg font-medium text-gray-900 dark:text-white mb-4">分类信息</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">分类名称</label>
              <n-input
                v-model:value="category.name"
                placeholder="请输入分类名称"
                class="rounded-lg"
                :bordered="true"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">分类ID</label>
              <n-input
                :value="category.id"
                placeholder="分类ID"
                class="rounded-lg"
                :bordered="true"
                disabled
              />
            </div>
          </div>
          <div class="mt-4">
            <n-button
              type="primary"
              @click="updateCategory"
              :loading="updatingCategory"
              class="rounded-lg"
            >
              更新分类信息
            </n-button>
          </div>
        </div>
      </div>

      <!-- 文章排序卡片 -->
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-sm">
        <div class="p-6">
          <div class="flex justify-between items-center mb-6">
            <h2 class="text-lg font-medium text-gray-900 dark:text-white">文章排序</h2>
            <n-button
              type="success"
              @click="updateArticleOrders"
              :loading="updatingArticles"
              class="rounded-lg"
            >
              保存文章顺序
            </n-button>
          </div>

          <!-- 骨架加载 -->
          <div v-if="loadingArticles" class="space-y-4">
            <div v-for="i in 5" :key="i" class="flex items-center p-4 bg-gray-50 dark:bg-gray-750 rounded-lg">
              <div class="bg-gray-200 dark:bg-gray-700 p-2 rounded mr-4 w-8 h-8"></div>
              <div class="flex-1">
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
                <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
              </div>
            </div>
          </div>

          <!-- 文章列表 -->
          <draggable
            v-else
            v-model="articles"
            item-key="path"
            handle=".drag-handle"
            ghost-class="bg-blue-50 dark:bg-blue-900/20 shadow-md"
            chosen-class="ring-2 ring-blue-500"
            @start="onDragStart"
            @end="onDragEnd"
            class="space-y-4"
          >
            <template #item="{ element: article }">
              <div 
                class="flex items-center p-4 bg-gray-50 dark:bg-gray-750 rounded-lg hover:shadow-md transition-all duration-200 cursor-move"
              >
                <Icon 
                  icon="material-symbols:drag-indicator" 
                  class="drag-handle text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors cursor-grab active:cursor-grabbing mr-4"
                />
                <div class="flex-1">
                  <h3 class="font-medium text-gray-900 dark:text-white">{{ article.title || '无标题文章' }}</h3>
                  <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">{{ article.path }}</p>
                </div>
                <n-tag
                  size="small"
                  :type="getIdentityType(article.identity)"
                  :bordered="false"
                  class="rounded-full px-2 py-0.5 ml-4"
                >
                  {{ getIdentityLabel(article.identity || '') }}
                </n-tag>
              </div>
            </template>
          </draggable>

          <!-- 空状态 -->
          <div
            v-if="articles.length === 0 && !loadingArticles"
            class="py-12 flex flex-col items-center justify-center rounded-lg bg-gray-50 dark:bg-gray-750"
          >
            <div class="w-16 h-16 flex items-center justify-center bg-gray-100 dark:bg-gray-700 rounded-full mb-4">
              <Icon icon="material-symbols:article-off" class="text-gray-400 dark:text-gray-500 w-7 h-7" />
            </div>
            <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-1">暂无文章</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400">该分类下还没有文章</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useMessage, NButton, NInput, NTag } from 'naive-ui';
import { Icon } from '@iconify/vue';
import draggable from 'vuedraggable';
import { CategoryService } from '../services/CategoryService';
import { ArticleService } from '../services/ArticleService';
import type { CategoryModel } from '../models';
import type { ArticleModel } from '../models';

const route = useRoute();
const router = useRouter();
const message = useMessage();

const categoryId = ref<string>(route.params.id as string);
const category = ref<CategoryModel>({ id: '', name: '', order: 0 });
const articles = ref<ArticleModel[]>([]);
const loadingArticles = ref(false);
const updatingCategory = ref(false);
const updatingArticles = ref(false);

// 根据权限值获取显示标签
const getIdentityLabel = (identity: string) => {
  const options = [
    { label: '所有人', value: 'Member' },
    { label: '部员', value: 'Department' },
    { label: '部长', value: 'Minister' },
    { label: '社长', value: 'President' },
    { label: '创始人', value: 'Founder' }
  ];
  const option = options.find(item => item.value === identity);
  return option ? option.label : '未知';
};

// 根据权限值获取标签类型
const getIdentityType = (identity: string | null | undefined) => {
  switch (identity) {
    case 'Member':
      return 'success';
    case 'Department':
      return 'info';
    case 'Minister':
      return 'warning';
    case 'President':
      return 'error';
    case 'Founder':
      return 'default';
    default:
      return 'default';
  }
};

// 加载分类信息
const loadCategory = async () => {
  try {
    const categories = await CategoryService.getAllCategories();
    const foundCategory = categories.find(c => c.id === categoryId.value);
    if (foundCategory) {
      category.value = foundCategory;
    } else {
      message.error('分类不存在');
      goBack();
    }
  } catch (error) {
    console.error('加载分类失败:', error);
    message.error('加载分类失败');
    goBack();
  }
};

// 加载分类下的文章
const loadArticles = async () => {
  try {
    loadingArticles.value = true;
    const allArticles = await ArticleService.getAllArticles();
    // 过滤出当前分类的文章
    articles.value = allArticles.filter(article => article.category?.id === categoryId.value);
    // 按当前顺序排序
    articles.value.sort((a, b) => (a.articleOrder || 0) - (b.articleOrder || 0));
  } catch (error) {
    console.error('加载文章失败:', error);
    message.error('加载文章失败');
  } finally {
    loadingArticles.value = false;
  }
};

// 更新分类信息
const updateCategory = async () => {
  try {
    updatingCategory.value = true;
    await CategoryService.createOrUpdateCategory(category.value);
    message.success('分类信息更新成功');
  } catch (error) {
    console.error('更新分类失败:', error);
    message.error('更新分类失败');
  } finally {
    updatingCategory.value = false;
  }
};

// 更新文章顺序
const updateArticleOrders = async () => {
  try {
    updatingArticles.value = true;
    // 构建文章顺序更新请求
    for (let i = 0; i < articles.value.length; i++) {
      const article = articles.value[i];
      await ArticleService.updateArticle(article.path, {
        articleOrder: i
      });
    }
    message.success('文章顺序更新成功');
  } catch (error) {
    console.error('更新文章顺序失败:', error);
    message.error('更新文章顺序失败');
  } finally {
    updatingArticles.value = false;
  }
};

// 拖拽开始事件
const onDragStart = () => {
  // 添加全局样式防止选中文本
  document.body.classList.add('dragging');
};

// 拖拽结束事件
const onDragEnd = () => {
  // 移除全局样式
  document.body.classList.remove('dragging');
};

// 返回分类管理页面
const goBack = () => {
  router.push('/Centre/Category');
};

onMounted(async () => {
  await loadCategory();
  await loadArticles();
});
</script>

<style scoped>
/* 防止拖拽时选中文本 */
div {
  user-select: none;
}

/* 拖拽时的全局样式 */
:deep(.dragging) {
  user-select: none;
}

/* 拖拽手柄样式优化 */
.drag-handle {
  user-select: none;
  -webkit-user-drag: none;
  -khtml-user-drag: none;
  -moz-user-drag: none;
  -o-user-drag: none;
  user-drag: none;
}

/* 拖拽时的幽灵元素样式 */
:deep(.bg-blue-50) {
  background-color: rgb(239 246 255 / 1) !important;
}

:deep(.dark\:bg-blue-900\/20) {
  background-color: rgb(30 58 138 / 0.2) !important;
}

:deep(.shadow-md) {
  box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1) !important;
}

/* 拖拽时选中元素的样式 */
:deep(.ring-2) {
  outline: 2px solid rgb(59 130 246);
  outline-offset: 2px;
}
</style>
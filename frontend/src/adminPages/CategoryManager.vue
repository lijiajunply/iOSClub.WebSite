<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <!-- 页面头部 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="flex items-center space-x-3">
          <div>
            <h1 class="text-2xl font-semibold tracking-tight">分类管理</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">管理文章分类及其显示顺序</p>
          </div>
        </div>
      </div>
    </div>

    <!-- 主内容区 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-12">
      <!-- 骨架加载 -->
      <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
        <div v-for="i in 8" :key="i" class="rounded-xl bg-white/90 dark:bg-gray-800/90 backdrop-blur-md border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
          <div class="p-5">
            <div class="flex items-center mb-4">
              <div class="bg-gray-200 dark:bg-gray-700 p-2.5 rounded-xl mr-3 w-10 h-10"></div>
              <div class="bg-gray-200 dark:bg-gray-700 rounded-full h-5 w-16"></div>
            </div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
            <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/3"></div>
          </div>
        </div>
      </div>

      <!-- 分类列表 -->
      <draggable
        v-else
        v-model="categories"
        item-key="id"
        handle=".drag-handle"
        ghost-class="drag-ghost"
        chosen-class="drag-chosen"
        @start="onCategoryDragStart"
        @end="onCategoryDragEnd"
        class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
      >
        <template #item="{ element: category }">
          <div 
            class="group rounded-xl bg-white/90 dark:bg-gray-800/90 backdrop-blur-md border border-gray-200 dark:border-gray-700 shadow-sm hover:shadow-md transition-all duration-300 overflow-hidden cursor-move"
          >
            <div class="p-5">
              <div class="flex items-center justify-between mb-4">
                <div class="flex items-center">
                  <Icon 
                    icon="material-symbols:drag-indicator" 
                    class="drag-handle text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors cursor-grab active:cursor-grabbing mr-3"
                  />
                </div>
                <n-button
                  type="primary"
                  size="small"
                  quaternary
                  circle
                  class="h-8 w-8 p-0 rounded-full"
                  @click.stop="goToCategorySetting(category)"
                >
                  <template #icon>
                    <Icon icon="material-symbols:settings" class="w-4 h-4" />
                  </template>
                </n-button>
              </div>
              
              <h3 class="font-medium text-lg mb-2 text-gray-900 dark:text-gray-100">{{ category.name }}</h3>
              
              <div class="flex items-center text-sm text-gray-500 dark:text-gray-400">
                <Icon icon="material-symbols:sort" class="mr-1 w-4 h-4" />
                <span>顺序: {{ category.order }}</span>
              </div>
            </div>
          </div>
        </template>
      </draggable>

      <!-- 空状态 -->
      <div
        v-if="categories.length === 0 && !loading"
        class="col-span-full py-16 flex flex-col items-center justify-center rounded-2xl bg-white/70 dark:bg-gray-800/70 backdrop-blur-sm border border-dashed border-gray-300 dark:border-gray-600"
      >
        <div class="w-16 h-16 flex items-center justify-center bg-gray-100 dark:bg-gray-700 rounded-full mb-4">
          <Icon icon="material-symbols:category-off" class="text-gray-400 dark:text-gray-500 w-7 h-7" />
        </div>
        <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-1">暂无分类</h3>
        <p class="text-sm text-gray-500 dark:text-gray-400">请先创建分类</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { NTag, NButton, useMessage } from 'naive-ui';
import { Icon } from '@iconify/vue';
import draggable from 'vuedraggable';
import { CategoryService } from '../services/CategoryService';
import type { CategoryModel } from '../models';

const message = useMessage();
const router = useRouter();
const loading = ref(true);
const categories = ref<CategoryModel[]>([]);

// 加载所有分类
const loadCategories = async () => {
  try {
    loading.value = true;
    categories.value = await CategoryService.getAllCategories();
  } catch (error) {
    message.error((error as Error).message || '加载分类失败');
  } finally {
    loading.value = false;
  }
};

// 跳转到单个分类设置页面
const goToCategorySetting = (category: CategoryModel) => {
  router.push(`/Centre/Category/${category.id}`);
};

// 拖拽开始事件
const onCategoryDragStart = () => {
  // 添加全局样式防止选中文本
  document.body.classList.add('dragging');
};

// 拖拽结束事件
const onCategoryDragEnd = async () => {
  try {
    // 构建分类顺序字典 - 使用ID而不是名称
    const categoryOrders: Record<string, number> = {};
    categories.value.forEach((category, index) => {
      category.order = index;
      categoryOrders[category.id] = index;
    });
    
    // 批量更新分类顺序
    await CategoryService.updateCategoryOrders(categoryOrders);
    
    message.success('分类顺序更新成功');
  } catch (error) {
    message.error((error as Error).message || '更新分类顺序失败');
    // 失败时重新加载数据
    await loadCategories();
  } finally {
    // 移除全局样式
    document.body.classList.remove('dragging');
  }
};

onMounted(() => {
  loadCategories();
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
:deep(.drag-ghost) {
  background-color: rgb(239 246 255 / 1) !important;
  background-color: rgb(30 58 138 / 0.2) !important;
  box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1) !important;
  opacity: 0.8;
}

/* 拖拽时选中元素的样式 */
:deep(.drag-chosen) {
  outline: 2px solid rgb(59 130 246);
  outline-offset: 2px;
}
</style>

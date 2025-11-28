<template>
  <n-menu
      v-if="!loading"
      :value="activeKey"
      :options="menuOptions"
      :root-indent="24"
      :indent="36"
      @update:value="handleSelect"
      class="apple-menu"
  />
  <div v-else class="p-6">
    <div class="flex items-center justify-center">
      <n-skeleton width="200" height="20" class="mb-2"/>
    </div>
    <div class="space-y-2 mt-4">
      <n-skeleton width="160" height="32" class="ml-8"/>
      <n-skeleton width="160" height="32" class="ml-8"/>
      <n-skeleton width="160" height="32" class="ml-8"/>
      <n-skeleton width="200" height="20" class="mt-6"/>
      <n-skeleton width="160" height="32" class="ml-8"/>
      <n-skeleton width="160" height="32" class="ml-8"/>
    </div>
  </div>
</template>

<script setup lang="ts">
import {computed, onMounted, ref} from 'vue'
import {useRoute, useRouter} from 'vue-router'
import {NMenu, NSkeleton} from 'naive-ui'
import {ArticleService} from '../services/ArticleService'
import type {ArticleModel} from '../models'

const emit = defineEmits(['menu-item-click'])
const route = useRoute()
const router = useRouter()

const activeKey = computed(() => route.path)
const loading = ref(true)
const menuOptions = ref<any[]>([])

// 用于跟踪是否正在刷新当前页面
const isReloading = ref(false)

// 从后端获取分类文章并生成菜单
const fetchCategoryArticles = async () => {
  // 定义默认菜单
  const defaultMenu = [
    {
      type: 'group',
      label: '社团简介',
      key: 'about-group',
      children: [
        {
          label: '关于我们',
          key: '/About'
        },
        {
          label: '社团结构',
          key: '/Structure'
        },
        {
          label: '其他组织',
          key: '/OtherOrg'
        }
      ]
    }
  ]

  const l = ['About', 'Structure']

  try {
    loading.value = true
    const categoryArticles = await ArticleService.getAllCategoryArticles()

    // 转换为菜单选项格式
    // 合并菜单：默认菜单 + 过滤后的API菜单
    menuOptions.value = Object.entries(categoryArticles).map(([category, articles]) => {
      const c = articles.map((article: ArticleModel) => ({
        label: article.title || '无标题',
        key: l.includes(article.path) ? `/${article.path}` : `/Article/${article.path}`
      }));

      if (category === '社团简介') {
         c.push({
           label: '其他组织',
           key: '/OtherOrg'
         })
      }

      return {
        type: 'group',
        label: category,
        key: `${category}-group`,
        children: c
      }
    })
  } catch (error) {
    console.error('获取分类文章失败:', error)
    // 失败时使用默认菜单
    menuOptions.value = defaultMenu
  } finally {
    loading.value = false
  }
}

const handleSelect = (key: string) => {
  emit('menu-item-click', key)
  // 如果当前路由与点击的路由相同，则刷新页面
  if (route.path === key) {
    // 设置刷新状态并强制刷新当前页面
    isReloading.value = true
    window.location.reload()
  } else {
    // 跳转到新页面
    router.push(key)
  }
}

onMounted(() => {
  fetchCategoryArticles()
})
</script>

<style>
@reference 'tailwindcss';

/* 苹果风格的菜单样式 */
.apple-menu {
  background: transparent !important;
}

.apple-menu .n-menu-item-group-title {
  font-size: 20px;
  font-weight: 600;
  color: #86868b;
  text-transform: uppercase;
  letter-spacing: 0.01em;
  margin: 12px 24px 4px;
  padding-left: 0 !important;
  @apply border-b-1  border-b-gray-200 dark:border-b-gray-600 ;
}

.apple-menu .n-menu-item {
  font-size: 15px;
  font-weight: 400;
  color: #1d1d1f;
  border-radius: 8px;
  margin: 2px 12px;
  transition: all 0.2s ease;
}

.apple-menu .n-menu-item-content {
  padding: 10px 16px !important;
}

/* 动画效果 */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.apple-menu {
  animation: fadeIn 0.3s ease-in;
}
</style>
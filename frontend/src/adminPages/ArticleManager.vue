<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 页面操作栏 -->
      <div class="flex items-center justify-end space-x-3 mb-8">
        <n-button
          type="primary"
          size="small"
          @click="goToCategoryManager"
          class="rounded-full bg-blue-500 hover:bg-blue-600"
        >
          <template #icon>
            <Icon icon="material-symbols:category" class="w-4 h-4"/>
          </template>
          管理分类和文章排序
        </n-button>
      </div>
      
      <!-- 主要内容区域 -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
        <!-- 骨架加载 -->
        <template v-if="loading">
          <div v-for="i in 8" :key="i"
               class="rounded-2xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            <div class="p-5 h-full flex flex-col">
              <div class="flex items-center mb-3">
                <div class="bg-gray-100 dark:bg-gray-700 p-2.5 rounded-xl mr-3 w-10 h-10"></div>
                <div class="bg-gray-100 dark:bg-gray-700 rounded-full h-5 w-16"></div>
              </div>

              <div class="flex-1">
                <div class="h-4 bg-gray-100 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
                <div class="h-4 bg-gray-100 dark:bg-gray-700 rounded w-1/2"></div>
                <div class="h-3 bg-gray-100 dark:bg-gray-700 rounded w-1/3 mt-3"></div>
              </div>

              <div class="flex justify-end space-x-2 pt-3">
                <div class="bg-gray-100 dark:bg-gray-700 rounded-full w-8 h-8"></div>
                <div class="bg-gray-100 dark:bg-gray-700 rounded-full w-8 h-8"></div>
              </div>
            </div>
          </div>
        </template>

        <!-- 文章卡片 -->
        <template v-else>
          <div
              v-for="article in articles"
              :key="article.path"
              class="group rounded-2xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-sm hover:shadow-lg hover:scale-[1.02] transition-all duration-300 overflow-hidden cursor-pointer"
              @click="editArticle(article)"
          >
            <div class="p-5 h-full flex flex-col">
              <div class="flex items-center mb-3">
                <div class="bg-indigo-50 dark:bg-indigo-900/20 p-2.5 rounded-xl mr-3 border border-indigo-100 dark:border-indigo-800/30">
                  <Icon icon="material-symbols:article" class="text-indigo-600 dark:text-indigo-400 w-5 h-5"/>
                </div>
                <n-tag
                    size="small"
                    :type="getIdentityType(article.identity)"
                    :bordered="false"
                    class="rounded-full px-2 py-0.5">
                  {{ getIdentityLabel(article.identity || '') }}
                </n-tag>
              </div>

              <div class="flex-1">
                <h3 class="font-medium text-base md:text-lg mb-2 line-clamp-2 text-gray-900 group-hover:text-indigo-600 dark:text-white dark:group-hover:text-indigo-400 transition-colors">
                  {{ article.title || '无标题文章' }}
                </h3>
                <div class="flex flex-wrap items-center gap-2 text-xs text-gray-500 dark:text-gray-400 mt-1">
                  <span class="flex items-center">
                    <Icon icon="material-symbols:category" class="mr-1 w-3 h-3"/>
                    {{ article.category?.name || '其他' }}
                  </span>
                  <span class="flex items-center">
                    <Icon icon="material-symbols:schedule" class="mr-1 w-3 h-3"/>
                    {{ formatDate(article.lastWriteTime) }}
                  </span>
                </div>
              </div>

              <!-- 操作按钮 -->
              <div
                  class="flex justify-end space-x-2 pt-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                <n-button
                    type="primary"
                    size="small"
                    quaternary
                    circle
                    class="h-8 w-8 p-0 rounded-full"
                    @click.stop="editArticle(article)"
                >
                  <template #icon>
                    <Icon icon="material-symbols:edit" class="w-4 h-4"/>
                  </template>
                </n-button>
                <n-button
                    type="error"
                    size="small"
                    quaternary
                    circle
                    class="h-8 w-8 p-0 rounded-full"
                    @click.stop="deleteArticle(article)"
                >
                  <template #icon>
                    <Icon icon="material-symbols:delete" class="w-4 h-4"/>
                  </template>
                </n-button>
              </div>
            </div>
          </div>

          <!-- 空状态 -->
          <div
              v-if="articles.length === 0 && !loading"
              class="col-span-full py-16 flex flex-col items-center justify-center rounded-2xl bg-white dark:bg-gray-800 border border-dashed border-gray-200 dark:border-gray-700"
          >
            <div class="w-16 h-16 flex items-center justify-center bg-gray-50 dark:bg-gray-700 rounded-full mb-4 border border-gray-100 dark:border-gray-600">
              <Icon icon="material-symbols:article-off" class="text-gray-400 dark:text-gray-500 w-7 h-7"/>
            </div>
            <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-1">暂无文章</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">创建您的第一篇文章开始管理</p>
            <n-button
                type="primary"
                class="rounded-full bg-blue-500 hover:bg-blue-600 px-6 py-2"
                @click="openCreateModal">
              <template #icon>
                <Icon icon="material-symbols:add-circle" class="w-4.5 h-4.5"/>
              </template>
              创建第一篇文章
            </n-button>
          </div>
        </template>
      </div>
      
      <!-- 添加文章按钮 -->
      <div class="fixed bottom-6 right-6 z-10">
        <button
            class="bg-blue-600 text-white w-14 h-14 rounded-full shadow-lg flex items-center justify-center hover:bg-blue-700 hover:scale-105 transition-all duration-200"
            @click="openCreateModal"
        >
          <Icon icon="material-symbols:add" class="w-6 h-6"/>
        </button>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount} from 'vue'
import {useRouter} from 'vue-router'
import {useDialog, useMessage, NButton, NTag} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {ArticleService} from '../services/ArticleService'
import type {ArticleModel} from '../models'
import {useLayoutStore} from '../stores/LayoutStore'

const router = useRouter()
const dialog = useDialog()
const message = useMessage()
const layoutStore = useLayoutStore()

const articles = ref<ArticleModel[]>([])
const loading = ref(false)

// 根据权限值获取显示标签
const getIdentityLabel = (identity: string) => {
  const options = [
    {label: '所有人', value: 'Member'},
    {label: '部员', value: 'Department'},
    {label: '部长', value: 'Minister'},
    {label: '社长', value: 'President'},
    {label: '创始人', value: 'Founder'}
  ]
  const option = options.find(item => item.value === identity)
  return option ? option.label : '未知'
}

// 根据权限值获取标签类型
const getIdentityType = (identity: string | null | undefined) => {
  switch (identity) {
    case 'Member':
      return 'success'
    case 'Department':
      return 'info'
    case 'Minister':
      return 'warning'
    case 'President':
      return 'error'
    case 'Founder':
      return 'default'
    default:
      return 'default'
  }
}

// 格式化日期
const formatDate = (dateString: string) => {
  if (!dateString) return '未设置'
  try {
    const date = new Date(dateString)
    return date.toLocaleDateString('zh-CN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  } catch (error) {
    console.error('日期格式化失败:', error)
    return dateString
  }
}

// 获取所有文章
const fetchArticles = async () => {
  try {
    loading.value = true
    const data = await ArticleService.getAllArticles()
    articles.value = Array.isArray(data) ? data : []
  } catch (error) {
    console.error('获取文章列表失败:', error)
    message.error('获取文章列表失败: ' + (error instanceof Error ? error.message : String(error)))
  } finally {
    loading.value = false
  }
}

// 打开创建文章页面
const openCreateModal = () => {
  router.push('/Centre/Article/edit')
}

// 编辑文章
const editArticle = (article: ArticleModel) => {
  router.push(`/Centre/Article/edit/${article.path}`)
}

// 跳转到分类管理页面
const goToCategoryManager = () => {
  router.push('/Centre/Category')
}

// 删除文章
const deleteArticle = (article: ArticleModel) => {
  dialog.warning({
    title: '确认删除',
    content: `确定要删除文章"${article.title || '无标题'}"吗？此操作不可恢复。`,
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        // 调用删除API
        await ArticleService.deleteArticle(article.path)
        message.success('文章删除成功')
        // 重新获取文章列表
        await fetchArticles()
      } catch (error) {
        console.error('删除文章失败:', error)
        message.error('删除文章失败: ' + (error instanceof Error ? error.message : String(error)))
      }
    }
  })
}

onMounted(() => {
  // Set page header
  layoutStore.setPageHeader(
      '社团文章管理',
      '创建、编辑和删除社团文章'
  )

  // Show page actions
  layoutStore.setShowPageActions(true)

  fetchArticles()
})

onBeforeUnmount(() => {
  // Clear page header
  layoutStore.clearPageHeader()
})
</script>
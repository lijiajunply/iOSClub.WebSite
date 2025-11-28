<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 页面标题 -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <button @click="goBack" class="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors duration-200">
              <Icon icon="material-symbols:arrow-back" class="w-5 h-5 text-gray-600 dark:text-gray-300"/>
            </button>
            <h1 class="ml-3 text-2xl font-semibold tracking-tight">
              {{ editingArticle ? '编辑文章' : '新建文章' }}
            </h1>
          </div>
          <div class="flex items-center space-x-3">
            <n-button 
              @click="saveArticle" 
              type="primary" 
              :loading="saving" 
              class="rounded-full bg-blue-500 hover:bg-blue-600"
            >
              保存文章
            </n-button>
          </div>
        </div>
      </div>
      
      <!-- 内容区域 -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- 编辑区域 -->
        <div class="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden transition-all duration-300 hover:shadow-lg">
          <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <h2 class="text-lg font-semibold tracking-tight">编辑器</h2>
          </div>
          <div class="p-6">
            <n-form :model="editForm" ref="formRef" :rules="rules" class="space-y-6">
              <n-form-item label="文章路径 (Path)" path="path">
                <n-input
                    v-model:value="editForm.path"
                    :disabled="!!editingArticle"
                    placeholder="请输入文章路径，如：About"
                    class="rounded-xl"
                    :bordered="true"
                />
                <template #feedback>
                  <p class="text-xs mt-1 text-gray-500 dark:text-gray-400">
                    路径将作为文章的唯一标识，创建后不可修改
                  </p>
                </template>
              </n-form-item>

              <n-form-item label="文章标题" path="title">
                <n-input
                    v-model:value="editForm.title"
                    placeholder="请输入文章标题"
                    class="rounded-xl"
                    :bordered="true"
                />
              </n-form-item>

              <n-form-item label="文章分类" path="categoryId">
                <n-select
                    v-model:value="editForm.categoryName"
                    :options="categoryOptions"
                    placeholder="请选择文章分类"
                    filterable
                    clearable
                    tag
                    class="rounded-xl"
                    :bordered="true"
                    @update:value="handleCategoryChange"
                >
                </n-select>
                <template #feedback>
                  <p class="text-xs mt-1 text-gray-500 dark:text-gray-400">
                    分类用于左侧菜单分组显示，留空则显示在"其他"分类下
                  </p>
                </template>
              </n-form-item>

              <n-form-item label="访问权限" path="identity">
                <n-select
                    v-model:value="editForm.identity"
                    :options="identityOptions"
                    placeholder="请选择可查看权限"
                    class="rounded-xl"
                    :bordered="true"
                />
              </n-form-item>

              <n-form-item label="文章内容" path="content">
                <n-input
                    v-model:value="editForm.content"
                    type="textarea"
                    :autosize="{ minRows: 20 }"
                    placeholder="请输入文章内容（支持Markdown语法）"
                    class="rounded-xl font-mono text-sm"
                    :bordered="true"
                />
              </n-form-item>
            </n-form>
          </div>
        </div>

        <!-- 预览区域 -->
        <div class="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden transition-all duration-300 hover:shadow-lg hidden md:block">
          <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <h2 class="text-lg font-semibold tracking-tight">预览</h2>
          </div>
          <div class="p-6 markdown-preview">
            <MarkdownComponent :content="previewContent" :show-nav="false"/>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue'
import {useRoute, useRouter} from 'vue-router'
import {useMessage, NButton, NForm, NFormItem, NInput, NSelect, NInputNumber} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {ArticleService} from '../services/ArticleService'
import type {ArticleModel, ArticleCreateDto, ArticleUpdateDto} from '../models'
import MarkdownComponent from '../components/MarkdownComponent.vue'

interface EditFormType {
  path: string
  title: string
  content: string
  identity: 'Member' | 'Department' | 'Minister' | 'President' | 'Founder'
  categoryName?: string
  lastWriteTime: string
}

const route = useRoute()
const router = useRouter()
const message = useMessage()

const saving = ref(false)
const editingArticle = ref<ArticleModel | null>(null)
const formRef = ref<InstanceType<typeof NForm> | null>(null)
const categoryOptions = ref<Array<{ label: string, value: string }>>([])

const editForm = ref<EditFormType>({
  path: '',
  title: '',
  content: '',
  identity: 'Member',
  categoryName: '',
  lastWriteTime: new Date().toISOString(),
})

const identityOptions = [
  {label: '所有人', value: 'Member'},
  {label: '部员', value: 'Department'},
  {label: '部长', value: 'Minister'},
  {label: '社长', value: 'President'},
  {label: '创始人', value: 'Founder'}
]

const previewContent = computed(() => {
  return {
    title: editForm.value.title || '无标题文章',
    date: editForm.value.lastWriteTime,
    watch: 0,
    content: editForm.value.content || '在这里输入文章内容...'
  }
})

// 获取所有分类选项
const fetchCategoryOptions = async () => {
  try {
    // 获取所有分类列表
    const categories = await ArticleService.getAllCategories()

    console.log('获取分类列表成功:', categories)

    // 转换为选项格式
    categoryOptions.value = categories.map(category => ({
      label: category.name,
      value: category.name
    }))

    // 如果当前编辑的文章有分类，自动选择对应的分类
    if (editForm.value.categoryName) {
      // 分类选项已经加载，无需额外处理
    }
  } catch (error) {
    console.error('获取分类选项失败:', error)
    // 降级方案：从分类文章中获取分类信息
    try {
      const categoryArticles = await ArticleService.getAllCategoryArticles()
      const categories = Object.keys(categoryArticles)

      // 转换为选项格式
      categoryOptions.value = categories.map(category => ({
        label: category,
        value: category // 临时使用分类名称作为value，后续需要更新为categoryId
      }))
    } catch (fallbackError) {
      console.error('获取分类选项失败（降级方案）:', fallbackError)
    }
  }
}

// 处理分类变化
const handleCategoryChange = (categoryName: string) => {
  // 当分类变化时，更新categoryId
  editForm.value.categoryName = categoryName
}

const rules = {
  path: {
    required: true,
    message: '请输入文章路径',
    trigger: 'blur'
  },
  title: {
    required: true,
    message: '请输入文章标题',
    trigger: 'blur'
  },
  content: {
    required: true,
    message: '请输入文章内容',
    trigger: 'blur'
  }
}

// 返回文章管理页面
const goBack = () => {
  router.push('/Centre/Article')
}

// 获取文章详情
const fetchArticle = async (path: string) => {
  try {
    const article = await ArticleService.getArticleByPath(path)
    editingArticle.value = article
    editForm.value = {
      path: article.path,
      title: article.title || '',
      content: article.content || '',
      identity: (article.identity as EditFormType['identity']) || 'Member',
      categoryName: article.category?.name || '',
      lastWriteTime: article.lastWriteTime || new Date().toISOString(),
    }
  } catch (error) {
    console.error('获取文章失败:', error)
    message.error('获取文章失败: ' + (error instanceof Error ? error.message : String(error)))
  }
}

// 保存文章
const saveArticle = async () => {
  if (!formRef.value) return

  try {
    await formRef.value.validate()
  } catch (error) {
    message.error('请填写必填字段并修正错误')
    return
  }

  try {
    saving.value = true
    if (editingArticle.value) {
      // 更新文章
      const updateDto: ArticleUpdateDto = {
        title: editForm.value.title,
        content: editForm.value.content,
        identity: editForm.value.identity,
        category: editForm.value.categoryName,
      }
      await ArticleService.updateArticle(editForm.value.path, updateDto)
      message.success('文章更新成功')
    } else {
      // 创建新文章
      const createDto: ArticleCreateDto = {
        path: editForm.value.path,
        title: editForm.value.title,
        content: editForm.value.content,
        identity: editForm.value.identity,
        category: editForm.value.categoryName,
      }
      await ArticleService.createArticle(createDto)
      message.success('文章创建成功')
    }
    goBack()
  } catch (error) {
    console.error('保存文章失败:', error)
    message.error('保存文章失败: ' + (error instanceof Error ? error.message : String(error)))
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  // 先获取分类选项
  await fetchCategoryOptions()

  const articlePath = route.params.path as string
  if (articlePath) {
    await fetchArticle(articlePath)
  }
})
</script>

<style scoped>
:deep(.markdown-content img) {
  max-width: 100%;
}
</style>
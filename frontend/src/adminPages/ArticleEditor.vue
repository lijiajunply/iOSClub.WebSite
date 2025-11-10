<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <!-- 顶部导航栏 -->
    <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <div class="flex items-center">
            <n-button text @click="goBack" class="mr-4">
              <Icon icon="material-symbols:arrow-back" class="w-6 h-6 text-gray-600 dark:text-gray-300"/>
            </n-button>
            <h1 class="ml-2 text-lg font-semibold text-gray-900 dark:text-white">
              {{ editingArticle ? '编辑文章' : '新建文章' }}
            </h1>
          </div>
          <div class="flex items-center space-x-3">
            <n-button @click="saveArticle" type="primary" :loading="saving" class="rounded-full">
              保存文章
            </n-button>
          </div>
        </div>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- 编辑区域 -->
        <div class="bg-white dark:bg-gray-800 rounded-xl shadow-sm">
          <div class="p-4 border-b border-gray-200 dark:border-gray-700">
            <h2 class="text-base font-medium text-gray-900 dark:text-white">编辑器</h2>
          </div>
          <div class="p-4">
            <n-form :model="editForm" ref="formRef" :rules="rules">
              <div class="space-y-4">
                <n-form-item label="文章路径 (Path)" path="path">
                  <n-input
                      v-model:value="editForm.path"
                      :disabled="!!editingArticle"
                      placeholder="请输入文章路径，如：About"
                      class="rounded-lg"
                      :bordered="false"
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
                      class="rounded-lg"
                      :bordered="true"
                  />
                </n-form-item>

                <n-form-item label="访问权限" path="identity">
                  <n-select
                      v-model:value="editForm.identity"
                      :options="identityOptions"
                      placeholder="请选择可查看权限"
                      class="rounded-lg"
                      :bordered="true"
                  />
                </n-form-item>

                <n-form-item label="文章内容" path="content">
                  <n-input
                      v-model:value="editForm.content"
                      type="textarea"
                      :autosize="{ minRows: 20 }"
                      placeholder="请输入文章内容（支持Markdown语法）"
                      class="rounded-lg font-mono text-sm"
                      :bordered="true"
                  />
                </n-form-item>
              </div>
            </n-form>
          </div>
        </div>

        <!-- 预览区域 -->
        <div class="bg-white dark:bg-gray-800 rounded-xl shadow-sm">
          <div class="p-4 border-b border-gray-200 dark:border-gray-700">
            <h2 class="text-base font-medium text-gray-900 dark:text-white">预览</h2>
          </div>
          <div class="p-4 markdown-preview">
            <MarkdownComponent :content="previewContent" :show-nav="false"/>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue'
import {useRoute, useRouter} from 'vue-router'
import {useMessage, NButton, NForm, NFormItem, NInput, NSelect} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {ArticleService} from '../services/ArticleService'
import type {ArticleModel, ArticleCreateDto, ArticleUpdateDto} from '../models'
import MarkdownComponent from '../components/MarkdownComponent.vue'

interface EditFormType {
  path: string
  title: string
  content: string
  identity: 'Member' | 'Department' | 'Minister' | 'President' | 'Founder'
  lastWriteTime: string
}

const route = useRoute()
const router = useRouter()
const message = useMessage()

const saving = ref(false)
const editingArticle = ref<ArticleModel | null>(null)
const formRef = ref<InstanceType<typeof NForm> | null>(null)

const editForm = ref<EditFormType>({
  path: '',
  title: '',
  content: '',
  identity: 'Member',
  lastWriteTime: new Date().toISOString()
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
      lastWriteTime: article.lastWriteTime || new Date().toISOString()
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
        identity: editForm.value.identity
      }
      await ArticleService.updateArticle(editForm.value.path, updateDto)
      message.success('文章更新成功')
    } else {
      // 创建新文章
      const createDto: ArticleCreateDto = {
        path: editForm.value.path,
        title: editForm.value.title,
        content: editForm.value.content
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

onMounted(() => {
  const articlePath = route.params.path as string
  if (articlePath) {
    fetchArticle(articlePath)
  }
})
</script>

<style scoped>
:deep(.markdown-content img) {
  max-width: 100%;
}
</style>
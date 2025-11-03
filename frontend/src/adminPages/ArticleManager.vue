<template>
  <div class="min-h-screen">
    <!-- 顶部标题区域 -->
    <div class="pt-6 pb-4">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="mb-6">
          <h1 class="text-2xl md:text-3xl font-semibold text-gray-900 dark:text-white">
            社团文章管理
          </h1>
          <p class="text-gray-600 dark:text-gray-400 text-sm mt-1">
            创建、编辑和删除社团文章
          </p>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-20">
      <!-- 文章列表网格 -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        <!-- 骨架加载 -->
        <template v-if="loading">
          <div v-for="i in 8" :key="i" class="rounded-2xl bg-white/95 dark:bg-gray-800/95 backdrop-blur-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            <div class="p-5 h-full flex flex-col">
              <div class="flex items-center mb-3">
                <div class="bg-gray-200 dark:bg-gray-700 p-2.5 rounded-xl mr-3 w-10 h-10"></div>
                <div class="bg-gray-200 dark:bg-gray-700 rounded-full h-5 w-16"></div>
              </div>
              
              <div class="flex-1">
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
                <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/3 mt-3"></div>
              </div>

              <div class="flex justify-end space-x-2 pt-3">
                <div class="bg-gray-200 dark:bg-gray-700 rounded-full w-8 h-8"></div>
                <div class="bg-gray-200 dark:bg-gray-700 rounded-full w-8 h-8"></div>
              </div>
            </div>
          </div>
        </template>
        
        <!-- 文章卡片 -->
        <template v-else>
          <div
              v-for="article in articles"
              :key="article.path"
              class="group rounded-2xl bg-white/95 dark:bg-gray-800/95 backdrop-blur-xl border border-gray-200 dark:border-gray-700 shadow-sm hover:shadow-md hover:scale-[1.02] transition-all duration-300 overflow-hidden cursor-pointer"
              @click="editArticle(article)"
          >
            <div class="p-5 h-full flex flex-col">
              <div class="flex items-center mb-3">
                <div class="bg-indigo-100 dark:bg-indigo-900/30 p-2.5 rounded-xl mr-3">
                  <Icon icon="material-symbols:article" class="text-indigo-600 dark:text-indigo-400 w-5 h-5" />
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
                <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  <span class="flex items-center">
                    <Icon icon="material-symbols:schedule" class="mr-1 w-3 h-3" />
                    {{ formatDate(article.lastWriteTime) }}
                  </span>
                </p>
              </div>

              <!-- 操作按钮 -->
              <div class="flex justify-end space-x-2 pt-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
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
              class="col-span-full py-16 flex flex-col items-center justify-center rounded-2xl bg-white/70 dark:bg-gray-800/70 backdrop-blur-sm border border-dashed border-gray-300 dark:border-gray-600"
          >
            <div class="w-16 h-16 flex items-center justify-center bg-gray-100 dark:bg-gray-700 rounded-full mb-4">
              <Icon icon="material-symbols:article-off" class="text-gray-400 dark:text-gray-500 w-7 h-7" />
            </div>
            <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-1">暂无文章</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">创建您的第一篇文章开始管理</p>
            <n-button
                type="primary"
                class="rounded-full px-6 py-2"
                @click="openCreateModal">
              <template #icon>
                <Icon icon="material-symbols:add-circle" class="w-4.5 h-4.5" />
              </template>
              创建第一篇文章
            </n-button>
          </div>
        </template>
      </div>
    </div>

    <!-- 添加文章按钮 -->
    <div class="fixed bottom-6 right-6 z-10">
      <n-button
          type="primary"
          class="w-14 h-14 rounded-full shadow-lg backdrop-blur-xl flex items-center justify-center hover:scale-105 transition-transform duration-200"
          @click="openCreateModal"
      >
        <Icon icon="material-symbols:add" class="w-6 h-6"/>
      </n-button>
    </div>

    <!-- 编辑/创建文章对话框 -->
    <n-modal
        v-model:show="showEditModal"
        preset="card"
        style="width: 800px; max-width: 95vw;"
        :title="editingArticle ? '编辑文章' : '新建文章'"
        :bordered="false"
        :segmented="false"
        @close="handleModalClose"
        class="rounded-xl overflow-hidden bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700"
      >
      <n-form 
        :model="editForm" 
        ref="formRef" 
        :rules="rules"
        class="mt-4"
      >
        <div class="space-y-4">
          <n-form-item label="文章路径 (Path)" path="path">
            <n-input
                v-model:value="editForm.path"
                :disabled="!!editingArticle"
                placeholder="请输入文章路径，如：About"
                class="rounded-lg"
                :bordered="false"
            />
            <p class="text-xs mt-1 text-gray-500 dark:text-gray-400">
              路径将作为文章的唯一标识，创建后不可修改
            </p>
          </n-form-item>

          <n-form-item label="文章标题" path="title">
            <n-input
                v-model:value="editForm.title"
                placeholder="请输入文章标题"
                class="rounded-lg"
                :bordered="false"
            />
          </n-form-item>

          <n-form-item label="访问权限" path="identity">
            <n-select
                v-model:value="editForm.identity"
                :options="identityOptions"
                placeholder="请选择可查看权限"
                class="rounded-lg"
                :bordered="false"
            />
          </n-form-item>

          <n-form-item label="文章内容" path="content">
            <n-input
                v-model:value="editForm.content"
                type="textarea"
                :autosize="{ minRows: 12, maxRows: 20 }"
                placeholder="请输入文章内容（支持Markdown语法）"
                class="rounded-lg"
                :bordered="false"
            />
          </n-form-item>
        </div>
      </n-form>

      <template #footer>
        <div class="flex justify-end space-x-4 pt-4">
          <n-button
              @click="showEditModal = false"
              class="rounded-full px-6"
              :ghost="true"
          >
            取消
          </n-button>
          <n-button
              type="primary"
              @click="saveArticle"
              :loading="saving"
              class="rounded-full px-6"
          >
            保存
          </n-button>
        </div>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useDialog, useMessage, NButton, NCard, NForm, NFormItem, NInput, NModal, NSpin, NTag, NSelect } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { ArticleService } from '../services/ArticleService'
import type { ArticleModel, ArticleCreateDto, ArticleUpdateDto } from '../models'

interface EditFormType {
  path: string
  title: string
  content: string
  identity: 'Member' | 'Department' | 'Minister' | 'President' | 'Founder'
  lastWriteTime: string
}

const dialog = useDialog()
const message = useMessage()

const articles = ref<ArticleModel[]>([])
const loading = ref(false)
const saving = ref(false)
const showEditModal = ref(false)
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

// 根据权限值获取显示标签
const getIdentityLabel = (identity: string) => {
  const option = identityOptions.find(item => item.value === identity)
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

// 打开创建文章模态框
const openCreateModal = () => {
  editingArticle.value = null
  // 重置表单
  editForm.value = {
    path: '',
    title: '',
    content: '',
    identity: 'Member',
    lastWriteTime: new Date().toISOString()
  }
  // 重置表单验证状态
  formRef.value?.resetValidation()
  showEditModal.value = true
}

// 编辑文章
const editArticle = (article: ArticleModel) => {
  editingArticle.value = article
  editForm.value = {
    path: article.path,
    title: article.title || '',
    content: article.content || '',
    identity: (article.identity as EditFormType['identity']) || 'Member',
    lastWriteTime: article.lastWriteTime || new Date().toISOString()
  }
  showEditModal.value = true
}

// 处理模态框关闭
const handleModalClose = () => {
  formRef.value?.resetValidation()
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

// 保存文章
const saveArticle = async () => {
  if (!formRef.value) return
  
  const valid = await formRef.value.validate()
  if (!valid) {
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
    showEditModal.value = false
    // 重新获取文章列表
    await fetchArticles()
  } catch (error) {
    console.error('保存文章失败:', error)
    message.error('保存文章失败: ' + (error instanceof Error ? error.message : String(error)))
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  fetchArticles()
})
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* 自定义滚动条样式 */
:deep(.n-input__textarea-el) {
  scrollbar-width: thin;
  scrollbar-color: rgba(107, 114, 128, 0.5) transparent;
}

:deep(.n-input__textarea-el)::-webkit-scrollbar {
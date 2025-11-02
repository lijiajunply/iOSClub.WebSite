<template>
  <div class="min-h-screen">
    <!-- 顶部标题区域 -->
    <div class="pt-6 pb-4">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="mb-6">
          <h1 class="text-2xl md:text-3xl font-semibold text-gray-900 dark:text-white">
            社团文章
          </h1>
          <p class="text-gray-600 dark:text-gray-400 text-sm mt-1">
            管理社团的所有文章
          </p>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-20">
      <!-- 文章列表网格 -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
        <div
            v-for="article in articles"
            :key="article.path"
            class="group rounded-2xl bg-white/80 dark:bg-gray-800/80 backdrop-blur-2xl border border-gray-200/80 dark:border-gray-700/80 shadow-sm hover:shadow-md transition-all duration-300 overflow-hidden cursor-pointer"
            @click="editArticle(article)"
        >
          <div class="p-5 h-full flex flex-col">
            <div class="flex-1">
              <h3 class="font-medium text-base md:text-lg mb-2 line-clamp-2 text-gray-900 group-hover:text-blue-600 dark:text-white dark:group-hover:text-blue-400 transition-colors">
                {{ article.title || '' }}
              </h3>
              <p class="text-xs text-gray-500 dark:text-gray-400 mb-3">
                更新时间: {{ formatDate(article.lastWriteTime) }}
              </p>
              <n-tag
                  size="small"
                  :type="getIdentityType(article.identity)"
                  :bordered="false">
                {{ getIdentityLabel(article.identity || '') }}
              </n-tag>
            </div>

            <!-- 操作按钮 -->
            <div class="flex justify-end space-x-2 pt-3">
              <n-button
                  type="primary"
                  size="small"
                  quaternary
                  circle
                  @click.stop="editArticle(article)"
              >
                <template #icon>
                  <Icon icon="antd:edit-outlined" size="18"/>
                </template>
              </n-button>
              <n-button
                  type="error"
                  size="small"
                  quaternary
                  circle
                  @click.stop="deleteArticle(article)"
              >
                <template #icon>
                  <Icon icon="antd:delete-outlined" size="18"/>
                </template>
              </n-button>
            </div>
          </div>
        </div>

        <!-- 空状态 -->
        <div
            v-if="articles.length === 0 && !loading"
            class="col-span-full py-16 flex flex-col items-center justify-center rounded-2xl bg-white/50 dark:bg-gray-800/50 border border-dashed border-gray-300 dark:border-gray-600"
        >
          <n-empty description="暂无文章" size="large">
            <template #icon>
              <Icon icon="antd:file-text-outlined"/>
            </template>
          </n-empty>
          <n-button
              type="primary"
              class="mt-4 rounded-full px-5 py-2 text-sm"
              @click="openCreateModal">
            创建第一篇文章
          </n-button>
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <n-spin size="large"/>
      </div>
    </div>

    <!-- 添加文章按钮 -->
    <div class="fixed bottom-6 right-6 z-10">
      <n-button
          type="primary"
          class="w-14 h-14 rounded-full shadow-lg backdrop-blur-xl flex items-center justify-center"
          @click="openCreateModal"
      >
        <Icon icon="antd:plus-outlined" size="24"/>
      </n-button>
    </div>

    <!-- 编辑/创建文章对话框 -->
    <n-modal
        v-model:show="showEditModal"
        preset="card"
        style="width: 800px; max-width: 95vw; border-radius: 24px;"
        :title="editingArticle ? '编辑文章' : '新建文章'"
        :bordered="false"
        segmented
        @close="handleModalClose"
    >
      <n-form :model="editForm" ref="formRef" :rules="rules">
        <n-form-item label="文章路径 (Path)" path="path">
          <n-input
              v-model:value="editForm.path"
              :disabled="!!editingArticle"
              placeholder="请输入文章路径，如：About"
              round
          />
          <p class="text-xs mt-1 text-gray-500 dark:text-gray-400">
            路径将作为文章的唯一标识，创建后不可修改
          </p>
        </n-form-item>

        <n-form-item label="文章标题" path="title">
          <n-input
              v-model:value="editForm.title"
              placeholder="请输入文章标题"
              round
          />
        </n-form-item>

        <n-form-item label="访问权限" path="identity">
          <n-select
              v-model:value="editForm.identity"
              :options="identityOptions"
              placeholder="请选择可查看权限"
              round
          />
        </n-form-item>

        <n-form-item label="文章内容" path="content">
          <n-input
              v-model:value="editForm.content"
              type="textarea"
              :autosize="{ minRows: 12, maxRows: 20 }"
              placeholder="请输入文章内容（支持Markdown语法）"
              round
          />
        </n-form-item>
      </n-form>

      <template #footer>
        <div class="flex justify-end space-x-3">
          <n-button
              @click="showEditModal = false"
              quaternary
              round
              class="px-5"
          >
            取消
          </n-button>
          <n-button
              type="primary"
              @click="saveArticle"
              :loading="saving"
              round
              class="px-5"
          >
            保存
          </n-button>
        </div>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import {ref, onMounted} from 'vue'
import {
  NButton,
  NEmpty,
  NSpin,
  NModal,
  NForm,
  NFormItem,
  NInput,
  NSelect,
  useMessage,
  useDialog,
  NTag
} from 'naive-ui'
import {ArticleService} from "../services/ArticleService"
import {type ArticleModel, type ArticleCreateDto, type ArticleUpdateDto} from '../models'
import {Icon} from '@iconify/vue'

const message = useMessage()
const dialog = useDialog()

const articles = ref<ArticleModel[]>([])
const loading = ref(true)
const showEditModal = ref(false)
const editingArticle = ref<ArticleModel | null>(null)
const saving = ref(false)
const formRef = ref()

// 为编辑表单定义明确的类型
interface EditFormType {
  path: string
  title: string
  content: string
  identity: 'Member' | 'Department' | 'Minister' | 'President' | 'Founder'
  lastWriteTime: string
}

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
    title: article.title,
    content: article.content,
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
    content: `确定要删除文章"${article.title}"吗？此操作不可恢复。`,
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
  const valid = await formRef.value?.validate()
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
</style>
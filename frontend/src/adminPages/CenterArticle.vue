<template>
  <div class="min-h-screen bg-gray-50 py-8">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">社团文章</h1>
        <p class="text-gray-600">管理社团的所有文章</p>
      </div>

      <!-- 文章列表 -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
        <n-card
            v-for="article in articles"
            :key="article.path"
            hoverable
            class="group cursor-pointer rounded-xl transition-all duration-300 hover:shadow-lg"
            @click="editArticle(article)"
        >
          <div class="flex flex-col h-full">
            <div class="flex-1">
              <h3 class="text-lg font-semibold text-gray-900 mb-2 group-hover:text-purple-600 transition-colors">
                {{ article.title }}
              </h3>
              <p class="text-sm text-gray-500 mb-4">
                更新时间: {{ formatDate(article.lastWriteTime) }}
              </p>
            </div>

            <!-- 管理员操作按钮 -->
            <div class="flex justify-end space-x-2 mt-4">
              <n-button
                  type="primary"
                  size="small"
                  @click.stop="editArticle(article)"
              >
                编辑
              </n-button>
              <n-button
                  type="error"
                  size="small"
                  @click.stop="deleteArticle(article)"
              >
                删除
              </n-button>
            </div>
          </div>
        </n-card>

        <!-- 空状态 -->
        <n-empty
            v-if="articles.length === 0 && !loading"
            class="col-span-full py-12"
            description="暂无文章"
        />
      </div>

      <!-- 添加文章按钮 -->
      <n-button
          type="primary"
          class="fixed bottom-8 right-8 rounded-full w-14 h-14 text-xl shadow-lg"
          @click="openCreateModal"
      >
        +
      </n-button>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <n-spin size="large" />
      </div>
    </div>

    <!-- 编辑/创建文章对话框 -->
    <n-modal
        v-model:show="showEditModal"
        preset="card"
        style="width: 800px; max-width: 95vw;"
        :title="editingArticle ? '编辑文章' : '新建文章'"
        @close="handleModalClose"
    >
      <n-form :model="editForm" ref="formRef" :rules="rules">
        <n-form-item label="文章路径 (Path)" path="path">
          <n-input
              v-model:value="editForm.path"
              :disabled="!!editingArticle"
              placeholder="请输入文章路径，如：About"
          />
          <p class="text-xs text-gray-500 mt-1">路径将作为文章的唯一标识，创建后不可修改</p>
        </n-form-item>

        <n-form-item label="文章标题" path="title">
          <n-input
              v-model:value="editForm.title"
              placeholder="请输入文章标题"
          />
        </n-form-item>

        <n-form-item label="访问权限" path="identity">
          <n-select
              v-model:value="editForm.identity"
              :options="identityOptions"
              placeholder="请选择可查看权限"
          />
        </n-form-item>

        <n-form-item label="文章内容" path="content">
          <n-input
              v-model:value="editForm.content"
              type="textarea"
              :autosize="{ minRows: 15 }"
              placeholder="请输入文章内容（支持Markdown语法）"
          />
        </n-form-item>
      </n-form>
      <template #footer>
        <n-space justify="end">
          <n-button @click="showEditModal = false">取消</n-button>
          <n-button type="primary" @click="saveArticle" :loading="saving">保存</n-button>
        </n-space>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  NCard,
  NButton,
  NEmpty,
  NSpin,
  NModal,
  NForm,
  NFormItem,
  NInput,
  NSelect,
  NSpace,
  useMessage,
  useDialog
} from 'naive-ui'
import { type ArticleModel, type ArticleProps, ArticleService } from "../services/ArticleService.ts"

const message = useMessage()
const dialog = useDialog()

const articles = ref<ArticleProps[]>([])
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
  { label: '所有人', value: 'Member' },
  { label: '部员', value: 'Department' },
  { label: '部长', value: 'Minister' },
  { label: '社长', value: 'President' },
  { label: '创始人', value: 'Founder' }
]

// 根据权限值获取显示标签
const getIdentityLabel = (identity: string) => {
  const option = identityOptions.find(item => item.value === identity)
  return option ? option.label : '未知'
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
      month: 'long',
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
    const data = await ArticleService.getArticles()
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
      // 更新文章 - 更新时间戳
      const updatedForm = {
        ...editForm.value,
        lastWriteTime: new Date().toISOString()
      }
      await ArticleService.updateArticle(editForm.value.path, updatedForm)
      message.success('文章更新成功')
    } else {
      // 创建新文章
      const newArticle = {
        ...editForm.value,
        lastWriteTime: new Date().toISOString()
      }
      await ArticleService.createArticle(newArticle)
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
.fixed-button {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  z-index: 10;
}
</style>
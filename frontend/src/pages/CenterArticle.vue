<template>
  <div class="article-page">
    <n-card hoverable class="article-card">
      <div class="article-header">
        <h1 class="article-title">{{ roomArticle.title }}</h1>
        <div class="article-meta">
          <n-space align="center" size="large">
            <n-icon size="16">
              <CalendarOutline />
            </n-icon>
            <span class="article-date">{{ formatDate(roomArticle.date) }}</span>
            <n-icon size="16">
              <EyeOutline />
            </n-icon>
            <span class="article-watch">浏览: {{ roomArticle.watch }}</span>
          </n-space>
        </div>

        <!-- 管理员操作按钮 -->
        <div class="article-actions">
          <n-space>
            <n-button type="primary" @click="editArticle">
              <template #icon>
                <n-icon>
                  <CreateOutline />
                </n-icon>
              </template>
              编辑
            </n-button>
            <n-button type="error" @click="deleteArticle">
              <template #icon>
                <n-icon>
                  <TrashOutline />
                </n-icon>
              </template>
              删除
            </n-button>
          </n-space>
        </div>
      </div>

      <n-divider />

      <div class="article-content">
        <MarkdownComponent :content="roomArticle.content"/>
      </div>
    </n-card>

    <!-- 编辑文章对话框 -->
    <n-modal v-model:show="showEditModal" preset="card" style="width: 800px; max-width: 95vw;" title="编辑文章">
      <n-form :model="editForm" ref="formRef">
        <n-form-item label="标题" path="title">
          <n-input v-model:value="editForm.title" placeholder="请输入文章标题" />
        </n-form-item>
        <n-form-item label="内容" path="content">
          <n-input v-model:value="editForm.content" type="textarea" :autosize="{ minRows: 10 }" placeholder="请输入文章内容（支持Markdown语法）" />
        </n-form-item>
        <n-form-item label="权限" path="identity">
          <n-select v-model:value="editForm.identity" :options="identityOptions" placeholder="请选择可查看权限" />
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
import { ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  NCard,
  NSpace,
  NIcon,
  NDivider,
  NButton,
  NModal,
  NForm,
  NFormItem,
  NInput,
  NSelect,
  useMessage
} from 'naive-ui'
import {
  CalendarOutline,
  EyeOutline,
  CreateOutline,
  TrashOutline
} from '@vicons/ionicons5'
import MarkdownComponent from "../components/MarkdownComponent.vue"
import { type ArticleModel, type ArticleProps, ArticleService } from "../services/ArticleService.ts"
import { useAuthorizationStore } from '../stores/Authorization'

const roomArticle = ref<ArticleProps>({
  title: '',
  date: '',
  watch: 0,
  content: '',
})

const route = useRoute()
const router = useRouter()
const message = useMessage()
const authorizationStore = useAuthorizationStore()

// 用户信息
const userInfo = ref({
  isAdmin: false
})

// 编辑表单相关
const showEditModal = ref(false)
const saving = ref(false)
const formRef = ref()
const editForm = ref({
  title: '',
  content: '',
  identity: 'Member'
})

const identityOptions = [
  { label: '所有人', value: 'Member' },
  { label: '部员', value: 'Department' },
  { label: '部长', value: 'Minister' },
  { label: '社长', value: 'President' },
  { label: '创始人', value: 'Founder' }
]

// 获取用户信息
const fetchUserInfo = async () => {
  try {
    const token = localStorage.getItem('Authorization');
    if (!token) {
      userInfo.value.isAdmin = false;
      return;
    }

    const response = await fetch('https://www.xauat.site/api/Member/GetData', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const userData = await response.json();
      userInfo.value.isAdmin = ['Founder', 'President', 'Minister'].includes(userData.identity);
    } else {
      userInfo.value.isAdmin = false;
    }
  } catch (error) {
    console.error('获取用户信息失败:', error);
    userInfo.value.isAdmin = false;
  }
}

watch(
    () => route.params.id,
    async (newId) => {
      if (typeof newId !== 'string') return
      try {
        const a = await ArticleService.getArticle(newId)
        roomArticle.value = {
          title: a.title,
          date: a.lastWriteTime,
          watch: a.watch,
          content: a.content
        } as ArticleProps

        // 同时获取用户信息
        await fetchUserInfo()
      } catch (error) {
        console.error('获取文章失败:', error)
        message.error('获取文章失败')
      }
    },
    { immediate: true }
)

const formatDate = (dateString: string) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// 编辑文章
const editArticle = () => {
  const articleId = route.params.id as string
  editForm.value = {
    title: roomArticle.value.title,
    content: roomArticle.value.content,
    identity: 'Member' // 默认值，实际应该从后端获取
  }
  showEditModal.value = true
}

// 保存文章
const saveArticle = async () => {
  try {
    saving.value = true
    const articleId = route.params.id as string
    const updatedArticle = {
      ...editForm.value,
      path: articleId,
      lastWriteTime: new Date().toISOString()
    }

    await ArticleService.updateArticle(articleId, updatedArticle)
    message.success('文章更新成功')

    // 更新页面显示
    roomArticle.value.title = editForm.value.title
    roomArticle.value.content = editForm.value.content

    showEditModal.value = false
  } catch (error) {
    console.error('更新文章失败:', error)
    message.error('更新文章失败')
  } finally {
    saving.value = false
  }
}

// 删除文章
const deleteArticle = () => {
  const articleId = route.params.id as string
  window.$dialog.warning({
    title: '确认删除',
    content: '确定要删除这篇文章吗？此操作不可恢复。',
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        await ArticleService.deleteArticle(articleId)
        message.success('文章删除成功')
        // 跳转到文章列表页或其他页面
        router.push('/Blog')
      } catch (error) {
        console.error('删除文章失败:', error)
        message.error('删除文章失败')
      }
    }
  })
}
</script>

<style scoped>
.article-page {
  max-width: 900px;
  margin: 20px auto;
  padding: 0 20px;
}

.article-card {
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.article-header {
  padding: 20px 0;
}

.article-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 16px;
  color: #1c1f23;
  line-height: 1.3;
}

.article-meta {
  display: flex;
  align-items: center;
  font-size: 0.95rem;
  color: #666;
}

.article-actions {
  margin-top: 20px;
}

.article-content {
  line-height: 1.7;
  padding: 10px 0;
}

.article-content :deep(img) {
  max-width: 100%;
  height: auto;
  border-radius: 8px;
  margin: 16px 0;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.article-content :deep(h1) {
  font-size: 1.8rem;
  margin: 1.8rem 0 1.2rem;
  color: #1c1f23;
  font-weight: 600;
}

.article-content :deep(h2) {
  font-size: 1.5rem;
  margin: 1.6rem 0 1rem;
  color: #1c1f23;
  font-weight: 600;
}

.article-content :deep(h3) {
  font-size: 1.3rem;
  margin: 1.4rem 0 0.8rem;
  color: #1c1f23;
  font-weight: 600;
}

.article-content :deep(p) {
  margin-bottom: 1.2rem;
  font-size: 1rem;
  color: #333;
}

.article-content :deep(code) {
  background-color: #f6f7f9;
  padding: 2px 6px;
  border-radius: 4px;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, Courier, monospace;
  font-size: 0.9em;
  color: #d6336c;
}

.article-content :deep(pre) {
  background-color: #f6f7f9;
  padding: 16px;
  border-radius: 8px;
  overflow-x: auto;
  margin: 20px 0;
  box-shadow: inset 0 1px 2px rgba(0, 0, 0, 0.05);
}

.article-content :deep(pre code) {
  background: none;
  padding: 0;
  color: inherit;
  font-size: 0.9em;
}

.article-content :deep(blockquote) {
  border-left: 4px solid #dee2e6;
  padding: 8px 0 8px 20px;
  margin: 20px 0;
  background-color: #f8f9fa;
  border-radius: 0 4px 4px 0;
}

.article-content :deep(blockquote p) {
  margin-bottom: 0;
}

.article-content :deep(ul),
.article-content :deep(ol) {
  margin: 1.2rem 0;
  padding-left: 2rem;
}

.article-content :deep(li) {
  margin-bottom: 0.6rem;
}

.article-content :deep(table) {
  width: 100%;
  border-collapse: collapse;
  margin: 1.5rem 0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border-radius: 8px;
  overflow: hidden;
}

.article-content :deep(th),
.article-content :deep(td) {
  border: 1px solid #dee2e6;
  padding: 12px;
  text-align: left;
}

.article-content :deep(th) {
  background-color: #f1f3f5;
  font-weight: 600;
}

.article-content :deep(tr:nth-child(even)) {
  background-color: #f8f9fa;
}

@media (max-width: 768px) {
  .article-page {
    padding: 0 12px;
  }

  .article-title {
    font-size: 1.6rem;
  }

  .article-content :deep(h1) {
    font-size: 1.5rem;
  }

  .article-content :deep(h2) {
    font-size: 1.3rem;
  }

  .article-content :deep(h3) {
    font-size: 1.1rem;
  }
}
</style>
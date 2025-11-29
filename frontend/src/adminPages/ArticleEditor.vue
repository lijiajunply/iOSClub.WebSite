<template>
  <!-- 页面容器：Apple 风格浅灰背景，暗黑模式深色背景 -->
  <div class="min-h-screen bg-[#f5f5f7] dark:bg-black transition-colors duration-300 p-4 sm:p-6 lg:p-8 font-sans">

    <!-- 主要布局网格 -->
    <div class="max-w-[1600px] mx-auto grid grid-cols-1 xl:grid-cols-2 gap-6 lg:gap-8 items-start">

      <!-- 左侧：编辑区域 -->
      <div class="flex flex-col gap-6">

        <!-- 标题输入块 (突出显示) -->
        <div class="ios-card p-6">
          <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-2 ml-1">
            Title
          </label>
          <n-input
              v-model:value="editForm.title"
              placeholder="请输入文章标题..."
              class="text-2xl font-bold !bg-transparent"
              type="textarea"
              :autosize="{ minRows: 1, maxRows: 3 }"
              :bordered="false"
          >
            <template #suffix>
              <Icon icon="solar:pen-2-bold-duotone" class="text-gray-400 text-xl" />
            </template>
          </n-input>
        </div>

        <!-- 元数据设置组 (类似 iOS 设置列表风格) -->
        <div class="ios-group">
          <!-- 路径设置 -->
          <div class="ios-list-item">
            <div class="ios-label">
              <Icon icon="solar:link-circle-bold-duotone" class="text-blue-500 text-xl" />
              <span>路径 ID</span>
            </div>
            <div class="ios-input-wrapper">
              <n-input
                  v-model:value="editForm.path"
                  :disabled="!!editingArticle"
                  placeholder="Unique Path"
                  class="text-right !bg-transparent ios-input-reset"
                  :bordered="false"
              />
            </div>
          </div>

          <!-- 分类选择 -->
          <div class="ios-list-item">
            <div class="ios-label">
              <Icon icon="solar:folder-with-files-bold-duotone" class="text-orange-500 text-xl" />
              <span>所属分类</span>
            </div>
            <div class="ios-input-wrapper w-1/2">
              <n-select
                  v-model:value="editForm.categoryName"
                  :options="categoryOptions"
                  placeholder="选择分类"
                  filterable
                  clearable
                  tag
                  class="ios-select-reset"
                  :bordered="false"
                  @update:value="handleCategoryChange"
              />
            </div>
          </div>

          <!-- 权限选择 -->
          <div class="ios-list-item text-gray-400">
            <div class="ios-label">
              <Icon icon="solar:shield-user-bold-duotone" class="text-green-500 text-xl" />
              <span>访问权限</span>
            </div>
            <div class="ios-input-wrapper w-1/2">
              <n-select
                  v-model:value="editForm.identity"
                  :options="identityOptions"
                  placeholder="选择权限"
                  class="ios-select-reset"
                  :bordered="false"
              />
            </div>
          </div>

          <div class="px-6 py-2 bg-gray-50/50 dark:bg-white/5">
            <p class="text-xs text-gray-400 dark:text-gray-500">
              路径设定后无法更改；留空分类将归入“其他”。
            </p>
          </div>
        </div>

        <!-- 正文编辑区域 -->
        <div class="ios-card flex flex-col min-h-[500px] relative group">
          <!-- 顶部工具栏模拟 -->
          <div class="h-10 border-b border-gray-100 dark:border-gray-800 flex items-center px-4 gap-3">
            <div class="flex gap-1.5">
              <div class="w-3 h-3 rounded-full bg-red-400/80"></div>
              <div class="w-3 h-3 rounded-full bg-yellow-400/80"></div>
              <div class="w-3 h-3 rounded-full bg-green-400/80"></div>
            </div>
            <span class="text-xs text-gray-400 ml-2">Markdown Mode</span>
          </div>

          <n-form ref="formRef" :model="editForm" :rules="rules" class="flex-1">
            <n-form-item path="content" :show-label="false" class="h-full !mb-0" content-class="h-full" feedback-class="hidden">
              <n-input
                  v-model:value="editForm.content"
                  type="textarea"
                  placeholder="# 开始你的创作..."
                  class="!bg-transparent font-mono text-[15px] leading-relaxed p-4 !h-full"
                  :bordered="false"
                  :autosize="{
                     minRows: 3,
                  }"
              />
            </n-form-item>
          </n-form>

          <!-- 底部字符统计模拟 -->
          <div class="absolute bottom-4 right-4 text-xs text-gray-400 bg-white/80 dark:bg-black/60 backdrop-blur-md px-2 py-1 rounded-lg border border-gray-100 dark:border-gray-700 transition-opacity opacity-0 group-hover:opacity-100">
            {{ editForm.content?.length || 0 }} characters
          </div>
        </div>
      </div>

      <!-- 右侧：实时预览 (Sticky) -->
      <div class="hidden xl:flex flex-col gap-6 sticky top-24 h-[calc(100vh-8rem)]">
        <div class="flex items-center justify-between px-2">
          <h3 class="text-lg font-semibold text-gray-800 dark:text-gray-200 flex items-center gap-2">
            <Icon icon="solar:eye-bold-duotone" class="text-indigo-500" />
            实时预览
          </h3>
          <span class="text-xs px-2 py-1 rounded-md bg-gray-200 dark:bg-gray-800 text-gray-500 font-mono">Preview</span>
        </div>

        <!-- 模拟 iPad/Paper 效果 -->
        <div class="ios-card flex-1 overflow-hidden flex flex-col shadow-lg border-0 ring-4 ring-gray-200/50 dark:ring-gray-800/50">
          <div class="flex-1 overflow-y-auto custom-scrollbar p-8 bg-white dark:bg-[#1c1c1e]">
            <div class="max-w-none prose dark:prose-invert prose-slate mx-auto">
              <MarkdownComponent :content="previewContent" :show-nav="false"/>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, defineComponent, h } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMessage, NForm, NFormItem, NInput, NSelect } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { ArticleService } from '../services/ArticleService'
import type { ArticleModel, ArticleCreateDto, ArticleUpdateDto } from '../models'
import MarkdownComponent from '../components/MarkdownComponent.vue'
import { useLayoutStore } from '../stores/LayoutStore'

// --- Types ---
interface EditFormType {
  path: string
  title: string
  content: string
  identity: 'Member' | 'Department' | 'Minister' | 'President' | 'Founder'
  categoryName?: string
  lastWriteTime: string
}

// --- State ---
const route = useRoute()
const router = useRouter()
const message = useMessage()
const layoutStore = useLayoutStore()

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
  { label: '所有人 (Member)', value: 'Member' },
  { label: '部员仅见 (Department)', value: 'Department' },
  { label: '部长仅见 (Minister)', value: 'Minister' },
  { label: '社长仅见 (President)', value: 'President' },
  { label: '创始人 (Founder)', value: 'Founder' }
]

// --- Computed ---
const previewContent = computed(() => {
  return {
    title: editForm.value.title || 'Untitled',
    date: editForm.value.lastWriteTime,
    watch: 0,
    content: editForm.value.content || '> Start typing to preview...'
  }
})

const rules = {
  path: { required: true, message: '请输入文章路径', trigger: 'blur' },
  title: { required: true, message: '请输入文章标题', trigger: 'blur' },
  content: { required: true, message: '请输入文章内容', trigger: 'blur' }
}

// --- Methods ---
const fetchCategoryOptions = async () => {
  try {
    const categories = await ArticleService.getAllCategories()
    categoryOptions.value = categories.map(category => ({
      label: category.name,
      value: category.name
    }))
  } catch (error) {
    console.error('Primary category fetch failed, using fallback:', error)
    try {
      const categoryArticles = await ArticleService.getAllCategoryArticles()
      categoryOptions.value = Object.keys(categoryArticles).map(cat => ({
        label: cat,
        value: cat
      }))
    } catch (fallbackError) {
      console.error('Fallback category fetch failed:', fallbackError)
    }
  }
}

const handleCategoryChange = (categoryName: string) => {
  editForm.value.categoryName = categoryName
}

const goBack = () => {
  router.push('/Centre/Article')
}

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
    message.error('获取文章失败: ' + (error instanceof Error ? error.message : String(error)))
  }
}

const saveArticle = async () => {
  if (!formRef.value) return
  try {
    await formRef.value.validate()
  } catch (error) {
    message.error('请完善必填信息')
    return
  }

  try {
    saving.value = true
    const dto = {
      title: editForm.value.title,
      content: editForm.value.content,
      identity: editForm.value.identity,
      category: editForm.value.categoryName,
    }

    if (editingArticle.value) {
      await ArticleService.updateArticle(editForm.value.path, dto as ArticleUpdateDto)
      message.success('已更新')
    } else {
      await ArticleService.createArticle({ ...dto, path: editForm.value.path } as ArticleCreateDto)
      message.success('已发布')
    }
    goBack()
  } catch (error) {
    message.error('保存失败: ' + (error instanceof Error ? error.message : String(error)))
  } finally {
    saving.value = false
  }
}

// --- Lifecycle ---
onMounted(async () => {
  layoutStore.setShowPageActions(true)

  // Custom Actions Component (Apple Style Buttons)
  const ActionsComponent = defineComponent({
    setup() {
      return () => h('div', { class: 'flex items-center justify-end space-x-3' }, [
        // Cancel Button - Circle with Icon
        h('button', {
          class: 'group w-10 h-10 rounded-full bg-gray-200/60 dark:bg-gray-700/60 hover:bg-gray-300 dark:hover:bg-gray-600 flex items-center justify-center transition-all backdrop-blur-sm active:scale-95',
          onClick: goBack
        }, [
          h(Icon, {
            icon: 'solar:close-circle-bold',
            class: 'text-gray-500 dark:text-gray-300 w-6 h-6 group-hover:text-gray-700 dark:group-hover:text-white transition-colors'
          }),
        ]),
        // Save Button - Blue Pill
        h('button', {
          class: 'h-10 px-6 rounded-full bg-[#007AFF] hover:bg-[#0071E3] dark:bg-[#0A84FF] active:scale-95 shadow-sm hover:shadow flex items-center gap-2 transition-all disabled:opacity-50 disabled:cursor-not-allowed',
          onClick: saveArticle,
          disabled: saving.value
        }, [
          h('span', { class: 'text-white font-medium text-[15px]' }, saving.value ? 'Saving...' : 'Done'),
          !saving.value && h(Icon, { icon: 'solar:check-read-bold', class: 'text-white w-5 h-5' })
        ])
      ])
    }
  })

  layoutStore.setActionsComponent(ActionsComponent)

  await fetchCategoryOptions()
  const articlePath = route.params.path as string
  if (articlePath) {
    await fetchArticle(articlePath)
  }
  // Header Setup
  layoutStore.setPageHeader(
      editingArticle.value ? '编辑文章' : '新建文章',
      ''
  )
})

onBeforeUnmount(() => {
  layoutStore.clearPageHeader()
})
</script>

<style scoped>
/* iOS/Apple Design Language Utilities */

/* 卡片基座 */
.ios-card {
  background-color: #ffffff;
  border-radius: 1rem;
  border: 1px solid rgba(229, 231, 235, 0.6);
  overflow: hidden;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.ios-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
}

.dark .ios-card {
  background-color: #1C1C1E;
  border: 1px solid rgba(31, 41, 55, 0.6);
  box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.20);
}

/* 编辑器高度自适应 */
:deep(.n-form-item-blank) {
  height: 100%;
}

:deep(.n-form-item .n-form-item-blank) {
  height: 100%;
}

:deep(.n-form-item .n-form-item-blank .n-input) {
  height: 100%;
}

:deep(.n-form-item .n-form-item-blank .n-input .n-input__textarea-el) {
  height: 100% !important;
}

/* 分组容器 (类似 iOS Settings) */
.ios-group {
  background-color: #ffffff;
  border-radius: 1rem;
  border: 1px solid rgba(229, 231, 235, 0.6);
  overflow: hidden;
}

.dark .ios-group {
  background-color: #1C1C1E;
  border: 1px solid rgba(31, 41, 55, 0.6);
}

/* 列表项 */
.ios-list-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid rgb(243, 244, 246);
  height: 3.5rem;
  transition: colors 0.2s ease;
}

.ios-list-item:last-child {
  border-bottom: none;
}

.ios-list-item:hover {
  background-color: rgba(249, 250, 251, 1);
}

.dark .ios-list-item {
  border-bottom: 1px solid rgb(31, 41, 55);
}

.dark .ios-list-item:hover {
  background-color: rgba(255, 255, 255, 0.05);
}

/* 列表左侧标签 */
.ios-label {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: rgb(55, 65, 81);
}

.dark .ios-label {
  color: rgb(229, 231, 235);
}

/* 输入框 Wrapper (去边框) */
.ios-input-wrapper {
  flex: 1;
  display: flex;
  justify-content: flex-end;
}

/* Override NaiveUI Default Styles for cleaner look */
:deep(.ios-input-reset .n-input__input-el),
:deep(.ios-input-reset .n-input__textarea-el) {
  text-align: right;
  font-family: -apple-system, BlinkMacSystemFont, "SF Pro Text", "Helvetica Neue", sans-serif;
}

/* 自定义 Select 文字靠右 */
:deep(.ios-select-reset .n-base-selection-label) {
  text-align: right;
  justify-content: flex-end;
  background-color: transparent !important;
}

:deep(.ios-select-reset .n-base-selection) {
  --n-border: none !important;
  --n-box-shadow-active: none !important;
  --n-box-shadow-focus: none !important;
  background-color: transparent !important;
}

/* 滚动条美化 */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(156, 163, 175, 0.3);
  border-radius: 20px;
}

.dark .custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(255, 255, 255, 0.2);
}

/* Dark Mode Text Adjustments */
.dark .ios-list-item {
  border-color: rgb(31, 41, 55);
}
</style>
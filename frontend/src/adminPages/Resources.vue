<template>
  <div
      class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300 main-content"
      :class="{ 'with-sidebar': layoutStore.showSidebar && !layoutStore.isMobile }"
      :style="{ padding: isMobile ? '0.5rem' : '1rem' }"
  >
    <div class="p-6" :style="{ padding: isMobile ? '0.5rem' : '1rem' }">
      <n-page-header subtitle="社团资源管理" @back="goBack">
        <template #title>
          <div class="text-2xl font-bold">社团资源</div>
        </template>
        <template #extra>
          <n-button
              v-if="authorizationStore.isAdmin"
              type="primary"
              @click="showAddResourceModal"
              size="medium"
              class="transition-transform duration-200 hover:scale-105 active:scale-95"
          >
            添加资源
          </n-button>
        </template>
        <template #avatar>
          <n-avatar>
            <n-icon>
              <BookOutline />
            </n-icon>
          </n-avatar>
        </template>
      </n-page-header>

      <n-card class="mt-6 overflow-hidden">
        <n-input
            v-model:value="searchTerm"
            placeholder="搜索资源..."
            clearable
            class="mb-4"
        >
          <template #prefix>
            <n-icon><SearchOutline /></n-icon>
          </template>
        </n-input>

        <div v-if="filteredResources.length === 0" class="text-center py-8">
          <n-empty description="社团现在还没有任何资源">
            <template #icon>
              <n-icon>
                <BookOutline />
              </n-icon>
            </template>
          </n-empty>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <n-card
              v-for="resource in filteredResources"
              :key="resource.id"
              hoverable
              class="rounded-lg transition-transform duration-200 hover:scale-[1.02]"
          >
            <div class="flex flex-col h-full">
              <div class="mb-3">
                <h3 class="text-xl font-bold mb-2">{{ resource.name }}</h3>
                <p class="text-gray-600 dark:text-gray-300 mb-3">
                  {{ resource.description || '无描述' }}
                </p>
                <div class="mb-4">
                  <n-tag
                      v-for="tag in getResourceTags(resource)"
                      :key="tag"
                      type="info"
                      size="small"
                      class="mr-1 mb-1"
                  >
                    {{ tag }}
                  </n-tag>
                </div>
              </div>

              <div v-if="authorizationStore.isAdmin" class="mt-auto">
                <n-space justify="end">
                  <n-button
                      type="primary"
                      size="small"
                      @click="editResource(resource)"
                      class="transition-transform duration-200 hover:scale-105 active:scale-95"
                  >
                    编辑
                  </n-button>
                  <n-button
                      type="error"
                      size="small"
                      @click="deleteResource(resource)"
                      class="transition-transform duration-200 hover:scale-105 active:scale-95"
                  >
                    删除
                  </n-button>
                </n-space>
              </div>
            </div>
          </n-card>
        </div>
      </n-card>

      <n-modal
          v-model:show="showModal"
          preset="card"
          :style="mergedModalStyle"
          :title="editingResource.id ? '编辑资源' : '添加资源'"
          @after-leave="resetForm"
          :mask-closable="!isMobile"
      >
        <n-form ref="formRef" :model="editingResource" :rules="rules" label-placement="left" label-width="auto">
          <n-form-item label="资源名称" path="name">
            <n-input v-model:value="editingResource.name" placeholder="请输入资源名称" />
          </n-form-item>
          <n-form-item label="资源描述" path="description">
            <n-input v-model:value="editingResource.description" placeholder="请输入资源描述" type="textarea" />
          </n-form-item>
          <n-form-item label="资源标签" path="tag">
            <n-dynamic-tags v-model:value="resourceTags" />
          </n-form-item>
        </n-form>
        <template #footer>
          <n-space justify="end">
            <n-button @click="showModal = false">取消</n-button>
            <n-button type="primary" @click="saveResource" :loading="saving">保存</n-button>
          </n-space>
        </template>
      </n-modal>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMessage } from 'naive-ui'
import {
  NPageHeader,
  NCard,
  NInput,
  NModal,
  NForm,
  NFormItem,
  NButton,
  NAvatar,
  NIcon,
  NSpace,
  NDynamicTags,
  useDialog,
  NTag,
  NEmpty
} from 'naive-ui'
import { BookOutline, SearchOutline } from '@vicons/ionicons5'
import type { DataTableColumns } from 'naive-ui'
import { useAuthorizationStore } from '../stores/Authorization'
import { useLayoutStore } from '../stores/LayoutStore'

interface Resource {
  id: string
  name: string
  description: string | null
  tag: string | null
}

const router = useRouter()
const message = useMessage()
const dialog = useDialog()
const authorizationStore = useAuthorizationStore()
const layoutStore = useLayoutStore()

const showModal = ref(false)
const searchTerm = ref('')
const formRef = ref()
const saving = ref(false)
const isMobile = ref(window.innerWidth < 768)

const editingResource = ref<Resource>({
  id: '',
  name: '',
  description: '',
  tag: ''
})

const resourceTags = ref<string[]>([])

// 检测是否为移动设备
const checkIsMobile = () => {
  isMobile.value = window.innerWidth < 768
}

// 页面返回
const goBack = () => {
  router.back()
}

// 重置表单
const resetForm = () => {
  editingResource.value = {
    id: '',
    name: '',
    description: '',
    tag: ''
  }
  resourceTags.value = []
}

// 获取资源标签
const getResourceTags = (resource: Resource) => {
  if (!resource.tag) return []
  return resource.tag.split(',').map(t => t.trim()).filter(t => t.length > 0)
}

// 计算过滤后的资源列表
const filteredResources = computed(() => {
  if (!searchTerm.value) {
    return resources.value
  }
  const term = searchTerm.value.toLowerCase()
  return resources.value.filter(resource =>
      resource.name.toLowerCase().includes(term) ||
      (resource.description && resource.description.toLowerCase().includes(term)) ||
      (resource.tag && resource.tag.toLowerCase().includes(term))
  )
})

const resources = ref<Resource[]>([])

// 表单验证规则
const rules = {
  name: {
    required: true,
    message: '请输入资源名称',
    trigger: 'blur'
  }
}

// 合并模态框样式（修复重复:style错误的核心）
const mergedModalStyle = computed(() => {
  return isMobile.value
      ? {
        width: '100%',
        maxWidth: '100%',
        padding: '0.5rem'
      }
      : {
        width: '600px',
        maxWidth: '600px'
      }
})

// 显示添加资源模态框
const showAddResourceModal = () => {
  resetForm()
  showModal.value = true
}

// 编辑资源
const editResource = (resource: Resource) => {
  editingResource.value = { ...resource }
  if (resource.tag) {
    resourceTags.value = resource.tag.split(',').map(t => t.trim()).filter(t => t.length > 0)
  } else {
    resourceTags.value = []
  }
  showModal.value = true
}

// 删除资源
const deleteResource = (resource: Resource) => {
  dialog.warning({
    title: '确认删除',
    content: `确定要删除资源 "${resource.name}" 吗？`,
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        const token = localStorage.getItem('Authorization')
        const response = await fetch(`/api/Project/DeleteResource?id=${resource.id}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        })

        if (response.ok) {
          message.success('删除成功')
          fetchResources()
        } else {
          message.error('删除失败')
        }
      } catch (error) {
        console.error('删除资源时出错:', error)
        message.error('删除失败')
      }
    }
  })
}

// 保存资源
const saveResource = async (e: Event) => {
  e.preventDefault()
  formRef.value?.validate(async (errors: any) => {
    if (!errors) {
      saving.value = true
      try {
        const token = localStorage.getItem('Authorization')
        const resourceToSave = {
          ...editingResource.value,
          tag: resourceTags.value.join(',')
        }

        const url = editingResource.value.id
            ? '/api/Project/UpdateResource'
            : '/api/Project/AddResource'

        const response = await fetch(url, {
          method: editingResource.value.id ? 'PUT' : 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(resourceToSave)
        })

        if (response.ok) {
          message.success(editingResource.value.id ? '更新成功' : '添加成功')
          showModal.value = false
          fetchResources()
        } else {
          message.error(editingResource.value.id ? '更新失败' : '添加失败')
        }
      } catch (error) {
        console.error('保存资源时出错:', error)
        message.error('保存失败')
      } finally {
        saving.value = false
      }
    }
  })
}

// 获取资源列表
const fetchResources = async () => {
  try {
    const token = localStorage.getItem('Authorization')
    const response = await fetch('/api/Project/GetResources', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (response.ok) {
      const data = await response.json()
      resources.value = data
    } else {
      message.error('获取资源列表失败')
    }
  } catch (error) {
    console.error('获取资源列表时出错:', error)
    message.error('获取资源列表失败')
  }
}

// 组件挂载时初始化
onMounted(() => {
  checkIsMobile()
  window.addEventListener('resize', checkIsMobile)
  fetchResources()
})

// 组件卸载时清理
onUnmounted(() => {
  window.removeEventListener('resize', checkIsMobile)
})
</script>

<style scoped>
/* 移动端适配补充样式 */
@media (max-width: 768px) {
  :deep(.n-card) {
    margin-top: 0.5rem;
    border-radius: 8px;
  }

  :deep(.n-page-header) {
    padding: 0.5rem;
  }

  :deep(.n-modal .n-form-item) {
    margin-bottom: 12px;
  }

  :deep(.n-modal .n-form-item-control) {
    min-height: auto;
  }
}
</style>
<template>
  <div class="min-h-screen transition-colors duration-300"
       :class="{ 'with-sidebar': layoutStore.showSidebar && !layoutStore.isMobile }">
    <div class="px-4 py-6 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8">
        <div class="flex items-center">
          <div>
            <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">社团资源</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">社团资源管理</p>
          </div>
        </div>
        
        <n-button v-if="authorizationStore.isAdmin()" type="primary" @click="showAddResourceModal"
                  class="transition-all duration-200 hover:shadow-md">
          <template #icon>
            <n-icon>
              <Add />
            </n-icon>
          </template>
          添加资源
        </n-button>
      </div>

      <!-- Search -->
      <div class="mb-8">
        <n-input v-model:value="searchTerm" placeholder="搜索资源..." clearable size="large" class="max-w-md">
          <template #prefix>
            <n-icon>
              <Search />
            </n-icon>
          </template>
        </n-input>
      </div>

      <!-- Empty State -->
      <div v-if="filteredResources.length === 0" class="flex flex-col items-center justify-center py-16">
        <n-icon size="48" class="mb-4 text-gray-400">
          <FolderOpen />
        </n-icon>
        <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">暂无资源</h3>
        <p class="text-gray-500 dark:text-gray-400 mb-6">社团现在还没有任何资源</p>
        <n-button v-if="authorizationStore.isAdmin()" type="primary" @click="showAddResourceModal">
          添加第一个资源
        </n-button>
      </div>

      <!-- Resource Grid -->
      <div v-else class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
        <div v-for="resource in filteredResources" :key="resource.id"
             class="overflow-hidden transition-all duration-200 bg-white rounded-xl dark:bg-neutral-800 shadow-sm hover:shadow-md">
          <div class="p-6">
            <div class="flex items-start justify-between">
              <div class="flex-1 min-w-0">
                <h3 class="text-lg font-semibold text-gray-900 truncate dark:text-white">{{ resource.name }}</h3>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400 line-clamp-2">
                  {{ resource.description || '无描述' }}
                </p>
              </div>
            </div>
            
            <div class="mt-4">
              <div v-if="getResourceTags(resource).length > 0" class="flex flex-wrap gap-2">
                <n-tag v-for="tag in getResourceTags(resource)" :key="tag" type="info" size="small" round>
                  {{ tag }}
                </n-tag>
              </div>
              <div v-else class="text-xs text-gray-400 dark:text-gray-500">无标签</div>
            </div>
            
            <div v-if="authorizationStore.isAdmin" class="flex mt-6 space-x-3">
              <n-button size="small" type="primary" quaternary @click="editResource(resource)">
                编辑
              </n-button>
              <n-button size="small" type="error" quaternary @click="deleteResource(resource)">
                删除
              </n-button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <n-modal v-model:show="showModal" preset="card" :style="modalStyle" :title="editingResource.id ? '编辑资源' : '添加资源'"
             @after-leave="resetForm" :mask-closable="false">
      <n-form ref="formRef" :model="editingResource" :rules="rules" label-placement="top" label-width="auto">
        <n-form-item label="资源名称" path="name">
          <n-input v-model:value="editingResource.name" placeholder="请输入资源名称" />
        </n-form-item>
        <n-form-item label="资源描述" path="description">
          <n-input v-model:value="editingResource.description" placeholder="请输入资源描述" type="textarea" :autosize="{ minRows: 3 }" />
        </n-form-item>
        <n-form-item label="资源标签" path="tag">
          <n-dynamic-tags v-model:value="resourceTags" />
        </n-form-item>
      </n-form>
      
      <template #footer>
        <div class="flex justify-end space-x-3">
          <n-button @click="showModal = false" quaternary>取消</n-button>
          <n-button type="primary" @click="saveResource" :loading="saving" class="transition-all duration-200 hover:shadow-md">
            保存
          </n-button>
        </div>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMessage } from 'naive-ui'
import {
  NButton,
  NInput,
  NModal,
  NForm,
  NFormItem,
  NIcon,
  NTag,
  NDynamicTags
} from 'naive-ui'
import { ArrowBack, Add, Search, FolderOpen } from '@vicons/ionicons5'
import { useAuthorizationStore } from '../stores/Authorization'
import { useLayoutStore } from '../stores/LayoutStore'
import { ResourceService } from '../services/ResourceService'
import { ResourceModel } from '../models'

interface Resource {
  id: string
  name: string
  description: string | null
  tag: string | null
}

const router = useRouter()
const message = useMessage()
const authorizationStore = useAuthorizationStore()
const layoutStore = useLayoutStore()

const showModal = ref(false)
const searchTerm = ref('')
const formRef = ref()
const saving = ref(false)

const editingResource = ref<Resource>({
  id: '',
  name: '',
  description: '',
  tag: '',
})

const resourceTags = ref<string[]>([])

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
    tag: '',
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

// 模态框样式
const modalStyle = {
  width: '100%',
  maxWidth: '520px',
  borderRadius: '12px'
}

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
  const dialog = window.$dialog?.warning({
    title: '确认删除',
    content: `确定要删除资源 "${resource.name}" 吗？此操作不可撤销。`,
    positiveText: '删除',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        await ResourceService.deleteResource(resource.id)
        message.success('删除成功')
        await fetchResources()
      } catch (error: any) {
        console.error('删除资源时出错:', error)
        message.error('删除失败: ' + error.message)
      }
    }
  })
}

// 保存资源
const saveResource = async () => {
  formRef.value?.validate(async (errors: any) => {
    if (!errors) {
      saving.value = true
      try {
        // 创建符合 ResourceModel 接口的对象
        const resourceToSave: ResourceModel = {
          id: editingResource.value.id,
          name: editingResource.value.name,
          description: editingResource.value.description || '',
          tag: resourceTags.value.join(',')
        }

        if (editingResource.value.id) {
          // 更新资源
          await ResourceService.updateResource(resourceToSave)
          message.success('更新成功')
        } else {
          // 创建资源
          await ResourceService.createResource(resourceToSave)
          message.success('添加成功')
        }

        showModal.value = false
        await fetchResources()
      } catch (error: any) {
        console.error('保存资源时出错:', error)
        message.error('保存失败: ' + error.message)
      } finally {
        saving.value = false
      }
    }
  })
}

// 获取资源列表
const fetchResources = async () => {
  try {
    const data = await ResourceService.getAllResources()
    resources.value = data.map(resource => ({
      id: resource.id,
      name: resource.name,
      description: resource.description,
      tag: resource.tag
    }))
  } catch (error: any) {
    console.error('获取资源列表时出错:', error)
    message.error('获取资源列表失败: ' + error.message)
  }
}

// 组件挂载时初始化
onMounted(() => {
  fetchResources()
})
</script>

<style scoped>
.main-content {
  padding: 0;
}

@media (min-width: 768px) {
  .main-content.with-sidebar {
    margin-left: 240px;
  }
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
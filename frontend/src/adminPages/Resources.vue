<template>
  <div class="min-h-screen transition-colors duration-300 bg-background text-foreground"
       :class="{ 'with-sidebar': layoutStore.showSidebar && !layoutStore.isMobile }">
    <div class="px-4 py-6 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-8 gap-4">
        <div>
          <h1 class="text-3xl font-semibold tracking-tight">社团资源</h1>
          <p class="text-base text-muted-foreground mt-1">社团资源管理</p>
        </div>
        
        <n-button v-if="authorizationStore.isAdmin()" type="primary" @click="showAddResourceModal"
                  class="rounded-full px-5 py-2.5 text-base transition-all duration-200 hover:shadow-lg">
          <template #icon>
                  <Icon icon="ion:add" />
                </template>
          添加资源
        </n-button>
      </div>

      <!-- Search -->
      <div class="mb-8">
        <n-input v-model:value="searchTerm" placeholder="搜索资源..." clearable size="large" 
                 class="max-w-md rounded-2xl bg-card border-0 shadow-sm focus-within:shadow-md transition-shadow">
          <template #prefix>
            <Icon icon="ion:search" class="text-muted-foreground" />
          </template>
        </n-input>
      </div>

      <!-- Empty State -->
      <div v-if="filteredResources.length === 0" class="flex flex-col items-center justify-center py-20 rounded-3xl bg-card border border-border shadow-sm">
        <div class="w-20 h-20 rounded-full bg-muted flex items-center justify-center mb-6">
          <Icon icon="ion:folder-open" size="32" class="text-muted-foreground" />
        </div>
        <h3 class="text-2xl font-semibold mb-2">暂无资源</h3>
        <p class="text-muted-foreground mb-8 max-w-md text-center px-4">社团现在还没有任何资源，请添加第一个资源</p>
        <n-button v-if="authorizationStore.isAdmin()" type="primary" @click="showAddResourceModal"
                  class="rounded-full px-6 py-2.5 text-base">
          添加第一个资源
        </n-button>
      </div>

      <!-- Resource Grid -->
      <div v-else class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <div v-for="resource in filteredResources" :key="resource.id"
             class="overflow-hidden transition-all duration-300 bg-card rounded-3xl shadow-sm border border-border hover:shadow-xl">
          <div class="p-6">
            <div class="flex items-start justify-between">
              <div class="flex-1 min-w-0">
                <h3 class="text-xl font-semibold truncate mb-2">{{ resource.name }}</h3>
                <p class="text-base text-muted-foreground line-clamp-2">
                  {{ resource.description || '无描述' }}
                </p>
              </div>
            </div>
            
            <div class="mt-5">
              <div v-if="getResourceTags(resource).length > 0" class="flex flex-wrap gap-2">
                <n-tag v-for="tag in getResourceTags(resource)" :key="tag" type="info" size="small" round
                       class="rounded-full px-3 py-1 text-xs">
                  {{ tag }}
                </n-tag>
              </div>
              <div v-else class="text-sm text-muted-foreground">无标签</div>
            </div>
            
            <div v-if="authorizationStore.isAdmin" class="flex mt-8 space-x-3">
              <n-button size="small" type="primary" tertiary @click="editResource(resource)"
                        class="rounded-full px-4 py-1.5 text-sm">
                编辑
              </n-button>
              <n-button size="small" type="error" tertiary @click="deleteResource(resource)"
                        class="rounded-full px-4 py-1.5 text-sm">
                删除
              </n-button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <n-modal v-model:show="showModal" preset="card" :style="modalStyle" :title="editingResource.id ? '编辑资源' : '添加资源'"
             @after-leave="resetForm" :mask-closable="false" class="rounded-3xl overflow-hidden">
      <n-form ref="formRef" :model="editingResource" :rules="rules" label-placement="top" label-width="auto">
        <n-form-item label="资源名称" path="name">
          <n-input v-model:value="editingResource.name" placeholder="请输入资源名称" 
                   class="rounded-2xl bg-card border-border" />
        </n-form-item>
        <n-form-item label="资源描述" path="description">
          <n-input v-model:value="editingResource.description" placeholder="请输入资源描述" 
                   type="textarea" :autosize="{ minRows: 4 }" class="rounded-2xl bg-card border-border" />
        </n-form-item>
        <n-form-item label="资源标签" path="tag">
          <n-dynamic-tags v-model:value="resourceTags" class="rounded-2xl bg-card border-border" />
        </n-form-item>
      </n-form>
      
      <template #footer>
        <div class="flex justify-end space-x-3 pt-2">
          <n-button @click="showModal = false" quaternary class="rounded-full px-5 py-2.5">取消</n-button>
          <n-button type="primary" @click="saveResource" :loading="saving" 
                    class="rounded-full px-5 py-2.5 transition-all duration-200 hover:shadow-md">
            保存
          </n-button>
        </div>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useMessage } from 'naive-ui'
import {
  NButton,
  NInput,
  NModal,
  NForm,
  NFormItem,
  NTag,
  NDynamicTags
} from 'naive-ui'
import { Icon } from '@iconify/vue'
import { useAuthorizationStore } from '../stores/Authorization'
import { useLayoutStore } from '../stores/LayoutStore'
import { ResourceService } from '../services/ResourceService'
import { useDialog } from 'naive-ui'
import {ResourceModel} from "../models";

interface Resource {
  id: string
  name: string
  description: string | null
  tag: string | null | undefined
}

const message = useMessage()
const dialog = useDialog()
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
  maxWidth: '520px'
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
  dialog.warning({
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
        description: resource.description || null,
        tag: resource.tag === undefined ? null : resource.tag
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
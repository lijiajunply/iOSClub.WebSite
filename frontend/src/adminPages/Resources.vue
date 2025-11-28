<template>
  <div class="min-h-screen transition-colors duration-300 text-gray-900 dark:text-gray-100">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

      <!-- Search -->
      <div class="mb-8 w-full">
        <div class="relative w-full">
          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
            <Icon icon="ion:search" class="text-gray-400 w-5 h-5"/>
          </div>
          <input v-model="searchTerm" placeholder="搜索资源..."
                   class="w-full pl-12 pr-4 py-3 rounded-2xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all">
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!loading && filteredResources.length === 0"
           class="flex flex-col items-center justify-center py-20 rounded-3xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-sm">
        <div class="w-20 h-20 rounded-full bg-gray-100 dark:bg-gray-700 flex items-center justify-center mb-6">
          <Icon icon="ion:folder-open" class="w-10 h-10 text-gray-400 dark:text-gray-500"/>
        </div>
        <h3 class="text-2xl font-semibold mb-2">暂无资源</h3>
        <p class="text-gray-500 dark:text-gray-400 mb-8 max-w-md text-center px-4">
          社团现在还没有任何资源，请添加第一个资源</p>
        <button v-if="authorizationStore.isAdmin()" @click="showAddResourceModal"
                  class="rounded-full px-6 py-2.5 text-base bg-blue-500 hover:bg-blue-600 text-white transition-colors">
          添加第一个资源
        </button>
      </div>

      <!-- Resource Grid -->
      <div v-else-if="!loading" class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <div v-for="resource in filteredResources" :key="resource.id"
             class="overflow-hidden transition-all duration-300 bg-white dark:bg-gray-800 rounded-3xl shadow-sm border border-gray-200 dark:border-gray-700 hover:shadow-xl">
          <div class="p-6">
            <div class="flex items-start justify-between">
              <div class="flex-1 min-w-0">
                <h3 class="text-xl font-semibold truncate mb-2">{{ resource.name }}</h3>
                <p class="text-base text-gray-500 dark:text-gray-400 line-clamp-2">
                  {{ resource.description || '无描述' }}
                </p>
              </div>
            </div>

            <div class="mt-5">
              <div v-if="getResourceTags(resource).length > 0" class="flex flex-wrap gap-2">
                <span v-for="tag in getResourceTags(resource)" :key="tag"
                      class="rounded-full px-3 py-1 text-xs bg-blue-100 dark:bg-blue-900/50 text-blue-800 dark:text-blue-200">
                  {{ tag }}
                </span>
              </div>
              <div v-else class="text-sm text-gray-500 dark:text-gray-400">无标签</div>
            </div>

            <div v-if="authorizationStore.isAdmin" class="flex mt-8 space-x-3">
              <button @click="editResource(resource)"
                      class="rounded-full px-4 py-1.5 text-sm bg-blue-100 hover:bg-blue-200 dark:bg-blue-900/50 dark:hover:bg-blue-800 text-blue-700 dark:text-blue-200 transition-colors">
                编辑
              </button>
              <button @click="deleteResource(resource)"
                      class="rounded-full px-4 py-1.5 text-sm bg-red-100 hover:bg-red-200 dark:bg-red-900/50 dark:hover:bg-red-800 text-red-700 dark:text-red-200 transition-colors">
                删除
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Skeleton Loading -->
      <div v-else class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <SkeletonLoader v-for="i in 6" :key="i" type="card"/>
      </div>
    </main>

    <!-- Modal -->
    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div
          class="bg-white dark:bg-gray-800 rounded-2xl overflow-hidden w-full max-w-md transform transition-all duration-300 ease-out shadow-2xl">
        <div class="p-6">
          <div class="flex items-center justify-between mb-5">
            <h3 class="text-2xl font-bold text-gray-800 dark:text-gray-100">{{
                editingResource.id ? '编辑资源' : '添加资源'
              }}</h3>
            <button @click="showModal = false"
                    class="text-gray-400 hover:text-gray-500 dark:text-gray-400 dark:hover:text-gray-300 rounded-full p-1 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
              <Icon icon="ion:close" class="w-6 h-6"/>
            </button>
          </div>

          <form @submit.prevent="saveResource" class="space-y-5">
            <div>
              <label class="block text-sm font-medium mb-2 text-gray-700 dark:text-gray-300">资源名称</label>
              <input v-model="editingResource.name" placeholder="请输入资源名称"
                     class="w-full px-4 py-3 rounded-xl bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                     required/>
            </div>

            <div>
              <label class="block text-sm font-medium mb-2 text-gray-700 dark:text-gray-300">资源描述</label>
              <textarea v-model="editingResource.description" placeholder="请输入资源描述"
                        class="w-full px-4 py-3 rounded-xl bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent min-h-[120px] transition-all"/>
            </div>

            <div>
              <label class="block text-sm font-medium mb-2 text-gray-700 dark:text-gray-300">资源标签</label>
              <div class="flex flex-wrap gap-2 mb-3">
                <span v-for="(tag, index) in resourceTags" :key="index"
                      class="rounded-full px-3 py-1.5 text-sm bg-linear-to-r from-blue-500 to-indigo-500 text-white flex items-center shadow-sm">
                  {{ tag }}
                  <button @click="removeTag(index)" type="button"
                          class="ml-2 text-white hover:text-gray-200 focus:outline-none">
                    <Icon icon="ion:close" class="w-4 h-4"/>
                  </button>
                </span>
              </div>
              <div class="flex gap-2">
                <input v-model="newTag" @keyup.enter="addTag" placeholder="输入标签后按回车"
                       class="flex-1 px-4 py-3 rounded-xl bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"/>
                <button @click="addTag" type="button"
                        class="px-5 py-3 rounded-xl bg-linear-to-r from-gray-100 to-gray-200 dark:from-gray-600 dark:to-gray-700 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-200 hover:from-gray-200 hover:to-gray-300 dark:hover:from-gray-500 dark:hover:to-gray-600 transition-all shadow-sm">
                  添加
                </button>
              </div>
            </div>

            <div class="flex justify-end space-x-4 pt-4">
              <button @click="showModal = false" type="button"
                      class="rounded-xl px-6 py-3 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors font-medium">
                取消
              </button>
              <button type="submit" :disabled="saving"
                      class="rounded-xl px-6 py-3 transition-all duration-300 font-medium shadow-md hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed flex items-center bg-linear-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white"
                      :class="{ 'opacity-50 cursor-not-allowed': saving }">
                <span v-if="saving" class="mr-2">
                  <Icon icon="ion:loading" class="w-5 h-5 animate-spin"/>
                </span>
                {{ saving ? '保存中...' : '保存' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted, onBeforeUnmount, defineComponent, h} from 'vue'
import {Icon} from '@iconify/vue'
import SkeletonLoader from '../components/SkeletonLoader.vue'
import {useAuthorizationStore} from '../stores/Authorization'
import {ResourceService} from '../services/ResourceService'
import {useDialog} from 'naive-ui'
import type {ResourceModel} from '../models'
import {useLayoutStore} from '../stores/LayoutStore'

interface Resource {
  id: string
  name: string
  description: string | null
  tag: string | null | undefined
}

const dialog = useDialog()
const authorizationStore = useAuthorizationStore()
const layoutStore = useLayoutStore()

const showModal = ref(false)
const searchTerm = ref('')
const saving = ref(false)
const newTag = ref('')
const loading = ref(false)

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
  newTag.value = ''
}

// 添加标签
const addTag = () => {
  if (newTag.value.trim() && !resourceTags.value.includes(newTag.value.trim())) {
    resourceTags.value.push(newTag.value.trim())
  }
  newTag.value = ''
}

// 删除标签
const removeTag = (index: number) => {
  resourceTags.value.splice(index, 1)
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

// 显示添加资源模态框
const showAddResourceModal = () => {
  resetForm()
  showModal.value = true
}

// 编辑资源
const editResource = (resource: Resource) => {
  editingResource.value = {...resource}
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
        await fetchResources()
        // 使用原生浏览器通知替代 message.success
        console.log('删除成功')
      } catch (error: any) {
        console.error('删除资源时出错:', error)
        // 使用原生浏览器通知替代 message.error
        alert('删除失败: ' + error.message)
      }
    }
  })
}

// 保存资源
const saveResource = async () => {
  if (!editingResource.value.name.trim()) {
    alert('请输入资源名称')
    return
  }

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
      console.log('更新成功')
    } else {
      // 创建资源
      await ResourceService.createResource(resourceToSave)
      console.log('添加成功')
    }

    showModal.value = false
    await fetchResources()
  } catch (error: any) {
    console.error('保存资源时出错:', error)
    alert('保存失败: ' + error.message)
  } finally {
    saving.value = false
  }
}

// 获取资源列表
const fetchResources = async () => {
  try {
    loading.value = true
    const data = await ResourceService.getAllResources()
    resources.value = data.map(resource => ({
      id: resource.id,
      name: resource.name,
      description: resource.description || null,
      tag: resource.tag === undefined ? null : resource.tag
    }))
  } catch (error: any) {
    console.error('获取资源列表时出错:', error)
    alert('获取资源列表失败: ' + error.message)
  } finally {
    loading.value = false
  }
}

// 组件挂载时初始化
onMounted(() => {
  fetchResources()

  // Set page header
  layoutStore.setPageHeader(
      '社团资源',
      '社团资源管理'
  )

  // Show page actions
  layoutStore.setShowPageActions(true)

  // 创建操作栏组件
  const ActionsComponent = defineComponent({
    setup() {
      return () => h('div', { class: 'flex items-center justify-end space-x-3' }, [
        // 添加资源按钮
        h('button', {
          class: 'rounded-full bg-blue-500 hover:bg-blue-600 text-white flex items-center gap-2 px-4 py-2 transition-colors',
          onClick: showAddResourceModal
        }, [
          h(Icon, { icon: 'ion:add', class: 'w-4 h-4' }),
          h('span', '添加资源')
        ])
      ])
    }
  })

  // 注册操作栏组件到LayoutStore
  layoutStore.setActionsComponent(ActionsComponent)
})

onBeforeUnmount(() => {
  // Clear page header and actions
  layoutStore.clearPageHeader()
  layoutStore.setActionsComponent(null)
})
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
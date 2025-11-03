<template>
  <div class="min-h-screen transition-colors duration-300 p-4 md:p-6">
    <!-- 页面标题和操作按钮 -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">项目管理</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400">管理社团的所有项目</p>
      </div>
      
      <n-button
          type="primary"
          @click="showAddProjectModal"
          class="rounded-full"
          :ghost="false"
      >
        <template #icon>
          <Icon icon="material-symbols:add-circle" />
        </template>
        添加项目
      </n-button>
    </div>

    <!-- 项目标签页 -->
    <n-card
        class="mb-6 rounded-2xl overflow-hidden bg-white/90 dark:bg-gray-800/90 backdrop-blur-sm border border-gray-200 dark:border-gray-700 shadow-sm transition-colors duration-300"
        :bordered="false"
    >
      <n-tabs v-model:value="activeTab" class="mt-2" size="large">
        <n-tab-pane name="active" tab="进行中">
          <div v-if="loading" class="mt-4 space-y-4">
            <SkeletonLoader v-for="i in 3" :key="i" type="card" />
          </div>
          <div v-else-if="activeProjects.length > 0" class="mt-4 space-y-4">
            <div
                v-for="project in activeProjects"
                :key="project.id"
                class="bg-white dark:bg-gray-750 rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-all duration-300 border border-gray-100 dark:border-gray-700 p-4"
              >
              <div class="flex items-start justify-between">
                <div class="flex items-start">
                  <div class="bg-blue-100 dark:bg-blue-900/30 p-3 rounded-xl mr-4">
                    <Icon icon="material-symbols:folder-open" size="24" class="text-blue-600 dark:text-blue-400" />
                  </div>
                  <div class="flex-1">
                    <div class="flex items-center justify-between">
                      <h3 class="font-medium text-gray-900 dark:text-white text-lg">{{ project.name }}</h3>
                      <n-tag 
                        :type="getProjectStatusType(project.status)" 
                        class="rounded-full px-3 py-0.5"
                      >
                        {{ project.status }}
                      </n-tag>
                    </div>
                    <p class="text-sm text-gray-600 dark:text-gray-300 mt-1 line-clamp-2">{{ project.description }}</p>
                    <div class="mt-3 flex items-center justify-between">
                      <span class="text-xs text-gray-500 dark:text-gray-400">
                        {{ formatDate(project.startDate) }} - {{ formatDate(project.endDate) || '进行中' }}
                      </span>
                      <div class="flex items-center space-x-2">
                        <span class="text-sm font-medium text-gray-700 dark:text-gray-300">{{ project.progress }}%</span>
                      </div>
                    </div>
                    <!-- 项目进度条 -->
                    <div class="mt-2">
                      <n-progress 
                        type="line" 
                        :percentage="project.progress" 
                        class="h-1.5 rounded-full"
                        :indicator-placement="'none'"
                        :color="getStatusProgressColor(project.status)"
                      />
                    </div>
                  </div>
                </div>
                <div class="flex space-x-2 ml-4">
                  <n-button
                      text
                      type="info"
                      size="small"
                      @click="editProject(project)"
                      class="rounded-full h-9 w-9 p-0 flex items-center justify-center"
                    >
                    <Icon icon="material-symbols:edit" size="18" />
                  </n-button>
                  <n-button
                      text
                      type="default"
                      size="small"
                      @click="viewProjectDetails(project)"
                      class="rounded-full h-9 w-9 p-0 flex items-center justify-center"
                    >
                    <Icon icon="material-symbols:visibility" size="18" />
                  </n-button>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="text-center py-12">
            <div class="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gray-100 dark:bg-gray-700 mb-4">
              <Icon icon="material-symbols:folder-open" size="36" class="text-gray-400 dark:text-gray-500" />
            </div>
            <p class="text-gray-500 dark:text-gray-400 mb-3">暂无进行中的项目</p>
            <n-button 
              type="primary" 
              text 
              @click="showAddProjectModal"
              class="rounded-full"
            >
              添加项目
            </n-button>
          </div>
        </n-tab-pane>
        <n-tab-pane name="completed" tab="已完成">
          <div v-if="loading" class="mt-4 space-y-4">
            <SkeletonLoader v-for="i in 3" :key="i" type="card" />
          </div>
          <div v-else-if="completedProjects.length > 0" class="mt-4 space-y-4">
            <div
                v-for="project in completedProjects"
                :key="project.id"
                class="bg-white dark:bg-gray-750 rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-all duration-300 border border-gray-100 dark:border-gray-700 p-4"
              >
              <div class="flex items-start justify-between">
                <div class="flex items-start">
                  <div class="bg-green-100 dark:bg-green-900/30 p-3 rounded-xl mr-4">
                    <Icon icon="material-symbols:check-box" size="24" class="text-green-600 dark:text-green-400" />
                  </div>
                  <div class="flex-1">
                    <div class="flex items-center justify-between">
                      <h3 class="font-medium text-gray-900 dark:text-white text-lg">{{ project.name }}</h3>
                      <n-tag 
                        type="success" 
                        class="rounded-full px-3 py-0.5"
                      >
                        已完成
                      </n-tag>
                    </div>
                    <p class="text-sm text-gray-600 dark:text-gray-300 mt-1 line-clamp-2">{{ project.description }}</p>
                    <div class="mt-3">
                      <span class="text-xs text-gray-500 dark:text-gray-400">
                        {{ formatDate(project.startDate) }} - {{ formatDate(project.endDate) }}
                      </span>
                    </div>
                  </div>
                </div>
                <div class="flex space-x-2 ml-4">
                  <n-button
                      text
                      type="default"
                      size="small"
                      @click="viewProjectDetails(project)"
                      class="rounded-full h-9 w-9 p-0 flex items-center justify-center"
                    >
                    <Icon icon="material-symbols:visibility" size="18" />
                  </n-button>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="text-center py-12">
            <div class="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gray-100 dark:bg-gray-700 mb-4">
              <Icon icon="material-symbols:check-box" size="36" class="text-gray-400 dark:text-gray-500" />
            </div>
            <p class="text-gray-500 dark:text-gray-400">暂无已完成的项目</p>
          </div>
        </n-tab-pane>
      </n-tabs>
    </n-card>

    <!-- 添加/编辑项目模态框 -->
    <n-modal
        v-model:show="showModal"
        :title="editingProject.id ? '编辑项目' : '添加项目'"
        preset="card"
        class="max-w-lg w-full rounded-xl"
        style="--n-modal-border-radius: 12px"
      >
        <n-form
            :model="editingProject"
            :rules="rules"
            ref="formRef"
            class="mt-4"
          >
          <n-form-item label="项目名称" path="name">
            <n-input 
              v-model:value="editingProject.name" 
              placeholder="请输入项目名称"
              class="rounded-lg"
              :bordered="false"
            />
          </n-form-item>
          <n-form-item label="项目描述" path="description">
            <n-input
                v-model:value="editingProject.description"
                type="textarea"
                placeholder="请输入项目描述"
                :autosize="{ minRows: 3, maxRows: 5 }"
                class="rounded-lg"
                :bordered="false"
              />
          </n-form-item>
          <n-form-item label="开始日期" path="startDate">
            <n-date-picker
                v-model:value="editingProject.startDate"
                type="date"
                placeholder="选择开始日期"
                class="rounded-lg"
                :bordered="false"
              />
          </n-form-item>
          <n-form-item label="结束日期" path="endDate">
            <n-date-picker
                v-model:value="editingProject.endDate"
                type="date"
                placeholder="选择结束日期"
                class="rounded-lg"
                :bordered="false"
              />
          </n-form-item>
          <n-form-item label="状态" path="status">
            <n-select
                v-model:value="editingProject.status"
                :options="statusOptions"
                placeholder="选择项目状态"
                class="rounded-lg"
                :bordered="false"
              />
          </n-form-item>
          <n-form-item label="进度" path="progress" v-if="editingProject.status !== '已完成'">
            <div class="flex items-center space-x-3">
              <n-input-number 
                v-model:value="editingProject.progress"
                :min="0"
                :max="100"
                class="w-20 rounded-lg"
                :bordered="false"
              />
              <span class="text-sm text-gray-500 dark:text-gray-400">%</span>
            </div>
          </n-form-item>
          <n-form-item>
            <div class="flex justify-end space-x-4">
              <n-button @click="showModal = false" class="rounded-full" :ghost="true">取消</n-button>
              <n-button 
                type="primary" 
                @click="saveProject" 
                class="rounded-full"
                :loading="loading"
              >
                保存
              </n-button>
            </div>
          </n-form-item>
        </n-form>
      </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useMessage } from 'naive-ui'
import { NButton, NCard, NModal, NForm, NFormItem, NInput, NDatePicker, NSelect, NTabs, NTabPane, NTag, NProgress, NInputNumber } from 'naive-ui'
import { Icon } from '@iconify/vue'
import SkeletonLoader from '../components/SkeletonLoader.vue'
import { ProjectService } from '../services/ProjectService'

// 定义项目类型
interface Project {
  id: string | number | null
  name: string
  description: string
  startDate: string | null
  endDate: string | null
  status: string
  progress: number
}

const message = useMessage()
const showModal = ref(false)
const formRef = ref<InstanceType<typeof NForm>>()
const loading = ref(false)
const activeTab = ref('active')

const projects = ref<Project[]>([])

const editingProject = ref<Project>({
  id: null,
  name: '',
  description: '',
  startDate: null,
  endDate: null,
  status: '进行中',
  progress: 0
})

const rules = {
  name: {
    required: true,
    message: '请输入项目名称',
    trigger: 'blur'
  },
  description: {
    required: true,
    message: '请输入项目描述',
    trigger: 'blur'
  },
  startDate: {
    required: true,
    message: '请选择开始日期',
    trigger: 'change'
  },
  endDate: {
    required: true,
    message: '请选择结束日期',
    trigger: 'change'
  },
  status: {
    required: true,
    message: '请选择项目状态',
    trigger: 'change'
  }
}

const statusOptions = [
  { label: '规划中', value: '规划中' },
  { label: '进行中', value: '进行中' },
  { label: '已完成', value: '已完成' },
  { label: '已暂停', value: '已暂停' }
]

const activeProjects = computed(() => {
  return projects.value.filter(project => project.status !== '已完成')
})

const completedProjects = computed(() => {
  return projects.value.filter(project => project.status === '已完成')
})

const getProjectStatusType = (status: string): string => {
  switch (status) {
    case '规划中': return 'default'
    case '进行中': return 'info'
    case '已完成': return 'success'
    case '已暂停': return 'warning'
    default: return 'default'
  }
}

const getStatusProgressColor = (status: string): string => {
  switch (status) {
    case '规划中': return '#6b7280'
    case '进行中': return '#3b82f6'
    case '已完成': return '#10b981'
    case '已暂停': return '#f59e0b'
    default: return '#3b82f6'
  }
}

const formatDate = (dateString: string | null): string => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
}

const showAddProjectModal = () => {
  editingProject.value = {
    id: null,
    name: '',
    description: '',
    startDate: null,
    endDate: null,
    status: '进行中',
    progress: 0
  }
  showModal.value = true
}

const editProject = (project: Project) => {
  editingProject.value = { ...project }
  showModal.value = true
}

const viewProjectDetails = (project: Project) => {
  message.info(`查看项目 "${project.name}" 的详细信息`)
  // 实际项目中可以跳转到项目详情页
}

const saveProject = async (e?: Event) => {
  if (e) e.preventDefault()
  if (!formRef.value) return
  
  try {
    await formRef.value.validate()
    loading.value = true
    
    const projectData = {
      id: editingProject.value.id || undefined,
      name: editingProject.value.name,
      description: editingProject.value.description,
      startTime: editingProject.value.startDate ? new Date(editingProject.value.startDate).toISOString() : null,
      endTime: editingProject.value.endDate ? new Date(editingProject.value.endDate).toISOString() : null,
      status: editingProject.value.status,
      progress: editingProject.value.progress
    }

    if (editingProject.value.id) {
      // 更新项目
      await ProjectService.createOrUpdateProject(projectData)
      message.success('项目信息已更新')
    } else {
      // 添加新项目
      await ProjectService.createOrUpdateProject(projectData)
      message.success('新项目已添加')
    }
    
    showModal.value = false
    await fetchProjects()
  } catch (error: any) {
    console.error('保存项目时出错:', error)
    message.error('保存项目失败: ' + (error.message || '未知错误'))
  } finally {
    loading.value = false
  }
}

// 获取项目列表
const fetchProjects = async () => {
  try {
    loading.value = true
    const data = await ProjectService.getAllProjects()
    projects.value = data.map(project => ({
      id: project.id,
      name: project.name,
      description: project.description || '',
      startDate: project.startTime,
      endDate: project.endTime,
      status: project.status || '进行中',
      progress: project.progress || 0
    }))
  } catch (error: any) {
    console.error('获取项目列表时出错:', error)
    message.error('获取项目列表失败: ' + (error.message || '未知错误'))
  } finally {
    loading.value = false
  }
}

// 组件挂载时获取项目数据
onMounted(() => {
  fetchProjects()
})
</script>
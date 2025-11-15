<script setup lang="ts">
import {ref, computed, onMounted} from 'vue'
import {useRouter, useRoute} from 'vue-router'
import {NCard, useMessage} from 'naive-ui'
import {NDatePicker, NButton, NSelect, NModal} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {ProjectService} from '../services/ProjectService'
import type {ProjectModel, StaffModel, TaskModel} from '../models'

const router = useRouter()
const route = useRoute()
const message = useMessage()

// 获取路由参数
const projectId = computed(() => route.params.id as string | undefined)

// 项目数据
const project = ref<ProjectModel | null>(null)
const projectMembers = ref<StaffModel[]>([])
const projectTasks = ref<TaskModel[]>([])

// 加载状态
const loading = ref(false)
const loadingMembers = ref(false)
const loadingTasks = ref(false)

// 添加任务相关
const showAddTaskDialog = ref(false)
const taskForm = ref({
  name: '',
  description: '',
  startTime: Date.now(),
  endTime: Date.now() + 86400000, // 默认结束时间为明天
  status: '未开始'
})

// 初始化数据
onMounted(async () => {
  if (!projectId.value) {
    message.error('项目ID不能为空')
    await router.push('/Centre/Admin')
    return
  }

  await loadProjectData()
  await loadProjectMembers()
  await loadProjectTasks()
})

// 加载项目数据
const loadProjectData = async () => {
  if (!projectId.value) return

  try {
    loading.value = true
    const projects = await ProjectService.getAllProjects()
    const foundProject = projects.find(p => p.id === projectId.value)

    if (foundProject) {
      project.value = foundProject
    } else {
      message.error('未找到项目数据')
      await router.push('/Centre/Admin')
    }
  } catch (error: any) {
    console.error('加载项目数据失败:', error)
    message.error(error.message || '加载项目数据失败')
  } finally {
    loading.value = false
  }
}

// 加载项目成员
const loadProjectMembers = async () => {
  if (!projectId.value || !project.value) return

  try {
    loadingMembers.value = true
    projectMembers.value = project.value.staffs || []
  } catch (error: any) {
    console.error('加载项目成员失败:', error)
    message.error(error.message || '加载项目成员失败')
  } finally {
    loadingMembers.value = false
  }
}

// 加载项目任务
const loadProjectTasks = async () => {
  if (!projectId.value || !project.value) return

  try {
    loadingTasks.value = true
    projectTasks.value = project.value.tasks || []
  } catch (error: any) {
    console.error('加载项目任务失败:', error)
    message.error(error.message || '加载项目任务失败')
  } finally {
    loadingTasks.value = false
  }
}

// 格式化日期
const formatDate = (date: Date | number) => {
  if (!date) return ''
  const d = new Date(date)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

// 获取姓名首字母
const getInitials = (name: string) => {
  if (!name) return ''
  return name.charAt(0).toUpperCase()
}

// 打开添加任务对话框
const openAddTaskDialog = () => {
  taskForm.value = {
    name: '',
    description: '',
    startTime: Date.now(),
    endTime: Date.now() + 86400000,
    status: '未开始'
  }
  showAddTaskDialog.value = true
}

// 添加任务
const addTask = async () => {
  if (!projectId.value) return

  if (!taskForm.value.name.trim()) {
    message.warning('任务名称不能为空')
    return
  }

  if (taskForm.value.startTime >= taskForm.value.endTime) {
    message.warning('结束时间必须大于开始时间')
    return
  }

  try {
    // 这里应该调用实际的API来添加任务
    // 暂时模拟添加任务的操作
    const newTask: TaskModel = {
      id: Date.now().toString(),
      ...taskForm.value,
      startTime: Number(taskForm.value.startTime),
      endTime: Number(taskForm.value.endTime)
    }

    projectTasks.value.push(newTask)
    showAddTaskDialog.value = false
    message.success('任务添加成功')
  } catch (error: any) {
    console.error('添加任务失败:', error)
    message.error(error.message || '添加任务失败')
  }
}

// 删除任务
const deleteTask = async (taskId: string) => {
  if (!projectId.value) return

  try {
    // 这里应该调用实际的API来删除任务
    // 暂时模拟删除任务的操作
    projectTasks.value = projectTasks.value.filter(task => task.id !== taskId)
    message.success('任务删除成功')
  } catch (error: any) {
    console.error('删除任务失败:', error)
    message.error(error.message || '删除任务失败')
  }
}
</script>

<template>
  <div
      class="min-h-screen bg-gradient-to-br from-white to-gray-50 dark:from-gray-900 dark:to-gray-800 text-gray-900 dark:text-white transition-colors duration-300">
    <!-- 页面头部 -->
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 class="text-2xl font-semibold tracking-tight">
            项目详情
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            查看和管理项目信息
          </p>
        </div>

        <button
            @click="router.push('/Centre/Admin')"
            class="px-4 py-2 rounded-lg bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors flex items-center"
        >
          <Icon icon="ion:arrow-back" class="mr-1.5"/>
          返回
        </button>
      </div>

      <!-- 项目基本信息 -->
      <div v-if="loading"
           class="bg-white/80 dark:bg-gray-800/80 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden mb-8">
        <div class="p-6">
          <div class="animate-pulse space-y-4">
            <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/3"></div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/4"></div>
          </div>
        </div>
      </div>

      <div v-else-if="project"
           class="bg-white/80 dark:bg-gray-800/80 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden mb-8">
        <div class="p-6">
          <div class="flex justify-between items-start">
            <div>
              <h2 class="text-xl font-semibold mb-2">{{ project.name }}</h2>
              <p class="text-gray-600 dark:text-gray-300 mb-4">{{ project.description }}</p>
              <div class="flex flex-wrap gap-4 text-sm">
                <div>
                  <span class="text-gray-500 dark:text-gray-400">部门:</span>
                  <span class="ml-2 font-medium">{{ project.department }}</span>
                </div>
                <div>
                  <span class="text-gray-500 dark:text-gray-400">开始时间:</span>
                  <span class="ml-2 font-medium">{{ formatDate(project.startTime) }}</span>
                </div>
                <div>
                  <span class="text-gray-500 dark:text-gray-400">结束时间:</span>
                  <span class="ml-2 font-medium">{{ formatDate(project.endTime) }}</span>
                </div>
              </div>
            </div>
            <button
                @click="router.push(`/Centre/ProjectEditor/${project.id}`)"
                class="px-4 py-2 rounded-lg bg-blue-500 hover:bg-blue-600 text-white transition-colors flex items-center"
            >
              <Icon icon="ion:edit" class="mr-1.5"/>
              编辑
            </button>
          </div>
        </div>
      </div>

      <!-- 项目统计与信息展示 -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <!-- 项目成员 -->
        <div
            class="bg-white/80 dark:bg-gray-800/80 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <h2 class="text-lg font-medium">项目成员</h2>
            <p class="text-sm text-gray-500 dark:text-gray-400">参与该项目的成员列表</p>
          </div>
          <div class="p-6">
            <div v-if="loadingMembers" class="space-y-3">
              <div v-for="i in 4" :key="i" class="bg-gray-100 dark:bg-gray-700 rounded-lg p-3 animate-pulse">
                <div class="h-4 bg-gray-200 dark:bg-gray-600 rounded w-1/3 mb-2"></div>
                <div class="h-3 bg-gray-200 dark:bg-gray-600 rounded w-1/2"></div>
              </div>
            </div>
            <div v-else-if="projectMembers.length === 0" class="text-center py-6">
              <Icon icon="ion:people-off" class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500"/>
              <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">暂无项目成员</p>
            </div>
            <div v-else class="space-y-3">
              <div
                  v-for="member in projectMembers"
                  :key="member.userId"
                  class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3 flex items-center"
              >
                <div
                    class="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-300 font-medium mr-3">
                  {{ getInitials(member.name) }}
                </div>
                <div>
                  <p class="font-medium">{{ member.name }}</p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">{{ member.department }} - {{
                      member.identity
                    }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 项目任务 -->
        <div
            class="bg-white/80 dark:bg-gray-800/80 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700 flex justify-between items-center">
            <div>
              <h2 class="text-lg font-medium">项目任务</h2>
              <p class="text-sm text-gray-500 dark:text-gray-400">该项目下的任务列表</p>
            </div>
            <button
                @click="openAddTaskDialog"
                class="px-3 py-1.5 rounded-lg bg-blue-500 hover:bg-blue-600 text-white text-sm transition-colors flex items-center"
            >
              <Icon icon="ion:add" class="mr-1"/>
              添加任务
            </button>
          </div>
          <div class="p-6">
            <div v-if="loadingTasks" class="space-y-3">
              <div v-for="i in 4" :key="i" class="bg-gray-100 dark:bg-gray-700 rounded-lg p-3 animate-pulse">
                <div class="flex justify-between mb-2">
                  <div class="h-4 bg-gray-200 dark:bg-gray-600 rounded w-1/3"></div>
                  <div class="h-4 bg-gray-200 dark:bg-gray-600 rounded w-1/6"></div>
                </div>
                <div class="h-3 bg-gray-200 dark:bg-gray-600 rounded w-full mb-2"></div>
                <div class="h-3 bg-gray-200 dark:bg-gray-600 rounded w-full mb-2"></div>
                <div class="flex justify-between">
                  <div class="h-3 bg-gray-200 dark:bg-gray-600 rounded w-1/4"></div>
                  <div class="h-3 bg-gray-200 dark:bg-gray-600 rounded w-1/4"></div>
                </div>
              </div>
            </div>
            <div v-else-if="projectTasks.length === 0" class="text-center py-6">
              <Icon icon="ion:document-text-off" class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500"/>
              <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">暂无项目任务</p>
            </div>
            <div v-else class="space-y-3">
              <div
                  v-for="task in projectTasks"
                  :key="task.id"
                  class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3"
              >
                <div class="flex justify-between items-start mb-2">
                  <p class="font-medium">{{ task.name }}</p>
                  <span
                      class="text-xs px-2 py-1 rounded-full"
                      :class="{
                      'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300': task.status === '完成',
                      'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300': task.status === '进行中',
                      'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300': task.status === '未开始',
                      'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300': task.status === '延期'
                    }"
                  >
                    {{ task.status }}
                  </span>
                </div>
                <p class="text-sm text-gray-600 dark:text-gray-400 mb-2 line-clamp-2">{{ task.description }}</p>
                <div class="flex justify-between items-center text-xs text-gray-500 dark:text-gray-400">
                  <span>{{ formatDate(task.startTime) }} - {{ formatDate(task.endTime) }}</span>
                  <div class="flex items-center space-x-2">
                    <span>{{ task.users?.length || 0 }} 人参与</span>
                    <button
                        @click="deleteTask(task.id)"
                        class="text-red-500 hover:text-red-700 transition-colors"
                    >
                      <Icon icon="ion:trash"/>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 添加任务对话框 -->
    <n-modal v-model:show="showAddTaskDialog" title="添加任务">
      <n-card style="width: 600px"
              :bordered="false"
              size="huge"
              role="dialog"
              aria-modal="true">
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              任务名称 <span class="text-red-500">*</span>
            </label>
            <input
                v-model="taskForm.name"
                type="text"
                required
                placeholder="请输入任务名称"
                class="w-full px-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              任务描述
            </label>
            <textarea
                v-model="taskForm.description"
                rows="3"
                placeholder="请输入任务描述"
                class="w-full px-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all resize-none"
            ></textarea>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                开始时间 <span class="text-red-500">*</span>
              </label>
              <n-date-picker
                  v-model:value="taskForm.startTime"
                  type="datetime"
                  required
                  class="w-full"
                  :format="'yyyy-MM-dd HH:mm:ss'"
                  :value-format="'x'"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                结束时间 <span class="text-red-500">*</span>
              </label>
              <n-date-picker
                  v-model:value="taskForm.endTime"
                  type="datetime"
                  required
                  class="w-full"
                  :format="'yyyy-MM-dd HH:mm:ss'"
                  :value-format="'x'"
              />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
              状态
            </label>
            <n-select
                v-model:value="taskForm.status"
                :options="[
              { label: '未开始', value: '未开始' },
              { label: '进行中', value: '进行中' },
              { label: '完成', value: '完成' },
              { label: '延期', value: '延期' }
            ]"
            />
          </div>
        </div>

        <template #footer>
          <div class="flex justify-end space-x-3">
            <n-button @click="showAddTaskDialog = false">取消</n-button>
            <n-button type="primary" @click="addTask">确定</n-button>
          </div>
        </template>
      </n-card>
    </n-modal>
  </div>
</template>

<style scoped>
/* 自定义样式 */
:deep(.n-date-picker),
:deep(.n-select) {
  width: 100%;
}
</style>
<template>
  <div class="min-h-screen bg-gradient-to-br from-white to-gray-50 dark:from-gray-900 dark:to-gray-800 text-gray-900 dark:text-white transition-colors duration-300">
    <!-- 页面头部 -->
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 class="text-2xl font-semibold tracking-tight">
            {{ isEditing ? '编辑项目' : '新增项目' }}
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            {{ isEditing ? '修改现有项目信息' : '创建一个新的项目' }}
          </p>
        </div>
        
        <button
          @click="handleCancel"
          class="px-4 py-2 rounded-lg bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors flex items-center"
        >
          <Icon icon="ion:arrow-back" class="mr-1.5" />
          返回
        </button>
      </div>
      
      <!-- 表单区域 -->
      <div class="bg-white/80 dark:bg-gray-800/80 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="p-6">
          <form @submit.prevent="handleSubmit">
            <!-- 项目基本信息 -->
            <div class="space-y-6">
              <!-- 项目名称 -->
              <div>
                <label for="projectName" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                  项目名称 <span class="text-red-500">*</span>
                </label>
                <input
                  id="projectName"
                  v-model="formData.name"
                  type="text"
                  required
                  placeholder="请输入项目名称"
                  class="w-full px-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all"
                />
              </div>
              
              <!-- 项目描述 -->
              <div>
                <label for="projectDescription" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                  项目描述 <span class="text-red-500">*</span>
                </label>
                <textarea
                  id="projectDescription"
                  v-model="formData.description"
                  rows="4"
                  required
                  placeholder="请详细描述项目内容、目标和范围"
                  class="w-full px-4 py-2.5 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all resize-none"
                ></textarea>
              </div>
              
              <!-- 项目时间 -->
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <!-- 开始时间 -->
                <div>
                  <label for="startTime" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                    开始时间 <span class="text-red-500">*</span>
                  </label>
                  <n-date-picker
                    id="startTime"
                    v-model:value="formData.startTime"
                    type="datetime"
                    required
                    class="w-full"
                    :format="'yyyy-MM-dd HH:mm:ss'"
                    :value-format="'x'"
                  />
                </div>
                
                <!-- 结束时间 -->
                <div>
                  <label for="endTime" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                    结束时间 <span class="text-red-500">*</span>
                  </label>
                  <n-date-picker
                    id="endTime"
                    v-model:value="formData.endTime"
                    type="datetime"
                    required
                    class="w-full"
                    :format="'yyyy-MM-dd HH:mm:ss'"
                    :value-format="'x'"
                  />
                </div>
              </div>
              
              <!-- 所属部门 -->
              <div>
                <label for="department" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                  所属部门 <span class="text-red-500">*</span>
                </label>
                <n-select
                  id="department"
                  v-model:value="formData.department"
                  placeholder="选择项目所属部门"
                  :options="departmentOptions"
                  class="w-full"
                />
              </div>
            </div>
            
            <!-- 表单底部操作按钮 -->
            <div class="flex flex-col sm:flex-row sm:justify-end gap-3 mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
              <button
                type="button"
                @click="handleCancel"
                class="px-4 py-2.5 rounded-lg bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-200 transition-colors flex-1 sm:flex-none flex items-center justify-center"
              >
                <Icon icon="ion:close" class="mr-1.5" />
                取消
              </button>
              <button
                type="submit"
                :disabled="showLoadingModal || !isFormValid"
                class="px-6 py-2.5 rounded-lg bg-blue-500 hover:bg-blue-600 text-white transition-colors flex-1 sm:flex-none flex items-center justify-center"
                :class="{ 'opacity-70 cursor-not-allowed': showLoadingModal || !isFormValid }"
              >
                <n-spin v-if="showLoadingModal" class="mr-2" />
                <Icon v-else icon="ion:checkmark" class="mr-1.5" />
                {{ isEditing ? '更新项目' : '创建项目' }}
              </button>
            </div>
          </form>
        </div>
      </div>
      
      <!-- 项目统计与信息展示 -->
      <div v-if="isEditing" class="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6">
        <!-- 项目成员 -->
        <div class="bg-white/80 dark:bg-gray-800/80 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
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
              <Icon icon="ion:people-off" class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" />
              <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">暂无项目成员</p>
            </div>
            <div v-else class="space-y-3">
              <div
                v-for="member in projectMembers"
                :key="member.userId"
                class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3 flex items-center justify-between"
              >
                <div class="flex items-center">
                  <div class="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-300 font-medium mr-3">
                    {{ getInitials(member.name) }}
                  </div>
                  <div>
                    <p class="font-medium">{{ member.name }}</p>
                    <p class="text-xs text-gray-500 dark:text-gray-400">{{ member.department }} - {{ member.identity }}</p>
                  </div>
                </div>
                <button
                  @click="removeMember(member.userId)"
                  class="text-gray-400 hover:text-red-500 transition-colors"
                >
                  <Icon icon="ion:close-circle" />
                </button>
              </div>
            </div>
            <button
              v-if="!loadingMembers"
              @click="addMembers"
              class="mt-4 w-full px-4 py-2 rounded-lg bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-200 transition-colors flex items-center justify-center"
            >
              <Icon icon="ion:add-circle" class="mr-1.5" />
              添加成员
            </button>
          </div>
        </div>
        
        <!-- 项目任务 -->
        <div class="bg-white/80 dark:bg-gray-800/80 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <h2 class="text-lg font-medium">项目任务</h2>
            <p class="text-sm text-gray-500 dark:text-gray-400">该项目下的任务列表</p>
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
              <Icon icon="ion:document-text-off" class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" />
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
                  <span>{{ task.users?.length || 0 }} 人参与</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- 加载状态遮罩 -->
      <n-modal v-model:show="showLoadingModal" preset="card" title="" :closable="false" :mask-closable="false">
        <div class="flex justify-center items-center py-12">
          <n-spin size="large" />
          <div class="ml-4">
            <div class="text-lg font-medium">{{ isEditing ? '更新项目中...' : '创建项目中...' }}</div>
            <div class="text-gray-500 dark:text-gray-400 mt-2">请稍候，正在处理您的请求</div>
          </div>
        </div>
      </n-modal>
    
    <!-- 添加成员对话框 -->
    <n-dialog
      v-model:show="showAddMemberDialog"
      title="添加项目成员"
      :content-style="{maxHeight: '60vh', overflow: 'auto'}"
    >
      <div class="space-y-4">
        <div class="relative">
          <input
            v-model="memberSearchQuery"
            type="text"
            placeholder="搜索成员名称或部门..."
            class="w-full pl-10 pr-4 py-2 rounded-lg bg-gray-100 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
          />
          <Icon icon="ion:search" class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" width="20" height="20" />
        </div>
        <div class="grid grid-cols-1 gap-2 max-h-[40vh] overflow-y-auto">
          <div
            v-for="member in filteredAvailableMembers"
            :key="member.userId"
            class="flex items-center justify-between bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            :class="{ 'ring-2 ring-blue-500': selectedMembers.includes(member.userId) }"
            @click="toggleMemberSelection(member.userId)"
          >
            <div class="flex items-center">
              <div class="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-300 font-medium mr-3">
                {{ getInitials(member.name) }}
              </div>
              <div>
                <p class="font-medium">{{ member.name }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ member.department }} - {{ member.identity }}</p>
              </div>
            </div>
            <n-checkbox
              :checked="selectedMembers.includes(member.userId)"
              @update:checked="(checked) => handleCheckboxChange(checked, member.userId)"
            />
          </div>
          <div v-if="filteredAvailableMembers.length === 0" class="text-center py-4 text-gray-500 dark:text-gray-400">
            未找到匹配的成员
          </div>
        </div>
      </div>
      <template #action>
        <div class="flex justify-between w-full">
          <n-button @click="showAddMemberDialog = false">取消</n-button>
          <n-button type="primary" @click="confirmAddMembers">确定</n-button>
        </div>
      </template>
    </n-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useMessage } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { NDatePicker, NSelect, NModal, NDialog, NButton, NCheckbox, NSpin } from 'naive-ui'
import { ProjectService } from '../services/ProjectService'
import type { ProjectModel, StaffModel, TaskModel } from '../models/ProjectModel'

const router = useRouter()
const route = useRoute()
const message = useMessage()

// 获取路由参数，判断是新增还是编辑
const projectId = computed(() => route.params.id as string | undefined)
const isEditing = computed(() => !!projectId.value)

// 表单数据
const formData = ref<ProjectModel>({
  id: '',
  name: '',
  description: '',
  startTime: Date.now(),
  endTime: Date.now(),
  department: '',
  staffs: [],
  tasks: []
})

// 加载状态
const showLoadingModal = ref(false)
const loadingMembers = ref(false)
const loadingTasks = ref(false)

// 部门选项
const departmentOptions = ref<{label: string; value: string}[]>([])

// 项目成员和任务
const projectMembers = ref<StaffModel[]>([])
const projectTasks = ref<TaskModel[]>([])

// 添加成员对话框相关
const showAddMemberDialog = ref(false)
const availableMembers = ref<StaffModel[]>([])
const selectedMembers = ref<string[]>([])
const memberSearchQuery = ref('')

// 计算属性：表单验证
const isFormValid = computed(() => {
  return formData.value.name.trim() !== '' &&
         formData.value.description.trim() !== '' &&
         formData.value.department !== '' &&
         !!formData.value.startTime &&
         !!formData.value.endTime &&
         Number(formData.value.endTime) > Number(formData.value.startTime)
})

// 计算属性：过滤可用成员
const filteredAvailableMembers = computed(() => {
  const query = memberSearchQuery.value.toLowerCase().trim()
  // 过滤掉已在项目中的成员
  const currentMemberIds = projectMembers.value.map(m => m.userId)
  
  if (!query) {
    return availableMembers.value.filter(m => !currentMemberIds.includes(m.userId))
  }
  
  return availableMembers.value
    .filter(m => !currentMemberIds.includes(m.userId))
    .filter(m => 
      m.name.toLowerCase().includes(query) || 
      m.department.toLowerCase().includes(query) ||
      m.identity.toLowerCase().includes(query)
    )
})

// 初始化数据
onMounted(async () => {
  await loadDepartmentOptions()
  
  if (isEditing.value) {
    await loadProjectData()
    await loadProjectMembers()
    await loadProjectTasks()
    await loadAvailableMembers()
  }
})

// 加载部门选项
const loadDepartmentOptions = async () => {
  try {
    // 这里简化处理，实际应该从DepartmentService获取
    departmentOptions.value = [
      { label: 'iOS开发部', value: 'iOS开发部' },
      { label: 'UI/UX设计部', value: 'UI/UX设计部' },
      { label: '产品策划部', value: '产品策划部' },
      { label: '运营部', value: '运营部' },
      { label: '技术支持部', value: '技术支持部' }
    ]
  } catch (error) {
    console.error('加载部门选项失败:', error)
    message.error('加载部门选项失败')
  }
}

// 加载项目数据
const loadProjectData = async () => {
  if (!projectId.value) return
  
  try {
    // 从ProjectService获取项目详情
    const projects = await ProjectService.getAllProjects()
    const project = projects.find(p => p.id === projectId.value)
    if (project) {
      // 确保时间字段是时间戳格式
      const formattedProject = {
        ...project,
        startTime: project.startTime instanceof Date ? project.startTime.getTime() : (typeof project.startTime === 'number' ? project.startTime : Date.now()),
        endTime: project.endTime instanceof Date ? project.endTime.getTime() : (typeof project.endTime === 'number' ? project.endTime : Date.now())
      }
      formData.value = formattedProject
    } else {
      message.error('未找到项目数据')
      router.push('/Centre/Admin')
    }
  } catch (error: any) {
    console.error('加载项目数据失败:', error)
    message.error(error.message || '加载项目数据失败')
  }
}

// 加载项目成员
const loadProjectMembers = async () => {
  if (!projectId.value) return
  
  try {
    loadingMembers.value = true
    // 从项目数据中获取成员信息
    const projects = await ProjectService.getAllProjects()
    const project = projects.find(p => p.id === projectId.value)
    if (project && project.staffs) {
      projectMembers.value = project.staffs
    } else {
      projectMembers.value = []
    }
  } catch (error: any) {
    console.error('加载项目成员失败:', error)
    message.error(error.message || '加载项目成员失败')
  } finally {
    loadingMembers.value = false
  }
}

// 加载项目任务
const loadProjectTasks = async () => {
  if (!projectId.value) return
  
  try {
    loadingTasks.value = true
    // 从项目数据中获取任务信息
    const projects = await ProjectService.getAllProjects()
    const project = projects.find(p => p.id === projectId.value)
    if (project && project.tasks) {
      projectTasks.value = project.tasks
    } else {
      projectTasks.value = []
    }
  } catch (error: any) {
    console.error('加载项目任务失败:', error)
    message.error(error.message || '加载项目任务失败')
  } finally {
    loadingTasks.value = false
  }
}

// 加载可用成员
const loadAvailableMembers = async () => {
  try {
    // 从所有项目中获取所有可能的成员（实际项目中应该有专门的API获取所有成员）
    const allProjects = await ProjectService.getAllProjects()
    const allStaffs = new Map<string, StaffModel>()
    
    // 收集所有项目中的成员
    allProjects.forEach(project => {
      if (project.staffs) {
        project.staffs.forEach(staff => {
          allStaffs.set(staff.userId, staff)
        })
      }
    })
    
    // 转换为数组
    availableMembers.value = Array.from(allStaffs.values())
  } catch (error: any) {
    console.error('加载可用成员失败:', error)
    message.error(error.message || '加载可用成员失败')
  }
}

// 提交表单
const handleSubmit = async () => {
  if (!isFormValid.value) return
  
  try {
    showLoadingModal.value = true
    
    // 准备提交数据，确保时间字段格式正确
    const submitData: ProjectModel = {
      ...formData.value,
      id: isEditing.value ? formData.value.id : generateId(),
      staffs: projectMembers.value,
      tasks: projectTasks.value,
      // 确保时间戳格式
      startTime: Number(formData.value.startTime),
      endTime: Number(formData.value.endTime)
    }
    
    // 调用服务保存数据
    await ProjectService.createOrUpdateProject(submitData)
    
    message.success(isEditing.value ? '项目更新成功' : '项目创建成功')
    router.push('/Centre/Admin')
  } catch (error: any) {
    console.error('保存项目失败:', error)
    message.error(error.message || '保存项目失败')
  } finally {
    showLoadingModal.value = false
  }
}

// 取消操作
const handleCancel = () => {
  router.push('/Centre/Admin')
}

// 生成唯一ID (简化版)
const generateId = () => {
  return Date.now().toString(36) + Math.random().toString(36).substr(2)
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

// 添加成员对话框
const addMembers = () => {
  selectedMembers.value = []
  showAddMemberDialog.value = true
}

// 切换成员选择
const toggleMemberSelection = (userId: string) => {
  const index = selectedMembers.value.indexOf(userId)
  if (index > -1) {
    selectedMembers.value.splice(index, 1)
  } else {
    selectedMembers.value.push(userId)
  }
}

// 处理复选框变化
const handleCheckboxChange = (checked: boolean, userId: string) => {
  if (checked && !selectedMembers.value.includes(userId)) {
    selectedMembers.value.push(userId)
  } else if (!checked) {
    selectedMembers.value = selectedMembers.value.filter(id => id !== userId)
  }
}

// 确认添加成员
const confirmAddMembers = async () => {
  if (!projectId.value) {
    message.warning('请先保存项目，再添加成员')
    return
  }
  
  try {
    // 添加选中的成员到项目中
    const membersToAdd = availableMembers.value.filter(m => selectedMembers.value.includes(m.userId))
    
    // 调用ProjectService.changeMember方法更新后端数据
    for (const member of membersToAdd) {
      await ProjectService.changeMember(member.userId, projectId.value)
    }
    
    projectMembers.value = [...projectMembers.value, ...membersToAdd]
    message.success(`成功添加 ${membersToAdd.length} 名成员`)
    showAddMemberDialog.value = false
  } catch (error: any) {
    console.error('添加成员失败:', error)
    message.error(error.message || '添加成员失败')
  }
}

// 移除成员
const removeMember = async (userId: string) => {
  if (!projectId.value) {
    message.warning('请先保存项目，再移除成员')
    return
  }
  
  try {
    // 调用ProjectService.changeMember方法更新后端数据
    await ProjectService.changeMember(userId, projectId.value)
    
    projectMembers.value = projectMembers.value.filter(m => m.userId !== userId)
    message.success('成员已移除')
  } catch (error: any) {
    console.error('移除成员失败:', error)
    message.error(error.message || '移除成员失败')
  }
}
</script>

<style scoped>
/* 自定义样式 */
:deep(.n-date-picker),
:deep(.n-select) {
  width: 100%;
}

:deep(.n-dialog-content) {
  max-height: 60vh;
  overflow: auto;
}

/* 动画效果 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
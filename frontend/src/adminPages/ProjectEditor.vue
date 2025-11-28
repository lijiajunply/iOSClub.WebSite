<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <!-- 页面头部 -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">


      <!-- 返回按钮 -->
      <div class="mb-6">
        <button
            @click="handleCancel"
            class="px-4 py-2.5 rounded-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors flex items-center text-gray-700 dark:text-gray-200"
        >
          <Icon icon="ion:arrow-back" class="mr-2"/>
          返回
        </button>
      </div>

      <!-- 表单区域 -->
      <div
          class="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="p-6">
          <form @submit.prevent="handleSubmit">
            <!-- 项目基本信息 -->
            <div class="space-y-6">
              <!-- 项目名称 -->
              <div>
                <label for="projectName" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                  项目名称 <span class="text-red-500">*</span>
                </label>
                <n-input
                    id="projectName"
                    v-model:value="formData.name"
                    type="text"
                    required
                    placeholder="请输入项目名称"
                    class="w-full"
                />
              </div>

              <!-- 项目描述 -->
              <div>
                <label for="projectDescription"
                       class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
                  项目描述 <span class="text-red-500">*</span>
                </label>
                <n-input
                    id="projectDescription"
                    v-model:value="formData.description"
                    type="textarea"
                    rows="4"
                    required
                    placeholder="请详细描述项目内容、目标和范围"
                    class="w-full"
                />
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
                      v-model:value="formData.startTime as any"
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
                      v-model:value="formData.endTime as any"
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
            <div
                class="flex flex-col sm:flex-row sm:justify-end gap-3 mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
              <button
                  @click="handleCancel"
                  class="px-4 py-2.5 rounded-full bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-200 transition-colors flex-1 sm:flex-none flex items-center justify-center"
              >
                <Icon icon="ion:close" class="mr-2"/>
                取消
              </button>
              <button
                  :disabled="showLoadingModal || !isFormValid"
                  class="px-6 py-2.5 rounded-full bg-blue-500 hover:bg-blue-600 text-white transition-colors flex-1 sm:flex-none flex items-center justify-center"
                  :class="{ 'opacity-70 cursor-not-allowed': showLoadingModal || !isFormValid }"
                  type="submit"
              >
                <div v-if="showLoadingModal" class="mr-2 h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                <Icon v-if="!showLoadingModal" icon="ion:checkmark" class="mr-2"/>
                {{ isEditing ? '更新项目' : '创建项目' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </main>

    <!-- 加载状态遮罩 -->
    <n-modal v-model:show="showLoadingModal" preset="dialog" title="" :closable="false" :mask-closable="false"
             class="w-full max-w-md">
      <div class="flex justify-center items-center py-12">
        <n-spin size="large"/>
        <div class="ml-4">
          <div class="text-lg font-medium">{{ isEditing ? '更新项目中...' : '创建项目中...' }}</div>
          <div class="text-gray-500 dark:text-gray-400 mt-2">请稍候，正在处理您的请求</div>
        </div>
      </div>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted, onBeforeUnmount} from 'vue'
import {useRouter, useRoute} from 'vue-router'
import {useMessage} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {NDatePicker, NSelect, NModal} from 'naive-ui'
import {ProjectService} from '../services/ProjectService'
import type {ProjectModel} from '../models'
import {useLayoutStore} from '../stores/LayoutStore'

const router = useRouter()
const route = useRoute()
const message = useMessage()
const layoutStore = useLayoutStore()

// 获取路由参数，判断是新增还是编辑
const projectId = computed(() => route.params.id as string | undefined)
const isEditing = computed(() => !!projectId.value && projectId.value !== 'new')

// 表单数据
const formData = ref<Omit<ProjectModel, 'staffs' | 'tasks'>>({
  id: '',
  name: '',
  description: '',
  startTime: Date.now(),
  endTime: Date.now(),
  department: ''
})

// 加载状态
const showLoadingModal = ref(false)

// 部门选项
const departmentOptions = ref<{ label: string; value: string }[]>([])

// 计算属性：表单验证
const isFormValid = computed(() => {
  return formData.value.name.trim() !== '' &&
      formData.value.description.trim() !== '' &&
      formData.value.department !== '' &&
      !!formData.value.startTime &&
      !!formData.value.endTime &&
      Number(formData.value.endTime) > Number(formData.value.startTime)
})

// 初始化数据
onMounted(async () => {
  await loadDepartmentOptions()

  if (isEditing.value) {
    await loadProjectData()
  }

  // Set page header
  layoutStore.setPageHeader(
      isEditing.value ? '编辑项目' : '新增项目',
      isEditing.value ? '修改现有项目信息' : '创建一个新的项目'
  )

  // Show page actions (none for this page)
  layoutStore.setShowPageActions(false)
})

onBeforeUnmount(() => {
  // Clear page header
  layoutStore.clearPageHeader()
})

// 加载部门选项
const loadDepartmentOptions = async () => {
  try {
    // 这里简化处理，实际应该从DepartmentService获取
    departmentOptions.value = [
      {label: 'iOS开发部', value: 'iOS开发部'},
      {label: 'UI/UX设计部', value: 'UI/UX设计部'},
      {label: '产品策划部', value: '产品策划部'},
      {label: '运营部', value: '运营部'},
      {label: '技术支持部', value: '技术支持部'}
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
        startTime: project.startTime instanceof Date ? project.startTime.getTime() : (project.startTime),
        endTime: project.endTime instanceof Date ? project.endTime.getTime() : (project.endTime)
      }
      // 移除staffs和tasks，因为这些在ProjectData.vue中处理
      delete (formattedProject as any).staffs
      delete (formattedProject as any).tasks
      formData.value = formattedProject
    } else {
      message.error('未找到项目数据')
      await router.push('/Centre/Admin')
    }
  } catch (error: any) {
    console.error('加载项目数据失败:', error)
    message.error(error.message || '加载项目数据失败')
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
      // 初始化空的staffs和tasks数组
      staffs: [],
      tasks: []
    } as ProjectModel

    // 调用服务保存数据
    await ProjectService.createOrUpdateProject(submitData)

    message.success(isEditing.value ? '项目更新成功' : '项目创建成功')
    await router.push('/Centre/Admin')
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
</script>

<style scoped>
/* 自定义样式 */
:deep(.n-date-picker),
:deep(.n-select) {
  width: 100%;
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
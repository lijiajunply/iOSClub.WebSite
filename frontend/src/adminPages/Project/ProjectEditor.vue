<template>
  <!-- 页面容器 -->
  <div class="page-wrapper min-h-screen transition-colors duration-300">

    <!-- 主要内容区域 -->
    <main class="max-w-3xl mx-auto px-4 sm:px-6 py-10">

      <!-- 居中的表单容器 -->
      <div class="ios-card rounded-3xl overflow-hidden">
        <div class="p-8 sm:p-10">

          <div class="mb-8 text-center sm:text-left">
            <h2 class="text-2xl font-bold tracking-tight title-text">项目详情</h2>
            <p class="mt-2 text-sm subtitle-text">
              请填写下方的项目核心信息，带 <span class="text-red-500">*</span> 为必填项。
            </p>
          </div>

          <form @submit.prevent="handleSubmit" class="space-y-8">

            <!-- 第一组：基本信息 -->
            <div class="form-section">
              <div class="space-y-6">
                <!-- 项目名称 -->
                <div class="form-group">
                  <label for="projectName" class="form-label block text-sm font-medium mb-2 ml-1">
                    项目名称
                  </label>
                  <n-input
                      id="projectName"
                      v-model:value="formData.name"
                      type="text"
                      placeholder="例如：iOS 18系统适配"
                      class="ios-input"
                      size="large"
                  />
                </div>

                <!-- 项目描述 -->
                <div class="form-group">
                  <label for="projectDescription" class="form-label block text-sm font-medium mb-2 ml-1">
                    项目描述
                  </label>
                  <n-input
                      id="projectDescription"
                      v-model:value="formData.description"
                      type="textarea"
                      :autosize="{ minRows: 3, maxRows: 6 }"
                      placeholder="描述项目的目标、范围及预期成果..."
                      class="ios-input"
                  />
                </div>
              </div>
            </div>

            <div class="w-full h-px divider-line my-2"></div>

            <!-- 第二组：元数据 (两列布局) -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
              <!-- 所属部门 -->
              <div class="form-group sm:col-span-2">
                <label for="department" class="form-label block text-sm font-medium mb-2 ml-1">
                  所属部门
                </label>
                <n-select
                    id="department"
                    v-model:value="formData.department"
                    placeholder="选择部门"
                    :options="departmentOptions"
                    class="ios-select"
                    size="large"
                />
              </div>

              <!-- 开始时间 -->
              <div class="form-group">
                <label for="startTime" class="form-label block text-sm font-medium mb-2 ml-1">
                  开始时间
                </label>
                <n-date-picker
                    id="startTime"
                    v-model:value="formData.startTime as any"
                    type="datetime"
                    class="ios-date-picker w-full"
                    size="large"
                    :actions="['now']"
                />
              </div>

              <!-- 结束时间 -->
              <div class="form-group">
                <label for="endTime" class="form-label block text-sm font-medium mb-2 ml-1">
                  结束时间
                </label>
                <n-date-picker
                    id="endTime"
                    v-model:value="formData.endTime as any"
                    type="datetime"
                    class="ios-date-picker w-full"
                    size="large"
                />
              </div>
            </div>

            <!-- 底部按钮区域 -->
            <div class="pt-8 flex flex-col sm:flex-row items-center justify-end gap-4">
              <button
                  type="button"
                  @click="handleCancel"
                  class="cancel-btn w-full sm:w-auto px-8 py-3 rounded-full font-medium transition-all"
              >
                取消
              </button>
              <button
                  type="submit"
                  :disabled="showLoadingModal || !isFormValid"
                  class="submit-btn w-full sm:w-auto px-8 py-3 rounded-full text-white font-medium shadow-lg flex items-center justify-center transition-all hover:scale-105 active:scale-95"
                  :class="{ 'opacity-50 cursor-not-allowed': showLoadingModal || !isFormValid }"
              >
                <Icon v-if="showLoadingModal" icon="eos-icons:loading" class="mr-2 animate-spin"/>
                <Icon v-else icon="ion:cloud-upload-outline" class="mr-2"/>
                {{ isEditing ? '保存更改' : '创建项目' }}
              </button>
            </div>

          </form>
        </div>
      </div>
    </main>

    <!-- Loading Modal (自定义样式) -->
    <n-modal v-model:show="showLoadingModal" :mask-closable="false">
      <div class="loading-card p-6 rounded-2xl flex flex-col items-center justify-center shadow-2xl">
        <n-spin size="large" stroke="#007AFF"/>
        <div class="mt-4 text-base font-medium title-text">
          {{ isEditing ? '正在同步...' : '正在创建...' }}
        </div>
      </div>
    </n-modal>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useMessage, NInput, NDatePicker, NSelect, NModal, NSpin } from 'naive-ui'
import { Icon } from '@iconify/vue'
// 假设这些引入路径是正确的，基于你提供的代码
import { ProjectService } from '../../services/ProjectService'
import type { ProjectModel } from '../../models'
import { useLayoutStore } from '../../stores/LayoutStore'

const router = useRouter()
const route = useRoute()
const message = useMessage()
const layoutStore = useLayoutStore()

// 路由参数
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

const showLoadingModal = ref(false)
const departmentOptions = ref<{ label: string; value: string }[]>([])

// 表单验证
const isFormValid = computed(() => {
  return formData.value.name.trim() !== '' &&
      formData.value.description.trim() !== '' &&
      formData.value.department !== '' &&
      !!formData.value.startTime &&
      !!formData.value.endTime &&
      Number(formData.value.endTime) > Number(formData.value.startTime)
})

// Lifecycle
onMounted(async () => {
  await loadDepartmentOptions()

  // 隐藏传统的 Layout Header 动作，因为使用了自定义 Header
  layoutStore.setShowPageActions(false)

  // 如果是全屏沉浸式设计，甚至可以考虑隐藏 Layout 的 Header
  // layoutStore.setPageHeader(...) // 这里不做设置，或者设置为空，视全局 Layout 逻辑而定

  if (isEditing.value) {
    await loadProjectData()
  }
})

onBeforeUnmount(() => {
  layoutStore.clearPageHeader()
})

// 逻辑方法
const loadDepartmentOptions = async () => {
  try {
    // 模拟数据
    departmentOptions.value = [
      {label: 'iOS 开发部', value: 'iOS开发部'},
      {label: 'UI/UX 设计中心', value: 'UI/UX设计部'},
      {label: '产品策划部', value: '产品策划部'},
      {label: '市场运营部', value: '运营部'},
      {label: '平台架构部', value: '技术支持部'}
    ]
  } catch (error) {
    console.error(error)
    message.error('加载部门数据失败')
  }
}

const loadProjectData = async () => {
  if (!projectId.value) return

  try {
    const projects = await ProjectService.getAllProjects()
    const project = projects.find(p => p.id === projectId.value)
    if (project) {
      // 格式化时间戳
      const formattedProject = {
        ...project,
        startTime: project.startTime instanceof Date ? project.startTime.getTime() : (project.startTime),
        endTime: project.endTime instanceof Date ? project.endTime.getTime() : (project.endTime)
      }
      // 移除复杂对象，避免表单干扰
      delete (formattedProject as any).staffs
      delete (formattedProject as any).tasks
      formData.value = formattedProject
    } else {
      message.error('未找到项目')
      await router.push('/Centre/Admin')
    }
  } catch (error: any) {
    message.error('加载失败: ' + error.message)
  }
}

const handleSubmit = async () => {
  if (!isFormValid.value) return

  try {
    showLoadingModal.value = true

    const submitData: ProjectModel = {
      ...formData.value,
      id: isEditing.value ? formData.value.id : generateId(),
      staffs: [],
      tasks: []
    } as ProjectModel

    await ProjectService.createOrUpdateProject(submitData)
    message.success(isEditing.value ? '已保存更改' : '项目创建成功')

    // 延迟一点跳转，让用户看到成功状态
    setTimeout(() => {
      router.push('/Centre/Admin')
    }, 500)
  } catch (error: any) {
    message.error(error.message || '操作失败')
  } finally {
    showLoadingModal.value = false
  }
}

const handleCancel = () => {
  router.push('/Centre/Admin')
}

const generateId = () => {
  return Date.now().toString(36) + Math.random().toString(36).substr(2)
}
</script>

<style scoped>
/* --- Apple Style System Colors & Variables --- */
.page-wrapper {
  background-color: #F5F5F7; /* Apple Light Gray Background */
  color: #1D1D1F;
  font-family: -apple-system, BlinkMacSystemFont, "SF Pro Text", "Helvetica Neue", sans-serif;
}

.nav-header {
  background-color: rgba(255, 255, 255, 0.72);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-bottom-color: rgba(0, 0, 0, 0.1); /* Subtle border */
}

.nav-back-btn {
  color: #007AFF; /* iOS Blue */
}

.ios-card {
  background-color: #FFFFFF;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.04); /* Very subtle, diffused shadow */
  border: 1px solid rgba(0, 0, 0, 0.02);
}

.title-text {
  color: #1D1D1F;
}

.subtitle-text {
  color: #86868B;
}

.form-label {
  color: #86868B;
}

.divider-line {
  background-color: #E5E5E5;
}

/* Buttons */
.submit-btn {
  background-color: #007AFF; /* iOS Blue */
  /* Apple gradient nuance often seen in marketing */
  background-image: linear-gradient(to bottom right, #007AFF, #0062CC);
}

.submit-btn:disabled {
  background-color: #A1A1A6;
  background-image: none;
}

.cancel-btn {
  background-color: #F5F5F7;
  color: #1D1D1F;
}

.cancel-btn:hover {
  background-color: #E8E8ED;
}

/* Naive UI Overrides for iOS look */
/* 让 Naive UI 的 Input 更像 iOS */
:deep(.n-input), :deep(.n-base-selection) {
  border-radius: 10px !important;
  background-color: #F5F5F7 !important; /* Input 本身也是浅灰背景 */
  border: 1px solid transparent !important;
  transition: all 0.2s ease;
}

:deep(.n-input:hover), :deep(.n-base-selection:hover) {
  background-color: #E8E8ED !important;
}

:deep(.n-input--focus), :deep(.n-base-selection--focus) {
  background-color: #FFFFFF !important;
  border: 1px solid #007AFF !important;
  box-shadow: 0 0 0 3px rgba(0, 122, 255, 0.1) !important;
}

.loading-card {
  background-color: #FFFFFF;
}

/* --- Dark Mode Overrides --- */
.dark .page-wrapper {
  background-color: #000000; /* True Black for OLED feel */
  color: #F5F5F7;
}

.dark .nav-header {
  background-color: rgba(28, 28, 30, 0.72);
  border-bottom-color: rgba(255, 255, 255, 0.1);
}

.dark .ios-card {
  background-color: #1C1C1E; /* iOS Dark Gray Card */
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.4);
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.dark .title-text {
  color: #FFFFFF;
}

.dark .subtitle-text {
  color: #A1A1A6;
}

.dark .form-label {
  color: #98989D;
}

.dark .divider-line {
  background-color: #38383A;
}

.dark .cancel-btn {
  background-color: #2C2C2E;
  color: #FFFFFF;
}

.dark .cancel-btn:hover {
  background-color: #3A3A3C;
}

.dark .loading-card {
  background-color: #1C1C1E;
}

/* Dark Mode Inputs */
.dark :deep(.n-input), .dark :deep(.n-base-selection) {
  background-color: #2C2C2E !important;
  color: white !important;
}

.dark :deep(.n-input:hover), .dark :deep(.n-base-selection:hover) {
  background-color: #3A3A3C !important;
}

.dark :deep(.n-input--focus), .dark :deep(.n-base-selection--focus) {
  background-color: #1C1C1E !important;
  border: 1px solid #0A84FF !important; /* iOS Dark Mode Blue */
  box-shadow: 0 0 0 3px rgba(10, 132, 255, 0.2) !important;
}

.dark :deep(.n-input__placeholder) {
  color: #636366 !important;
}
</style>
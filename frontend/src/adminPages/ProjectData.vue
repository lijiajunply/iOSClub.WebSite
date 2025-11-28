<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useMessage } from 'naive-ui'
import { NDatePicker, NSelect, NModal, NInput, NButton } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { ProjectService } from '../services/ProjectService'
import type { ProjectModel, StaffModel, TaskModel } from '../models'
import { useLayoutStore } from '../stores/LayoutStore'

const router = useRouter()
const route = useRoute()
const message = useMessage()
const layoutStore = useLayoutStore()

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

  // 设置 Layout Store (保持原有逻辑)
  layoutStore.setPageHeader('项目详情', '查看和管理项目信息')
  layoutStore.setShowPageActions(false)
})

onBeforeUnmount(() => {
  layoutStore.clearPageHeader()
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
  return `${d.getFullYear()}/${String(d.getMonth() + 1).padStart(2, '0')}/${String(d.getDate()).padStart(2, '0')}`
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
    message.error(error.message || '添加任务失败')
  }
}

// 删除任务
const deleteTask = async (taskId: string) => {
  try {
    projectTasks.value = projectTasks.value.filter(task => task.id !== taskId)
    message.success('任务已移除')
  } catch (error: any) {
    message.error(error.message || '删除任务失败')
  }
}
</script>

<template>
  <div class="page-container min-h-screen transition-colors duration-500">
    <main class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8 sm:py-12">

      <!-- 顶部导航栏 -->
      <nav class="flex items-center justify-between mb-8 sm:mb-12 fade-in">
        <button
            @click="router.push('/Centre/Admin')"
            class="nav-button group flex items-center gap-2 px-4 py-2 rounded-full transition-all active:scale-95"
        >
          <Icon icon="ion:chevron-back" class="w-5 h-5 text-gray-500 group-hover:text-gray-900 dark:text-gray-400 dark:group-hover:text-white transition-colors" />
          <span class="font-medium text-gray-600 group-hover:text-gray-900 dark:text-gray-300 dark:group-hover:text-white transition-colors">返回列表</span>
        </button>

        <div class="hidden sm:flex gap-3">
          <button class="action-icon-btn" title="分享">
            <Icon icon="ion:share-outline" class="w-5 h-5" />
          </button>
          <button class="action-icon-btn" title="更多">
            <Icon icon="ion:ellipsis-horizontal" class="w-5 h-5" />
          </button>
        </div>
      </nav>

      <!-- 加载态骨架屏 -->
      <div v-if="loading" class="animate-pulse space-y-8">
        <div class="h-48 rounded-3xl bg-gray-200 dark:bg-gray-800 w-full"></div>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="h-64 rounded-3xl bg-gray-200 dark:bg-gray-800"></div>
          <div class="h-64 rounded-3xl bg-gray-200 dark:bg-gray-800"></div>
        </div>
      </div>

      <!-- 主要内容区 -->
      <div v-else-if="project" class="space-y-8">

        <!-- 项目概览卡片 (Hero Card) -->
        <section class="apple-card hero-card p-8 sm:p-10 relative overflow-hidden fade-in-up">
          <div class="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
            <div>
              <div class="flex items-center gap-3 mb-3">
                 <span class="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-600 dark:bg-blue-500/20 dark:text-blue-300">
                   {{ project.department }}
                 </span>
                <span class="text-sm text-gray-500 dark:text-gray-400 font-medium tracking-wide">ID: {{ project.id }}</span>
              </div>
              <h1 class="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white tracking-tight mb-3">
                {{ project.name }}
              </h1>
              <p class="text-lg text-gray-600 dark:text-gray-300 max-w-2xl leading-relaxed">
                {{ project.description }}
              </p>

              <div class="mt-6 flex flex-wrap items-center gap-6 text-sm font-medium text-gray-500 dark:text-gray-400">
                <div class="flex items-center gap-2">
                  <Icon icon="ion:calendar-outline" class="w-5 h-5" />
                  <span>{{ formatDate(project.startTime) }}</span>
                  <span class="px-1">→</span>
                  <span>{{ formatDate(project.endTime) }}</span>
                </div>
              </div>
            </div>

            <button
                @click="router.push(`/Centre/ProjectEditor/${project.id}`)"
                class="primary-btn shrink-0 flex items-center gap-2 px-6 py-3 rounded-full font-semibold transition-all active:scale-95"
            >
              <Icon icon="ion:create-outline" class="w-5 h-5" />
              <span>编辑项目</span>
            </button>
          </div>

          <!-- 装饰性背景圆 -->
          <div class="absolute -right-20 -top-20 w-64 h-64 bg-blue-500/10 dark:bg-blue-500/5 rounded-full blur-3xl pointer-events-none"></div>
        </section>

        <!-- 左右分栏布局 -->
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">

          <!-- 左侧：项目成员 (占 5/12) -->
          <div class="lg:col-span-5 flex flex-col gap-6">
            <section class="apple-card p-6 h-full fade-in-up" style="animation-delay: 0.1s;">
              <div class="flex items-center justify-between mb-6">
                <h2 class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
                  <Icon icon="ion:people" class="text-blue-500" />
                  团队成员
                </h2>
                <span class="text-xs font-medium px-2.5 py-1 bg-gray-100 dark:bg-gray-700/50 text-gray-500 dark:text-gray-400 rounded-md">
                    {{ projectMembers.length }}人
                  </span>
              </div>

              <div v-if="loadingMembers" class="space-y-4">
                <div v-for="i in 3" :key="i" class="h-12 bg-gray-100 dark:bg-gray-700 rounded-xl w-full animate-pulse"></div>
              </div>

              <div v-else-if="projectMembers.length === 0" class="py-10 text-center">
                <div class="w-16 h-16 mx-auto bg-gray-50 dark:bg-gray-800 rounded-full flex items-center justify-center mb-3">
                  <Icon icon="ion:person-add-outline" class="w-8 h-8 text-gray-300 dark:text-gray-600" />
                </div>
                <p class="text-gray-400 dark:text-gray-500 text-sm">暂无成员</p>
              </div>

              <div v-else class="space-y-3 max-h-[500px] overflow-y-auto custom-scrollbar pr-2">
                <div
                    v-for="member in projectMembers"
                    :key="member.userId"
                    class="group flex items-center p-3 rounded-2xl hover:bg-gray-50 dark:hover:bg-white/5 transition-colors cursor-default"
                >
                  <div class="w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 text-white flex items-center justify-center font-bold text-sm shadow-md mr-4 shrink-0">
                    {{ getInitials(member.name) }}
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-semibold text-gray-900 dark:text-gray-100 truncate">{{ member.name }}</p>
                    <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ member.department }} · {{ member.identity }}</p>
                  </div>
                  <button class="opacity-0 group-hover:opacity-100 p-2 text-gray-400 hover:text-blue-500 transition-all">
                    <Icon icon="ion:chatbubble-ellipses-outline" class="w-5 h-5" />
                  </button>
                </div>
              </div>
            </section>
          </div>

          <!-- 右侧：任务列表 (占 7/12) -->
          <div class="lg:col-span-7 flex flex-col gap-6">
            <section class="apple-card p-6 h-full fade-in-up" style="animation-delay: 0.2s;">
              <div class="flex items-center justify-between mb-6">
                <h2 class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
                  <Icon icon="ion:checkmark-circle" class="text-green-500" />
                  任务进度
                </h2>
                <button
                    @click="openAddTaskDialog"
                    class="text-sm font-medium text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 px-3 py-1.5 bg-blue-50 dark:bg-blue-900/20 rounded-lg transition-colors flex items-center gap-1"
                >
                  <Icon icon="ion:add" class="w-4 h-4" />
                  新建任务
                </button>
              </div>

              <div v-if="loadingTasks" class="space-y-4">
                <div v-for="i in 3" :key="i" class="h-20 bg-gray-100 dark:bg-gray-700 rounded-xl w-full animate-pulse"></div>
              </div>

              <div v-else-if="projectTasks.length === 0" class="py-12 text-center flex flex-col items-center">
                <div class="w-20 h-20 bg-gray-50 dark:bg-gray-800 rounded-full flex items-center justify-center mb-4 shadow-inner">
                  <Icon icon="ion:clipboard-outline" class="w-10 h-10 text-gray-300 dark:text-gray-600" />
                </div>
                <h3 class="text-gray-900 dark:text-white font-medium mb-1">一切就绪</h3>
                <p class="text-gray-400 dark:text-gray-500 text-sm">创建一个新任务开始工作吧</p>
              </div>

              <div v-else class="space-y-4">
                <div
                    v-for="task in projectTasks"
                    :key="task.id"
                    class="task-item group relative bg-white dark:bg-white/5 border border-gray-100 dark:border-white/5 p-4 rounded-2xl transition-all hover:shadow-md hover:-translate-y-0.5"
                >
                  <div class="flex justify-between items-start mb-2">
                    <h3 class="font-semibold text-gray-900 dark:text-gray-100 pr-8">{{ task.name }}</h3>
                    <!-- 状态胶囊 -->
                    <div :class="['status-pill',
                        task.status === '完成' ? 'status-success' :
                        task.status === '进行中' ? 'status-warning' :
                        task.status === '延期' ? 'status-danger' : 'status-default'
                      ]">
                      {{ task.status }}
                    </div>
                  </div>

                  <p class="text-sm text-gray-500 dark:text-gray-400 mb-3 line-clamp-2">{{ task.description }}</p>

                  <div class="flex items-center justify-between pt-2 border-t border-gray-100 dark:border-white/5">
                    <div class="flex items-center gap-2 text-xs text-gray-400 dark:text-gray-500 font-medium">
                      <Icon icon="ion:time-outline" class="w-3.5 h-3.5" />
                      {{ formatDate(task.endTime) }} 截止
                    </div>

                    <div class="flex items-center gap-3 opacity-0 group-hover:opacity-100 transition-opacity">
                      <button class="text-gray-400 hover:text-blue-500 transition-colors">
                        <Icon icon="ion:create-outline" class="w-4 h-4" />
                      </button>
                      <button @click="deleteTask(task.id)" class="text-gray-400 hover:text-red-500 transition-colors">
                        <Icon icon="ion:trash-outline" class="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>

      <!-- 弹窗 -->
      <n-modal
          v-model:show="showAddTaskDialog"
          class="custom-modal"
          preset="card"
          :bordered="false"
          size="huge"
          style="width: 600px; max-width: 90vw;"
      >
        <template #header>
          <div class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <div class="w-8 h-8 rounded-full bg-blue-500 text-white flex items-center justify-center">
              <Icon icon="ion:add" class="w-5 h-5" />
            </div>
            添加新任务
          </div>
        </template>

        <div class="space-y-6 py-4">
          <div class="form-group">
            <label>任务名称</label>
            <n-input v-model:value="taskForm.name" placeholder="例如：前端页面开发" size="large" />
          </div>

          <div class="form-group">
            <label>详细描述</label>
            <n-input
                v-model:value="taskForm.description"
                type="textarea"
                placeholder="描述任务的具体内容..."
                :autosize="{ minRows: 3, maxRows: 5 }"
            />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="form-group">
              <label>开始时间</label>
              <n-date-picker
                  v-model:value="taskForm.startTime"
                  type="datetime"
                  size="large"
                  class="w-full"
              />
            </div>
            <div class="form-group">
              <label>截止时间</label>
              <n-date-picker
                  v-model:value="taskForm.endTime"
                  type="datetime"
                  size="large"
                  class="w-full"
              />
            </div>
          </div>

          <div class="form-group">
            <label>初始状态</label>
            <n-select
                v-model:value="taskForm.status"
                size="large"
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
          <div class="flex justify-end gap-3 pt-2">
            <button @click="showAddTaskDialog = false" class="px-5 py-2.5 rounded-full text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-white/10 transition-colors font-medium">
              取消
            </button>
            <button @click="addTask" class="px-6 py-2.5 rounded-full bg-blue-600 hover:bg-blue-700 text-white shadow-lg shadow-blue-500/30 transition-all active:scale-95 font-medium">
              确认添加
            </button>
          </div>
        </template>
      </n-modal>

    </main>
  </div>
</template>

<style scoped>
/* 全局容器背景 */
.page-container {
  background-color: #F5F5F7; /* Apple Light Gray */
}

/* 原生 CSS 实现暗黑模式覆盖 */
.dark .page-container {
  background-color: #000000; /* True Black */
}

/* --- 卡片风格 (Apple Style) --- */
.apple-card {
  background-color: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px); /* Safari */
  border-radius: 24px;
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.04);
}

.dark .apple-card {
  background-color: rgba(28, 28, 30, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.2);
}

/* 导航返回按钮 */
.nav-button {
  background-color: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(0, 0, 0, 0.05);
}

.dark .nav-button {
  background-color: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

/* 功能图标按钮 */
.action-icon-btn {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: rgba(255, 255, 255, 0.5);
  color: #666;
  transition: background-color 0.2s, color 0.2s;
}

.action-icon-btn:hover {
  background-color: #fff;
  color: #000;
}

.dark .action-icon-btn {
  background-color: rgba(255, 255, 255, 0.1);
  color: #aaa;
}

.dark .action-icon-btn:hover {
  background-color: rgba(255, 255, 255, 0.2);
  color: #fff;
}

/* 主按钮 */
.primary-btn {
  background: linear-gradient(180deg, #007AFF 0%, #0062CC 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(0, 122, 255, 0.3);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.dark .primary-btn {
  background: linear-gradient(180deg, #0A84FF 0%, #0066CC 100%);
  box-shadow: 0 4px 12px rgba(10, 132, 255, 0.2);
}

/* --- 状态标签 (Finding Style) --- */
.status-pill {
  font-size: 11px;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 12px;
  line-height: 1.5;
}
.status-default { background-color: #F2F2F7; color: #8E8E93; }
.dark .status-default { background-color: rgba(255,255,255,0.1); color: #98989D; }

.status-success { background-color: #E3FDF0; color: #238E53; }
.dark .status-success { background-color: rgba(52, 199, 89, 0.15); color: #32D74B; }

.status-warning { background-color: #FFF4E5; color: #C96F04; }
.dark .status-warning { background-color: rgba(255, 149, 0, 0.15); color: #FF9F0A; }

.status-danger { background-color: #FFEBEB; color: #CD3D3D; }
.dark .status-danger { background-color: rgba(255, 69, 58, 0.15); color: #FF453A; }

/* --- NaiveUI 定制 (兼容原生CSS) --- */
.form-group label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 6px;
  color: #333;
}
.dark .form-group label {
  color: #ddd;
}

/* 强制 Naive Modal 背景 (因为 Preset 会自动带背景) */
:deep(.n-card) {
  background-color: #fff !important;
  border-radius: 24px !important;
}
.dark :deep(.n-card) {
  background-color: #1c1c1e !important;
  border: 1px solid #333;
}
:deep(.n-card-header__main) {
  color: inherit !important;
}

/* 滚动条美化 */
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(0,0,0,0.1);
  border-radius: 10px;
}
.dark .custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: rgba(255,255,255,0.1);
}

/* 简单动画 */
.fade-in {
  animation: fadeIn 0.6s ease-out forwards;
  opacity: 0;
}
.fade-in-up {
  animation: fadeInUp 0.6s ease-out forwards;
  opacity: 0;
  transform: translateY(20px);
}

@keyframes fadeIn {
  to { opacity: 1; }
}
@keyframes fadeInUp {
  to { opacity: 1; transform: translateY(0); }
}
</style>
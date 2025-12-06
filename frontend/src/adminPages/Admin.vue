<template>
  <div class="dashboard-container min-h-screen transition-colors duration-300">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">

      <!-- 页面标题区域 (可选，如果 layoutStore 已经处理了可以更简化，这里做个视觉占位增强苹果感) -->
      <div class="mb-8 px-2">
        <h2 class="text-3xl font-bold tracking-tight text-slate-900 dark:text-white text-effect">
          概览
        </h2>
        <p class="mt-1 text-gray-500 dark:text-gray-400 text-sm">
          {{ formatDate(new Date()) }}
        </p>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div v-for="i in 8" :key="i" class="apple-card-base h-40 animate-pulse bg-gray-200 dark:bg-gray-800"></div>
      </div>

      <!-- 数据概览卡片网格 -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div
            v-for="item in statItems"
            :key="item.key"
            class="apple-card group relative overflow-hidden p-6 flex flex-col justify-between transition-all duration-300 select-none"
        >
          <!-- 卡片头部：图标和标签 -->
          <div class="flex items-start justify-between">
            <div class="flex flex-col">
              <span class="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">
                {{ item.label }}
              </span>
              <!-- 标题 -->
              <span class="text-3xl font-bold tracking-tight text-gray-900 dark:text-white font-display">
                {{ formatNumber(statistics[item.key as keyof typeof statistics]) }}
              </span>
            </div>

            <!-- 图标容器 (类似 iOS App 图标) -->
            <div
                :class="['w-10 h-10 rounded-xl flex items-center justify-center shadow-sm transition-transform group-hover:scale-110', item.bgClass]">
              <Icon :icon="item.icon" :class="['w-6 h-6', item.textClass]"/>
            </div>
          </div>

          <!-- 装饰性背景光晕 (仅 Hover 时强化) -->
          <div
              :class="['absolute -bottom-4 -right-4 w-24 h-24 rounded-full blur-2xl opacity-0 group-hover:opacity-10 transition-opacity duration-500 pointer-events-none', item.bgClass]"></div>
        </div>
      </div>

      <!-- 监控数据区域 -->
      <div class="mt-12">
        <!-- 监控数据标题 -->
        <div class="mb-6 px-2">
          <h2 class="text-2xl font-bold tracking-tight text-slate-900 dark:text-white text-effect">
            系统监控
          </h2>
          <p class="mt-1 text-gray-500 dark:text-gray-400 text-sm">
            实时性能指标与系统状态
          </p>
        </div>

        <!-- 监控数据加载状态 -->
        <div v-if="monitoringLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          <div v-for="i in 6" :key="i" class="apple-card-base h-40 animate-pulse bg-gray-200 dark:bg-gray-800"></div>
        </div>

        <!-- 监控数据卡片 -->
        <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          <!-- HTTP性能指标 -->
          <div
              class="apple-card group relative overflow-hidden p-6 flex flex-col justify-between transition-all duration-300 select-none col-span-1 lg:col-span-3">
            <div class="flex items-start justify-between mb-4">
              <div class="flex items-center gap-2">
                <div class="w-8 h-8 rounded-xl bg-blue-100 dark:bg-blue-900/40 flex items-center justify-center">
                  <Icon icon="ion:server" class="w-5 h-5 text-blue-600 dark:text-blue-400"/>
                </div>
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white">HTTP 性能</h3>
              </div>
              <span class="text-xs text-gray-500 dark:text-gray-400">
                {{ formatDate(monitoringData.timestamp) }}
              </span>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
              <div class="space-y-1">
                <span class="text-xs text-gray-500 dark:text-gray-400">总请求数</span>
                <span class="text-2xl font-bold text-gray-900 dark:text-white">{{
                    monitoringData.http.totalRequests
                  }}</span>
              </div>
              <div class="space-y-1">
                <span class="text-xs text-gray-500 dark:text-gray-400">每秒请求数</span>
                <span class="text-2xl font-bold text-gray-900 dark:text-white">{{
                    monitoringData.http.requestsPerSecond.toFixed(2)
                  }}</span>
              </div>
              <div class="space-y-1">
                <span class="text-xs text-gray-500 dark:text-gray-400">错误率</span>
                <span class="text-2xl font-bold text-gray-900 dark:text-white">{{
                    monitoringData.http.errorRate.toFixed(2)
                  }}%</span>
              </div>
              <div class="space-y-1">
                <span class="text-xs text-gray-500 dark:text-gray-400">平均响应时间</span>
                <span class="text-2xl font-bold text-gray-900 dark:text-white">{{
                    monitoringData.http.averageResponseTime.toFixed(2)
                  }}ms</span>
              </div>
            </div>
          </div>

          <!-- 系统资源指标 -->
          <div
              class="apple-card group relative overflow-hidden p-6 flex flex-col justify-between transition-all duration-300 select-none">
            <div class="flex items-start justify-between mb-4">
              <div class="flex items-center gap-2">
                <div class="w-8 h-8 rounded-xl bg-green-100 dark:bg-green-900/40 flex items-center justify-center">
                  <Icon icon="ion:hardware-chip" class="w-5 h-5 text-green-600 dark:text-green-400"/>
                </div>
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white">系统资源</h3>
              </div>
            </div>
            <div class="space-y-4">
              <div class="space-y-1">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">CPU 使用率</span>
                  <span class="font-medium text-gray-900 dark:text-white">{{
                      monitoringData.system.cpuUsage.toFixed(2)
                    }}s</span>
                </div>
                <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                  <div class="bg-green-600 dark:bg-green-500 h-2 rounded-full"
                       :style="{ width: `${Math.min(monitoringData.system.cpuUsage * 10, 100)}%` }"></div>
                </div>
              </div>
              <div class="space-y-1">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">内存使用</span>
                  <span class="font-medium text-gray-900 dark:text-white">{{
                      (monitoringData.system.memoryUsage / (1024 * 1024)).toFixed(2)
                    }} MB</span>
                </div>
                <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                  <div class="bg-green-600 dark:bg-green-500 h-2 rounded-full"
                       :style="{ width: `${Math.min((monitoringData.system.memoryUsage / (1024 * 1024 * 1024)) * 100, 100)}%` }"></div>
                </div>
              </div>
              <div class="space-y-1">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">GC 收集次数</span>
                  <span class="font-medium text-gray-900 dark:text-white">{{
                      monitoringData.system.gcCollections.toFixed(0)
                    }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 应用指标 -->
          <div
              class="apple-card group relative overflow-hidden p-6 flex flex-col justify-between transition-all duration-300 select-none">
            <div class="flex items-start justify-between mb-4">
              <div class="flex items-center gap-2">
                <div class="w-8 h-8 rounded-xl bg-purple-100 dark:bg-purple-900/40 flex items-center justify-center">
                  <Icon icon="ion:apps" class="w-5 h-5 text-purple-600 dark:text-purple-400"/>
                </div>
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white">应用状态</h3>
              </div>
            </div>
            <div class="space-y-4">
              <div class="space-y-1">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">活跃请求</span>
                  <span class="font-medium text-gray-900 dark:text-white">{{ monitoringData.app.activeRequests }}</span>
                </div>
                <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                  <div class="bg-purple-600 dark:bg-purple-500 h-2 rounded-full"
                       :style="{ width: `${Math.min(monitoringData.app.activeRequests * 5, 100)}%` }"></div>
                </div>
              </div>
              <div class="space-y-1">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">当前连接数</span>
                  <span class="font-medium text-gray-900 dark:text-white">{{
                      monitoringData.app.connectionCount
                    }}</span>
                </div>
              </div>
              <div class="space-y-1">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">请求队列长度</span>
                  <span class="font-medium text-gray-900 dark:text-white">{{
                      monitoringData.app.requestQueueLength
                    }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 请求方法分布 -->
          <div class="apple-card group relative overflow-hidden p-6 flex flex-col justify-between transition-all duration-300 select-none">
            <div class="flex items-start justify-between mb-4">
              <div class="flex items-center gap-2">
                <div class="w-8 h-8 rounded-xl bg-amber-100 dark:bg-amber-900/40 flex items-center justify-center">
                  <Icon icon="ion:grid" class="w-5 h-5 text-amber-600 dark:text-amber-400"/>
                </div>
                <h3 class="text-lg font-semibold text-gray-900 dark:text-white">请求方法分布</h3>
              </div>
            </div>
            <div class="space-y-3">
              <div v-for="(count, method) in monitoringData.http.requestsByMethod" :key="method" class="space-y-1">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-500 dark:text-gray-400">{{ method }}</span>
                  <span class="font-medium text-gray-900 dark:text-white">{{ count }}</span>
                </div>
                <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                  <div class="bg-amber-600 dark:bg-amber-500 h-2 rounded-full" :style="{ width: `${(count / monitoringData.http.totalRequests) * 100}%` }"></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 数据统计控制 -->
        <div class="mt-6 flex justify-end">
          <button
            class="flex items-center gap-2 px-4 py-1.5 rounded-full bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-200 text-sm font-medium transition-colors shadow-sm active:scale-95 duration-200"
            @click="resetDataStats"
          >
            <Icon icon="ion:refresh" class="w-4 h-4"/>
            <span>重置数据统计</span>
          </button>
        </div>

        <!-- 数据访问统计 -->
        <div class="mt-8">
          <div class="mb-6 px-2">
            <h2 class="text-2xl font-bold tracking-tight text-slate-900 dark:text-white text-effect">
              数据访问统计
            </h2>
            <p class="mt-1 text-gray-500 dark:text-gray-400 text-sm">
              系统资源访问情况统计
            </p>
          </div>

          <div class="apple-card group relative overflow-hidden p-6 transition-all duration-300">
            <div v-if="monitoringLoading" class="animate-pulse">
              <div class="h-8 bg-gray-200 dark:bg-gray-700 rounded mb-4"></div>
              <div class="space-y-3">
                <div v-for="i in 5" :key="i" class="h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
              </div>
            </div>
            <div v-else class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead>
                  <tr>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      实体类型
                    </th>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      访问次数
                    </th>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      平均响应时间
                    </th>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      最后访问时间
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  <tr v-for="(item, index) in dataAccessStats" :key="index" class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
                    <td class="px-4 py-3 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                      {{ item.entityType || '未知' }}
                    </td>
                    <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {{ item.accessCount || 0 }}
                    </td>
                    <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {{ (item.averageResponseTime || 0).toFixed(2) }}ms
                    </td>
                    <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {{ item.lastAccessTime ? new Date(item.lastAccessTime).toLocaleString() : 'N/A' }}
                    </td>
                  </tr>
                  <tr v-if="dataAccessStats.length === 0">
                    <td colspan="4" class="px-4 py-8 text-center text-sm text-gray-500 dark:text-gray-400">
                      暂无数据访问统计
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- 数据变化统计 -->
        <div class="mt-8">
          <div class="mb-6 px-2">
            <h2 class="text-2xl font-bold tracking-tight text-slate-900 dark:text-white text-effect">
              数据变化统计
            </h2>
            <p class="mt-1 text-gray-500 dark:text-gray-400 text-sm">
              数据增删改操作统计
            </p>
          </div>

          <div class="apple-card group relative overflow-hidden p-6 transition-all duration-300">
            <div v-if="monitoringLoading" class="animate-pulse">
              <div class="h-8 bg-gray-200 dark:bg-gray-700 rounded mb-4"></div>
              <div class="space-y-3">
                <div v-for="i in 5" :key="i" class="h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
              </div>
            </div>
            <div v-else class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead>
                  <tr>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      实体类型
                    </th>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      创建次数
                    </th>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      更新次数
                    </th>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      删除次数
                    </th>
                    <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      总变化量
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  <tr v-for="(item, index) in dataChangeStats" :key="index" class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
                    <td class="px-4 py-3 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                      {{ item.entityType || '未知' }}
                    </td>
                    <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400 text-green-600 dark:text-green-400">
                      +{{ item.createdCount || 0 }}
                    </td>
                    <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400 text-blue-600 dark:text-blue-400">
                      {{ item.updatedCount || 0 }}
                    </td>
                    <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400 text-red-600 dark:text-red-400">
                      -{{ item.deletedCount || 0 }}
                    </td>
                    <td class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400 font-medium">
                      {{ (item.createdCount || 0) + (item.updatedCount || 0) + (item.deletedCount || 0) }}
                    </td>
                  </tr>
                  <tr v-if="dataChangeStats.length === 0">
                    <td colspan="5" class="px-4 py-8 text-center text-sm text-gray-500 dark:text-gray-400">
                      暂无数据变化统计
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- 隐藏的文件输入 -->
      <input ref="fileInput" type="file" accept=".json" multiple @change="uploadFiles" style="display: none"/>
    </main>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted, onBeforeUnmount, defineComponent, h} from 'vue'
import {useMessage, useDialog} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {DataCentreService} from '../services/DataCentreService'
import {MonitoringService} from '../services/MonitoringService'
import {useAuthorizationStore} from '../stores/Authorization'
import {useLayoutStore} from '../stores/LayoutStore'

// Types
interface StatItemConfig {
  key: string
  label: string
  icon: string
  bgClass: string // Tailwind background class
  textClass: string // Tailwind text class
}

// Stores & Utils
const message = useMessage()
const dialog = useDialog()
const authorizationStore = useAuthorizationStore()
const layoutStore = useLayoutStore()

// State
const fileInput = ref<HTMLInputElement>()
const loading = ref(false)
const monitoringLoading = ref(false)
const isAdmin = ref(false)
const statistics = ref({
  members: 0,
  staffs: 0,
  projects: 0,
  tasks: 0,
  resources: 0,
  departments: 0,
  todos: 0
})

// Monitoring data state
const monitoringData = ref({
  http: {
    totalRequests: 0,
    requestsPerSecond: 0,
    errorRate: 0,
    averageResponseTime: 0,
    responseTimeByStatus: {} as Record<string, number>,
    requestsByMethod: {} as Record<string, number>
  },
  system: {
    cpuUsage: 0,
    memoryUsage: 0,
    gcCollections: 0
  },
  app: {
    activeRequests: 0,
    connectionCount: 0,
    requestQueueLength: 0
  },
  timestamp: new Date()
})

// HTTP stats state
const httpStats = ref({
  totalRequests: 0,
  requestsPerSecond: 0,
  errorRate: 0,
  averageResponseTime: 0
})

// Data access stats state
const dataAccessStats = ref([] as any[])
const dataChangeStats = ref([] as any[])
const statsTopCount = ref(10)

// Configuration for specific visual items (Bento Style Configuration)
// 这里配置每个卡片的颜色和图标，模拟 Apple 系统应用的配色
const statItems = computed<StatItemConfig[]>(() => [
  {
    key: 'members',
    label: '社员总数',
    icon: 'ion:people',
    bgClass: 'bg-blue-100 dark:bg-blue-900/40',
    textClass: 'text-blue-600 dark:text-blue-400'
  },
  {
    key: 'staffs',
    label: '部员人数',
    icon: 'material-symbols:badge',
    bgClass: 'bg-green-100 dark:bg-green-900/40',
    textClass: 'text-green-600 dark:text-green-400'
  },
  {
    key: 'projects',
    label: '项目归档',
    icon: 'material-symbols:folder',
    bgClass: 'bg-purple-100 dark:bg-purple-900/40',
    textClass: 'text-purple-600 dark:text-purple-400'
  },
  {
    key: 'tasks',
    label: '任务统计',
    icon: 'material-symbols:task',
    bgClass: 'bg-amber-100 dark:bg-amber-900/40',
    textClass: 'text-amber-600 dark:text-amber-400'
  },
  {
    key: 'resources',
    label: '资源池',
    icon: 'material-symbols:folder-open',
    bgClass: 'bg-pink-100 dark:bg-pink-900/40',
    textClass: 'text-pink-600 dark:text-pink-400'
  },
  {
    key: 'departments',
    label: '部门架构',
    icon: 'material-symbols:groups',
    bgClass: 'bg-indigo-100 dark:bg-indigo-900/40',
    textClass: 'text-indigo-600 dark:text-indigo-400'
  },
  {
    key: 'todos',
    label: '待办事项',
    icon: 'ion:checkbox',
    bgClass: 'bg-red-100 dark:bg-red-900/40',
    textClass: 'text-red-600 dark:text-red-400'
  }
])

// Helper: Format numbers nicely (e.g. 1,000)
const formatNumber = (num: number) => {
  return new Intl.NumberFormat('en-US').format(num)
}

// Helper: Format Date
const formatDate = (date: Date | string) => {
  try {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    if (isNaN(dateObj.getTime())) {
      return '无效时间';
    }
    return new Intl.DateTimeFormat('zh-CN', {dateStyle: 'full', timeStyle: 'short'}).format(dateObj);
  } catch (error) {
    return '无效时间';
  }
}

// --- Logic Actions ---

const triggerFileInput = () => fileInput.value?.click()

const uploadFiles = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = target.files
  if (!files || !files.length) return

  try {
    loading.value = true // Show loading visual feedback
    for (let i = 0; i < files.length; i++) {
      const file = files[i]
      try {
        await DataCentreService.updateDataFromJson(file)
        message.success(`文件 "${file.name}" 已同步`)
      } catch (error) {
        console.error(error)
        message.error(`解析 "${file.name}" 失败`)
      }
    }
    // Refresh data after upload
    await fetchData()
  } catch (error) {
    message.error('上传流程异常')
  } finally {
    loading.value = false
    // Reset input
    target.value = ''
  }
}

const downloadAllData = async () => {
  const token = localStorage.getItem('Authorization')
  if (!token) return message.error('未授权')

  try {
    const blob = await DataCentreService.exportJson()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `backup-${new Date().toISOString().split('T')[0]}.json`
    document.body.appendChild(a)
    a.click()
    setTimeout(() => {
      document.body.removeChild(a);
      URL.revokeObjectURL(url)
    }, 100)
    message.success('备份已导出')
  } catch (e) {
    message.error('导出失败')
  }
}

const removeAllData = () => {
  const d = dialog.warning({
    title: '抹除所有数据',
    content: '此操作将清空数据库且无法撤销。请确保已备份数据。',
    positiveText: '确认抹除',
    negativeText: '取消',
    // Style adjustment for dialog buttons if needed via global theme,
    // here we stick to standard NaiveUI call
    onPositiveClick: async () => {
      d.loading = true
      try {
        const token = localStorage.getItem('Authorization')
        if (!token) throw new Error('No Token')

        const res = await fetch('https://www.xauat.site/api/Admin/RemoveAllData', {
          method: 'DELETE',
          headers: {'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json'}
        })

        if (res.ok) {
          message.success('数据已重置')
          await fetchData()
        } else {
          throw new Error('API Error')
        }
      } catch (e) {
        message.error('重置失败')
      } finally {
        d.loading = false
      }
    }
  })
}

const fetchMonitoringData = async () => {
  monitoringLoading.value = true
  try {
    // 获取性能监控数据
    const performanceData = await MonitoringService.getPerformanceMetrics()
    monitoringData.value = performanceData
    
    // 获取HTTP统计数据
    const httpData = await MonitoringService.getHttpStats()
    httpStats.value = httpData
    
    // 获取数据访问统计
    const accessData = await MonitoringService.getDataAccessStats(undefined, statsTopCount.value)
    dataAccessStats.value = accessData.data || accessData
    
    // 获取数据变化统计
    const changeData = await MonitoringService.getDataChangeStats(undefined, statsTopCount.value)
    dataChangeStats.value = changeData.data || changeData
  } catch (e) {
    console.error('Failed to fetch monitoring data:', e)
    message.error('无法获取监控数据')
  } finally {
    monitoringLoading.value = false
  }
}

const resetDataStats = async () => {
  try {
    const d = dialog.warning({
      title: '重置数据统计',
      content: '此操作将重置所有数据统计信息，是否继续？',
      positiveText: '确认重置',
      negativeText: '取消',
      onPositiveClick: async () => {
        d.loading = true
        try {
          await MonitoringService.resetDataStats()
          message.success('数据统计已重置')
          await fetchMonitoringData()
        } catch (error) {
          message.error('重置失败')
        } finally {
          d.loading = false
        }
      }
    })
  } catch (e) {
    message.error('重置数据统计失败')
  }
}

const fetchData = async () => {
  loading.value = true
  try {
    statistics.value = await DataCentreService.getCentreData()
    await fetchMonitoringData()
  } catch (e) {
    message.error('无法获取核心数据')
  } finally {
    loading.value = false
  }
}

// --- Lifecycle & Header Actions Injection ---

onMounted(async () => {
  layoutStore.setPageHeader('数据分析', '系统核心指标监控')
  layoutStore.setShowPageActions(true)

  // Custom Action Component (Redesigned for pill-shape/iOS style)
  const ActionsComponent = defineComponent({
    setup() {
      const isOpen = ref(false)

      // Close dropdown when clicking outside
      onMounted(() => {
        document.addEventListener('click', (e: MouseEvent) => {
          const target = e.target as HTMLElement
          if (!target.closest('.ios-dropdown-trigger')) {
            isOpen.value = false
          }
        })
      })

      return () => h('div', {class: 'flex items-center gap-3'}, [
        // Upload Button - iOS Pill Style
        h('button', {
          class: 'flex items-center gap-2 px-4 py-1.5 rounded-full bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium transition-colors shadow-sm active:scale-95 duration-200',
          onClick: triggerFileInput
        }, [
          h(Icon, {icon: 'ion:cloud-upload', class: 'w-4 h-4'}),
          h('span', '导入数据')
        ]),

        // More Options Button & Dropdown
        h('div', {class: 'relative ios-dropdown-trigger'}, [
          h('button', {
            onClick: (e) => {
              e.stopPropagation();
              isOpen.value = !isOpen.value
            },
            class: `w-8 h-8 rounded-full flex items-center justify-center transition-colors duration-200 ${isOpen.value ? 'bg-gray-200 dark:bg-gray-700' : 'bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700'}`
          }, [
            h(Icon, {icon: 'ion:ellipsis-horizontal', class: 'text-gray-600 dark:text-gray-300'})
          ]),

          // Dropdown Menu (Glassmorphism look handled by CSS classes below)
          h('div', {
            class: `absolute right-0 mt-3 w-48 py-1 rounded-xl apple-dropdown origin-top-right transition-all duration-200 ${isOpen.value ? 'opacity-100 scale-100 visible' : 'opacity-0 scale-95 invisible'}`
          }, [
            h('div', {class: 'px-1 py-1'}, [
              h('button', {
                onClick: () => {
                  isOpen.value = false;
                  downloadAllData()
                },
                class: 'w-full text-left px-3 py-2 text-sm rounded-lg text-gray-700 dark:text-gray-200 hover:bg-blue-50 dark:hover:bg-blue-900/30 hover:text-blue-600 transition-colors flex items-center gap-2'
              }, [
                h(Icon, {icon: 'ion:download-outline', class: 'w-4 h-4'}),
                h('span', '导出备份')
              ]),
              h('div', {class: 'h-px bg-gray-100 dark:bg-gray-700 my-1 mx-2'}), // Divider
              h('button', {
                onClick: () => {
                  isOpen.value = false;
                  removeAllData()
                },
                class: 'w-full text-left px-3 py-2 text-sm rounded-lg text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors flex items-center gap-2'
              }, [
                h(Icon, {icon: 'ion:trash-outline', class: 'w-4 h-4'}),
                h('span', '清空数据')
              ])
            ])
          ])
        ])
      ])
    }
  })

  layoutStore.setActionsComponent(ActionsComponent)
  isAdmin.value = authorizationStore.isAdmin()
  await fetchData()
})

onBeforeUnmount(() => {
  layoutStore.clearPageHeader()
})
</script>

<style scoped>
/*
  Apple Design System Styles
  使用原生 CSS 实现 Tailwind 4.x 现有工具类无法完全覆盖的细腻效果
  尤其是暗黑模式下的边框和阴影处理
*/

.dashboard-container {
  /* 浅灰背景，模拟 iOS 设置页面背景 */
  background-color: #f2f2f7;
}

/* Apple 卡片基础样式 */
.apple-card {
  background-color: rgba(255, 255, 255, 1);
  border-radius: 20px; /* iOS 风格大圆角 */
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05), 0 4px 12px rgba(0, 0, 0, 0.01);
  border: 1px solid rgba(0, 0, 0, 0.02);
}

.apple-card-base {
  border-radius: 20px;
}

/* 下拉菜单样式 (仿 macOS/iOS 菜单) */
.apple-dropdown {
  background-color: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(0, 0, 0, 0.05);
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  z-index: 50;
}

/* --- Dark Mode Overrides --- */
.dark .dashboard-container {
  background-color: #000000; /* 深黑背景 */
}

.dark .apple-card {
  /* Apple 暗色模式常用的深灰色 (#1c1c1e) */
  background-color: #1c1c1e;
  border: 1px solid rgba(255, 255, 255, 0.08); /* 微弱的应用内边框 */
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.dark .apple-dropdown {
  background-color: #252525;
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.4);
}

/* 字体微调，使用 iOS 系统字体栈 */
.font-display {
  font-family: -apple-system, BlinkMacSystemFont, "SF Pro Display", "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
}
</style>
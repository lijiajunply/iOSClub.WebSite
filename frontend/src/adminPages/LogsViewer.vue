<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 页面标题 -->
      <div class="mb-8">
        <div>
          <h1 class="text-2xl font-semibold tracking-tight">系统日志管理</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">查看、过滤和分析系统运行日志</p>
        </div>
      </div>

      <!-- 日志统计图表区域 -->
      <div
          class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl p-6 mb-6 transition-all duration-300 shadow-sm hover:shadow-lg">
        <h2 class="text-lg font-medium text-gray-900 dark:text-white mb-4">日志统计</h2>

        <!-- 统计卡片 -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
          <!-- 总日志数 -->
          <div
              class="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-4 border-l-4 border-blue-500 transition-transform hover:scale-[1.02]">
            <p class="text-sm text-gray-500 dark:text-gray-400">总日志数</p>
            <p class="text-2xl font-bold mt-1">{{ statisticsLoading ? '-' : totalLogsCount }}</p>
            <div class="flex items-center text-xs text-gray-500 dark:text-gray-400 mt-2">
              <span class="w-1 h-1 bg-gray-400 rounded-full mr-1"></span>
              {{ statisticsLoading ? '加载中...' : `今日新增: ${Math.floor(totalLogsCount / 2)}` }}
            </div>
            <div v-if="statisticsError && !statisticsLoading" class="text-xs text-red-500 mt-1">
              {{ statisticsError }}
            </div>
          </div>

          <!-- 错误日志数 -->
          <div
              class="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-4 border-l-4 border-red-500 transition-transform hover:scale-[1.02]">
            <p class="text-sm text-gray-500 dark:text-gray-400">错误日志</p>
            <p class="text-2xl font-bold mt-1 text-red-500">{{ errorLogsCount }}</p>
            <div class="flex items-center text-xs text-gray-500 dark:text-gray-400 mt-2">
              <span class="w-1 h-1 bg-red-400 rounded-full mr-1"></span>
              需紧急处理
            </div>
          </div>

          <!-- 警告日志数 -->
          <div
              class="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-4 border-l-4 border-yellow-500 transition-transform hover:scale-[1.02]">
            <p class="text-sm text-gray-500 dark:text-gray-400">警告日志</p>
            <p class="text-2xl font-bold mt-1 text-yellow-500">{{ warningLogsCount }}</p>
            <div class="flex items-center text-xs text-gray-500 dark:text-gray-400 mt-2">
              <span class="w-1 h-1 bg-yellow-400 rounded-full mr-1"></span>
              需要关注
            </div>
          </div>

          <!-- 信息日志数 -->
          <div
              class="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-4 border-l-4 border-green-500 transition-transform hover:scale-[1.02]">
            <p class="text-sm text-gray-500 dark:text-gray-400">信息日志</p>
            <p class="text-2xl font-bold mt-1 text-green-500">{{ infoLogsCount }}</p>
            <div class="flex items-center text-xs text-gray-500 dark:text-gray-400 mt-2">
              <span class="w-1 h-1 bg-green-400 rounded-full mr-1"></span>
              正常运行信息
            </div>
          </div>
        </div>
      </div>

      <!-- 过滤和操作栏 -->
      <div
          class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl p-6 mb-6 transition-all duration-300 shadow-sm hover:shadow-lg">
        <!-- 快速搜索 -->
        <div class="mb-4">
          <div class="relative">
            <Icon icon="material-symbols:search"
                  class="z-50 absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 dark:text-gray-500 w-4.5 h-4.5"/>
            <n-input v-model:value="searchKeyword" placeholder="搜索日志消息或来源..." class="pl-10"
                     @keyup.enter="applyFilters"/>
            <n-button v-if="searchKeyword" type="default" text circle size="small"
                      class="absolute right-2 top-1/2 transform -translate-y-1/2"
                      @click="searchKeyword = ''; applyFilters()">
              <Icon icon="material-symbols:close" class="z-50 w-4 h-4"/>
            </n-button>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <!-- 日志级别过滤 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              日志级别
            </label>
            <n-select v-model:value="selectedLevel" placeholder="选择日志级别" :options="logLevelOptions" clearable
                      class="w-full" @update:value="applyFilters"/>
          </div>

          <!-- 时间范围 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              时间范围
            </label>
            <n-date-picker v-model:value="dateRange" type="daterange" clearable class="w-full"
                           format="yyyy-MM-dd HH:mm:ss" @update:value="applyFilters"/>
          </div>

          <!-- 操作按钮组 -->
          <div class="flex items-end gap-2">
            <n-button type="default" @click="resetFilters" class="flex-1">
              重置过滤
            </n-button>
            <n-button type="primary" @click="refreshLogs" :loading="loading" class="flex-1">
              <template #icon>
                <Icon icon="material-symbols:refresh" class="w-4.5 h-4.5"/>
              </template>
              刷新日志
            </n-button>
            <n-button type="error" @click="showCleanupDialog = true" class="flex-1">
              <template #icon>
                <Icon icon="material-symbols:delete-outline" class="w-4.5 h-4.5"/>
              </template>
              清理日志
            </n-button>
          </div>
        </div>

        <!-- 高级过滤 (默认折叠) -->
        <div class="mt-4">
          <n-collapse>
            <n-collapse-item title="高级过滤" name="1">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
                <!-- 异常信息过滤 -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    包含异常
                  </label>
                  <n-switch v-model:value="hasException" @update:value="applyFilters"/>
                </div>

                <!-- 快速时间范围选择 -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    快速时间选择
                  </label>
                  <div class="flex gap-2 flex-wrap">
                    <n-button v-for="range in quickTimeRanges" :key="range.value" type="default" size="small"
                              :secondary="selectedQuickTimeRange !== range.value"
                              :tertiary="selectedQuickTimeRange === range.value"
                              @click="applyQuickTimeRange(range.value)"
                              class="text-xs">
                      {{ range.label }}
                    </n-button>
                  </div>
                </div>
              </div>
            </n-collapse-item>
          </n-collapse>
        </div>
      </div>

      <!-- 日志列表 -->
      <div
          class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl overflow-hidden transition-all duration-300 shadow-sm hover:shadow-lg">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                时间
              </th>
              <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                级别
              </th>
              <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                消息
              </th>
            </tr>
            </thead>
            <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <!-- 骨架加载 -->
            <template v-if="loading">
              <tr v-for="i in 5" :key="i">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-32"></div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="h-5 bg-gray-200 dark:bg-gray-700 rounded w-16"></div>
                </td>
                <td class="px-6 py-4">
                  <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
                  <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
                </td>
              </tr>
            </template>

            <!-- 日志列表 -->
            <template v-else>
              <tr v-for="log in paginatedLogs" :key="`${log.timestamp}-${log.level}`"
                  class="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-150">
                <td class="px-4 sm:px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                  {{ formatDateTime(log.timestamp) }}
                </td>
                <td class="px-4 sm:px-6 py-4 whitespace-nowrap">
                  <n-tag :type="getLogLevelType(log.level)" :bordered="false" class="rounded-full">
                    {{ log.level }}
                  </n-tag>
                </td>
                <td class="px-4 sm:px-6 py-4 text-sm text-gray-900 dark:text-gray-100">
                  <div
                      class="line-clamp-2 cursor-pointer hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
                      @click="showLogDetails(log)">
                    {{ log.message || '无消息内容' }}
                  </div>
                </td>
              </tr>
            </template>

            <tr v-if="paginatedLogs.length === 0">
              <td colspan="3" class="px-4 sm:px-6 py-12 text-center">
                <div class="flex flex-col items-center">
                  <div
                      class="w-16 h-16 flex items-center justify-center bg-gray-100 dark:bg-gray-700 rounded-full mb-4">
                    <Icon icon="material-symbols:history-off" class="text-gray-400 dark:text-gray-500 w-7 h-7"/>
                  </div>
                  <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-1">暂无日志记录</h3>
                  <p class="text-sm text-gray-500 dark:text-gray-400">系统尚未生成任何日志记录</p>
                </div>
              </td>
            </tr>
            </tbody>
          </table>
        </div>

        <!-- 分页 -->
        <div class="px-4 sm:px-6 py-4 border-t border-gray-200 dark:border-gray-700 flex items-center justify-between">
          <div class="flex-1 flex justify-between sm:hidden">
            <n-button type="default" size="small" disabled v-if="currentPage === 1">
              上一页
            </n-button>
            <n-button type="default" size="small" v-else @click="currentPage--">
              上一页
            </n-button>
            <n-button type="default" size="small" disabled v-if="!hasMore">
              下一页
            </n-button>
            <n-button type="default" size="small" v-else @click="currentPage++">
              下一页
            </n-button>
          </div>
          <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
            <div>
              <p class="text-sm text-gray-700 dark:text-gray-300">
                显示第 <span class="font-medium">{{ Math.max(0, (currentPage - 1) * pageSize + 1) }}</span> 到 <span
                  class="font-medium">{{ Math.min(totalLogs, currentPage * pageSize) }}</span> 条，共 <span
                  class="font-medium">{{ totalLogs }}</span> 条日志
              </p>
            </div>
            <div>
              <n-pagination v-model:page="currentPage" v-model:page-size="pageSize" :page-sizes="[10, 20, 50, 100]"
                            :item-count="totalLogs" show-size-picker show-quick-jumper show-total
                            @update:page="handlePageChange"
                            @update:page-size="handlePageSizeChange"/>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>

  <!-- 日志详情对话框 -->
  <n-modal v-model:show="showLogDetailsDialog" preset="dialog" title="日志详情" :width="600" :bordered="false"
           :mask-closable="false" class="backdrop-blur-md rounded-xl">
    <div v-if="selectedLog" class="space-y-4">
      <div>
        <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">时间</h3>
        <p class="text-gray-900 dark:text-white">{{ formatDateTime(selectedLog.timestamp) }}</p>
      </div>
      <div>
        <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">级别</h3>
        <n-tag :type="getLogLevelType(selectedLog.level)" :bordered="false" class="rounded-full">
          {{ selectedLog.level }}
        </n-tag>
      </div>
      <div>
        <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">消息</h3>
        <p class="text-gray-900 dark:text-white whitespace-pre-wrap">{{ selectedLog.message || '无消息内容' }}</p>
      </div>
      <div v-if="selectedLog.properties && Object.keys(selectedLog.properties).length > 0">
        <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">属性</h3>
        <pre
            class="bg-gray-100 dark:bg-gray-800 p-3 rounded-md text-sm text-gray-900 dark:text-gray-100 overflow-auto max-h-40 shadow-inner">
        {{ JSON.stringify(selectedLog.properties, null, 2) }}</pre>
      </div>
      <div v-if="selectedLog.exception">
        <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">异常信息</h3>
        <pre
            class="bg-gray-100 dark:bg-gray-800 p-3 rounded-md text-sm text-gray-900 dark:text-gray-100 overflow-auto max-h-40 shadow-inner">
        {{ selectedLog.exception }}</pre>
      </div>
    </div>
  </n-modal>

  <!-- 清理日志对话框 -->
  <n-modal v-model:show="showCleanupDialog" preset="card" title="清理旧日志" :bordered="false" style="max-width: 500px"
           class="backdrop-blur-md rounded-xl">
    <div class="space-y-4">
      <div class="text-sm text-gray-600 dark:text-gray-400">
        此操作将删除指定天数前的所有日志，操作不可恢复。请谨慎执行。
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
          保留最近的日志天数
        </label>
        <n-input-number v-model:value="cleanupDays" :min="1" :max="365" :step="1" placeholder="输入天数"/>
        <div class="text-xs text-gray-500 dark:text-gray-500 mt-1">
          例如：输入7将保留最近7天的日志，删除更早的日志
        </div>
      </div>
    </div>
    <template #footer>
      <div class="flex justify-end gap-2">
        <n-button type="default" @click="showCleanupDialog = false">
          取消
        </n-button>
        <n-button type="error" @click="confirmCleanup" :loading="cleanupLoading">
          <template #icon>
            <Icon icon="material-symbols:delete-outline" class="w-4.5 h-4.5"/>
          </template>
          确认清理
        </n-button>
      </div>
    </template>
  </n-modal>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue'
import {
  NTag,
  NSelect,
  NDatePicker,
  NButton,
  NModal,
  NPagination,
  NInput,
  NSwitch,
  NCollapse,
  NCollapseItem,
  NInputNumber,
  useMessage
} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {LogsService, type LogEntry} from '../services/LogsService';

// 响应式数据
const logs = ref<LogEntry[]>([])
const loading = ref<boolean>(false)
const statisticsLoading = ref<boolean>(false)
const statisticsError = ref<string | null>(null)
const selectedLevel = ref<string | null>(null)
const dateRange = ref<[number, number] | null>(null)
const currentPage = ref<number>(1)
const pageSize = ref<number>(20)
const totalLogs = ref<number>(0)
const totalPages = ref<number>(1)
const searchKeyword = ref<string>('')
const hasException = ref<boolean>(false)
const selectedLog = ref<LogEntry | null>(null)
const showLogDetailsDialog = ref<boolean>(false)
const selectedQuickTimeRange = ref<string | null>(null)

// 清理日志相关数据
const showCleanupDialog = ref<boolean>(false)
const cleanupDays = ref<number>(7)
const cleanupLoading = ref<boolean>(false)

// 消息提示
const message = useMessage()

// 统计数据
const totalLogsCount = ref<number>(0)
const errorLogsCount = ref<number>(0)
const warningLogsCount = ref<number>(0)
const infoLogsCount = ref<number>(0)

// 日志级别选项
const logLevelOptions = [
  {label: 'Information', value: 'Information'},
  {label: 'Warning', value: 'Warning'},
  {label: 'Error', value: 'Error'},
  {label: 'Critical', value: 'Critical'},
  {label: 'Debug', value: 'Debug'},
  {label: 'Trace', value: 'Trace'}
]

// 时间范围选项
const quickTimeRanges = [
  {label: '今天', value: 'today'},
  {label: '昨天', value: 'yesterday'},
  {label: '近7天', value: '7days'},
  {label: '近30天', value: '30days'},
  {label: '本周', value: 'thisWeek'},
  {label: '本月', value: 'thisMonth'}
]

// 工具函数
const getLogLevelType = (level: string): 'default' | 'success' | 'error' | 'warning' | 'primary' | 'info' => {
  switch (level) {
    case 'Information':
      return 'info'
    case 'Warning':
      return 'warning'
    case 'Error':
    case 'Critical':
      return 'error'
    case 'Debug':
    case 'Trace':
      return 'default'
    default:
      return 'default'
  }
}

const formatDateTime = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

// 快速时间范围选择
const applyQuickTimeRange = async (range: string): Promise<void> => {
  selectedQuickTimeRange.value = range
  const now = new Date()
  const start = new Date()

  switch (range) {
    case 'today':
      start.setHours(0, 0, 0, 0)
      break
    case 'yesterday':
      start.setDate(now.getDate() - 1)
      start.setHours(0, 0, 0, 0)
      now.setDate(now.getDate() - 1)
      now.setHours(23, 59, 59, 999)
      break
    case '7days':
      start.setDate(now.getDate() - 6)
      start.setHours(0, 0, 0, 0)
      break
    case '30days':
      start.setDate(now.getDate() - 29)
      start.setHours(0, 0, 0, 0)
      break
    case 'thisWeek':
      // 设置为本周的第一天（周一）
      const day = now.getDay() || 7 // 将周日(0)转换为7
      start.setDate(now.getDate() - day + 1)
      start.setHours(0, 0, 0, 0)
      break
    case 'thisMonth':
      start.setDate(1)
      start.setHours(0, 0, 0, 0)
      break
  }

  dateRange.value = [
    start.getTime(),
    now.getTime()
  ]

  await applyFilters()
}

// 直接使用从API获取的已过滤日志数据，不再进行前端过滤
const filteredLogs = computed((): LogEntry[] => {
  return logs.value;
})

// 移除前端分页逻辑，使用API返回的数据
const paginatedLogs = computed((): LogEntry[] => {
  return filteredLogs.value
})


// 获取日志数据，支持多条件搜索
const getLogs = async (): Promise<void> => {
  if (loading.value) return;

  loading.value = true;
  try {
    // 确定时间范围参数
    let timeRangeParam: string | undefined;
    if (selectedQuickTimeRange.value === 'today') {
      timeRangeParam = 'today';
    } else if (selectedQuickTimeRange.value && selectedQuickTimeRange.value.endsWith('days')) {
      // 提取天数（如7days -> 7）
      const daysMatch = selectedQuickTimeRange.value.match(/^(\d+)days$/);
      if (daysMatch) {
        timeRangeParam = daysMatch[1];
      }
    }

    // 调用更新后的服务方法，传入所有搜索参数
    const response = await LogsService.getRecentLogs(
        currentPage.value,
        pageSize.value,
        searchKeyword.value,
        selectedLevel.value || undefined,
        timeRangeParam
    );

    logs.value = response.data;
    totalLogs.value = response.totalCount;
    totalPages.value = response.totalPages;
  } catch (error) {
    console.error('获取日志时发生错误:', error);
    // 可以添加错误提示逻辑
  } finally {
    loading.value = false;
  }
}

// 刷新日志
const refreshLogs = async (): Promise<void> => {
  await getLogs();
}

// 应用过滤条件
const applyFilters = async (): Promise<void> => {
  // 重置到第一页
  currentPage.value = 1;
  await getLogs();
}

// 重置过滤条件
const resetFilters = (): void => {
  selectedLevel.value = null;
  searchKeyword.value = '';
  dateRange.value = null;
  selectedQuickTimeRange.value = null;
  hasException.value = false;
  currentPage.value = 1;
  applyFilters();
}

// 处理页码变化
const handlePageChange = async (page: number): Promise<void> => {
  currentPage.value = page;
  await getLogs();
}

// 处理每页数量变化
const handlePageSizeChange = async (size: number): Promise<void> => {
  pageSize.value = size;
  currentPage.value = 1;
  await getLogs();
}

// 显示日志详情
const showLogDetails = (log: LogEntry): void => {
  selectedLog.value = log;
  showLogDetailsDialog.value = true;
}

// 加载统计数据
const loadStatistics = async (): Promise<void> => {
  statisticsLoading.value = true;
  statisticsError.value = null;
  try {
    const stats = await LogsService.getLogStatistics();
    totalLogsCount.value = stats.totalCount;
    errorLogsCount.value = stats.levelCounts['Error'] || 0;
    warningLogsCount.value = stats.levelCounts['Warning'] || 0;
    infoLogsCount.value = stats.levelCounts['Information'] || 0;
  } catch (error) {
    statisticsError.value = '加载统计数据失败';
    console.error('加载统计数据失败:', error);
  } finally {
    statisticsLoading.value = false;
  }
}

// 确认清理日志
const confirmCleanup = async (): Promise<void> => {
  if (!cleanupDays.value || cleanupDays.value < 1) {
    message.warning('请输入有效的天数');
    return;
  }

  cleanupLoading.value = true;
  try {
    const result = await LogsService.cleanupOldLogs(cleanupDays.value);
    message.success(result.Message);
    showCleanupDialog.value = false;

    // 清理成功后，刷新日志和统计数据
    await getLogs();
    await loadStatistics();
  } catch (error: any) {
    message.error(error.message || '清理日志失败');
    console.error('清理日志失败:', error);
  } finally {
    cleanupLoading.value = false;
  }
}

// 生命周期钩子
onMounted(async (): Promise<void> => {
  // 加载日志数据
  await getLogs();
  // 加载统计数据
  await loadStatistics();
})

// 计算属性 - 是否有更多页
const hasMore = computed((): boolean => {
  return currentPage.value < totalPages.value
})
</script>

<style scoped>
/* 自定义样式优化 */
.n-button {
  border-radius: 10px !important;
  transition: all 0.2s ease;
}

.n-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.n-select,
.n-input,
.n-date-picker {
  border-radius: 10px !important;
  border: 1px solid #e5e7eb !important;
}

.dark .n-select,
.dark .n-input,
.dark .n-date-picker {
  border-color: #374151 !important;
}

.n-dialog {
  border-radius: 20px !important;
  overflow: hidden;
}

/* 表格样式优化 */
table {
  border-collapse: separate;
  border-spacing: 0;
}

table thead th {
  font-weight: 500;
  letter-spacing: 0.05em;
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

/* 统计卡片样式优化 */
@media (max-width: 640px) {
  .grid-cols-4 {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
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
            v-for="(item, index) in statItems"
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
            <div :class="['w-10 h-10 rounded-xl flex items-center justify-center shadow-sm transition-transform group-hover:scale-110', item.bgClass]">
              <Icon :icon="item.icon" :class="['w-6 h-6', item.textClass]" />
            </div>
          </div>

          <!-- 装饰性背景光晕 (仅 Hover 时强化) -->
          <div :class="['absolute -bottom-4 -right-4 w-24 h-24 rounded-full blur-2xl opacity-0 group-hover:opacity-10 transition-opacity duration-500 pointer-events-none', item.bgClass]"></div>
        </div>
      </div>

      <!-- 隐藏的文件输入 -->
      <input ref="fileInput" type="file" accept=".json" multiple @change="uploadFiles" style="display: none" />
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, nextTick, defineComponent, h } from 'vue'
import { useMessage, useDialog } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { DataCentreService } from '../services/DataCentreService'
import { useAuthorizationStore } from '../stores/Authorization'
import { useLayoutStore } from '../stores/LayoutStore'

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
const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('zh-CN', { dateStyle: 'full' }).format(date)
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
    setTimeout(() => { document.body.removeChild(a); URL.revokeObjectURL(url) }, 100)
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
          headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }
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

const fetchData = async () => {
  loading.value = true
  try {
    statistics.value = await DataCentreService.getCentreData()
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

      return () => h('div', { class: 'flex items-center gap-3' }, [
        // Upload Button - iOS Pill Style
        h('button', {
          class: 'flex items-center gap-2 px-4 py-1.5 rounded-full bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium transition-colors shadow-sm active:scale-95 duration-200',
          onClick: triggerFileInput
        }, [
          h(Icon, { icon: 'ion:cloud-upload', class: 'w-4 h-4' }),
          h('span', '导入数据')
        ]),

        // More Options Button & Dropdown
        h('div', { class: 'relative ios-dropdown-trigger' }, [
          h('button', {
            onClick: (e) => { e.stopPropagation(); isOpen.value = !isOpen.value },
            class: `w-8 h-8 rounded-full flex items-center justify-center transition-colors duration-200 ${isOpen.value ? 'bg-gray-200 dark:bg-gray-700' : 'bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700'}`
          }, [
            h(Icon, { icon: 'ion:ellipsis-horizontal', class: 'text-gray-600 dark:text-gray-300' })
          ]),

          // Dropdown Menu (Glassmorphism look handled by CSS classes below)
          h('div', {
            class: `absolute right-0 mt-3 w-48 py-1 rounded-xl apple-dropdown origin-top-right transition-all duration-200 ${isOpen.value ? 'opacity-100 scale-100 visible' : 'opacity-0 scale-95 invisible'}`
          }, [
            h('div', { class: 'px-1 py-1' }, [
              h('button', {
                onClick: () => { isOpen.value = false; downloadAllData() },
                class: 'w-full text-left px-3 py-2 text-sm rounded-lg text-gray-700 dark:text-gray-200 hover:bg-blue-50 dark:hover:bg-blue-900/30 hover:text-blue-600 transition-colors flex items-center gap-2'
              }, [
                h(Icon, { icon: 'ion:download-outline', class: 'w-4 h-4' }),
                h('span', '导出备份')
              ]),
              h('div', { class: 'h-px bg-gray-100 dark:bg-gray-700 my-1 mx-2' }), // Divider
              h('button', {
                onClick: () => { isOpen.value = false; removeAllData() },
                class: 'w-full text-left px-3 py-2 text-sm rounded-lg text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors flex items-center gap-2'
              }, [
                h(Icon, { icon: 'ion:trash-outline', class: 'w-4 h-4' }),
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
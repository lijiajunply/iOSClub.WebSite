<template>
  <div class="min-h-screen bg-[#F5F5F7] dark:bg-black text-slate-900 dark:text-slate-100 transition-colors duration-300 selection:bg-blue-500/30">
    <!-- 顶部导航/欢迎区 -->
    <header class="sticky top-0 z-20 px-6 py-4 sm:px-8 lg:px-12 glass-effect border-b border-slate-200/50 dark:border-neutral-800/50 flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold tracking-tight -mb-1">控制中心</h1>
        <p class="text-xs font-medium text-slate-500 dark:text-slate-400 mt-1">
          {{ welcomeMessage }}，{{ userInfo.name }}
        </p>
      </div>
      <div class="flex items-center gap-4">

      </div>
    </header>

    <main class="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">

      <!-- 顶部主要信息区：Bento Grid 布局 -->
      <div class="grid grid-cols-1 md:grid-cols-12 gap-6">

        <!-- 个人卡片 (占比较大) -->
        <div class="md:col-span-12 lg:col-span-4 ios-card relative overflow-hidden group">
          <div class="absolute inset-0 bg-gradient-to-br from-blue-50 via-transparent to-purple-50 dark:from-blue-900/20 dark:via-transparent dark:to-purple-900/20 opacity-50"></div>
          <div class="relative z-10 flex flex-col h-full p-6 justify-between">
            <div class="flex items-start justify-between">
               <div class="flex flex-col">
                  <span class="text-sm font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">{{ getRoleText(userInfo.role) }}</span>
                  <h2 class="text-3xl font-bold mt-1">{{ userInfo.name }}</h2>
                  <p class="text-xs text-slate-400 mt-1 font-mono">ID: {{ userInfo.id }}</p>
               </div>
               <div class="bg-white/80 dark:bg-neutral-800/80 backdrop-blur-md p-1 rounded-full shadow-sm">
                 <Icon icon="solar:verified-check-bold" class="w-6 h-6 text-blue-500" />
               </div>
            </div>

            <div class="mt-8">
              <button
                @click="goToPersonalData"
                class="w-full py-2.5 rounded-xl bg-slate-900 dark:bg-white text-white dark:text-black font-medium text-sm hover:scale-[1.02] transition-transform active:scale-95 shadow-lg shadow-slate-200 dark:shadow-none flex items-center justify-center gap-2"
              >
                <Icon icon="solar:pen-new-square-linear" class="w-4 h-4" />
                编辑 iMember 资料
              </button>
            </div>
          </div>
        </div>

        <!-- 统计数据 (EChart + 关键指标) -->
        <div v-if="userInfo.isAdmin" class="md:col-span-12 lg:col-span-8 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
           <!-- 成员概览 -->
           <div class="ios-card p-5 flex flex-col justify-between hover:scale-[1.02] transition-all cursor-pointer" @click="router.push('/Centre/MemberData')">
             <div class="flex items-center justify-between mb-2">
                <div class="p-2 bg-indigo-100 dark:bg-indigo-500/20 rounded-lg text-indigo-600 dark:text-indigo-400">
                  <Icon icon="solar:users-group-rounded-bold-duotone" class="w-6 h-6" />
                </div>
                <span class="text-xs font-medium text-green-500 bg-green-100 dark:bg-green-900/30 px-2 py-0.5 rounded-full">+2.4%</span>
             </div>
             <div>
               <div class="text-sm text-slate-500 dark:text-slate-400 font-medium">总成员</div>
               <div class="text-2xl font-bold mt-0.5">{{ statistics.members }}</div>
             </div>
           </div>

           <!-- 项目概览 -->
           <div class="ios-card p-5 flex flex-col justify-between hover:scale-[1.02] transition-all cursor-pointer" @click="router.push('/Centre/Department')">
             <div class="flex items-center justify-between mb-2">
                <div class="p-2 bg-orange-100 dark:bg-orange-500/20 rounded-lg text-orange-600 dark:text-orange-400">
                  <Icon icon="solar:folder-with-files-bold-duotone" class="w-6 h-6" />
                </div>
             </div>
             <div>
               <div class="text-sm text-slate-500 dark:text-slate-400 font-medium">进行中项目</div>
               <div class="text-2xl font-bold mt-0.5">{{ statistics.projects }}</div>
             </div>
           </div>

           <!-- 任务完成率 (EChart Mini) -->
           <div class="ios-card p-5 flex flex-col justify-between sm:col-span-2 md:col-span-1 hover:scale-[1.02] transition-all">
              <div class="flex justify-between items-start">
                 <div class="text-sm text-slate-500 dark:text-slate-400 font-medium">任务完成率</div>
                 <Icon icon="solar:chart-2-bold-duotone" class="w-5 h-5 text-slate-400" />
              </div>
              <div class="flex-1 flex items-center justify-center h-20 w-full" ref="chartContainer">
                 <!-- Chart rendered here -->
              </div>
              <div class="text-center text-xs font-medium text-slate-400">{{ statistics.tasks }} 个待办任务</div>
           </div>
        </div>
      </div>

      <!-- 工具栏 (横向滚动) -->
      <section>
        <div class="flex items-center justify-between mb-4 px-2">
          <h3 class="text-lg font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
            <Icon icon="solar:widget-5-bold-duotone" class="text-blue-500" /> iTools
          </h3>
        </div>
        <div class="ios-card p-1 overflow-hidden">
          <div v-if="loading.tools" class="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-6 gap-4 p-4">
             <div v-for="n in 6" :key="n" class="h-24 bg-slate-100 dark:bg-neutral-800 animate-pulse rounded-xl"></div>
          </div>
          <div v-else class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3 p-3">
            <div
              v-for="(tool, index) in tools" :key="index"
              @click="openTool(tool.url)"
              class="group flex flex-col items-center justify-center p-4 rounded-xl transition-all cursor-pointer"
            >
              <div class="w-14 h-14 rounded-2xl flex items-center justify-center mb-3 shadow-sm bg-white dark:bg-neutral-800 border border-slate-100 dark:border-neutral-700 group-hover:scale-110 transition-transform duration-300">
                 <template v-if="tool.icon && !tool.icon.startsWith('http')">
                    <IconFont :type="tool.icon" className="w-8 h-8 text-slate-700 dark:text-slate-200" />
                 </template>
                 <img v-else :src="fixImageUrl(tool)" class="w-8 h-8 object-contain rounded" :alt="tool.name" />
              </div>
              <span class="text-xs font-medium text-center line-clamp-1 text-slate-600 dark:text-slate-300 group-hover:text-blue-600 dark:group-hover:text-blue-400">{{ tool.name }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- 任务与资源 (Grid) -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">

        <!-- 我的任务列表 -->
        <section class="ios-card flex flex-col h-[400px]">
          <div class="rounded-t-3xl p-5 border-b border-slate-100 dark:border-neutral-800 flex justify-between items-center bg-white/50 dark:bg-neutral-800/50 backdrop-blur-sm sticky top-0 z-10">
            <div class="flex items-center gap-2">
              <div class="w-1.5 h-5 rounded-full bg-blue-500"></div>
              <h3 class="font-bold text-lg">提醒事项</h3>
            </div>
            <n-tag size="small" round class="!bg-slate-100 dark:!bg-neutral-700 !text-slate-500">
              {{ tasks.length }} 个待办
            </n-tag>
          </div>

          <div class="flex-1 overflow-y-auto p-2 custom-scrollbar">
             <div v-if="loading.tasks" class="space-y-3 p-3">
                <div v-for="i in 3" :key="i" class="h-16 bg-slate-100 dark:bg-neutral-800 rounded-xl animate-pulse"></div>
             </div>
             <div v-else-if="tasks.length === 0" class="h-full flex flex-col items-center justify-center text-slate-400">
                <Icon icon="solar:checklist-minimalistic-linear" class="w-12 h-12 mb-2 opacity-50" />
                <p class="text-sm">无待办事项</p>
             </div>
             <div v-else class="space-y-2">
               <div v-for="task in tasks" :key="task.id" class="group flex items-start gap-3 p-3 rounded-xl hover:bg-slate-50 dark:hover:bg-neutral-800/50 transition-colors">
                  <div class="mt-1">
                    <div
                      class="w-5 h-5 rounded-full border-2 flex items-center justify-center cursor-pointer transition-colors"
                      :class="task.status ? 'bg-blue-500 border-blue-500' : 'border-slate-300 dark:border-slate-600 hover:border-blue-400'"
                    >
                      <Icon v-if="task.status" icon="solar:check-read-linear" class="text-white w-3 h-3" />
                    </div>
                  </div>
                  <div class="flex-1 min-w-0">
                    <h4 class="text-sm font-medium truncate" :class="{'line-through text-slate-400': task.status}">{{ task.title }}</h4>
                    <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5 line-clamp-1">{{ task.description || '无详细描述' }}</p>
                    <div class="flex items-center gap-2 mt-1.5">
                       <span class="text-[10px] px-1.5 py-0.5 rounded bg-slate-100 dark:bg-neutral-700 text-slate-500">
                         {{ formatDateSimple(task.startTime) }}
                       </span>
                    </div>
                  </div>
               </div>
             </div>
          </div>
        </section>

        <!-- 社团资源 / 部门管理 -->
        <section class="grid grid-rows-2 gap-6 h-[400px]">

          <!-- 资源快捷入口 -->
          <div
            v-if="userInfo.isAdmin"
            @click="goToResources"
            class="ios-card p-6 relative overflow-hidden group cursor-pointer hover:ring-2 ring-blue-500/30 transition-all"
          >
            <div class="absolute right-[-20px] top-[-20px] w-32 h-32 bg-gradient-to-br from-purple-400/20 to-blue-400/20 blur-3xl rounded-full transition-transform group-hover:scale-150"></div>
            <div class="relative z-10 h-full flex flex-col justify-between">
              <div class="flex items-center gap-3">
                <div class="p-2.5 bg-purple-100 dark:bg-purple-500/20 rounded-xl text-purple-600 dark:text-purple-300">
                  <Icon icon="solar:cloud-storage-bold-duotone" class="w-7 h-7" />
                </div>
                <div>
                  <h3 class="font-bold text-lg">资源云盘</h3>
                  <p class="text-xs text-slate-500 dark:text-slate-400">共 {{ statistics.resources }} 个文件</p>
                </div>
              </div>
              <div class="flex items-center text-sm font-medium text-purple-600 dark:text-purple-400 mt-4">
                浏览所有资源 <Icon icon="solar:alt-arrow-right-linear" class="ml-1 group-hover:translate-x-1 transition-transform" />
              </div>
            </div>
          </div>

          <!-- 部门入口 -->
          <div
            v-if="userInfo.isAdmin"
            @click="goToDepartment"
            class="ios-card p-6 relative overflow-hidden group cursor-pointer hover:ring-2 ring-blue-500/30 transition-all"
          >
             <!-- 背景装饰 -->
             <div class="absolute inset-0 bg-[url('https://assets.codepen.io/1462889/pattern-1.svg')] opacity-[0.03] dark:opacity-[0.07]"></div>

             <div class="relative z-10 flex items-center justify-between h-full">
               <div class="flex items-center gap-4">
                 <div class="flex -space-x-3">
                    <!-- 模拟头像堆叠 -->
                    <div class="w-10 h-10 rounded-full border-2 border-white dark:border-neutral-900 bg-blue-100 flex items-center justify-center text-xs font-bold">S</div>
                    <div class="w-10 h-10 rounded-full border-2 border-white dark:border-neutral-900 bg-green-100 flex items-center justify-center text-xs font-bold">H</div>
                    <div class="w-10 h-10 rounded-full border-2 border-white dark:border-neutral-900 bg-pink-100 flex items-center justify-center text-xs font-bold">M</div>
                    <div class="w-10 h-10 rounded-full border-2 border-white dark:border-neutral-900 bg-slate-100 dark:bg-neutral-800 flex items-center justify-center text-xs text-slate-500">+{{ statistics.departments }}</div>
                 </div>
                 <div>
                    <h3 class="font-bold text-lg">部门与组织</h3>
                    <p class="text-xs text-slate-500 dark:text-slate-400">管理架构与权限</p>
                 </div>
               </div>
               <div class="h-10 w-10 rounded-full bg-slate-100 dark:bg-neutral-800 flex items-center justify-center group-hover:bg-blue-500 group-hover:text-white transition-colors">
                 <Icon icon="solar:settings-minimalistic-bold-duotone" class="w-6 h-6" />
               </div>
             </div>
          </div>
        </section>
      </div>

    </main>

    <!-- 全局背景装饰 -->
    <div class="fixed top-0 left-0 w-full h-full pointer-events-none -z-10 overflow-hidden">
       <div class="absolute top-[-10%] right-[-5%] w-[500px] h-[500px] bg-blue-400/10 rounded-full blur-[100px]"></div>
       <div class="absolute bottom-[-10%] left-[-5%] w-[600px] h-[600px] bg-purple-400/10 rounded-full blur-[120px]"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { NTag } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { ToolService } from '../services/ToolService'
import { UserService } from '../services/UserService'
import { ProjectService } from '../services/ProjectService'
import { DataCentreService } from "../services/DataCentreService"
import IconFont from "../components/IconFont.vue"
import * as echarts from 'echarts'
import '//at.alicdn.com/t/c/font_4612528_md4hjwjgcb.js';

const router = useRouter()

// --- Interfaces ---
interface UserInfo {
  name: string
  id: string
  role: string
  isAdmin: boolean
  gender: string
}

interface Tool {
  id?: number
  name: string
  icon?: string
  url: string
  description?: string
}

interface Task {
  id: number
  title: string
  description: string
  startTime: string
  endTime: string
  status: boolean
}

interface Statistics {
  members: number
  staffs: number
  projects: number
  tasks: number
  resources: number
  departments: number
}

// --- State ---
const userInfo = ref<UserInfo>({
  name: '',
  id: '',
  role: '',
  isAdmin: false,
  gender: '男'
})

const tools = ref<Tool[]>([])
const tasks = ref<Task[]>([])
const statistics = ref<Statistics>({
  members: 0,
  staffs: 0,
  projects: 0,
  tasks: 0,
  resources: 0,
  departments: 0
})

const loading = ref({
  tools: true,
  tasks: true,
  resources: true,
  departments: true,
  statistics: true
})

const chartContainer = ref<HTMLElement | null>(null)

// --- Computed ---
const welcomeMessage = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6) return '凌晨好'
  if (hour < 12) return '早上好'
  if (hour < 18) return '下午好'
  return '晚上好'
})

// --- Methods ---

const getRoleText = (role: string): string => {
  const map: Record<string, string> = {
    'Founder': '创始人',
    'President': '社长/团支书',
    'Minister': '部长/副部长',
    'Department': '部员',
    'Member': 'Member', // 英文看起来更简洁
  }
  return map[role] || role
}

const formatDateSimple = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return `${date.getMonth() + 1}月${date.getDate()}日`
}

const getUserAvatar = () => {
  // 假设这里是图片逻辑，实际需根据项目路径调整
  if (userInfo.value.gender === '男') {
    return new URL('/assets/Centre/男生.png', import.meta.url).href
  } else {
    return new URL('/assets/Centre/女生.png', import.meta.url).href
  }
}

const fixImageUrl = (tool: Tool) => {
   if (tool.icon) {
    return tool.icon.replace(/([^:])(\/\/)/g, '$1/')
  }
  try {
    const domain = tool.url.replace("https://", "").replace("http://", "").split('/')[0];
    return `https://${domain}/favicon.ico`;
  } catch {
    return ''
  }
}

// --- Actions ---
const goToPersonalData = () => router.push('/Centre/PersonalData')
const goToResources = () => router.push('/Centre/Resources')
const goToDepartment = () => router.push('/Centre/Department')
const openTool = (url: string) => window.open(url, '_blank')

// --- ECharts ---
const initChart = () => {
  if (!chartContainer.value) return

  const myChart = echarts.init(chartContainer.value)
  const option = {
    tooltip: { trigger: 'item' },
    series: [
      {
        name: 'Access From',
        type: 'pie',
        radius: ['60%', '90%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 5,
          borderColor: '#fff',
          borderWidth: 1
        },
        label: { show: false },
        emphasis: { label: { show: false } },
        labelLine: { show: false },
        data: [
          { value: 1048, name: 'Done', itemStyle: { color: '#3B82F6' } }, // Blue
          { value: 735, name: 'Todo', itemStyle: { color: '#E2E8F0' } },  // Slate-200
        ]
      }
    ]
  }

  // 适配深色模式（简单示例，实际需监听主题变化）
  const isDark = document.documentElement.classList.contains('dark')
  if (isDark) {
      option.series[0].itemStyle.borderColor = '#171717'
      option.series[0].data[1].itemStyle.color = '#404040'
  }

  myChart.setOption(option)

  window.addEventListener('resize', () => myChart.resize())
}

// --- Data Fetching ---
// 这里保留原有逻辑，仅做 TS 类型适配和轻微简化
const fetchTools = async () => {
  try {
    loading.value.tools = true
    const res = await ToolService.getTools()
    tools.value = res.links || []
  } catch (error) {
    // Mock Data
    tools.value = [
      { id: 1, name: 'iLibrary', icon: 'solar:book-bookmark-linear', url: 'https://ilibrary.xauat.site' },
      { id: 2, name: '官网', icon: 'solar:global-linear', url: 'https://www.xauat.site' },
      { id: 3, name: 'AI Platform', icon: 'solar:magic-stick-3-linear', url: 'https://iosai.xauat.site' },
      { id: 4, name: '导航', icon: 'solar:map-point-linear', url: '#' },
      { id: 5, name: 'GitHub', icon: 'mdi:github', url: '#' },
    ]
  } finally {
    loading.value.tools = false
  }
}

const fetchUserInfo = async () => {
  try {
    const userData = await UserService.getUserData()
    userInfo.value = {
      name: userData.userName,
      id: userData.userId,
      role: userData.identity || '普通成员',
      isAdmin: ['Founder', 'President', 'Minister'].includes(userData.identity),
      gender: userData.gender || '男'
    }
  } catch (error) {
    userInfo.value = { name: 'luckyfish', id: '220202', role: 'Founder', isAdmin: true, gender: '男' }
  }
}

const fetchTasks = async () => {
  try {
    loading.value.tasks = true
    const todoData = await ProjectService.getYourTasks()
    tasks.value = todoData.map((task: any) => ({
       id: task.id,
       title: task.name,
       description: task.description,
       startTime: task.startTime,
       endTime: task.endTime,
       status: task.status === 'Done'
    }))
  } catch (error) {
     tasks.value = [
         { id: 1, title: '审核纳新申请', description: '2024秋季招新', startTime: '2023-10-01', endTime: '', status: false },
         { id: 2, title: '部署新官网', description: 'Frontend v3.0', startTime: '2023-10-05', endTime: '', status: true },
         { id: 3, title: '服务器维护', description: 'SSL证书更新', startTime: '2023-09-28', endTime: '', status: false }
     ]
  } finally {
    loading.value.tasks = false
  }
}

const fetchStatistics = async () => {
    try {
        loading.value.statistics = true
        statistics.value = await DataCentreService.getCentreData()
    } catch (e) {
        statistics.value = { members: 128, staffs: 42, projects: 15, tasks: 36, resources: 24, departments: 4}
    } finally {
        loading.value.statistics = false
        // 数据加载完后初始化图表
        nextTick(() => initChart())
    }
}

onMounted(async () => {
  await Promise.all([
    fetchTools(),
    fetchUserInfo(),
    fetchTasks(),
    fetchStatistics()
  ])
})

</script>

<style scoped>
/* 核心卡片样式：模仿 iOS 18 / macOS */
.ios-card {
  background-color: #ffffff;
  border-radius: 1.5rem;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(226, 232, 240, 0.6);
  transition: all 0.3s ease;
}

.dark .ios-card {
  background-color: #171717;
  border: 1px solid #262626;
}

/* 磨砂玻璃效果 */
.glass-effect {
  background-color: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  filter: saturate(1.5);
}

.dark .glass-effect {
  background-color: rgba(0, 0, 0, 0.7);
}

/* 自定义滚动条 */
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #e2e8f0;
  border-radius: 9999px;
}
.dark .custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #404040;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
</style>
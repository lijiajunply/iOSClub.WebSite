<template>
  <div class="min-h-screen bg-white/90 dark:bg-gray-800/90 transition-colors duration-300">
    <!-- 主内容区域 -->
    <div class="p-4 md:p-6">
      <!-- 页面标题 -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">管理中心</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">社团管理系统数据分析</p>
        </div>

        <div class="flex items-center space-x-2">
          <button
              v-if="userInfo.isAdmin"
              @click="triggerFileInput"
              class="hidden sm:flex items-center px-4 py-2 bg-blue-500 text-white rounded-full hover:bg-blue-600 transition-colors text-sm"
          >
            <Icon icon="material-symbols:upload" class="mr-1" width="20" height="20" />
            上传数据
          </button>

          <div v-if="userInfo.isAdmin" class="relative" ref="dropdownContainer">
            <button
                @click="toggleDropdown"
                class="h-9 w-9 rounded-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            >
              <Icon icon="material-symbols:more-horiz" width="20" height="20" class="text-gray-700 dark:text-gray-300" />
            </button>
            
            <div 
                v-if="dropdownOpen" 
                class="absolute right-0 mt-2 w-48 bg-white dark:bg-gray-700 rounded-lg shadow-lg py-1 z-10 border border-gray-200 dark:border-gray-600"
            >
              <button
                  @click="handleDropdownSelect('download')"
                  class="block w-full text-left px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-600"
              >
                下载所有数据
              </button>
              <button
                  @click="handleDropdownSelect('remove')"
                  class="block w-full text-left px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-gray-100 dark:hover:bg-gray-600"
              >
                删除所有数据
              </button>
              <button
                  @click="handleDropdownSelect('upload')"
                  class="block w-full text-left px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-600"
              >
                上传Json数据
              </button>
            </div>
          </div>

          <input
              ref="fileInput"
              type="file"
              accept=".json"
              multiple
              @change="uploadFiles"
              style="display: none"
          />
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="space-y-8">
        <!-- 数据概览卡片骨架 -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-8">
          <SkeletonLoader v-for="i in 7" :key="i" type="card" />
        </div>

        <!-- 图表区域骨架 -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          <SkeletonLoader v-for="i in 2" :key="i" type="chart" />
        </div>
      </div>

      <!-- 数据概览卡片 -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-8">
        <StatCard
            title="社员人数"
            :value="studentsCount"
            icon="material-symbols:people"
            color="blue"
        />
        <StatCard
            title="部员人数"
            :value="staffs.length"
            icon="material-symbols:badge"
            color="green"
        />
        <StatCard
            title="项目数"
            :value="projectsCount"
            icon="material-symbols:folder"
            color="purple"
        />
        <StatCard
            title="任务数"
            :value="tasksCount"
            icon="material-symbols:task"
            color="amber"
        />
        <StatCard
            title="资源数"
            :value="resourcesCount"
            icon="material-symbols:folder-open"
            color="pink"
        />
        <StatCard
            title="部门数"
            :value="departmentsCount"
            icon="material-symbols:groups"
            color="indigo"
        />
        <StatCard
            title="待办数"
            :value="todosCount"
            icon="material-symbols:check-box-outline-blank"
            color="red"
        />
      </div>

      <!-- 图表区域 -->
      <div v-if="!loading" class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <!-- 成员分布图表 -->
        <div class="rounded-2xl overflow-hidden bg-white dark:bg-gray-700/50 backdrop-blur-sm border border-gray-200 dark:border-gray-700 shadow-sm transition-colors duration-300">
          <div class="p-4 border-b border-gray-100 dark:border-gray-700">
            <h3 class="font-medium text-gray-900 dark:text-white">成员分布</h3>
          </div>
          <div class="p-4">
            <div ref="memberChartRef" class="h-72"></div>
          </div>
        </div>

        <!-- 项目进度图表 -->
        <div class="rounded-2xl overflow-hidden bg-white dark:bg-gray-700/50 backdrop-blur-sm border border-gray-200 dark:border-gray-700 shadow-sm transition-colors duration-300">
          <div class="p-4 border-b border-gray-100 dark:border-gray-700">
            <h3 class="font-medium text-gray-900 dark:text-white">项目概览</h3>
          </div>
          <div class="p-4">
            <div ref="projectChartRef" class="h-72"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount, nextTick, watch} from 'vue'
import {useMessage, useDialog} from 'naive-ui'
import {Icon} from '@iconify/vue'
import SkeletonLoader from '../components/SkeletonLoader.vue'
import * as echarts from 'echarts'
import StatCard from '../components/StatCard.vue'
import {MemberQueryService} from '../services/MemberQueryService'
import {ProjectService} from '../services/ProjectService'
import {ResourceService} from '../services/ResourceService'
import {DepartmentService} from '../services/DepartmentService'
import {TodoService} from '../services/TodoService'

const message = useMessage()
const dialog = useDialog()

// Refs
const fileInput = ref<HTMLInputElement>()
const memberChartRef = ref<HTMLElement>()
const projectChartRef = ref<HTMLElement>()
const dropdownContainer = ref<HTMLElement>()
let memberChart: echarts.ECharts | null = null
let projectChart: echarts.ECharts | null = null
const dropdownOpen = ref(false)
const loading = ref(false)

// Data state
const studentsCount = ref(0)
const staffs = ref<any[]>([])
const projectsCount = ref(0)
const tasksCount = ref(0)
const resourcesCount = ref(0)
const departmentsCount = ref(0)
const todosCount = ref(0)
const userInfo = ref({
  isAdmin: false
})

// Identity dictionary mapping
const identityDictionary: Record<string, string> = {
  'Founder': '创始人',
  'President': '社长/团支书',
  'Minister': '部长/副部长',
  'Department': '部员',
  'Member': '普通成员'
}

// File handling methods
const triggerFileInput = () => {
  fileInput.value?.click()
}

// Dropdown methods
const toggleDropdown = () => {
  dropdownOpen.value = !dropdownOpen.value
}

const handleDropdownSelect = (key: string) => {
  dropdownOpen.value = false
  switch (key) {
    case 'download':
      downloadAllData()
      break
    case 'remove':
      removeAllData()
      break
    case 'upload':
      triggerFileInput()
      break
  }
}

// Click outside to close dropdown
const handleClickOutside = (event: MouseEvent) => {
  if (dropdownContainer.value && !dropdownContainer.value.contains(event.target as Node)) {
    dropdownOpen.value = false
  }
}

// Upload files
const uploadFiles = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = target.files
  if (!files || !files.length) return

  try {
    for (let i = 0; i < files.length; i++) {
      const file = files[i]
      const reader = new FileReader()

      reader.onload = async (e) => {
        try {
          const result = e.target?.result
          if (!result) return

          const allData = JSON.parse(result as string)
          message.success(`文件 "${file.name}" 上传成功`)
        } catch (error) {
          console.error('解析文件时出错:', error)
          message.error(`解析文件 "${file.name}" 时出错`)
        }
      }

      reader.readAsText(file)
    }
  } catch (error) {
    console.error('上传文件时出错:', error)
    message.error('上传文件时出错')
  }
}

// Download all data
const downloadAllData = async () => {
  try {
    const token = localStorage.getItem('Authorization')
    if (!token) {
      message.error('未找到认证信息')
      return
    }

    const data = {
      students: [],
      presidents: [],
      tasks: [],
      projects: [],
      resources: [],
      departments: [],
      todos: [],
      articles: []
    }

    const jsonString = JSON.stringify(data, null, 2)
    const blob = new Blob([jsonString], {type: 'application/json'})
    const url = URL.createObjectURL(blob)

    const a = document.createElement('a')
    a.href = url
    a.download = 'allData.json'
    document.body.appendChild(a)
    a.click()

    setTimeout(() => {
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    }, 100)

    message.success('数据下载成功')
  } catch (error) {
    console.error('下载数据时出错:', error)
    message.error('下载数据时出错')
  }
}

// Remove all data
const removeAllData = () => {
  dialog.warning({
    title: '确认删除',
    content: '确定要删除所有数据吗？此操作不可撤销。',
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        const token = localStorage.getItem('Authorization')
        if (!token) {
          message.error('未找到认证信息')
          return
        }

        const response = await fetch('https://www.xauat.site/api/Admin/RemoveAllData', {
          method: 'DELETE',
          headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
          }
        })

        if (response.ok) {
          message.success('所有数据已删除')
          await fetchData()
        } else {
          message.error('删除数据失败')
        }
      } catch (error) {
        console.error('删除数据时出错:', error)
        message.error('删除数据时出错')
      }
    }
  })
}

// Fetch data
const fetchData = async () => {
  try {
    // 并行获取所有数据
    const [membersData, projectsData, resourcesData, departmentsData, todoStats] = await Promise.all([
      // 获取成员数据
      MemberQueryService.getAllDataByPage(1, 1),
      // 获取项目数据
      ProjectService.getAllProjects(),
      // 获取资源数据
      ResourceService.getAllResources(),
      // 获取部门数据
      DepartmentService.getAllDepartments(),
      // 获取待办统计数据
      TodoService.getTodoStatistics(),
    ])

    // 更新成员数据
    studentsCount.value = membersData.totalCount || 0

    // 更新项目和任务数据
    projectsCount.value = projectsData.length || 0
    let totalTasks = 0
    projectsData.forEach(project => {
      if (project.tasks && Array.isArray(project.tasks)) {
        totalTasks += project.tasks.length
      }
    })
    tasksCount.value = totalTasks

    // 更新资源数据
    resourcesCount.value = resourcesData.length || 0

    // 更新部门数据
    departmentsCount.value = departmentsData.length || 0

    // 更新待办数据
    todosCount.value = todoStats.total || 0
  } catch (error) {
    console.error('获取数据时出错:', error)
    message.error('获取数据时出错')
  }
}

// Update member chart
const updateMemberChart = () => {
  if (!memberChart) return

  // 统计成员身份分布
  const identityStats: Record<string, number> = {
    'Founder': 0,
    'President': 0,
    'Minister': 0,
    'Department': 0,
    'Member': 0
  }

  staffs.value.forEach(staff => {
    if (identityStats.hasOwnProperty(staff.identity)) {
      identityStats[staff.identity]++
    }
  })

  const isDark = document.documentElement.classList.contains('dark')
  const textColor = isDark ? '#e5e7eb' : '#374151'

  const option = {
    tooltip: {
      trigger: 'item',
      backgroundColor: isDark ? '#1f2937' : '#ffffff',
      borderColor: isDark ? '#374151' : '#e5e7eb',
      textStyle: {
        color: textColor
      }
    },
    legend: {
      bottom: '0%',
      left: 'center',
      textStyle: {
        color: textColor
      }
    },
    series: [
      {
        name: '成员分布',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: isDark ? '#1f2937' : '#ffffff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: '18',
            fontWeight: 'bold',
            color: textColor
          }
        },
        labelLine: {
          show: false
        },
        data: [
          {value: identityStats.Founder, name: identityDictionary.Founder, itemStyle: {color: '#f59e0b'}},
          {value: identityStats.President, name: identityDictionary.President, itemStyle: {color: '#3b82f6'}},
          {value: identityStats.Minister, name: identityDictionary.Minister, itemStyle: {color: '#10b981'}},
          {value: identityStats.Department, name: identityDictionary.Department, itemStyle: {color: '#8b5cf6'}},
          {value: identityStats.Member, name: identityDictionary.Member, itemStyle: {color: '#6b7280'}}
        ].filter(item => item.value > 0)
      }
    ]
  }

  memberChart.setOption(option)
}

// Update project chart
const updateProjectChart = () => {
  if (!projectChart) return

  const isDark = document.documentElement.classList.contains('dark')
  const textColor = isDark ? '#e5e7eb' : '#374151'
  const backgroundColor = isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.05)'

  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      },
      backgroundColor: isDark ? '#1f2937' : '#ffffff',
      borderColor: isDark ? '#374151' : '#e5e7eb',
      textStyle: {
        color: textColor
      }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: [
      {
        type: 'category',
        data: ['总项目', '任务总数', '待完成'],
        axisLine: {
          lineStyle: {
            color: isDark ? '#374151' : '#e5e7eb'
          }
        },
        axisLabel: {
          color: textColor
        }
      }
    ],
    yAxis: [
      {
        type: 'value',
        axisLine: {
          lineStyle: {
            color: isDark ? '#374151' : '#e5e7eb'
          }
        },
        axisLabel: {
          color: textColor
        },
        splitLine: {
          lineStyle: {
            color: backgroundColor
          }
        }
      }
    ],
    series: [
      {
        name: '数量',
        type: 'bar',
        barWidth: '60%',
        data: [
          {value: projectsCount.value, itemStyle: {color: '#8b5cf6'}},
          {value: tasksCount.value, itemStyle: {color: '#3b82f6'}},
          {value: todosCount.value, itemStyle: {color: '#f59e0b'}}
        ],
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        }
      }
    ]
  }

  projectChart.setOption(option)
}

// Initialize charts
const initCharts = () => {
  if (memberChartRef.value) {
    memberChart = echarts.init(memberChartRef.value)
    updateMemberChart()
  }

  if (projectChartRef.value) {
    projectChart = echarts.init(projectChartRef.value)
    updateProjectChart()
  }
}

// Handle theme change
const handleThemeChange = () => {
  if (memberChart) {
    updateMemberChart()
  }
  if (projectChart) {
    updateProjectChart()
  }
}

// Handle window resize
const handleResize = () => {
  if (memberChart) {
    memberChart.resize()
  }
  if (projectChart) {
    projectChart.resize()
  }
}

// Mount and unmount lifecycle
onMounted(async () => {
  await fetchData()
  await nextTick()
  initCharts()

  // Listen for theme changes
  const observer = new MutationObserver(handleThemeChange)
  observer.observe(document.documentElement, {attributes: true, attributeFilter: ['class']})

  // Listen for window resize
  window.addEventListener('resize', handleResize)
  
  // Add click outside listener
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  if (memberChart) {
    memberChart.dispose()
  }
  if (projectChart) {
    projectChart.dispose()
  }
  window.removeEventListener('resize', handleResize)
  document.removeEventListener('click', handleClickOutside)
})

// Watch for data changes to update charts
watch([studentsCount, staffs, projectsCount, tasksCount, todosCount], () => {
  updateMemberChart()
  updateProjectChart()
})
</script>

<style scoped>
/* Custom styles if needed */
</style>
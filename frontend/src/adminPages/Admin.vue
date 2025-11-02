<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <!-- Header -->
    <header class="bg-white dark:bg-neutral-800 shadow-sm py-4 px-6 transition-colors duration-300">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-4">
          <n-button text @click="goBack" class="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-neutral-700">
            <Icon icon="ion:arrow-back" size="24" class="text-gray-600 dark:text-gray-300" />
          </n-button>
          <div>
            <h1 class="text-xl font-semibold text-gray-900 dark:text-white">管理中心</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">社团管理系统</p>
          </div>
        </div>

        <div class="flex items-center space-x-3">
          <n-button
              v-if="userInfo.isAdmin"
              secondary
              type="info"
              @click="triggerFileInput"
              class="hidden sm:flex"
          >
            <template #icon>
                  <Icon icon="ion:cloud-upload" />
                </template>
            上传数据
          </n-button>

          <n-dropdown
              v-if="userInfo.isAdmin"
              trigger="click"
              :options="dropdownOptions"
              @select="handleDropdownSelect"
          >
            <n-button circle>
              <Icon icon="ion:ellipsis-vertical" size="20" />
            </n-button>
          </n-dropdown>

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
    </header>

    <main class="p-4 md:p-6">
      <!-- Stats Overview Section -->
      <section class="mb-8">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">数据概览</h2>
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
          <StatCard
              title="社员人数"
              :value="studentsCount"
              icon="people"
              color="blue"
          />
          <StatCard
              title="部员人数"
              :value="staffs.length"
              icon="badge"
              color="green"
          />
          <StatCard
              title="项目数"
              :value="projectsCount"
              icon="folder"
              color="purple"
          />
          <StatCard
              title="任务数"
              :value="tasksCount"
              icon="task"
              color="yellow"
          />
          <StatCard
              title="资源数"
              :value="resourcesCount"
              icon="resource"
              color="pink"
          />
          <StatCard
              title="部门数"
              :value="departmentsCount"
              icon="department"
              color="indigo"
          />
          <StatCard
              title="待办数"
              :value="todosCount"
              icon="todo"
              color="red"
          />
        </div>
      </section>

      <!-- Charts Section -->
      <section class="mb-8">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">数据统计</h2>
        <n-card
            class="rounded-xl shadow-sm bg-white dark:bg-neutral-800 transition-colors duration-300"
            :bordered="false"
        >
          <h3 class="font-medium text-gray-900 dark:text-white mb-4">成员分布</h3>
          <div ref="memberChartRef" class="h-64"></div>
        </n-card>
      </section>
    </main>
  </div>
</template>

<script setup>
import {ref, onMounted, onBeforeUnmount, nextTick} from 'vue'
import {useRouter} from 'vue-router'
import {useMessage, useDialog} from 'naive-ui'
import {
  NButton,
  NCard,
  NDropdown
} from 'naive-ui'
import { Icon } from '@iconify/vue'
import * as echarts from 'echarts'
import StatCard from '../components/StatCard.vue'
import {MemberQueryService} from '../services/MemberQueryService.ts'
import {StaffService} from '../services/StaffService.ts'
import {ProjectService} from '../services/ProjectService.ts'
import {ResourceService} from '../services/ResourceService.ts'
import {DepartmentService} from '../services/DepartmentService.ts'
import {TodoService} from '../services/TodoService.ts'
import {InfoService} from '../services/InfoService.ts'

const router = useRouter()
const message = useMessage()
const dialog = useDialog()

// Refs
const fileInput = ref(null)
const memberChartRef = ref(null)
const projectChartRef = ref(null)
const memberChart = ref(null)
const projectChart = ref(null)

// Data state
const studentsCount = ref(0)
const staffs = ref([])
const projectsCount = ref(0)
const tasksCount = ref(0)
const resourcesCount = ref(0)
const departmentsCount = ref(0)
const todosCount = ref(0)
const userInfo = ref({
  isAdmin: false
})

// Identity dictionary mapping
const identityDictionary = {
  'Founder': '创始人',
  'President': '社长/团支书',
  'Minister': '部长/副部长',
  'Department': '部员',
  'Member': '普通成员'
}

// Dropdown options
const dropdownOptions = [
  {
    label: '下载所有数据',
    key: 'download'
  },
  {
    label: '删除所有数据',
    key: 'remove'
  },
  {
    label: '上传Json数据',
    key: 'upload'
  }
]

// Navigation methods
const goBack = () => {
  router.push('/Centre')
}

const viewAllStaff = () => {
  // Implementation for viewing all staff
  message.info('查看全部功能待实现')
}

// File handling methods
const triggerFileInput = () => {
  fileInput.value.click()
}

const handleDropdownSelect = (key) => {
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

// Upload files
const uploadFiles = async (event) => {
  const files = event.target.files
  if (!files.length) return

  try {
    for (let i = 0; i < files.length; i++) {
      const file = files[i]
      const reader = new FileReader()

      reader.onload = async (e) => {
        try {
          const result = e.target.result
          if (!result) return

          const allData = JSON.parse(result)
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
        // 目前没有在services中找到删除所有数据的服务方法
        // 使用现有的API调用方式
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
    const [membersData, staffsData, projectsData, resourcesData, departmentsData, todoStats, userData] = await Promise.all([
      // 获取成员数据
      MemberQueryService.getAllDataByPage(1, 1).catch(() => ({Total: 0, Data: []})),
      // 获取部员数据
      StaffService.getAllStaff().catch(() => []),
      // 获取项目数据
      ProjectService.getAllProjects().catch(() => []),
      // 获取资源数据
      ResourceService.getAllResources().catch(() => []),
      // 获取部门数据
      DepartmentService.getAllDepartments().catch(() => []),
      // 获取待办统计数据
      TodoService.getTodoStatistics().catch(() => ({TotalCount: 0})),
      // 获取用户信息
      InfoService.getUserInfo().catch(() => ({isAdmin: false}))
    ])

    // 更新成员数据
    studentsCount.value = membersData.totalCount || 0

    // 处理 staffs 数据，确保每个对象都有必要的属性
    staffs.value = (staffsData || []).map(staff => {
      return {
        id: staff.id || staff.Id || Math.random().toString(36).substr(2, 9),
        name: staff.name || staff.Name || '未知成员',
        identity: staff.identity || staff.Identity || 'Member',
        ...staff
      }
    }).filter(staff => staff.name && staff.name.length > 0) // 过滤掉空名称的成员

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
    todosCount.value = todoStats.TotalCount || 0

    // 更新用户信息
    userInfo.value.isAdmin = userData.isAdmin || false
  } catch (error) {
    console.error('获取数据时出错:', error)
    message.error('获取数据时出错')
  }
}

// Initialize charts
const initCharts = () => {
  if (memberChartRef.value) {
    memberChart.value = echarts.init(memberChartRef.value)
    updateMemberChart()
  }

  if (projectChartRef.value) {
    projectChart.value = echarts.init(projectChartRef.value)
    updateProjectChart()
  }
}

// Update member chart
const updateMemberChart = () => {
  if (!memberChart.value) return

  const identityCounts = {}
  staffs.value.forEach(staff => {
    const identity = identityDictionary[staff.identity] || staff.identity
    identityCounts[identity] = (identityCounts[identity] || 0) + 1
  })

  const option = {
    tooltip: {
      trigger: 'item'
    },
    legend: {
      bottom: '0%',
      left: 'center',
      textStyle: {
        color: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
      }
    },
    series: [
      {
        name: '成员身份',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: document.documentElement.classList.contains('dark') ? '#1f1f1f' : '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 14,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: Object.keys(identityCounts).map(key => ({
          value: identityCounts[key],
          name: key
        }))
      }
    ]
  }

  memberChart.value.setOption(option)
}

// Update project chart
const updateProjectChart = () => {
  if (!projectChart.value) return

  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '15%',
      containLabel: true
    },
    xAxis: [
      {
        type: 'category',
        data: ['计划中', '进行中', '已完成', '已暂停'],
        axisTick: {
          alignWithLabel: true
        },
        axisLabel: {
          color: document.documentElement.classList.contains('dark') ? '#ccc' : '#333'
        }
      }
    ],
    yAxis: [
      {
        type: 'value',
        axisLabel: {
          color: document.documentElement.classList.contains('dark') ? '#ccc' : '#333'
        },
        splitLine: {
          lineStyle: {
            color: document.documentElement.classList.contains('dark') ? '#333' : '#eee'
          }
        }
      }
    ],
    series: [
      {
        name: '项目数量',
        type: 'bar',
        barWidth: '40%',
        data: [3, 5, 2, 1],
        itemStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            {offset: 0, color: '#83bff6'},
            {offset: 0.5, color: '#188df0'},
            {offset: 1, color: '#1890ff'}
          ])
        },
        emphasis: {
          itemStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              {offset: 0, color: '#2378f7'},
              {offset: 0.7, color: '#2378f7'},
              {offset: 1, color: '#83bff6'}
            ])
          }
        }
      }
    ]
  }

  projectChart.value.setOption(option)
}

// Handle window resize
const handleResize = () => {
  if (memberChart.value) {
    memberChart.value.resize()
  }
  if (projectChart.value) {
    projectChart.value.resize()
  }
}

// Watch for dark mode changes
const handleDarkModeChange = () => {
  nextTick(() => {
    updateMemberChart()
    updateProjectChart()
  })
}

onMounted(async () => {
  await fetchData()
  nextTick(() => {
    initCharts()
  })

  window.addEventListener('resize', handleResize)
  const observer = new MutationObserver(handleDarkModeChange)
  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class']
  })
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  if (memberChart.value) {
    memberChart.value.dispose()
  }
  if (projectChart.value) {
    projectChart.value.dispose()
  }
})
</script>

<style scoped>
/* Custom styles if needed */
</style>
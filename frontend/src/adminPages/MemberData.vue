<script setup lang="ts">
import { ref, computed, h, onMounted, nextTick } from 'vue'
import { 
  SearchOutline, 
  Refresh, 
  Download, 
  PersonAdd 
} from '@vicons/ionicons5'
import { useDialog, useMessage } from 'naive-ui'
// 导入所有需要的 NaiveUI 组件
import {
  NCard,
  NButton,
  NIcon,
  NDropdown,
  NTabs,
  NTabPane,
  NSelect,
  NInput,
  NDataTable,
  NModal,
  NForm,
  NFormItem,
  NRadioGroup,
  NSpace,
  NRadio
} from 'naive-ui'
import { MemberQueryService } from '../services/MemberQueryService'
import { MemberManagementService } from '../services/MemberManagementService'
import { DateCentreService } from '../services/DateCentreService'
import type { MemberModel, PaginatedMemberResponse } from '../models'
import * as echarts from 'echarts'

const dialog = useDialog()
const message = useMessage()
const showModal = ref(false)
const searchTerm = ref('')
const searchItem = ref('姓名')
const formRef = ref<any>(null)
const loading = ref(false)
const activeTab = ref('memberData')

// 分页相关状态
const currentPage = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)

// 图表引用
const yearChartRef = ref<HTMLDivElement | null>(null)
const collegeChartRef = ref<HTMLDivElement | null>(null)
const gradeChartRef = ref<HTMLDivElement | null>(null)
const genderChartRef = ref<HTMLDivElement | null>(null)
const politicalChartRef = ref<HTMLDivElement | null>(null)

// ECharts实例
let yearChart: echarts.ECharts | null = null
let collegeChart: echarts.ECharts | null = null
let gradeChart: echarts.ECharts | null = null
let genderChart: echarts.ECharts | null = null
let politicalChart: echarts.ECharts | null = null

// 搜索选项 - 与后端API匹配
const searchItems = [
  { label: '姓名', value: 'username' },
  { label: '学号', value: 'userid' },
  { label: '学院', value: 'academy' },
  { label: '专业班级', value: 'classname' },
  { label: '手机号', value: 'phone_num' }
]

// 下载选项
const downloadOptions = [
  { label: '下载CSV文件', key: 'csv' },
  { label: '下载JSON文件', key: 'json' }
]

// 学院选项
const academyOptions = [
  '建筑学院',
  '城乡规划学院',
  '环境与市政工程学院',
  '建筑设备科学与工程学院',
  '土木工程学院',
  '交通运输工程学院',
  '环境工程学院',
  '材料科学与工程学院',
  '管理学院',
  '机电工程学院',
  '冶金工程学院',
  '信息与控制工程学院',
  '艺术学院',
  '理学院',
  '文学院',
  '马克思主义学院',
  '体育学院',
  '继续教育学院'
].map(academy => ({ label: academy, value: academy }))

// 政治面貌选项
const politicalLandscapeOptions = [
  '群众',
  '共青团员',
  '中共党员'
].map(political => ({ label: political, value: political }))

// 性别选项
const genderOptions = ['男', '女']

// 成员数据
const members = ref<MemberModel[]>([])

const currentMember = ref<MemberModel>({
  identity: '',
  userName: '',
  userId: '',
  academy: null as string | null,
  className: '',
  phoneNum: '',
  politicalLandscape: null as string | null,
  gender: null as string | null,
  joinTime: '',
  passwordHash: '',
  eMail: null as string | null
} as MemberModel)

const rules = {
  userName: {
    required: true,
    message: '请输入姓名',
    trigger: 'blur'
  },
  userId: [
    {
      required: true,
      message: '请输入学号',
      trigger: 'blur'
    },
    {
      min: 10,
      max: 10,
      message: '学号应为10位数字',
      trigger: 'blur'
    }
  ],
  academy: {
    required: true,
    message: '请选择学院',
    trigger: 'change'
  },
  className: {
    required: true,
    message: '请输入专业班级',
    trigger: 'blur'
  },
  phoneNum: [
    {
      required: true,
      message: '请输入手机号',
      trigger: 'blur'
    },
    {
      pattern: /^1[3-9]\d{9}$/,
      message: '请输入正确的手机号',
      trigger: 'blur'
    }
  ],
  politicalLandscape: {
    required: true,
    message: '请选择政治面貌',
    trigger: 'change'
  },
  gender: {
    required: true,
    message: '请选择性别',
    trigger: 'change'
  }
}

const columns = [
  {
    title: '姓名',
    key: 'userName',
    width: 100
  },
  {
    title: '学号',
    key: 'userId',
    width: 150
  },
  {
    title: '学院',
    key: 'academy',
    width: 200
  },
  {
    title: '专业班级',
    key: 'className',
    width: 150
  },
  {
    title: '手机号',
    key: 'phoneNum',
    width: 150
  },
  {
    title: '政治面貌',
    key: 'politicalLandscape',
    width: 120
  },
  {
    title: '性别',
    key: 'gender',
    width: 80
  },
  {
    title: '操作',
    key: 'actions',
    width: 150,
    render(row: MemberModel) {
      return [
        h(
          NButton,
          {
            strong: true,
            tertiary: true,
            size: 'small',
            onClick: () => editMember(row)
          },
          { default: () => '编辑' }
        ),
        h(
          NButton,
          {
            strong: true,
            tertiary: true,
            size: 'small',
            type: 'error',
            onClick: () => deleteMember(row)
          },
          { default: () => '删除' }
        )
      ]
    }
  }
]

// 分页配置
const paginationConfig = computed(() => {
  return {
    page: currentPage.value,
    pageSize: pageSize.value,
    showSizePicker: true,
    pageSizes: [10, 20, 30, 50],
    itemCount: totalCount.value,
  }
})

const onChange = (page: number) => {
  currentPage.value = page
  // 带搜索条件的分页
  fetchMembers()
}
const onUpdatePageSize = (size: number) => {
  pageSize.value = size
  currentPage.value = 1
  // 带搜索条件的分页
  fetchMembers()
}

// 直接使用从API获取的成员数据，不再需要前端过滤
const filteredMembers = computed(() => members.value)

// 图表数据
const yearData = ref<{ year: string; value: number }[]>([])
const collegeData = ref<{ type: string; value: number }[]>([])
const gradeData = ref<{ grade: string; value: number }[]>([])
const genderData = ref<{ type: string; value: number }[]>([])
const politicalData = ref<{ type: string; sales: number }[]>([])

// 加载图表数据的函数
const loadChartData = async () => {
  try {
    loading.value = true
    
    // 并行获取所有图表数据
    const [yearResult, collegeResult, gradeResult, genderResult, landscapeResult] = await Promise.all([
      DateCentreService.getYearData(),
      DateCentreService.getCollegeData(),
      DateCentreService.getGradeData(),
      DateCentreService.getGenderData(),
      DateCentreService.getLandscapeData()
    ])
    
    // 处理历年人数数据
    yearData.value = yearResult
    
    // 处理学院分布数据
    collegeData.value = collegeResult
    
    // 处理年级分布数据
    gradeData.value = gradeResult.map(item => ({
      grade: item.grade || item.type,
      value: item.value || item.value
    }))
    
    // 处理性别分布数据
    genderData.value = genderResult
    
    // 处理政治面貌数据
    politicalData.value = landscapeResult.map(item => ({
      type: item.type,
      sales: item.sales
    }))
    
    // 渲染当前激活的图表
    if (activeTab.value !== 'memberData') {
      handleTabChange(activeTab.value)
    }
  } catch (error: any) {
    console.error('获取图表数据时出错:', error)
    message.error('获取图表数据失败: ' + (error.message || '未知错误'))
  } finally {
    loading.value = false
  }
}

// 获取成员数据（支持搜索）
const fetchMembers = async () => {
  try {
    loading.value = true
    const response: PaginatedMemberResponse = await MemberQueryService.getAllDataByPage(
      currentPage.value,
      pageSize.value,
      searchTerm.value,
      searchItem.value
    )

    console.log(response)
    
    members.value = response.data
    totalCount.value = response.totalCount
  } catch (error: any) {
    console.error('获取成员数据时出错:', error)
    message.error('获取成员数据时出错: ' + (error.message || '未知错误'))
  } finally {
    loading.value = false
  }
}

// 初始化图表
const initChart = (chartRef: HTMLDivElement | null, option: any) => {
  if (chartRef) {
    const chart = echarts.init(chartRef)
    chart.setOption(option)
    return chart
  }
  return null
}

// 渲染历年数据图表
const renderYearChart = () => {
  if (!yearChartRef.value) return

  const option = {
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: yearData.value.map(item => item.year)
    },
    yAxis: {
      type: 'value'
    },
    series: [{
      data: yearData.value.map(item => item.value),
      type: 'line',
      smooth: true,
      itemStyle: {
        color: '#3b82f6'
      },
      areaStyle: {
        color: '#3b82f6'
      }
    }]
  }

  yearChart = initChart(yearChartRef.value, option)
}

// 渲染学院分布图表
const renderCollegeChart = () => {
  if (!collegeChartRef.value) return

  const option = {
    tooltip: {
      trigger: 'item'
    },
    series: [{
      type: 'pie',
      radius: ['40%', '70%'],
      data: collegeData.value.map(item => ({
        name: item.type,
        value: item.value
      })),
      emphasis: {
        itemStyle: {
          shadowBlur: 10,
          shadowOffsetX: 0,
          shadowColor: 'rgba(0, 0, 0, 0.5)'
        }
      }
    }]
  }

  collegeChart = initChart(collegeChartRef.value, option)
}

// 渲染年级分布图表
const renderGradeChart = () => {
  if (!gradeChartRef.value) return

  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    xAxis: {
      type: 'category',
      data: gradeData.value.map(item => item.grade)
    },
    yAxis: {
      type: 'value'
    },
    series: [{
      data: gradeData.value.map(item => item.value),
      type: 'bar',
      itemStyle: {
        color: '#10b981'
      }
    }]
  }

  gradeChart = initChart(gradeChartRef.value, option)
}

// 渲染性别分布图表
const renderGenderChart = () => {
  if (!genderChartRef.value) return

  const option = {
    tooltip: {
      trigger: 'item'
    },
    series: [{
      type: 'pie',
      radius: ['40%', '70%'],
      data: genderData.value.map(item => ({
        name: item.type,
        value: item.value
      })),
      emphasis: {
        itemStyle: {
          shadowBlur: 10,
          shadowOffsetX: 0,
          shadowColor: 'rgba(0, 0, 0, 0.5)'
        }
      }
    }]
  }

  genderChart = initChart(genderChartRef.value, option)
}

// 渲染政治面貌图表
const renderPoliticalChart = () => {
  console.log(politicalData.value)
  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    xAxis: {
      type: 'category',
      data: politicalData.value.map(item => item.type)
    },
    yAxis: {
      type: 'value'
    },
    series: [{
      data: politicalData.value.map(item => item.sales),
      type: 'bar',
      itemStyle: {
        color: '#8b5cf6'
      }
    }]
  }

  console.log(option)
  politicalChart = initChart(politicalChartRef.value, option)
}

// 标签页切换处理
const handleTabChange = (name: string) => {
  nextTick(() => {
    switch (name) {
      case 'yearData':
        renderYearChart()
        break
      case 'collegeData':
        renderCollegeChart()
        break
      case 'gradeData':
        renderGradeChart()
        break
      case 'genderData':
        renderGenderChart()
        break
      case 'politicalData':
        renderPoliticalChart()
        break
    }
  })
}

const showAddMemberModal = () => {
  currentMember.value = {
    identity: '',
    userName: '',
    userId: '',
    academy: '',
    className: '',
    phoneNum: '',
    politicalLandscape: '',
    gender: '',
    joinTime: new Date().toISOString(),
    passwordHash: '',
    eMail: null
  }
  showModal.value = true
}

const editMember = (member: MemberModel) => {
  currentMember.value = { ...member }
  showModal.value = true
}

const deleteMember = async (member: MemberModel) => {
  dialog.warning({
    title: '确认删除',
    content: `确定要删除成员 ${member.userName} 吗？`,
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        await MemberManagementService.deleteMember(member.userId)
        const index = members.value.findIndex(m => m.userId === member.userId)
        if (index !== -1) {
          members.value.splice(index, 1)
          message.success(`删除成员: ${member.userName} 成功`)
          // 更新总数
          totalCount.value--
        }
      } catch (error: any) {
        console.error('删除成员时出错:', error)
        message.error('删除成员时出错: ' + (error.message || '未知错误'))
      }
    }
  })
}

const saveMember = async () => {
  formRef.value?.validate(async (errors: any) => {
    if (!errors) {
      try {
        if (currentMember.value.identity) {
          // 更新成员
          await MemberManagementService.updateMember(currentMember.value)
          const index = members.value.findIndex(m => m.identity === currentMember.value.identity)
          if (index !== -1) {
            members.value[index] = { ...currentMember.value }
          }
          message.success('成员信息已更新')
        } else {
          // 添加新成员 (这里需要根据实际情况调整)
          members.value.push({ ...currentMember.value })
          message.success('新成员已添加')
          // 更新总数
          totalCount.value++
        }

        showModal.value = false
      } catch (error: any) {
        console.error('保存成员信息时出错:', error)
        message.error('保存成员信息时出错: ' + (error.message || '未知错误'))
      }
    } else {
      message.error('请检查表单填写是否正确')
    }
  })
}

// 搜索成员 - 使用后端API进行搜索
const searchMembers = () => {
  // 重置为第一页并获取搜索结果
  currentPage.value = 1
  fetchMembers()
}

// 导出数据
const handleDownloadSelect = (key: string) => {
  if (key === 'csv') {
    exportToCSV()
  } else if (key === 'json') {
    exportToJSON()
  }
}

// 导出为 CSV 格式
const exportToCSV = () => {
  try {
    // 定义 CSV 头部
    const headers = ['姓名', '学号', '学院', '专业班级', '手机号', '政治面貌', '性别']
    const keys = ['userName', 'userId', 'academy', 'className', 'phoneNum', 'politicalLandscape', 'gender'] as const

    // 创建 CSV 内容
    let csvContent = '\uFEFF' + headers.join(',') + '\n' // 添加 UTF-8 BOM

    // 添加数据行
    members.value.forEach(member => {
      const row = keys.map(key => {
        const value = member[key] || ''
        // 转义包含逗号或引号的值
        return `"${value.toString().replace(/"/g, '""')}"`
      })
      csvContent += row.join(',') + '\n'
    })

    // 创建并下载文件
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.setAttribute('href', url)
    link.setAttribute('download', '成员数据.csv')
    link.style.visibility = 'hidden'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)

    message.success('CSV数据导出成功')
  } catch (error) {
    console.error('导出CSV数据失败:', error)
    message.error('导出CSV数据失败')
  }
}

// 导出为 JSON 格式
const exportToJSON = () => {
  try {
    const dataStr = JSON.stringify(members.value, null, 2)
    const blob = new Blob([dataStr], { type: 'application/json;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.setAttribute('href', url)
    link.setAttribute('download', '成员数据.json')
    link.style.visibility = 'hidden'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)

    message.success('JSON数据导出成功')
  } catch (error) {
    console.error('导出JSON数据失败:', error)
    message.error('导出JSON数据失败')
  }
}

// 窗口大小变化时重置图表大小
const handleResize = () => {
  if (yearChart) yearChart.resize()
  if (collegeChart) collegeChart.resize()
  if (gradeChart) gradeChart.resize()
  if (genderChart) genderChart.resize()
  if (politicalChart) politicalChart.resize()
}

// 组件挂载时获取成员数据
onMounted(() => {
  fetchMembers()
  loadChartData()
  window.addEventListener('resize', handleResize)
})
</script>

<template>
  <div class="min-h-screen p-4 md:p-6">
    <div class="rounded-xl mb-6 p-4">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6">
        <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">成员数据管理</h1>
        <div class="flex flex-wrap gap-2">
          <n-button type="primary" @click="showAddMemberModal">
            <template #icon>
              <n-icon>
                <PersonAdd />
              </n-icon>
            </template>
            添加成员
          </n-button>
          <n-dropdown trigger="hover" :options="downloadOptions" @select="handleDownloadSelect">
            <n-button>
              <template #icon>
                <n-icon>
                  <Download />
                </n-icon>
              </template>
              导出数据
            </n-button>
          </n-dropdown>
          <n-button @click="fetchMembers">
            <template #icon>
              <n-icon>
                <Refresh />
              </n-icon>
            </template>
            刷新
          </n-button>
        </div>
      </div>

      <n-tabs type="line" animated v-model:value="activeTab" class="mt-4" @update:value="handleTabChange">
        <n-tab-pane name="memberData" tab="成员数据">
          <div class="space-y-4">
            <div class="flex flex-col sm:flex-row gap-2">
              <n-select v-model:value="searchItem" :options="searchItems" style="width: 150px" class="flex-shrink-0" />
              <n-input v-model:value="searchTerm" placeholder="请输入搜索项" clearable @keyup.enter="searchMembers">
                <template #suffix>
                  <n-icon>
                    <SearchOutline />
                  </n-icon>
                </template>
              </n-input>
              <n-button type="primary" @click="searchMembers">
                搜索
              </n-button>
            </div>

            <n-data-table remote :columns="columns" :data="filteredMembers" @update:page-size="onUpdatePageSize" @update:page="onChange" :pagination="paginationConfig" :bordered="false"
              :loading="loading" class="rounded-lg overflow-hidden" />
          </div>
        </n-tab-pane>

        <n-tab-pane name="yearData" tab="历年人数">
          <div class="grid grid-cols-1 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">历年人数变化</h3>
              <div ref="yearChartRef" class="h-80 w-full"></div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="collegeData" tab="学院分布">
          <div class="grid grid-cols-1 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">学院分布</h3>
              <div ref="collegeChartRef" class="h-80 w-full"></div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="gradeData" tab="年级分布">
          <div class="grid grid-cols-1 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">年级分布</h3>
              <div ref="gradeChartRef" class="h-80 w-full"></div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="genderData" tab="男女比例">
          <div class="grid grid-cols-1 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">性别分布</h3>
              <div ref="genderChartRef" class="h-80 w-full"></div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="politicalData" tab="政治面貌">
          <div class="grid grid-cols-1 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">政治面貌分布</h3>
              <div ref="politicalChartRef" class="h-80 w-full"></div>
            </n-card>
          </div>
        </n-tab-pane>
      </n-tabs>
    </div>

    <!-- 添加/编辑成员模态框 -->
    <n-modal v-model:show="showModal" preset="card" class="w-full max-w-md rounded-xl"
      :title="currentMember.identity ? '编辑成员' : '添加成员'">
      <n-form :model="currentMember" :rules="rules" ref="formRef">
        <n-form-item label="姓名" path="userName">
          <n-input v-model:value="currentMember.userName" placeholder="请输入姓名" />
        </n-form-item>
        <n-form-item label="学号" path="userId">
          <n-input v-model:value="currentMember.userId" placeholder="请输入学号" />
        </n-form-item>
        <n-form-item label="学院" path="academy">
          <n-select v-model:value="currentMember.academy" :options="academyOptions" filterable placeholder="请选择学院" />
        </n-form-item>
        <n-form-item label="专业班级" path="className">
          <n-input v-model:value="currentMember.className" placeholder="请输入专业班级" />
        </n-form-item>
        <n-form-item label="手机号" path="phoneNum">
          <n-input v-model:value="currentMember.phoneNum" placeholder="请输入手机号" />
        </n-form-item>
        <n-form-item label="政治面貌" path="politicalLandscape">
          <n-select v-model:value="currentMember.politicalLandscape" :options="politicalLandscapeOptions"
            placeholder="请选择政治面貌" />
        </n-form-item>
        <n-form-item label="性别" path="gender">
          <n-radio-group v-model:value="currentMember.gender" name="gender">
            <n-space>
              <n-radio v-for="gender in genderOptions" :key="gender" :value="gender">
                {{ gender }}
              </n-radio>
            </n-space>
          </n-radio-group>
        </n-form-item>
      </n-form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="saveMember">保存</n-button>
        </div>
      </template>
    </n-modal>
  </div>
</template>

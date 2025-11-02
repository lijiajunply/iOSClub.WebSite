<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 md:p-6 transition-colors duration-200">
    <n-card class="rounded-xl shadow-sm border-0 bg-white dark:bg-gray-800 mb-6 transition-colors duration-200">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6">
        <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">成员数据管理</h1>
        <div class="flex flex-wrap gap-2">
          <n-button type="primary" @click="showAddMemberModal" secondary>
            <template #icon>
              <n-icon><PersonAdd /></n-icon>
            </template>
            添加成员
          </n-button>
          <n-dropdown trigger="hover" :options="downloadOptions" @select="handleDownloadSelect">
            <n-button secondary>
              <template #icon>
                <n-icon><Download /></n-icon>
              </template>
              导出数据
            </n-button>
          </n-dropdown>
          <n-button @click="fetchMembers" secondary>
            <template #icon>
              <n-icon><Refresh /></n-icon>
            </template>
            刷新
          </n-button>
        </div>
      </div>

      <n-tabs type="line" animated v-model:value="activeTab" class="mt-4">
        <n-tab-pane name="memberData" tab="成员数据">
          <div class="space-y-4">
            <div class="flex flex-col sm:flex-row gap-2">
              <n-select
                v-model:value="searchItem"
                :options="searchItems"
                style="width: 150px"
                class="flex-shrink-0"
              />
              <n-input
                v-model:value="searchTerm"
                placeholder="请输入搜索项"
                clearable
                @keyup.enter="searchMembers"
              >
                <template #suffix>
                  <n-icon><SearchOutline /></n-icon>
                </template>
              </n-input>
              <n-button type="primary" @click="searchMembers">
                搜索
              </n-button>
            </div>

            <n-data-table
              :columns="columns"
              :data="filteredMembers"
              :pagination="pagination"
              :bordered="false"
              striped
              :loading="loading"
              class="rounded-lg overflow-hidden"
            />
          </div>
        </n-tab-pane>

        <n-tab-pane name="yearData" tab="历年人数">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">历年人数变化</h3>
              <div class="h-64 flex items-end justify-between px-2">
                <div 
                  v-for="(item, index) in yearData" 
                  :key="index" 
                  class="flex flex-col items-center flex-1 mx-1"
                >
                  <div class="text-xs text-gray-500 dark:text-gray-400 mb-1">{{ item.year }}</div>
                  <div
                    class="w-full rounded-t-md bg-blue-500 hover:bg-blue-600 transition-all duration-300"
                    :style="{ height: (item.value / maxYearValue * 100) + '%' }"
                  ></div>
                  <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">{{ item.value }}</div>
                </div>
              </div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="collegeData" tab="学院分布">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200 md:col-span-2">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">学院分布</h3>
              <div class="h-64">
                <div 
                  v-for="(item, index) in collegeDataSorted" 
                  :key="index" 
                  class="flex items-center mb-2"
                >
                  <div class="w-24 text-sm text-gray-600 dark:text-gray-300 truncate">{{ item.type }}</div>
                  <div class="flex-1 mx-2">
                    <div class="h-6 rounded-full bg-gray-200 dark:bg-gray-700 overflow-hidden">
                      <div 
                        class="h-full rounded-full bg-blue-500"
                        :style="{ width: (item.value / totalCollegeValue * 100) + '%' }"
                      ></div>
                    </div>
                  </div>
                  <div class="w-10 text-sm text-gray-900 dark:text-white">{{ item.value }}</div>
                </div>
              </div>
            </n-card>
            
            <div class="space-y-4">
              <n-card 
                v-for="item in collegeData.slice(0, 4)" 
                :key="item.type"
                class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200"
              >
                <div class="flex justify-between items-center">
                  <div>
                    <div class="font-medium text-gray-900 dark:text-white">{{ item.type }}</div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">当前成员</div>
                  </div>
                  <div class="text-2xl font-bold text-blue-500">{{ item.value }}</div>
                </div>
              </n-card>
            </div>
          </div>
        </n-tab-pane>

        <n-tab-pane name="gradeData" tab="年级分布">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">年级分布</h3>
              <div class="h-64 overflow-y-auto pr-2">
                <div 
                  v-for="(item, index) in gradeDataSorted" 
                  :key="index" 
                  class="flex items-center mb-3"
                >
                  <div class="w-16 text-sm text-gray-600 dark:text-gray-300">{{ item.年级 }}</div>
                  <div class="flex-1 mx-2">
                    <div class="h-5 rounded-full bg-gray-200 dark:bg-gray-700 overflow-hidden">
                      <div 
                        class="h-full rounded-full bg-green-500"
                        :style="{ width: (item.人数 / maxGradeValue * 100) + '%' }"
                      ></div>
                    </div>
                  </div>
                  <div class="w-8 text-sm text-gray-900 dark:text-white">{{ item.人数 }}</div>
                </div>
              </div>
            </n-card>
            
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">年级分布图</h3>
              <div class="h-64 flex items-end justify-between px-2">
                <div 
                  v-for="(item, index) in gradeDataSorted.slice(0, 8)" 
                  :key="index" 
                  class="flex flex-col items-center flex-1 mx-1"
                >
                  <div class="text-xs text-gray-500 dark:text-gray-400 mb-1">{{ item.年级 }}</div>
                  <div
                    class="w-full rounded-t-md bg-green-500 hover:bg-green-600 transition-all duration-300"
                    :style="{ height: (item.人数 / maxGradeValue * 100) + '%' }"
                  ></div>
                  <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">{{ item.人数 }}</div>
                </div>
              </div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="genderData" tab="男女比例">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">性别分布</h3>
              <div class="h-48 flex items-center justify-center">
                <div class="w-full max-w-md mx-auto">
                  <div class="flex h-12 rounded-full bg-gray-200 dark:bg-gray-700 overflow-hidden">
                    <div 
                      v-for="(item, index) in genderData" 
                      :key="index"
                      class="flex items-center justify-center text-white font-medium"
                      :class="item.type === '男' ? 'bg-blue-500' : 'bg-pink-500'"
                      :style="{ width: (item.value / totalGenderValue * 100) + '%' }"
                    >
                      {{ item.type }}: {{ item.value }}
                    </div>
                  </div>
                  <div class="mt-6 text-center text-gray-600 dark:text-gray-300">
                    {{ genderInfo }}
                  </div>
                </div>
              </div>
            </n-card>
            
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">统计数据</h3>
              <div class="space-y-4">
                <div 
                  v-for="(item, index) in genderData" 
                  :key="index"
                  class="flex items-center justify-between p-4 rounded-lg"
                  :class="item.type === '男' ? 'bg-blue-50 dark:bg-blue-900/30' : 'bg-pink-50 dark:bg-pink-900/30'"
                >
                  <div class="flex items-center">
                    <div 
                      class="w-10 h-10 rounded-full flex items-center justify-center text-white font-medium mr-3"
                      :class="item.type === '男' ? 'bg-blue-500' : 'bg-pink-500'"
                    >
                      {{ item.type }}
                    </div>
                    <div>
                      <div class="font-medium text-gray-900 dark:text-white">{{ item.type }}</div>
                      <div class="text-sm text-gray-500 dark:text-gray-400">性别</div>
                    </div>
                  </div>
                  <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ item.value }}</div>
                </div>
              </div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="politicalData" tab="政治面貌">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">政治面貌分布</h3>
              <div class="h-64 overflow-y-auto pr-2">
                <div 
                  v-for="(item, index) in politicalDataSorted" 
                  :key="index" 
                  class="flex items-center mb-3"
                >
                  <div class="w-24 text-sm text-gray-600 dark:text-gray-300 truncate">{{ item.type }}</div>
                  <div class="flex-1 mx-2">
                    <div class="h-5 rounded-full bg-gray-200 dark:bg-gray-700 overflow-hidden">
                      <div 
                        class="h-full rounded-full bg-purple-500"
                        :style="{ width: (item.sales / maxPoliticalValue * 100) + '%' }"
                      ></div>
                    </div>
                  </div>
                  <div class="w-8 text-sm text-gray-900 dark:text-white">{{ item.sales }}</div>
                </div>
              </div>
            </n-card>
            
            <n-card class="rounded-xl bg-white dark:bg-gray-800 transition-colors duration-200">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">政治面貌占比</h3>
              <div class="h-64 flex items-center justify-center">
                <div class="w-48 h-48 rounded-full border-8 border-purple-500 flex items-center justify-center">
                  <div class="text-center">
                    <div class="text-3xl font-bold text-gray-900 dark:text-white">
                      {{ Math.round((politicalData.find(p => p.type === '共青团员')?.sales || 0) / totalPoliticalValue * 100) }}%
                    </div>
                    <div class="text-gray-600 dark:text-gray-300">共青团员</div>
                  </div>
                </div>
              </div>
            </n-card>
          </div>
        </n-tab-pane>
      </n-tabs>
    </n-card>

    <!-- 添加/编辑成员模态框 -->
    <n-modal v-model:show="showModal" preset="card" class="w-full max-w-md rounded-xl" :title="currentMember.id ? '编辑成员' : '添加成员'">
      <n-form :model="currentMember" :rules="rules" ref="formRef">
        <n-form-item label="姓名" path="userName">
          <n-input v-model:value="currentMember.userName" placeholder="请输入姓名" />
        </n-form-item>
        <n-form-item label="学号" path="userId">
          <n-input v-model:value="currentMember.userId" placeholder="请输入学号" />
        </n-form-item>
        <n-form-item label="学院" path="academy">
          <n-select
            v-model:value="currentMember.academy"
            :options="academyOptions"
            filterable
            placeholder="请选择学院"
          />
        </n-form-item>
        <n-form-item label="专业班级" path="className">
          <n-input v-model:value="currentMember.className" placeholder="请输入专业班级" />
        </n-form-item>
        <n-form-item label="手机号" path="phoneNum">
          <n-input v-model:value="currentMember.phoneNum" placeholder="请输入手机号" />
        </n-form-item>
        <n-form-item label="政治面貌" path="politicalLandscape">
          <n-select
            v-model:value="currentMember.politicalLandscape"
            :options="politicalLandscapeOptions"
            placeholder="请选择政治面貌"
          />
        </n-form-item>
        <n-form-item label="性别" path="gender">
          <n-radio-group v-model:value="currentMember.gender" name="gender">
            <n-space>
              <n-radio
                v-for="gender in genderOptions"
                :key="gender"
                :value="gender"
              >
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

    <!-- 文件上传隐藏输入框 -->
    <input
      ref="fileInput"
      type="file"
      accept=".json"
      @change="handleFileUpload"
      style="display: none"
      multiple
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, h, onMounted } from 'vue'
import { 
  SearchOutline, 
  Refresh, 
  Download, 
  PersonAdd 
} from '@vicons/ionicons5'
import { useDialog, useMessage } from 'naive-ui'
import { MemberQueryService } from '../services/MemberQueryService'
import { MemberManagementService } from '../services/MemberManagementService'
import type { MemberModel } from '../models'

const dialog = useDialog()
const message = useMessage()
const showModal = ref(false)
const searchTerm = ref('')
const searchItem = ref('姓名')
const formRef = ref<any>(null)
const loading = ref(false)
const activeTab = ref('memberData')
const fileInput = ref<HTMLInputElement | null>(null)

// 搜索选项
const searchItems = [
  { label: '姓名', value: '姓名' },
  { label: '学号', value: '学号' },
  { label: '学院', value: '学院' }
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
].map(academy => ({ label: academy, value: academy }));

// 政治面貌选项
const politicalLandscapeOptions = [
  '群众',
  '共青团员',
  '中共党员'
].map(political => ({ label: political, value: political }));

// 性别选项
const genderOptions = ['男', '女'];

// 成员数据
const members = ref<MemberModel[]>([])

const currentMember = ref({
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
})

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
          'n-button',
          {
            strong: true,
            tertiary: true,
            size: 'small',
            onClick: () => editMember(row)
          },
          { default: () => '编辑' }
        ),
        h(
          'n-button',
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

const pagination = {
  pageSize: 10
}

const filteredMembers = computed(() => {
  if (!searchTerm.value) return members.value
  const term = searchTerm.value.toLowerCase()

  switch (searchItem.value) {
    case '姓名':
      return members.value.filter(member => member.userName.toLowerCase().includes(term))
    case '学号':
      return members.value.filter(member => member.userId.includes(term))
    case '学院':
      return members.value.filter(member => member.academy && member.academy.toLowerCase().includes(term))
    default:
      return members.value
  }
})

// 图表数据
const yearData = ref<{ year: string; value: number }[]>([])
const collegeData = ref<{ type: string; value: number }[]>([])
const gradeData = ref<{ 年级: string; 人数: number }[]>([])
const genderData = ref<{ type: string; value: number }[]>([])
const politicalData = ref<{ type: string; sales: number }[]>([])

// 计算最大值用于图表缩放
const maxYearValue = computed(() => {
  return Math.max(...yearData.value.map(item => item.value), 1)
})

const maxGradeValue = computed(() => {
  return Math.max(...gradeData.value.map(item => item.人数), 1)
})

const maxPoliticalValue = computed(() => {
  return Math.max(...politicalData.value.map(item => item.sales), 1)
})

const totalCollegeValue = computed(() => {
  return collegeData.value.reduce((sum, item) => sum + item.value, 0)
})

const totalGenderValue = computed(() => {
  return genderData.value.reduce((sum, item) => sum + item.value, 0)
})

const totalPoliticalValue = computed(() => {
  return politicalData.value.reduce((sum, item) => sum + item.sales, 0)
})

// 排序后的数据
const collegeDataSorted = computed(() => {
  return [...collegeData.value].sort((a, b) => b.value - a.value)
})

const gradeDataSorted = computed(() => {
  return [...gradeData.value].sort((a, b) => b.人数 - a.人数)
})

const politicalDataSorted = computed(() => {
  return [...politicalData.value].sort((a, b) => b.sales - a.sales)
})

const genderInfo = computed(() => {
  if (genderData.value.length === 0) return ''
  const man = genderData.value.find(item => item.type === '男')?.value || 0
  const woman = genderData.value.find(item => item.type === '女')?.value || 0
  const total = man + woman
  if (total === 0) return ''

  return `社团现有男生${man}人，女生${woman}人，比例为${(man / woman * 100).toFixed(2)}%，男生占总人数${(man / total * 100).toFixed(2)}%`
})

// 获取成员数据
const fetchMembers = async () => {
  try {
    loading.value = true
    const data = await MemberQueryService.getAllData()
    members.value = data

    // 处理图表数据
    processChartData(data)
  } catch (error: any) {
    console.error('获取成员数据时出错:', error)
    message.error('获取成员数据时出错: ' + (error.message || '未知错误'))
  } finally {
    loading.value = false
  }
}

// 处理图表数据
const processChartData = (data: MemberModel[]) => {
  // 历年数据（模拟）
  yearData.value = [
    { year: '2019学年', value: 33 },
    { year: '2020学年', value: 1 },
    { year: '2021学年', value: 274 },
    { year: '2022学年', value: 329 }
  ]

  // 学院分布数据
  const collegeMap: Record<string, number> = {}
  data.forEach(member => {
    if (member.academy) {
      collegeMap[member.academy] = (collegeMap[member.academy] || 0) + 1
    }
  })

  collegeData.value = Object.keys(collegeMap).map(key => ({
    type: key,
    value: collegeMap[key]
  }))

  // 年级分布数据
  const gradeMap: Record<string, number> = {}
  data.forEach(member => {
    if (member.userId && member.userId.length >= 2) {
      const grade = member.userId.substring(0, 2)
      gradeMap[grade] = (gradeMap[grade] || 0) + 1
    }
  })

  gradeData.value = Object.keys(gradeMap).map(key => ({
    年级: key + '级',
    人数: gradeMap[key]
  }))

  // 性别分布数据
  const genderMap: Record<string, number> = { 男: 0, 女: 0 }
  data.forEach(member => {
    if (member.gender === '男' || member.gender === '女') {
      genderMap[member.gender] = (genderMap[member.gender] || 0) + 1
    }
  })

  genderData.value = Object.keys(genderMap).map(key => ({
    type: key,
    value: genderMap[key]
  }))

  // 政治面貌数据
  const politicalMap: Record<string, number> = {}
  data.forEach(member => {
    if (member.politicalLandscape) {
      politicalMap[member.politicalLandscape] = (politicalMap[member.politicalLandscape] || 0) + 1
    }
  })

  politicalData.value = Object.keys(politicalMap).map(key => ({
    type: key,
    sales: politicalMap[key]
  }))
}

const showAddMemberModal = () => {
  currentMember.value = {
    identity: '',
    userName: '',
    userId: '',
    academy: null,
    className: '',
    phoneNum: '',
    politicalLandscape: null,
    gender: null,
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
          // 重新处理图表数据
          processChartData(members.value)
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
        }

        // 重新处理图表数据
        processChartData(members.value)
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

// 搜索成员
const searchMembers = () => {
  // 实际搜索已在 filteredMembers 中处理
  message.info('搜索完成')
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
    const headers = ['姓名', '学号', '学院', '专业班级', '手机号', '政治面貌', '性别'];
    const keys = ['userName', 'userId', 'academy', 'className', 'phoneNum', 'politicalLandscape', 'gender'] as const;

    // 创建 CSV 内容
    let csvContent = '\uFEFF' + headers.join(',') + '\n'; // 添加 UTF-8 BOM

    // 添加数据行
    members.value.forEach(member => {
      const row = keys.map(key => {
        const value = member[key] || '';
        // 转义包含逗号或引号的值
        return `"${value.toString().replace(/"/g, '""')}"`;
      });
      csvContent += row.join(',') + '\n';
    });

    // 创建并下载文件
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', '成员数据.csv');
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    message.success('CSV数据导出成功');
  } catch (error) {
    console.error('导出CSV数据失败:', error);
    message.error('导出CSV数据失败');
  }
}

// 导出为 JSON 格式
const exportToJSON = () => {
  try {
    const dataStr = JSON.stringify(members.value, null, 2);
    const blob = new Blob([dataStr], { type: 'application/json;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', '成员数据.json');
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    message.success('JSON数据导出成功');
  } catch (error) {
    console.error('导出JSON数据失败:', error);
    message.error('导出JSON数据失败');
  }
}

// 处理文件上传
const handleFileUpload = (event: Event) => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = (e) => {
    try {
      const jsonData = JSON.parse(e.target?.result as string);
      // 这里应该处理上传的数据
      message.success('文件上传成功');
    } catch (error) {
      message.error('解析JSON文件失败');
    }
  };
  reader.readAsText(file);

  // 重置文件输入框
  target.value = '';
}

// 上传文件
const uploadFile = () => {
  fileInput.value?.click();
}

// 组件挂载时获取成员数据
onMounted(() => {
  fetchMembers()
})
</script>

<style scoped>
/* 使用 TailwindCSS 类，尽量减少自定义样式 */
</style>
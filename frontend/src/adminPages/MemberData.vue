<template>
  <div class="p-6">
    <n-card title="成员数据管理" class="mb-6">
      <template #header-extra>
        <n-space>
          <n-button type="primary" @click="showAddMemberModal">
            添加成员
          </n-button>
          <n-dropdown trigger="hover" :options="downloadOptions" @select="handleDownloadSelect">
            <n-button>导出数据</n-button>
          </n-dropdown>
          <n-button @click="fetchMembers">
            <template #icon>
              <n-icon><Refresh /></n-icon>
            </template>
            刷新
          </n-button>
        </n-space>
      </template>

      <n-tabs type="line" animated v-model:value="activeTab">
        <n-tab-pane name="memberData" tab="成员数据">
          <n-space vertical>
            <n-space>
              <n-select
                  v-model:value="searchItem"
                  :options="searchItems"
                  style="width: 150px"
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
              <n-button type="primary" @click="searchMembers">搜索</n-button>
            </n-space>

            <n-data-table
                :columns="columns"
                :data="filteredMembers"
                :pagination="pagination"
                :bordered="false"
                striped
                :loading="loading"
            />
          </n-space>
        </n-tab-pane>

        <n-tab-pane name="yearData" tab="历年人数">
          <div class="chart-container">
            <n-card title="历年人数统计">
              <div class="simple-chart">
                <div class="chart-title">历年人数变化</div>
                <div class="chart-content">
                  <div v-for="(item, index) in yearData" :key="index" class="chart-bar">
                    <div class="bar-label">{{ item.year }}</div>
                    <div class="bar-container">
                      <div
                          class="bar"
                          :style="{ height: (item.value / maxYearValue * 200) + 'px' }"
                      ></div>
                    </div>
                    <div class="bar-value">{{ item.value }}</div>
                  </div>
                </div>
              </div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="collegeData" tab="学院分布">
          <div class="chart-container">
            <n-grid :cols="24" :x-gap="12" :y-gap="12">
              <n-grid-item :span="24">
                <n-card title="学院分布统计">
                  <div class="simple-chart">
                    <div class="chart-title">各学院人数分布</div>
                    <div class="pie-chart">
                      <div
                          v-for="(item, index) in collegeDataSorted"
                          :key="index"
                          class="pie-segment"
                          :style="{ 
                          backgroundColor: pieColors[index % pieColors.length],
                          width: (item.value / totalCollegeValue * 100) + '%'
                        }"
                      >
                        <span class="pie-label">{{ item.type }} ({{ item.value }})</span>
                      </div>
                    </div>
                  </div>
                </n-card>
              </n-grid-item>
              <n-grid-item
                  v-for="item in collegeData"
                  :key="item.type"
                  :span="12"
              >
                <n-card :title="item.type">
                  <n-statistic label="当前成员" :value="item.value">
                    <template #prefix>
                      <n-icon><PeopleOutline /></n-icon>
                    </template>
                  </n-statistic>
                </n-card>
              </n-grid-item>
            </n-grid>
          </div>
        </n-tab-pane>

        <n-tab-pane name="gradeData" tab="年级分布">
          <div class="chart-container">
            <n-card title="年级分布统计">
              <div class="simple-chart">
                <div class="chart-title">各年级人数分布</div>
                <div class="chart-content horizontal">
                  <div
                      v-for="(item, index) in gradeDataSorted"
                      :key="index"
                      class="horizontal-bar"
                  >
                    <div class="bar-label">{{ item.年级 }}</div>
                    <div class="bar-container horizontal">
                      <div
                          class="bar horizontal"
                          :style="{ width: (item.人数 / maxGradeValue * 300) + 'px' }"
                      ></div>
                      <div class="bar-value">{{ item.人数 }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="genderData" tab="男女比例">
          <div class="chart-container">
            <n-card title="男女比例统计">
              <div class="simple-chart">
                <div class="chart-title">性别分布</div>
                <div class="gender-chart">
                  <div
                      v-for="(item, index) in genderData"
                      :key="index"
                      class="gender-segment"
                      :style="{ 
                      backgroundColor: item.type === '男' ? '#409EFF' : '#FF6666',
                      width: (item.value / totalGenderValue * 100) + '%'
                    }"
                  >
                    <span class="gender-label">{{ item.type }}: {{ item.value }}</span>
                  </div>
                </div>
              </div>
              <div class="gender-info">{{ genderInfo }}</div>
            </n-card>
          </div>
        </n-tab-pane>

        <n-tab-pane name="politicalData" tab="政治面貌">
          <div class="chart-container">
            <n-card title="政治面貌统计">
              <div class="simple-chart">
                <div class="chart-title">政治面貌分布</div>
                <div class="chart-content">
                  <div
                      v-for="(item, index) in politicalDataSorted"
                      :key="index"
                      class="chart-bar"
                  >
                    <div class="bar-label">{{ item.type }}</div>
                    <div class="bar-container">
                      <div
                          class="bar"
                          :style="{ height: (item.sales / maxPoliticalValue * 200) + 'px' }"
                      ></div>
                    </div>
                    <div class="bar-value">{{ item.sales }}</div>
                  </div>
                </div>
              </div>
            </n-card>
          </div>
        </n-tab-pane>
      </n-tabs>
    </n-card>

    <!-- 添加/编辑成员模态框 -->
    <n-modal v-model:show="showModal" preset="card" style="width: 600px;" :title="currentMember.id ? '编辑成员' : '添加成员'">
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
          <n-radio-group v-model:value="currentMember.gender">
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
        <n-space justify="end">
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="saveMember">保存</n-button>
        </n-space>
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

<script setup>
import { ref, computed, h, onMounted } from 'vue'
import { SearchOutline, Refresh, PeopleOutline } from '@vicons/ionicons5'
import { useDialog, useMessage } from 'naive-ui'
import { MemberQueryService } from '../services/MemberQueryService'
import { MemberManagementService } from '../services/MemberManagementService'

const dialog = useDialog()
const message = useMessage()
const showModal = ref(false)
const searchTerm = ref('')
const searchItem = ref('姓名')
const formRef = ref(null)
const loading = ref(false)
const activeTab = ref('memberData')
const fileInput = ref(null)

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

// 饼图颜色
const pieColors = ['#409EFF', '#67C23A', '#E6A23C', '#F56C6C', '#909399', '#FF6666', '#66CCCC'];

// 成员数据
const members = ref([])

const currentMember = ref({
  id: null,
  userName: '',
  userId: '',
  academy: null,
  className: '',
  phoneNum: '',
  politicalLandscape: null,
  gender: null
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
    render(row) {
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
const yearData = ref([])
const collegeData = ref([])
const gradeData = ref([])
const genderData = ref([])
const politicalData = ref([])

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
  } catch (error) {
    console.error('获取成员数据时出错:', error)
    message.error('获取成员数据时出错: ' + error.message)
  } finally {
    loading.value = false
  }
}

// 处理图表数据
const processChartData = (data) => {
  // 历年数据（模拟）
  yearData.value = [
    { year: '2019学年', value: 33 },
    { year: '2020学年', value: 1 },
    { year: '2021学年', value: 274 },
    { year: '2022学年', value: 329 }
  ]

  // 学院分布数据
  const collegeMap = {}
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
  const gradeMap = {}
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
  const genderMap = { 男: 0, 女: 0 }
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
  const politicalMap = {}
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
    id: null,
    userName: '',
    userId: '',
    academy: null,
    className: '',
    phoneNum: '',
    politicalLandscape: null,
    gender: null
  }
  showModal.value = true
}

const editMember = (member) => {
  currentMember.value = { ...member }
  showModal.value = true
}

const deleteMember = async (member) => {
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
      } catch (error) {
        console.error('删除成员时出错:', error)
        message.error('删除成员时出错: ' + error.message)
      }
    }
  })
}

const saveMember = async (e) => {
  e.preventDefault()
  formRef.value?.validate(async (errors) => {
    if (!errors) {
      try {
        const token = localStorage.getItem('Authorization')
        if (!token) {
          message.error('未找到认证信息')
          return
        }

        // 这里应该调用实际的API来保存成员信息
        // 由于没有看到对应的API，暂时只做前端演示
        if (currentMember.value.id) {
          // 更新成员
          const index = members.value.findIndex(m => m.id === currentMember.value.id)
          if (index !== -1) {
            members.value[index] = { ...currentMember.value }
          }
          message.success('成员信息已更新')
        } else {
          // 添加新成员
          currentMember.value.id = members.value.length + 1
          members.value.push({ ...currentMember.value })
          message.success('新成员已添加')
        }

        // 重新处理图表数据
        processChartData(members.value)
        showModal.value = false
      } catch (error) {
        console.error('保存成员信息时出错:', error)
        message.error('保存成员信息时出错')
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
const handleDownloadSelect = (key) => {
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
    const keys = ['userName', 'userId', 'academy', 'className', 'phoneNum', 'politicalLandscape', 'gender'];

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
const handleFileUpload = (event) => {
  const file = event.target.files[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = (e) => {
    try {
      const jsonData = JSON.parse(e.target.result);
      // 这里应该处理上传的数据
      message.success('文件上传成功');
    } catch (error) {
      message.error('解析JSON文件失败');
    }
  };
  reader.readAsText(file);

  // 重置文件输入框
  event.target.value = '';
}

// 上传文件
const uploadFile = () => {
  fileInput.value.click();
}

// 组件挂载时获取成员数据
onMounted(() => {
  fetchMembers()
})
</script>

<style scoped>
.chart-container {
  padding: 20px 0;
}

.gender-info {
  text-align: center;
  color: #666;
  margin-top: 20px;
  font-size: 14px;
}

.simple-chart {
  padding: 20px;
}

.chart-title {
  text-align: center;
  font-size: 18px;
  font-weight: bold;
  margin-bottom: 20px;
}

.chart-content {
  display: flex;
  justify-content: space-around;
  align-items: flex-end;
  height: 250px;
  padding: 20px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
}

.chart-content.horizontal {
  flex-direction: column;
  align-items: flex-start;
  height: auto;
}

.chart-bar {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin: 0 10px;
}

.bar-label {
  margin-bottom: 10px;
  font-size: 12px;
}

.bar-container {
  display: flex;
  align-items: flex-end;
  justify-content: center;
  width: 60px;
  height: 200px;
}

.bar-container.horizontal {
  width: 300px;
  height: 30px;
  align-items: center;
  margin-right: 20px;
}

.bar {
  width: 40px;
  background-color: #409EFF;
  border-radius: 4px 4px 0 0;
}

.bar.horizontal {
  height: 20px;
  border-radius: 4px;
}

.bar-value {
  margin-top: 10px;
  font-size: 12px;
}

.pie-chart {
  display: flex;
  height: 50px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

.pie-segment {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  position: relative;
}

.pie-label {
  position: absolute;
  bottom: -25px;
  font-size: 12px;
  white-space: nowrap;
}

.gender-chart {
  display: flex;
  height: 50px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

.gender-segment {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.gender-label {
  color: white;
  font-weight: bold;
  font-size: 14px;
}

.horizontal-bar {
  display: flex;
  align-items: center;
  margin: 10px 0;
}
</style>
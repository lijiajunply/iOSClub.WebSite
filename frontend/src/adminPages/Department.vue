<template>
  <div class="apple-container min-h-screen max-sm:p-0 p-6 md:p-8 transition-colors duration-300">
    <div class="apple-card p-2 md:rounded-3xl">
      <n-tabs
          type="segment"
          animated
          class="apple-tabs"
          @update:value="handleTabChange"
      >
        <!-- 总览 Tab -->
        <n-tab-pane name="overview" tab="总览面板">
          <div class="space-y-8 mt-6 animate-fade-in">

            <!-- 顶部统计卡片组 -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <!-- 领导层概览 -->
              <div class="apple-sub-card p-6 flex flex-col justify-between h-full">
                <div class="flex items-center justify-between mb-4">
                  <div class="flex items-center gap-2 text-gray-500 dark:text-gray-400">
                    <Icon icon="ion:ribbon-outline" class="text-xl"/>
                    <span class="text-sm font-medium">领导核心</span>
                  </div>
                  <div class="flex gap-2">
                    <button v-if="!loading" @click="() => openAddMember(null)" class="apple-icon-btn text-blue-500">
                      <Icon icon="ion:add-circle" width="24"/>
                    </button>
                  </div>
                </div>

                <div v-if="loading" class="animate-pulse space-y-2">
                  <div class="h-8 bg-gray-200 dark:bg-gray-700 rounded-md w-3/4"></div>
                </div>
                <div v-else class="flex flex-wrap gap-2 content-start">
                  <div v-for="member in ministers" :key="member.userId"
                       class="apple-chip group">
                    <span class="font-medium">{{ member.userName }}</span>
                    <button @click.stop="() => deleteMember(member, ministers)"
                            class="ml-1 opacity-0 group-hover:opacity-100 transition-opacity text-red-500">
                      <Icon icon="ion:close-circle"/>
                    </button>
                  </div>
                  <div v-if="ministers.length === 0" class="text-gray-400 italic text-sm">暂无领导成员</div>
                </div>
              </div>

              <!-- 成员概览 -->
              <div class="apple-sub-card p-6 flex flex-col justify-between h-full">
                <div class="flex items-center justify-between mb-4">
                  <div class="flex items-center gap-2 text-gray-500 dark:text-gray-400">
                    <Icon icon="ion:people-outline" class="text-xl"/>
                    <span class="text-sm font-medium">成员总数</span>
                  </div>
                  <button @click="downloadMemberInfo" class="apple-icon-btn text-blue-500" title="导出数据">
                    <Icon icon="ion:cloud-download-outline" width="24"/>
                  </button>
                </div>
                <div class="text-4xl font-bold text-gray-900 dark:text-white tracking-tight">
                  {{ loading ? '-' : members.length }}
                  <span class="text-lg font-normal text-gray-400 ml-1">人</span>
                </div>
              </div>

              <!-- 项目概览 -->
              <div class="apple-sub-card p-6 flex flex-col justify-between h-full">
                <div class="flex items-center justify-between mb-4">
                  <div class="flex items-center gap-2 text-gray-500 dark:text-gray-400">
                    <Icon icon="ion:folder-open-outline" class="text-xl"/>
                    <span class="text-sm font-medium">运行项目</span>
                  </div>
                  <button @click="addProject" class="apple-icon-btn text-blue-500">
                    <Icon icon="ion:add-circle" width="24"/>
                  </button>
                </div>
                <div class="text-4xl font-bold text-gray-900 dark:text-white tracking-tight">
                  {{ loading ? '-' : projects.length }}
                  <span class="text-lg font-normal text-gray-400 ml-1">个</span>
                </div>
              </div>
            </div>

            <!-- 数据图表区 -->
            <section>
              <h3 class="section-title mb-4">数据透视</h3>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div v-for="(chartId, index) in ['departmentChart', 'collegeChart', 'genderChart']"
                     :key="chartId"
                     class="apple-sub-card p-4 h-[350px] flex flex-col">
                    <span class="text-sm font-medium text-gray-500 dark:text-gray-400 mb-2 ml-2">
                      {{ ['部门分布', '学院分布', '男女比例'][index] }}
                    </span>
                  <div class="flex-1 rounded-xl overflow-hidden relative">
                    <div :id="chartId" class="w-full h-full"></div>
                    <!-- Loading State for Charts -->
                    <div v-if="loading"
                         class="absolute inset-0 z-10 flex items-center justify-center bg-white/50 dark:bg-black/50 backdrop-blur-sm">
                      <Icon icon="ion:load-c" class="animate-spin text-3xl text-blue-500"/>
                    </div>
                  </div>
                </div>
              </div>
            </section>

            <!-- 成员列表表格 -->
            <section class="apple-sub-card overflow-hidden">
              <div class="p-4 border-b border-gray-100 dark:border-white/10 flex justify-between items-center">
                <h3 class="font-semibold text-lg">全体成员名单</h3>
              </div>
              <n-data-table
                  :columns="memberColumns"
                  :data="members"
                  :pagination="pagination"
                  :bordered="false"
                  :loading="loading"
                  class="apple-table"
              />
            </section>

            <!-- 项目卡片网格 -->
            <section>
              <h3 class="section-title mb-4">项目一览</h3>
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <div v-for="project in projects" :key="project.id"
                     class="apple-item-card group cursor-pointer"
                     @click="openProject(project)">
                  <div class="flex justify-between items-start mb-3">
                    <div class="p-2 rounded-lg bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400">
                      <Icon icon="ion:briefcase" width="20"/>
                    </div>
                    <div v-if="project.department?.name"
                         class="text-xs font-medium px-2 py-1 rounded-full bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-300">
                      {{ project.department.name }}
                    </div>
                  </div>

                  <h4 class="text-lg font-semibold mb-2 group-hover:text-blue-600 transition-colors">{{
                      project.title
                    }}</h4>
                  <p class="text-sm text-gray-500 dark:text-gray-400 line-clamp-2 mb-4 h-10">{{
                      project.description
                    }}</p>

                  <div
                      class="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-all translate-y-2 group-hover:translate-y-0"
                      @click.stop>
                    <button class="apple-btn-sm secondary" @click="editProject(project)">编辑</button>
                    <button class="apple-btn-sm danger" @click="deleteProject(project)">删除</button>
                  </div>
                </div>
                <!-- 空状态 -->
                <div v-if="projects.length === 0 && !loading"
                     class="col-span-full py-12 flex flex-col items-center justify-center text-gray-400">
                  <Icon icon="ion:file-tray-outline" width="48" class="mb-2 opacity-50"/>
                  <p>暂无项目</p>
                </div>
              </div>
            </section>

          </div>
        </n-tab-pane>

        <!-- 动态部门 Tab -->
        <n-tab-pane
            v-for="department in departments"
            :key="department.id"
            :name="department.id || ''"
            :tab="department.name"
        >
          <div class="space-y-8 mt-6 animate-fade-in" v-if="!loading">

            <!-- 部门头部信息 -->
            <div class="apple-sub-card p-8 relative overflow-hidden">
              <!-- 装饰背景 -->
              <div
                  class="absolute -right-10 -top-10 w-64 h-64 bg-blue-500/10 rounded-full blur-3xl pointer-events-none"></div>

              <div class="relative z-10">
                <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-6">
                  <div>
                    <h2 class="text-3xl font-bold text-gray-900 dark:text-white tracking-tight">{{
                        department.name
                      }}</h2>
                  </div>
                  <div class="flex gap-2">
                    <button @click="() => openDepartment(department)" class="apple-btn secondary">
                      <Icon icon="ion:settings-outline" class="mr-1"/>
                      设置
                    </button>
                    <button @click="() => deleteDepartment(department)" class="apple-btn danger">
                      <Icon icon="ion:trash-outline" class="mr-1"/>
                      删除
                    </button>
                  </div>
                </div>
                <p class="text-gray-600 dark:text-gray-300 max-w-3xl leading-relaxed text-lg">
                  {{ department.description }}
                </p>
              </div>
            </div>

            <!-- 部门内容双栏布局 -->
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

              <!-- 左侧：部长与信息 -->
              <div class="space-y-6">
                <div class="apple-sub-card p-6">
                  <div class="flex justify-between items-center mb-4">
                    <h3 class="font-semibold text-lg">管理团队</h3>
                    <div class="flex gap-1">
                      <button @click="() => openAddMember(department, 'Minister')"
                              class="apple-icon-btn text-blue-500">
                        <Icon icon="ion:add"/>
                      </button>
                      <button @click="() => deleteAll(department.ministers)" class="apple-icon-btn text-red-500">
                        <Icon icon="ion:trash-bin-outline"/>
                      </button>
                    </div>
                  </div>

                  <div class="flex flex-wrap gap-2">
                    <div v-for="member in department.ministers" :key="member.userId"
                         class="apple-chip large blue group">
                      <Icon icon="ion:shield-checkmark" class="mr-1 text-blue-600 dark:text-blue-300 opacity-70"/>
                      <span>{{ member.name }}</span>
                      <button @click="() => deleteMember(member, department.ministers)"
                              class="ml-1 hover:text-red-600 transition-colors">
                        <Icon icon="ion:close"/>
                      </button>
                    </div>
                    <div v-if="!department.ministers?.length" class="text-sm text-gray-400 py-2">
                      暂未指派部长
                    </div>
                  </div>
                </div>

                <div class="apple-sub-card p-6 bg-gradient-to-br from-blue-500 to-indigo-600 text-white border-none">
                  <h3 class="text-white/90 font-medium mb-1">项目统计</h3>
                  <div class="text-3xl font-bold mb-4">{{ department.projects?.length || 0 }}</div>
                  <button @click="addProject"
                          class="w-full py-2 bg-white/20 hover:bg-white/30 rounded-lg text-sm font-medium transition-colors backdrop-blur-md flex items-center justify-center">
                    <Icon icon="ion:add" class="mr-1"/>
                    新建部门项目
                  </button>
                </div>
              </div>

              <!-- 右侧：成员列表 -->
              <div class="lg:col-span-2 apple-sub-card overflow-hidden flex flex-col">
                <div
                    class="p-4 border-b border-gray-100 dark:border-white/10 flex justify-between items-center bg-gray-50/50 dark:bg-white/5">
                  <div class="flex items-center gap-2">
                    <h3 class="font-semibold">部门成员</h3>
                    <span
                        class="bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-300 text-xs px-2 py-0.5 rounded-full">{{
                        department.members?.length || 0
                      }}</span>
                  </div>
                  <div class="flex gap-2">
                    <button @click="() => openAddMember(department, 'Department')" class="apple-btn-sm primary">
                      添加成员
                    </button>
                    <button @click="() => deleteAll(department.members)" class="apple-btn-sm danger">清空</button>
                  </div>
                </div>
                <div class="flex-1 overflow-auto">
                  <n-data-table
                      :columns="staffColumns"
                      :data="department.members"
                      :pagination="pagination"
                      :bordered="false"
                      class="apple-table"
                  />
                </div>
              </div>
            </div>

            <!-- 部门项目 -->
            <section v-if="department.projects !== null && department.projects!.length > 0">
              <h3 class="section-title mb-4">归属项目</h3>
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <div v-for="project in department.projects" :key="project.id"
                     class="apple-item-card group cursor-pointer"
                     @click="openProject(project)">
                  <h4 class="text-lg font-semibold mb-2 group-hover:text-blue-600 transition-colors">{{
                      project.title
                    }}</h4>
                  <p class="text-sm text-gray-500 dark:text-gray-400 line-clamp-2 mb-4">{{ project.description }}</p>
                  <div class="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-all" @click.stop>
                    <button class="apple-btn-sm secondary" @click="editProject(project)">编辑</button>
                    <button class="apple-btn-sm danger" @click="deleteProject(project, department.projects)">删除
                    </button>
                  </div>
                </div>
              </div>
            </section>
          </div>
          <!-- Loading Skeleton for specific tabs -->
          <div v-else class="p-12 flex justify-center">
            <Icon icon="ion:load-c" class="animate-spin text-4xl text-gray-300"/>
          </div>
        </n-tab-pane>
      </n-tabs>
    </div>
  </div>

  <!-- 模态框组件 - 样式重写 -->
  <!-- 添加成员 -->
  <n-modal
      v-model:show="showAddMemberModal"
      preset="card"
      class="apple-modal"
      :title="`添加${addMemberType === 'minister' ? '部长' : '成员'}`"
      :bordered="false"
      size="huge"
  >
    <div class="space-y-6">
      <div class="relative">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <Icon icon="ion:search" class="text-gray-400"/>
        </div>
        <input
            v-model="searchKeyword"
            @keyup.enter="searchMembers"
            type="text"
            placeholder="搜索姓名或学号..."
            class="w-full pl-10 pr-4 py-3 bg-gray-100 dark:bg-white/10 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 transition-all"
        />
      </div>

      <div class="min-h-[200px]">
        <n-data-table
            v-if="searchResults.length > 0"
            :columns="searchColumns"
            :data="searchResults"
            class="apple-table"
            :bordered="false"
            :pagination="{pageSize: 5}"
        />
        <div v-else class="h-full flex flex-col items-center justify-center text-gray-400 gap-2 py-8">
          <Icon icon="ion:search-outline" width="48" class="opacity-20"/>
          <span>{{ searchKeyword ? '未找到匹配成员' : '输入关键词开始搜索' }}</span>
        </div>
      </div>
    </div>
  </n-modal>

  <!-- 编辑部门 -->
  <n-modal
      v-model:show="showDepartmentModal"
      preset="card"
      class="apple-modal"
      :title="editingDepartment ? '编辑部门' : '新建部门'"
      :bordered="false"
  >
    <n-form :model="departmentForm" :rules="departmentRules" ref="departmentFormRef" class="space-y-4">
      <n-form-item label="部门名称" path="name">
        <n-input v-model:value="departmentForm.name" placeholder="例如：技术部" class="apple-input-Override"/>
      </n-form-item>
      <n-form-item label="职能描述" path="description">
        <n-input
            v-model:value="departmentForm.description"
            placeholder="描述该部门的主要职责..."
            type="textarea"
            :autosize="{ minRows: 4, maxRows: 6 }"
            class="apple-input-Override"
        />
      </n-form-item>
    </n-form>
    <template #footer>
      <div class="flex justify-end gap-3">
        <button @click="showDepartmentModal = false" class="apple-btn secondary">取消</button>
        <button @click="saveDepartment" class="apple-btn primary">完成</button>
      </div>
    </template>
  </n-modal>

  <!-- 更改部门 -->
  <n-modal
      v-model:show="showChangeDepartmentModalRef"
      preset="card"
      class="apple-modal"
      title="人事调动"
      :bordered="false"
  >
    <div v-if="selectedStaff" class="space-y-6">
      <div class="bg-gray-50 dark:bg-white/5 p-4 rounded-xl flex items-center gap-4">
        <div
            class="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 font-bold text-xl">
          {{ selectedStaff.name.charAt(0) }}
        </div>
        <div>
          <div class="font-medium text-gray-900 dark:text-white">{{ selectedStaff.name }}</div>
          <div class="text-sm text-gray-500">{{ selectedStaff.userId }} | {{
              selectedStaff.department || '无部门'
            }}
          </div>
        </div>
      </div>

      <div>
        <label class="block text-sm font-medium mb-2 text-gray-600 dark:text-gray-400">调动至</label>
        <n-select v-model:value="targetDepartment" :options="departmentOptions" placeholder="选择新部门"/>
      </div>
    </div>
    <template #footer>
      <div class="flex justify-end gap-3">
        <button @click="showChangeDepartmentModalRef = false" class="apple-btn secondary">取消</button>
        <button
            @click="handleChangeDepartment"
            class="apple-btn primary"
            :disabled="!targetDepartment || targetDepartment === selectedStaff?.department"
        >确认调动
        </button>
      </div>
    </template>
  </n-modal>
</template>

<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount, h, computed, nextTick, watch, defineComponent} from 'vue'
import {useRouter} from 'vue-router'
import {
  useMessage,
  NTabs,
  NTabPane,
  NSelect,
  NInput,
  NDataTable,
  NModal,
  NForm,
  NFormItem,
} from 'naive-ui'
import type {DataTableColumns} from 'naive-ui'
import {Icon} from '@iconify/vue'
import {DepartmentService} from '../services/DepartmentService'
import {StaffService} from '../services/StaffService'
import {ProjectService} from '../services/ProjectService'
import type {Department, DepartmentModel, MemberModel, Project, StudentModel, StaffModel} from '../models'
import * as echarts from 'echarts'
import {MemberQueryService} from "../services/MemberQueryService";
import {useLayoutStore} from '../stores/LayoutStore';

const router = useRouter()
const message = useMessage()
const layoutStore = useLayoutStore()

// --- 数据状态 ---
const ministers = ref<MemberModel[]>([])
const members = ref<MemberModel[]>([])
const projects = ref<Project[]>([])
const departments = ref<Department[]>([])
const staffs = ref<MemberModel[]>([])
const loading = ref(true) // 默认 loading true

const showChangeDepartmentModalRef = ref(false)
const selectedStaff = ref<StaffModel | null>(null)
const targetDepartment = ref('')

const showAddMemberModal = ref(false)
const showDepartmentModal = ref(false)
const searchKeyword = ref('')
const searchResults = ref<StudentModel[]>([])
const addMemberType = ref('member')
const departmentFormRef = ref<InstanceType<typeof NForm> | null>(null)

const departmentForm = ref({
  name: '',
  description: ''
})

const editingDepartment = ref<Department | null>(null)
const currentDepartment = ref<Department | null>(null)

const pagination = {pageSize: 8} // 调整每页数量适配卡片高度

// --- Helper Components for Render Functions (Tailwind Styled) ---
const AppleButton = (props: {
  type?: 'primary' | 'danger' | 'secondary',
  size?: 'small',
  onClick: () => void,
  text: string
}) => {
  const baseClass = "inline-flex items-center justify-center font-medium transition-all active:scale-95 rounded-lg"
  const sizeClass = props.size === 'small' ? 'px-2.5 py-1 text-xs' : 'px-4 py-2 text-sm'

  let colorClass: string
  if (props.type === 'danger') colorClass = 'bg-red-50 text-red-600 hover:bg-red-100 dark:bg-red-500/10 dark:text-red-400 dark:hover:bg-red-500/20'
  else if (props.type === 'primary') colorClass = 'bg-blue-600 text-white hover:bg-blue-700 shadow-sm shadow-blue-500/30'
  else colorClass = 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-white/10 dark:text-gray-200 dark:hover:bg-white/20'

  return h('button', {
    class: `${baseClass} ${sizeClass} ${colorClass}`,
    onClick: (e: Event) => {
      e.stopPropagation();
      props.onClick()
    }
  }, props.text)
}

// --- Table Columns Configuration ---
const memberColumns: DataTableColumns<MemberModel> = [
  {
    title: '姓名', key: 'userName', width: 100,
    render: (row) => h('span', {class: 'font-medium text-gray-900 dark:text-gray-100'}, row.userName)
  },
  {title: '学号', key: 'userId', width: 120, className: 'text-gray-500'},
  {title: '学院', key: 'academy', width: 150},
  {title: '性别', key: 'gender', width: 60},
  {title: '专业班级', key: 'className', width: 140},
  {title: '手机', key: 'phoneNum', width: 120},
  {
    title: '操作',
    key: 'actions',
    width: 80,
    render: (row) => AppleButton({type: 'danger', size: 'small', onClick: () => deleteMember(row), text: '移除'})
  }
]

const staffColumns: DataTableColumns<StaffModel> = [
  {
    title: '姓名', key: 'name', width: 100,
    render: (row) => h('div', {class: 'flex items-center gap-2'}, [
      h(Icon, {icon: 'ion:person-circle-outline', class: 'text-lg text-gray-400'}),
      h('span', {class: 'font-medium'}, row.name)
    ])
  },
  {title: '学号', key: 'userId', width: 120, className: 'text-gray-500 font-mono text-xs'},
  {
    title: '操作',
    key: 'actions',
    width: 140,
    render: (row) => h('div', {class: 'flex gap-2'}, [
      AppleButton({type: 'secondary', size: 'small', onClick: () => showChangeDepartmentModal(row), text: '调岗'}),
      AppleButton({type: 'danger', size: 'small', onClick: () => deleteStaff(row), text: '移除'})
    ])
  }
]

const searchColumns: DataTableColumns<any> = [
  {title: '姓名', key: 'userName', width: 100, render: (row) => h('b', row.userName)},
  {title: '学号', key: 'userId', width: 120},
  {title: '学院', key: 'academy', width: 150},
  {
    title: '操作',
    key: 'actions',
    width: 80,
    render: (row) => AppleButton({type: 'primary', size: 'small', onClick: () => addMember(row), text: '添加'})
  }
]

const departmentRules = {
  name: {required: true, message: '请输入部门名称', trigger: 'blur'},
  description: {required: true, message: '请输入部门简介', trigger: 'blur'}
}

const departmentOptions = computed(() => {
  return departments.value.map(dept => ({
    label: dept.name,
    value: dept.name
  }))
})

// --- Actions ---

const openDepartment = (department: Department | null = null) => {
  if (department) {
    editingDepartment.value = department
    departmentForm.value = {
      name: department.name || '',
      description: department.description || ''
    }
  } else {
    editingDepartment.value = null
    departmentForm.value = {name: '', description: ''}
  }
  showDepartmentModal.value = true
}

const showChangeDepartmentModal = (staff: StaffModel) => {
  selectedStaff.value = staff
  targetDepartment.value = staff.department || ''
  showChangeDepartmentModalRef.value = true
}

const openAddMember = (department: Department | null = null, type = 'member') => {
  currentDepartment.value = department
  showAddMemberModal.value = true
  searchKeyword.value = ''
  searchResults.value = []
  addMemberType.value = type
}

// --- CRUD Operations (Logic Preserved) ---

const deleteAll = async (list: any[] | undefined) => {
  try {
    if (list && Array.isArray(list)) {
      const listCopy = [...list]
      for (const member of listCopy) {
        await StaffService.deleteStaff(member.userId)
      }
      await fetchData()
      message.success('清空成功')
    }
  } catch (error: any) {
    console.error('Error:', error)
    message.error('操作失败')
  }
}

const deleteMember = async (member: any, list?: any[]) => {
  try {
    await StaffService.deleteStaff(member.userId)
    if (list && Array.isArray(list)) {
      const index = list.findIndex(m => m.userId === member.userId)
      if (index > -1) list.splice(index, 1)
    } else {
      const index = members.value.findIndex(m => m.userId === member.userId)
      if (index > -1) members.value.splice(index, 1)
    }
    message.success('已移除成员')
    await fetchData() // Refresh mostly for charts
  } catch (error: any) {
    message.error('删除失败')
  }
}

const deleteStaff = async (staff: any) => {
  const res = await StaffService.deleteStaff(staff.userId)
  if (!res) return message.error('操作失败')
  message.success('成员已删除')
  await fetchData()
}

const downloadMemberInfo = async () => {
  try {
    const blob = await DepartmentService.exportJson()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'member.json'
    document.body.appendChild(a)
    a.click()
    setTimeout(() => {
      document.body.removeChild(a);
      URL.revokeObjectURL(url)
    }, 100)
    message.success('下载已开始')
  } catch (error) {
    message.error('导出失败')
  }
}

const addProject = () => router.push('/Centre/ProjectEditor')
const editProject = (project: Project) => router.push(`/Centre/ProjectEditor/${project.id}`)
const openProject = (project: Project) => router.push(`/Centre/ProjectData/${project.id}`)

const deleteProject = async (project: Project, _?: Project[]) => {
  try {
    await ProjectService.deleteProject(project.id)
    await fetchData()
    message.success('项目已删除')
  } catch (error: any) {
    message.error('删除失败')
  }
}

const deleteDepartment = async (department: Department) => {
  try {
    await DepartmentService.deleteDepartment(department.name)
    await fetchData()
    message.success('部门已删除')
  } catch (error: any) {
    message.error('删除失败')
  }
}

const searchMembers = async () => {
  if (!searchKeyword.value) {
    searchResults.value = []
    return
  }
  try {
    searchResults.value = await MemberQueryService.search(searchKeyword.value, 'username')
  } catch (error) {
    message.error('搜索出错')
  }
}

const addMember = async (member: StudentModel) => {
  try {
    const commonData = {
      userId: member.userId,
      name: member.userName,
      academy: member.academy,
      className: member.className,
      gender: member.gender,
      phoneNum: member.phoneNum,
      politicalLandscape: member.politicalLandscape,
    }

    if (currentDepartment.value) {
      await StaffService.createStaff({
        ...commonData,
        identity: addMemberType.value === 'minister' ? 'Minister' : 'Department',
        department: currentDepartment.value.name
      } as StaffModel);
      message.success(`已添加至 ${currentDepartment.value.name}`)
    } else {
      await StaffService.createStaff({
        ...commonData,
        identity: 'President',
        department: ''
      } as StaffModel);
      message.success(`已添加至领导层`)
    }
    await fetchData()
    showAddMemberModal.value = false
  } catch (error: any) {
    message.error('添加失败: ' + (error.message || '未知错误'))
  }
}

const saveDepartment = async () => {
  try {
    await departmentFormRef.value?.validate()
    const departmentData = {
      key: editingDepartment.value?.id.toString() || '',
      name: departmentForm.value.name,
      description: departmentForm.value.description
    } as DepartmentModel

    if (editingDepartment.value) {
      await DepartmentService.updateDepartment(departmentData)
      message.success('部门已更新')
    } else {
      await DepartmentService.createDepartment(departmentData)
      message.success('部门已创建')
    }
    await fetchData()
    showDepartmentModal.value = false
  } catch (error: any) {
    message.error('保存失败')
  }
}

const handleChangeDepartment = async () => {
  if (!selectedStaff.value || !targetDepartment.value) return
  try {
    await StaffService.changeDepartment(selectedStaff.value.userId, targetDepartment.value)
    showChangeDepartmentModalRef.value = false
    message.success('调岗成功')
    await fetchData()
  } catch (error: any) {
    message.error('调岗失败')
  }
}

// --- Data Fetching ---
const fetchData = async () => {
  loading.value = true
  try {
    const departmentsData = await DepartmentService.getAllDepartments()
    departments.value = departmentsData.map(dept => ({
      id: dept.key,
      name: dept.name,
      description: dept.description,
      ministers: dept.staffs?.filter((staff: any) => staff.identity === 'President' || staff.identity === 'Minister') || [],
      members: dept.staffs?.filter((staff: any) => staff.identity === 'Department') || [],
      projects: (dept.projects || []).map(project => ({
        id: project.id,
        title: project.name,
        description: project.description,
        department: {id: 0, name: project.department}
      }))
    } as Department))

    staffs.value = await StaffService.getAllStaff()
    ministers.value = staffs.value.filter(staff => staff.identity === 'President')
    members.value = staffs.value.filter(staff => staff.identity !== 'Founder')

    const projectsData = await ProjectService.getAllProjects()
    projects.value = projectsData.map(project => ({
      id: project.id,
      title: project.name,
      description: project.description,
      department: {id: 0, name: project.department}
    })) as Project[]

  } catch (error: any) {
    console.error(error)
    message.error('数据加载失败')
  } finally {
    loading.value = false
    // Trigger chart render after data is ready
    await nextTick(() => renderAllCharts())
  }
}

// --- Charts ---

const departmentData = computed(() => {
  if (!staffs.value.length) return []
  const map: Record<string, number> = {}
  departments.value.forEach(dept => {
    map[dept.name] = (dept.members?.length || 0) + (dept.ministers?.length || 0)
  })
  return Object.entries(map).map(([name, value]) => ({name, value}))
})

const collegeData = computed(() => {
  if (!staffs.value.length) return []
  const map: Record<string, number> = {}
  staffs.value.forEach(s => {
    const c = s.academy || '未知';
    map[c] = (map[c] || 0) + 1
  })
  return Object.entries(map).map(([name, value]) => ({name, value}))
})

const genderData = computed(() => {
  if (!staffs.value.length) return []
  const map = {'男': 0, '女': 0}
  staffs.value.forEach(s => {
    if (s.gender === '男') map['男']++; else if (s.gender === '女') map['女']++
  })
  return Object.entries(map).map(([name, value]) => ({name, value}))
})

const initChart = (id: string, options: any) => {
  const dom = document.getElementById(id)
  if (!dom) return
  const chart = echarts.init(dom as HTMLElement)
  chart.setOption(options)

  // Auto Resize
  const resizeHandler = () => chart.resize()
  window.addEventListener('resize', resizeHandler)
  // Store implementation to remove listener later if needed (skipped for this simple implementation)
}

const getCommonChartOptions = (data: any[], name: string) => {
  // Check if dark mode is likely active by checking body class or text color,
  // but here we'll just use neutral colors that work on both or slightly transparent.
  // For true adaptive ECharts, passing a theme is better.
  return {
    tooltip: {trigger: 'item', backgroundColor: 'rgba(255,255,255,0.95)', borderRadius: 8, textStyle: {color: '#333'}},
    legend: {bottom: 0, left: 'center', textStyle: {color: 'inherit'}, icon: 'circle'}, // Inherit css color doesn't always work in canvas, use transparent logic or simple gray
    series: [{
      name: name,
      type: 'pie',
      radius: ['40%', '65%'],
      center: ['50%', '45%'],
      itemStyle: {borderRadius: 8, borderColor: 'rgba(0,0,0,0)', borderWidth: 2},
      label: {show: false},
      data: data
    }]
  }
}

const renderAllCharts = () => {
  initChart('departmentChart', getCommonChartOptions(departmentData.value, '部门分布'))
  initChart('collegeChart', getCommonChartOptions(collegeData.value, '学院分布'))
  initChart('genderChart', getCommonChartOptions(genderData.value, '男女比例'))
}

watch([departmentData, collegeData, genderData], () => renderAllCharts())

const handleTabChange = (name: string) => {
  if (name === 'overview') nextTick(() => renderAllCharts())
}

// --- Lifecycle ---
onMounted(() => {
  fetchData()
  layoutStore.setPageHeader('社团中枢', '组织架构与人员管理')
  layoutStore.setShowPageActions(true)

  const ActionsComponent = defineComponent({
    setup() {
      return () => h('button', {
        class: 'px-4 py-2 rounded-full bg-blue-600 hover:bg-blue-700 text-white transition-transform active:scale-95 flex items-center text-sm font-medium shadow-lg shadow-blue-500/30',
        onClick: () => openDepartment()
      }, [h(Icon, {icon: 'ion:add', class: 'mr-1'}), '新建部门'])
    }
  })
  layoutStore.setActionsComponent(ActionsComponent);
})

onBeforeUnmount(() => {
  layoutStore.clearPageHeader()
})

</script>

<style scoped>
/* 原生 CSS 适配暗黑模式 */
/* Apple Style Base */
.apple-container {
  background-color: #F1F4F9; /* iCloud light gray */
}

.apple-card {
  background-color: rgba(255, 255, 255, 0.65);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.03);
}

.apple-sub-card {
  background-color: #FFFFFF;
  border-radius: 24px; /* Apple uses larger border radiuses now */
  border: 1px solid rgba(0, 0, 0, 0.02); /* Very subtle border */
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.apple-sub-card:hover {
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
}

/* Buttons */
.apple-btn {
  padding: 8px 16px;
  border-radius: 9999px; /* Pill shape */
  font-weight: 500;
  font-size: 14px;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  display: inline-flex;
  align-items: center;
}

.apple-btn:active {
  transform: scale(0.96);
}

.apple-btn.primary {
  background-color: #007AFF; /* System Blue */
  color: white;
  box-shadow: 0 2px 6px rgba(0, 122, 255, 0.3);
}

.apple-btn.secondary {
  background-color: rgba(0, 0, 0, 0.05);
  color: #1d1d1f;
}

.apple-btn.danger {
  background-color: rgba(255, 59, 48, 0.1);
  color: #FF3B30;
}

.apple-btn-sm {
  padding: 4px 10px;
  font-size: 12px;
  border-radius: 8px;
  font-weight: 500;
  transition: opacity 0.2s;
}

.apple-btn-sm.secondary {
  background-color: #f5f5f7;
  color: #666;
}

.apple-btn-sm.primary {
  background-color: #eef6ff;
  color: #007AFF;
}

.apple-btn-sm.danger {
  background-color: #fff2f2;
  color: #FF3B30;
}

.apple-icon-btn {
  padding: 4px;
  border-radius: 50%;
  transition: background-color 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.apple-icon-btn:hover {
  background-color: rgba(0, 0, 0, 0.05);
}

/* Chips */
.apple-chip {
  display: inline-flex;
  align-items: center;
  padding: 4px 12px;
  border-radius: 99px;
  font-size: 13px;
  background-color: #F5F5F7;
  color: #1D1D1F;
  transition: background-color 0.2s;
}

.apple-chip.large {
  padding: 6px 16px;
  font-size: 14px;
}

.apple-chip.blue {
  background-color: #F0F8FF;
  color: #007AFF;
}

/* Item Card (Project) */
.apple-item-card {
  background-color: #FFFFFF;
  border-radius: 20px;
  padding: 20px;
  border: 1px solid transparent;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  transition: all 0.3s ease;
}

.apple-item-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.08);
  border-color: rgba(0, 122, 255, 0.1);
}

/* Titles */
.section-title {
  font-size: 20px;
  font-weight: 600;
  color: #1d1d1f;
  letter-spacing: -0.01em;
}

/* Table overrides */
:deep(.apple-table .n-data-table-th) {
  background-color: transparent;
  border-bottom: 1px solid #f0f0f0;
  font-weight: 600;
  color: #86868b;
}

:deep(.apple-table .n-data-table-td) {
  background-color: transparent;
  border-bottom: 1px solid #f5f5f7;
  padding: 16px 12px;
}

:deep(.apple-table .n-data-table-tr:last-child .n-data-table-td) {
  border-bottom: none;
}

/* Modal Override */
:deep(.apple-modal.n-modal) {
  border-radius: 24px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
}

:deep(.apple-modal .n-card-header) {
  border-bottom: none;
  padding-top: 28px;
  padding-left: 28px;
}

:deep(.apple-modal .n-card-header__main) {
  font-size: 22px;
  font-weight: 600;
}

/* DARK MODE */
.dark .apple-container {
  background-color: #000000; /* Pure black for heavy contrast */
}

.dark .apple-card {
  background-color: rgba(28, 28, 30, 0.6);
  border-color: rgba(255, 255, 255, 0.1);
}

.dark .apple-sub-card {
  background-color: #1C1C1E; /* System Gray 6 Dark */
  border-color: rgba(255, 255, 255, 0.05);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.dark .apple-sub-card:hover {
  background-color: #242426;
}

.dark .apple-item-card {
  background-color: #1C1C1E;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.dark .apple-item-card:hover {
  background-color: #2C2C2E;
}

.dark .section-title {
  color: #F5F5F7;
}

.dark .apple-btn.secondary {
  background-color: rgba(255, 255, 255, 0.1);
  color: #F5F5F7;
}

.dark .apple-chip {
  background-color: #2C2C2E;
  color: #E5E5E7;
}

.dark .apple-chip.blue {
  background-color: rgba(10, 132, 255, 0.15);
  color: #64D2FF;
}

.dark .apple-icon-btn:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

/* Table Dark */
.dark :deep(.apple-table .n-data-table-th) {
  border-bottom: 1px solid #38383A;
  color: #98989D;
}

.dark :deep(.apple-table .n-data-table-td) {
  border-bottom: 1px solid #2C2C2E;
  color: #D1D1D6;
}

.dark :deep(.apple-modal.n-card) {
  background-color: #1C1C1E;
  border: 1px solid #38383A;
}

/* Animation Utility */
.animate-fade-in {
  animation: fadeIn 0.4s cubic-bezier(0.4, 0, 0.2, 1) forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <!-- 页面头部 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="flex items-center space-x-3">
          <n-button
              text
              circle
              @click="goBack"
              class="hover:bg-gray-200 dark:hover:bg-gray-700 rounded-full"
          >
            <n-icon size="24">
              <ChevronBack/>
            </n-icon>
          </n-button>
          <div>
            <h1 class="text-2xl font-semibold tracking-tight">社团部门</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">社团部门管理</p>
          </div>
        </div>

        <div class="flex flex-wrap gap-2">
          <n-button
              secondary
              strong
              @click="downloadData"
          >
            <template #icon>
              <n-icon>
                <Download/>
              </n-icon>
            </template>
            下载文件
          </n-button>
          <n-button
              secondary
              strong
              @click="uploadData"
          >
            <template #icon>
              <n-icon>
                <Upload/>
              </n-icon>
            </template>
            上传数据
          </n-button>
          <n-button
              type="primary"
              @click="() => openDepartment()"
          >
            <template #icon>
              <n-icon>
                <Plus/>
              </n-icon>
            </template>
            添加部门
          </n-button>
        </div>
      </div>
    </div>

    <!-- 主内容区 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-12">
      <n-card
          class="overflow-hidden bg-white dark:bg-gray-800 border-0 rounded-2xl shadow-sm"
          content-style="padding: 0;"
      >
        <!-- 标签页导航 -->
        <n-tabs
            type="line"
            animated
            class="w-full"
            :tabs-padding="20"
            @update:value="handleTabChange"
        >
          <!-- 总览标签页 -->
          <n-tab-pane name="overview" tab="总览">
            <div class="p-6 space-y-8">
              <!-- 领导层部分 -->
              <section>
                <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-4">
                  <h2 class="text-lg font-medium">社长/团支书/秘书长</h2>
                  <div class="flex gap-2">
                    <n-button
                        secondary
                        strong
                        size="small"
                        @click="() => openAddMember()"
                    >
                      添加成员
                    </n-button>
                    <n-button
                        type="error"
                        secondary
                        strong
                        size="small"
                        @click="deleteAllMinisters"
                    >
                      全部删除
                    </n-button>
                  </div>
                </div>

                <div class="flex flex-wrap gap-2 mt-4">
                  <n-tag
                      v-for="member in ministers"
                      :key="member.userId"
                      type="primary"
                      closable
                      @close="() => deleteMember(member, ministers)"
                      class="bg-blue-100 hover:bg-blue-200 dark:bg-blue-900/30 dark:hover:bg-blue-900/50 text-blue-800 dark:text-blue-200 cursor-pointer rounded-full px-3 py-1"
                  >
                    {{ member.userName }}
                  </n-tag>
                  <n-tag
                      v-if="ministers.length === 0"
                      type="default"
                      class="bg-gray-100 dark:bg-gray-700 text-gray-500 dark:text-gray-300 rounded-full px-3 py-1"
                  >
                    暂无领导成员
                  </n-tag>
                </div>
              </section>

              <!-- 成员列表 -->
              <section>
                <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-4">
                  <h2 class="text-lg font-medium">成员</h2>
                  <div class="flex flex-col sm:flex-row sm:items-center gap-4">
                    <div class="flex items-center text-sm text-gray-500 dark:text-gray-400">
                      <n-icon class="mr-1" size="16">
                        <Users/>
                      </n-icon>
                      <span>所有部员: {{ members.length }}</span>
                    </div>
                    <n-button
                        type="primary"
                        secondary
                        strong
                        size="small"
                        @click="downloadMemberInfo"
                    >
                      下载部员信息
                    </n-button>
                  </div>
                </div>

                <div class="bg-gray-100 dark:bg-gray-700/50 rounded-xl overflow-hidden">
                  <n-data-table
                      :columns="memberColumns"
                      :data="members"
                      :pagination="pagination"
                      :bordered="false"
                      :single-line="false"
                      class="min-w-full"
                  />
                </div>
              </section>

              <!-- 项目卡片 -->
              <section>
                <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
                  <h2 class="text-lg font-medium">项目</h2>
                  <n-button
                      type="primary"
                      @click="addProject"
                  >
                    <template #icon>
                      <n-icon>
                        <Plus/>
                      </n-icon>
                    </template>
                    添加
                  </n-button>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
                  <div
                      v-for="project in projects"
                      :key="project.id"
                      class="bg-white dark:bg-gray-700 rounded-xl shadow-sm hover:shadow-md transition-all duration-300 p-5 border border-gray-200 dark:border-gray-600"
                  >
                    <div class="flex justify-between items-start mb-3">
                      <h3 class="font-medium text-gray-900 dark:text-gray-100">{{ project.title }}</h3>
                      <n-tag
                          v-if="project.department && project.department.name"
                          type="info"
                          class="bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-200 rounded-full"
                          size="small"
                      >
                        {{ project.department.name }}
                      </n-tag>
                    </div>
                    <p class="text-sm text-gray-500 dark:text-gray-400 mb-4 line-clamp-2">{{ project.description }}</p>
                    <div class="flex justify-end gap-2">
                      <n-button text size="small">去看看</n-button>
                      <n-button size="small" @click="() => editProject(project)">编辑</n-button>
                      <n-button type="error" secondary size="small" @click="() => deleteProject(project)">删除
                      </n-button>
                    </div>
                  </div>
                  <div
                      v-if="projects.length === 0"
                      class="col-span-full flex flex-col items-center justify-center py-12 text-gray-400 dark:text-gray-500"
                  >
                    <n-empty description="暂无项目" size="large"/>
                    <p class="mt-2">还没有添加任何项目</p>
                  </div>
                </div>
              </section>

              <!-- 数据统计卡片 -->
              <section>
                <h2 class="text-lg font-medium mb-4">数据统计</h2>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
                  <!-- 部门分布 -->
                  <div
                      class="bg-white dark:bg-gray-700 rounded-xl shadow-sm p-5 border border-gray-200 dark:border-gray-600">
                    <h3 class="text-sm font-medium mb-3 text-gray-600 dark:text-gray-300">部门分布</h3>
                    <div class="h-80 flex items-center justify-center">
                      <n-empty
                          v-if="!departmentData || departmentData.length === 0"
                          description="暂无数据"
                          size="small"
                      />
                      <div v-else class="w-full h-full">
                        <div
                            id="departmentChart"
                            class="w-full h-full"
                        ></div>
                      </div>
                    </div>
                  </div>

                  <!-- 学院分布 -->
                  <div
                      class="bg-white dark:bg-gray-700 rounded-xl shadow-sm p-5 border border-gray-200 dark:border-gray-600">
                    <h3 class="text-sm font-medium mb-3 text-gray-600 dark:text-gray-300">学院分布</h3>
                    <div class="h-80 flex items-center justify-center">
                      <n-empty
                          v-if="!collegeData || collegeData.length === 0"
                          description="暂无数据"
                          size="small"
                      />
                      <div v-else class="w-full h-full">
                        <div
                            id="collegeChart"
                            class="w-full h-full"
                        ></div>
                      </div>
                    </div>
                  </div>

                  <!-- 男女比例 -->
                  <div
                      class="bg-white dark:bg-gray-700 rounded-xl shadow-sm p-5 border border-gray-200 dark:border-gray-600">
                    <h3 class="text-sm font-medium mb-3 text-gray-600 dark:text-gray-300">男女比例</h3>
                    <div class="h-80 flex items-center justify-center">
                      <n-empty
                          v-if="!genderData || genderData.length === 0"
                          description="暂无数据"
                          size="small"
                      />
                      <div v-else class="w-full h-full">
                        <div
                            id="genderChart"
                            class="w-full h-full"
                        ></div>
                      </div>
                    </div>
                  </div>
                </div>
              </section>
            </div>
          </n-tab-pane>

          <!-- 各部门标签页 -->
          <n-tab-pane
              v-for="department in departments"
              :key="department.id"
              :name="department.id"
              :tab="department.name"
          >
            <div class="p-6 space-y-8">
              <!-- 部门信息头部 -->
              <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-4">
                <div>
                  <h2 class="text-xl font-medium">{{ department.name }}</h2>
                  <p class="text-sm text-gray-500 dark:text-gray-400 mt-1 max-w-2xl">{{ department.description }}</p>
                </div>
                <div class="flex flex-wrap gap-2">
                  <n-button
                      secondary
                      strong
                      size="small"
                      @click="() => openAddMember(department)"
                  >
                    添加部长
                  </n-button>
                  <n-button
                      type="error"
                      secondary
                      strong
                      size="small"
                      @click="() => deleteAll(department.ministers)"
                  >
                    删除所有部长
                  </n-button>
                  <n-button
                      type="primary"
                      secondary
                      strong
                      size="small"
                      @click="() => openDepartment(department)"
                  >
                    编辑部门
                  </n-button>
                  <n-button
                      type="error"
                      secondary
                      strong
                      size="small"
                      @click="() => deleteDepartment(department)"
                  >
                    删除部门
                  </n-button>
                </div>
              </div>

              <!-- 部长列表 -->
              <section>
                <h3 class="text-lg font-medium mb-3">部长/副部长</h3>
                <div class="flex flex-wrap gap-2">
                  <n-tag
                      v-for="member in department.ministers"
                      :key="member.userId"
                      type="primary"
                      closable
                      @close="() => deleteMember(member, department.ministers)"
                      class="bg-blue-100 hover:bg-blue-200 dark:bg-blue-900/30 dark:hover:bg-blue-900/50 text-blue-800 dark:text-blue-200 cursor-pointer rounded-full px-3 py-1"
                  >
                    {{ member.name }}
                  </n-tag>
                  <n-tag
                      v-if="!department.ministers || department.ministers.length === 0"
                      type="default"
                      class="bg-gray-100 dark:bg-gray-700 text-gray-500 dark:text-gray-300 rounded-full px-3 py-1"
                  >
                    暂无部长
                  </n-tag>
                </div>
              </section>

              <!-- 部门成员 -->
              <section>
                <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-4">
                  <h3 class="text-lg font-medium">成员</h3>
                  <div class="flex gap-2">
                    <n-button
                        secondary
                        strong
                        size="small"
                        @click="() => openAddMember(department, 'Department')"
                    >
                      添加成员
                    </n-button>
                    <n-button
                        type="error"
                        secondary
                        strong
                        size="small"
                        @click="() => deleteAll(department.members)"
                    >
                      全部删除
                    </n-button>
                  </div>
                </div>

                <div class="bg-gray-100 dark:bg-gray-700/50 rounded-xl overflow-hidden">
                  <n-data-table
                      :columns="staffColumns"
                      :data="department.members"
                      :pagination="pagination"
                      :bordered="false"
                      :single-line="false"
                      class="min-w-full"
                  />
                </div>
              </section>

              <!-- 部门项目 -->
              <section>
                <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
                  <h3 class="text-lg font-medium">项目</h3>
                  <n-button
                      type="primary"
                      @click="addProject"
                  >
                    <template #icon>
                      <n-icon>
                        <Plus/>
                      </n-icon>
                    </template>
                    添加
                  </n-button>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
                  <div
                      v-for="project in department.projects"
                      :key="project.id"
                      class="bg-white dark:bg-gray-700 rounded-xl shadow-sm hover:shadow-md transition-all duration-300 p-5 border border-gray-200 dark:border-gray-600"
                  >
                    <h3 class="font-medium text-gray-900 dark:text-gray-100 mb-3">{{ project.title }}</h3>
                    <p class="text-sm text-gray-500 dark:text-gray-400 mb-4 line-clamp-2">{{ project.description }}</p>
                    <div class="flex justify-end gap-2">
                      <n-button text size="small">去看看</n-button>
                      <n-button size="small" @click="() => editProject(project)">编辑</n-button>
                      <n-button type="error" secondary size="small"
                                @click="() => deleteProject(project, department.projects)">删除
                      </n-button>
                    </div>
                  </div>
                  <div
                      v-if="!department.projects || department.projects.length === 0"
                      class="col-span-full flex flex-col items-center justify-center py-12 text-gray-400 dark:text-gray-500"
                  >
                    <n-empty description="暂无项目" size="large"/>
                    <p class="mt-2">该部门还没有添加任何项目</p>
                  </div>
                </div>
              </section>
            </div>
          </n-tab-pane>
        </n-tabs>
      </n-card>
    </div>

    <!-- 添加成员模态框 -->
    <n-modal
        v-model:show="showAddMemberModal"
        preset="card"
        style="width: 90%; max-width: 600px;"
        title="添加成员"
        :bordered="false"
        class="rounded-2xl overflow-hidden"
    >
      <div class="space-y-4">
        <div class="flex gap-2">
          <n-input
              v-model:value="searchKeyword"
              placeholder="搜索成员（姓名/学号）"
              @keyup.enter="searchMembers"
              clearable
          >
            <template #prefix>
              <n-icon>
                <Search/>
              </n-icon>
            </template>
          </n-input>
          <n-button type="primary" @click="searchMembers">搜索</n-button>
        </div>

        <n-data-table
            v-if="searchResults.length > 0"
            :columns="searchColumns"
            :data="searchResults"
            class="mt-2"
            :bordered="false"
            :pagination="false"
            :single-line="false"
        />

        <div v-else-if="searchKeyword && searchResults.length === 0"
             class="text-center py-8 text-gray-500 dark:text-gray-400">
          <n-empty description="未找到相关成员"/>
        </div>

        <div v-else class="text-center py-8 text-gray-500 dark:text-gray-400">
          <n-empty description="请输入关键词搜索成员">
            <template #icon>
              <n-icon>
                <Search/>
              </n-icon>
            </template>
          </n-empty>
        </div>
      </div>
      <template #footer>
        <div class="flex justify-end gap-2">
          <n-button @click="showAddMemberModal = false">取消</n-button>
        </div>
      </template>
    </n-modal>

    <!-- 添加或更改部门模态框 -->
    <n-modal
        v-model:show="showDepartmentModal"
        preset="card"
        style="width: 90%; max-width: 500px;"
        :title="editingDepartment ? '编辑部门' : '添加部门'"
        :bordered="false"
        class="rounded-2xl overflow-hidden"
    >
      <n-form
          :model="departmentForm"
          :rules="departmentRules"
          ref="departmentFormRef"
          class="space-y-4"
      >
        <n-form-item label="部门名称" path="name">
          <n-input
              v-model:value="departmentForm.name"
              placeholder="请输入部门名称"
          />
        </n-form-item>
        <n-form-item label="部门简介" path="description">
          <n-input
              v-model:value="departmentForm.description"
              placeholder="请输入部门简介"
              type="textarea"
              :autosize="{ minRows: 3, maxRows: 5 }"
          />
        </n-form-item>
      </n-form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <n-button @click="showDepartmentModal = false">取消</n-button>
          <n-button
              type="primary"
              @click="saveDepartment"
          >
            {{ editingDepartment ? '保存' : '添加' }}
          </n-button>
        </div>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import {ref, onMounted, h, computed, nextTick, watch} from 'vue'
import {useRouter} from 'vue-router'
import {
  useMessage,
  NButton,
  NCard,
  NTabs,
  NTabPane,
  NInput,
  NForm,
  NFormItem,
  NDataTable,
  NModal,
  NTag,
  NEmpty,
  NIcon,
  type DataTableColumns
} from 'naive-ui'
import {People as Users, Download, CloudUpload as Upload, Add as Plus, ChevronBack, Search} from '@vicons/ionicons5'
import {DepartmentService} from '../services/DepartmentService'
import {StaffService} from '../services/StaffService'
import {ProjectService} from '../services/ProjectService'
import type {Department, DepartmentModel, MemberModel, Project} from '../models'
import * as echarts from 'echarts'

const router = useRouter()
const message = useMessage()

// 数据状态
const ministers = ref<MemberModel[]>([])
const members = ref<MemberModel[]>([])
const projects = ref<Project[]>([])
const departments = ref<Department[]>([])
const staffs = ref<MemberModel[]>([])

// 模态框状态
const showAddMemberModal = ref(false)
const showDepartmentModal = ref(false)
const searchKeyword = ref('')
const searchResults = ref([])
const departmentFormRef = ref<InstanceType<typeof NForm> | null>(null)

// 表单数据
const departmentForm = ref({
  name: '',
  description: ''
})

const editingDepartment = ref<Department | null>(null)
const currentDepartment = ref<Department | null>(null)

// 表格分页
const pagination = {
  pageSize: 10
}

// 表格列定义
const memberColumns: DataTableColumns<MemberModel> = [
  {title: '姓名', key: 'userName', width: 100},
  {title: '学号', key: 'userId', width: 120},
  {title: '学院', key: 'academy', width: 150},
  {title: '政治面貌', key: 'politicalLandscape', width: 100},
  {title: '性别', key: 'gender', width: 60},
  {title: '专业班级', key: 'className', width: 120},
  {title: '手机号码', key: 'phoneNum', width: 120},
  {
    title: '身份', key: 'identity', width: 80,
  },
  {
    title: '操作',
    key: 'actions',
    width: 80,
    render: (row) => h(NButton, {
      type: 'error',
      secondary: true,
      size: 'small',
      onClick: () => deleteMember(row)
    }, {default: () => '删除'})
  }
]

const staffColumns: DataTableColumns<MemberModel> = [
  {title: '姓名', key: 'name', width: 100},
  {title: '学号', key: 'userId', width: 120},
  {
    title: '操作',
    key: 'actions',
    width: 80,
    render: (row) => h(NButton, {
      type: 'error',
      secondary: true,
      size: 'small',
      onClick: () => deleteStaff(row)
    }, {default: () => '删除'})
  }
]

const searchColumns: DataTableColumns<any> = [
  {title: '姓名', key: 'userName', width: 100},
  {title: '学号', key: 'userId', width: 120},
  {title: '学院', key: 'academy', width: 150},
  {
    title: '操作',
    key: 'actions',
    width: 80,
    render: (row) => h(NButton, {
      type: 'primary',
      secondary: true,
      size: 'small',
      onClick: () => addMember(row)
    }, {default: () => '添加'})
  }
]

// 表单验证规则
const departmentRules = {
  name: {
    required: true,
    message: '请输入部门名称',
    trigger: 'blur'
  },
  description: {
    required: true,
    message: '请输入部门简介',
    trigger: 'blur'
  }
}

// 导航方法
const goBack = () => {
  router.push('/Centre')
}

// 操作方法
const downloadData = () => {
  console.log('下载文件')
  message.success('下载文件功能待实现')
}

const uploadData = () => {
  console.log('上传Json数据')
  message.success('上传数据功能待实现')
}

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

const openAddMember = (department: Department | null = null, type = '') => {
  currentDepartment.value = department
  showAddMemberModal.value = true
  searchKeyword.value = ''
  searchResults.value = []
}

const deleteAllMinisters = () => {
  // 实际应用中应该添加确认对话框
  ministers.value = []
  message.success('所有部长已删除')
}

const deleteAll = (list: any[] | undefined) => {
  // 实际应用中应该添加确认对话框
  if (list && Array.isArray(list)) {
    list.length = 0
    message.success('所有成员已删除')
  }
}

const deleteMember = (member: MemberModel, list?: MemberModel[]) => {
  if (list && Array.isArray(list)) {
    const index = list.findIndex(m => m.userId === member.userId)
    if (index > -1) {
      list.splice(index, 1)
      message.success('成员已删除')
    }
  } else {
    const index = members.value.findIndex(m => m.userId === member.userId)
    if (index > -1) {
      members.value.splice(index, 1)
      message.success('成员已删除')
    }
  }
}

const deleteStaff = (staff: MemberModel) => {
  message.success('部员已删除')
}

const downloadMemberInfo = () => {
  console.log('下载部员信息')
  message.success('下载部员信息功能待实现')
}

const addProject = () => {
  console.log('添加项目')
  message.success('添加项目功能待实现')
}

const editProject = (project: Project) => {
  console.log('编辑项目', project)
  message.success('编辑项目功能待实现')
}

const deleteProject = (project: Project, list?: Project[]) => {
  if (list && Array.isArray(list)) {
    const index = list.findIndex(p => p.id === project.id)
    if (index > -1) {
      list.splice(index, 1)
      message.success('项目已删除')
    }
  } else {
    const index = projects.value.findIndex(p => p.id === project.id)
    if (index > -1) {
      projects.value.splice(index, 1)
      message.success('项目已删除')
    }
  }
}

const deleteDepartment = async (department: Department) => {
  try {
    // 实际应用中应该添加确认对话框
    await DepartmentService.deleteDepartment(department.name)
    const index = departments.value.findIndex(d => d.id === department.id)
    if (index > -1) {
      departments.value.splice(index, 1)
      message.success('部门已删除')
    }
  } catch (error: any) {
    console.error('删除部门时发生错误:', error)
    message.error('删除部门时发生错误: ' + (error.message || '未知错误'))
  }
}

const searchMembers = async () => {
  if (!searchKeyword.value) {
    searchResults.value = []
    return
  }

  try {
    // 这里应该使用成员查询服务，暂时保留原逻辑
    const token = localStorage.getItem('Authorization')
    const response = await fetch(`https://www.xauat.site/api/Member/Search?keyword=${encodeURIComponent(searchKeyword.value)}`, {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (response.ok) {
      const data = await response.json()
      searchResults.value = data.map((item: any) => ({
        id: item.id,
        userName: item.userName,
        userId: item.userId,
        academy: item.academy
      }))
    } else {
      message.error('搜索成员失败')
    }
  } catch (error) {
    console.error('搜索成员时发生错误:', error)
    message.error('搜索成员时发生错误')
  }
}

const addMember = (member: any) => {
  if (currentDepartment.value) {
    // 添加到特定部门
    if (!currentDepartment.value.ministers) {
      currentDepartment.value.ministers = []
    }
    currentDepartment.value.ministers.push(member)
    message.success(`已添加${member.userName}到${currentDepartment.value.name}`)
  } else {
    // 添加到领导层
    ministers.value.push(member)
    message.success(`已添加${member.userName}到领导层`)
  }
  showAddMemberModal.value = false
}

const saveDepartment = async () => {
  try {
    await departmentFormRef.value?.validate()

    const departmentData = {
      key: editingDepartment.value?.id.toString || '',
      name: departmentForm.value.name,
      description: departmentForm.value.description
    } as DepartmentModel

    if (editingDepartment.value) {
      // 更新部门
      await DepartmentService.updateDepartment(departmentData)
      // 更新本地数据
      if (editingDepartment.value) {
        editingDepartment.value.name = departmentForm.value.name
        editingDepartment.value.description = departmentForm.value.description
      }
      message.success('部门更新成功')
    } else {
      // 创建部门
      await DepartmentService.createDepartment(departmentData)
      // 重新获取部门列表以获取新创建的部门
      await fetchData()
      message.success('部门添加成功')
    }

    showDepartmentModal.value = false
  } catch (error: any) {
    console.error('保存部门时发生错误:', error)
    message.error('保存部门时发生错误: ' + (error.message || '未知错误'))
  }
}

// 获取数据
const fetchData = async () => {
  try {
    // 获取所有部门信息
    const departmentsData = await DepartmentService.getAllDepartments()
    departments.value = departmentsData.map(dept => ({
      id: dept.key, // 需要根据实际情况调整
      name: dept.name,
      description: dept.description,
      ministers: dept.staffs?.filter((staff: any) =>
          staff.identity === 'President' || staff.identity === 'Minister') || [],
      members: dept.staffs?.filter((staff: any) =>
          staff.identity === 'Department') || [],
      projects: (dept.projects || []).map(project => ({
        id: parseInt(project.id),
        title: project.name,
        description: project.description,
        department: {
          id: 0, // 需要根据实际情况调整
          name: project.department
        }
      }))
    } as Department))

    // 获取员工信息
    staffs.value = await StaffService.getAllStaff()

    // 分离领导层和普通成员
    ministers.value = staffs.value
        .filter(staff => staff.identity === 'President')

    members.value = staffs.value
        .filter(staff => staff.identity !== 'Founder')

    // 获取项目信息
    const projectsData = await ProjectService.getAllProjects()
    projects.value = projectsData.map(project => ({
      id: project.id as unknown as number, // 需要根据实际情况调整
      title: project.name,
      description: project.description,
      department: {
        id: 0, // 需要根据实际情况调整
        name: project.department
      }
    })) as Project[]

  } catch (error: any) {
    console.error('获取部门数据失败:', error)
    message.error('获取数据时发生错误: ' + (error.message || '未知错误'))
  }
}

// 计算属性：部门分布数据
const departmentData = computed(() => {
  if (!staffs.value || staffs.value.length === 0) return []

  const departmentCount: Record<string, number> = {}

  departments.value.forEach(dept => {
    departmentCount[dept.name] = dept.members.length + dept.ministers.length
  })

  // 转换为图表需要的格式
  return Object.entries(departmentCount).map(([name, count]) => ({
    name,
    value: count
  }))
})

// 计算属性：学院分布数据
const collegeData = computed(() => {
  if (!staffs.value || staffs.value.length === 0) return []

  const collegeCount: Record<string, number> = {}

  // 统计各学院人数
  staffs.value.forEach(staff => {
    const college = staff.academy || '未知学院'
    collegeCount[college] = (collegeCount[college] || 0) + 1
  })

  // 转换为图表需要的格式
  return Object.entries(collegeCount).map(([name, count]) => ({
    name,
    value: count
  }))
})

// 计算属性：男女比例数据
const genderData = computed(() => {
  if (!staffs.value || staffs.value.length === 0) return []

  const genderCount: Record<string, number> = {
    男: 0,
    女: 0
  }

  // 统计男女比例
  staffs.value.forEach(staff => {
    if (staff.gender === '男') {
      genderCount['男']++
    } else if (staff.gender === '女') {
      genderCount['女']++
    }
  })

  // 转换为图表需要的格式
  return Object.entries(genderCount).map(([name, count]) => ({
    name,
    value: count
  }))
})

// 图表初始化函数
const initChart = (chartId: string, option: any) => {
  const chartDom = document.getElementById(chartId)
  if (chartDom) {
    const myChart = echarts.init(chartDom)
    myChart.setOption(option)

    // 响应式处理
    window.addEventListener('resize', () => {
      myChart.resize()
    })
  }
}

// 渲染部门分布图表
const renderDepartmentChart = () => {
  const option = {
    tooltip: {
      trigger: 'item'
    },
    legend: {
      bottom: '0%',
      left: 'center'
    },
    series: [
      {
        name: '部门分布',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 20,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: departmentData.value
      }
    ]
  }

  initChart('departmentChart', option)
}

// 渲染学院分布图表
const renderCollegeChart = () => {
  const option = {
    tooltip: {
      trigger: 'item'
    },
    legend: {
      bottom: '0%',
      left: 'center'
    },
    series: [
      {
        name: '学院分布',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 20,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: collegeData.value
      }
    ]
  }

  initChart('collegeChart', option)
}

// 渲染男女比例图表
const renderGenderChart = () => {
  const option = {
    tooltip: {
      trigger: 'item'
    },
    legend: {
      bottom: '0%',
      left: 'center'
    },
    series: [
      {
        name: '男女比例',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 20,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: genderData.value
      }
    ]
  }

  initChart('genderChart', option)
}

// 在数据更新后重新渲染图表
const renderAllCharts = () => {
  nextTick(() => {
    renderDepartmentChart()
    renderCollegeChart()
    renderGenderChart()
  })
}

// 在组件挂载后初始化图表
onMounted(() => {
  fetchData().then(() => {
    renderAllCharts()
  })
})

// 监听数据变化，重新渲染图表
watch([departmentData, collegeData, genderData], () => {
  renderAllCharts()
})

// 监听标签页切换，重新渲染图表
const handleTabChange = (name: string) => {
  if (name === 'overview') {
    nextTick(() => {
      renderAllCharts()
    })
  }
}

</script>

<style scoped>
/* 自定义样式以增强苹果风格 */
.n-button {
  transition: all 0.2s ease;
  border-radius: 8px;
}

.n-card {
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.n-tabs-nav {
  padding: 0 16px;
}

.n-tabs-tab {
  padding: 12px 4px;
  margin-right: 24px;
}

.n-data-table {
  font-size: 14px;
}

.n-data-table .n-data-table-thead .n-data-table-th {
  background-color: transparent;
  font-weight: 500;
  color: #6b7280;
}

.dark .n-data-table .n-data-table-thead .n-data-table-th {
  color: #9ca3af;
}

/* 响应式优化 */
@media (max-width: 640px) {
  .n-tabs-tab {
    margin-right: 16px;
  }
}
</style>
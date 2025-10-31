<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <div class="p-6">
      <n-page-header subtitle="社团部门管理" @back="goBack">
        <template #title>
          <div class="text-2xl font-bold">社团部门</div>
        </template>
        <template #extra>
          <div class="flex flex-wrap gap-2">
            <n-button @click="downloadData">下载文件</n-button>
            <n-button @click="uploadData">上传Json数据</n-button>
            <n-button type="primary" @click="openDepartment">添加部门</n-button>
          </div>
        </template>
        <template #avatar>
          <n-avatar>
            <n-icon>
              <PeopleOutline />
            </n-icon>
          </n-avatar>
        </template>
      </n-page-header>

      <n-card class="mt-6">
        <n-tabs type="line" animated>
          <!-- 总览标签页 -->
          <n-tab-pane name="总览" tab="总览">
            <div class="flex justify-between items-center flex-wrap gap-2 mb-4">
              <h2 class="text-xl font-bold">社长/团支书/秘书长</h2>
              <n-space>
                <n-button @click="openAddMember">添加成员</n-button>
                <n-button type="error" @click="deleteAllMinisters">全部删除</n-button>
              </n-space>
            </div>

            <div class="mb-6">
              <n-space>
                <n-button
                    v-for="member in ministers"
                    :key="member.id"
                    type="primary"
                    @click="() => deleteMember(member, ministers)"
                >
                  {{ member.name }}
                </n-button>
              </n-space>
            </div>

            <n-divider />

            <div class="flex justify-between items-center flex-wrap gap-2 mb-4">
              <h2 class="text-xl font-bold">成员</h2>
              <n-space>
                <n-statistic label="所有部员" :value="members.length">
                  <template #prefix>
                    <n-icon>
                      <PeopleOutline />
                    </n-icon>
                  </template>
                </n-statistic>
                <n-button type="primary" @click="downloadMemberInfo">下载部员信息</n-button>
              </n-space>
            </div>

            <n-data-table
                :columns="memberColumns"
                :data="members"
                :pagination="pagination"
                :bordered="false"
            />

            <n-divider />

            <div class="flex justify-between items-center flex-wrap gap-2 mb-4">
              <h2 class="text-xl font-bold">项目</h2>
              <n-button type="primary" @click="addProject">添加</n-button>
            </div>

            <n-grid :cols="1" :md="2" :lg="3" :x-gap="12" :y-gap="12">
              <n-grid-item v-for="project in projects" :key="project.id">
                <n-card hoverable>
                  <div class="h-48 flex flex-col justify-between">
                    <div>
                      <h3 class="text-lg font-bold">{{ project.title }}</h3>
                      <n-tag v-if="project.department && project.department.name" type="info" class="mt-2">
                        {{ project.department.name }}
                      </n-tag>
                      <p class="text-gray-600 text-sm mt-2">{{ project.description }}</p>
                    </div>
                    <div class="flex gap-2 mt-4">
                      <n-button text>去看看</n-button>
                      <n-button @click="() => editProject(project)">更改项目</n-button>
                      <n-button type="error" @click="() => deleteProject(project)">删除</n-button>
                    </div>
                  </div>
                </n-card>
              </n-grid-item>
            </n-grid>

            <div v-if="projects.length === 0" class="text-center py-8 text-gray-500">
              <n-empty description="没有项目" />
            </div>

            <n-divider />

            <h2 class="text-xl font-bold mb-4">部门分布</h2>
            <div class="h-64">
              <n-empty v-if="!departmentData || departmentData.length === 0" description="暂无数据" />
              <div v-else class="bg-gray-100 rounded p-4 h-full flex items-center justify-center">
                <p>部门分布图表</p>
              </div>
            </div>

            <n-divider />

            <h2 class="text-xl font-bold mb-4">学院分布</h2>
            <div class="h-64">
              <n-empty v-if="!collegeData || collegeData.length === 0" description="暂无数据" />
              <div v-else class="bg-gray-100 rounded p-4 h-full flex items-center justify-center">
                <p>学院分布图表</p>
              </div>
            </div>

            <n-divider />

            <h2 class="text-xl font-bold mb-4">男女比例</h2>
            <div class="h-64">
              <n-empty v-if="!genderData || genderData.length === 0" description="暂无数据" />
              <div v-else class="bg-gray-100 rounded p-4 h-full flex items-center justify-center">
                <p>男女比例图表</p>
              </div>
            </div>
          </n-tab-pane>

          <!-- 各部门标签页 -->
          <n-tab-pane
              v-for="department in departments"
              :key="department.id"
              :name="department.name"
              :tab="department.name"
          >
            <div class="flex justify-between items-center flex-wrap gap-2 mb-4">
              <h2 class="text-xl font-bold">部长/副部长</h2>
              <n-space>
                <n-button @click="() => openAddMember(department)">添加成员</n-button>
                <n-button type="error" @click="() => deleteAll(department.ministers)">全部删除</n-button>
                <n-button type="primary" @click="() => openDepartment(department)">更改部门</n-button>
                <n-button type="error" @click="() => deleteDepartment(department)">删除部门</n-button>
              </n-space>
            </div>

            <div class="mb-6">
              <n-space>
                <n-button
                    v-for="member in department.ministers"
                    :key="member.id"
                    type="primary"
                    @click="() => deleteMember(member, department.ministers)"
                >
                  {{ member.name }}
                </n-button>
              </n-space>
            </div>

            <n-divider />

            <div class="flex justify-between items-center flex-wrap gap-2 mb-4">
              <h2 class="text-xl font-bold">成员</h2>
              <n-space>
                <n-button @click="() => openAddMember(department, 'Department')">添加成员</n-button>
                <n-button type="error" @click="() => deleteAll(department.ministers)">全部删除</n-button>
              </n-space>
            </div>

            <n-data-table
                :columns="staffColumns"
                :data="department.members"
                :pagination="pagination"
                :bordered="false"
            />

            <n-divider />

            <div class="flex justify-between items-center flex-wrap gap-2 mb-4">
              <h2 class="text-xl font-bold">项目</h2>
              <n-button type="primary" @click="addProject">添加</n-button>
            </div>

            <n-grid :cols="1" :md="2" :lg="3" :x-gap="12" :y-gap="12">
              <n-grid-item v-for="project in department.projects" :key="project.id">
                <n-card hoverable>
                  <div class="h-48 flex flex-col justify-between">
                    <div>
                      <h3 class="text-lg font-bold">{{ project.title }}</h3>
                      <n-tag v-if="project.department && project.department.name" type="info" class="mt-2">
                        {{ project.department.name }}
                      </n-tag>
                      <p class="text-gray-600 text-sm mt-2">{{ project.description }}</p>
                    </div>
                    <div class="flex gap-2 mt-4">
                      <n-button text>去看看</n-button>
                      <n-button @click="() => editProject(project)">更改项目</n-button>
                      <n-button type="error" @click="() => deleteProject(project, department.projects)">删除</n-button>
                    </div>
                  </div>
                </n-card>
              </n-grid-item>
            </n-grid>

            <div v-if="!department.projects || department.projects.length === 0" class="text-center py-8 text-gray-500">
              <n-empty description="没有项目" />
            </div>
          </n-tab-pane>
        </n-tabs>
      </n-card>

      <!-- 添加成员模态框 -->
      <n-modal v-model:show="showAddMemberModal" preset="card" style="width: 600px;" title="添加成员">
        <n-input v-model:value="searchKeyword" placeholder="搜索成员" @keyup.enter="searchMembers" />
        <n-button class="mt-2" @click="searchMembers">搜索</n-button>

        <n-data-table
            v-if="searchResults.length > 0"
            :columns="searchColumns"
            :data="searchResults"
            class="mt-4"
        />

        <div v-else class="text-center py-8 text-gray-500">
          <n-empty description="请输入关键词搜索成员" />
        </div>
      </n-modal>

      <!-- 添加或更改部门模态框 -->
      <n-modal v-model:show="showDepartmentModal" preset="card" style="width: 600px;" :title="editingDepartment ? '更改部门' : '添加部门'">
        <n-form :model="departmentForm" :rules="departmentRules" ref="departmentFormRef">
          <n-form-item label="部门名称" path="name">
            <n-input v-model:value="departmentForm.name" placeholder="请输入部门名称" />
          </n-form-item>
          <n-form-item label="部门简介" path="description">
            <n-input v-model:value="departmentForm.description" placeholder="请输入部门简介" type="textarea" />
          </n-form-item>
          <n-form-item>
            <n-button type="primary" @click="saveDepartment" block>
              {{ editingDepartment ? '更改' : '添加' }}
            </n-button>
          </n-form-item>
        </n-form>
      </n-modal>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, h } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthorizationStore } from '../stores/Authorization.js'
import {
  NPageHeader,
  NCard,
  NTabs,
  NTabPane,
  NButton,
  NAvatar,
  NIcon,
  NSpace,
  NDataTable,
  NGrid,
  NGridItem,
  NTag,
  NDivider,
  NModal,
  NInput,
  NForm,
  NFormItem,
  NStatistic,
  NEmpty,
  useMessage
} from 'naive-ui'
import { PeopleOutline } from '@vicons/ionicons5'

const router = useRouter()
const authorizationStore = useAuthorizationStore()
const message = useMessage()

// 数据状态
const ministers = ref([])
const members = ref([])
const projects = ref([])
const departments = ref([])
const departmentData = ref([])
const collegeData = ref([])
const genderData = ref([])

// 模态框状态
const showAddMemberModal = ref(false)
const showDepartmentModal = ref(false)
const searchKeyword = ref('')
const searchResults = ref([])
const departmentFormRef = ref(null)

// 表单数据
const departmentForm = ref({
  name: '',
  description: ''
})

const editingDepartment = ref(null)

// 表格分页
const pagination = {
  pageSize: 10
}

// 表格列定义
const memberColumns = [
  { title: '姓名', key: 'userName' },
  { title: '学号', key: 'userId' },
  { title: '学院', key: 'academy' },
  { title: '政治面貌', key: 'politicalLandscape' },
  { title: '性别', key: 'gender' },
  { title: '专业班级', key: 'className' },
  { title: '手机号码', key: 'phoneNum' },
  { title: '身份', key: 'identity' },
  {
    title: '操作',
    key: 'actions',
    render: (row) => {
      return h(NButton, { type: 'error', onClick: () => deleteMember(row) }, { default: () => '删除' })
    }
  }
]

const staffColumns = [
  { title: '姓名', key: 'name' },
  { title: '学号', key: 'userId' },
  {
    title: '操作',
    key: 'actions',
    render: (row) => {
      return h(NButton, { type: 'error', onClick: () => deleteStaff(row) }, { default: () => '删除' })
    }
  }
]

const searchColumns = [
  { title: '姓名', key: 'userName' },
  { title: '学号', key: 'userId' },
  { title: '学院', key: 'academy' },
  {
    title: '操作',
    key: 'actions',
    render: (row) => {
      return h(NButton, { type: 'primary', onClick: () => addMember(row) }, { default: () => '添加' })
    }
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
}

const uploadData = () => {
  console.log('上传Json数据')
}

const openDepartment = (department = null) => {
  if (department) {
    editingDepartment.value = department
    departmentForm.value = {
      name: department.name || '',
      description: department.description || ''
    }
  } else {
    editingDepartment.value = null
    departmentForm.value = { name: '', description: '' }
  }
  showDepartmentModal.value = true
}

const openAddMember = (department = null, type = '') => {
  showAddMemberModal.value = true
}

const deleteAllMinisters = () => {
  console.log('删除所有部长')
}

const deleteAll = (list) => {
  console.log('删除所有成员', list)
}

const deleteMember = (member, list) => {
  console.log('删除成员', member)
}

const deleteStaff = (staff) => {
  console.log('删除部员', staff)
}

const downloadMemberInfo = () => {
  console.log('下载部员信息')
}

const addProject = () => {
  console.log('添加项目')
}

const editProject = (project) => {
  console.log('编辑项目', project)
}

const deleteProject = (project, list) => {
  console.log('删除项目', project)
}

const deleteDepartment = (department) => {
  console.log('删除部门', department)
}

const searchMembers = async () => {
  if (!searchKeyword.value) {
    searchResults.value = []
    return
  }

  try {
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
      searchResults.value = data.map(item => ({
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

const addMember = (member) => {
  console.log('添加成员', member)
  showAddMemberModal.value = false
}

const saveDepartment = async (e) => {
  e.preventDefault()

  try {
    await departmentFormRef.value?.validate()

    const token = localStorage.getItem('Authorization')
    const url = editingDepartment.value
        ? `https://www.xauat.site/api/Department/UpdateDepartment/${editingDepartment.value.id}`
        : 'https://www.xauat.site/api/Department/AddDepartment'

    const method = editingDepartment.value ? 'PUT' : 'POST'

    const response = await fetch(url, {
      method: method,
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        name: departmentForm.value.name,
        description: departmentForm.value.description
      })
    })

    if (response.ok) {
      message.success(editingDepartment.value ? '部门更新成功' : '部门添加成功')
      showDepartmentModal.value = false
      await fetchData()
    } else {
      message.error(editingDepartment.value ? '部门更新失败' : '部门添加失败')
    }
  } catch (error) {
    console.error('保存部门时发生错误:', error)
    message.error('保存部门时发生错误')
  }
}

// 获取数据
const fetchData = async () => {
  try {
    const token = localStorage.getItem('Authorization')

    // 获取所有部门信息
    const departmentsResponse = await fetch('https://www.xauat.site/api/Department/GetAll', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (departmentsResponse.ok) {
      const departmentsData = await departmentsResponse.json()
      departments.value = departmentsData.map(dept => ({
        id: dept.id,
        name: dept.name,
        description: dept.description,
        ministers: dept.ministers || [],
        members: dept.members || [],
        projects: dept.projects || []
      }))
    } else {
      message.error('获取部门数据失败')
    }

    // 获取成员信息
    const membersResponse = await fetch('https://www.xauat.site/api/Member/GetInfo', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (membersResponse.ok) {
      const membersData = await membersResponse.json()
      members.value = membersData.Staffs || []

      // 获取部长信息
      ministers.value = membersData.Ministers || []
    } else {
      message.error('获取成员数据失败')
    }

    // 获取项目信息
    const projectsResponse = await fetch('https://www.xauat.site/api/Project/GetAll', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (projectsResponse.ok) {
      const projectsData = await projectsResponse.json()
      projects.value = projectsData
    } else {
      message.error('获取项目数据失败')
    }

  } catch (error) {
    console.error('获取部门数据失败:', error)
    message.error('获取数据时发生错误')
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.n-card {
  border-radius: 12px;
}

.n-tab-pane {
  padding: 16px 0;
}
</style>

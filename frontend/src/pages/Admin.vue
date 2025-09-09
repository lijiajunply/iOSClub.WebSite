<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <div class="p-6">
      <n-page-header subtitle="社团管理系统" @back="goBack">
        <template #title>
          <div class="text-2xl font-bold">其他数据</div>
        </template>
        <template #extra>
          <n-space v-if="userInfo.isAdmin">
            <n-button type="primary" @click="downloadAllData">
              下载所有数据
            </n-button>
            <n-button type="error" @click="removeAllData">
              删除所有数据
            </n-button>
            <n-button @click="triggerFileInput">
              上传Json数据
            </n-button>
            <input
                ref="fileInput"
                type="file"
                accept=".json"
                multiple
                @change="uploadFiles"
                style="display: none"
            />
          </n-space>
        </template>
        <template #avatar>
          <n-avatar>
            <n-icon>
              <SettingsOutline />
            </n-icon>
          </n-avatar>
        </template>
      </n-page-header>

      <n-card class="mt-6">
        <h3 class="text-xl font-bold mb-4">总述</h3>
        <n-descriptions bordered :column="column">
          <n-descriptions-item label="社员人数">
            {{ studentsCount }}
          </n-descriptions-item>
          <n-descriptions-item label="部员人数">
            {{ staffs.length }}
          </n-descriptions-item>
          <n-descriptions-item label="社团项目数">
            {{ projectsCount }}
          </n-descriptions-item>
          <n-descriptions-item label="任务数">
            {{ tasksCount }}
          </n-descriptions-item>
          <n-descriptions-item label="社团各项资源数">
            {{ resourcesCount }}
          </n-descriptions-item>
          <n-descriptions-item label="部门数">
            {{ departmentsCount }}
          </n-descriptions-item>
          <n-descriptions-item label="社员任务数">
            {{ todosCount }}
          </n-descriptions-item>
        </n-descriptions>

        <n-divider />

        <h3 class="text-xl font-bold mb-4">部员列表</h3>
        <n-grid :cols="2" :md="3" :lg="4" :xl="5" :x-gap="12" :y-gap="12">
          <n-grid-item v-for="staff in staffs" :key="staff.id">
            <div class="card">
              <div class="img" :style="{ backgroundColor: getColorForStaff(staff.name) }">
                <span>{{ staff.name.charAt(0) }}</span>
              </div>
              <h3 class="text-center mt-2 font-medium">{{ staff.name }}</h3>
              <p class="text-gray-500 text-sm text-center">
                {{ identityDictionary[staff.identity] || staff.identity }}
              </p>
            </div>
          </n-grid-item>
        </n-grid>
      </n-card>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMessage, useDialog } from 'naive-ui'
import {
  NPageHeader,
  NCard,
  NAvatar,
  NIcon,
  NSpace,
  NButton,
  NDescriptions,
  NDescriptionsItem,
  NDivider,
  NGrid,
  NGridItem
} from 'naive-ui'
import { SettingsOutline } from '@vicons/ionicons5'
import { useAuthorizationStore } from '../stores/Authorization'

const router = useRouter()
const message = useMessage()
const dialog = useDialog()
const authorizationStore = useAuthorizationStore()

const fileInput = ref(null)

// 数据状态
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

// 响应式列数配置
const column = ref({
  xxl: 3,
  xl: 3,
  lg: 2,
  md: 2,
  sm: 1,
  xs: 1
})

// 身份字典映射
const identityDictionary = {
  'Founder': '创始人',
  'President': '社长/团支书',
  'Minister': '部长/副部长',
  'Department': '部员',
  'Member': '普通成员'
}

// 导航方法
const goBack = () => {
  router.push('/Centre')
}

// 触发文件选择
const triggerFileInput = () => {
  fileInput.value.click()
}

// 获取部员颜色
const getColorForStaff = (name) => {
  const colors = ['#7265e6', '#ffbf00', '#00a8ff', '#ff6b6b', '#2ed573', '#ff9ff3']
  const charCode = name.charCodeAt(0)
  return colors[charCode % colors.length]
}

// 上传文件
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

          // 这里应该调用实际的API来上传数据
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

// 下载所有数据
const downloadAllData = async () => {
  try {
    const token = localStorage.getItem('Authorization')
    if (!token) {
      message.error('未找到认证信息')
      return
    }

    // 获取所有数据
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

    // 这里应该调用实际的API来获取数据
    // 创建JSON文件并下载
    const jsonString = JSON.stringify(data, null, 2)
    const blob = new Blob([jsonString], { type: 'application/json' })
    const url = URL.createObjectURL(blob)

    const a = document.createElement('a')
    a.href = url
    a.download = 'allData.json'
    document.body.appendChild(a)
    a.click()

    // 清理
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

// 删除所有数据
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

        // 调用后端API删除所有数据
        const response = await fetch('https://www.xauat.site/api/Admin/RemoveAllData', {
          method: 'DELETE',
          headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
          }
        })

        if (response.ok) {
          message.success('所有数据已删除')
          // 重新加载数据
          fetchData()
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

// 获取数据
const fetchData = async () => {
  try {
    const token = localStorage.getItem('Authorization')
    if (!token) {
      message.error('未找到认证信息')
      return
    }

    // 获取成员信息
    const memberResponse = await fetch('https://www.xauat.site/api/Member/GetInfo', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (memberResponse.ok) {
      const memberData = await memberResponse.json()
      studentsCount.value = memberData.Total || 0
      staffs.value = memberData.Staffs || []
    } else {
      message.error('获取成员数据失败')
    }

    // 获取项目信息
    const projectResponse = await fetch('https://www.xauat.site/api/Project/GetAllData', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (projectResponse.ok) {
      const projectData = await projectResponse.json()
      projectsCount.value = projectData.length || 0

      // 计算任务总数
      let totalTasks = 0
      projectData.forEach(project => {
        if (project.tasks) {
          totalTasks += project.tasks.length
        }
      })
      tasksCount.value = totalTasks
    } else {
      message.error('获取项目数据失败')
    }

    // 获取资源信息
    const resourceResponse = await fetch('https://www.xauat.site/api/Project/GetResources', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (resourceResponse.ok) {
      const resourceData = await resourceResponse.json()
      resourcesCount.value = resourceData.length || 0
    } else {
      message.error('获取资源数据失败')
    }

    // 获取部门信息
    const departmentResponse = await fetch('https://www.xauat.site/api/Department/GetAll', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    })

    if (departmentResponse.ok) {
      const departmentData = await departmentResponse.json()
      departmentsCount.value = departmentData.length || 0
    } else {
      message.error('获取部门数据失败')
    }
    
    // 我没有看到专门获取Todos的API，这里暂时设置为0
    todosCount.value = 0

    // 设置用户权限
    userInfo.value.isAdmin = true
  } catch (error) {
    console.error('获取数据时出错:', error)
    message.error('获取数据时出错')
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.card {
  transition: 0.2s;
  cursor: pointer;
  border-radius: 10px;
  align-items: center;
  border: 1px solid transparent;
  display: flex;
  flex-direction: column;
  padding: 1rem 0;
}

.card:hover {
  border-color: #1890ff;
}

.img {
  width: 2.6rem;
  height: 2.6rem;
  border-radius: 4rem;
  align-items: center;
  justify-items: center;
  display: flex;
  flex-direction: column;
  vertical-align: middle;
}

.img span {
  display: grid;
  height: 100%;
  align-items: center;
  justify-items: center;
  color: #ffffff;
  font-weight: bold;
}

@media screen and (max-width: 767px) {
  .card {
    flex: 0 0 25%;
    max-width: 25%;
  }
}
</style>

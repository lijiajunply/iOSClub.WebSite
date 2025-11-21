<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 个人信息卡片 -->
      <div class="mb-8">
        <div
            class="bg-white dark:bg-neutral-800 rounded-2xl border border-gray-200 dark:border-neutral-700 overflow-hidden transition-all duration-300 hover:shadow-lg">
          <div class="p-6 flex flex-col sm:flex-row items-center sm:items-start space-y-6 sm:space-y-0 sm:space-x-8">
            <div class="relative">
              <img
                  :src="getUserAvatar()"
                  :alt="userInfo.name"
                  class="w-24 h-24 rounded-full object-cover border-4 border-gray-100 dark:border-neutral-700"
              />
            </div>
            <div class="text-center sm:text-left flex-1">
              <h2 class="text-2xl font-semibold tracking-tight">{{ userInfo.name }}</h2>
              <div class="mt-1 flex flex-wrap items-center justify-center sm:justify-start gap-3">
                <span class="text-gray-500 dark:text-gray-400 text-sm">ID: {{ userInfo.id }}</span>
                <div class="h-2 w-2 rounded-full bg-gray-400 dark:bg-neutral-600"></div>
                <n-tag :type="getRoleType(userInfo.role)" class="rounded-full px-3 py-1">
                  {{ identityDictionary[userInfo.role] }}
                </n-tag>
              </div>
              <div class="mt-4">
                <n-button
                    type="primary"
                    size="small"
                    class="rounded-full bg-blue-500 hover:bg-blue-600"
                    @click="goToPersonalData"
                >
                  <template #icon>
                    <Icon icon="material-symbols:person-outline" class="w-4 h-4"/>
                  </template>
                  编辑个人信息
                </n-button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- iTool 工具卡片 -->
      <section class="mb-8">
        <div
            class="bg-white dark:bg-neutral-800 rounded-2xl border border-gray-200 dark:border-neutral-700 overflow-hidden transition-all duration-300">
          <div class="px-6 py-4 border-b border-gray-200 dark:border-neutral-700">
            <h2 class="text-lg font-semibold tracking-tight">iTool</h2>
            <p class="text-sm text-gray-500 dark:text-gray-400">iOS Club 出品的小工具</p>
          </div>
          <div class="px-6 py-5">
            <div v-if="loading.tools" class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-5">
              <SkeletonLoader v-for="i in 5" :key="i" type="card"/>
            </div>
            <div v-else-if="tools.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
              <n-empty description="暂无可用工具"/>
            </div>
            <div v-else class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-5">
              <div
                  v-for="(tool, index) in tools"
                  :key="index"
                  class="flex flex-col items-center text-center p-4 rounded-xl hover:bg-gray-50 dark:hover:bg-neutral-700 cursor-pointer transition-all duration-200 group"
                  @click="openTool(tool.url)"
              >
                <div
                    class="w-14 h-14 mb-3 flex items-center justify-center bg-gray-100 dark:bg-neutral-700 rounded-xl group-hover:bg-blue-50 dark:group-hover:bg-blue-900/30 transition-colors duration-200">
                  <template v-if="tool.icon && !tool.icon.startsWith('http')">
                    <IconFont :type="tool.icon" className="w-6 h-6"/>
                  </template>
                  <template v-else>
                    <img
                        :src="fixImageUrl(tool)"
                        class="w-6 h-6 object-contain rounded"
                        :alt="`${tool.name}的图标`"
                    />
                  </template>
                </div>
                <h3 class="text-sm font-medium line-clamp-1 w-full group-hover:text-blue-500 transition-colors">
                  {{ tool.name }}</h3>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- 我的任务和社团资源 -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
        <!-- 我的任务 -->
        <section class="max-h-100 h-full">
          <div
              class="bg-white dark:bg-neutral-800 rounded-2xl border border-gray-200 dark:border-neutral-700 overflow-hidden transition-all duration-300 h-full">
            <div class="px-6 py-4 border-b border-gray-200 dark:border-neutral-700">
              <h2 class="text-lg font-semibold tracking-tight">我的任务</h2>
              <p class="text-sm text-gray-500 dark:text-gray-400">待完成的工作任务</p>
            </div>
            <div class="p-6">
              <div v-if="loading.tasks" class="space-y-4">
                <SkeletonLoader v-for="i in 3" :key="i" type="list" :count="1"/>
              </div>
              <div v-else-if="tasks.length === 0" class="flex flex-col items-center justify-center py-10 text-center">
                <n-empty description="暂无待完成任务"/>
              </div>
              <div v-else class="space-y-4 max-h-64 overflow-y-auto pr-2">
                <div
                    v-for="task in tasks"
                    :key="task.id"
                    class="p-4 rounded-xl bg-gray-50 dark:bg-neutral-700 hover:bg-gray-100 dark:hover:bg-neutral-600 transition-colors duration-200"
                >
                  <div class="flex justify-between items-start">
                    <h3 class="font-medium text-sm">{{ task.title }}</h3>
                    <n-tag
                        :type="getTaskStatusType(task.status)"
                        size="small"
                        class="rounded-full"
                    >
                      {{ getTaskStatusText(task.status) }}
                    </n-tag>
                  </div>
                  <p v-if="task.description" class="text-xs text-gray-500 dark:text-gray-400 mt-1 line-clamp-2">
                    {{ task.description }}</p>
                  <div class="mt-3 text-xs text-gray-400">
                    {{ formatDateRange(task.startTime, task.endTime) }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- 社团资源 -->
        <section class="max-h-100 h-full" v-if="userInfo.isAdmin">
          <div
              class="bg-white dark:bg-neutral-800 rounded-2xl border border-gray-200 dark:border-neutral-700 overflow-hidden transition-all duration-300 cursor-pointer hover:shadow-lg h-full"
              @click="goToResources"
          >
            <div class="px-6 py-4 border-b border-gray-200 dark:border-neutral-700">
              <h2 class="text-lg font-semibold tracking-tight">社团资源</h2>
              <p class="text-sm text-gray-500 dark:text-gray-400">iOS Club 资源全览</p>
            </div>
            <div class="p-6">
              <div v-if="loading.resources" class="space-y-4">
                <SkeletonLoader v-for="i in 3" :key="i" type="list" :count="1"/>
              </div>
              <div v-else-if="resources.length === 0"
                   class="flex flex-col items-center justify-center py-10 text-center">
                <n-empty description="暂无可用资源"/>
              </div>
              <div v-else class="space-y-4 max-h-64 overflow-y-auto pr-2">
                <div
                    v-for="resource in resources"
                    :key="resource.id"
                    class="p-4 rounded-xl bg-gray-50 dark:bg-neutral-700 hover:bg-gray-100 dark:hover:bg-neutral-600 transition-colors duration-200"
                >
                  <h3 class="font-medium text-sm">{{ resource.name }}</h3>
                  <p v-if="resource.description" class="text-xs text-gray-500 dark:text-gray-400 mt-1 line-clamp-2">
                    {{ resource.description }}</p>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>

      <!-- 管理员视图 -->
      <div v-if="userInfo.isAdmin" class="mb-8">
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <!-- 社团部门 -->
          <div class="lg:col-span-1 max-h-100 h-full">
            <div
                class="bg-white dark:bg-neutral-800 rounded-2xl border border-gray-200 dark:border-neutral-700 overflow-hidden transition-all duration-300 cursor-pointer hover:shadow-lg h-full"
                @click="goToDepartment"
            >
              <div class="px-6 py-4 border-b border-gray-200 dark:border-neutral-700">
                <h2 class="text-lg font-semibold tracking-tight">社团部门</h2>
                <p class="text-sm text-gray-500 dark:text-gray-400">iOS 部门管理</p>
              </div>
              <div class="p-6">
                <div v-if="loading.departments" class="space-y-3">
                  <SkeletonLoader v-for="i in 4" :key="i" type="list" :count="1"/>
                </div>
                <div v-else-if="departments.length === 0"
                     class="flex flex-col items-center justify-center py-10 text-center">
                  <n-empty description="暂无部门信息"/>
                </div>
                <div v-else class="space-y-3 max-h-64 overflow-y-auto pr-2">
                  <div
                      v-for="department in departments"
                      :key="department.name"
                      class="p-3 rounded-lg bg-gray-50 dark:bg-neutral-700 hover:bg-gray-100 dark:hover:bg-neutral-600 transition-colors duration-200"
                  >
                    <h3 class="font-medium text-sm">{{ department.name }}</h3>
                    <p v-if="department.description" class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      {{ department.description }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 数据中心 -->
          <div class="lg:col-span-2 max-h-100 h-full">
            <div
                class="bg-white dark:bg-neutral-800 rounded-2xl border border-gray-200 dark:border-neutral-700 overflow-hidden transition-all duration-300 hover:shadow-lg h-full">
              <div class="px-6 py-4 border-b border-gray-200 dark:border-neutral-700">
                <h2 class="text-lg font-semibold tracking-tight">数据中心</h2>
                <p class="text-sm text-gray-500 dark:text-gray-400">展示社团数据</p>
              </div>
              <div class="p-6">
                <div v-if="loading.statistics" class="grid grid-cols-2 sm:grid-cols-3 gap-4">
                  <SkeletonLoader v-for="i in 6" :key="i" type="card"/>
                </div>
                <div v-else class="grid grid-cols-2 sm:grid-cols-3 gap-4">
                  <router-link to="/Centre/MemberData" class="block h-full">
                    <div
                        class="p-4 rounded-xl bg-blue-50 dark:bg-blue-900/20 border border-blue-100 dark:border-blue-800/30">
                      <div class="flex justify-between items-start">
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-400">当前成员</p>
                          <p class="text-2xl font-semibold mt-1 text-blue-600 dark:text-blue-400">{{
                              statistics.members
                            }}</p>
                        </div>
                        <div
                            class="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-800/50 flex items-center justify-center">
                          <Icon icon="material-symbols:groups-outline"
                                class="w-4 h-4 text-blue-600 dark:text-blue-400"/>
                        </div>
                      </div>
                    </div>
                  </router-link>
                  <router-link to="/Centre/Department" class="block h-full">
                    <div
                        class="p-4 rounded-xl bg-green-50 dark:bg-green-900/20 border border-green-100 dark:border-green-800/30">
                      <div class="flex justify-between items-start">
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-400">部员数量</p>
                          <p class="text-2xl font-semibold mt-1 text-green-600 dark:text-green-400">{{
                              statistics.staffs
                            }}</p>
                        </div>
                        <div
                            class="w-8 h-8 rounded-full bg-green-100 dark:bg-green-800/50 flex items-center justify-center">
                          <Icon icon="material-symbols:person-outline"
                                class="w-4 h-4 text-green-600 dark:text-green-400"/>
                        </div>

                      </div>
                    </div>
                  </router-link>
                  <router-link to="/Centre/Department" class="block h-full">
                    <div
                        class="p-4 rounded-xl bg-purple-50 dark:bg-purple-900/20 border border-purple-100 dark:border-purple-800/30">
                      <div class="flex justify-between items-start">
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-400">项目数量</p>
                          <p class="text-2xl font-semibold mt-1 text-purple-600 dark:text-purple-400">
                            {{ statistics.projects }}</p>
                        </div>
                        <div
                            class="w-8 h-8 rounded-full bg-purple-100 dark:bg-purple-800/50 flex items-center justify-center">
                          <Icon icon="material-symbols:work-outline"
                                class="w-4 h-4 text-purple-600 dark:text-purple-400"/>
                        </div>

                      </div>
                    </div>
                  </router-link>
                  <router-link to="/Centre/Department" class="block h-full">
                    <div
                        class="p-4 rounded-xl bg-amber-50 dark:bg-amber-900/20 border border-amber-100 dark:border-amber-800/30">
                      <div class="flex justify-between items-start">
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-400">任务数量</p>
                          <p class="text-2xl font-semibold mt-1 text-amber-600 dark:text-amber-400">{{
                              statistics.tasks
                            }}</p>
                        </div>
                        <div
                            class="w-8 h-8 rounded-full bg-amber-100 dark:bg-amber-800/50 flex items-center justify-center">
                          <Icon icon="material-symbols:check-circle-outline"
                                class="w-4 h-4 text-amber-600 dark:text-amber-400"/>
                        </div>

                      </div>
                    </div>
                  </router-link>
                  <router-link to="/Centre/Resources" class="block h-full">
                    <div
                        class="p-4 rounded-xl bg-red-50 dark:bg-red-900/20 border border-red-100 dark:border-red-800/30">
                      <div class="flex justify-between items-start">
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-400">资源数量</p>
                          <p class="text-2xl font-semibold mt-1 text-red-600 dark:text-red-400">{{
                              statistics.resources
                            }}</p>
                        </div>
                        <div
                            class="w-8 h-8 rounded-full bg-red-100 dark:bg-red-800/50 flex items-center justify-center">
                          <Icon icon="material-symbols:database-outline"
                                class="w-4 h-4 text-red-600 dark:text-red-400"/>
                        </div>

                      </div>
                    </div>
                  </router-link>
                  <router-link to="/Centre/Department" class="block h-full">
                    <div
                        class="p-4 rounded-xl bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-800/30">
                      <div class="flex justify-between items-start">
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-400">部门数量</p>
                          <p class="text-2xl font-semibold mt-1 text-indigo-600 dark:text-indigo-400">
                            {{ statistics.departments }}</p>
                        </div>
                        <div
                            class="w-8 h-8 rounded-full bg-indigo-100 dark:bg-indigo-800/50 flex items-center justify-center">
                          <Icon icon="material-symbols:apartment-outline"
                                class="w-4 h-4 text-indigo-600 dark:text-indigo-400"/>
                        </div>
                      </div>
                    </div>
                  </router-link>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import {ref, onMounted} from 'vue'
import {useRouter} from 'vue-router'
import {NTag, NButton, NEmpty} from 'naive-ui'
import {Icon} from '@iconify/vue'
import SkeletonLoader from '../components/SkeletonLoader.vue'
import {ToolService} from '../services/ToolService.ts'
import {UserService} from '../services/UserService.ts'
import {ProjectService} from '../services/ProjectService.ts'
import {ResourceService} from '../services/ResourceService.ts'
import {DepartmentService} from '../services/DepartmentService.ts'
import {DataCentreService} from "../services/DataCentreService.ts";
import IconFont from "../components/IconFont.vue";

const router = useRouter()

// 用户信息
const userInfo = ref({
  name: '',
  id: '',
  role: '',
  isAdmin: false,
  gender: '男'
})

// 工具数据
const tools = ref([])

// 任务数据
const tasks = ref([])

// 资源数据
const resources = ref([])

// 部门数据
const departments = ref([])

// 统计数据
const statistics = ref({
  members: 0,
  staffs: 0,
  projects: 0,
  tasks: 0,
  resources: 0,
  departments: 0
})

const identityDictionary = {
  'Founder': '创始人',
  'President': '社长/团支书',
  'Minister': '部长/副部长',
  'Department': '部员',
  'Member': '普通成员'
}

// 加载状态
const loading = ref({
  tools: true,
  tasks: true,
  resources: true,
  departments: true,
  statistics: true
})

// 获取用户头像
const getUserAvatar = () => {
  try {
    if (userInfo.value.gender === '男') {
      return new URL('/assets/Centre/男生.png', import.meta.url).href
    } else {
      return new URL('/assets/Centre/女生.png', import.meta.url).href
    }
  } catch (error) {
    console.error('获取头像失败:', error)
    // 返回默认头像
    return ''
  }
}

// 获取角色类型（用于标签显示）
const getRoleType = (role) => {
  const roleMap = {
    '创始人': 'success',
    '社长/团支书': 'primary',
    '部长/副部长': 'info',
    '部员': 'default',
    '普通成员': 'default'
  }
  return roleMap[role] || 'default'
}

// 获取任务状态类型
const getTaskStatusType = (status) => {
  return status === true ? 'success' : 'info'
}

// 获取任务状态文本
const getTaskStatusText = (status) => {
  return status === true ? '已完成' : '进行中'
}

// 格式化日期范围
const formatDateRange = (start, end) => {
  if (!start && !end) return '无时间信息'
  if (!start) return `截止: ${end}`
  if (!end) return `开始: ${start}`
  return `${start} - ${end}`
}

// 修复图片URL中的重复斜杠问题
const fixImageUrl = (tool) => {
  if (tool.icon) {
    return tool.icon.replace(/([^:])(\/\/)/g, '$1/')
  }

  const domain = tool.url.replace("https://", "").replace("http://", "").split('/')[0];
  return `https://${domain}/favicon.ico`;
}

// 导航到个人数据页面
const goToPersonalData = () => {
  router.push('/Centre/PersonalData')
}

// 导航到资源页面
const goToResources = () => {
  router.push('/Centre/Resources')
}

// 导航到部门页面
const goToDepartment = () => {
  router.push('/Centre/Department')
}

// 打开工具链接
const openTool = (url) => {
  window.open(url, '_blank')
}

// 获取工具数据
const fetchTools = async () => {
  try {
    loading.value.tools = true
    const res = await ToolService.getTools()
    tools.value = res.links || []
  } catch (error) {
    console.error('获取工具数据失败:', error)
    // 使用mock数据
    tools.value = [
      {
        id: 1,
        name: 'iLibrary',
        icon: 'book',
        url: 'https://ilibrary.xauat.site',
        description: 'iOS Club 图书馆管理系统'
      },
      {
        id: 2,
        name: 'iOSAI',
        icon: 'robot',
        url: 'https://iosai.xauat.site',
        description: 'iOS Club 人工智能平台'
      },
      {
        id: 3,
        name: 'AIAPI服务',
        icon: 'api',
        url: 'https://aiapi.xauat.site',
        description: '人工智能API服务平台'
      },
      {
        id: 4,
        name: '建大导航',
        icon: 'compass',
        url: 'https://navigation.xauat.site',
        description: '西安建筑科技大学校园导航系统'
      },
      {
        id: 5,
        name: '社团官网',
        icon: 'website',
        url: 'https://www.xauat.site',
        description: 'iOS Club 官方网站'
      }
    ]
  } finally {
    loading.value.tools = false
  }
}

// 获取用户信息
const fetchUserInfo = async () => {
  try {
    const userData = await UserService.getUserData()
    userInfo.value = {
      name: userData.userName,
      id: userData.userId,
      role: userData.identity || '普通成员',
      isAdmin: userData.identity === 'Founder' || userData.identity === 'President' || userData.identity === 'Minister',
      gender: userData.gender || '男'
    }
  } catch (error) {
    console.error('获取用户信息时发生错误:', error)
    // 使用mock数据
    userInfo.value = {
      name: '测试用户',
      id: '1001',
      role: '部员',
      isAdmin: true,
      gender: '男'
    }
  }
}

// 获取任务数据
const fetchTasks = async () => {
  try {
    loading.value.tasks = true
    const todoData = await ProjectService.getYourTasks()
    tasks.value = todoData.map(task => ({
      id: task.id,
      title: task.name,
      description: '',
      startTime: task.startTime ? new Date(task.startTime).toLocaleDateString() : '',
      endTime: task.endTime ? new Date(task.endTime).toLocaleDateString() : '',
      status: task.status === 'Done' || false
    }))
  } catch (error) {
    console.error('获取任务数据时发生错误:', error)
    // 使用mock数据
    tasks.value = [
      {
        id: 1,
        title: '完成项目文档编写',
        description: '编写iOS Club官网项目的技术文档和用户手册',
        startTime: '2023-10-01',
        endTime: '2023-10-15',
        status: false
      },
      {
        id: 2,
        title: '准备周会演示',
        description: '准备下周例会的项目进度演示材料',
        startTime: '2023-10-05',
        endTime: '2023-10-08',
        status: false
      },
      {
        id: 3,
        title: '修复登录页面bug',
        description: '修复用户反馈的登录页面在iOS设备上的显示问题',
        startTime: '2023-09-28',
        endTime: '2023-10-02',
        status: true
      }
    ]
  } finally {
    loading.value.tasks = false
  }
}

// 获取资源数据
const fetchResources = async () => {
  try {
    loading.value.resources = true
    const resourceData = await ResourceService.getAllResources()
    resources.value = resourceData.map(resource => ({
      id: resource.id,
      name: resource.name,
      description: resource.description,
    }))
    statistics.value.resources = resources.value.length
  } catch (error) {
    console.error('获取资源数据时发生错误:', error)
    // 使用mock数据
    resources.value = [
      {
        id: 1,
        name: 'iOS开发手册',
        description: 'iOS Club内部iOS开发学习资料汇总',
        tag: 'iOS,开发,手册'
      },
      {
        id: 2,
        name: 'Swift语言教程',
        description: 'Swift编程语言从入门到精通',
        tag: 'Swift,教程,语言'
      },
      {
        id: 3,
        name: 'Git版本控制',
        description: 'Git版本控制系统使用指南',
        tag: 'Git,版本控制,工具'
      }
    ]
    statistics.value.resources = resources.value.length
  } finally {
    loading.value.resources = false
  }
}

// 获取部门数据
const fetchDepartments = async () => {
  try {
    loading.value.departments = true
    const departmentData = await DepartmentService.getAllDepartments()
    departments.value = departmentData.map(dept => ({
      name: dept.name,
      description: dept.description
    }))
    statistics.value.departments = departments.value.length
  } catch (error) {
    console.error('获取部门数据时发生错误:', error)
    // 使用mock数据
    departments.value = [
      {
        name: '软件部',
        description: '感受软件开发的魅力'
      },
      {
        name: '硬件部',
        description: '感受硬件开发的魅力'
      },
      {
        name: '交流实践部',
        description: '组织各种活动并参与其中'
      },
      {
        name: '新媒体部',
        description: '分享社团的点滴'
      }
    ]
    statistics.value.departments = departments.value.length
  } finally {
    loading.value.departments = false
  }
}

// 获取统计数据
const fetchStatistics = async () => {
  try {
    loading.value.statistics = true
    // 获取统计数据需要从多个服务中组合

    statistics.value = await DataCentreService.getCentreData();
  } catch (error) {
    console.error('获取统计数据时发生错误:', error)
    // 使用mock数据
    statistics.value = {
      members: 128,
      staffs: 42,
      projects: 15,
      tasks: 36,
      resources: 24,
      departments: 4
    }
  } finally {
    loading.value.statistics = false
  }
}

// 分割标签字符串为数组
const splitTags = (tagString) => {
  if (!tagString) return []
  return tagString.split(',').map(tag => tag.trim()).filter(tag => tag.length > 0)
}

// 页面加载时获取数据
onMounted(async () => {
  // 并发获取所有数据
  await Promise.all([
    fetchTools(),
    fetchUserInfo(),
    fetchTasks(),
    fetchResources(),
    fetchDepartments(),
    fetchStatistics()
  ])
})
</script>

<style scoped>
/* 自定义滚动条样式 */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background-color: rgba(156, 163, 175, 0.5);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background-color: rgba(156, 163, 175, 0.7);
}
</style>
<template>
  <div class="min-h-screen bg-gray-50">
    <div class="container mx-auto px-4 max-w-7xl">
      <!-- 头部区域 -->
      <div class="text-center mb-12 pt-8">
        <h1 class="text-4xl md:text-5xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
          您的iMember中心
        </h1>
        <p class="text-lg text-gray-600 mt-2">
          西安建筑科技大学iOS众创空间俱乐部
        </p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        <!-- 个人信息卡片 -->
        <div class="lg:col-span-1">
          <n-card hoverable class="rounded-2xl h-full">
            <div class="flex flex-col items-center text-center">
              <img
                  :src="getUserAvatar()"
                  :alt="userInfo.name"
                  class="w-24 h-24 rounded-full mb-4"
                  @error="handleImageError"
              />
              <h2 class="text-2xl font-bold">{{ userInfo.name }}</h2>
              <p class="text-gray-600">ID: {{ userInfo.id }}</p>
              <p class="text-gray-800 font-medium">{{ userInfo.role }}</p>
              <n-button
                  type="primary"
                  class="mt-4"
                  @click="goToPersonalData"
              >
                编辑信息
              </n-button>
            </div>
          </n-card>
        </div>

        <!-- iTool 工具卡片 -->
        <div class="lg:col-span-2">
          <n-card hoverable class="rounded-2xl">
            <template #header>
              <div>
                <div class="font-bold text-2xl">iTool</div>
                <div class="text-gray-500 text-sm mt-1">iOS Club出品的小工具</div>
              </div>
            </template>

            <div v-if="tools.length === 0" class="text-center py-4 text-gray-500">
              社团还没加入iOS App
            </div>
            <div v-else class="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-4">
              <div
                  v-for="(tool, index) in tools"
                  :key="index"
                  class="flex flex-col items-center cursor-pointer hover:bg-gray-50 p-2 rounded-lg transition-colors"
                  @click="openTool(tool.url)"
              >
                <div class="w-10 h-10 mb-2 flex items-center justify-center">
                  <template v-if="!tool.icon.startsWith('http')">
                    <IconFont
                        :type="`#icon-${tool.icon}`"
                        class="text-[28px]"
                    />
                  </template>
                  <template v-else>
                    <img
                        :src="fixImageUrl(tool.icon)"
                        :style="{ height: '28px', width: '28px', borderRadius: '6px' }"
                        :alt="`${tool.name}的图标`"
                        @error="(e) => handleImageError(e, tool)"
                        data-debug="image-icon"
                    />
                  </template>
                </div>
                <span class="text-sm text-center truncate w-full">{{ tool.name }}</span>
              </div>
            </div>
          </n-card>
        </div>
      </div>

      <!-- 我的任务和社团资源 -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <!-- 我的任务 -->
        <n-card hoverable class="rounded-2xl">
          <template #header>
            <div>
              <div class="font-bold text-2xl">我的任务</div>
              <div class="text-gray-500 text-sm mt-1">iStaff 的任务</div>
            </div>
          </template>

          <div v-if="tasks.length === 0" class="text-center py-4 text-gray-500">
            您的任务都已经完成了
          </div>
          <div v-else class="max-h-96 overflow-y-auto">
            <n-list>
              <n-list-item v-for="task in tasks" :key="task.id">
                <div class="flex justify-between items-center">
                  <div>
                    <div class="font-medium">{{ task.title }}</div>
                    <div class="text-sm text-gray-500">{{ formatDateRange(task.startTime, task.endTime) }}</div>
                  </div>
                  <n-tag :type="getTaskStatusType(task.status)" size="small">
                    {{ getTaskStatusText(task.status) }}
                  </n-tag>
                </div>
              </n-list-item>
            </n-list>
          </div>
        </n-card>

        <!-- 社团资源 -->
        <n-card hoverable class="rounded-2xl">
          <template #header>
            <div>
              <div class="font-bold text-2xl">社团资源</div>
              <div class="text-gray-500 text-sm mt-1">iOS Club资源全览</div>
            </div>
          </template>

          <div v-if="resources.length === 0" class="text-center py-4 text-gray-500">
            社团现在还没有资源
          </div>
          <div v-else class="max-h-96 overflow-y-auto">
            <n-list>
              <n-list-item v-for="resource in resources" :key="resource.id" @click="goToResources" class="cursor-pointer hover:bg-gray-50">
                <div>
                  <div class="font-medium">{{ resource.name }}</div>
                  <div class="text-sm text-gray-500">{{ resource.description }}</div>
                  <div class="mt-1">
                    <n-tag
                        v-for="tag in splitTags(resource.tag)"
                        :key="tag"
                        type="info"
                        size="small"
                        class="mr-1"
                    >
                      {{ tag }}
                    </n-tag>
                  </div>
                </div>
              </n-list-item>
            </n-list>
          </div>
        </n-card>
      </div>

      <!-- 管理员视图 -->
      <div v-if="userInfo.isAdmin" class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <!-- 社团部门 -->
        <n-card hoverable class="rounded-2xl" @click="goToDepartment">
          <template #header>
            <div>
              <div class="font-bold text-2xl">社团部门</div>
              <div class="text-gray-500 text-sm mt-1">iOS 部门管理</div>
            </div>
          </template>

          <div v-if="departments.length === 0" class="text-center py-4 text-gray-500">
            社团现在还没有部门
          </div>
          <div v-else class="max-h-96 overflow-y-auto">
            <n-list>
              <n-list-item v-for="department in departments" :key="department.name">
                <div>
                  <div class="font-medium">{{ department.name }}</div>
                  <div class="text-sm text-gray-500">{{ department.description }}</div>
                </div>
              </n-list-item>
            </n-list>
          </div>
        </n-card>

        <!-- 数据中心 -->
        <n-card hoverable class="rounded-2xl" @click="goToMemberData">
          <template #header>
            <div>
              <div class="font-bold text-2xl">数据中心</div>
              <div class="text-gray-500 text-sm mt-1">展示社团数据</div>
            </div>
          </template>

          <div class="grid grid-cols-2 gap-4">
            <div class="text-center p-4 bg-blue-50 rounded-lg">
              <div class="text-2xl font-bold">{{ statistics.members }}</div>
              <div class="text-gray-600">当前成员</div>
            </div>
            <div class="text-center p-4 bg-green-50 rounded-lg">
              <div class="text-2xl font-bold">{{ statistics.staffs }}</div>
              <div class="text-gray-600">部员数量</div>
            </div>
            <div class="text-center p-4 bg-yellow-50 rounded-lg">
              <div class="text-2xl font-bold">{{ statistics.projects }}</div>
              <div class="text-gray-600">项目数量</div>
            </div>
            <div class="text-center p-4 bg-purple-50 rounded-lg">
              <div class="text-2xl font-bold">{{ statistics.tasks }}</div>
              <div class="text-gray-600">任务数量</div>
            </div>
            <div class="text-center p-4 bg-red-50 rounded-lg">
              <div class="text-2xl font-bold">{{ statistics.resources }}</div>
              <div class="text-gray-600">资源数量</div>
            </div>
            <div class="text-center p-4 bg-indigo-50 rounded-lg">
              <div class="text-2xl font-bold">{{ statistics.departments }}</div>
              <div class="text-gray-600">部门数量</div>
            </div>
          </div>
        </n-card>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { NCard, NList, NListItem, NTag, NButton } from 'naive-ui'
import { useAuthorizationStore } from '../stores/Authorization'
import '../lib/iconfont' // 导入图标库
import IconFont from '../components/IconFont.vue'
import { ToolService } from '../services/ToolService.js'

const router = useRouter()
const authorizationStore = useAuthorizationStore()

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

// 获取用户头像
const getUserAvatar = () => {
  if (userInfo.value.gender === '男') {
    return new URL('../assets/Centre/男生.png', import.meta.url).href
  } else {
    return new URL('../assets/Centre/女生.png', import.meta.url).href
  }
}

// 获取任务状态类型
const getTaskStatusType = (status) => {
  if (status === true) return 'success'
  return 'info'
}

// 获取任务状态文本
const getTaskStatusText = (status) => {
  if (status === true) return '已完成'
  return '进行中'
}

// 格式化日期范围
const formatDateRange = (start, end) => {
  if (!start && !end) return '无时间信息'
  if (!start) return `截止: ${end}`
  if (!end) return `开始: ${start}`
  return `${start} - ${end}`
}

// 修复图片URL中的重复斜杠问题
const fixImageUrl = (url) => {
  return url.replace(/([^:]\/)\/+/g, '$1');
}

// 图标加载失败时替换为备用图标
const handleImageError = (event, tool) => {
  // 使用默认的社团Logo作为备用图标
  const toolsImage = new URL('../assets/Centre/AppleLogo.jpg', import.meta.url).href;
  if (event.target.src === toolsImage) return;

  console.debug(`[${tool.name}]图标加载失败，使用备用图标`, {
    failedUrl: event.target.src,
    fallback: toolsImage
  });
  event.target.src = toolsImage;
}

// 导航到个人数据页面
const goToPersonalData = () => {
  router.push('/admin/personal-data')
}

// 导航到资源页面
const goToResources = () => {
  router.push('/admin/resources')
}

// 导航到部门页面
const goToDepartment = () => {
  router.push('/admin/department')
}

// 导航到成员数据页面
const goToMemberData = () => {
  router.push('/admin/member-data')
}

// 打开工具链接
const openTool = (url) => {
  window.open(url, '_blank')
}

// 退出登录
const logout = () => {
  authorizationStore.logout()
  router.push('/login')
}

// 获取工具数据
const fetchTools = async () => {
  try {
    const res = await ToolService.getTools();
    tools.value = res.links;
  } catch (error) {
    console.error('获取工具数据失败:', error);
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
    ];
  }
}

// 获取用户信息
const fetchUserInfo = async () => {
  try {
    const token = localStorage.getItem('Authorization');
    console.log('获取用户信息，token:', token);

    if (!token) {
      console.error('未找到用户token');
      router.push('/login');
      return;
    }

    const response = await fetch('https://www.xauat.site/api/Member/GetData', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    });

    console.log('用户信息响应状态:', response.status);

    if (response.ok) {
      const userData = await response.json();
      console.log('获取到的用户数据:', userData);

      userInfo.value.name = userData.userName;
      userInfo.value.id = userData.userId;
      userInfo.value.gender = userData.gender === '男' ? '男' : '女';

      switch (userData.identity) {
        case 'Founder':
          userInfo.value.role = '创始人';
          break;
        case 'President':
          userInfo.value.role = '社长/团支书';
          break;
        case 'Minister':
          userInfo.value.role = '部长/副部长';
          break;
        case 'Department':
          userInfo.value.role = '部员';
          break;
        case 'Member':
          userInfo.value.role = '普通成员';
          break;
        default:
          userInfo.value.role = userData.identity;
      }

      userInfo.value.isAdmin = ['Founder', 'President', 'Minister'].includes(userData.identity);

      console.log('更新后的用户信息:', userInfo.value);
    } else if (response.status === 401) {
      console.error('认证已过期，请重新登录');
      authorizationStore.logout();
      router.push('/login');
    } else {
      console.error('获取用户信息失败:', response.status);
      const errorText = await response.text();
      console.error('错误详情:', errorText);
    }
  } catch (error) {
    console.error('获取用户信息时发生错误:', error);
  }
}

// 获取任务数据
const fetchTasks = async () => {
  try {
    const token = localStorage.getItem('Authorization');
    if (!token) {
      console.error('未找到用户token');
      return;
    }

    const response = await fetch('https://www.xauat.site/api/Project/GetYourTasks', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const taskData = await response.json();
      tasks.value = taskData.map(task => ({
        id: task.id,
        title: task.title,
        description: task.description,
        startTime: task.startTime,
        endTime: task.endTime,
        status: task.status
      }));
    } else if (response.status === 401) {
      console.error('认证已过期，请重新登录');
      authorizationStore.logout();
      router.push('/login');
    } else {
      console.error('获取任务数据失败:', response.status);
    }
  } catch (error) {
    console.error('获取任务数据时发生错误:', error);
  }
}

// 获取资源数据
const fetchResources = async () => {
  try {
    const token = localStorage.getItem('Authorization');
    if (!token) {
      console.error('未找到用户token');
      return;
    }

    const response = await fetch('https://www.xauat.site/api/Project/GetResources', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const resourceData = await response.json();
      resources.value = resourceData.map(resource => ({
        id: resource.id,
        name: resource.name,
        description: resource.description,
        tag: resource.tag
      }));

      statistics.value.resources = resourceData.length;
    } else if (response.status === 401) {
      console.error('认证已过期，请重新登录');
      authorizationStore.logout();
      router.push('/login');
    } else {
      console.error('获取资源数据失败:', response.status);
    }
  } catch (error) {
    console.error('获取资源数据时发生错误:', error);
  }
}

// 获取部门数据
const fetchDepartments = async () => {
  try {
    const token = localStorage.getItem('Authorization');
    if (!token) {
      console.error('未找到用户token');
      return;
    }

    // 获取所有部门信息（管理员和普通用户都可以获取）
    const allDepartmentsResponse = await fetch('https://www.xauat.site/api/Department/GetAll', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    });

    if (allDepartmentsResponse.ok) {
      const allDepartments = await allDepartmentsResponse.json();
      departments.value = allDepartments;
      statistics.value.departments = allDepartments.length;
    } else if (allDepartmentsResponse.status === 401) {
      console.error('认证已过期，请重新登录');
      authorizationStore.logout();
      router.push('/login');
    } else {
      console.error('获取部门数据失败:', allDepartmentsResponse.status);
      // 提供默认部门数据
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
      ];
      statistics.value.departments = departments.value.length;
    }
  } catch (error) {
    console.error('获取部门数据时发生错误:', error);
    // 提供默认部门数据
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
    ];
    statistics.value.departments = departments.value.length;
  }
}

// 获取统计数据
const fetchStatistics = async () => {
  try {
    const token = localStorage.getItem('Authorization');
    if (!token) {
      console.error('未找到用户token');
      return;
    }

    // 获取成员统计数据
    const membersResponse = await fetch('https://www.xauat.site/api/Member/GetInfo', {
      method: 'GET',
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
      }
    });

    if (membersResponse.ok) {
      const membersData = await membersResponse.json();
      console.log('获取到的成员数据:', membersData);
      
      if (membersData.Total !== undefined) {
        statistics.value.members = membersData.Total;
      }
      if (membersData.StaffsCount !== undefined) {
        statistics.value.staffs = membersData.StaffsCount;
      }
      if (membersData.Projects !== undefined) {
        statistics.value.projects = membersData.Projects.length;
      }
      if (membersData.Tasks !== undefined) {
        statistics.value.tasks = membersData.Tasks.length;
      }
      if (membersData.Resources !== undefined) {
        statistics.value.resources = membersData.Resources.length;
      }
      if (membersData.Departments !== undefined) {
        statistics.value.departments = membersData.Departments.length;
      }
    } else if (membersResponse.status === 401) {
      console.error('认证已过期，请重新登录');
      authorizationStore.logout();
      router.push('/login');
    } else {
      console.error('获取统计数据失败:', membersResponse.status);
    }
  } catch (error) {
    console.error('获取统计数据时发生错误:', error);
  }
}

// 分割标签字符串为数组
const splitTags = (tagString) => {
  if (!tagString) return [];
  return tagString.split(',').map(tag => tag.trim()).filter(tag => tag.length > 0);
}

onMounted(async () => {
  console.log('Centre页面加载');

  // 检查用户是否已认证
  if (!authorizationStore.isAuthenticated) {
    router.push('/login');
    return;
  }

  // 获取工具数据
  await fetchTools();

  // 获取用户信息
  await fetchUserInfo();

  // 获取任务数据
  await fetchTasks();

  // 获取资源数据
  await fetchResources();

  // 获取部门数据
  await fetchDepartments();

  // 获取统计数据
  await fetchStatistics();

  console.log('加载Centre页面数据完成');
})
</script>

<style scoped>
.rounded-2xl {
  border-radius: 1rem;
}

.cursor-pointer {
  cursor: pointer;
}

a {
  text-decoration: none;
}
</style>

<template>
  <div class="relative">
    <!-- 侧边栏蒙层 -->
    <div
        v-if="layoutStore.isMobile && layoutStore.showSidebar"
        class="fixed inset-0 bg-black/20 backdrop-blur-sm z-40"
        @click="layoutStore.toggleSidebar()"
    ></div>

    <!-- 侧边栏主体 -->
    <aside
        class="
        fixed top-0 left-0 h-screen
        bg-white/90 dark:bg-gray-800/90 backdrop-blur-md
        border-r border-gray-100 dark:border-gray-700
        transition-all duration-300 ease-in-out
        z-50 overflow-hidden
        flex flex-col
      "
        :class="{
        'w-64': !layoutStore.isSidebarCollapsed,
        'w-20': layoutStore.isSidebarCollapsed,
        'transform -translate-x-full': layoutStore.isMobile && !layoutStore.showSidebar,
        'shadow-lg': layoutStore.isMobile
      }"
    >
      <!-- 侧边栏头部 -->
      <div class="p-4 dark:border-gray-700 flex items-center" 
           :class="layoutStore.isSidebarCollapsed ? 'justify-center' : 'justify-between'">
        <router-link to="/" class="flex items-center gap-3 group" v-if="!layoutStore.isSidebarCollapsed">
          <img
              src="/assets/iOS_Club_LOGO.png"
              alt="iOS Club Logo"
              class="w-10 h-10 rounded-lg object-contain"
              @error="handleImageError"
          />
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white whitespace-nowrap">iMember</h2>
        </router-link>
        
        <button 
          @click="layoutStore.toggleSidebarCollapse()" 
          class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors hidden md:block"
          :class="layoutStore.isSidebarCollapsed ? 'ml-0' : ''"
        >
          <Icon 
            :icon="layoutStore.isSidebarCollapsed ? 'cuida:sidebar-collapse-outline' : 'cuida:sidebar-expand-outline'"
            class="w-5 h-5 text-gray-500 dark:text-gray-400"
          />
        </button>
      </div>

      <!-- 侧边栏导航 -->
      <nav class="flex-1 overflow-y-auto py-4">
        <ul class="space-y-1 px-3">
          <li v-for="item in filteredMenuItems" :key="item.name">
            <router-link
                :to="item.path"
                class="
                flex items-center px-4 py-2.5 rounded-lg transition-all duration-200
                text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700/50 hover:text-blue-600! dark:hover:text-blue-400!
              "
                :class="{
                'bg-blue-50 dark:bg-blue-900/30 text-blue-600! dark:text-blue-400!': $route.path === item.path,
                'justify-center': layoutStore.isSidebarCollapsed
              }"
                @click="closeSidebar"
            >
              <Icon :icon="item.icon" class="w-5 h-5" :class="{'mx-auto': layoutStore.isSidebarCollapsed, 'mr-3': !layoutStore.isSidebarCollapsed}" />
              <span v-if="!layoutStore.isSidebarCollapsed" class="text-sm font-medium whitespace-nowrap">{{ item.name }}</span>
            </router-link>
          </li>
        </ul>
      </nav>

      <!-- 侧边栏底部 -->
      <div class="p-4 border-t border-gray-100 dark:border-gray-700">
        <NButton
            quaternary
            class="w-full justify-start"
            :class="{'justify-center': layoutStore.isSidebarCollapsed}"
            @click="logout"
        >
          <n-icon class="mr-2" :class="{'mx-auto': layoutStore.isSidebarCollapsed, 'mr-2': !layoutStore.isSidebarCollapsed}">
            <Icon icon="ion:log-out" :size="18"/>
          </n-icon>
          <span v-if="!layoutStore.isSidebarCollapsed" class="font-medium text-sm">退出登录</span>
        </NButton>
      </div>
    </aside>

    <!-- 主内容区域的边距 -->
    <div
        class="transition-all duration-300"
        :style="{ marginLeft: layoutStore.showSidebar && !layoutStore.isMobile ? (layoutStore.isSidebarCollapsed ? '5rem' : '16rem') : '0' }"
    >
      <slot></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import {computed, onMounted, onBeforeUnmount} from 'vue'
import {useRouter} from 'vue-router'
import {useAuthorizationStore} from '../stores/Authorization'
import {useLayoutStore} from '../stores/LayoutStore'
import {NButton, NIcon} from 'naive-ui'
import {Icon} from '@iconify/vue'

const router = useRouter()
const authorizationStore = useAuthorizationStore()
const layoutStore = useLayoutStore()

// 窗口大小变化时更新 isMobile
const handleResize = () => {
  layoutStore.handleResize()
}

onMounted(() => {
  window.addEventListener('resize', handleResize)
  layoutStore.handleResize()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})

// 关闭侧边栏
const closeSidebar = () => {
  if (layoutStore.isMobile) {
    layoutStore.showSidebar = false
  }
}

// 解析JWT token获取用户身份
const getUserRole = () => {
  const token = authorizationStore.getAuthorization
  if (!token) return null

  try {
    const payload = token.split('.')[1]
    const decodedPayload = atob(payload)
    const userInfo = JSON.parse(decodedPayload)
    return userInfo['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        userInfo.role ||
        null
  } catch (e) {
    console.error('解析token失败:', e)
    return null
  }
}

// 使用iconify图标
const menuItems = [
  {name: '主页', path: '/Centre', icon: 'ion:home-outline'},
  {name: '个人数据', path: '/Centre/PersonalData', icon: 'ion:person'},
  {name: '社团部门', path: '/Centre/Department', icon: 'ion:business', requiresRole: 'Minister'},
  {name: '社团资源', path: '/Centre/Resources', icon: 'ion:book', requiresRole: 'Minister'},
  {name: '社团文章', path: '/Centre/Article', icon: 'ion:document-text', requiresRole: 'Minister'},
  {name: '成员数据', path: '/Centre/MemberData', icon: 'ion:people', requiresRole: 'Minister'},
  {name: '其他数据', path: '/Centre/Admin', icon: 'ion:cog', requiresRole: 'Minister'},
  {name: '客户端 OAuth2 管理', path: '/Centre/Client', icon: 'ion:key', requiresRole: 'Minister'},
  {name: '日志查看', path: '/Centre/Logs', icon: 'ion:albums', requiresRole: 'Minister'},
]

// 根据用户角色过滤菜单项
const filteredMenuItems = computed(() => {
  const userRole = getUserRole()
  if (!userRole) return menuItems.filter(item => !item.requiresRole)

  // 定义角色层级
  const roleHierarchy: Record<string, number> = {
    'Member': 1,
    'Department': 2,
    'Minister': 3,
    'President': 4,
    'Founder': 5
  }

  const userRoleLevel = roleHierarchy[userRole as string] || 0

  return menuItems.filter(item => {
    if (!item.requiresRole) return true

    const requiredRoleLevel = roleHierarchy[item.requiresRole] || 0
    return userRoleLevel >= requiredRoleLevel
  })
})

const logout = async () => {
  await authorizationStore.logout()
  await router.push('/')
}

const handleImageError = (e: Event) => {
  const target = e.target as HTMLImageElement
  target.src = '/assets/default-logo.png'
}
</script>

<style scoped>
/* 自定义滚动条样式 */
::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: rgba(156, 163, 175, 0.3);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(156, 163, 175, 0.5);
}

.dark ::-webkit-scrollbar-thumb {
  background: rgba(156, 163, 175, 0.2);
}

.dark ::-webkit-scrollbar-thumb:hover {
  background: rgba(156, 163, 175, 0.4);
}

a {
  text-decoration: none;
  color: inherit;
}
</style>
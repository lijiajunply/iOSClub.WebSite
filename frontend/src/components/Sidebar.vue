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
        fixed top-0 left-0 h-screen w-64 
        bg-white/90 dark:bg-gray-800/90 backdrop-blur-md
        border-r border-gray-100 dark:border-gray-700
        transition-transform duration-300 ease-in-out
        z-50 overflow-hidden
        flex flex-col
      "
        :class="{
        'transform -translate-x-full': layoutStore.isMobile && !layoutStore.showSidebar,
        'shadow-lg': layoutStore.isMobile
      }"
    >
      <!-- 侧边栏头部 -->
      <div class="p-6 border-b border-gray-100 dark:border-gray-700 flex items-center">
        <router-link to="/" class="flex items-center gap-3 group">
          <img
              src="/assets/iOS_Club_LOGO.png"
              alt="iOS Club Logo"
              class="w-10 h-10 rounded-lg object-contain"
              @error="handleImageError"
          />
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">iMember</h2>
        </router-link>
      </div>

      <!-- 侧边栏导航 -->
      <nav class="flex-1 overflow-y-auto py-4">
        <ul class="space-y-1 px-3">
          <li v-for="item in filteredMenuItems" :key="item.name">
            <router-link
                :to="item.path"
                class="
                flex items-center px-4 py-2.5 rounded-lg transition-all duration-200
                text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700/50 hover:!text-blue-600 dark:hover:!text-blue-400
              "

                :class="{
                'bg-blue-50 dark:bg-blue-900/30 !text-blue-600 dark:!text-blue-400':
                   $route.path === item.path
              }"
                @click="closeSidebar"
            >
              <component :is="item.icon" class="w-5 h-5 mr-3"/>
              <span class="text-sm font-medium">{{ item.name }}</span>
            </router-link>
          </li>
        </ul>
      </nav>

      <!-- 侧边栏底部 -->
      <div class="p-4 border-t border-gray-100 dark:border-gray-700">
        <NButton
            quaternary
            class="w-full justify-start"
            @click="logout"
        >
          <n-icon class="mr-2">
            <LogOut :size="18"/>
          </n-icon>
          <span class="font-medium text-sm">退出登录</span>
        </NButton>
      </div>
    </aside>

    <!-- 主内容区域的边距 -->
    <div
        class="transition-all duration-300"
        :style="{ marginLeft: layoutStore.showSidebar && !layoutStore.isMobile ? '16rem' : '0' }"
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
import {LogOut, HomeOutline, Person, Business, Book, DocumentText, People, Cog, Key} from '@vicons/ionicons5'

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

// 使用组件图标替换emoji
const menuItems = [
  {name: '主页', path: '/Centre', icon: HomeOutline},
  {name: '个人数据', path: '/Centre/PersonalData', icon: Person},
  {name: '社团部门', path: '/Centre/Department', icon: Business, requiresRole: 'Minister'},
  {name: '社团资源', path: '/Centre/Resources', icon: Book, requiresRole: 'Minister'},
  {name: '社团文章', path: '/Centre/Article', icon: DocumentText, requiresRole: 'Minister'},
  {name: '成员数据', path: '/Centre/MemberData', icon: People, requiresRole: 'Minister'},
  {name: '其他数据', path: '/Centre/Admin', icon: Cog, requiresRole: 'Minister'},
  {name: '客户端 OAuth2 管理', path: '/Centre/Client', icon: Key, requiresRole: 'Minister'}
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

const logout = () => {
  authorizationStore.logout()
  router.push('/')
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

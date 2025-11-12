<template>
  <div class="min-h-screen bg-white dark:bg-black transition-colors duration-300">
    <n-layout>
      <!-- Header with apple-style -->
      <n-layout-header
        class="fixed top-0 left-0 right-0 z-50 bg-white/80 dark:bg-black/80 backdrop-blur-xl border-b border-gray-200 dark:border-gray-800"
        :class="{ 'shadow-md': isScrolled }"
      >
        <div class="mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex items-center justify-between h-16">
            <!-- Logo and Title -->
            <router-link to="/" class="flex items-center gap-3 group">
              <img
                src="/assets/iOS_Club_LOGO.png"
                alt="iOS Club Logo"
                class="w-8 h-8 transition-transform group-hover:scale-105"
              />
              <div class="text-lg font-semibold text-gray-900 dark:text-gray-100 hidden sm:block">
                iOS Club of XAUAT
              </div>
            </router-link>

            <!-- Desktop Menu -->
            <nav class="hidden md:flex items-center space-x-1">
              <!-- 关于我们 Dropdown -->
              <n-dropdown
                trigger="hover"
                :options="aboutUsOptions"
                @select="handleSelect"
                placement="bottom"
              >
                <button
                  class="px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors flex items-center"
                >
                  关于我们
                  <Icon icon="material-symbols:arrow-drop-down" class="ml-1" />
                </button>
              </n-dropdown>

              <!-- 社团动态 Dropdown -->
              <n-dropdown
                trigger="hover"
                :options="communityOptions"
                @select="handleSelect"
                placement="bottom"
              >
                <button
                  class="px-3 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors flex items-center"
                >
                  社团动态
                  <Icon icon="material-symbols:arrow-drop-down" class="ml-1" />
                </button>
              </n-dropdown>

              <!-- Login/Register or Logout Button -->
              <button
                v-if="!isCentreRoute"
                class="ml-4 px-4 py-2 bg-blue-500 text-white text-sm font-medium rounded-full hover:bg-blue-600 transition-colors"
                @click="() => router.push('/login')"
              >
                登录/注册
              </button>
              <button
                v-else
                class="ml-4 px-4 py-2 bg-blue-500 text-white text-sm font-medium rounded-full hover:bg-blue-600 transition-colors"
                @click="toCentre"
              >
                进入中心
              </button>

              <!-- Theme Toggle -->
              <button
                @click="mainToggleTheme"
                class="ml-2 w-9 h-9 flex items-center justify-center rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                aria-label="切换暗夜模式"
              >
                <Icon
                  v-if="!isDark"
                  icon="material-symbols:light-mode"
                  class="h-5 w-5 text-yellow-500"
                />
                <Icon v-else icon="material-symbols:dark-mode" class="h-5 w-5 text-blue-400" />
              </button>
            </nav>

            <!-- Mobile Menu Button -->
            <button
              class="md:hidden p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              @click="drawerVisible = !drawerVisible"
            >
              <Icon
                v-if="!drawerVisible"
                icon="material-symbols:menu"
                class="text-gray-900 dark:text-gray-100"
                width="20"
                height="20"
              />
              <Icon
                v-else
                icon="material-symbols:close"
                class="text-gray-900 dark:text-gray-100"
                width="20"
                height="20"
              />
            </button>
          </div>
        </div>
      </n-layout-header>

      <!-- Main Content -->
      <n-layout-content class="pt-16">
        <!-- Mobile Drawer Menu -->
        <transition name="slide-down">
          <div
            v-if="drawerVisible"
            class="fixed inset-0 z-40 bg-white dark:bg-black pt-16"
          >
            <div class="px-4 py-1 space-y-0.5">
              <router-link
                to="/"
                class="block px-4 py-2.5 text-base font-medium text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800"
                @click="drawerVisible = false"
              >
                首页
              </router-link>

              <!-- About Section -->
              <div class="pt-1.5 pb-0.5">
                <div class="px-4 text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  关于我们
                </div>
                <router-link
                  v-for="item in aboutUsOptions"
                  :key="item.key"
                  :to="item.key"
                  class="block px-4 py-2.5 text-base font-medium text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800"
                  @click="drawerVisible = false"
                >
                  {{ item.label }}
                </router-link>
              </div>

              <!-- Community Section -->
              <div class="pt-1.5 pb-1.5">
                <div class="px-4 text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  社团动态
                </div>
                <router-link
                  v-for="item in communityOptions"
                  :key="item.key"
                  :to="item.key"
                  class="block px-4 py-2.5 text-base font-medium text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800"
                  @click="drawerVisible = false"
                >
                  {{ item.label }}
                </router-link>
              </div>

              <div class="pt-1.5 border-t border-gray-200 dark:border-gray-800">
                <button
                  v-if="!isCentreRoute"
                  class="w-full px-4 py-2.5 bg-blue-500 text-white text-base font-medium rounded-lg hover:bg-blue-600 transition-colors"
                  @click="() => { router.push('/login'); drawerVisible = false }"
                >
                  登录/注册
                </button>
                <button
                  v-else
                  class="w-full px-4 py-2.5 bg-blue-500 text-white text-base font-medium rounded-lg hover:bg-blue-600 transition-colors"
                  @click="logout"
                >
                  退出登录
                </button>

                <button
                  @click="mainToggleTheme"
                  class="mt-1.5 w-full px-4 py-2.5 flex items-center justify-center gap-2 text-base font-medium text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                >
                  <Icon
                    v-if="!isDark"
                    icon="material-symbols:light-mode"
                    class="h-5 w-5 text-yellow-500"
                  />
                  <Icon v-else icon="material-symbols:dark-mode" class="h-5 w-5 text-blue-400" />
                  {{ isDark ? '浅色模式' : '深色模式' }}
                </button>
              </div>
            </div>
          </div>
        </transition>

        <!-- Page Content -->
        <div class="flex-1">
          <router-view />
        </div>
      </n-layout-content>
    </n-layout>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthorizationStore } from '../stores/Authorization'
import { NLayout, NLayoutHeader, NLayoutContent, NDropdown } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { useThemeStore } from '../stores/theme'
import { storeToRefs } from 'pinia'

const themeStore = useThemeStore()
const { isDark } = storeToRefs(themeStore)
// 解构方法
const { toggleTheme } = themeStore

// 切换主题
const mainToggleTheme = () => {
  toggleTheme()
}

const router = useRouter()
const authorizationStore = useAuthorizationStore()
const drawerVisible = ref(false)
const isScrolled = ref(false)

// 判断当前是否在Centre路由下
const isCentreRoute = computed(() => {
  return authorizationStore.isAuthenticated
})

// Handle scroll effect
const handleScroll = () => {
  isScrolled.value = window.scrollY > 10
}

const toCentre = () => {
  router.push('/Centre')
}

// 退出登录功能
const logout = () => {
  authorizationStore.logout()
  drawerVisible.value = false
  // 如果当前在Centre相关页面，跳转到首页
  if (isCentreRoute.value) {
    router.push('/')
  }
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})

// Dropdown options for desktop menu
const aboutUsOptions = [
  {
    label: '社团简介',
    key: '/About'
  },
  {
    label: '社团结构',
    key: '/Structure'
  },
  {
    label: '其他组织',
    key: '/OtherOrg'
  },
  {
    label: '竞赛资源',
    key: '/Article/Competitions'
  },
  {
    label: '社团历史',
    key: '/History'
  }
]

const communityOptions = [
  {
    label: '社团活动',
    key: '/Event'
  },
  {
    label: '技术博客',
    key: '/Blog'
  },
  {
    label: 'iOS App',
    key: '/Tools'
  },
  {
    label: '精品项目',
    key: '/Projects'
  }
]

// Handle dropdown selection
const handleSelect = (key: string) => {
  router.push(key)
}
</script>

<style scoped>
/* Animations */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}

.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
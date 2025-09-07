<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthorizationStore } from '../stores/Authorization.ts'
// 补充图标导入（根据实际图标库调整）
import { MenuOutline } from '@vicons/ionicons5'

const router = useRouter()
const authorizationStore = useAuthorizationStore()

const isScrolled = ref(false)
const drawerVisible = ref(false)

const handleScroll = () => {
  isScrolled.value = window.scrollY > 10
}

const logout = () => {
  authorizationStore.logout()
  router.push('/')
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
  // 初始化时检查一次滚动状态
  handleScroll()
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<template>
  <n-layout class="min-h-screen bg-gray-50">
    <!-- Header -->
    <n-layout-header
        class="apple-header"
        :class="{ 'scrolled': isScrolled }"
    >
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <div class="flex items-center gap-3">
            <!-- 优化图片加载 -->
            <img
                src="../assets/iOS_Club_LOGO.png"
                alt="iOS Club Logo"
                class="w-10 h-10"
                @error="(e: Event) => (e.target as HTMLImageElement).src = '../assets/default-logo.png'"
            />
            <div class="text-xl font-semibold text-gray-900">
              iOS Club 管理中心
            </div>
          </div>

          <!-- Desktop Menu -->
          <nav class="hidden md:flex items-center space-x-4">
            <router-link to="/admin/centre" class="nav-link" aria-current="page">
              控制台
            </router-link>
            <router-link to="/admin/member-data" class="nav-link">
              成员管理
            </router-link>
            <router-link to="/admin/department" class="nav-link">
              部门管理
            </router-link>
            <router-link to="/admin/projects" class="nav-link">
              项目管理
            </router-link>

            <button
                class="apple-button-primary ml-4"
                @click="logout"
                aria-label="退出登录"
            >
              登出
            </button>
          </nav>

          <!-- Mobile Menu Button -->
          <button
              class="md:hidden apple-icon-button"
              @click="drawerVisible = !drawerVisible"
              aria-expanded="drawerVisible"
              aria-label="菜单"
          >
            <n-icon size="24">
              <MenuOutline/>
            </n-icon>
          </button>
        </div>
      </div>
    </n-layout-header>

    <!-- Mobile Drawer Menu -->
    <transition name="slide-down">
      <div
          v-if="drawerVisible"
          class="mobile-menu md:hidden"
          role="navigation"
      >
        <div class="px-4 py-6 space-y-1">
          <router-link
              to="/admin/centre"
              class="mobile-menu-item"
              @click="drawerVisible = false"
              aria-current="page"
          >
            控制台
          </router-link>
          <router-link
              to="/admin/member-data"
              class="mobile-menu-item"
              @click="drawerVisible = false"
          >
            成员管理
          </router-link>
          <router-link
              to="/admin/department"
              class="mobile-menu-item"
              @click="drawerVisible = false"
          >
            部门管理
          </router-link>
          <router-link
              to="/admin/projects"
              class="mobile-menu-item"
              @click="drawerVisible = false"
          >
            项目管理
          </router-link>
          <button
              class="mobile-menu-item text-left w-full"
              @click="() => { drawerVisible = false; logout() }"
              aria-label="退出登录"
          >
            登出
          </button>
        </div>
      </div>
    </transition>

    <!-- 移动端遮罩层（优化体验） -->
    <transition name="fade">
      <div
          v-if="drawerVisible"
          class="mobile-menu-overlay md:hidden"
          @click="drawerVisible = false"
      ></div>
    </transition>

    <!-- Main Content -->
    <n-layout-content class="pt-16">
      <router-view />
    </n-layout-content>
  </n-layout>
</template>

<style scoped>
@import 'tailwindcss';

.apple-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
}

.apple-header.scrolled {
  background: rgba(255, 255, 255, 0.95);
  box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1);
}

.nav-link {
  @apply px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-100 transition-colors duration-200;
}

.apple-button-primary {
  @apply px-4 py-2 rounded-md text-sm font-medium text-white bg-gradient-to-r from-blue-500 to-purple-600 hover:from-blue-600 hover:to-purple-700 transition-all duration-300 shadow-md hover:shadow-lg;
}

.apple-icon-button {
  @apply p-2 rounded-md text-gray-700 hover:text-gray-900 hover:bg-gray-100 transition-colors duration-200;
}

.mobile-menu {
  position: fixed;
  top: 64px;
  left: 0;
  right: 0;
  background: white;
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  z-index: 999;
}

.mobile-menu-item {
  @apply block px-4 py-3 rounded-md text-base font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 transition-colors duration-200;
}

.mobile-menu-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.2);
  z-index: 998;
}

.mobile-menu-section {
  @apply border-t border-gray-200 pt-4;
}

.mobile-menu-section-title {
  @apply px-4 py-2 text-xs font-semibold text-gray-500 uppercase tracking-wider;
}

.mobile-menu-subitem {
  @apply block px-4 py-3 rounded-md text-base font-medium text-gray-600 hover:text-gray-900 hover:bg-gray-50 transition-colors duration-200 ml-4;
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: transform 0.3s ease;
}

.slide-down-enter-from,
.slide-down-leave-to {
  transform: translateY(-100%);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
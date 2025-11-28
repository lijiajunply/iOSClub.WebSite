<template>
  <div class="layout-container">
    <n-layout
        class="h-[calc(100vh-64px)] bg-ios transition-colors duration-300"
        has-sider
    >
      <n-layout-sider
          v-if="!isMobile"
          collapse-mode="width"
          :collapsed-width="0"
          :width="280"
          :native-scrollbar="false"
          bordered
          class="glass-sidebar"
      >
        <div class="flex flex-col h-full">
          <!-- Sidebar Header / Search placeholder -->
          <div class="p-4" style="">
            <div class="flex items-center space-x-2 text-gray-500 dark:text-gray-400">
              <Icon icon="lucide:layout-grid" class="w-5 h-5"/>
              <span class="text-sm font-semibold tracking-wide uppercase opacity-70">Library</span>
            </div>
          </div>

          <!-- Menu Content Wrapper -->
          <n-scrollbar class="flex-1 -mr-2 pr-2">
            <MenuContent @menu-item-click="handleMenuClick"/>
          </n-scrollbar>
        </div>
      </n-layout-sider>

      <!-- Main Content Area -->
      <n-layout-content
          class="bg-transparent"
          :class="{ 'p-0': isMobile, 'p-4': !isMobile }"
      >
        <div class="content-card w-full h-full flex flex-col overflow-hidden bg-white dark:bg-[#1e1e1e]">

          <!-- Breadcrumb / Mobile Header Toolbar -->
          <div
              class="h-14 px-6 flex items-center justify-between border-b border-gray-100 dark:border-white/5 shrink-0 glass-header z-10">
            <!-- Mobile Toggle -->
            <button
                v-if="isMobile"
                @click="mobileMenuOpen = true"
                class="p-2 -ml-2 rounded-full hover:bg-gray-100 dark:hover:bg-white/10 transition-colors"
            >
              <Icon icon="lucide:menu" class="w-6 h-6 text-gray-600 dark:text-gray-300"/>
            </button>

            <!-- Title / Breadcrumb placeholder -->
            <div class="flex items-center space-x-2 text-sm font-medium text-gray-500 dark:text-gray-400">
              <span>Documents</span>
              <!-- Dynamic Paths -->
              <template v-for="(crumb, index) in breadcrumbs" :key="index">
                <Icon icon="lucide:chevron-right" class="w-4 h-4 mx-1 text-gray-300 dark:text-gray-600 shrink-0"/>
                <Icon :icon="index === breadcrumbs.length - 1 ? 'lucide:file-text' : 'lucide:folder' "
                      class="w-4 h-4 mx-1 shrink-0"
                      :class="index === breadcrumbs.length - 1
                          ? 'font-semibold text-gray-800 dark:text-gray-100'
                          : 'text-gray-500 dark:text-gray-400'"/>
                <span
                    class="truncate"
                    :class="index === breadcrumbs.length - 1
                          ? 'font-semibold text-gray-800 dark:text-gray-100'
                          : 'text-gray-500 dark:text-gray-400'"
                >
                        {{ crumb }}
                      </span>
              </template>
            </div>

            <!-- Right Actions -->
            <div class="flex items-center space-x-3">
              <Icon icon="lucide:search"
                    class="w-5 h-5 text-gray-400 cursor-pointer hover:text-gray-600 dark:hover:text-gray-200"/>
              <Icon icon="lucide:more-horizontal"
                    class="w-5 h-5 text-gray-400 cursor-pointer hover:text-gray-600 dark:hover:text-gray-200"/>
            </div>
          </div>

          <!-- Router View Container -->
          <div class="flex-1 overflow-auto relative scroll-smooth">
            <div class="min-h-full">
              <router-view v-slot="{ Component }">
                <transition name="fade-scale" mode="out-in">
                  <component :is="Component"/>
                </transition>
              </router-view>
            </div>
          </div>
        </div>
      </n-layout-content>
    </n-layout>

    <!-- Mobile Sheet/Drawer (iOS Style) -->
    <n-drawer
        v-model:show="mobileMenuOpen"
        placement="bottom"
        height="85vh"
        class="ios-drawer rounded-t-2xl"
        :trap-focus="false"
        :block-scroll="false"
    >
      <n-drawer-content
          body-content-style="padding: 0;"
          class="bg-gray-50 dark:bg-[#1c1c1e]"
      >
        <template #header>
          <div class="w-full flex justify-center pt-2 pb-4 cursor-grab">
            <div class="w-12 h-1.5 bg-gray-300 dark:bg-gray-600 rounded-full"></div>
          </div>
        </template>

        <div class="px-6 pb-10 h-full overflow-y-auto">
          <h3 class="text-2xl font-bold mb-6 text-gray-900 dark:text-white tracking-tight">Navigation</h3>
          <MenuContent @menu-item-click="handleMenuClick"/>
        </div>
      </n-drawer-content>
    </n-drawer>

  </div>
</template>

<script setup lang="ts">
import {computed, onBeforeUnmount, onMounted, ref} from 'vue'
import {useRouter, useRoute} from 'vue-router'
import {NDrawer, NDrawerContent, NLayout, NLayoutContent, NLayoutSider, NScrollbar} from 'naive-ui'
import {Icon} from '@iconify/vue'
import MenuContent from '../components/MenuContent.vue'

const router = useRouter()
const route = useRoute()
const mobileMenuOpen = ref(false)
const windowWidth = ref(window.innerWidth)

// Breakpoint for mobile view (iPad Mini portrait and below)
const MOBILE_BREAKPOINT = 768
const isMobile = computed(() => windowWidth.value < MOBILE_BREAKPOINT)

const handleResize = () => {
  windowWidth.value = window.innerWidth
  if (!isMobile.value) {
    mobileMenuOpen.value = false
  }
}

const handleMenuClick = (key: string) => {
  if (isMobile.value) {
    mobileMenuOpen.value = false
  }
  router.push(key)
}

const breadcrumbs = computed(() => {
  const path = route.path
  if (!path) return []

  return decodeURIComponent(path)
      .split('/')
      .filter(segment => segment && segment !== 'Article')
})

onMounted(() => {
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.layout-container {
  /* Set base font stack similar to Apple System */
  font-family: -apple-system, BlinkMacSystemFont, "SF Pro Text", "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
}

/* Light Mode Sidebar Background (Apple Sidebar Grey) */
.bg-ios :deep(.n-layout-sider) {
  background-color: #fbfbfd;
}

/* Dark Mode Sidebar Background (Apple Dark Grey) */
.dark .bg-ios :deep(.n-layout-sider) {
  background-color: #2d2d2d; /* Slightly lighter than pure black */
}

/* Sidebar Styling */
.glass-sidebar :deep(.n-layout-sider-scroll-container) {
  /* Ensure content doesn't get cut off */
  min-height: 100%;
}

/* Apply backdrop blur if supported for that frosted glass look */
@supports (backdrop-filter: blur(20px)) {
  .glass-sidebar {
    background-color: rgba(249, 249, 251, 0.8) !important;
    backdrop-filter: blur(20px);
    -webkit-backdrop-filter: blur(20px);
  }

  .dark .bg-ios .glass-sidebar {
    background-color: rgba(45, 45, 45, 0.8) !important;
  }
}

/* The Main "Card" area */
.content-card {
  /* Only round corners on Desktop */
  border-radius: 12px;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05),
  0 4px 12px 0 rgba(0, 0, 0, 0.02);

  /* Subtle border for definition */
  border: 1px solid rgba(0, 0, 0, 0.06);
}

.bg-ios-dark .content-card {
  border: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow: 0 4px 24px 0 rgba(0, 0, 0, 0.2);
}

/* Mobile specific adjustments */
@media (max-width: 768px) {
  .content-card {
    border-radius: 0;
    border: none;
  }
}

/* Animations */
.fade-scale-enter-active,
.fade-scale-leave-active {
  transition: opacity 0.25s ease, transform 0.25s cubic-bezier(0.2, 0.0, 0.2, 1);
}

.fade-scale-enter-from {
  opacity: 0;
  transform: scale(0.98) translateY(10px);
}

.fade-scale-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Scrollbar Customization to look like macOS native */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background-color: rgba(0, 0, 0, 0.2);
  border-radius: 10px;
  border: 2px solid transparent; /* Creates padding effect */
  background-clip: content-box;
}

::-webkit-scrollbar-thumb:hover {
  background-color: rgba(0, 0, 0, 0.4);
}

.dark ::-webkit-scrollbar-thumb {
  background-color: rgba(255, 255, 255, 0.2);
}

.dark ::-webkit-scrollbar-thumb:hover {
  background-color: rgba(255, 255, 255, 0.4);
}

/* Drawer Styling */
:deep(.n-drawer.ios-drawer) {
  border-top-left-radius: 16px;
  border-top-right-radius: 16px;
}
</style>
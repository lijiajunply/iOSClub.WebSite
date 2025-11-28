<template>
  <!--
    App Background:
    Light: 浅灰色 (类似 iPadOS 背景)
    Dark: 纯黑或极深灰
  -->
  <div class="flex h-screen w-full overflow-hidden bg-[#F2F2F7] dark:bg-[#000000]">

    <Sidebar />

    <div class="flex-1 flex flex-col h-full relative min-w-0">

      <!-- Header: 使用 Sticky 定位和高斯模糊，模仿 iOS 导航栏 -->
      <header class="sticky top-0 z-30 shrink-0 header-glass border-b border-gray-200/30 dark:border-white/5 h-16">
        <div class="flex items-center justify-between px-4 h-full max-w-[1600px] mx-auto w-full">

          <!-- Left: Mobile Toggle & Title -->
          <div class="flex items-center gap-4 overflow-hidden">
            <button
                v-if="isMobile"
                @click="toggleSidebar"
                class="p-2 -ml-2 rounded-full active:bg-gray-200 dark:active:bg-white/10 transition-colors"
            >
              <Icon icon="ph:list" class="text-gray-900 dark:text-white text-xl" />
            </button>

            <div class="flex flex-col justify-center">
              <h1
                  class="text-[17px] md:text-[19px] font-semibold text-gray-900 dark:text-gray-100 truncate leading-tight"
              >
                {{ pageTitle || 'iMember' }}
              </h1>
              <p
                  v-if="pageSubtitle"
                  class="text-[11px] md:text-[13px] text-gray-500 dark:text-gray-400 truncate font-medium"
              >
                {{ pageSubtitle }}
              </p>
            </div>
          </div>

          <!-- Right: Actions & Theme -->
          <div class="flex items-center gap-3 shrink-0">
            <!-- Dynamic Action Component -->
            <div v-if="showPageActions" class="hidden sm:flex items-center gap-2">
              <component :is="layoutStore.actionsComponent" v-if="layoutStore.actionsComponent" />
              <div v-else v-html="layoutStore.pageActionsContent" />
            </div>

            <div class="h-4 w-[1px] bg-gray-300 dark:bg-gray-700 mx-1 hidden sm:block"></div>

            <!-- Theme Toggle (Apple style switch) -->
            <button
                @click="toggleTheme"
                class="group relative p-2 rounded-full transition-all duration-300 bg-gray-200/50 dark:bg-white/10 hover:bg-gray-300/50 dark:hover:bg-white/20"
                aria-label="切换主题"
            >
              <div class="relative w-5 h-5 overflow-hidden">
                <Icon
                    v-if="!isDark"
                    icon="ph:sun-fill"
                    class="w-5 h-5 text-orange-500 absolute transition-all duration-500 rotate-0 scale-100"
                />
                <Icon
                    v-else
                    icon="ph:moon-stars-fill"
                    class="w-5 h-5 text-yellow-400 absolute transition-all duration-500 rotate-0 scale-100"
                />
              </div>
            </button>
          </div>
        </div>
      </header>

      <!-- Mobile Only: Extra Actions Bar (如果 Header 放不下) -->
      <div
          v-if="showPageActions && isMobile"
          class="sm:hidden px-4 py-2 bg-white/50 dark:bg-gray-800/50 backdrop-blur-md border-b border-gray-200/30 dark:border-white/5 flex justify-end gap-2"
      >
        <component :is="layoutStore.actionsComponent" v-if="layoutStore.actionsComponent" />
        <div v-else v-html="layoutStore.pageActionsContent" />
      </div>

      <!-- Main Content Area -->
      <main class="flex-1 overflow-y-auto overflow-x-hidden">
        <div class="w-full">
          <router-view>
          </router-view>
        </div>
      </main>

    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Icon } from '@iconify/vue'
import Sidebar from '../components/Sidebar.vue'
import { useAuthorizationStore } from '../stores/Authorization'
import { useThemeStore } from '../stores/theme'
import { storeToRefs } from 'pinia'
import { useLayoutStore } from "../stores/LayoutStore"

const router = useRouter()
const store = useAuthorizationStore()
const themeStore = useThemeStore()
const { isDark } = storeToRefs(themeStore)
const { toggleTheme } = themeStore
const layoutStore = useLayoutStore()
const { isMobile, pageTitle, pageSubtitle, showPageActions } = storeToRefs(layoutStore)

const toggleSidebar = () => {
  layoutStore.showSidebar = !layoutStore.showSidebar
}

onMounted(async () => {
  const isValid = await store.validate()
  if (!isValid) {
    await store.logout()
    await router.push('/login')
  }
})
</script>

<style scoped>
.header-glass {
  background-color: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
}

.dark .header-glass {
  background-color: rgba(28, 28, 30, 0.7);
}

/* 页面切换动画 */
.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.page-fade-enter-from {
  opacity: 0;
  transform: translateY(5px);
}

.page-fade-leave-to {
  opacity: 0;
  transform: translateY(-5px);
}
</style>
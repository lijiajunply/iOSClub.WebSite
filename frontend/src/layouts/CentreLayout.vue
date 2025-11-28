<template>
  <div class="flex min-h-screen bg-gray-50 dark:bg-black transition-colors duration-300">
    <Sidebar/>

    <div class="flex-1 flex flex-col overflow-hidden h-screen">
      <!-- Header -->
      <header class="bg-white/90 dark:bg-gray-800/90 backdrop-blur-md h-16 border-b border-gray-100 dark:border-gray-700">
        <div class="flex items-center justify-between px-2 py-3 h-full max-w-7xl mx-auto">
          <div class="md:hidden flex items-center gap-3">
            <button
                @click="toggleSidebar"
                class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 mr-2 transition-colors"
            >
              <Icon
                  icon="material-symbols:menu"
                  class="text-gray-700 dark:text-gray-300"
                  width="32"
                  height="32"
              />
            </button>
          </div>
          
          <!-- Desktop Header Content -->
          <div class="hidden md:flex items-center justify-between flex-1 mx-4">
            <div v-if="pageTitle">
              <h1 class="text-lg font-semibold tracking-tight text-gray-900 dark:text-white">{{ pageTitle }}</h1>
              <p v-if="pageSubtitle" class="text-xs text-gray-500 dark:text-gray-400">{{ pageSubtitle }}</p>
            </div>
            <div class="flex items-center space-x-2" v-if="showPageActions">
              <slot name="actions"></slot>
            </div>
          </div>
          
          <div class="flex items-center space-x-2">
            <!-- Theme Toggle -->
            <button
                @click="toggleTheme"
                class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                aria-label="切换主题"
            >
              <Icon
                  v-if="!isDark"
                  icon="material-symbols:light-mode"
                  class="h-6 w-6 text-yellow-500"
              />
              <Icon
                  v-else
                  icon="material-symbols:dark-mode"
                  class="h-6 w-6 text-blue-400"
              />
            </button>
          </div>
        </div>
      </header>

      <!-- Mobile Header Content -->
      <div v-if="pageTitle && isMobile" class="bg-white/90 dark:bg-gray-800/90 border-b border-gray-100 dark:border-gray-700 px-4 py-3">
        <div class="flex flex-col space-y-3">
          <div>
            <h1 class="text-lg font-semibold tracking-tight text-gray-900 dark:text-white">{{ pageTitle }}</h1>
            <p v-if="pageSubtitle" class="text-xs text-gray-500 dark:text-gray-400">{{ pageSubtitle }}</p>
          </div>
          <div class="flex items-center space-x-2" v-if="showPageActions">
            <slot name="actions"></slot>
          </div>
        </div>
      </div>

      <!-- Main Content -->
      <main class="flex-1 overflow-y-auto bg-white/90 dark:bg-gray-800/90">
        <router-view/>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import {onMounted} from 'vue'
import {useRouter} from 'vue-router'
import {Icon} from '@iconify/vue'
import Sidebar from '../components/Sidebar.vue'
import {useAuthorizationStore} from '../stores/Authorization'
import {useThemeStore} from '../stores/theme'
import {storeToRefs} from 'pinia'
import {useLayoutStore} from "../stores/LayoutStore";

const router = useRouter()
const store = useAuthorizationStore()
const themeStore = useThemeStore()
const {isDark} = storeToRefs(themeStore)
const {toggleTheme} = themeStore
const layoutStore = useLayoutStore()
const {isMobile, pageTitle, pageSubtitle, showPageActions} = storeToRefs(layoutStore)

const toggleSidebar = () => {
  layoutStore.showSidebar = !layoutStore.showSidebar
}

onMounted(async () => {
  const isValid = await store.validate()
  if (!isValid) {
    store.logout()
    await router.push('/login')
  }
})
</script>
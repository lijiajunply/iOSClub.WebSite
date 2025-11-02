<template>
  <div class="flex min-h-screen bg-gray-50 dark:bg-black transition-colors duration-300">
    <Sidebar v-if="sidebarVisible"/>

    <div class="flex-1 flex flex-col overflow-hidden h-screen">
      <!-- Header -->
      <header class="bg-white/90 dark:bg-gray-800/90 backdrop-blur-md border-b border-gray-200 dark:border-gray-800 h-22">
        <div class="flex items-center justify-between px-4 py-3 h-full">
          <div class="flex items-center">
            <button
                @click="toggleSidebar"
                class="block md:hidden p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 mr-2 transition-colors"
            >
              <Icon
                  icon="material-symbols:menu"
                  class="text-gray-700 dark:text-gray-300"
                  width="32"
                  height="32"
              />
            </button>
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
                  class="h-8 w-8 text-yellow-500"
              />
              <Icon
                  v-else
                  icon="material-symbols:dark-mode"
                  class="h-8 w-8 text-blue-400"
              />
            </button>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <main class="flex-1 overflow-y-auto bg-white/90 dark:bg-gray-800/90">
        <router-view/>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref} from 'vue'
import {useRouter} from 'vue-router'
import {Icon} from '@iconify/vue'
import Sidebar from '../components/Sidebar.vue'
import {useAuthorizationStore} from '../stores/Authorization'
import {useThemeStore} from '../stores/theme'
import {storeToRefs} from 'pinia'

const router = useRouter()
const store = useAuthorizationStore()
const themeStore = useThemeStore()
const {isDark} = storeToRefs(themeStore)
const {toggleTheme} = themeStore

// 控制侧边栏显示状态
const sidebarVisible = ref(true)

const toggleSidebar = () => {
  sidebarVisible.value = !sidebarVisible.value
}

onMounted(async () => {
  const isValid = await store.validate()
  if (!isValid) {
    store.logout()
    await router.push('/login')
  }
})
</script>
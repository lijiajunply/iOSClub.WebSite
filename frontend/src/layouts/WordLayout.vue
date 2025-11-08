<template>
  <n-layout class="bg-gray-100 dark:bg-neutral-900" style="height: calc(100vh - 64px);">
    <n-layout has-sider position="absolute" class="macos-layout">
      <!-- Desktop sidebar (macOS style) -->
      <n-layout-sider
          :collapsed-width="4"
          :width="280"
          :native-scrollbar="false"
          show-trigger="bar"
          :on-update-collapsed="(b: boolean) => (collapsed = b)"
          :collapsed="collapsed"
          class="bg-gray-50 dark:bg-neutral-800 border-r border-gray-200 dark:border-neutral-700 macos-sidebar"
      >
        <n-scrollbar class="h-screen">
          <div class="pb-20 pt-2">
            <MenuContent @menu-item-click="handleMenuClick"/>
          </div>
        </n-scrollbar>
      </n-layout-sider>

      <!-- Main content area -->
      <n-layout-content class="relative">
        <div class="min-h-screen">
          <!-- Mobile menu button -->
          <Transition name="fade">
            <button
                v-if="!drawerVisible"
                @click="drawerVisible = true"
                class="md:hidden fixed bottom-6 right-6 z-50 w-14 h-14 bg-blue-500 rounded-full shadow-lg flex items-center justify-center hover:shadow-xl transition-all duration-200"
            >
              <Icon icon="mdi:menu" width="24" class="text-white"/>
            </button>
          </Transition>

          <!-- Page content slot -->
          <router-view/>
        </div>
      </n-layout-content>
    </n-layout>

    <!-- Mobile drawer menu -->
    <n-drawer v-model:show="drawerVisible" :width="280" placement="right">
      <n-drawer-content>
        <template #header>
          <h3 class="text-lg font-semibold">文档导航</h3>
        </template>
        <div class="py-2">
          <MenuContent @menu-item-click="handleMenuClick"/>
        </div>
      </n-drawer-content>
    </n-drawer>
  </n-layout>
</template>

<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount} from 'vue'
import {useRouter} from 'vue-router'
import {
  NLayout,
  NLayoutSider,
  NLayoutContent,
  NDrawer,
  NDrawerContent,
  NScrollbar
} from 'naive-ui'
import {Icon} from '@iconify/vue'
import MenuContent from '../components/MenuContent.vue'

const router = useRouter()
const drawerVisible = ref(false)
const collapsed = ref(false)

const handleMenuClick = (key: string) => {
  drawerVisible.value = false
  router.push(key)
}

// Handle window resize
const handleResize = () => {
  const width = window.innerWidth
  collapsed.value = width <= 768
}

// Add event listener when component is mounted
onMounted(() => {
  handleResize() // Check once on initialization
  window.addEventListener('resize', handleResize)
})

// Remove event listener before component is unmounted
onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.macos-layout {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
}

.macos-sidebar {
  border-right: 1px solid #e5e5e5;
}

.dark .macos-sidebar {
  border-right: 1px solid #3f3f46;
}

.macos-sidebar-header {
  background: linear-gradient(to bottom, #f9f9f9, #f5f5f5);
}

.dark .macos-sidebar-header {
  background: linear-gradient(to bottom, #2d2d2d, #27272a);
}

.macos-toolbar {
  backdrop-filter: blur(10px);
  background: rgba(255, 255, 255, 0.7);
  border: 1px solid rgba(0, 0, 0, 0.1);
}

.dark .macos-toolbar {
  background: rgba(38, 38, 38, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Scrollbar styling for WebKit browsers */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background-color: rgba(0, 0, 0, 0.2);
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background-color: rgba(0, 0, 0, 0.3);
}

.dark ::-webkit-scrollbar-thumb {
  background-color: rgba(255, 255, 255, 0.2);
}

.dark ::-webkit-scrollbar-thumb:hover {
  background-color: rgba(255, 255, 255, 0.3);
}
</style>
<template>
  <n-layout class="min-h-screen bg-gray-50">
    <n-layout has-sider position="absolute">
      <!-- 桌面端侧边栏 -->
      <n-layout-sider
          :collapsed-width="4"
          :width="320"
          :native-scrollbar="false"
          show-trigger="arrow-circle"
          :on-update-collapsed="(b) => collapsed = b"
          :collapsed="collapsed"
          content-style="padding: 24px;"
          class="bg-white/80 dark:bg-black/80 backdrop-blur-lg border-r border-gray-200 dark:border-gray-700"
      >
        <n-scrollbar class="h-screen">
          <div class="pb-20">
            <MenuContent @menu-item-click="handleMenuClick"/>
          </div>
        </n-scrollbar>
      </n-layout-sider>

      <!-- 主内容区域 -->
      <n-layout-content class="relative">
        <div class=" py-4 md:px-8 md:py-8 min-h-screen">
          <!-- 移动端菜单按钮 -->
          <transition name="fade">
            <button
                v-if="!drawerVisible"
                @click="drawerVisible = true"
                class="md:hidden fixed bottom-12 left-4 z-50 w-12 h-12 bg-white rounded-full shadow-lg flex items-center justify-center hover:shadow-xl transition-shadow duration-200"
            >
              <n-icon size="24" class="text-gray-700">
                <MenuOutline/>
              </n-icon>
            </button>
          </transition>

          <!-- 页面内容插槽 -->
          <router-view/>
        </div>
      </n-layout-content>
    </n-layout>

    <!-- 移动端抽屉菜单 -->
    <n-drawer
        v-model:show="drawerVisible"
        :width="280"
        placement="left"
    >
      <n-drawer-content
      >
        <div class="py-4">
          <MenuContent @menu-item-click="handleMenuClick"/>
        </div>
      </n-drawer-content>
    </n-drawer>

    <n-layout-footer class="w-full py-4 text-center text-gray-500 bg-white/80 dark:bg-neutral-900/80 dark:text-gray-400 transition-colors duration-300">
      Copyright © 2023 - 2024 XAUAT iOS Club<br>
      西安建筑科技大学 ｜ 陕ICP备2024031872号 ｜
      <a href="https://gitee.com/XAUATIOSClub" target="_blank" class="text-blue-600 dark:text-purple-400 underline">Gitee</a>
    </n-layout-footer>

  </n-layout>
</template>

<script setup>
import {ref, onMounted, onBeforeUnmount} from 'vue'
import {useRouter} from 'vue-router'
import {NLayout, NLayoutSider, NLayoutContent, NDrawer, NDrawerContent, NScrollbar, NIcon} from 'naive-ui'
import {MenuOutline} from '@vicons/ionicons5'
import MenuContent from '../components/MenuContent.vue'
import {RouterView} from 'vue-router'

const router = useRouter()
const drawerVisible = ref(false)
const collapsed = ref(false)

const handleMenuClick = (key) => {
  drawerVisible.value = false
  router.push(key)
}

// 处理窗口大小变化
const handleResize = () => {
  const width = window.innerWidth
  collapsed.value = width <= 768;
}

// 组件挂载时添加事件监听器
onMounted(() => {
  handleResize() // 初始化时检查一次
  window.addEventListener('resize', handleResize)
})

// 组件卸载前移除事件监听器
onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

.n-layout-footer {
  background: rgba(255,255,255,0.8);
  color: #6b7280;
  transition: background 0.3s, color 0.3s;
}
.dark .n-layout-footer {
  background: rgba(23,23,23,0.8);
  color: #a3a3a3;
}
</style>
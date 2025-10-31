<template>
  <div class="layout-container">
    <Sidebar v-if="showSidebar" />

    <div class="main-content" :class="{ 'with-sidebar': showSidebar }">
      <router-view />
    </div>
  </div>
  <n-layout-footer class="w-full py-4 text-center text-gray-500 bg-white/80 dark:bg-neutral-900/80 dark:text-gray-400 transition-colors duration-300">
    Copyright © 2023 - 2024 XAUAT iOS Club<br>
    西安建筑科技大学 ｜ 陕ICP备2024031872号 ｜
    <a href="https://gitee.com/XAUATIOSClub" target="_blank" class="text-blue-600 dark:text-purple-400 underline">Gitee</a>
  </n-layout-footer>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Sidebar from '../components/Sidebar.vue'

const route = useRoute()

const showSidebar = computed(() => {
  // 当路径以/Centre开头时显示侧边栏
  return route.path.startsWith('/Centre')
})
</script>

<style scoped>
.layout-container {
  display: flex;
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4edf9 100%);
}

.main-content {
  flex: 1;
  padding: 20px;
  transition: margin-left 0.3s ease;
}

@media (max-width: 768px) {
  .main-content.with-sidebar {
    margin-left: 0;
  }
}
</style>


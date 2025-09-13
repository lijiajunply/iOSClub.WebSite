<template>
  <div class="layout-container">
    <Sidebar v-if="showSidebar" />

    <div class="main-content" :class="{ 'with-sidebar': showSidebar }">
      <router-view />
    </div>
  </div>
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


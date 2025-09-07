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
  return route.path !== '/Centre'
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
}

.main-content.with-sidebar {
  margin-left: 250px;
}

@media (max-width: 768px) {
  .main-content.with-sidebar {
    margin-left: 200px;
  }
}
</style>

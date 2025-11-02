<template>
  <div class="layout-container">
    <Sidebar v-if="showSidebar" />

    <div class="main-content" :class="{ 'with-sidebar': showSidebar }">
      <router-view />
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRoute , useRouter} from 'vue-router'
import Sidebar from '../components/Sidebar.vue'
import { useAuthorizationStore } from '../stores/Authorization'

const route = useRoute()
const router = useRouter()
const store = useAuthorizationStore()

const showSidebar = computed(() => {
  // 当路径以/Centre开头时显示侧边栏
  return route.path.startsWith('/Centre')
})

onMounted(async () => {
  const isValid = await store.validate()
  if (!isValid) {
    store.logout()
    router.push('/login')
  }
})
</script>

<style scoped>
.layout-container {
  display: flex;
  min-height: 100vh;
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


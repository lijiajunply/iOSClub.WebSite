<!-- src/pages/AdminDashboard.vue -->
<template>
  <div class="p-4">
    <h2 class="text-2xl font-bold mb-4">管理员面板</h2>
    <n-button @click="logout" class="mb-4">退出登录</n-button>
    <n-table :columns="columns" :data="members" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthorizationStore } from '../stores/Authorization'

const members = ref([])
const columns = [
  { title: '姓名', key: 'userName' },
  { title: '学号', key: 'userId' },
  { title: '身份', key: 'identity' }
]

onMounted(async () => {
  try {
    const res = await fetch('https://localhost:7257/api/President/GetAllData', {
      headers: { Authorization: `Bearer ${useAuthorizationStore().Authorization}` }
    })
    const data = await res.json()
    members.value = JSON.parse(GZipServer.DecompressString(data))
  } catch (e) {
    console.error('获取成员失败', e)
  }
})

const logout = () => {
  useAuthorizationStore().logout()
  location.href = '/Login'
}
</script>
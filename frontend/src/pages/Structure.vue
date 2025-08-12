<template>
  <div class="p-4">
    <h2 class="text-2xl font-bold mb-4">社团结构</h2>
    <n-card v-for="dept in departments" :key="dept.name" class="mb-4">
      <h3>{{ dept.name }}</h3>
      <p>{{ dept.description }}</p>
    </n-card>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthorizationStore } from '../stores/Authorization'

const departments = ref([])
const authorization = useAuthorizationStore()

onMounted(async () => {
  try {
    const res = await fetch('https://localhost:7257/api/Department', {
      headers: { Authorization: `Bearer ${authorization.Authorization}` }
    })
    departments.value = await res.json()
  } catch (e) {
    console.error('获取部门失败', e)
  }
})
</script>
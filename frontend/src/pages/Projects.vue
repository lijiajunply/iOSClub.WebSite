<!-- src/pages/Projects.vue -->
<template>
  <div class="p-4">
    <h2 class="text-2xl font-bold mb-4">精品项目</h2>
    <div v-for="project in projects" :key="project.id" class="mb-4">
      <n-card>
        <h3>{{ project.title }}</h3>
        <p>{{ project.description }}</p>
        <p class="text-sm text-gray-500">
          {{ project.startTime }} - {{ project.endTime }}
        </p>
      </n-card>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthorizationStore } from '../stores/Authorization'

const projects = ref([])

onMounted(async () => {
  try {
    const res = await fetch('https://localhost:7257/api/Project/GetYourProjects', {
      headers: { Authorization: `Bearer ${useAuthorizationStore().Authorization}` }
    })
    projects.value = await res.json()
  } catch (e) {
    console.error('获取项目失败', e)
  }
})
</script>
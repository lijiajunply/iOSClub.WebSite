<!-- src/pages/Tools.vue -->
<template>
  <div class="p-4">
    <h2 class="text-2xl font-bold mb-4">精品资源</h2>
    <div v-for="resource in resources" :key="resource.id" class="mb-4">
      <n-card>
        <h3>{{ resource.name }}</h3>
        <p>{{ resource.description }}</p>
      </n-card>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthorizationStore } from '../stores/Authorization'

const resources = ref([])

onMounted(async () => {
  try {
    const res = await fetch('https://localhost:7257/api/Project/GetResources', {
      headers: { Authorization: `Bearer ${useAuthorizationStore().Authorization}` }
    })
    resources.value = await res.json()
  } catch (e) {
    console.error('获取资源失败', e)
  }
})
</script>
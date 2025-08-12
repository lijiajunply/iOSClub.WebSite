<!-- src/pages/Articles.vue -->
<template>
  <div class="p-4">
    <h2 class="text-2xl font-bold mb-4">社团动态</h2>
    <div v-for="article in articles" :key="article.path" class="mb-4">
      <n-card>
        <h3>{{ article.title }}</h3>
        <p>{{ article.content }}</p>
        <small class="text-gray-500">{{ article.lastWriteTime }}</small>
      </n-card>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthorizationStore } from '../stores/Authorization'

const articles = ref([])

onMounted(async () => {
  try {
    const res = await fetch('https://localhost:7257/api/Article/GetArticle', {
      headers: { Authorization: `Bearer ${useAuthorizationStore().Authorization}` }
    })
    articles.value = await res.json()
  } catch (e) {
    console.error('获取文章失败', e)
  }
})
</script>
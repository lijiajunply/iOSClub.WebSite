<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <MarkdownComponent :content="formattedArticle"/>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue'
import MarkdownComponent from "../../components/MarkdownComponent.vue";
import {type ArticleModel, ArticleService} from "../../services/ArticleService";

const roomArticle = ref<ArticleModel>({
  Path: "",
  Title: '',
  LastWriteTime: '',
  Watch: 0,
  Content: ''
})

const formattedArticle = computed(() => {
  return {
    title: roomArticle.value.Title,
    date: roomArticle.value.LastWriteTime,
    watch: roomArticle.value.Watch || 0,
    content: roomArticle.value.Content
  };
});

onMounted(async () => {
  roomArticle.value = await ArticleService.getArticle('Structure')
})
</script>
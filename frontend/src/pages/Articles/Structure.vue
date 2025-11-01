<template>
  <MarkdownComponent :content="formattedArticle"/>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue'
import MarkdownComponent from "../../components/MarkdownComponent.vue";
import {type ArticleModel, ArticleService} from "../../services/ArticleService";

const roomArticle = ref<ArticleModel>({
  path: "",
  title: '',
  lastWriteTime: '',
  watch: 0,
  content: ''
})

const formattedArticle = computed(() => {
  return {
    title: roomArticle.value.title,
    date: roomArticle.value.lastWriteTime,
    watch: roomArticle.value.watch || 0,
    content: roomArticle.value.content
  };
});

onMounted(async () => {
  roomArticle.value = await ArticleService.getArticle('Structure')
})
</script>
<template>
  <MarkdownComponent :content="roomArticle"/>
</template>

<script setup lang="ts">
import {ref, onMounted} from 'vue'
import MarkdownComponent from "../../components/MarkdownComponent.vue";
import {type ArticleProps, ArticleService} from "../../services/ArticleService.ts";

const roomArticle = ref<ArticleProps>({
  title: '',
  date: '',
  watch: 0,
  content: '',
})

onMounted(async () =>{
  const a = await ArticleService.getArticle('About')
  roomArticle.value = {
    title: a.title,
    date: a.lastWriteTime,
    watch: a.watch,
    content: a.content
  } as ArticleProps
})
</script>

<style scoped>

</style>
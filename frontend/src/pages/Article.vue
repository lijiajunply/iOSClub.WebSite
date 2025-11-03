<template>
  <MarkdownComponent :content="formattedArticle"/>
</template>

<script setup lang="ts">
import {ref, watch, computed} from 'vue'
import {useRoute} from 'vue-router'
import MarkdownComponent from "../components/MarkdownComponent.vue";
import {ArticleService} from "../services/ArticleService";
import {ArticleModel} from "../models";

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

const route = useRoute()

watch(
    () => route.params.id,
    async (newId) => {
      if (typeof newId !== 'string') return
      roomArticle.value = await ArticleService.getArticle(newId)
    },
    {immediate: true}
)
</script>

<style scoped>

</style>
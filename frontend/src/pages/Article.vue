<template>
  <MarkdownComponent :content="formattedArticle"/>
</template>

<script setup lang="ts">
import {ref, watch, computed} from 'vue'
import {useRoute} from 'vue-router'
import MarkdownComponent from "../components/MarkdownComponent.vue";
import {type ArticleModel, ArticleService} from "../services/ArticleService";

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
<template>
  <MarkdownComponent :content="roomArticle"/>
</template>

<script setup lang="ts">
import {ref, watch} from 'vue'
import {useRoute} from 'vue-router'
import MarkdownComponent from "../components/MarkdownComponent.vue";
import {type ArticleProps, ArticleService} from "../services/ArticleService.ts";

const roomArticle = ref<ArticleProps>({
  title: '',
  date: '',
  watch: 0,
  content: '',
})

const route = useRoute()

watch(
    () => route.params.id,
    async (newId) => {
      if (typeof newId !== 'string') return
      const a = await ArticleService.getArticle(newId)
      roomArticle.value = {
        title: a.title,
        date: a.lastWriteTime,
        watch: a.watch,
        content: a.content
      } as ArticleProps
    },
    {immediate: true}
)
</script>

<style scoped>

</style>
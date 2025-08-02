<script setup lang="ts">
import { ref, onMounted, watch } from "vue";

const props = defineProps({
  content: String  // 修正属性名，与模板中使用的 :content 匹配
})

const s = ref('')

// 延迟加载 markdown-it 相关库，确保只在客户端执行
let md = null;

const initMarkdown = async () => {
  // 确保只在浏览器环境中执行
  if (typeof window !== 'undefined') {
    try {
      const markdownit = (await import('markdown-it')).default;
      const markdownAnchor = (await import('markdown-it-anchor')).default;
      const markdownitFootnote = (await import('markdown-it-footnote')).default;
      const markdownitTaskLists = (await import('markdown-it-task-lists')).default;
      const markdownitAttrs = (await import('markdown-it-attrs')).default;
      const mdExpandTabs = (await import('markdown-it-expand-tabs')).default;
      const mdSup = (await import('markdown-it-sup')).default;
      const mdSub = (await import('markdown-it-sub')).default;
      const mdMark = (await import('markdown-it-mark')).default;

      md = new markdownit({
        html: true,
        breaks: true,
        linkify: true,
        typography: true,
      })

      md.use(markdownAnchor)
      md.use(markdownitFootnote)
      md.use(markdownitTaskLists)
      md.use(markdownitAttrs)
      md.use(mdExpandTabs)
      md.use(mdSup)
      md.use(mdSub)
      md.use(mdMark)

      // 只在需要时加载 mermaid
      // try {
      //   const markdownItMermaid = (await import('markdown-it-mermaid')).default
      //   md.use(markdownItMermaid)
      // } catch (e) {
      //   console.warn('Mermaid plugin not available:', e)
      // }
    } catch (error) {
      console.error('Failed to initialize markdown-it:', error)
      // 降级处理：只使用基本的 markdown 渲染
      const markdownit = (await import('markdown-it')).default;
      md = new markdownit({
        html: true,
        breaks: true,
        linkify: true,
      })
    }
  }
}

const markdownToHTML = (markdown) => {
  if (!md) return ''
  try {
    return md.render(markdown || '')
  } catch (error) {
    console.error('Error rendering markdown:', error)
    return markdown || ''
  }
}

// 修复属性名，使用 content 而不是 context
watch(() => props.content, (newValue) => {
  if (md) {
    s.value = markdownToHTML(newValue)
  }
})

onMounted(async () => {
  await initMarkdown()
  s.value = markdownToHTML(props.content)
})
</script>

<template>
  <div class="prose">
    <div v-html="s"/>
  </div>
</template>

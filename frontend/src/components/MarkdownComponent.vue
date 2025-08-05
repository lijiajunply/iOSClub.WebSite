<script setup lang="ts">
import {nextTick, onMounted, ref, watch, computed} from "vue";
import {NAnchor, NAnchorLink, NIcon} from 'naive-ui'
// import {ArticleService, type ArticleModel} from "../services/ArticleService.js";
import {CalendarOutlined, EyeFilled} from "@vicons/antd";

import MarkdownIt from 'markdown-it'
import markdownitFootnote from 'markdown-it-footnote'
import markdownitTaskList from 'markdown-it-task-lists'
import markdownitAttrs from 'markdown-it-attrs'
import mdExpandTabs from 'markdown-it-expand-tabs'
import mdSup from 'markdown-it-sup'
import mdSub from 'markdown-it-sub'
import mdMark from 'markdown-it-mark'
import markdownItAnchor from 'markdown-it-anchor'
import markdownItContainer from 'markdown-it-container'

import mermaid from 'mermaid'
import Prism from "prismjs"
import "prismjs/themes/prism-tomorrow.min.css"

interface ArticleProps {
  title: string;
  date: string;
  watch: number;
  content: string;
}

const props = withDefaults(defineProps<ArticleProps>(), {
  watch: 0, // 默认值
  // 其他字段的默认值...
});

// 存储标题结构
const headings = ref([])

// 初始化 mermaid
mermaid.initialize({startOnLoad: true})

// 创建 markdown-it 实例
const md = new MarkdownIt({
  html: true,
  linkify: true,
  typographer: true
})

// 使用 anchor 插件
md.use(markdownItAnchor, {
  permalink: true,
  permalinkBefore: true,
  permalinkSymbol: '#',
  permalinkClass: 'apple-link-no-icon',
})
md.use(markdownitFootnote)
md.use(markdownitTaskList, {label: false, labelAfter: false})
md.use(markdownitAttrs, {
  allowedAttributes: ['id', 'class', 'target', 'src'] 
})

md.use(mdExpandTabs)
    .use(mdSup)
    .use(mdSub)
    .use(mdMark)

// 配置自定义容器
const containerOptions = [
  {
    name: 'warning',
    className: 'warning'
  },
  {
    name: 'danger',
    className: 'danger'
  },
  {
    name: 'tip',
    className: 'tip'
  }
]

containerOptions.forEach(({name, className}) => {
  md.use(markdownItContainer, name, {
    validate: (params) => {
      return params.trim().match(new RegExp(`^${name}\\s+(.*)$`))
    },
    render: (tokens, idx) => {
      const m = tokens[idx].info.trim().match(new RegExp(`^${name}\\s+(.*)$`))
      if (tokens[idx].nesting === 1) {
        return `<div class="${className} custom-block"><p style="font-weight: bold">${md.utils.escapeHtml(m[1])}</p>\n`
      } else {
        return '</div>\n'
      }
    }
  })
})

// 解析标题生成导航数据
const extractHeadings = (markdown) => {
  const tokens = md.parse(markdown, {})
  const extractedHeadings = []

  for (let i = 0; i < tokens.length; i++) {
    const token = tokens[i]

    if (token.type === 'heading_open') {
      const level = parseInt(token.tag.slice(1)) // h1 -> 1, h2 -> 2
      const nextToken = tokens[i + 1]

      if (nextToken && nextToken.type === 'inline') {
        const text = nextToken.content
        // 生成 ID（与 markdown-it-anchor 保持一致）
        const id = text.toLowerCase()
            .replace(/[^\w\u4e00-\u9fa5]+/g, '-')
            .replace(/^-+|-+$/g, '')

        extractedHeadings.push({
          id,
          text,
          level,
          href: `#${id}`
        })
      }
    }
  }

  return extractedHeadings
}

// 构建层级结构的标题树
const buildHeadingTree = (flatHeadings) => {
  const tree = []
  const stack = []

  flatHeadings.forEach(heading => {
    const item = {...heading, children: []}

    // 找到合适的父级
    while (stack.length > 0 && stack[stack.length - 1].level >= heading.level) {
      stack.pop()
    }

    if (stack.length === 0) {
      tree.push(item)
    } else {
      stack[stack.length - 1].children.push(item)
    }

    stack.push(item)
  })

  return tree
}

// 渲染 markdown 的函数
const render = async (markdown) => {
  // 提取标题
  const extractedHeadings = extractHeadings(markdown)
  headings.value = buildHeadingTree(extractedHeadings)

  const html = md.render(markdown)

  // 等待 DOM 更新
  await nextTick()

  // 初始化 mermaid 图表
  await mermaid.run({
    nodes: document.querySelectorAll('.language-mermaid'),
  })

  // 代码高亮
  setTimeout(() => {
    Prism.highlightAll()
  }, 50)

  return html
}

// 使用 computed 代替 ref + watch
const html = ref('')

watch(() => props.content, async (newValue) => {
      if (!md || !newValue) return '';

      try {
        html.value = await render(newValue);
      } catch (error) {
        console.error('Error rendering markdown:', error);
        html.value = newValue;
      }
    }
)

watch(
    () => props.content,
    async (newArticle) => {
      if (!newArticle || !newArticle.content) return;
      try {
        html.value = await render(newArticle.content);
      } catch (error) {
        console.error('渲染 markdown 失败:', error);
        html.value = newArticle.content;
      }
    },
    { immediate: true } // 初始化时立即执行
);

// 递归渲染导航链接
const renderAnchorLinks = (items, depth = 0) => {
  return items.map(item => ({
    title: item.text,
    href: item.href,
    children: item.children?.length > 0 ? renderAnchorLinks(item.children, depth + 1) : undefined
  }))
}

// 计算导航数据
const anchorLinks = computed(() => renderAnchorLinks(headings.value))

</script>

<template>
  <div class="min-h-screen">
    <div v-if="content" class="flex">
      <!-- 文章头部 -->
      <div class="p-8 mb-6">
        <div class="mb-6">
          <div class="text-xl lg:text-4xl font-bold text-gray-900 leading-tight mb-4">
            {{ content.title }}
          </div>
          <div class="flex items-center space-x-6 text-gray-500 text-sm">
                  <span class="flex items-center">
                    <n-icon size="16" class="mr-2">
                      <CalendarOutlined/>
                    </n-icon>
                    {{ content.date }}
                  </span>
            <span class="flex items-center">
                    <n-icon size="16" class="mr-2">
                      <EyeFilled/>
                    </n-icon>
                    {{ content.watch }} 次阅读
                  </span>
          </div>
        </div>

        <div class="prose max-w-none">
          <div>
            <!-- 主内容区域 -->
            <div class="prose max-w-none pr-8">
              <div v-html="html"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- 导航栏 -->
      <div class="sticky top-8 h-fit" v-if="headings.length > 0">
        <div class="pl-4">
          <h3 class="text-sm font-semibold mb-4">
            目录
          </h3>
          <n-anchor
              :show-rail="false"
              :show-background="false"
              :bound="100"
              class="toc-anchor"
          >
            <div v-for="link in anchorLinks" :key="link.href">
              <n-anchor-link
                  :title="link.title"
                  :href="link.href"
              >
                <div v-if="link.children?.length > 0">
                  <n-anchor-link
                      v-for="subLink in link.children"
                      :key="subLink.href"
                      :title="subLink.title"
                      :href="subLink.href"
                  />
                </div>
              </n-anchor-link>
            </div>
          </n-anchor>
        </div>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-else class="flex items-center justify-center h-full">
      <div class="text-center">
        <n-icon size="64" class="text-gray-300 mb-4">
          <svg viewBox="0 0 24 24">
            <path fill="currentColor"
                  d="M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.89 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm4 18H6V4h7v5h5v11z"/>
          </svg>
        </n-icon>
        <p class="text-gray-500 text-lg">请选择一篇文章阅读</p>
      </div>
    </div>
  </div>

</template>

<style scoped>
@reference 'tailwindcss';

/* 导航栏样式 */
:deep(.toc-anchor) {
  @apply text-sm;
}

:deep(.toc-anchor .n-anchor-link__title) {
  @apply text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100;
  @apply py-1 transition-colors duration-200;
}

:deep(.toc-anchor .n-anchor-link--active .n-anchor-link__title) {
  @apply text-blue-600 dark:text-blue-400 font-medium;
}

/* 响应式设计 */
@media (max-width: 1280px) {
  .w-64 {
    display: none;
  }

  .flex-1 {
    @apply pr-0;
  }
}

/* 可以添加额外的样式 */
.prose :deep(img) {
  max-width: 100%;
  height: auto;
  border-radius: 0.5rem;
}

.prose :deep(code) {
  background-color: #f3f4f6;
  padding: 0.2em 0.4em;
  border-radius: 0.25rem;
  font-size: 0.875em;
}

.prose :deep(pre) {
  background-color: #1f2937;
  color: #f9fafb;
  padding: 1em;
  border-radius: 0.5rem;
  overflow-x: auto;
}

.prose :deep(pre code) {
  background-color: transparent;
  color: inherit;
  padding: 0;
}

.prose :deep(blockquote) {
  border-left: 4px solid #d1d5db;
  padding-left: 1rem;
  margin: 1rem 0;
  color: #6b7280;
}
</style>
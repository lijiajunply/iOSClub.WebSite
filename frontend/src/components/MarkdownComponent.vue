<script setup lang="ts">
import {nextTick, ref, watch, computed, onMounted} from "vue";
import {Icon} from "@iconify/vue";
import * as echarts from 'echarts'

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

import markdownItMermaid from '@jsonlee_12138/markdown-it-mermaid';
import Prism from "prismjs"

interface Content {
  title: string
  date: string
  watch: number
  content: string
}

const props = defineProps<{
  content?: Content
}>()

// 存储标题结构
const headings = ref<Array<{ id: string; text: string; level: number; href: string; children: any[] }>>([])

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
    .use(markdownItMermaid({delay: 100}))

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
    validate: (params: string) => {
      return params.trim().match(new RegExp(`^${name}\\s+(.*)$`))
    },
    render: (tokens: any[], idx: number) => {
      const m = tokens[idx].info.trim().match(new RegExp(`^${name}\\s+(.*)$`))
      if (tokens[idx].nesting === 1) {
        return `<div class="${className} custom-block"><p class="custom-block-title">${md.utils.escapeHtml(m[1])}</p>\n`
      } else {
        return '</div>\n'
      }
    }
  })
})

// 解析标题生成导航数据
const extractHeadings = (markdown: string) => {
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
          href: `#${id}`,
          children: []
        })
      }
    }
  }

  return extractedHeadings
}

// 构建层级结构的标题树
const buildHeadingTree = (flatHeadings: Array<{ id: string; text: string; level: number; href: string }>) => {
  const tree: Array<{ id: string; text: string; level: number; href: string; children: any[] }> = []
  const stack: Array<{ id: string; text: string; level: number; href: string; children: any[] }> = []

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
const render = async (markdown: string) => {
  // 提取标题
  const extractedHeadings = extractHeadings(markdown)
  headings.value = buildHeadingTree(extractedHeadings)

  const html = md.render(markdown)

  // 等待 DOM 更新
  await nextTick()

  // 初始化图表
  setTimeout(() => {
    // 初始化 ECharts 图表
    const chartElements = document.querySelectorAll('.echarts-chart')
    chartElements.forEach(element => {
          const chartConfig = element.getAttribute('data-config')
          if (chartConfig) {
            try {
              const option = JSON.parse(chartConfig)
              const chart = echarts.init(element as HTMLElement)
              chart.setOption(option)

              // 响应式调整
              const resizeObserver = new ResizeObserver(() => {
                chart.resize()
              })
              resizeObserver.observe(element)
            } catch (e) {
              console.error('ECharts配置解析失败:', e)
            }
          }
        }
    )

    // 代码高亮
    Prism.highlightAll()
  }, 50)

  return html
}

// 使用 computed 代替 ref + watch
const html = ref('')

watch(() => props.content, async (newValue) => {
      if (!md || !newValue) return '';
      html.value = await render(newValue.content);
    }, {immediate: true}
)

// 递归渲染导航链接
const renderAnchorLinks = (items: Array<{
  id: string;
  text: string;
  level: number;
  href: string;
  children: any[]
}>, depth = 0): Array<{ title: string; href: string; children?: Array<{ title: string; href: string }> }> => {
  return items.map((item: any) => ({
    title: item.text,
    href: item.href,
    children: item.children?.length > 0 ? renderAnchorLinks(item.children, depth + 1) : undefined
  }))
}

// 计算导航数据
const anchorLinks = computed(() => renderAnchorLinks(headings.value))
const date = computed(() => props.content?.date ? new Date(props.content.date).toLocaleDateString('zh-CN') : '')

onMounted(() => {
  // 添加滚动监听器以高亮当前活动的锚点
  const handleScroll = () => {
    const headings = document.querySelectorAll('h1, h2, h3, h4, h5, h6')
    let activeHeadingId = ''

    for (let i = headings.length - 1; i >= 0; i--) {
      const heading = headings[i]
      const rect = heading.getBoundingClientRect()
      if (rect.top <= 100) {
        activeHeadingId = heading.id
        break
      }
    }

    // 移除所有活动类
    document.querySelectorAll('.toc-link').forEach(link => {
      link.classList.remove('active')
    })

    // 为当前活动标题添加活动类
    if (activeHeadingId) {
      const activeLink = document.querySelector(`.toc-link[href="#${activeHeadingId}"]`)
      if (activeLink) {
        activeLink.classList.add('active')
      }
    }
  }

  window.addEventListener('scroll', handleScroll)
})
</script>

<template>
  <div v-if="content" class="flex flex-col md:flex-row">
    <!-- 文章内容区 -->
    <div class="w-full md:w-4/5 p-4 md:p-8">
      <article class="prose prose-gray max-w-none dark:prose-invert">
        <!-- 文章头部 -->
        <header class="mb-8 border-b border-gray-200 pb-6 dark:border-gray-700">
          <h1 class="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {{ content.title }}
          </h1>
          <div class="flex items-center gap-4 text-gray-500 dark:text-gray-400 text-sm">
            <time class="flex items-center gap-1">
              <Icon icon="mdi:calendar" width="16" height="16"/>
              {{ date }}
            </time>
            <span class="flex items-center gap-1">
              <Icon icon="mdi:eye" width="16" height="16"/>
              {{ content.watch }} 次阅读
            </span>
          </div>
        </header>

        <!-- 文章内容 -->
        <div class="markdown-content">
          <div v-html="html"></div>
        </div>
      </article>
    </div>

    <!-- 目录导航 -->
    <div v-if="headings.length > 0" class="hidden md:block w-1/5 sticky top-8 h-fit self-start p-4">
      <nav class="toc-nav bg-gray-50 dark:bg-gray-800 rounded-xl p-4">
        <h3 class="text-sm font-semibold mb-3 text-gray-900 dark:text-white">
          目录
        </h3>
        <ul class="space-y-1">
          <li
              v-for="link in anchorLinks"
              :key="link.href"
              class="toc-item"
          >
            <a
                :href="link.href"
                class="toc-link block py-1 text-sm text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white transition-colors"
                :class="{ 'active font-medium text-blue-600 dark:text-blue-400': false }"
            >
              {{ link.title }}
            </a>

            <!-- 子目录 -->
            <ul v-if="link.children && link.children.length > 0" class="ml-3 mt-1 space-y-1">
              <li
                  v-for="subLink in link.children"
                  :key="subLink.href"
              >
                <a
                    :href="subLink.href"
                    class="toc-link block py-1 text-xs text-gray-500 hover:text-gray-900 dark:text-gray-500 dark:hover:text-white transition-colors"
                    :class="{ 'active font-medium text-blue-600 dark:text-blue-400': false }"
                >
                  {{ subLink.title }}
                </a>
              </li>
            </ul>
          </li>
        </ul>
      </nav>
    </div>
  </div>

  <!-- 空状态 -->
  <div v-else class="flex flex-col items-center justify-center h-full p-8 text-center">
    <div
        class="bg-gray-200 dark:bg-gray-700 border-2 border-dashed rounded-xl w-16 h-16 flex items-center justify-center mb-4">
      <Icon icon="mdi:file-document-outline" width="32" height="32" class="text-gray-500 dark:text-gray-400"/>
    </div>
    <p class="text-gray-500 dark:text-gray-400 text-lg">
      请选择一篇文章阅读
    </p>
  </div>
</template>

<style scoped>
@reference 'tailwindcss';
@import "prismjs/themes/prism-tomorrow.min.css";

/* 自定义块样式 */
.custom-block {
  @apply rounded-lg p-4 my-4 border;
}

.custom-block-title {
  @apply font-bold text-base mb-2;
}

.warning {
  @apply bg-amber-50 border-amber-500 dark:bg-amber-900;
}

.danger {
  @apply bg-red-50 border-red-500 dark:bg-red-900;
}

.tip {
  @apply bg-blue-50 border-blue-500 dark:bg-blue-900;
}

/* 目录链接活动状态 */
.toc-link.active {
  @apply font-medium text-blue-600 dark:text-blue-400;
}

/* Markdown 内容样式 */
.markdown-content :deep(h1),
.markdown-content :deep(h2),
.markdown-content :deep(h3),
.markdown-content :deep(h4),
.markdown-content :deep(h5),
.markdown-content :deep(h6) {
  @apply font-semibold text-gray-900 dark:text-white;
}

.markdown-content :deep(h1) {
  @apply text-3xl mt-8 mb-4;
}

.markdown-content :deep(h2) {
  @apply text-2xl mt-6 mb-3;
}

.markdown-content :deep(h3) {
  @apply text-xl mt-4 mb-2;
}

.markdown-content :deep(p) {
  @apply text-gray-700 dark:text-gray-300 mb-4 leading-relaxed;
}

.markdown-content :deep(a) {
  @apply text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 underline;
}

.markdown-content :deep(strong) {
  @apply font-semibold;
}

.markdown-content :deep(em) {
  @apply italic;
}

.markdown-content :deep(ul),
.markdown-content :deep(ol) {
  @apply pl-6 mb-4;
}

.markdown-content :deep(li) {
  @apply mb-1;
}

.markdown-content :deep(ul li) {
  @apply list-disc;
}

.markdown-content :deep(ol li) {
  @apply list-decimal;
}

.markdown-content :deep(blockquote) {
  @apply border-l-4 border-gray-300 dark:border-gray-600 pl-4 ml-2 py-1 my-4 text-gray-600 dark:text-gray-400;
}

.markdown-content :deep(code) {
  @apply bg-gray-100 dark:bg-gray-800 px-1.5 py-0.5 rounded text-sm font-mono;
}

.markdown-content :deep(pre) {
  @apply bg-gray-800 dark:bg-gray-900 rounded-lg p-4 my-4 overflow-x-auto;
}

.markdown-content :deep(pre code) {
  @apply bg-transparent p-0 rounded-none;
}

.markdown-content :deep(table) {
  @apply min-w-full border-collapse my-4;
}

.markdown-content :deep(th),
.markdown-content :deep(td) {
  @apply border border-gray-300 dark:border-gray-700 px-4 py-2;
}

.markdown-content :deep(th) {
  @apply bg-gray-100 dark:bg-gray-800 font-semibold;
}

.markdown-content :deep(img) {
  @apply rounded-lg mx-auto my-4;
}

.markdown-content :deep(hr) {
  @apply border-gray-300 dark:border-gray-700 my-8;
}
</style>
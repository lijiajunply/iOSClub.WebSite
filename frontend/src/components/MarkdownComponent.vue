<script setup lang="ts">
import {nextTick, ref, watch, computed, onMounted, withDefaults} from "vue";
import {Icon} from "@iconify/vue";

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
import {useRoute} from "vue-router";

interface Content {
  title: string
  date: string
  content: string,
  identity?: string
}

const props = withDefaults(defineProps<{
  content?: Content
  showNav?: boolean
}>(), {
  showNav: true
})

const route = useRoute();

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
  permalink: markdownItAnchor.permalink.ariaHidden({
    placement: 'before',
    space: true,
    class: 'apple-link-no-icon',
    renderHref: (href: string) => `${route.path}#${href}`,
  })
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

const formattedDate = computed(() => {
  if (!props.content?.date) return '';
  return new Date(props.content.date).toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
});

const identityInfo = computed(() => {
  const map: Record<string, { label: string; colorClass: string }> = {
    'Department': {label: '部员', colorClass: 'tag-cyan'},
    'Minister': {label: '部长', colorClass: 'tag-orange'},
    'President': {label: '社长', colorClass: 'tag-red'},
    'Founder': {label: '创始人', colorClass: 'tag-purple'},
  };
  // 默认 fallback
  return map[props.content?.identity || ''] || null;
});

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

// 添加处理锚点点击的方法
const handleAnchorClick = (event: Event, href: string) => {
  event.preventDefault();
  const targetId = href.substring(1).toLowerCase(); // 移除 # 前缀

  // 对 targetId 进行 URL 编码
  const encodedTargetId = encodeURIComponent(targetId);

  console.log(targetId)

  // 如果仍然找不到，尝试通过属性选择器查找
  const targetElement = document.querySelector(`[id="${encodedTargetId}"]`);

  if (targetElement) {
    // 平滑滚动到目标元素
    targetElement.scrollIntoView({behavior: 'smooth'});
  }
};

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
    <div class="w-full lg:pr-8" :class="[showNav && headings.length > 0 ? 'lg:w-4/5' : 'lg:w-full']">
      <article class="prose prose-gray max-w-none dark:prose-invert">
        <!-- 文章头部 -->
        <header class="backdrop-blur-sm">
          <!-- 身份与日期行 -->
          <h1 class="mb-2 text-3xl font-bold text-gray-900 dark:text-gray-100 tracking-tight">
            {{ content.title }}
          </h1>
          <div
              class="flex items-center gap-4 text-sm text-gray-400 mb-8 border-b border-gray-100 dark:border-gray-800 pb-4">
                     <span class="flex items-center gap-1">
                        <Icon icon="solar:calendar-date-bold"/>
                        {{ formattedDate }}
                     </span>
            <span v-if="identityInfo"
                  class="px-2.5 py-0.5 rounded-full text-xs border transition-colors"
                  :class="identityInfo.colorClass">
                  {{ identityInfo.label }}
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
    <div v-if="showNav && headings.length > 0" class="hidden lg:block w-1/5 sticky top-8 h-fit self-start p-4">
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
            <div
                @click="handleAnchorClick($event, link.href)"
                class="toc-link block py-1 text-sm link"
            >
              {{ link.title }}
            </div>

            <!-- 子目录 -->
            <ul v-if="link.children && link.children.length > 0" class="ml-3 mt-1 space-y-1">
              <li
                  v-for="subLink in link.children"
                  :key="subLink.href"
              >
                <div
                    @click="handleAnchorClick($event, subLink.href)"
                    class="toc-link block py-1 text-xs text-blue-500"
                >
                  {{ subLink.title }}
                </div>
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

.tag-blue {
  background-color: #E1F3FF;
  color: #007AFF;
  border-color: rgba(0, 122, 255, 0.2);
}

.tag-cyan {
  background-color: #E0F8F2;
  color: #00A3FF;
  border-color: rgba(0, 163, 255, 0.2);
}

.tag-orange {
  background-color: #FFF4E5;
  color: #FF9500;
  border-color: rgba(255, 149, 0, 0.2);
}

.tag-red {
  background-color: #FFEAE9;
  color: #FF3B30;
  border-color: rgba(255, 59, 48, 0.2);
}

.tag-purple {
  background-color: #F3E8FF;
  color: #AF52DE;
  border-color: rgba(175, 82, 222, 0.2);
}

/* Dark Mode Tags */
.dark .tag-blue {
  background-color: rgba(0, 122, 255, 0.15);
  color: #64D2FF;
  border-color: rgba(100, 210, 255, 0.2);
}

.dark .tag-cyan {
  background-color: rgba(100, 210, 255, 0.15);
  color: #70D7FF;
  border-color: rgba(112, 215, 255, 0.2);
}

.dark .tag-orange {
  background-color: rgba(255, 149, 0, 0.15);
  color: #FFD60A;
  border-color: rgba(255, 214, 10, 0.2);
}

.dark .tag-red {
  background-color: rgba(255, 69, 58, 0.15);
  color: #FF9F0A;
  border-color: rgba(255, 159, 10, 0.2);
}

.dark .tag-purple {
  background-color: rgba(191, 90, 242, 0.15);
  color: #DA8FFF;
  border-color: rgba(218, 143, 255, 0.2);
}

/* 自定义块样式 */
:deep(.custom-block) {
  @apply rounded-lg p-4 my-4;
}

:deep(.custom-block-title) {
  @apply font-bold text-base mb-2;
}

.markdown-content :deep(.warning) {
  @apply bg-amber-100 ;
}

.dark .markdown-content :deep(.warning) {
  @apply bg-amber-900;
}

.markdown-content :deep(.danger) {
  @apply bg-red-100 ;
}

.dark .markdown-content :deep( .danger) {
  @apply bg-red-900;
}

.markdown-content :deep(.tip) {
  @apply bg-blue-50;
}

.dark .markdown-content :deep(.tip) {
  @apply bg-blue-900;
}

.toc-link {
  @apply text-sky-700 cursor-pointer;
}

.toc-link:hover {
  @apply text-cyan-800;
}

.dark .toc-link {
  @apply text-sky-300;
}

.dark .toc-link:hover {
  @apply text-cyan-400;
}

/* Markdown 内容样式 */
.markdown-content :deep(h1),
.markdown-content :deep(h2),
.markdown-content :deep(h3),
.markdown-content :deep(h4),
.markdown-content :deep(h5),
.markdown-content :deep(h6) {
  @apply font-semibold;
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
  @apply mb-4 leading-relaxed;
}

.markdown-content :deep(h1),
.markdown-content :deep(h2),
.markdown-content :deep(h3),
.markdown-content :deep(h4),
.markdown-content :deep(h5),
.markdown-content :deep(h6),
.markdown-content :deep(p) {
  @apply text-gray-900;
}

.dark .markdown-content :deep(h1),
.dark .markdown-content :deep(h2),
.dark .markdown-content :deep(h3),
.dark .markdown-content :deep(h4),
.dark .markdown-content :deep(h5),
.dark .markdown-content :deep(h6),
.dark .markdown-content :deep(p) {
  @apply text-white;
}

.markdown-content :deep(a) {
  @apply underline;
}

.markdown-content :deep(a) {
  @apply text-blue-600 hover:text-blue-800;
}

.dark .markdown-content :deep(a) {
  @apply text-blue-400 hover:text-blue-300;
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
  @apply border-l-4 pl-4 ml-2 py-1 my-4;
}

.markdown-content :deep(blockquote) {
  @apply border-gray-300 text-gray-600;
}

.dark .markdown-content :deep(blockquote) {
  @apply border-gray-600 text-gray-400;
}

.markdown-content :deep(code) {
  @apply px-1.5 py-0.5 rounded text-sm font-mono;
}

.markdown-content :deep(code) {
  @apply bg-gray-100;
}

.dark .markdown-content :deep(code) {
  @apply bg-gray-800;
}

.markdown-content :deep(pre) {
  @apply rounded-lg p-4 my-4 overflow-x-auto;
}

.markdown-content :deep(pre) {
  @apply bg-gray-800;
}

.dark .markdown-content :deep(pre) {
  @apply bg-gray-900;
}

.markdown-content :deep(pre code) {
  @apply bg-transparent p-0 rounded-none;
}

.markdown-content :deep(table) {
  @apply min-w-full border-collapse my-4;
}

.markdown-content :deep(th),
.markdown-content :deep(td) {
  @apply px-4 py-2;
}

.markdown-content :deep(th),
.markdown-content :deep(td) {
  @apply border border-gray-300;
}

.dark .markdown-content :deep(th),
.dark .markdown-content :deep(td) {
  @apply border border-gray-700;
}

.markdown-content :deep(th) {
  @apply font-semibold;
}

.markdown-content :deep(th) {
  @apply bg-gray-100;
}

.dark .markdown-content :deep(th) {
  @apply bg-gray-800;
}

.markdown-content :deep(img) {
  @apply rounded-lg mx-auto my-4;
}

.markdown-content :deep(hr) {
  @apply my-8;
}

.markdown-content :deep(hr) {
  @apply border-gray-300;
}

.dark .markdown-content :deep(hr) {
  @apply border-gray-700;
}
</style>
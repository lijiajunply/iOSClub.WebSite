<script setup lang="ts">
import { nextTick, ref, watch, computed, onBeforeUnmount } from "vue";
import { useRoute } from "vue-router";
import { Icon } from "@iconify/vue";

// Markdown Libraries
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
const activeHeadingId = ref<string>('');

// --- Markdown Configuration ---
const md = new MarkdownIt({
  html: true,
  linkify: true,
  typographer: true
})

// Anchor Config: Add scroll-margin-top class to handle sticky headers
md.use(markdownItAnchor, {
  permalink: markdownItAnchor.permalink.linkInsideHeader({
    symbol: '', // No symbol, make the whole header clickable or just invisible
    placement: 'before',
    class: 'absolute -ml-6 opacity-0 hover:opacity-100 text-blue-500 w-6 h-full flex items-center justify-center',
    renderHref: (href: string) => `${route.path}#${href}`,
  }),
  // 重要：slugify 函数确保中文标题也能生成合法的 ID
  slugify: (s: string) => s.trim().toLowerCase().replace(/\s+/g, '-').replace(/[^\w\u4e00-\u9fa5-]/g, '')
})

// ... Other Plugins (Footnote, tasklist, etc - kept same)
md.use(markdownitFootnote)
md.use(markdownitTaskList, { label: false, labelAfter: false })
md.use(markdownitAttrs)
md.use(mdExpandTabs).use(mdSup).use(mdSub).use(mdMark)
// Mermaid might need CSS adjustment in dark mode contexts, relying on plugin defaults for now
md.use(markdownItMermaid({ delay: 200 }))

// Custom Containers (Alerts) - MacOS Style
const containerOptions = [
  { name: 'warning', icon: 'lucide:alert-triangle', color: 'text-amber-500', bg: 'bg-amber-50 dark:bg-amber-500/10', border: 'border-amber-200 dark:border-amber-500/20' },
  { name: 'danger', icon: 'lucide:x-circle', color: 'text-red-500', bg: 'bg-red-50 dark:bg-red-500/10', border: 'border-red-200 dark:border-red-500/20' },
  { name: 'tip', icon: 'lucide:lightbulb', color: 'text-blue-500', bg: 'bg-blue-50 dark:bg-blue-500/10', border: 'border-blue-200 dark:border-blue-500/20' }
]

containerOptions.forEach(opt => {
  md.use(markdownItContainer, opt.name, {
    validate: (params: string) => params.trim().match(new RegExp(`^${opt.name}\\s+(.*)$`)),
    render: (tokens: any[], idx: number) => {
      const m = tokens[idx].info.trim().match(new RegExp(`^${opt.name}\\s+(.*)$`))
      if (tokens[idx].nesting === 1) {
        // Use Tailwind utility classes directly in the rendered HTML
        return `<div class="${opt.bg} ${opt.border} border rounded-xl p-4 my-6 flex gap-3 shadow-sm">
                  <div class="min-w-0 flex-1">
                    <p class="font-semibold text-sm ${opt.color} mb-1 opacity-90">${md.utils.escapeHtml(m[1] || opt.name.toUpperCase())}</p>
                    <div class="text-sm opacity-90 leading-relaxed">`
      } else {
        return '</div></div></div>\n'
      }
    }
  })
})

// --- Logic: Headings Extraction ---
// Keep headings reactive
const headings = ref<Array<{ id: string; text: string; level: number; children: any[] }>>([])

const extractHeadings = (markdown: string) => {
  // Use a temporary renderer to parse without applying side effects if needed,
  // currently parsing same doc is fine.
  const tokens = md.parse(markdown, {})
  const result = []

  // Custom slugify logic to match markdown-it-anchor
  const slugify = (s: string) => s.trim().toLowerCase().replace(/\s+/g, '-').replace(/[^\w\u4e00-\u9fa5-]/g, '')

  for (let i = 0; i < tokens.length; i++) {
    if (tokens[i].type === 'heading_open') {
      const level = parseInt(tokens[i].tag.slice(1))
      // Support h2 and h3 for TOC (h1 is usually title)
      if (level > 3 || level < 2) continue;

      const inline = tokens[i + 1]
      if (inline && inline.type === 'inline') {
        const text = inline.content
        const id = slugify(text)
        result.push({ id, text, level, children: [] })
      }
    }
  }
  return result
}

// Flat list for TOC (Since we just want indentation visually)
// Alternatively, build a tree if we want collapsible
const flatHeadings = computed(() => headings.value)

// --- Rendering ---
const html = ref('')

watch(() => props.content, async (newVal) => {
  if (!newVal) {
    html.value = ''
    return
  }

  // Extract Headings first
  headings.value = extractHeadings(newVal.content)

  // Render HTML
  let rendered = md.render(newVal.content)

  // Post-process: Add scroll-mt to headings for sticky header offset
  // Using a regex replacement for simplicity, though manipulating tokens is cleaner
  // This ensures that when we click a hash link, it doesn't go under the header
  rendered = rendered.replace(/<(h[1-6])/g, '<$1 class="scroll-mt-20 group relative"')

  html.value = rendered

  await nextTick()
  Prism.highlightAll()
}, { immediate: true })

// --- Metadata Formatting ---
const formattedDate = computed(() => {
  if (!props.content?.date) return '';
  return new Date(props.content.date).toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
});

const identityInfo = computed(() => {
  const map: Record<string, { label: string; bg: string; text: string }> = {
    'Department': { label: '部员', bg: 'bg-cyan-100 dark:bg-cyan-500/20', text: 'text-cyan-700 dark:text-cyan-300' },
    'Minister': { label: '部长', bg: 'bg-orange-100 dark:bg-orange-500/20', text: 'text-orange-700 dark:text-orange-300' },
    'President': { label: '社长', bg: 'bg-red-100 dark:bg-red-500/20', text: 'text-red-700 dark:text-red-300' },
    'Founder': { label: '创始人', bg: 'bg-purple-100 dark:bg-purple-500/20', text: 'text-purple-700 dark:text-purple-300' },
  };
  return map[props.content?.identity || ''] || null;
});

// --- TOC Interaction ---
const handleAnchorClick = (e: Event, id: string) => {
  e.preventDefault();
  const el = document.getElementById(id);
  if (el) {
    el.scrollIntoView({ behavior: 'smooth' });
    history.replaceState(null, '', `${route.path}#${id}`);
    activeHeadingId.value = id;
  }
};

// Scroll Spy
let observer: IntersectionObserver | null = null;
const setupObserver = () => {
  if (observer) observer.disconnect();

  observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        activeHeadingId.value = entry.target.id;
      }
    });
  }, { rootMargin: '-100px 0px -60% 0px' }); // Trigger when element is near top

  document.querySelectorAll('h2, h3').forEach(h => {
    observer?.observe(h);
  });
};

watch(html, () => {
  nextTick(() => setTimeout(setupObserver, 200));
});

onBeforeUnmount(() => observer?.disconnect());
</script>

<template>
  <div v-if="content" class="relative flex w-full flex-col lg:flex-row lg:gap-12">

    <!-- Left: Main Content -->
    <article class="min-w-0 flex-1 pb-20">

      <!-- Article Header -->
      <header class="mb-8 border-b border-zinc-100 pb-8 dark:border-zinc-800">
        <h1 class="mb-4 text-3xl font-extrabold tracking-tight text-zinc-900 dark:text-white sm:text-4xl">
          {{ content.title }}
        </h1>

        <div class="flex flex-wrap items-center gap-3 text-sm">
          <!-- Date Pill -->
          <div class="flex items-center gap-1.5 rounded-full bg-zinc-100 px-3 py-1 text-zinc-500 dark:bg-zinc-800 dark:text-zinc-400">
            <Icon icon="lucide:calendar" class="h-3.5 w-3.5" />
            <time>{{ formattedDate }}</time>
          </div>

          <!-- Identity Pill -->
          <div v-if="identityInfo"
               class="flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold"
               :class="[identityInfo.bg, identityInfo.text]">
            {{ identityInfo.label }}
          </div>
        </div>
      </header>

      <!-- Formatting Content Wrapper -->
      <!--
         prose-zinc: Neutral gray typographic scale
         max-w-none: Allow full width of container
         prose-headings: scroll-mt-20 included via renderer but styling here
      -->
      <div
          class="prose prose-zinc max-w-none dark:prose-invert
          prose-headings:font-bold prose-headings:tracking-tight
          prose-h1:text-3xl prose-h2:text-2xl prose-h3:text-xl
          prose-code:text-xs prose-code:font-mono prose-code:before:content-none prose-code:after:content-none
          prose-code:bg-zinc-100 prose-code:dark:bg-zinc-800 prose-code:px-1.5 prose-code:py-0.5 prose-code:rounded-md
          prose-pre:bg-[#1e1e20] prose-pre:rounded-xl prose-pre:text-sm prose-pre:leading-relaxed
          prose-img:rounded-xl prose-img:shadow-sm"
          v-html="html"
      ></div>
    </article>

    <!-- Right: Table of Contents (Desktop Only) -->
    <!-- Apple Developer Documentation Style: Right thin rail -->
    <aside v-if="showNav && flatHeadings.length > 0" class="hidden lg:block lg:w-64 lg:shrink-0">
      <div class="sticky top-4 overflow-y-auto max-h-[calc(100vh-120px)] pt-2 pr-2">
        <div class="mb-4 text-xs font-bold uppercase tracking-wider text-zinc-400 dark:text-zinc-500">
          本页目录
        </div>

        <nav class="relative">
          <!-- Decorative Line -->
          <div class="absolute left-0 top-0 bottom-0 w-[1px] bg-zinc-200 dark:bg-zinc-800 pointer-events-none"></div>

          <ul class="space-y-0.5 text-sm">
            <li v-for="heading in flatHeadings" :key="heading.id">
              <a
                  :href="`${route.path}#${heading.id}`"
                  @click="handleAnchorClick($event, heading.id)"
                  class="group flex items-start py-1.5 pl-4 transition-colors border-l-2 -ml-[1px]"
                  :class="[
                  activeHeadingId === heading.id
                    ? 'border-blue-500 text-blue-600 dark:text-blue-400 font-medium'
                    : 'border-transparent text-zinc-500 hover:text-zinc-900 dark:text-zinc-500 dark:hover:text-zinc-300 hover:border-zinc-300 dark:hover:border-zinc-600'
                ]"
              >
               <span :class="heading.level === 3 ? 'pl-3' : ''" class="truncate block">
                 {{ heading.text }}
               </span>
              </a>
            </li>
          </ul>
        </nav>
      </div>
    </aside>

  </div>

  <!-- Empty State -->
  <div v-else class="flex h-[60vh] flex-col items-center justify-center text-center">
    <div class="mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-zinc-100 dark:bg-zinc-800">
      <Icon icon="lucide:book-open" class="h-8 w-8 text-zinc-400" />
    </div>
    <h3 class="text-lg font-medium text-zinc-900 dark:text-white">暂无内容</h3>
    <p class="mt-1 text-zinc-500 dark:text-zinc-400">请从左侧菜单选择要阅读的文档</p>
  </div>
</template>

<style scoped>
/* Prism Theme Override for Minimalist Look */
@import "prismjs/themes/prism-tomorrow.min.css"; /* Using tomorrow night as base for dark code blocks */

/* Customize scroll behavior */
:deep(*):focus {
  outline: none;
}

/* Enhancing code block visuals inside typography */
:deep(pre) {
  margin-top: 1.5em;
  margin-bottom: 1.5em;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

/* Style for blockquotes to match Apple Notes */
:deep(blockquote) {
  font-style: normal;
  border-left-width: 3px;
  border-color: #e4e4e7; /* zinc-200 */
  color: #71717a; /* zinc-500 */
  background: transparent;
  padding-left: 1rem;
}

.dark :deep(blockquote) {
  border-color: #3f3f46; /* zinc-700 */
  color: #a1a1aa; /* zinc-400 */
}

/* Table Styling */
:deep(table) {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  border-radius: 8px;
  border: 1px solid #e4e4e7;
  overflow: hidden;
  font-size: 0.875rem;
}
.dark :deep(table) {
  border-color: #3f3f46;
}

:deep(thead tr) {
  background-color: #f4f4f5; /* zinc-100 */
}
.dark :deep(thead tr) {
  background-color: #27272a; /* zinc-800 */
}

:deep(th), :deep(td) {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #e4e4e7;
  text-align: left;
}
.dark :deep(th), .dark :deep(td) {
  border-bottom-color: #3f3f46;
}

:deep(th) {
  font-weight: 600;
  color: #18181b;
}
.dark :deep(th) {
  color: #f4f4f5;
}

:deep(tr:last-child td) {
  border-bottom: none;
}

/* Links */
:deep(.prose a) {
  text-decoration-line: none;
  font-weight: 500;
  color: #3b82f6;
  transition: color 0.15s;
}
:deep(.prose a:hover) {
  color: #2563eb;
  text-decoration-line: underline;
}

/* Mermaid Diagram Sizing */
:deep(.mermaid) {
  display: flex;
  justify-content: center;
  margin: 2rem 0;
  background-color: #f9fafb;
  padding: 1rem;
  border-radius: 0.5rem;
}
.dark :deep(.mermaid) {
  background-color: #1a1a1a;
}
</style>
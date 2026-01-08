<script setup lang="ts">
import {ref, computed} from 'vue'
import {Icon} from '@iconify/vue'
import {NCollapse, NCollapseItem} from 'naive-ui'

// --- 类型定义 ---
interface Category {
  id: string
  title: string
  icon: string
  desc: string
}

interface FaqItem {
  id: string
  question: string
  answer: string
  category: string
}

// --- 数据 Mock ---
const categories = ref<Category[]>([
  {id: 'start', title: '快速开始', icon: 'ion:rocket-outline', desc: ''},
  {id: 'account', title: '账户管理', icon: 'ion:person-outline', desc: '设置、安全与隐私'},
  {id: 'billing', title: '账单订阅', icon: 'ion:card-outline', desc: ''},
  {id: 'tech', title: '技术支持', icon: 'ion:hammer-outline', desc: ''},
  {id: 'community', title: '社区论坛', icon: 'ion:chatbubbles-outline', desc: ''},
  {id: 'contact', title: '联系我们', icon: 'ion:mail-outline', desc: '获取人工帮助'},
])

const faqs = ref<FaqItem[]>([
  {
    id: '1',
    category: 'start',
    question: '如何重置我的账户密码？',
    answer: '您可以在登录页面点击“忘记密码”，或者进入设置页面中的“安全”选项卡进行修改。我们会向您的注册邮箱发送一封验证邮件。'
  },
  {
    id: '2',
    category: 'billing',
    question: '',
    answer: ''
  },
  {
    id: '3',
    category: 'account',
    question: '？',
    answer: ''
  },
  {
    id: '4',
    category: 'tech',
    question: '',
    answer: ''
  },
])

// --- 逻辑 ---
const searchQuery = ref('')

// 简单的搜索过滤逻辑
const filteredFaqs = computed(() => {
  if (!searchQuery.value) return faqs.value
  const query = searchQuery.value.toLowerCase()
  return faqs.value.filter(item =>
      item.question.toLowerCase().includes(query) ||
      item.answer.toLowerCase().includes(query)
  )
})
</script>

<template>
  <!--
    整体容器
    Apple 风格背景: 亮色倾向于非常浅的灰色(#F5F5F7)，暗色倾向于深层黑(#000000)或极深灰
  -->
  <div
      class="min-h-screen bg-[#F5F5F7] dark:bg-black text-[#1d1d1f] dark:text-[#f5f5f7] transition-colors duration-300 font-sans pb-20">

    <!-- Hero Section -->
    <div class="relative pt-24 pb-16 px-6 flex flex-col items-center justify-center text-center">
      <h1 class="text-4xl md:text-5xl font-bold tracking-tight mb-4 text-[#1d1d1f] dark:text-white">
        嗨，我们可以帮您什么？
      </h1>
      <p class="text-lg text-gray-500 dark:text-gray-400 mb-10 max-w-2xl">
        搜索热门话题，浏览分类，或者查找常见问题解答。
      </p>

      <!-- 搜索框 (Spotlight Style) -->
      <div class="w-full max-w-2xl relative group">
        <div class="absolute inset-y-0 left-4 flex items-center pointer-events-none z-10">
          <Icon icon="ion:search-outline"
                class="text-gray-400 text-xl group-focus-within:text-[#0071e3] transition-colors"/>
        </div>
        <!-- 使用原生 input 实现更精细的 Apple 风格控制，也可以用 Naive NInput -->
        <input
            v-model="searchQuery"
            type="text"
            placeholder="搜索问题、话题或关键词..."
            class="w-full py-4 pl-12 pr-4 bg-white dark:bg-[#1c1c1e]
                 border border-gray-300/50 dark:border-white/10
                 rounded-2xl shadow-sm
                 text-lg placeholder:text-gray-400
                 focus:outline-none focus:ring-4 focus:ring-[#0071e3]/20 focus:border-[#0071e3]
                 transition-all duration-300"
        />
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="container mx-auto px-6">

      <!-- 分类网格 (Bento Grid 风格) -->
      <div v-if="!searchQuery" class="mb-16">
        <h2 class="text-2xl font-semibold mb-6 px-1">浏览分类</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          <div
              v-for="cat in categories"
              :key="cat.id"
              class="group cursor-pointer p-6 rounded-2xl bg-white dark:bg-[#1c1c1e]
                   border border-transparent dark:border-white/5
                   shadow-sm hover:shadow-xl hover:scale-[1.02]
                   transition-all duration-300 ease-out"
          >
            <!--Icon Container-->
            <div
                class="w-12 h-12 rounded-xl bg-gray-100 dark:bg-gray-700/50 flex items-center justify-center mb-4 text-[#0071e3] transition-colors group-hover:bg-[#0071e3] group-hover:text-white">
              <Icon :icon="cat.icon" width="24" height="24"/>
            </div>
            <h3 class="text-lg font-semibold mb-1 group-hover:text-[#0071e3] transition-colors">
              {{ cat.title }}
            </h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed">
              {{ cat.desc }}
            </p>
          </div>
        </div>
      </div>

      <!-- FAQ 区域 -->
      <div class="mx-auto">
        <h2 class="text-2xl font-semibold mb-6 px-1 flex items-center gap-2">
          <Icon v-if="searchQuery" icon="ion:search"/>
          {{ searchQuery ? '搜索结果' : '常见问题' }}
        </h2>

        <!-- Custom Container for List (Replacing NCard) -->
        <div
            class="bg-white dark:bg-[#1c1c1e] rounded-2xl shadow-sm border border-gray-200/50 dark:border-white/10 overflow-hidden">
          <n-collapse
              accordion
              arrow-placement="right"
              display-directive="show"
          >
            <!-- 无结果状态 -->
            <div v-if="filteredFaqs.length === 0" class="p-8 text-center text-gray-500">
              <Icon icon="ion:document-text-outline" class="text-4xl mx-auto mb-2 opacity-30"/>
              <p>没有找到与 "{{ searchQuery }}" 相关的内容</p>
            </div>

            <n-collapse-item
                v-for="faq in filteredFaqs"
                :key="faq.id"
                :title="faq.question"
                :name="faq.id"
                class="group border-b border-gray-100 dark:border-white/5 last:border-0 px-6 py-4 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors mt-0!"
            >
              <template #header>
                <span class="text-base font-medium text-[#1d1d1f] dark:text-gray-200">
                  {{ faq.question }}
                </span>
              </template>

              <!-- 内容区域 -->
              <div class="text-base text-gray-600 dark:text-gray-300 leading-relaxed pt-2 pb-2">
                {{ faq.answer }}
              </div>
            </n-collapse-item>
          </n-collapse>
        </div>
      </div>

      <!-- 底部联系 (Call to Action) -->
      <div class="mt-20 text-center">
        <h3 class="text-xl font-semibold mb-2">找不到答案？</h3>
        <p class="text-gray-500 mb-6">我们的支持团队随时准备为您服务。</p>
        <button
            class="bg-[#0071e3] hover:bg-[#0077ED] text-white px-6 py-3 rounded-full font-medium transition-all hover:shadow-lg active:scale-95 flex items-center justify-center gap-2 mx-auto">
          <Icon icon="ion:chatbox-ellipses-outline"/>
          联系支持团队
        </button>
      </div>

    </div>
  </div>
</template>

<style scoped>
/*
  样式微调：为了适配 Naive UI 的 Collapse 组件到 Apple 风格
  我们在 scoped 中覆盖一些 naive ui 的内部细节，使其更极简。
*/

:deep(.n-collapse .n-collapse-item .n-collapse-item__header) {
  padding-top: 0 !important;
  padding-bottom: 0 !important;
}

:deep(.n-collapse .n-collapse-item__content-inner) {
  padding-top: 0 !important;
}

/* 去除 Naive 默认的 focus 线条，保持 clean */
:deep(.n-collapse-item__header:focus) {
  outline: none !important;
}
</style>
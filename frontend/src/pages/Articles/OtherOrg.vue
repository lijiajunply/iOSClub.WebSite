<script setup lang="ts">
import { ref } from 'vue'
import { NImage, NDivider } from 'naive-ui'
import { Icon } from '@iconify/vue'
import xiaomiImg from '/assets/other/xiaomi.jpg'
import huaweiImg from '/assets/other/huawei.jpg'
import aircraftImg from '/assets/other/aircraft.jpg'

// 类型定义
interface Organization {
  id: string
  title: string
  image: string
  description: string[]
  color: string
  icon: string
}

interface OffCampusOrg {
  name: string
  url: string
  icon: string
}

// 组织数据
const organizations = ref<Organization[]>([
  {
    id: 'xiaomi',
    title: '米粉俱乐部',
    image: xiaomiImg,
    color: 'from-orange-500 to-red-500',
    icon: 'simple-icons:xiaomi',
    description: [
      '欢迎来到西建大米粉俱乐部——一个充满创新和科技热情的社区！我们是由小米公司在西安建筑科技大学创办的科技类俱乐部。这里，我们共同探索科技的无限可能，分享智能生活的每一个精彩瞬间。',
      '作为米粉俱乐部的一员，你将有机会参与到最前沿的科技讨论，体验最新的小米产品，甚至参与产品测试和反馈。我们的活动涵盖了科技讲座、工作坊、创新竞赛等多种形式，旨在激发每一位成员的创造力和潜能。',
      '我们诚挚邀请每一位对科技充满热情的朋友加入我们的大家庭。无论你是科技爱好者，还是寻求创新灵感的学者，西建大米粉俱乐部都是你理想的归宿。在这里，你可以与志同道合的伙伴交流思想，共同成长。',
      '加入我们，一起在科技的道路上，启航新征程！我们期待着你的加入，一起创造属于我们的未来。立即联系我们，开启你的米粉之旅吧。'
    ]
  },
  {
    id: 'huawei',
    title: '花粉俱乐部',
    image: huaweiImg,
    color: 'from-red-500 to-pink-500',
    icon: 'simple-icons:huawei',
    description: [
      '欢迎来到西建大花粉俱乐部! 我们是由华为公司在西安建筑科技大学创办的科技类俱乐部。这里，我们共同探索科技的无限可能，分享智能生活的每一个精彩瞬间。',
      '我们诚挚邀请每一位对科技充满热情的朋友加入我们的大家庭。无论你是科技爱好者，还是寻求创新灵感的学者，西建大花粉俱乐部都是你理想的归宿。在这里，你可以与志同道合的伙伴交流思想，共同成长。',
      '加入我们，一起在科技的道路上，启航新征程！我们期待着你的加入，一起创造属于我们的未来。立即联系我们，开启你的花粉之旅吧。'
    ]
  },
  {
    id: 'aircraft',
    title: '航模社',
    image: aircraftImg,
    color: 'from-blue-500 to-cyan-500',
    icon: 'mdi:airplane',
    description: [
      '航模社是一个充满激情与创新精神的学生社团，致力于航空模型和无人机的设计、制作与飞行。航模社不仅为学生提供了一个展示自己技术和创意的平台，还培养了团队合作和解决问题的能力。',
      '如果你对航空模型感兴趣，欢迎加入西安建筑科技大学航模社。无论你是新手还是有经验的模型爱好者，这里都有适合你的活动和项目。让我们一起在蓝天中翱翔，探索航空科技的无限可能！'
    ]
  }
])

// 校外组织
const offCampusOrgs = ref<OffCampusOrg[]>([
  {
    name: '西建大仙建协会【MC社】',
    url: 'https://skin.xauatcraft.com/',
    icon: 'game-icons:mining'
  },
  {
    name: '西安邮电大学Linux兴趣小组',
    url: 'https://www.xiyoulinux.com/',
    icon: 'mdi:linux'
  },
  {
    name: '西安邮电大学MC兴趣交流团体【邮立方】',
    url: 'https://cop.cooo.site/',
    icon: 'mdi:cube-outline'
  }
])

const hoveredOrgId = ref<string | null>(null)
const hoveredLinkIndex = ref<number | null>(null)
</script>

<template>
  <div class="min-h-screen transition-colors duration-500">
    <!-- Hero Section -->
    <div class="relative overflow-hidden bg-gradient-to-br from-blue-50 via-white to-purple-50 dark:from-zinc-900 dark:via-black dark:to-zinc-900">
      <div class="absolute inset-0 bg-[url('/grid.svg')] opacity-[0.02]"></div>
      <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-16 sm:py-24 relative">
        <div class="text-center max-w-3xl mx-auto">
          <div class="inline-flex items-center justify-center w-16 h-16 sm:w-20 sm:h-20 rounded-3xl bg-gradient-to-br from-blue-500 to-purple-600 mb-6 shadow-lg shadow-blue-500/25">
            <Icon icon="mdi:account-group" class="w-8 h-8 sm:w-10 sm:h-10 text-white" />
          </div>
          <h1 class="text-4xl sm:text-5xl lg:text-6xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-gray-900 via-gray-800 to-gray-900 dark:from-white dark:via-gray-100 dark:to-white mb-6 tracking-tight">
            其他组织
          </h1>
          <p class="text-lg sm:text-xl text-gray-600 dark:text-gray-400 leading-relaxed max-w-2xl mx-auto">
            除了 iOS Club，还有许多分社团和挂靠组织。同时也和校内外许多社团和组织保持着密切交流。
          </p>
        </div>
      </div>
    </div>

    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-12 sm:py-16">
      <!-- 组织列表 -->
      <div class="space-y-8 mb-16">
        <template v-for="(org, index) in organizations" :key="org.id">
          <section
              :id="org.id"
              class="group relative overflow-hidden rounded-3xl bg-white dark:bg-zinc-900 shadow-sm hover:shadow-2xl hover:shadow-gray-200/50 dark:hover:shadow-zinc-800/50 transition-all duration-500"
              @mouseenter="hoveredOrgId = org.id"
              @mouseleave="hoveredOrgId = null"
          >
            <!-- 渐变装饰背景 -->
            <div
                class="absolute inset-0 opacity-0 group-hover:opacity-5 dark:group-hover:opacity-10 transition-opacity duration-500"
                :class="`bg-gradient-to-br ${org.color}`"
            ></div>

            <div class="relative flex flex-col lg:flex-row">
              <!-- 图片部分 -->
              <div class="lg:w-2/5 p-8 sm:p-12 flex items-center justify-center">
                <div class="relative">
                  <!-- Icon 装饰 -->
                  <div
                      class="absolute -top-4 -left-4 w-12 h-12 rounded-2xl flex items-center justify-center shadow-lg transition-all duration-500"
                      :class="`bg-gradient-to-br ${org.color} ${hoveredOrgId === org.id ? 'scale-110 rotate-12' : ''}`"
                  >
                    <Icon :icon="org.icon" class="w-6 h-6 text-white" />
                  </div>

                  <NImage
                      class="w-full max-w-[280px] rounded-2xl shadow-xl ring-1 ring-gray-200/50 dark:ring-zinc-700/50 transition-all duration-500 group-hover:scale-105 group-hover:shadow-2xl"
                      :src="org.image"
                      :preview-disabled="false"
                      object-fit="cover"
                      :height="280"
                  />
                </div>
              </div>

              <!-- 内容部分 -->
              <div class="lg:w-3/5 p-8 sm:p-12 flex flex-col justify-center">
                <div class="flex items-center gap-3 mb-6">
                  <h2 class="text-2xl sm:text-3xl font-semibold text-gray-900 dark:text-white tracking-tight">
                    {{ org.title }}
                  </h2>
                  <div
                      class="w-2 h-2 rounded-full transition-all duration-500"
                      :class="`bg-gradient-to-r ${org.color} ${hoveredOrgId === org.id ? 'w-8' : ''}`"
                  ></div>
                </div>

                <div class="space-y-4">
                  <p
                      v-for="(paragraph, pIndex) in org.description"
                      :key="pIndex"
                      class="text-gray-600 dark:text-gray-400 leading-relaxed text-base sm:text-lg transition-all duration-300"
                      :style="{
                      transitionDelay: `${pIndex * 50}ms`,
                      opacity: hoveredOrgId === org.id ? 1 : 0.9
                    }"
                  >
                    {{ paragraph }}
                  </p>
                </div>
              </div>
            </div>
          </section>

          <!-- 分隔线 -->
          <NDivider
              v-if="index !== organizations.length - 1"
              class="!my-0 opacity-0"
          />
        </template>
      </div>

      <!-- 校外组织 -->
      <section
          id="off-campus"
          class="rounded-3xl bg-white dark:bg-zinc-900 shadow-sm p-8 sm:p-12 relative overflow-hidden"
      >
        <!-- 装饰元素 -->
        <div class="absolute top-0 right-0 w-64 h-64 bg-gradient-to-br from-blue-500/5 to-purple-500/5 rounded-full blur-3xl"></div>

        <div class="relative">
          <div class="flex items-center gap-4 mb-8">
            <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center shadow-lg shadow-blue-500/25">
              <Icon icon="mdi:link-variant" class="w-6 h-6 text-white" />
            </div>
            <h3 class="text-2xl sm:text-3xl font-semibold text-gray-900 dark:text-white tracking-tight">
              校内外兄弟组织
            </h3>
          </div>

          <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <a
                v-for="(org, index) in offCampusOrgs"
                :key="index"
                :href="org.url"
                target="_blank"
                class="group relative p-6 rounded-2xl bg-gray-50 dark:bg-zinc-800/50 hover:bg-gradient-to-br hover:from-blue-50 hover:to-purple-50 dark:hover:from-zinc-800 dark:hover:to-zinc-700 transition-all duration-300 hover:shadow-lg hover:scale-[1.02] hover:-translate-y-1"
                @mouseenter="hoveredLinkIndex = index"
                @mouseleave="hoveredLinkIndex = null"
            >
              <!-- 图标 -->
              <div class="flex items-start gap-4">
                <div class="w-10 h-10 rounded-xl bg-white dark:bg-zinc-700 flex items-center justify-center shadow-sm group-hover:shadow-md transition-all duration-300 group-hover:scale-110">
                  <Icon :icon="org.icon" class="w-5 h-5 text-blue-500 dark:text-blue-400" />
                </div>

                <div class="flex-1 min-w-0">
                  <span class="block text-gray-900 dark:text-white font-medium mb-1 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                    {{ org.name }}
                  </span>
                  <span class="text-sm text-gray-500 dark:text-gray-500 truncate block">
                    {{ org.url.replace('https://', '') }}
                  </span>
                </div>

                <!-- 箭头图标 -->
                <Icon
                    icon="mdi:arrow-top-right"
                    class="w-5 h-5 text-gray-400 group-hover:text-blue-500 transition-all duration-300 group-hover:translate-x-1 group-hover:-translate-y-1"
                />
              </div>

              <!-- Hover 装饰条 -->
              <div
                  class="absolute bottom-0 left-0 h-1 bg-gradient-to-r from-blue-500 to-purple-600 transition-all duration-300 rounded-full"
                  :style="{ width: hoveredLinkIndex === index ? '100%' : '0%' }"
              ></div>
            </a>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
/* 平滑滚动 */
html {
  scroll-behavior: smooth;
}

/* 苹果风格的毛玻璃效果 */
@supports (backdrop-filter: blur(20px)) {
  .backdrop-blur-apple {
    backdrop-filter: blur(20px) saturate(180%);
  }
}

/* 自定义动画 */
@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-10px);
  }
}

.animate-float {
  animation: float 3s ease-in-out infinite;
}

/* 优化移动端体验 */
@media (max-width: 640px) {
  .container {
    padding-left: 1rem;
    padding-right: 1rem;
  }
}

/* 深色模式优化 */
@media (prefers-color-scheme: dark) {
  ::selection {
    background-color: rgba(59, 130, 246, 0.3);
  }
}

/* 浅色模式优化 */
@media (prefers-color-scheme: light) {
  ::selection {
    background-color: rgba(59, 130, 246, 0.2);
  }
}
</style>
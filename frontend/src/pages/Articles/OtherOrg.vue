<script setup lang="ts">
import {ref} from 'vue'
import {NImage} from 'naive-ui'
import {Icon} from '@iconify/vue'
// 保持原有图片引用路径
import xiaomiImg from '/assets/other/xiaomi.jpg'
import huaweiImg from '/assets/other/huawei.jpg'
import aircraftImg from '/assets/other/aircraft.jpg'

// --- 接口定义 ---
interface Organization {
  id: string
  title: string
  subtitle?: string // 新增副标题
  image: string
  description: string[]
  themeColor: string // 用于强调色
  icon: string
}

interface OffCampusOrg {
  name: string
  url: string
  icon: string
  category: string
}

// --- 数据 ---
const organizations = ref<Organization[]>([
  {
    id: 'xiaomi',
    title: '米粉俱乐部',
    subtitle: '探索科技无限可能',
    image: xiaomiImg,
    themeColor: '#ff6900', // 小米橙
    icon: 'simple-icons:xiaomi',
    description: [
      '欢迎来到西建大米粉俱乐部。这是一个充满创新和科技热情的社区，由小米公司在西安建筑科技大学创办。',
      '参与最前沿的科技讨论，体验最新的小米产品，甚至参与产品测试和反馈。',
      '无论你是科技爱好者，还是寻求创新灵感的学者，这里都是你理想的归宿。'
    ]
  },
  {
    id: 'huawei',
    title: '花粉俱乐部',
    subtitle: '构建万物互联的智能世界',
    image: huaweiImg,
    themeColor: '#cf0a2c', // 华为红
    icon: 'simple-icons:huawei',
    description: [
      '西建大花粉俱乐部是由华为公司创办的科技社团。我们共同探索科技的无限可能，分享智能生活的每一个精彩瞬间。',
      '诚挚邀请每一位对科技充满热情的朋友加入，与志同道合的伙伴交流思想，共同成长。'
    ]
  },
  {
    id: 'aircraft',
    title: '航模社',
    subtitle: '制霸蓝天，放飞梦想',
    image: aircraftImg,
    themeColor: '#007aff', // 苹果蓝作为通用科技色
    icon: 'mdi:airplane',
    description: [
      '致力于航空模型和无人机的设计、制作与飞行。航模社不仅提供展示技术和创意的平台，更培养团队合作能力。',
      '无论你是新手还是有经验的模型爱好者，让我们一起在蓝天中翱翔。'
    ]
  }
])

const offCampusOrgs = ref<OffCampusOrg[]>([
  {
    name: '仙建协会【MC社】',
    url: 'https://skin.xauatcraft.com/',
    icon: 'mdi:minecraft',
    category: 'Gaming'
  },
  {
    name: '西邮 Linux 小组',
    url: 'https://www.xiyoulinux.com/',
    icon: 'uim:linux',
    category: 'Tech'
  },
  {
    name: '西邮 MC 兴趣团体',
    url: 'https://cop.cooo.site/',
    icon: 'tabler:brand-minecraft',
    category: 'Gaming'
  }
])

const hoveredOrgId = ref<string | null>(null)
</script>

<template>
  <div
      class="min-h-screen w-full bg-[#F5F5F7] dark:bg-black text-[#1d1d1f] dark:text-[#f5f5f7] font-sans transition-colors duration-500 selection:bg-blue-500/30">

    <!-- Content Wrapper -->
    <div class="max-w-5xl mx-auto px-5 sm:px-6 pt-20 pb-32">

      <!-- Header Section -->
      <header class="text-center mb-20 opacity-0 animate-fade-in-up"
              style="animation-delay: 0.1s; animation-fill-mode: forwards;">
        <div class="inline-flex items-center justify-center mb-6">
          <div
              class="w-16 h-16 rounded-2xl bg-white dark:bg-[#1c1c1e] shadow-sm flex items-center justify-center border border-gray-200/50 dark:border-white/10">
            <Icon icon="mdi:account-group" class="text-4xl text-blue-500"/>
          </div>
        </div>
        <h1 class="text-4xl md:text-5xl font-semibold tracking-tight mb-4 text-black dark:text-white">
          更多精彩社团
        </h1>
        <p class="text-xl md:text-2xl font-medium text-[#86868b] max-w-2xl mx-auto leading-relaxed">
          除了 iOS Club，这里还有与其紧密合作的<br class="hidden sm:block"/>
          优秀科技组织与兴趣团体。
        </p>
      </header>

      <!-- Main Organizations List -->
      <div class="space-y-10 md:space-y-16">
        <div
            v-for="(org, idx) in organizations"
            :key="org.id"
            class="group relative flex flex-col md:flex-row bg-white dark:bg-[#1c1c1e] rounded-[32px] overflow-hidden shadow-sm hover:shadow-2xl transition-all duration-500 ease-out-apple md:h-[480px]"
            :class="{'md:flex-row-reverse': idx % 2 !== 0}"
            @mouseenter="hoveredOrgId = org.id"
            @mouseleave="hoveredOrgId = null"
        >
          <!-- Image Section (Half Width) -->
          <div class="w-full md:w-1/2 h-64 md:h-auto relative overflow-hidden bg-gray-100 dark:bg-[#2c2c2e]">
            <!-- Image Scale Effect -->
            <div class="w-full h-full transition-transform duration-700 ease-out-apple"
                 :class="hoveredOrgId === org.id ? 'scale-105' : 'scale-100'">
              <NImage
                  :src="org.image"
                  class="w-full h-full"
                  object-fit="cover"
                  :preview-disabled="false"
              />
            </div>

            <!-- Mobile Overlay Gradient (Only visible on small screens) -->
            <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent md:hidden"></div>

            <!-- Floating Logo Badge -->
            <div
                class="absolute top-6 left-6 md:top-8 md:left-8 w-12 h-12 md:w-14 md:h-14 rounded-full bg-white/90 dark:bg-black/60 backdrop-blur-md shadow-lg flex items-center justify-center z-10">
              <Icon :icon="org.icon" class="text-2xl md:text-3xl" :style="{ color: org.themeColor }"/>
            </div>
          </div>

          <!-- Text Section (Half Width) -->
          <div class="w-full md:w-1/2 p-8 md:p-12 flex flex-col justify-center relative z-0">
            <div class="mb-auto">
              <span class="text-xs font-bold uppercase tracking-wider text-[#86868b] block mb-2">Organization</span>
              <h2 class="text-3xl font-bold text-black dark:text-white mb-2 tracking-tight">
                {{ org.title }}
              </h2>
              <p class="text-lg font-medium text-blue-500 mb-6"
                 :style="{ color: hoveredOrgId === org.id ? org.themeColor : '' }">
                {{ org.subtitle }}
              </p>
            </div>

            <div
                class="prose dark:prose-invert prose-p:text-[#1d1d1f] dark:prose-p:text-[#d2d2d7] prose-p:font-normal prose-p:text-[15px] prose-p:leading-7">
              <p v-for="(para, i) in org.description.slice(0, 2)" :key="i">
                {{ para }}
              </p>
            </div>

            <!-- "Learn More" style link simulating Apple button -->
            <div
                class="mt-8 pt-6 border-t border-gray-100 dark:border-white/5 flex items-center text-sm font-semibold cursor-pointer group/link">
              <span class="group-hover/link:underline decoration-2 underline-offset-4 transition-all">了解详情</span>
              <Icon icon="mdi:chevron-right"
                    class="ml-1 text-lg opacity-60 group-hover/link:translate-x-1 transition-transform"/>
            </div>
          </div>
        </div>
      </div>

      <!-- Off-Campus / Grid Section -->
      <section class="mt-32">
        <div class="flex items-baseline justify-between mb-8 px-2">
          <h2 class="text-3xl font-semibold text-black dark:text-white tracking-tight">友情链接</h2>
          <span class="text-sm font-medium text-[#86868b]">校内外伙伴</span>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-5">
          <a
              v-for="link in offCampusOrgs"
              :key="link.url"
              :href="link.url"
              target="_blank"
              class="group relative bg-white dark:bg-[#1c1c1e] p-6 rounded-[24px] shadow-sm hover:scale-[1.02] active:scale-95 transition-all duration-300 ease-apple-spring border border-transparent hover:border-blue-500/20 dark:hover:border-blue-400/20"
          >
            <div class="flex items-start justify-between mb-8">
              <div
                  class="w-12 h-12 rounded-xl bg-gray-50 dark:bg-[#2c2c2e] flex items-center justify-center text-2xl group-hover:bg-blue-50 dark:group-hover:bg-blue-900/20 transition-colors">
                <Icon :icon="link.icon"
                      class="text-gray-600 dark:text-gray-300 group-hover:text-blue-600 dark:group-hover:text-blue-400"/>
              </div>
              <div
                  class="w-8 h-8 rounded-full bg-gray-100 dark:bg-[#2c2c2e] flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity transform translate-x-2 group-hover:translate-x-0">
                <Icon icon="mdi:arrow-top-right" class="text-sm text-gray-500"/>
              </div>
            </div>

            <div>
              <div class="text-xs font-semibold text-[#86868b] uppercase tracking-wide mb-1">{{ link.category }}</div>
              <h3 class="text-lg font-semibold text-black dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                {{ link.name }}
              </h3>
              <p class="text-sm text-[#86868b] mt-1 truncate opacity-0 group-hover:opacity-100 transition-opacity duration-300 delay-75">
                {{ link.url.replace(/https?:\/\//, '') }}
              </p>
            </div>
          </a>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
/*
  原生 CSS 定义
  虽然 Tailwind 4.0 可以做很多，但复杂的动画曲线和特定样式在这里维护更清晰
*/

/* Apple 风格的缓动曲线 (Spring 效果) */
.ease-apple-spring {
  transition-timing-function: cubic-bezier(0.25, 0.1, 0.25, 1.0);
}

.ease-out-apple {
  transition-timing-function: cubic-bezier(0.32, 0.72, 0, 1);
}

/* 简单的入场动画 */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-in-up {
  animation: fadeInUp 1s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

/* 强制 NImage 填满容器 */
:deep(.n-image img) {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* 优化黑暗模式下的平滑过渡 */
@media (prefers-color-scheme: dark) {
  .group:hover {
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7);
  }
}
</style>
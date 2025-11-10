<template>
  <div class="min-h-screen transition-colors duration-500">

    <!-- Hero Section - Apple Style -->
    <section class="relative flex items-center justify-center px-6 lg:px-8 overflow-hidden" style="min-height: calc(100vh - 64px);">
      <!-- Background Gradient -->
      <div
          class="absolute inset-0 bg-gradient-to-br from-blue-500/5 via-purple-500/5 to-pink-500/5 dark:from-blue-500/10 dark:via-purple-500/10 dark:to-pink-500/10"></div>

      <div class="relative max-w-7xl w-full mx-auto">
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">

          <!-- Logo Section -->
          <div class="flex justify-center lg:justify-start mt-6 md:mt-0 ">
            <div
                class="relative w-72 h-72 md:w-96 md:h-96 group"
                @mouseenter="logoHovered = true"
                @mouseleave="logoHovered = false"
            >
              <!-- Floating Background Layers -->
              <div
                  class="absolute inset-0 bg-gradient-to-br from-blue-500 to-blue-600 rounded-[3rem] transform transition-all duration-700 ease-out"
                  :class="logoHovered ? 'rotate-12 scale-105' : 'rotate-6'"
                  style="opacity: 0.15"
              ></div>
              <div
                  class="absolute inset-0 bg-gradient-to-br from-purple-500 to-purple-600 rounded-[3rem] transform transition-all duration-700 ease-out"
                  :class="logoHovered ? '-rotate-12 scale-105' : '-rotate-6'"
                  style="opacity: 0.15"
              ></div>

              <!-- Main Logo Card -->
              <div
                  class="relative w-full h-full bg-white/90 dark:bg-white/10 backdrop-blur-3xl rounded-[3rem] flex items-center justify-center shadow-2xl border border-gray-200/50 dark:border-white/10 transition-all duration-700"
                  :class="logoHovered ? 'scale-105' : 'scale-100'">
                <img
                    src="/assets/iOS_Club_LOGO.png"
                    alt="iOS Club Logo"
                    class="w-3/5 h-3/5 object-contain transition-transform duration-700"
                    :class="logoHovered ? 'scale-110' : 'scale-100'"
                />
              </div>
            </div>
          </div>

          <!-- Hero Content -->
          <div class="text-center lg:text-left space-y-8">
            <div class="space-y-4">
              <h1 class="text-4xl md:text-7xl font-bold tracking-tight gradient-text leading-tight">
                iOS Club of XAUAT
              </h1>
            </div>

            <p class="text-base md:text-2xl text-gray-700 dark:text-gray-300 leading-relaxed max-w-xl mx-auto lg:mx-0">
              西安建筑科技大学iOS众创空间俱乐部
            </p>

            <div class="flex items-center justify-center lg:justify-start gap-3 text-gray-500 dark:text-gray-400">
              <Icon icon="material-symbols:format-quote" class="text-base md:text-xl"/>
              <p class="text-base md:text-xl italic">
                Stay hungry, stay foolish
              </p>
            </div>

            <!-- CTA Buttons -->
            <div class="pt-4 flex flex-col sm:flex-row gap-4 justify-center lg:justify-start">
              <button
                  @click="scrollToAbout"
                  class="group relative px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white rounded-full font-medium text-lg transition-all duration-300 shadow-lg hover:shadow-xl hover:scale-105 active:scale-95"
              >
                <span class="flex items-center justify-center gap-2">
                  了解更多
                  <Icon icon="heroicons:arrow-down-20-solid"
                        class="text-xl group-hover:translate-y-1 transition-transform duration-300"/>
                </span>
              </button>

              <button
                  @click="router.push('/login')"
                  class="px-8 py-4 bg-white/80 dark:bg-white/10 hover:bg-white dark:hover:bg-white/20 backdrop-blur-xl text-gray-900 dark:text-white rounded-full font-medium text-lg transition-all duration-300 border border-gray-300 dark:border-white/20 hover:scale-105 active:scale-95 hover:shadow-lg"
              >
                登录/注册
              </button>
            </div>

            <div class="h-4 md:h-0"></div>
          </div>
        </div>
      </div>
    </section>

    <!-- About Section -->
    <section
        id="about"
        class="py-32 px-6 lg:px-8 "
    >
      <div class="max-w-7xl mx-auto">
        <!-- Section Header -->
        <div class="text-center mb-20 space-y-4">
          <h2 class="text-5xl md:text-6xl font-semibold text-gray-900 dark:text-white tracking-tight">
            关于我们
          </h2>
          <p class="text-xl text-gray-500 dark:text-gray-400">
            探索 iOS Club 的更多可能
          </p>
        </div>

        <!-- Feature Cards Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-16">
          <div
              v-for="(card, index) in cards"
              :key="index"
              class="group relative p-8 rounded-3xl bg-gradient-to-br from-gray-50 to-gray-100/50 dark:from-white/5 dark:to-white/[0.02] border border-gray-200/50 dark:border-white/10 hover:border-gray-300 dark:hover:border-white/20 transition-all duration-500 hover:scale-[1.02] hover:shadow-2xl cursor-pointer overflow-hidden"
          >
            <!-- Hover Gradient Effect -->
            <div
                class="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500 rounded-3xl"
                :style="{ background: `radial-gradient(circle at 50% 0%, ${card.color}15, transparent 70%)` }"
            ></div>

            <div class="relative z-10 space-y-4">
              <!-- Icon -->
              <div
                  class="inline-flex p-4 rounded-2xl transition-all duration-500 group-hover:scale-110"
                  :style="{
                  backgroundColor: `${card.color}15`,
                }"
              >
                <Icon
                    :icon="card.iconName"
                    class="text-4xl"
                    :style="{ color: card.color }"
                />
              </div>

              <!-- Title -->
              <h3 class="text-xl font-semibold text-gray-900 dark:text-white tracking-tight">
                {{ card.title }}
              </h3>

              <!-- Content -->
              <p class="text-base text-gray-600 dark:text-gray-400 leading-relaxed">
                {{ card.content }}
              </p>
            </div>

            <!-- Bottom Accent Line -->
            <div
                class="absolute bottom-0 left-0 right-0 h-1 transform scale-x-0 group-hover:scale-x-100 transition-transform duration-500 rounded-full"
                :style="{ backgroundColor: card.color }"
            ></div>
          </div>
        </div>

        <!-- More Details Button -->
        <div class="text-center">
          <button
              @click="router.push('/About')"
              class="group inline-flex items-center gap-3 px-10 py-5 bg-gray-900 dark:bg-white text-white dark:text-black rounded-full font-medium text-lg transition-all duration-300 hover:scale-105 active:scale-95 hover:shadow-2xl"
          >
            查看更多详情
            <Icon
                icon="heroicons:arrow-right-20-solid"
                class="text-xl group-hover:translate-x-2 transition-transform duration-300"
            />
          </button>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer
        class="relative border-t border-gray-200 dark:border-white/10 bg-white/50 dark:bg-white/[0.02] backdrop-blur-xl">
      <div class="max-w-7xl mx-auto px-6 lg:px-8 py-12">
        <div class="text-center space-y-6">
          <!-- Copyright -->
          <p class="text-sm text-gray-600 dark:text-gray-400">
            Copyright © 2023 - 2024 XAUAT iOS Club
          </p>

          <!-- Links -->
          <div class="flex flex-wrap justify-center items-center gap-4 text-sm">
            <a
                href="https://cn.xauat.edu.cn/"
                class="text-gray-700 dark:text-gray-300 hover:text-blue-600 dark:hover:text-blue-400 transition-colors duration-300"
                target="_blank"
                rel="noopener"
            >
              西安建筑科技大学
            </a>

            <span class="text-gray-300 dark:text-gray-700">·</span>

            <a
                href="https://beian.miit.gov.cn/"
                class="text-gray-700 dark:text-gray-300 hover:text-blue-600 dark:hover:text-blue-400 transition-colors duration-300"
                target="_blank"
                rel="noopener"
            >
              陕ICP备2024031872号
            </a>

            <span class="text-gray-300 dark:text-gray-700">·</span>

            <a
                href="https://gitee.com/XAUATiOSClub"
                class="inline-flex items-center gap-1.5 text-gray-700 dark:text-gray-300 hover:text-blue-600 dark:hover:text-blue-400 transition-colors duration-300"
                target="_blank"
                rel="noopener"
            >
              <Icon icon="simple-icons:gitee" class="text-base"/>
              Gitee
            </a>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import {ref} from 'vue'
import {useRouter} from 'vue-router'
import {Icon} from '@iconify/vue'

interface Card {
  iconName: string
  title: string
  content: string
  color: string
}

const router = useRouter()
const logoHovered = ref(false)

const cards: Card[] = [
  {
    iconName: 'heroicons:academic-cap-20-solid',
    title: '我们是谁?',
    content: '我们是由Apple公司资金支持，受学管和西安建筑科技大学大学生创新创业实践中心指导的创新创业类社团。',
    color: '#0071E3' // Apple Blue
  },
  {
    iconName: 'heroicons:bolt-solid',
    title: '和iOS Club一起结伴同行',
    content: '不管是零基础的小白还是大神，只要你有兴趣，这里就是你的天堂',
    color: '#FF9500' // Apple Orange
  },
  {
    iconName: 'heroicons:user-group-solid',
    title: 'iOS Club,不止iOS',
    content: '我们不止只有iOS，西建大iOS Club是一个跨专业与课堂的数码编程爱好者社团',
    color: '#34C759' // Apple Green
  },
  {
    iconName: 'heroicons:chat-bubble-left-ellipsis-solid',
    title: '和 iOS Club 一起玩',
    content: '活动覆盖编程和数码，不仅有最新数码设备体验活动，还有大咖沙龙和Apple官方活动',
    color: '#FF375F' // Apple Pink
  },
  {
    iconName: 'heroicons:code-bracket-solid',
    title: '合作软件开发',
    content: '加入我们，和志同道合的iMember一起合作开发，一起创造世界',
    color: '#AF52DE' // Apple Purple
  },
  {
    iconName: 'heroicons:arrow-trending-up-solid',
    title: '全校数码编程爱好者,联合起来！',
    content: '我们意图打造全校最大的科技社区，快来加入我们成为一名iMember吧',
    color: '#5E5CE6' // Apple Indigo
  }
]

const scrollToAbout = (): void => {
  const element = document.getElementById('about')
  if (element) {
    element.scrollIntoView({behavior: 'smooth', block: 'start'})
  }
}
</script>

<style scoped>
/* Apple-style Gradient Text */
.gradient-text {
  background: -webkit-linear-gradient(
      -64deg,
      #f9bf65, #ffab6b, #ff9977, #fc8986,
      #ef7e95, #e47da6, #d37fb5, #bf83c1,
      #ab8dcf, #9597d8, #7fa0dc, #6ca7da
  );
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  background-size: 200% 200%;
  animation: gradientFlow 8s ease infinite;
}

@keyframes gradientFlow {
  0%, 100% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
}

/* Smooth Scrolling */
html {
  scroll-behavior: smooth;
}

/* Custom Scrollbar - macOS Style */
::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.2);
  border-radius: 10px;
  border: 2px solid transparent;
  background-clip: padding-box;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.3);
  background-clip: padding-box;
}

.dark ::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
  background-clip: padding-box;
}

.dark ::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.3);
  background-clip: padding-box;
}

/* Apple SF Pro Font - Optional but recommended */
* {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Helvetica Neue', Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
</style>
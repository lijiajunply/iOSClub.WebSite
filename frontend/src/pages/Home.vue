<template>
  <div class="min-h-screen bg-white dark:bg-black transition-colors duration-500">
    <!-- Hero Section -->
    <section class="min-h-screen flex items-center justify-center px-4 sm:px-6 lg:px-8">
      <div class="max-w-7xl w-full">
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-16 items-center">

          <!-- Logo -->
          <div class="flex justify-center lg:justify-end">
            <div class="w-64 h-64 md:w-80 md:h-80 lg:w-96 lg:h-96 relative">
              <div
                  class="absolute inset-0 bg-gradient-to-br from-blue-500 to-purple-600 rounded-3xl transform rotate-6"></div>
              <div
                  class="absolute inset-0 bg-gradient-to-br from-blue-400 to-purple-500 rounded-3xl transform -rotate-6"></div>
              <div
                  class="relative w-full h-full bg-white dark:bg-black rounded-3xl flex items-center justify-center shadow-2xl">
                <img
                    src="/assets/iOS_Club_LOGO.png"
                    alt="iOS Club Logo"
                    class="w-3/4 h-3/4 object-contain"
                />
              </div>
            </div>
          </div>

          <!-- Content -->
          <div class="text-center lg:text-left space-y-6">
            <div class="text-5xl lg:text-6xl xl:text-7xl font-bold tracking-tight">
              <span class="gradient-text block mb-2">
                iOS Club
              </span>
              <span class="text-gray-700 dark:text-gray-300 block">
                of XAUAT
              </span>
            </div>

            <p class="text-lg md:text-xl lg:text-2xl text-gray-600 dark:text-gray-300 max-w-2xl">
              西安建筑科技大学iOS众创空间俱乐部
            </p>

            <p class="text-base md:text-lg lg:text-xl text-gray-500 dark:text-gray-500 italic">
              "Stay hungry, stay foolish"
            </p>

            <div class="pt-4 flex flex-col sm:flex-row gap-4 justify-center lg:justify-start">
              <n-button
                  type="primary"
                  size="large"
                  round
                  @click="scrollToAbout"
                  class="px-8"
              >
                了解更多
              </n-button>
              <n-button
                  size="large"
                  round
                  @click="router.push('/login')"
                  class="px-8"
                  secondary
              >
                登录/注册
              </n-button>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- About Section -->
    <section
        id="about"
        class="min-h-screen py-20 px-4 sm:px-6 lg:px-8"
    >
      <div class="max-w-7xl mx-auto">
        <h2 class="text-3xl md:text-4xl lg:text-5xl font-bold text-center mb-16 text-gray-900 dark:text-white">
          关于我们
        </h2>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          <div
              v-for="(card, index) in cards"
              :key="index"
              class="rounded-2xl border border-gray-200 dark:border-gray-800 transition-all duration-300 hover:shadow-xl p-6"
              :class="card.class"
          >
            <div class="text-5xl mb-4">
              <Icon :icon="card.iconName" :size="48" :color="card.color" />
            </div>
            <h3 class="text-xl font-semibold mb-3 text-gray-900 dark:text-white">
              {{ card.title }}
            </h3>
            <p class="text-gray-600 dark:text-gray-400">
              {{ card.content }}
            </p>
          </div>
        </div>

        <div class="mt-16 text-center">
          <n-button
              size="large"
              round
              @click="router.push('/About')"
              secondary
          >
            查看更多详情
          </n-button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import {useRouter} from 'vue-router'
import {NButton} from 'naive-ui'
import { Icon } from '@iconify/vue'

interface Card {
  iconName: string
  title: string
  content: string
  class: string
  color?: string
}

const router = useRouter()

const cards: Card[] = [
  {
    iconName: 'heroicons:academic-cap-20-solid',
    title: "我们是谁?",
    content: "我们是由Apple公司资金支持，受学管和西安建筑科技大学大学生创新创业实践中心指导的创新创业类社团。",
    class: "hover:border-blue-500",
    color: "#3B82F6" // blue-500
  },
  {
    iconName: 'heroicons:bolt-solid',
    title: "和iOS Club一起结伴同行",
    content: "不管是零基础的小白还是大神，只要你有兴趣，这里就是你的天堂",
    class: "hover:border-purple-500",
    color: "#8B5CF6" // purple-500
  },
  {
    iconName: 'mdi:account-group',
    title: "iOS Club,不止iOS",
    content: "我们不止只有iOS，西建大iOS Club是一个跨专业与课堂的数码编程爱好者社团",
    class: "hover:border-green-500",
    color: "#10B981" // green-500
  },
  {
    iconName: 'heroicons:chat-bubble-left-ellipsis-solid',
    title: "和 iOS Club 一起玩",
    content: "活动覆盖编程和数码，不仅有最新数码设备体验活动，还有大咖沙龙和Apple官方活动",
    class: "hover:border-yellow-500",
    color: "#F59E0B" // yellow-500
  },
  {
    iconName: 'lucide:code-xml',
    title: "合作软件开发",
    content: "加入我们，和志同道合的iMember一起合作开发，一起创造世界",
    class: "hover:border-pink-500",
    color: "#EC4899" // pink-500
  },
  {
    iconName: 'ion:trending-up',
    title: "全校数码编程爱好者,联合起来！",
    content: "我们意图打造全校最大的科技社区，快来加入我们成为一名iMember吧",
    class: "hover:border-indigo-500",
    color: "#6366F1" // indigo-500
  }
]

const scrollToAbout = () => {
  const element = document.getElementById('about')
  if (element) {
    element.scrollIntoView({behavior: 'smooth'})
  }
}
</script>

<style scoped>
/* 原版渐变文字 */
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
  background-size: 150% 150%;
  animation: gradientShift 8s ease infinite;
}

/* 渐变动画 */
@keyframes gradientShift {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}

/* 大屏优化 */
@media (min-width: 1280px) {
  .gradient-text {
    background-size: 200% 200%;
  }
}
</style>
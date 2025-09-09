<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <!-- 主要内容区 -->
    <div class="container mx-auto px-4 max-w-7xl">
      <!-- 头部区域 - 移动端标题改为"iOS Club活动" -->
      <PageStart
          :title="isMobile ? 'iOS Club 活动' : 'iOS Club 社团活动'"
          subtitle="Think Different"
          :img="appleLogo"
          gradient-class="bg-gradient-to-r from-purple-600 to-pink-600"
      />

      <!-- 活动卡片区域 -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-8 pb-16 ml-4 mr-4">
        <n-card
            v-for="(item, index) in cards"
            :key="index"
            hoverable
            class="group cursor-pointer animate-slide-up"
            :style="`animation-delay: ${index * 100}ms`"
            @click="handleCardClick(item)"
        >
          <template #cover>
            <div class="h-48 bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center overflow-hidden">
              <img
                  :src="item.imageUrl"
                  alt="Event Icon"
                  class="w-24 h-24 object-contain group-hover:scale-110 transition-transform duration-500"
              />
            </div>
          </template>

          <div class="p-6 space-y-4">
            <h2 class="text-2xl font-semibold text-gray-900 group-hover:text-purple-600 transition-colors duration-300">
              {{ item.title }}
            </h2>
            <p class="text-gray-600 leading-relaxed">
              {{ item.content }}
            </p>
            <div class="flex items-center text-purple-600 font-medium">
              <span>了解更多</span>
              <svg class="w-5 h-5 ml-2 group-hover:translate-x-2 transition-transform duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
              </svg>
            </div>
          </div>
        </n-card>
      </div>
    </div>

    <!-- 背景装饰 -->
    <div class="fixed top-0 left-0 w-full h-full pointer-events-none overflow-hidden -z-10">
      <div class="absolute -top-40 -right-40 w-80 h-80 bg-purple-100 rounded-full blur-3xl opacity-30 animate-float"></div>
      <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-pink-100 rounded-full blur-3xl opacity-30 animate-float-delayed"></div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { NCard } from 'naive-ui'
import PageStart from "../components/PageStart.vue";

// 导入图片
import appleLogo from '../assets/Centre/AppleLogo.jpg'
import visionProImage from '../assets/other/vision_pro.jpg'
import launchEventImage from '../assets/other/launch_event.jpg'
import classImage from '../assets/other/class.jpg'
import partyImage from '../assets/other/you_yuan_hui.jpg'

// 移动端判断状态
const isMobile = ref(false)

// 检查窗口尺寸
const checkScreenSize = () => {
  // 小于768px视为移动端
  isMobile.value = window.innerWidth <= 768
}

// 挂载时检查一次
onMounted(() => {
  checkScreenSize()
  // 监听窗口大小变化
  window.addEventListener('resize', checkScreenSize)
})

// 卸载时移除监听
onUnmounted(() => {
  window.removeEventListener('resize', checkScreenSize)
})

const cards = ref([
  {
    imageUrl: visionProImage,
    title: 'iOS Club和你一起体验最新产品',
    content: 'iOS Club与许多企业进行合作，我们将带您体验最新的设备与最新应用',
    url: '/Article/VisionPro'
  },
  {
    imageUrl: launchEventImage,
    title: 'iOS Club和你一起看发布会',
    content: 'iOS Club和你一起见证未来。未来已来，你来不来？',
    url: '/Article/PressConference'
  },
  {
    imageUrl: classImage,
    title: 'iOS Club和你一起夜校培训',
    content: 'iOS Club携手专业讲师，和你一起开启夜校培训，带你深入学习前沿知识',
    url: '/Article/Class'
  },
  {
    imageUrl: partyImage,
    title: 'iOS Club和你一起进行游园会',
    content: 'iOS Club和你一起漫步校园，在游园会中打卡互动、畅享欢乐时光',
    url: '/Article/Party'
  }
])

const handleCardClick = (item) => {
  if (item.url) {
    window.open(item.url, '_blank')
  }
}
</script>

<style scoped>
/* 动画定义 */
@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slide-up {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes gradient {
  0%, 100% {
    background-position: 0 50%;
  }
  50% {
    background-position: 100% 50%;
  }
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px);
  }
  50% {
    transform: translateY(-20px);
  }
}

.animate-slide-up {
  opacity: 0;
  animation: slide-up 0.8s ease-out forwards;
}

.animate-float {
  animation: float 6s ease-in-out infinite;
}

.animate-float-delayed {
  animation: float 6s ease-in-out 3s infinite;
}

/* NaiveUI 卡片自定义样式 */
:deep(.n-card) {
  border: none;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

:deep(.n-card:hover) {
  transform: translateY(-8px);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

:deep(.n-card__cover) {
  background: linear-gradient(135deg, #fafafa 0%, #f3f4f6 100%);
}

:deep(.n-divider) {
  margin: 0;
  background: linear-gradient(90deg, transparent, #e5e7eb, transparent);
}
</style>
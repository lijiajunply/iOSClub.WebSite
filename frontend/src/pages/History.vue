<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 主要内容区 -->
    <div class="container mx-auto px-4 max-w-7xl">
      <!-- 头部区域  -->
      <PageStart
          title="iOS Club 历史"
          subtitle="历史是时代的见证"
          :img="historyImage"
          gradient-class="bg-gradient-to-r from-blue-600 to-green-500"
      />

      <!-- 内容卡片区域 -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-8 pb-16 ml-4 mr-4">
        <n-card
            v-for="(item, index) in cards"
            :key="index"
            hoverable
            class="group cursor-pointer animate-slide-up"
            :style="`animation-delay: ${index * 100}ms`"
            @click="handleCardClick(item)"
        >

          <div class="p-6 space-y-4">
            <h2 class="text-2xl font-semibold text-gray-900 group-hover:text-blue-600 transition-colors duration-300">
              {{ item.title }}
            </h2>
            <p class="text-gray-600 leading-relaxed">
              {{ item.content }}
            </p>
            <div class="flex items-center text-blue-600 font-medium">
              <span>了解更多</span>
              <svg class="w-5 h-5 ml-2 group-hover:translate-x-2 transition-transform duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
              </svg>
            </div>
          </div>
        </n-card>
      </div>
    </div>
    
    <div class="fixed top-0 left-0 w-full h-full pointer-events-none overflow-hidden -z-10">
      <div class="absolute -top-40 -right-40 w-80 h-80 bg-blue-100 rounded-full blur-3xl opacity-30 animate-float"></div>
      <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-green-100 rounded-full blur-3xl opacity-30 animate-float-delayed"></div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { NCard } from 'naive-ui'
import { useRouter } from 'vue-router'
import PageStart from "../components/PageStart.vue";

import historyImage from '../assets/Centre/History.jpg';

const router = useRouter()

const cards = ref([
  {
    imageUrl: '', // 可以替换为实际图片路径
    title: "总述",
    content: "我们社团从何而来，又要到哪里去，这是一个值得思考的问题",
    url: "/Article/History-overview"
  },
  {
    imageUrl: '', // 可以替换为实际图片路径
    title: "创社时代",
    content: "社团的开始总是有着一段传奇故事",
    url: "/Article/History-founding"
  },
  {
    imageUrl: '', // 可以替换为实际图片路径
    title: "邵韩之治",
    content: "解决完社团的初步建设，接下来就要进行社团的发展了",
    url: "/Article/History-shaohan-reign"
  },
  {
    imageUrl: '', // 可以替换为实际图片路径
    title: "活动室变迁",
    content: "沧海桑田，我们正在努力",
    url: "/Article/History-Room"
  }
]);

const handleCardClick = (item) => {
  if (item.url) {
    router.push(item.url);
  }
};
</script>

<style scoped>
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

.gradient-text {
  background-clip: text;
  -webkit-background-clip: text;
  color: transparent;
  background-image: linear-gradient(to right, #4285f4, #34a853, #fbbc05, #ea4335);
}
</style>

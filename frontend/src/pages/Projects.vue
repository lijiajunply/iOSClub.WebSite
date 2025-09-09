<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <PageStart
        :title="isMobile ? 'iOS Club 项目' : 'iOS Club 社团项目'"
        subtitle="创造世界的新方式"
        gradient-class="bg-gradient-to-r from-orange-500 via-pink-500 to-pink-600"
        :img="appleLogo"
    />

    <!-- 项目卡片容器 - 优化间距和布局 -->
    <div class="flex justify-center items-center py-8 px-4">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-8 w-full max-w-6xl">
        <div
            v-for="(item, index) in cards"
            :key="index"
            class="group cursor-pointer animate-slide-up"
            :style="`animation-delay: ${index * 100}ms`"
            @click="() => window.open(item.url, '_blank')"
        >
          <div class="relative bg-white rounded-lg overflow-hidden shadow-md hover:shadow-xl transition-shadow duration-300">
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
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import PageStart from "../components/PageStart.vue";
import appleLogo from '../assets/Centre/AppleLogo.jpg'

// 检测是否为移动设备
const isMobile = ref(window.innerWidth < 640);

// 监听窗口大小变化
const handleResize = () => {
  isMobile.value = window.innerWidth < 640;
};

onMounted(() => {
  window.addEventListener('resize', handleResize);
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
});

const cards = [
  {
    title: "Old8Lang",
    content: "西建大iOS Club出品的脚本语言解释器 使用C#开发",
    url: "https://gitee.com/XAUATiOSClub/Old8Lang"
  },
  {
    title: "西建大iOS Club官网",
    content: "西建大iOS Club官网，使用Blazor开发",
    url: "https://gitee.com/XAUATiOSClub/iOSClub.Web"
  },
  {
    title: "代码综合平台",
    content: "将在线编辑器，编译器，OJ系统结合起来的代码综合平台，使用FastApi+Vue开发",
    url: "https://gitee.com/XAUATiOSClub/LetCoding"
  },
  {
    title: "文档生成平台",
    content: "将各式各样的文档进行生成，使用WPF+Asp.net Webapi开发",
    url: "https://gitee.com/XAUATiOSClub/DocumentMaking"
  },
  {
    title: "滑稽账本",
    content: "用.NET MAUI开发的账本App",
    url: "https://gitee.com/XAUATiOSClub/huaji-ledger"
  },
  {
    title: "西建导航",
    content: "用SwiftUI开发的西建大校园导航App",
    url: "https://gitee.com/XAUATiOSClub/XAUATNav"
  }
]
</script>

<style scoped>
.animate-slide-up {
  animation: slide-up 0.6s ease-out forwards;
  opacity: 0;
  transform: translateY(30px);
}

@keyframes slide-up {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>

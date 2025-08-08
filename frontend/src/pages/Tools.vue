<template>
  <div class="min-h-screen bg-gray-50">
    <div class="container mx-auto px-4 max-w-7xl">
      <PageStart
          title="iOS App 工具集"
          subtitle="西建大iOS Club 出品"
          :img="toolsImage"
          gradient-class="bg-gradient-to-r from-blue-600 to-green-500"
      />

      <div class="grid grid-cols-1 gap-6 pb-16 ml-4 mr-4 mt-8">
        <a
            v-for="(link, index) in models"
            :key="link.key"
            :href="link.url"
            target="_blank"
            class="group cursor-pointer animate-slide-up"
            :style="`animation-delay: ${index * 100}ms`"
            :title="link.description"
        >
          <n-card hoverable class="w-full">
            <div class="flex items-center justify-between p-6">
              <div class="flex items-center gap-4">

                <!-- 统一使用图片图标 -->
                <img
                    :src="fixImageUrl(link.icon)"
                    :style="{ height: '28px', width: '28px', borderRadius: '6px' }"
                    :alt="`${link.name}的图标`"
                    @error="(e) => handleImageError(e, link)"
                    data-debug="image-icon"
                />

                <span class="text-xl font-semibold text-gray-900 group-hover:text-blue-600 transition-colors duration-300">
                  {{ link.name }}
                </span>
              </div>
              <span class="text-gray-600 group-hover:text-blue-500 transition-colors duration-300">
                {{ link.description || '点击访问' }}
                <svg class="w-5 h-5 ml-2 inline group-hover:translate-x-2 transition-transform duration-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
                </svg>
              </span>
            </div>
          </n-card>
        </a>

        <div v-if="models.length === 0" class="empty-state animate-slide-up flex flex-col items-center justify-center p-16">
          <img
              src="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg"
              alt="空状态"
              :style="{ padding: '20px' }"
          />
          <div class="empty-description text-gray-600 mt-4">社团还没加入工具</div>
        </div>
      </div>
    </div>

    <div class="fixed top-0 left-0 w-full h-full pointer-events-none overflow-hidden -z-10">
      <div class="absolute -top-40 -right-40 w-80 h-80 bg-blue-100 rounded-full blur-3xl opacity-30 animate-float"></div>
      <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-green-100 rounded-full blur-3xl opacity-30 animate-float-delayed"></div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { NCard } from 'naive-ui';
import PageStart from "@/components/PageStart.vue";

import toolsImage from '../assets/Centre/AppleLogo.jpg';

// 本地静态数据（替代API请求）
const localData = {
  "key": "64efa33d787e9704abb775090af58749",
  "name": "社团出品",
  "description": "iOS Club 出品的App",
  "icon": "pingguo",
  "index": 3,
  "links": [
    {
      "key": "04c8f1afa7e6d5e290182b796d43d1f1",
      "name": "封面生成",
      "icon": "https://coverview.xauat.site//assets/logo-BF5ZA9n8.png",
      "url": "https://coverview.xauat.site/",
      "description": "",
      "index": 11
    },
    {
      "key": "3e8174acec310b710f09e28fc1714dfb",
      "name": "iLibrary",
      "icon": "https://i.ibb.co/mwnQc9m/wenjianjia.png",
      "url": "https://lib.xauat.site/",
      "description": "iOS 电子图书馆",
      "index": 11
    },
    {
      "key": "5667f32125841ffeb438f2eeded793c2",
      "name": "iOS AI",
      "icon": "https://i.ibb.co/Z1NN8T8S/ai.png",
      "url": "https://gpt.xauat.site",
      "description": "iOS Club出品的AI产品",
      "index": 1
    },
    {
      "key": "60d2fb9eaa07897822e19605f83da28a",
      "name": "AI API服务平台",
      "icon": "https://i.ibb.co/hFcFvxc8/AI.png",
      "url": "https://newapi.xauat.site/",
      "description": "AI服务平台",
      "index": 11
    },
    {
      "key": "6b989a9c1b4fe096d41c45507cafd15c",
      "name": "建大导航",
      "icon": "https://i.ibb.co/B5YLGjDr/daohang.png",
      "url": "https://link.xauat.site/",
      "description": "将建大各种东西收集起来",
      "index": 2
    },
    {
      "key": "8e09dfebc9a6e77c8e7bc22745d053d2",
      "name": "社团官网",
      "icon": "https://i.ibb.co/xqnx3m1d/zhizhang.png",
      "url": "https://www.xauat.site",
      "description": "",
      "index": 0
    },
    {
      "key": "c446db8c329b0d603d904fb533b498d1",
      "name": "建大百科",
      "icon": "https://i.ibb.co/qYQNKZLy/wenjian-1.png", 
      "url": "https://wiki.xauat.site",
      "description": "囊括所有资源",
      "index": 3
    },
    {
      "key": "ed156e19af36ba921a792908fad165ac",
      "name": "Markdown工具",
      "icon": "https://i.ibb.co/SLXZw6N/wenjian.png", 
      "url": "https://mark.xauat.site/",
      "description": "Markdown转公众号",
      "index": 4
    },
    {
      "key": "922ec40253e088738c3e07f4243abbd9",
      "name": "新生代培养计划",
      "icon": "https://i.ibb.co/XQfXp5B/icon.png", 
      "url": "https://plan.xauat.site/",
      "description": "西建大 iOS Club 新生代培养计划",
      "index": 8
    }
  ]
};

const models = ref(localData.links);
const FALLBACK_ICON = toolsImage;

// 修复图片URL中的重复斜杠问题
const fixImageUrl = (url) => {
  return url.replace(/([^:]\/)\/+/g, '$1');
};

// 图标加载失败时替换为备用图标
const handleImageError = (event, link) => {
  if (event.target.src === FALLBACK_ICON) return;

  console.debug(`[${link.name}]图标加载失败，使用备用图标`, {
    failedUrl: event.target.src,
    fallback: FALLBACK_ICON
  });
  event.target.src = FALLBACK_ICON;
};
</script>

<style scoped>
/* 移除字体图标相关样式（不再使用） */

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
  transform: translateY(-4px);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

a {
  text-decoration: none;
}

/* 确保备用图标显示正常 */
img[src$="AppleLogo.jpg"] {
  background-color: #f0f0f0;
  padding: 2px;
}
</style>
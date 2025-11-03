<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <div class="container mx-auto px-4 max-w-7xl">
      <PageStart
          :title="isMobile ? 'iOS Club 工具' : 'iOS Club 社团工具'"
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
          <n-card hoverable class="w-full dark:bg-neutral-800 dark:text-gray-100">
            <div class="flex items-center justify-between p-6">
              <div class="flex items-center gap-4">
                <template v-if="!link.icon.startsWith('http')">
                  <IconFont
                      :type="link.icon"
                      class="text-[28px] dark:text-gray-100"
                  />
                </template>
                <template v-else>
                  <img
                      :src="fixImageUrl(link.icon)"
                      :style="{ height: '28px', width: '28px', borderRadius: '6px' }"
                      :alt="`${link.name}的图标`"
                      @error="(e) => handleImageError(e, link)"
                      data-debug="image-icon"
                  />
                </template>

                <span class="text-lg font-semibold dark:text-gray-100">{{ link.name }}</span>
              </div>
              <!-- 移动端隐藏描述文字，仅保留箭头图标 -->
              <span class="text-gray-600 group-hover:text-blue-500 transition-colors duration-300">
                <span class="hidden sm:inline">{{ link.description || '点击访问' }}</span>
                <svg class="w-5 h-5 ml-2 inline group-hover:translate-x-2 transition-transform duration-300" fill="none"
                     stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
                </svg>
              </span>
            </div>
          </n-card>
        </a>

        <div v-if="models.length === 0"
             class="empty-state animate-slide-up flex flex-col items-center justify-center p-16">
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
      <div
          class="absolute -top-40 -right-40 w-80 h-80 bg-blue-100 rounded-full blur-3xl opacity-30 animate-float"></div>
      <div
          class="absolute -bottom-40 -left-40 w-80 h-80 bg-green-100 rounded-full blur-3xl opacity-30 animate-float-delayed"></div>
    </div>

  </div>
</template>

<script setup lang="ts">
import '../lib/iconfont'
import {ref, onMounted} from 'vue';
import {NCard} from 'naive-ui';
import PageStart from "../components/PageStart.vue";
import {LinkModel, ToolService} from "../services/ToolService";
import toolsImage from '/assets/Centre/AppleLogo.jpg';
import IconFont from "../components/IconFont.vue";

const models = ref<LinkModel[]>([] as LinkModel[]);

// 检测是否为移动设备
const isMobile = ref(window.innerWidth < 640);

// 修复图片URL中的重复斜杠问题
const fixImageUrl = (url: string) => {
  return url.replace(/([^:]\/)\/+/g, '$1');
};

// 图标加载失败时替换为备用图标
const handleImageError = (event: any, link: any) => {
  if (event.target.src === toolsImage) return;

  console.debug(`[${link.name}]图标加载失败，使用备用图标`, {
    failedUrl: event.target.src,
    fallback: toolsImage
  });
  event.target.src = toolsImage;
};

onMounted(async () => {
  const res = await ToolService.getTools();
  models.value = res.links;
  console.log(res.links)
})
</script>

<style scoped>
@import url('//at.alicdn.com/t/c/font_4612528_md4hjwjgcb.css');

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

/* 移动端优化调整 */
@media (max-width: 640px) {
  .flex.items-center.justify-between {
    padding: 4px 12px;
  }

  .text-xl {
    font-size: 16px;
  }

  .text-\[28px\] {
    font-size: 22px;
  }
}
</style>
<template>
  <PageStart title="iOS Club 社团项目" subtitle="创造世界的新方式"
             gradient-class="bg-gradient-to-r from-orange-500 via-pink-500 to-pink-600" :img="appleLogo"/>

  <div class="flex justify-center items-center py-16">
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 w-full max-w-6xl px-6">
      <div
          v-for="(item, index) in cards"
          :key="index"
          class="group"
      >
        <a :href="item.url" target="_blank" class="block">
          <div class="relative bg-white/80 backdrop-blur-md rounded-2xl p-8 h-64
                      border border-gray-200/60 shadow-sm
                      hover:shadow-xl hover:shadow-gray-200/40 hover:border-gray-300/60
                      hover:-translate-y-1 hover:bg-white/90
                      transition-all duration-300 ease-out">

            <!-- 装饰性渐变 -->
            <div class="absolute inset-0 rounded-2xl bg-gradient-to-br
                        from-purple-500/5 via-transparent to-pink-500/5
                        opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>

            <div class="relative h-full flex flex-col">
              <h2 class="text-2xl font-medium text-gray-900 mb-4
                         group-hover:text-transparent group-hover:bg-clip-text
                         group-hover:bg-gradient-to-r group-hover:from-purple-600
                         group-hover:to-pink-600 transition-all duration-300">
                {{ item.title }}
              </h2>

              <p class="text-gray-600 leading-relaxed line-clamp-3 flex-grow">
                {{ item.content }}
              </p>

              <!-- 箭头指示器 -->
              <div class="mt-4 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3"/>
                </svg>
              </div>
            </div>
          </div>
        </a>
      </div>
    </div>
  </div>
</template>

<script setup>
import PageStart from "@/components/PageStart.vue";
import appleLogo from '../assets/Centre/AppleLogo.jpg'

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
    title: "LuckyFish.Json",
    content: "用C#开发的json解析器",
    url: "https://gitee.com/XAUATiOSClub/LuckyFish.Json"
  },
  {
    title: "建大Wiki小程序",
    content: "采用 Uni - app 框架构建的Wiki小程序",
    url: "https://gitee.com/XAUATiOSClub/uniapp-ioswiki"
  },
  {
    title: "新生代培养计划",
    content: "基于前端技术栈的培训方案",
    url: "https://gitee.com/XAUATiOSClub/uniapp-ioswiki"
  },
  {
    title: "XAUAT 自动评教脚本",
    content: "基于 Tampermonkey 的用户脚本",
    url: "https://gitee.com/XAUATiOSClub/Xauat-automatic-teaching-evaluation"
  }
];
</script>

<style scoped>
@reference 'tailwindcss';
</style>
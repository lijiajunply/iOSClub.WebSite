---
title: 社团活动 - 西建大iOS Club
layout: page
---


<!-- 在 Vitepress 项目的 .md 文件中使用 -->
<script setup>
const projects = [
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
  }
];
</script>

  <!-- 标题区域 -->
  <div class="text-center py-12">
    <img 
      src="/Centre/AppleLogo.jpg" 
      alt="iOS Club Logo"
      class="mx-auto w-[40%] max-w-xs" 
    />
    <div class="flex justify-center mt-8 mb-4">
      <h1 class="gradient-text text-4xl md:text-5xl font-bold">
        iOS Club 社团项目
      </h1>
    </div>
    <p class="text-xl font-semibold text-gray-500">
      创造世界的新方式
    </p>
  </div>

  <!-- 项目卡片区域 -->
  <div class="flex justify-center">
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 w-[95%] my-10 max-w-7xl">
      <a 
        v-for="(item, index) in projects" 
        :key="index"
        :href="item.url" 
        target="_blank"
        class="transition-transform duration-300 hover:scale-[1.02] hover:shadow-lg"
      >
        <div class="card rounded-xl p-6 bg-gray-50 border-none h-48">
          <h3 class="text-xl font-bold">{{ item.title }}</h3>
          <p class="text-gray-500 mt-2">{{ item.content }}</p>
        </div>
      </a>
    </div>
  </div>

<style>
/* 渐变文字样式 */
.gradient-text {
  background: linear-gradient(45deg, #ff9a9e, #fad0c4, #fbc2eb, #a6c1ee);
  background-size: 300% 300%;
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
  animation: gradient 5s ease infinite;
}

@keyframes gradient {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

/* 卡片悬停效果 */
.card {
  transition: all 0.3s ease;
  box-shadow: 0 4px 6px rgba(0,0,0,0.05);
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 20px rgba(0,0,0,0.1);
}
</style>
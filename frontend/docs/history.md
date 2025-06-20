---
title: 社团活动 - 西建大iOS Club
layout: page
---

  <div class="flex flex-col min-h-screen">
    <div class="text-center py-12">
      <!-- 历史图片 -->
      <img
        src="/Centre/History.jpg"
        alt="iOS Club历史"
        class="h-30 max-w-xl mx-auto"
      />
      <!-- 渐变标题 -->
      <div class="flex justify-center items-center my-6">
        <h1 class="text-cilp text-4xl md:text-5xl font-bold">
          iOS Club 社团历史
        </h1>
      </div>
      <!-- 引用语 -->
      <p class="text-xl text-gray-500 font-medium">
        "历史是时代的见证"
      </p>
    </div>
    <!-- 历史卡片网格 -->
    <div class="flex justify-center items-center pb-16">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 w-[80%] max-w-6xl my-12">
        <a 
          v-for="card in cards"
          :key="card.title"
          :href="card.url"
          class="transition-transform duration-300 hover:scale-[1.02]"
        >
          <div class="card bg-white rounded-xl shadow-md p-6 h-48 flex flex-col justify-center">
            <h2 class="text-xl font-bold text-gray-800 mb-3">{{ card.title }}</h2>
            <p class="text-gray-500">{{ card.content }}</p>
          </div>
        </a>
      </div>
    </div>
  </div>

<script setup>
const cards = [
  {
    title: "总述",
    content: "我们社团从何而来，又要到哪里去，这是一个值得思考的问题",
    url: "/Article/History-Overview"
  },
  {
    title: "创社时代",
    content: "社团的开始总是有着一段传奇故事",
    url: "/Article/History-Founding"
  },
  {
    title: "邵韩之治",
    content: "解决完社团的初步建设，接下来就要进行社团的发展了",
    url: "/Article/History-Shao Han's Reign"
  }
];
</script>
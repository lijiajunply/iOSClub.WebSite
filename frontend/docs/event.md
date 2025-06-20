---
title: 社团活动
layout: page
---

<script setup>
const cards = [
  {
    title: 'iOS Club和你一起体验最新产品',
    content: 'iOS Club与许多企业进行合作，我们将带您体验最新的设备与最新应用',
    url: '/article/vision'
  },
  {
    title: 'iOS Club和你一起看发布会',
    content: 'iOS Club和你一起见证未来。未来已来，你来不来？',
    url: '/article/press-conference'
  }
]
</script>

<div class="flex flex-col items-center">
  <img src="/Centre/AppleLogo.jpg" class="w-40 md:w-60 lg:w-80 my-6" alt="Apple Logo" />
  
  <span class="text-clip text-transparent text-3xl md:text-4xl lg:text-5xl font-bold mb-4">
    iOS Club 社团活动
  </span>
  
  <p class="text-xl md:text-2xl text-gray-600 dark:text-gray-300 font-semibold mb-10">Think Different</p>
</div>

<div class="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-4xl mx-auto my-10 px-4">
  <div v-for="card in cards" :key="card.title" class="transform transition-all duration-300 hover:scale-105">
    <a :href="card.url" target="_blank" class="block h-full">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 h-full hover:shadow-xl transition-shadow duration-300">
        <h2 class="text-xl font-semibold mb-2 text-gray-800 dark:text-white">{{ card.title }}</h2>
        <p class="text-gray-600 dark:text-gray-300 mt-4">{{ card.content }}</p>
      </div>
    </a>
  </div>
</div>
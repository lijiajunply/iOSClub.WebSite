<template>
  <div class="mx-auto max-w-6xl px-4 py-8">
    <!-- 标题区 -->
    <div class="text-center mb-12">
      <div class="flex justify-center">
        <img src="/Centre/Article.jpg" alt="Article header" class="mb-6 rounded-xl shadow-lg" />
      </div>
      <h1 class="font-bold text-4xl mb-4">
        <span class="bg-clip-text text-transparent bg-gradient-to-r from-blue-500 to-purple-600">
          iOS Club 技术博客
        </span>
      </h1>
      <p class="text-xl text-gray-500 font-medium">记录每一个思维的并发点</p>
    </div>

    <!-- 文章卡片 -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-16">
      <div v-for="article in rssModels" :key="article.url" class="group">
        <a :href="article.url" target="_blank" rel="noopener">
          <div class="h-full bg-gray-50 rounded-xl overflow-hidden p-6 transition-all duration-300 group-hover:shadow-xl group-hover:scale-[1.02]">
            <div class="flex flex-col items-center justify-center h-full">
              <h3 class="text-xl font-bold mb-4 text-center">{{ article.title }}</h3>
              <img 
                :src="article.image || '/placeholder.png'" 
                :alt="article.title" 
                class="w-full max-w-xs object-contain"
              />
            </div>
          </div>
        </a>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-if="rssModels.length === 0" class="text-center py-12">
      <img 
        src="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg" 
        alt="No articles"
        class="mx-auto w-40 mb-4"
      />
      <p class="text-gray-500">暂无文章</p>
    </div>

    <!-- 订阅区域 -->
    <div v-if="entries.length > 0" class="py-8">
      <h2 class="text-2xl font-bold mb-6">更多订阅</h2>
      <div class="space-y-3">
        <a 
          v-for="entry in entries" 
          :key="entry.link[0].href"
          :href="entry.link[0].href"
          target="_blank" 
          rel="noopener"
          class="block p-3 rounded-lg transition-all duration-200 hover:bg-gray-50 hover:shadow-sm"
        >
          <div class="flex justify-between items-center">
            <span class="text-lg">{{ entry.title }}</span>
            <span class="text-gray-500">{{ formatDate(entry.updated) }}</span>
          </div>
        </a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

// RSS 文章模型
const rssModels = ref([])

// Atom feed 订阅模型
const entries = ref([])

// 格式化日期
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toISOString().split('T')[0]
}

// 获取 RSS 文章
const fetchRssArticles = async () => {
  try {
    const response = await fetch('https://test.xauat.site/feeds/MP_WXS_3226711201.json')
    const data = await response.json()
    return data?.items?.map(item => ({
      url: item.url || '',
      image: item.image || '',
      title: item.title || ''
    })) || []
  } catch (error) {
    console.error('Error fetching RSS articles:', error)
    return []
  }
}

// 获取 Atom 订阅
const fetchAtomEntries = async () => {
  try {
    const response = await fetch('https://test.xauat.site/feeds/all.atom')
    const xmlText = await response.text()
    
    // XML 解析器 (使用浏览器内置 DOMParser)
    const parser = new DOMParser()
    const xmlDoc = parser.parseFromString(xmlText, "application/xml")
    
    // 解析 entries
    return Array.from(xmlDoc.querySelectorAll('entry')).map(entry => {
      const title = entry.querySelector('title')?.textContent || ''
      const links = Array.from(entry.querySelectorAll('link')).map(link => ({
        href: link.getAttribute('href') || ''
      }))
      const updated = entry.querySelector('updated')?.textContent || ''
      
      return { title, link: links, updated }
    })
  } catch (error) {
    console.error('Error fetching Atom feed:', error)
    return []
  }
}

onMounted(async () => {
  rssModels.value = await fetchRssArticles()
  entries.value = await fetchAtomEntries()
})
</script>
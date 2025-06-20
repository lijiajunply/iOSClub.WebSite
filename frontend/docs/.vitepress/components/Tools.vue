<!-- 在 docs/.vitepress/theme/components/Tools.vue -->
<template>
  <div class="page-container py-8">
    <div class="text-center mb-12">
      <span class="text-clip text-transparent text-3xl md:text-4xl lg:text-4xl font-bold">
          iOS App - 西建大iOS Club
        </span>
    </div>
    
    <div class="flex justify-center">
      <div class="w-full max-w-4xl">
        <template v-if="models.length">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <a 
              v-for="link in models" 
              :key="link.name"
              :href="link.url" 
              target="_blank"
              class="tool-link"
              :title="link.description"
            >
              <div class="flex items-center justify-between p-4 rounded-xl transition-all">
                <div class="flex items-center">
                  <div v-if="link.icon" class="mr-3">
                    <img 
                      v-if="link.icon.startsWith('http')" 
                      :src="link.icon" 
                      alt="Icon" 
                      class="w-6 h-6 rounded object-cover"
                    />
                    <IconFont v-else :type="`#icon-${link.icon}`" class="w-6 h-6 rounded object-cover"/>
                  </div>
                  <div v-else class="mr-3">
                    <div class="flex items-center justify-center bg-gray-200 rounded w-6 h-6">
                      <i class="iconfont icon-link text-xs"></i>
                    </div>
                  </div>
                  <span class="btn-title font-medium">{{ link.name }}</span>
                </div>
                <span class="btn-description text-sm ml-2">{{ link.description }}</span>
              </div>
            </a>
          </div>
        </template>
        
        <template v-else>
          <div class="text-center py-12">
            <img 
              src="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg" 
              alt="Empty" 
              class="mx-auto mb-4 w-24 h-24"
            />
            <p class="text-gray-500 text-lg">社团还没加入</p>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import IconFont from '../components/IconFont.vue'

// 定义类型
const linkModel = {
  name: '',
  url: '',
  description: '',
  icon: ''
}

// 响应式数据
const models = ref([])

// 获取数据
const fetchData = async () => {
  try {
    const response = await fetch('https://link.xauat.site/api/Link/GetCategory/社团出品')
    if (!response.ok) throw new Error('Network response was not ok')
    
    const data = await response.json()
    models.value = data.links || []
  } catch (error) {
    console.error('数据获取失败:', error)
    models.value = []
  }
}

// 动态加载图标字体
const loadIconFont = () => {
  const existingScript = document.querySelector('script[src*="at.alicdn.com"]')
  if (existingScript) return
  
  const script = document.createElement('script')
  script.src = 'https://at.alicdn.com/t/c/font_4612528_md4hjwjgcb.js'
  document.head.appendChild(script)
}

// 组件挂载时获取数据
onMounted(() => {
  fetchData()
  loadIconFont()
})
</script>

<style scoped>
.tool-link {
  display: block;
  transition: transform 0.2s ease, border-color 0.2s ease;
  cursor: pointer;
  border: 1px solid #eaeaea;
  border-radius: 10px;
  overflow: hidden;
}

.tool-link:hover {
  transform: translateY(-2px);
  border-color: #3b82f6;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

.btn-title {
  color: #1f2937;
}

.btn-description {
  color: #6b7280;
}
</style>
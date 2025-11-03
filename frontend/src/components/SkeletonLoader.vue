<template>
  <div :class="['animate-pulse', themeClass]">
    <!-- 卡片骨架 -->
    <div v-if="type === 'card'" class="rounded-xl overflow-hidden border border-gray-200 dark:border-gray-700">
      <div class="h-32 bg-gray-200 dark:bg-gray-700"></div>
      <div class="p-4">
        <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-4"></div>
        <div class="space-y-2">
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-5/6"></div>
        </div>
        <div class="flex space-x-2 mt-4">
          <div class="h-8 w-20 bg-gray-200 dark:bg-gray-700 rounded"></div>
          <div class="h-8 w-20 bg-gray-200 dark:bg-gray-700 rounded"></div>
        </div>
      </div>
    </div>

    <!-- 列表骨架 -->
    <div v-else-if="type === 'list'" class="space-y-4">
      <div v-for="i in count" :key="i" class="flex items-center p-4 rounded-xl">
        <div class="rounded-full bg-gray-200 dark:bg-gray-700 h-12 w-12"></div>
        <div class="ml-4 flex-1 space-y-2">
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
        </div>
        <div class="h-8 w-20 bg-gray-200 dark:bg-gray-700 rounded"></div>
      </div>
    </div>

    <!-- 表格骨架 -->
    <div v-else-if="type === 'table'" class="space-y-4">
      <div class="grid grid-cols-12 gap-4 px-4 py-2">
        <div v-for="i in columns" :key="i" class="h-4 bg-gray-200 dark:bg-gray-700 rounded col-span-1"></div>
      </div>
      <div v-for="i in count" :key="i" class="grid grid-cols-12 gap-4 px-4 py-3 rounded">
        <div v-for="j in columns" :key="j" class="h-4 bg-gray-200 dark:bg-gray-700 rounded col-span-1"></div>
      </div>
    </div>

    <!-- 图表骨架 -->
    <div v-else-if="type === 'chart'" class="p-4 rounded-xl border border-gray-200 dark:border-gray-700">
      <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/3 mb-6"></div>
      <div class="h-64 bg-gray-200 dark:bg-gray-700 rounded"></div>
    </div>

    <!-- 个人信息骨架 -->
    <div v-else-if="type === 'profile'" class="p-4 rounded-xl">
      <div class="flex flex-col items-center">
        <div class="rounded-full bg-gray-200 dark:bg-gray-700 h-24 w-24 mb-4"></div>
        <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-2"></div>
        <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/3 mb-6"></div>
      </div>
      <div class="space-y-4">
        <div v-for="i in count" :key="i" class="space-y-2">
          <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/4"></div>
          <div class="h-10 bg-gray-200 dark:bg-gray-700 rounded"></div>
        </div>
      </div>
    </div>

    <!-- 标签骨架 -->
    <div v-else-if="type === 'tag'" class="bg-gray-200 dark:bg-gray-700 rounded-full px-3 py-1 h-6 w-20"></div>

    <!-- 按钮骨架 -->
    <div v-else-if="type === 'button'" class="bg-gray-200 dark:bg-gray-700 rounded-lg h-8 w-20"></div>

    <!-- 文本骨架 -->
    <div v-else-if="type === 'text'" class="h-4 bg-gray-200 dark:bg-gray-700 rounded" :style="{ width }"></div>

    <!-- 头像骨架 -->
    <div v-else-if="type === 'avatar'" class="rounded-md bg-gray-200 dark:bg-gray-700 h-8 w-8"></div>

    <!-- 默认骨架 -->
    <div v-else class="p-4 rounded-xl">
      <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-4"></div>
      <div class="space-y-2">
        <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded"></div>
        <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-5/6"></div>
        <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  type?: 'card' | 'list' | 'table' | 'chart' | 'profile' | 'tag' | 'button' | 'text' | 'avatar' | 'default'
  count?: number
  columns?: number
  width?: string
}

const props = withDefaults(defineProps<Props>(), {
  type: 'default',
  count: 3,
  columns: 4,
  width: '100px'
})

const themeClass = computed(() => {
  return 'bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700'
})
</script>
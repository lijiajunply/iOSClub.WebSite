<template>
  <n-card 
    class="rounded-xl shadow-sm bg-white dark:bg-neutral-800 transition-colors duration-300 hover:shadow-md"
    :bordered="false"
  >
    <div class="flex items-center">
      <div class="p-2 rounded-lg" :class="bgColorClass">
        <n-icon :size="iconSize" :color="iconColor">
          <component :is="iconComponent" />
        </n-icon>
      </div>
      <div class="ml-4">
        <p class="text-sm font-medium text-gray-500 dark:text-gray-400">{{ title }}</p>
        <p class="text-2xl font-semibold text-gray-900 dark:text-white">{{ value }}</p>
      </div>
    </div>
  </n-card>
</template>

<script setup>
import { computed } from 'vue'
import { NCard, NIcon } from 'naive-ui'
import { 
  People, 
  IdCard, 
  Folder, 
  List, 
  Document, 
  Business, 
  Checkbox
} from '@vicons/ionicons5'

const props = defineProps({
  title: {
    type: String,
    required: true
  },
  value: {
    type: [Number, String],
    required: true
  },
  icon: {
    type: String,
    default: 'people'
  },
  color: {
    type: String,
    default: 'blue'
  }
})

const iconSize = 24

const iconComponent = computed(() => {
  switch (props.icon) {
    case 'people':
      return People
    case 'badge':
      return IdCard
    case 'folder':
      return Folder
    case 'task':
      return List
    case 'resource':
      return Document
    case 'department':
      return Business
    case 'todo':
      return Checkbox
    default:
      return People
  }
})

const iconColor = computed(() => {
  switch (props.color) {
    case 'blue':
      return '#3b82f6'
    case 'green':
      return '#10b981'
    case 'purple':
      return '#8b5cf6'
    case 'yellow':
      return '#f59e0b'
    case 'pink':
      return '#ec4899'
    case 'indigo':
      return '#6366f1'
    case 'red':
      return '#ef4444'
    default:
      return '#3b82f6'
  }
})

const bgColorClass = computed(() => {
  switch (props.color) {
    case 'blue':
      return 'bg-blue-100 dark:bg-blue-900/50'
    case 'green':
      return 'bg-green-100 dark:bg-green-900/50'
    case 'purple':
      return 'bg-purple-100 dark:bg-purple-900/50'
    case 'yellow':
      return 'bg-yellow-100 dark:bg-yellow-900/50'
    case 'pink':
      return 'bg-pink-100 dark:bg-pink-900/50'
    case 'indigo':
      return 'bg-indigo-100 dark:bg-indigo-900/50'
    case 'red':
      return 'bg-red-100 dark:bg-red-900/50'
    default:
      return 'bg-blue-100 dark:bg-blue-900/50'
  }
})
</script>

<style scoped>
/* Custom styles if needed */
</style>
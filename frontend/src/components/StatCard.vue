<template>
  <div class="rounded-2xl bg-white dark:bg-neutral-800 transition-all duration-300 hover:shadow-lg p-5">
    <div class="flex items-center">
      <div class="p-3 rounded-xl" :class="bgColorClass">
        <Icon :icon="iconName" :width="iconSize" :height="iconSize" :color="iconColor"/>
      </div>
      <div class="ml-4">
        <p class="text-sm font-medium text-gray-500 dark:text-gray-400">{{ title }}</p>
        <p v-if="typeof value === 'string'" class="text-2xl font-semibold text-gray-900 dark:text-white">{{ value }}</p>
        <div v-else class="!text-2xl font-semibold text-gray-900 dark:text-white mt-1">
          <n-number-animation :from="0" :to="value"/>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {computed} from 'vue'
import {Icon} from '@iconify/vue'
import {NNumberAnimation} from 'naive-ui'

interface Props {
  title: string
  value: number | string
  icon?: string
  color?: string
}

const props = withDefaults(defineProps<Props>(), {
  icon: 'people',
  color: 'blue'
})

const iconSize = 24

const iconName = computed(() => {
  switch (props.icon) {
    case 'people':
      return 'ion:people'
    case 'badge':
      return 'ion:card'
    case 'folder':
      return 'ion:folder'
    case 'task':
      return 'ion:list'
    case 'resource':
      return 'ion:document'
    case 'department':
      return 'ion:business'
    case 'todo':
      return 'ion:checkbox'
    default:
      return props.icon
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
      return 'bg-blue-100 dark:bg-blue-900/30'
    case 'green':
      return 'bg-green-100 dark:bg-green-900/30'
    case 'purple':
      return 'bg-purple-100 dark:bg-purple-900/30'
    case 'yellow':
      return 'bg-yellow-100 dark:bg-yellow-900/30'
    case 'pink':
      return 'bg-pink-100 dark:bg-pink-900/30'
    case 'indigo':
      return 'bg-indigo-100 dark:bg-indigo-900/30'
    case 'red':
      return 'bg-red-100 dark:bg-red-900/30'
    default:
      return 'bg-blue-100 dark:bg-blue-900/30'
  }
})
</script>

<style scoped>
/* Custom styles if needed */
</style>
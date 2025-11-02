<template>
  <n-card 
    class="rounded-xl shadow-sm bg-white dark:bg-neutral-800 transition-colors duration-300 hover:shadow-md"
    :bordered="false"
  >
    <div class="flex flex-col items-center">
      <div 
        class="w-16 h-16 rounded-full flex items-center justify-center text-white font-bold text-xl mb-3"
        :style="{ backgroundColor: getColorForStaff(staff.name) }"
      >
        {{ staff.name.charAt(0) }}
      </div>
      <h3 class="font-medium text-gray-900 dark:text-white truncate w-full text-center">
        {{ staff.name }}
      </h3>
      <p class="text-xs text-gray-500 dark:text-gray-400 text-center mt-1">
        {{ identityDictionary[staff.identity] || staff.identity }}
      </p>
    </div>
  </n-card>
</template>

<script setup>
import { NCard } from 'naive-ui'

const props = defineProps({
  staff: {
    type: Object,
    required: true
  },
  identityDictionary: {
    type: Object,
    required: true
  }
})

const getColorForStaff = (name) => {
  const colors = ['#7265e6', '#ffbf00', '#00a8ff', '#ff6b6b', '#2ed573', '#ff9ff3']
  const charCode = name.charCodeAt(0)
  return colors[charCode % colors.length]
}
</script>

<style scoped>
/* Custom styles if needed */
</style>
<template>
  <div :class="isDark ? 'dark' : ''">
    <button
      @click="isDark = !isDark"
      class="fixed top-4 left-4 w-12 h-12 flex items-center justify-center rounded-full shadow-lg bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 transition-colors duration-300"
      style="z-index:9999;"
      aria-label="切换暗夜模式"
    >
      <svg v-if="!isDark" xmlns="http://www.w3.org/2000/svg" class="h-7 w-7 text-yellow-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <circle cx="12" cy="12" r="5" fill="currentColor" />
        <g stroke-width="2">
          <line x1="12" y1="2" x2="12" y2="4" />
          <line x1="12" y1="20" x2="12" y2="22" />
          <line x1="2" y1="12" x2="4" y2="12" />
          <line x1="20" y1="12" x2="22" y2="12" />
          <line x1="4.93" y1="4.93" x2="6.34" y2="6.34" />
          <line x1="17.66" y1="17.66" x2="19.07" y2="19.07" />
          <line x1="4.93" y1="19.07" x2="6.34" y2="17.66" />
          <line x1="17.66" y1="6.34" x2="19.07" y2="4.93" />
        </g>
      </svg>
      <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-7 w-7 text-blue-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path d="M21 12.79A9 9 0 1111.21 3a7 7 0 109.79 9.79z" fill="currentColor" />
      </svg>
    </button>
    <n-config-provider :theme="theme">
      <n-dialog-provider>
        <n-message-provider>
          <router-view/>
        </n-message-provider>
      </n-dialog-provider>
    </n-config-provider>
  </div>
</template>

<script setup lang="ts">
import { NMessageProvider, NDialogProvider, NConfigProvider, darkTheme } from 'naive-ui'
import { ref, computed, watch } from 'vue'
import { RouterView } from 'vue-router'

// 读取本地存储的暗夜状态
const isDark = ref(localStorage.getItem('isDark') === 'true')
const theme = computed(() => (isDark.value ? darkTheme : null))

// 监听切换并存储
watch(isDark, (val) => {
  localStorage.setItem('isDark', val ? 'true' : 'false')
})
</script>
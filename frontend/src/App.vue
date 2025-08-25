<template>
  <div :class="{ 'dark': isDark }">
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
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterView } from 'vue-router'

// 自动跟随系统暗夜模式
const isDark = ref(window.matchMedia('(prefers-color-scheme: dark)').matches)
const theme = computed(() => (isDark.value ? darkTheme : null))

let mediaQuery
onMounted(() => {
  mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
  const handler = (e) => { isDark.value = e.matches }
  mediaQuery.addEventListener('change', handler)
  // 组件卸载时移除监听
  onUnmounted(() => {
    mediaQuery.removeEventListener('change', handler)
  })
})
</script>
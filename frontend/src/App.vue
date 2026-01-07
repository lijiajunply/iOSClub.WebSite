<template>
  <n-config-provider :theme="theme" :locale="zhCN"
    :date-locale="dateZhCN">
    <n-dialog-provider>
      <n-message-provider>
        <AppContent />
      </n-message-provider>
    </n-dialog-provider>
  </n-config-provider>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { NConfigProvider, NDialogProvider, NMessageProvider, zhCN, dateZhCN, useMessage } from "naive-ui";
import { useThemeStore } from './stores/theme'
import { storeToRefs } from 'pinia'
import { setMessageInstance } from './utils/errorHandler'

const themeStore = useThemeStore()
const { theme } = storeToRefs(themeStore)
const { init } = themeStore

const message = useMessage()

onMounted(() => {
  init()
  setMessageInstance(message)
})
</script>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: 'App',
  components: {
    AppContent: {
      template: '<router-view />'
    }
  }
})
</script>
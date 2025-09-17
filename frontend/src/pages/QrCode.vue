<template>
  <n-grid :x-gap="12" :y-gap="12" cols="24" item-responsive justify="center">
    <n-gi
        span="0 800:7 1000:8"
    ></n-gi>
    <n-gi
        span="24 800:10 1000:8"
    >
      <n-card
          class="login-form w-full"
          :bordered="false"
      >
        <div class="flex gap-4">
          <n-image
              src="src/assets/iOS_Club_LOGO.png"
              :preview-disabled="true"
              class="w-[70px] h-[70px]"
              object-fit="cover"
          />
          <div class="flex-1 p-[5px] max-w-[calc(100%-70px)]">
            <div
                class="title-btn inline-block transition-all duration-200 cursor-pointer overflow-hidden whitespace-nowrap text-ellipsis w-full text-[21px] font-medium text-[#1c1f23] hover:scale-[1.02]"
                @click="handleTitleClick"
            >
              iOS Club of XAUAT 2025
            </div>
            <p class="mt-2 flex items-center gap-2">
              群号: 952954710
              <n-icon
                  :component="CopyOutline"
                  size="18"
                  class="cursor-pointer hover:text-primary"
                  @click="handleCopyClick"
              />
            </p>
          </div>
        </div>

        <n-divider/>

        <img
            alt="二维码"
            src="../assets/other/二维码.jpg"
            class="w-full"
        />

        <div class="phone-desktop text-center mt-[18px]">
          <p>扫一扫二维码，加入群聊</p>
        </div>
      </n-card>
    </n-gi>
  </n-grid>
</template>

<script setup lang="ts">
import {onMounted} from 'vue'
import {useMessage, NGrid, NGi, NCard, NImage, NIcon, NDivider} from 'naive-ui'
import {CopyOutline} from '@vicons/ionicons5'

const message = useMessage()

// 导航到外部链接
const handleTitleClick = () => {
  // 替换为您的实际链接
  // window.open(SignRecord.ios.qqApi, '_blank')
  // 或者使用您的自定义导航方法
  navigateTo('您的QQ群链接', 'https')
}

// 复制群号到剪贴板
const handleCopyClick = async () => {
  try {
    await navigator.clipboard.writeText('952954710')
    message.success('复制成功')
  } catch (error) {
    // 降级方案
    const result = copyTextFallback('952954710')
    message.info(`复制${result ? '成功' : '失败'}`)
  }
}

// 降级的复制方法
const copyTextFallback = (text: string): boolean => {
  const textArea = document.createElement('textarea')
  textArea.value = text
  textArea.style.position = 'fixed'
  textArea.style.opacity = '0'
  document.body.appendChild(textArea)
  textArea.focus()
  textArea.select()

  try {
    const successful = document.execCommand('copy')
    document.body.removeChild(textArea)
    return successful
  } catch (err) {
    document.body.removeChild(textArea)
    return false
  }
}

// 自定义导航方法（如果需要）
const navigateTo = (url: string, protocol: string = 'https') => {
  if (!url.startsWith('http')) {
    url = `${protocol}://${url}`
  }
  window.open(url, '_blank')
}

// 组件挂载后显示提示
onMounted(() => {
  message.info('如果微信提示无法跳转，请复制群号')
})
</script>

<style scoped>
@reference 'tailwindcss';

.login-form {
  @apply rounded-[10px] backdrop-blur-[20px] shadow-lg mt-[30px] mb-[30px] p-[15px] max-w-[100vw];
  box-shadow: 0 0 10px #c8c8c8;
}

@media screen and (max-width: 768px) {
  .login-form {
    @apply mt-5 shadow-none;
  }

  .title-btn:hover {
    transform: scale(1) !important;
  }

  /* 如果需要隐藏 header 和 footer，在父组件或全局样式中处理 */
}
</style>
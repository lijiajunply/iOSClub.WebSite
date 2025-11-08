<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 dark:from-neutral-900 dark:to-neutral-800 flex items-center justify-center p-4 transition-colors duration-300">
    <div class="w-full max-w-md bg-white/80 dark:bg-neutral-900/80 backdrop-blur-lg rounded-2xl shadow-xl p-8 transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-xl md:text-3xl font-semibold bg-gradient-to-r from-blue-500 to-indigo-500 bg-clip-text text-transparent">
          已退出登录
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
          您已成功退出登录系统
        </p>
      </div>

      <div class="flex flex-col items-center gap-4">
        <n-button type="primary" @click="handleLogin" class="w-full">
          返回登录页面
        </n-button>
        <n-button @click="handleGoHome">
          返回首页
        </n-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {NButton} from 'naive-ui'
import {useRouter} from 'vue-router'
import {useAuthorizationStore} from "../stores/Authorization.js"

const router = useRouter()
const authorizationStore = useAuthorizationStore()

// 初始化时执行登出操作
const initLogout = async () => {
  try {
    // 调用登出方法
    await authorizationStore.logout()
  } catch (error) {
    console.error('登出时出错:', error)
  }
}

// 页面加载时执行登出
initLogout()

const handleLogin = () => {
  router.push('/login')
}

const handleGoHome = () => {
  router.push('/')
}
</script>

<style scoped>
.btn {
  width: 100%;
  padding: 8px 0;
  background: linear-gradient(90deg, #FF99C8 0%, #646EF8 100%);
  border: none;
  border-radius: 12px;
  color: #FFFFFF;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn:hover:not(:disabled) {
  transform: scale(1.03);
  box-shadow: 0 6px 20px rgba(255, 153, 200, 0.3);
}

.btn:active:not(:disabled) {
  transform: scale(0.98);
  box-shadow: 0 4px 16px rgba(255, 153, 200, 0.2);
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}
</style>
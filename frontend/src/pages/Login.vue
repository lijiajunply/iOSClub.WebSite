<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 flex items-center justify-center p-4">
    <div class="w-full max-w-md bg-white/80 backdrop-blur-lg rounded-2xl shadow-xl p-8 transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-xl md:text-3xl font-semibold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
          登录到 iMember 账号
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
          Sign in to your account
        </p>
      </div>

      <n-form :model="form" :rules="rules" ref="formRef">
        <n-form-item path="email" label="学号">
          <n-input
            v-model:value="form.email"
            placeholder=""
            class="rounded-lg transition-all duration-300 focus:ring-2 focus:ring-blue-500/50"
          />
        </n-form-item>

        <n-form-item path="password" label="密码">
          <n-input
            v-model:value="form.password"
            type="password"
            placeholder="初始密码为学号"
            @keyup.enter="handleLogin"
            class="rounded-lg transition-all duration-300 focus:ring-2 focus:ring-blue-500/50"
          />
        </n-form-item>

        <div class="flex items-center justify-between mb-6">
          <n-checkbox v-model:checked:value="form.rememberMe">
            记住我
          </n-checkbox>
          <a href="#" class="text-sm text-blue-600 dark:text-purple-400 hover:underline">
            忘记密码？
          </a>
        </div>

        <button
          @click="handleLogin"
          block
          class="btn"
        >
          登录
        </button>
      </n-form>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          没有 iMember 账号？
          <a href="/Signup" class="text-blue-600 dark:text-purple-400 font-medium hover:underline">
            注册
          </a>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { NButton, NInput, NCheckbox, NForm, NFormItem } from 'naive-ui'

const formRef = ref()
const form = ref({
  email: '',
  password: '',
  rememberMe: false
})

const rules = {
  email: {
    required: true,
    message: '请输入您的学号',
    trigger: 'blur'
  },
  password: {
    required: true,
    message: '请输入您的密码',
    trigger: 'blur'
  }
}

const handleLogin = () => {
  formRef.value.validate((errors) => {
    if (!errors) {
      // 登录逻辑
      console.log('登录成功', form.value)
    }
  })
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
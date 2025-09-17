<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 dark:from-neutral-900 dark:to-neutral-800 flex items-center justify-center p-4 transition-colors duration-300">
    <div class="w-full max-w-md bg-white/80 dark:bg-neutral-900/80 backdrop-blur-lg rounded-2xl shadow-xl p-8 transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-xl md:text-3xl font-semibold bg-gradient-to-r from-blue-500 to-indigo-500 bg-clip-text text-transparent">
          登录 iMember 账号
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
          Welcome back!
        </p>
      </div>

      <n-form :model="form" :rules="rules" ref="formRef">
        <n-form-item path="name" label="姓名" class="dark:text-gray-100">
          <n-input
              v-model:value="form.name"
              placeholder="请输入您的姓名"
              class="dark:text-gray-100 dark:bg-neutral-800"
          />
        </n-form-item>

        <n-form-item path="studentId" label="学号" class="dark:text-gray-100">
          <n-input
              v-model:value="form.studentId"
              placeholder="请输入您的学号"
              @keyup.enter="handleLogin"
              class="dark:text-gray-100 dark:bg-neutral-800"
          />
        </n-form-item>

        <n-form-item path="password" label="密码" class="dark:text-gray-100">
          <n-input
              type="password"
              v-model:value="form.password"
              placeholder="请输入密码"
              @keyup.enter="handleLogin"
              class="dark:text-gray-100 dark:bg-neutral-800"
          />
        </n-form-item>

        <n-form-item>
          <n-checkbox v-model:checked="form.rememberMe" class="dark:text-gray-100">记住我</n-checkbox>
          <router-link to="/ForgotPassword" class="float-right text-blue-500 dark:text-blue-300">忘记密码?</router-link>
        </n-form-item>

        <button
            @click="handleLogin"
            :disabled="loading"
            block
            class="btn"
        >
          <span v-if="loading">登录中...</span>
          <span v-else>登录</span>
        </button>
        <div v-if="errors" class="text-red-500 text-center mt-4">{{ errorMsg }}</div>
      </n-form>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          没有 iMember 账号？
          <router-link to="/SignUp" class="text-blue-600 dark:text-purple-400 font-medium hover:underline">
            注册
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { NButton, NInput, NCheckbox, NForm, NFormItem } from 'naive-ui'
import { useAuthorizationStore } from '../stores/Authorization.ts'

const router = useRouter()
const authorizationStore = useAuthorizationStore()

const formRef = ref()
const form = ref({
  name: '',
  id: '',
  password: '',
  rememberMe: false
})
const loading = ref(false)
const errorMsg = ref('')

const rules = {
  name: {
    required: true,
    message: '请输入您的姓名',
    trigger: 'blur'
  },
  id: {
    required: true,
    message: '请输入您的学号',
    trigger: 'blur'
  },
  password: {
    required: true,
    message: '请输入密码',
    trigger: 'blur'
  }
}

// 页面加载时检查是否保存了登录信息
onMounted(() => {
  const savedLoginInfo = localStorage.getItem('savedLoginInfo')
  if (savedLoginInfo) {
    try {
      const info = JSON.parse(savedLoginInfo)
      form.value.name = info.name
      form.value.id = info.id // 对应学号
      form.value.rememberMe = info.rememberMe
    } catch (e) {
      console.error('Failed to parse saved login info:', e)
    }
  }
})

// 密码SHA-256哈希处理函数
const hashPassword = async (password) => {
  const encoder = new TextEncoder()
  const data = encoder.encode(password)
  const hashBuffer = await crypto.subtle.digest('SHA-256', data)
  const hashArray = Array.from(new Uint8Array(hashBuffer))
  return hashArray.map(b => b.toString(16).padStart(2, '0')).join('')
}

const handleLogin = async () => {
  if (loading.value) return
  formRef.value.validate(async (errors) => {
    if (!errors) {
      loading.value = true
      errorMsg.value = ''
      try {
        const hashedPassword = await hashPassword(form.value.password)

        const res = await fetch('https://www.xauat.site/api/Member/Login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            name: form.value.name,
            id: form.value.id,
            password: hashedPassword
          })
        })

        if (!res.ok) {
          const err = await res.json()
          throw new Error(err.message || '登录失败')
        }

        const token = await res.text()
        localStorage.setItem('Authorization', token)

        if (form.value.rememberMe) {
          localStorage.setItem('savedLoginInfo', JSON.stringify({
            name: form.value.name,
            id: form.value.id,
            password: hashedPassword
          }))
        } else {
          localStorage.removeItem('savedLoginInfo')
        }

        router.push('/Centre')
      } catch (err) {
        errorMsg.value = err.message || '登录失败'
      } finally {
        loading.value = false
      }
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

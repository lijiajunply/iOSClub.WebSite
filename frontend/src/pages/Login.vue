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
        <div v-if="errorMsg" class="text-red-500 text-center mt-4">{{ errorMsg }}</div>
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

const router = useRouter()

const formRef = ref()
const form = ref({
  email: '',
  password: '',
  rememberMe: false
})
const loading = ref(false)
const errorMsg = ref('')

const rules = {
  email: {
    required: true,
    message: '请输入您的姓名',
    trigger: 'blur'
  },
  password: {
    required: true,
    message: '请输入您的学号',
    trigger: 'blur'
  }
}

// 页面加载时检查是否保存了登录信息
onMounted(() => {
  const savedLoginInfo = localStorage.getItem('savedLoginInfo')
  if (savedLoginInfo) {
    try {
      const parsedInfo = JSON.parse(savedLoginInfo)
      form.value.email = parsedInfo.email || ''
      form.value.password = parsedInfo.password || ''
      form.value.rememberMe = true
    } catch (e) {
      console.error('解析保存的登录信息时出错:', e)
    }
  }
})

const handleLogin = () => {
  if (loading.value) return
  formRef.value.validate(async (errors) => {
    if (!errors) {
      loading.value = true
      errorMsg.value = ''
      try {
        const res = await fetch('https://www.xauat.site/api/Member/Login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            name: form.value.email,
            id: form.value.password
          })
        })
        if (!res.ok) {
          // 尝试解析错误响应
          let errorMessage = '登录失败'
          try {
            const errorData = await res.json()
            errorMessage = errorData.message || errorMessage
          } catch (e) {
            // 如果无法解析为JSON，则使用默认错误消息
          }
          throw new Error(errorMessage)
        }

        // 后端直接返回JWT token字符串，而不是JSON对象
        const token = await res.text()
        localStorage.setItem('Authorization', token)

        // 如果用户选择了"记住我"，则保存登录信息
        if (form.value.rememberMe) {
          const loginInfo = {
            email: form.value.email,
            password: form.value.password
          }
          localStorage.setItem('savedLoginInfo', JSON.stringify(loginInfo))
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

<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 flex items-center justify-center p-4">
    <div class="w-full max-w-md bg-white/80 backdrop-blur-lg rounded-2xl shadow-xl p-8 transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-xl md:text-3xl font-semibold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
          忘记密码
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
          请输入您的学号以重置密码
        </p>
      </div>

      <n-form :model="form" :rules="rules" ref="formRef">
        <n-form-item path="studentId" label="学号">
          <n-input
            v-model:value="form.studentId"
            placeholder="请输入您的学号"
            class="rounded-lg transition-all duration-300 focus:ring-2 focus:ring-blue-500/50"
          />
        </n-form-item>
      </n-form>

      <button
        @click="handleSubmit"
        :disabled="loading"
        block
        class="btn"
      >
        <span v-if="loading">提交中...</span>
        <span v-else>提交</span>
      </button>

      <div v-if="errorMsg" class="text-red-500 text-center mt-4">
        {{ errorMsg }}
      </div>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          记起来了？
          <router-link
            to="/Login"
            class="text-blue-600 dark:text-purple-400 font-medium hover:underline"
          >
            返回登录页
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { NForm, NFormItem, NInput } from 'naive-ui'

const router = useRouter()
const formRef = ref()
const loading = ref(false)
const errorMsg = ref('')
const form = ref({
  studentId: ''
})

const rules = {
  studentId: {
    required: true,
    message: '请输入您的学号',
    trigger: 'blur'
  }
}

const handleSubmit = () => {
  if (loading.value) return

  formRef.value.validate(async (errors) => {
    if (!errors) {
      loading.value = true
      errorMsg.value = ''

      try {
        const res = await fetch('/api/Member/ForgotPassword', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            studentId: form.value.studentId
          })
        })

        if (!res.ok) {
          const err = await res.json()
          throw new Error(err.message || '网络请求失败')
        }

        const data = await res.json()

        // 成功提示
        alert('重置邮件已发送，请查收您的邮箱')

        // 跳转到登录页
        router.push('/Login')
      } catch (err) {
        errorMsg.value = err.message || '请求失败，请稍后再试'
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
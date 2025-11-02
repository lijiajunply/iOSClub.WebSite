<template>
  <div class="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-neutral-900 dark:to-neutral-900 flex items-center justify-center p-4 transition-colors duration-300">
    <div class="w-full max-w-md bg-white dark:bg-neutral-900 rounded-2xl shadow-xl overflow-hidden transition-all duration-300 transform hover:shadow-2xl">
      <div class="p-1 bg-gradient-to-r from-blue-500 to-purple-500"></div>
      
      <div class="p-8">
        <div class="text-center mb-8">
          <div class="mx-auto w-16 h-16 rounded-full bg-blue-50 dark:bg-blue-900/20 flex items-center justify-center mb-4">
            <Icon icon="heroicons:key-solid" class="w-8 h-8 text-blue-500 dark:text-blue-400" />
          </div>
          <h1 class="text-2xl font-semibold text-gray-900 dark:text-white mb-2">
            忘记密码
          </h1>
          <p class="text-gray-500 dark:text-gray-400">
            请输入您的学号以重置密码
          </p>
        </div>

        <n-form :model="form" :rules="rules" ref="formRef">
          <n-form-item path="studentId" label="学号">
            <n-input
              v-model:value="form.studentId"
              placeholder="请输入您的学号"
              size="large"
              class="rounded-xl"
              :input-props="{ class: 'dark:bg-neutral-800 dark:text-white dark:placeholder-gray-400' }"
            />
          </n-form-item>
        </n-form>

        <n-button
          @click="handleSubmit"
          :loading="loading"
          type="primary"
          size="large"
          block
          class="mt-2 rounded-xl font-medium"
          :disabled="loading"
        >
          {{ loading ? '提交中...' : '发送重置邮件' }}
        </n-button>

        <div v-if="errorMsg" class="text-red-500 text-center mt-4 text-sm">
          {{ errorMsg }}
        </div>

        <div v-if="successMsg" class="text-green-500 text-center mt-4 text-sm">
          {{ successMsg }}
        </div>

        <div class="mt-6 text-center">
          <p class="text-sm text-gray-500 dark:text-gray-400">
            记起来了？
            <router-link
              to="/Login"
              class="text-blue-500 dark:text-blue-400 font-medium hover:underline"
            >
              返回登录页
            </router-link>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { NForm, NFormItem, NInput, NButton } from 'naive-ui'
import { Icon } from '@iconify/vue'

interface FormState {
  studentId: string
}

const router = useRouter()
const formRef = ref<InstanceType<typeof NForm> | null>(null)
const loading = ref(false)
const errorMsg = ref('')
const successMsg = ref('')
const form = ref<FormState>({
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

  formRef.value?.validate(async (errors) => {
    if (!errors) {
      loading.value = true
      errorMsg.value = ''
      successMsg.value = ''

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

        // 成功提示
        successMsg.value = '重置邮件已发送，请查收您的邮箱'

        // 3秒后跳转到登录页
        setTimeout(async () => {
          await router.push('/Login')
        }, 3000)
      } catch (err: any) {
        errorMsg.value = err.message || '请求失败，请稍后再试'
      } finally {
        loading.value = false
      }
    }
  })
}
</script>

<style scoped>
:deep(.n-form-item-label) {
  font-weight: 500;
  color: var(--color-foreground);
}

:deep(.n-input__border) {
  border-radius: 0.75rem;
  border-color: var(--color-border);
}

:deep(.n-input__border:hover) {
  border-color: var(--color-primary);
}

:deep(.n-input__border:focus-within) {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

:deep(.n-button) {
  border-radius: 0.75rem;
  font-weight: 500;
  transition: all 0.2s ease;
}

:deep(.n-button:hover:not(:disabled)) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2);
}
</style>
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
            {{ step === 1 ? '忘记密码' : '重置密码' }}
          </h1>
          <p class="text-gray-500 dark:text-gray-400">
            {{ step === 1 ? '请输入您的学号以重置密码' : '请输入验证码和新密码' }}
          </p>
        </div>

        <!-- 第一步：请求重置密码 -->
        <n-form v-if="step === 1" :model="formStep1" :rules="rulesStep1" ref="formStep1Ref">
          <n-form-item path="studentId" label="学号">
            <n-input
              v-model:value="formStep1.studentId"
              placeholder="请输入您的学号"
              size="large"
              class="rounded-xl"
              :input-props="{ class: 'dark:bg-neutral-800 dark:text-white dark:placeholder-gray-400' }"
            />
          </n-form-item>
        </n-form>

        <!-- 第二步：输入验证码和新密码 -->
        <n-form v-else :model="formStep2" :rules="rulesStep2" ref="formStep2Ref">
          <n-form-item path="studentId" label="学号">
            <n-input
              v-model:value="formStep2.studentId"
              placeholder="请输入您的学号"
              size="large"
              class="rounded-xl"
              :input-props="{ class: 'dark:bg-neutral-800 dark:text-white dark:placeholder-gray-400' }"
              :disabled="true"
            />
          </n-form-item>
          
          <n-form-item path="verificationCode" label="验证码">
            <n-input-otp
              v-model:value="formStep2.verificationCode"
              :length="6"
            />
          </n-form-item>
          
          <n-form-item path="newPassword" label="新密码">
            <n-input
              v-model:value="formStep2.newPassword"
              type="password"
              show-password-on="click"
              placeholder="请输入新密码"
              size="large"
              class="rounded-xl"
              :input-props="{ class: 'dark:bg-neutral-800 dark:text-white dark:placeholder-gray-400' }"
            />
          </n-form-item>
          
          <n-form-item path="confirmPassword" label="确认新密码">
            <n-input
              v-model:value="formStep2.confirmPassword"
              type="password"
              show-password-on="click"
              placeholder="请再次输入新密码"
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
          {{ 
            loading ? '提交中...' : 
            (step === 1 ? '发送重置邮件' : '重置密码') 
          }}
        </n-button>

        <div v-if="errorMsg" class="text-red-500 text-center mt-4 text-sm">
          {{ errorMsg }}
        </div>

        <div v-if="successMsg" class="text-green-500 text-center mt-4 text-sm">
          {{ successMsg }}
        </div>

        <div class="mt-6 text-center">
          <p class="text-sm text-gray-500 dark:text-gray-400">
            {{ step === 1 ? '记起来了？' : '返回登录页' }}
            <router-link
              v-if="step === 1"
              to="/Login"
              class="text-blue-500 dark:text-blue-400 font-medium hover:underline"
            >
              返回登录页
            </router-link>
            <button 
              v-else
              @click="step = 1"
              class="text-blue-500 dark:text-blue-400 font-medium hover:underline"
            >
              重新发送验证码
            </button>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { NForm, NFormItem, NInput, NButton, NInputOtp } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { AuthService } from '../services/AuthService';

interface FormStep1State {
  studentId: string
}

interface FormStep2State {
  studentId: string
  verificationCode: string[]
  newPassword: string
  confirmPassword: string
}

const router = useRouter()
const formStep1Ref = ref<InstanceType<typeof NForm> | null>(null)
const formStep2Ref = ref<InstanceType<typeof NForm> | null>(null)
const loading = ref(false)
const errorMsg = ref('')
const successMsg = ref('')
const step = ref(1) // 1: 发送验证码步骤, 2: 重置密码步骤

const formStep1 = ref<FormStep1State>({
  studentId: ''
})

const formStep2 = ref<FormStep2State>({
  studentId: '',
  verificationCode: Array(6).fill(''), // 初始化为包含6个空字符串的数组
  newPassword: '',
  confirmPassword: ''
})

const rulesStep1 = {
  studentId: {
    required: true,
    message: '请输入您的学号',
    trigger: 'blur'
  }
}

const rulesStep2 = {
  studentId: {
    required: true,
    message: '请输入您的学号',
    trigger: 'blur'
  },
  verificationCode: {
    required: true,
    message: '请输入验证码',
    trigger: 'blur',
    validator: (_: any, value: string[]) => {
      // 验证所有位置都有值
      if (!value || value.some(v => !v)) {
        return new Error('请输入完整的验证码');
      }
      return true;
    }
  },
  newPassword: {
    required: true,
    message: '请输入新密码',
    trigger: 'blur'
  },
  confirmPassword: {
    required: true,
    message: '请确认新密码',
    trigger: 'blur',
    validator: (_: any, value: string) => {
      if (value !== formStep2.value.newPassword) {
        return new Error('两次输入的密码不一致')
      }
      return true
    }
  }
}

const handleSubmit = () => {
  if (loading.value) return

  if (step.value === 1) {
    // 第一步：发送验证码
    formStep1Ref.value?.validate(async (errors) => {
      if (!errors) {
        loading.value = true
        errorMsg.value = ''
        successMsg.value = ''

        try {
          await AuthService.requestPasswordReset(formStep1.value.studentId);
          
          // 成功提示
          successMsg.value = '重置验证码已发送，请查收您的邮箱'
          
          // 初始化第二步表单
          formStep2.value.studentId = formStep1.value.studentId
          // 重置验证码数组
          formStep2.value.verificationCode = Array(6).fill('')
          
          // 延迟切换到第二步
          setTimeout(() => {
            step.value = 2
          }, 2000)
        } catch (err: any) {
          errorMsg.value = err.message || '请求失败，请稍后再试'
        } finally {
          loading.value = false
        }
      }
    })
  } else {
    // 第二步：重置密码
    formStep2Ref.value?.validate(async (errors) => {
      if (!errors) {
        loading.value = true
        errorMsg.value = ''
        successMsg.value = ''

        try {
          await AuthService.resetPassword(
            formStep2.value.studentId,
            formStep2.value.verificationCode.join(''), // 将数组连接成字符串
            formStep2.value.newPassword
          );
          
          // 成功提示
          successMsg.value = '密码重置成功，即将跳转到登录页面'

          // 3秒后跳转到登录页
          setTimeout(async () => {
            await router.push('/Login')
          }, 3000)
        } catch (err: any) {
          errorMsg.value = err.message || '密码重置失败，请稍后再试'
        } finally {
          loading.value = false
        }
      }
    })
  }
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

:deep(.n-input-otp) {
  gap: 8px;
}

:deep(.n-input-otp .n-input-otp-slot) {
  width: 48px;
  height: 48px;
  font-size: 18px;
  border-radius: 0.75rem;
  border-color: var(--color-border);
}

:deep(.n-input-otp .n-input-otp-slot:focus-within) {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}
</style>
<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 flex items-center justify-center p-4">
    <div class="w-full max-w-md bg-white/80 backdrop-blur-lg rounded-2xl shadow-xl p-8 transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-xl md:text-3xl font-semibold bg-gradient-to-r from-pink-500 to-indigo-500 bg-clip-text text-transparent">
          注册 iMember 账号
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
          Create your account
        </p>
      </div>


      <n-form :model="form" :rules="rules" ref="formRef">
        <n-form-item path="studentId" label="学号">
          <n-input v-model:value="form.studentId" placeholder="请输入学号" />
        </n-form-item>
        <n-form-item path="major" label="专业">
          <n-input v-model:value="form.major" placeholder="请输入专业" />
        </n-form-item>
        <n-form-item path="political" label="政治面貌">
          <n-input v-model:value="form.political" placeholder="如：群众/共青团员/中共预备党员/中共预备党员/其他社团组织成员" />
        </n-form-item>
        <n-form-item path="grade" label="年级">
          <n-input v-model:value="form.grade" placeholder="请输入年级，如2022" />
        </n-form-item>
        <n-form-item path="className" label="班级">
          <n-input v-model:value="form.className" placeholder="请输入班级" />
        </n-form-item>
        <n-form-item path="phone" label="电话号码">
          <n-input v-model:value="form.phone" placeholder="请输入电话号码" />
        </n-form-item>
        <n-form-item path="password" label="密码">
          <n-input v-model:value="form.password" type="password" placeholder="请输入密码" />
        </n-form-item>
        <n-form-item path="confirmPassword" label="确认密码">
          <n-input v-model:value="form.confirmPassword" type="password" placeholder="请再次输入密码" />
        </n-form-item>
        <button @click="handleSignup" :disabled="loading" block class="btn">
          <span v-if="loading">注册中...</span>
          <span v-else>注册</span>
        </button>
        <div v-if="errorMsg" class="text-red-500 text-center mt-4">{{ errorMsg }}</div>
        <div v-if="successMsg" class="text-green-600 text-center mt-4">{{ successMsg }}</div>
      </n-form>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          已有账号？
          <a href="/Login" class="text-blue-600 dark:text-purple-400 font-medium hover:underline">
            登录
          </a>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { NInput, NForm, NFormItem } from 'naive-ui'

const router = useRouter()
const formRef = ref()
const form = ref({
  studentId: '',
  major: '',
  political: '',
  grade: '',
  className: '',
  phone: '',
  password: '',
  confirmPassword: ''
})
const loading = ref(false)
const errorMsg = ref('')
const successMsg = ref('')

const rules = {
  studentId: { required: true, message: '请输入学号', trigger: 'blur' },
  major: { required: true, message: '请输入专业', trigger: 'blur' },
  political: { required: true, message: '请输入政治面貌', trigger: 'blur' },
  grade: { required: true, message: '请输入年级', trigger: 'blur' },
  className: { required: true, message: '请输入班级', trigger: 'blur' },
  phone: { required: true, message: '请输入电话号码', trigger: 'blur' },
  password: { required: true, message: '请输入密码', trigger: 'blur' },
  confirmPassword: [
    { required: true, message: '请确认密码', trigger: 'blur' },
    {
      validator(rule, value) {
        return value === form.value.password
      },
      message: '两次输入的密码不一致',
      trigger: 'blur'
    }
  ]
}

const handleSignup = () => {
  if (loading.value) return
  formRef.value.validate(async (errors) => {
    if (!errors) {
      loading.value = true
      errorMsg.value = ''
      successMsg.value = ''
      try {
        const res = await fetch('/api/Member/SignUp', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            studentId: form.value.studentId,
            major: form.value.major,
            political: form.value.political,
            grade: form.value.grade,
            className: form.value.className,
            phone: form.value.phone,
            password: form.value.password
          })
        })
        if (!res.ok) {
          const err = await res.json().catch(() => ({}))
          throw new Error(err.message || '注册失败')
        }
        successMsg.value = '注册成功，正在跳转登录页...'
        setTimeout(() => {
          router.push('/Login')
        }, 1500)
      } catch (err) {
        errorMsg.value = err.message || '注册失败'
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
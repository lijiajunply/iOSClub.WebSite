<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 dark:from-neutral-900 dark:to-neutral-800 flex items-center justify-center p-4 transition-colors duration-300">
    <div class="w-full max-w-md bg-white/80 dark:bg-neutral-900/80 backdrop-blur-lg rounded-2xl shadow-xl p-8 transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-xl md:text-3xl font-semibold bg-gradient-to-r from-pink-500 to-indigo-500 bg-clip-text text-transparent">
          注册 iMember 账号
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
          Create your account
        </p>
      </div>


      <n-form :model="form" :rules="rules" ref="formRef">
        <n-form-item path="name" label="姓名">
          <n-input v-model:value="form.name" placeholder="请输入姓名"/>
        </n-form-item>
        <n-form-item path="studentId" label="学号">
          <n-input v-model:value="form.studentId" placeholder="请输入学号"/>
        </n-form-item>
        <n-form-item path="major" label="学院">
          <n-select v-model:value="form.major" :options="academyOptions" placeholder="请选择学院"/>
        </n-form-item>
        <n-form-item path="political" label="政治面貌">
          <n-select v-model:value="form.political" :options="politicalOptions" placeholder="请选择政治面貌"/>
        </n-form-item>
        <n-form-item path="gender" label="性别">
          <n-select v-model:value="form.gender" :options="genderOptions" placeholder="请选择性别"/>
        </n-form-item>
        <n-form-item path="className" label="班级">
          <n-input v-model:value="form.className" placeholder="请输入班级 如机电2401"/>
        </n-form-item>
        <n-form-item path="phone" label="电话号码">
          <n-input v-model:value="form.phone" placeholder="请输入电话号码"/>
        </n-form-item>
        <n-form-item path="password" label="密码">
          <n-input v-model:value="form.password" type="password" placeholder="请输入密码"/>
        </n-form-item>
        <n-form-item path="confirmPassword" label="确认密码">
          <n-input v-model:value="form.confirmPassword" type="password" placeholder="请再次输入密码"/>
        </n-form-item>
        <button @click="handleSignup" :disabled="loading" class="btn">
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
import {ref} from 'vue'
import {useRouter} from 'vue-router'
import {NInput, NForm, NFormItem, NSelect} from 'naive-ui'

const router = useRouter()
const formRef = ref()
const form = ref({
  name: '',
  studentId: '',
  major: '',
  political: '',
  gender: '',
  className: '',
  phone: '',
  password: '',
  confirmPassword: ''
})
const loading = ref(false)
const errorMsg = ref('')
const successMsg = ref('')

// 全部学院选项
const academyOptions = [
  {label: '信息与控制工程学院', value: '信息与控制工程学院'},
  {label: '理学院', value: '理学院'},
  {label: '机电工程学院', value: '机电工程学院'},
  {label: '管理学院', value: '管理学院'},
  {label: '土木工程学院', value: '土木工程学院'},
  {label: '环境与市政工程学院', value: '环境与市政工程学院'},
  {label: '建筑设备科学与工程学院', value: '建筑设备科学与工程学院'},
  {label: '材料科学与工程学院', value: '材料科学与工程学院'},
  {label: '冶金工程学院', value: '冶金工程学院'},
  {label: '资源工程学院', value: '资源工程学院'},
  {label: '城市发展与现代交通学院', value: '城市发展与现代交通学院'},
  {label: '文学院', value: '文学院'},
  {label: '艺术学院', value: '艺术学院'},
  {label: '建筑学院', value: '建筑学院'},
  {label: '马克思主义学院', value: '马克思主义学院'},
  {label: '公共管理学院', value: '公共管理学院'},
  {label: '化学与化工学院', value: '化学与化工学院'},
  {label: '体育学院', value: '体育学院'},
  {label: '安德学院', value: '安德学院'},
  {label: '未来技术学院', value: '未来技术学院'},
  {label: '国际教育学院', value: '国际教育学院'},
]
//全部政治面貌选项
const politicalOptions = [
  {label: '群众', value: '群众'},
  {label: '共青团员', value: '共青团员'},
  {label: '中共预备党员', value: '中共预备党员'},
  {label: '中共党员', value: '中共党员'},
]

// 新增性别选项
const genderOptions = [
  {label: '男', value: '男'},
  {label: '女', value: '女'}
]

const rules = {
  name: {required: true, message: '请输入姓名', trigger: 'blur'},
  studentId: {required: true, message: '请输入学号', trigger: 'blur'},
  major: {required: true, message: '请选择学院', trigger: 'change'},
  political: {required: true, message: '请选择政治面貌', trigger: 'change'},
  gender: {required: true, message: '请选择性别', trigger: 'change'},
  className: {required: true, message: '请输入班级', trigger: 'blur'},
  phone: {required: true, message: '请输入电话号码', trigger: 'blur'},
  password: {required: true, message: '请输入密码', trigger: 'blur'},
  confirmPassword: [
    {required: true, message: '请确认密码', trigger: 'blur'},
    {
      validator(rule, value) {
        return value === form.password
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
          headers: {'Content-Type': 'application/json'},
          body: JSON.stringify({
            name: form.value.name,
            studentId: form.value.studentId,
            major: form.value.major,
            political: form.value.political,
            gender: form.value.gender,
            className: form.value.className,
            phone: form.value.phone,
            password: form.value.password
          })
        })
        if (!res.ok) {
          const err = await res.json().catch(() => ({}))
          console.log(err)
          // throw new Error(err.message || '注册失败')
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
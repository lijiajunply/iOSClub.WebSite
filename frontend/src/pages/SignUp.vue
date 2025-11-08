<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 dark:from-neutral-900 dark:to-neutral-800 flex items-center justify-center p-4 transition-colors duration-300">
    <div class="w-full max-w-md bg-white/80 dark:bg-neutral-900/80 backdrop-blur-lg rounded-2xl shadow-xl p-4 transition-all duration-300">
      <!-- 标题区域 -->
      <div class="text-center pt-8 pb-4 px-8">
        <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">
          注册 iMember 账号
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-1 text-sm">
          Create your iMember ID
        </p>
      </div>

      <!-- 步骤指示器 -->
      <div class="px-8 mb-6" v-if="currentStep < 4">
        <div class="flex items-center justify-between">
          <div v-for="(step, index) in steps" :key="index" class="flex flex-col items-center">
            <div 
              class="w-8 h-8 rounded-full flex items-center justify-center transition-all duration-300" 
              :class="{
                'bg-blue-500 text-white': currentStep >= index + 1,
                'bg-gray-100 dark:bg-gray-700 text-gray-500 dark:text-gray-400': currentStep < index + 1
              }"
            >
              {{ index + 1 }}
            </div>
            <span class="text-xs mt-1 text-center" :class="{
              'text-blue-500': currentStep >= index + 1,
              'text-gray-500 dark:text-gray-400': currentStep < index + 1
            }">
              {{ step }}
            </span>
          </div>
        </div>
        <!-- 进度条 -->
        <div class="relative h-1 bg-gray-100 dark:bg-gray-700 mt-2">
          <div
            class="absolute h-full bg-blue-500 transition-all duration-300 ease-out" 
            :style="{ width: `${(currentStep - 1) * 50}%` }"
          ></div>
        </div>
      </div>

      <!-- 表单区域 -->
      <div class="px-8 pb-8">
        <n-form :model="form" ref="formRef">
          <!-- 步骤 1: 基本信息 -->
          <div v-if="currentStep === 1">
            <n-form-item 
              path="name" 
              label="姓名" 
              :rules="[{ required: true, message: '请输入姓名', trigger: 'blur' }]"
            >
              <n-input 
                v-model:value="form.name" 
                placeholder="请输入姓名" 
                  required
                  :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            <n-form-item 
              path="studentId" 
              label="学号" 
              :rules="[{ required: true, message: '请输入学号', trigger: 'blur' }]"
            >
              <n-input 
                v-model:value="form.studentId" 
                placeholder="请输入学号" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            <n-form-item 
              path="gender" 
              label="性别" 
              :rules="[{ required: true, message: '请选择性别', trigger: 'change' }]"
            >
              <n-select 
                v-model:value="form.gender" 
                :options="genderOptions" 
                placeholder="请选择性别" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            <n-form-item 
              path="political" 
              label="政治面貌" 
              :rules="[{ required: true, message: '请选择政治面貌', trigger: 'change' }]"
            >
              <n-select 
                v-model:value="form.political" 
                :options="politicalOptions" 
                placeholder="请选择政治面貌" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
          </div>

          <!-- 步骤 2: 学校信息 -->
          <div v-if="currentStep === 2">
            <n-form-item 
              path="major" 
              label="学院" 
              :rules="[{ required: true, message: '请选择学院', trigger: 'change' }]"
            >
              <n-select 
                v-model:value="form.major" 
                :options="academyOptions" 
                placeholder="请选择学院" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            <n-form-item 
              path="className" 
              label="班级" 
              :rules="[{ required: true, message: '请输入班级', trigger: 'blur' }]"
            >
              <n-input 
                v-model:value="form.className" 
                placeholder="请输入班级 如机电2401" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            <n-form-item 
              path="phone" 
              label="电话号码" 
              :rules="[{ required: true, message: '请输入电话号码', trigger: 'blur' }]"
            >
              <n-input 
                v-model:value="form.phone" 
                placeholder="请输入电话号码" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            <n-form-item 
              path="email" 
              label="邮箱" 
              :rules="[
                { required: true, message: '请输入邮箱', trigger: 'blur' },
                { validator: emailValidator, trigger: 'blur' }
              ]"
            >
              <n-input 
                v-model:value="form.email" 
                placeholder="请输入邮箱" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
          </div>

          <!-- 步骤 3: 账号设置 -->
          <div v-if="currentStep === 3">
            <n-form-item 
              path="password" 
              label="密码" 
              :rules="[{ required: true, message: '请输入密码', trigger: 'blur' }]"
            >
              <n-input 
                v-model:value="form.password" 
                type="password" 
                placeholder="请输入密码" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            <n-form-item 
              path="confirmPassword" 
              label="确认密码" 
              :rules="[
                { required: true, message: '请再次输入密码', trigger: 'blur' },
                {
                  validator: (_: any, value: any) => {
                    return value === form.password || '两次输入的密码不一致'
                  },
                  trigger: 'blur'
                }
              ]"
            >
              <n-input 
                v-model:value="form.confirmPassword" 
                type="password" 
                placeholder="请再次输入密码" 
                required
                :attrs="{ class: 'rounded-lg' }"
              />
            </n-form-item>
            
            <!-- 确认信息摘要 -->
            <div class="mt-6 p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg">
              <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">确认信息</h3>
              <div class="grid grid-cols-2 gap-2 text-sm">
                <div class="text-gray-500 dark:text-gray-400">姓名:</div>
                <div class="text-gray-900 dark:text-white">{{ form.name }}</div>
                <div class="text-gray-500 dark:text-gray-400">学号:</div>
                <div class="text-gray-900 dark:text-white">{{ form.studentId }}</div>
                <div class="text-gray-500 dark:text-gray-400">学院:</div>
                <div class="text-gray-900 dark:text-white">{{ form.major }}</div>
                <div class="text-gray-500 dark:text-gray-400">班级:</div>
                <div class="text-gray-900 dark:text-white">{{ form.className }}</div>
              </div>
            </div>
          </div>

          <!-- 步骤 4: 注册成功 -->
          <div v-if="currentStep === 4" class="text-center py-8">
            <div class="w-16 h-16 bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-green-500" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
              </svg>
            </div>
            <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">注册成功！</h2>
            <p class="text-gray-500 dark:text-gray-400 mb-6">您的账号已创建完成，正在为您跳转...</p>
          </div>

          <!-- 错误信息显示 -->
          <div v-if="errorMsg" class="text-red-500 text-center mt-4 text-sm">{{ errorMsg }}</div>
          
          <!-- 导航按钮 -->
          <div v-if="currentStep < 4" class="flex space-x-4 mt-6">
            <button 
              v-if="currentStep > 1" 
              @click="prevStep" 
              type="button"
              class="flex-1 py-2.5 px-4 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors duration-200 text-sm font-medium"
            >
              上一步
            </button>
            <div v-else class="flex-1"></div>
            <button 
              @click="nextStep" 
              :disabled="loading" 
              type="button"
              class="flex-1 py-2.5 px-4 rounded-lg bg-blue-500 text-white hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-200 text-sm font-medium flex items-center justify-center"
            >
              <span v-if="loading" class="mr-2">
                <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </span>
              <span>{{ currentStep === 3 ? '完成注册' : '下一步' }}</span>
            </button>
          </div>
        </n-form>

        <!-- 登录链接 -->
        <div v-if="currentStep < 4" class="mt-6 text-center">
          <p class="text-sm text-gray-500 dark:text-gray-400">
            已有账号？
            <a href="/Login" class="text-blue-500 hover:text-blue-600 dark:text-blue-400 dark:hover:text-blue-300 font-medium">
              登录
            </a>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, reactive} from 'vue'
import {useRouter} from 'vue-router'
import {NInput, NForm, NFormItem, NSelect} from 'naive-ui'
import {useAuthorizationStore} from '../stores/Authorization'
import {ios} from '../services/AuthService'
import {isWeiXin, NavigateTo} from "../lib/site";

const router = useRouter()
const authorizationStore = useAuthorizationStore()

// 表单引用
const formRef = ref<InstanceType<typeof NForm>>()

// 当前步骤
const currentStep = ref(1)

// 步骤标题
const steps = ['基本信息', '学校信息', '账号设置']

// 表单数据（使用 reactive 以便 Naive UI 正确高亮验证状态）
const form = reactive({
  name: '',
  studentId: '',
  gender: '',
  political: '',
  major: '',
  className: '',
  phone: '',
  email: '',
  password: '',
  confirmPassword: ''
})

// 状态变量
const loading = ref(false)
const errorMsg = ref('')

// 下拉选项
const genderOptions = [
  {label: '男', value: '男'},
  {label: '女', value: '女'}
]

const politicalOptions = [
  {label: '群众', value: '群众'},
  {label: '共青团员', value: '共青团员'},
  {label: '中共预备党员', value: '中共预备党员'},
  {label: '中共党员', value: '中共党员'},
]

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

// 邮箱校验器（复用）
const emailValidator = (_rule: any, value: string) => {
  if (!value) return true // 让 required 规则处理空值
  const emailReg = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailReg.test(value) || '请输入有效的邮箱地址'
}

// 下一步
const nextStep = async () => {
  errorMsg.value = ''
  
  // 验证当前步骤的表单
  if (!formRef.value) return

  // 先做一个轻量的必填检查作为回退（防止 Naive UI validate 在某些版本下未能触发）
  const requiredMissing = (() => {
    if (currentStep.value === 1) {
      return !form.name || !form.studentId || !form.gender || !form.political
    }
    if (currentStep.value === 2) {
      return !form.major || !form.className || !form.phone || !form.email
    }
    if (currentStep.value === 3) {
      return !form.password || !form.confirmPassword
    }
    return false
  })()

  if (requiredMissing) {
    // 触发 Naive UI 的 validate 来显示红色提示（如果可用），并阻止前进
    try {
      await formRef.value.validate()
    } catch (e) {
      // ignore, validate should show field errors
    }
    errorMsg.value = '请填写本步骤的所有必填项'
    return
  }
  
  try {
    // 使用 validate 方法验证特定字段
    if (currentStep.value === 1) {
      await formRef.value.validate()
    } else if (currentStep.value === 2) {
      await formRef.value.validate()
    } else if (currentStep.value === 3) {
      await formRef.value.validate()
    }
    
    // 如果是最后一步，提交注册
    if (currentStep.value === 3) {
      await submitRegistration()
    } else {
      // 否则进入下一步
      currentStep.value++
      // 滚动到页面顶部
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }
  } catch (err: any) {
    // 验证失败，不执行下一步
    // 从错误信息中提取字段名并检查是否属于当前步骤
    if (err && Array.isArray(err)) {
      const currentStepFields = 
        currentStep.value === 1 ? ['name', 'studentId', 'gender', 'political'] :
        currentStep.value === 2 ? ['major', 'className', 'phone', 'email'] :
        currentStep.value === 3 ? ['password', 'confirmPassword'] : [];
      
      // 检查错误是否与当前步骤相关
      const hasCurrentStepError = err.some((error: any) => 
        error && currentStepFields.includes(error.field)
      );
      
      if (hasCurrentStepError) {
        return; // 有当前步骤的错误，不进入下一步
      }
      
      // 如果错误不是当前步骤的，则进入下一步
      if (currentStep.value === 3) {
        await submitRegistration()
      } else {
        currentStep.value++
        window.scrollTo({ top: 0, behavior: 'smooth' })
      }
    }
    return
  }
}

// 上一步
const prevStep = () => {
  if (currentStep.value > 1) {
    currentStep.value--
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

// 提交注册
const submitRegistration = async () => {
  loading.value = true
  errorMsg.value = ''
  
  try {
    // 密码哈希处理
    const encoder = new TextEncoder()
  const data = encoder.encode(form.password)
    const hashBuffer = await crypto.subtle.digest('SHA-256', data)
    const hashArray = Array.from(new Uint8Array(hashBuffer))
    const passwordHash = hashArray.map(b => b.toString(16).padStart(2, '0')).join('')

    // 提交注册请求
    const res = await authorizationStore.signup({
      userName: form.name,
      userId: form.studentId,
      academy: form.major,
      politicalLandscape: form.political,
      gender: form.gender,
      className: form.className,
      phoneNum: form.phone,
      joinTime: new Date().toISOString(),
      passwordHash: passwordHash,
      eMail: form.email
    })

    if (!res) {
      errorMsg.value = '注册失败，请稍后再试'
      return
    }
    
    // 显示成功步骤
    currentStep.value = 4
    
    // 延迟跳转
    setTimeout(async () => {
      const w = isWeiXin()
      if (w) {
        await router.push('QrCode')
      } else {
        NavigateTo(ios.url1, ios.url2)
      }
    }, 1200)

  } catch (err: any) {
    errorMsg.value = err.message || '请求失败，请稍后再试'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* 自定义NaiveUI组件样式 */
:deep(.n-form-item__label) {
  font-weight: 500;
  color: var(--color-foreground);
}

:deep(.n-input__border) {
  border-radius: 0.5rem;
  border-color: var(--color-border);
}

:deep(.n-input__border:hover) {
  border-color: var(--color-primary);
}

:deep(.n-input__border:focus-within) {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

:deep(.n-select__menu) {
  border-radius: 0.5rem;
}
</style>

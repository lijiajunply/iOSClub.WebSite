<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8 bg-white bg-opacity-70 backdrop-blur-lg rounded-xl shadow-xl p-8">
      <div class="text-center">
        <h2 class="text-3xl font-bold text-gray-900">注册</h2>
        <p class="mt-1 text-sm text-gray-500">
          {{ stepDescriptions[currentStep] }}
        </p>
      </div>

      <n-form
        ref="formRef"
        :model="formData"
        :rules="rules"
        @submit.prevent="handleSubmit"
        class="mt-8 space-y-6"
      >
        <!-- 步骤1: 基本信息 -->
        <div v-if="currentStep === 0" class="space-y-4">
          <n-form-item label="姓名" path="userName">
            <n-input
              v-model:value="formData.userName"
              placeholder="请输入您的姓名"
              clearable
            />
          </n-form-item>

          <n-form-item label="学号" path="userId">
            <n-input
              v-model:value="formData.userId"
              placeholder="请输入您的学号"
              clearable
            />
          </n-form-item>
        </div>

        <!-- 步骤2: 学院信息 -->
        <div v-if="currentStep === 1" class="space-y-4">
          <n-form-item label="学院" path="academy">
            <n-select
              v-model:value="formData.academy"
              placeholder="请选择学院"
              :options="academyOptions"
              clearable
            />
          </n-form-item>

          <n-form-item label="专业班级" path="className">
            <n-input
              v-model:value="formData.className"
              placeholder="请输入您的专业班级"
              clearable
            />
          </n-form-item>

          <n-form-item label="手机号码" path="phoneNum">
            <n-input
              v-model:value="formData.phoneNum"
              placeholder="请输入您的手机号码"
              clearable
            />
          </n-form-item>
        </div>

        <!-- 步骤3: 个人信息 -->
        <div v-if="currentStep === 2" class="space-y-4">
          <n-form-item label="性别" path="gender">
            <n-radio-group v-model:value="formData.gender">
              <n-space>
                <n-radio
                  v-for="gender in genders"
                  :key="gender"
                  :value="gender"
                  :label="gender"
                />
              </n-space>
            </n-radio-group>
          </n-form-item>

          <n-form-item label="政治面貌" path="politicalLandscape">
            <n-radio-group v-model:value="formData.politicalLandscape">
              <n-space>
                <n-radio
                  v-for="landscape in politicalLandscapes"
                  :key="landscape"
                  :value="landscape"
                  :label="landscape"
                />
              </n-space>
            </n-radio-group>
          </n-form-item>
        </div>

        <div class="flex justify-center space-x-4 pt-4">
          <n-button
            v-if="currentStep > 0"
            type="primary"
            @click="prevStep"
          >
            退回
          </n-button>
          
          <n-button
            type="primary"
            attr-type="submit"
          >
            {{ currentStep === 2 ? '完成注册' : '下一步' }}
          </n-button>
        </div>

        <div v-if="currentStep === 0" class="text-center text-sm text-gray-500 mt-4">
          已经有iMember账户了？
          <n-button text type="primary" tag="a" href="/login">现在登录!</n-button>
        </div>
      </n-form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import {
  NForm,
  NFormItem,
  NInput,
  NSelect,
  NRadioGroup,
  NRadio,
  NButton,
  useMessage,
  createDiscreteApi
} from 'naive-ui'

const { message } = createDiscreteApi(['message'])

// 表单步骤
const currentStep = ref(0)
const stepDescriptions = [
  '创建您的iMember账户',
  '请填写您的个人信息',
  '即将初始化您的iMember账户'
]

// 表单数据
const formData = reactive({
  userName: '',
  userId: '',
  academy: '',
  className: '',
  phoneNum: '',
  gender: '',
  politicalLandscape: ''
})

// 表单验证规则
const rules = {
  userName: [
    { required: true, message: '请输入姓名', trigger: 'blur' },
    { min: 2, max: 10, message: '姓名长度应在2-10个字符之间', trigger: 'blur' }
  ],
  userId: [
    { required: true, message: '请输入学号', trigger: 'blur' },
    { pattern: /^\d+$/, message: '学号应为数字', trigger: 'blur' }
  ],
  academy: [
    { required: true, message: '请选择学院', trigger: 'change' }
  ],
  className: [
    { required: true, message: '请输入专业班级', trigger: 'blur' }
  ],
  phoneNum: [
    { required: true, message: '请输入手机号码', trigger: 'blur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号码', trigger: 'blur' }
  ],
  gender: [
    { required: true, message: '请选择性别', trigger: 'change' }
  ],
  politicalLandscape: [
    { required: true, message: '请选择政治面貌', trigger: 'change' }
  ]
}

// 选项数据
const academyOptions = [
  '建筑学院',
  '土木工程学院',
  '环境与市政工程学院',
  '管理学院',
  '信息与控制工程学院',
  '艺术学院',
  '文学院',
  '理学院',
  '体育学院'
].map(v => ({ label: v, value: v }))

const genders = ['男', '女']
const politicalLandscapes = ['群众', '共青团员', '中共党员', '其他']

const formRef = ref(null)

// 上一步
const prevStep = () => {
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

// 表单提交
const handleSubmit = async (e) => {
  e.preventDefault()
  
  try {
    await formRef.value?.validate()
    
    if (currentStep.value < 2) {
      currentStep.value++
    } else {
      // 最后一步，提交表单
      await submitForm()
    }
  } catch (errors) {
    console.error('表单验证失败:', errors)
  }
}

// 提交表单到后端
const submitForm = async () => {
  try {
    // 这里应该是实际的API调用
    // const response = await api.register(formData)
    message.success('注册成功!')
    
    // 检查是否在微信中打开
    if (isWeiXin()) {
      // 微信中跳转到IOSPic页面
      window.location.href = '/IOSPic'
    } else {
      // 其他浏览器跳转到QQ或HTTPS链接
      window.location.href = 'https://your-ios-club-link.com'
    }
  } catch (error) {
    message.error('注册失败: ' + error.message)
  }
}

// 检查是否在微信中打开
const isWeiXin = () => {
  const ua = navigator.userAgent.toLowerCase()
  return ua.indexOf('micromessenger') !== -1
}
</script>

<style>
/* 响应式设计 */
@media (max-width: 768px) {
  .max-w-md {
    width: 100%;
    box-shadow: none;
    border-radius: 0;
  }
  
  .min-h-screen {
    align-items: flex-start;
    padding-top: 2rem;
  }
}
</style>
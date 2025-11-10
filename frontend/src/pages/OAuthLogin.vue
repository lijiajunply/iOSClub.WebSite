<template>
  <div
      class="min-h-screen bg-gradient-to-br from-slate-100 to-slate-200 dark:from-neutral-900 dark:to-neutral-800 flex items-center justify-center p-4 transition-colors duration-300">
    <div
        class="w-full max-w-md bg-white/80 dark:bg-neutral-900/80 backdrop-blur-lg rounded-2xl shadow-xl p-8 transition-all duration-300">
      <div class="text-center mb-8">
        <h1 class="text-xl md:text-3xl font-semibold bg-gradient-to-r from-blue-500 to-indigo-500 bg-clip-text text-transparent">
          iMember 登录授权
        </h1>
        <p class="text-gray-500 dark:text-gray-400 mt-2">
          第三方应用请求访问您的账号
        </p>
        <div v-if="clientAppInfo" class="mt-4 p-3 bg-blue-50 dark:bg-blue-900/30 rounded-lg">
          <p class="text-sm text-gray-600 dark:text-gray-300">
            应用名称: <span class="font-medium">{{ clientAppInfo.name || '未知应用' }}</span>
          </p>
        </div>
      </div>

      <n-form :model="form" :rules="rules" ref="formRef">
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
              placeholder="请输入密码 (未设置时为手机号码)"
              show-password-on="click"
              @keyup.enter="handleLogin"
              class="dark:text-gray-100 dark:bg-neutral-800"
          />
        </n-form-item>

        <div class="flex justify-between mb-4">
          <n-checkbox v-model:checked="form.rememberMe" class="dark:text-gray-100">记住我</n-checkbox>
          <router-link to="/ForgotPassword" class="float-right text-blue-500 dark:text-blue-300">忘记密码?</router-link>
        </div>

        <button
            @click="handleLogin"
            :disabled="loading"
            class="btn"
        >
          <span v-if="loading">登录中...</span>
          <span v-else>授权登录</span>
        </button>
        <div v-if="errorMsg" class="text-red-500 text-center mt-4">{{ errorMsg }}</div>
      </n-form>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          没有账号？
          <router-link to="/SignUp" class="text-blue-600 dark:text-purple-400 font-medium hover:underline">
            注册
          </router-link>
        </p>
        <p class="text-xs text-gray-400 dark:text-gray-500 mt-2">
          授权后，第三方应用将获得您的基本信息访问权限
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref} from 'vue'
import {useRoute} from 'vue-router'
import {NCheckbox, NForm, NFormItem, NInput} from 'naive-ui'
import {useAuthorizationStore} from "../stores/Authorization.js";
import {OAuthService} from '../services/OAuthService';
import {url} from '../services/Url';

const authorizationStore = useAuthorizationStore()
const route = useRoute()

const formRef = ref()
const form = ref({
  studentId: '',
  password: '',
  rememberMe: false
})
const loading = ref(false)
const errorMsg = ref('')
const clientAppInfo = ref<any>(null)

const rules = {
  studentId: {
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

// 从URL参数中获取OAuth相关参数
const getOAuthParams = () => {
  return {
    state: route.query.state as string || '',
    client_id: route.query.client_id as string || '',
    redirect_uri: route.query.redirect_uri as string || '',
    response_type: route.query.response_type as string || 'code',
    scope: route.query.scope as string || 'profile openid role',
  }
}

// 加载客户端应用信息
const loadClientAppInfo = async () => {
  const params = getOAuthParams()
  if (params.client_id) {
    clientAppInfo.value = await OAuthService.loadClientAppInfo(params.client_id);
  }
}

const handleLogin = async () => {
  if (loading.value) return
  formRef.value.validate(async (errors: any) => {
    if (!errors) {
      loading.value = true
      errorMsg.value = ''
      try {
        // 执行登录
        const res = await authorizationStore.login({
          userId: form.value.studentId,
          password: form.value.password,
          rememberMe: form.value.rememberMe
        }, Array.isArray(route.query.client_id) ? route.query.client_id[0] : route.query.client_id ?? '')

        if (!res) {
          errorMsg.value = '登录失败，请检查账号密码'
          return
        }

        // 获取OAuth参数
        const params = getOAuthParams()

        // 登录成功，先调用API将用户信息存储到服务器会话中
        const sessionStored = await OAuthService.storeSession(
            params.state,
            params.client_id,
            authorizationStore.getAuthorization
        );

        if (sessionStored) {
          // 会话存储成功，重定向到SSO/callback端点
          window.location.href = `${url}/SSO/callback?state=${encodeURIComponent(params.state)}`
        } else {
          errorMsg.value = '会话创建失败，请稍后重试'
        }
      } catch (err: any) {
        errorMsg.value = err.message || '登录失败'
      } finally {
        loading.value = false
      }
    }
  })
}

// 组件挂载时加载客户端应用信息
onMounted(() => {
  loadClientAppInfo()
})
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
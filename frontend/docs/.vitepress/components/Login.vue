<template>
    <div
        class="page-flex min-h-screen flex justify-center items-center p-4 bg-gradient-to-br from-blue-50 via-gray-50 to-indigo-50">
        <div class="login-container">
            <div class="logo-container">
                <div class="logo">iOS</div>
                <h2 class="text-2xl font-bold text-gray-800 mb-1">登录到iMember</h2>
            </div>
            <form @submit.prevent="handleSubmit" class="space-y-6">
                <div class="item">
                    <label class="block text-gray-700 text-sm font-medium mb-1">
                        姓名
                        <span class="text-red-500">*</span>
                    </label>
                    <input v-model="loginData.name" class="apple-input" placeholder="请输入您的姓名" required>
                </div>
                <div class="item">
                    <label class="block text-gray-700 text-sm font-medium mb-1">
                        学号
                        <span class="text-red-500">*</span>
                    </label>
                    <input v-model="loginData.id" class="apple-input" placeholder="请输入您的学号" required>
                </div>
                <div v-if="errorMessage" class="error-message">
                    {{ errorMessage }}
                </div>
                <div class="text-center mt-6">
                    <button type="submit" class="apple-button item">
                        <span v-if="isLoading">
                            登录中...
                        </span>
                        <span v-else>
                            登录
                        </span>
                    </button>
                    <p class="text-gray-600 mt-4 text-sm">
                        还没有账户？
                        <a href="/signup" class="signup-link">立即注册!</a>
                    </p>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue'

const loginData = ref({
    name: '',
    id: ''
})

// 加载状态
const isLoading = ref(false)

// 错误消息
const errorMessage = ref('')

// 模拟登录成功
const loginSuccess = () => {
    // 在实际应用中，这里会跳转到中心页面
    alert('登录成功！即将跳转到个人中心')

    // 模拟跳转到个人中心
    setTimeout(() => {
        window.location.href = "/Centre"
    }, 1500)
}

// 处理表单提交
const handleSubmit = () => {
    errorMessage.value = ''

    // 简单的客户端验证
    if (!loginData.value.name || !loginData.value.id) {
        errorMessage.value = '请输入姓名和学号'
        return
    }

    isLoading.value = true

    // 模拟API请求延迟
    setTimeout(() => {
        // 模拟登录逻辑
        if (loginData.value.id.startsWith('20')) {
            loginSuccess()
        } else {
            errorMessage.value = '用户账号信息出错，请检查后重试'
        }

        isLoading.value = false
    }, 1200)
}
</script>

<style>
.login-container {
    backdrop-filter: blur(10px);
    background-color: rgba(255, 255, 255, 0.7);
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    padding: 2rem;
    border-radius: 1rem;
    width: 28rem;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.login-container:hover {
    transform: translateY(-5px);
    box-shadow: 0 15px 30px rgba(0, 0, 0, 0.15);
}

.apple-input {
    border: 1px solid #d1d5db;
    transition: all 0.3s ease;
    width: 100%;
    line-height: 1.15;
    margin: 0;
    border-radius: .5rem;
    padding: .75rem 1rem;
    overflow: hidden;
    font-size: 1rem;
}

.apple-input:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
}

.apple-button {
    background-color: #2563eb;
    color: white;
    border: none;
    border-radius: 0.5rem;
    padding: 0.75rem 1.5rem;
    font-weight: 500;
    font-size: 1rem;
    cursor: pointer;
    transition: all 0.3s ease;
    width: 100%;
}

.apple-button:hover {
    background-color: #1d4ed8;
    transform: translateY(-2px);
}

.apple-button:active {
    transform: translateY(1px);
}

.signup-link {
    color: #2563eb;
    font-weight: 500;
    text-decoration: none;
    transition: color 0.2s;
}

.signup-link:hover {
    color: #1e40af;
    text-decoration: underline;
}

@media screen and (max-width: 768px) {
    .login-container {
        width: 100%;
        box-shadow: none;
        border-radius: 0;
        padding: 1.5rem;
    }

    .page-flex {
        align-items: flex-start !important;
        padding-top: 2rem;
    }

    body {
        font-size: 16px;
    }
}

.logo-container {
    text-align: center;
    margin-bottom: 1.5rem;
}

.logo {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    background: linear-gradient(135deg, #3b82f6, #8b5cf6);
    display: inline-flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-weight: bold;
    font-size: 1.5rem;
    margin-bottom: 1rem;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}

.error-message {
    color: #ef4444;
    font-size: 0.875rem;
    margin-top: 0.25rem;
    text-align: center;
}

.item{
    margin-bottom: 1rem;
}
</style>
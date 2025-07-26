<template>
  <div class="login-page-container">
    <div class="noise-overlay"></div>
    <div class="login-card">
      <div class="card-header">用户登录/注册</div>
      <div class="tabs-nav">
        <div
            v-for="(tab, index) in tabs"
            :key="tab.index"
            :class="['tab-item', { active: activeTab === tab.name }]"
            @click="switchTab(tab.name)"
            role="button"
            aria-label="切换到"
        >
          {{ tab.label }}
        </div>
      </div>
      <!-- 登录表单面板 -->
      <div class="pane-wrapper" v-if="activeTab === 'signin'">
        <form @submit.prevent="handleLogin">
          <div class="form-group">
            <label for="signin-username">用户名</label>
            <input
                type="text"
                id="signin-username"
                v-model="loginForm.username"
                placeholder="请输入用户名"
                required
                :class="{ 'invalid': loginErrors.username }"
            />
            <p class="error-message">{{ loginErrors.username }}</p>
          </div>
          <div class="form-group">
            <label for="signin-studentId">学号</label>
            <input
                type="text"
                id="signin-studentId"
                v-model="loginForm.studentId"
                placeholder="请输入学号"
                required
                :class="{ 'invalid': loginErrors.studentId }"
            />
            <p class="error-message">{{ loginErrors.studentId }}</p>
          </div>
          <div class="form-group">
            <label for="signin-password">密码</label>
            <input
                type="password"
                id="signin-password"
                v-model="loginForm.password"
                placeholder="请输入密码"
                required
                :minlength="6"
                :class="{ 'invalid': loginErrors.password }"
            />
            <p class="error-message">{{ loginErrors.password }}</p>
          </div>
          <button type="submit" class="btn" :disabled="isSubmitting">
            <span v-if="isSubmitting">登录中...</span>
            <span v-else>登录</span>
          </button>
        </form>
      </div>
      <!-- 注册表单面板 -->
      <div class="pane-wrapper" v-if="activeTab === 'signup'">
        <form @submit.prevent="handleRegister">
          <div class="form-group">
            <label for="signup-username">用户名</label>
            <input
                type="text"
                id="signup-username"
                v-model="registerForm.username"
                placeholder="请输入用户名"
                required
                :class="{ 'invalid': registerErrors.username }"
            />
            <p class="error-message">{{ registerErrors.username }}</p>
          </div>
          <div class="form-group">
            <label for="signup-studentId">学号</label>
            <input
                type="text"
                id="signup-studentId"
                v-model="registerForm.studentId"
                placeholder="请输入学号"
                required
                :class="{ 'invalid': registerErrors.studentId }"
            />
            <p class="error-message">{{ registerErrors.studentId }}</p>
          </div>
          <div class="form-group">
            <label for="signup-password">密码</label>
            <input
                type="password"
                id="signup-password"
                v-model="registerForm.password"
                placeholder="请输入密码（至少6位）"
                required
                minlength="6"
                :class="{ 'invalid': registerErrors.password }"
            />
            <p class="error-message">{{ registerErrors.password }}</p>
          </div>
          <div class="form-group">
            <label for="signup-confirmPassword">重复密码</label>
            <input
                type="password"
                id="signup-confirmPassword"
                v-model="registerForm.confirmPassword"
                placeholder="请重复输入密码"
                required
                minlength="6"
                :class="{ 'invalid': registerErrors.confirmPassword }"
            />
            <p class="error-message">{{ registerErrors.confirmPassword }}</p>
          </div>
          <button type="submit" class="btn" :disabled="isSubmitting">
            <span v-if="isSubmitting">注册中...</span>
            <span v-else>注册</span>
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import {ref, reactive} from 'vue';
import {useRouter} from 'vue-router'; // 假设使用Vue Router进行路由跳转

// 标签数据
const tabs = [
  {name: 'signin', label: '登录'},
  {name: 'signup', label: '注册'}
];

// 状态管理
const activeTab = ref('signin');
const isSubmitting = ref(false);
const router = useRouter();

// 登录表单数据
const loginForm = reactive({
  username: '',
  studentId: '',
  password: ''
});

// 注册表单数据
const registerForm = reactive({
  username: '',
  studentId: '',
  password: '',
  confirmPassword: ''
});

// 错误信息
const loginErrors = reactive({
  username: '',
  studentId: '',
  password: ''
});

const registerErrors = reactive({
  username: '',
  studentId: '',
  password: '',
  confirmPassword: ''
});

// 切换标签
const switchTab = (tabName) => {
  activeTab.value = tabName;
  // 切换时清空错误信息
  Object.keys(loginErrors).forEach(key => loginErrors[key] = '');
  Object.keys(registerErrors).forEach(key => registerErrors[key] = '');
};

// 验证登录表单
const validateLoginForm = () => {
  let isValid = true;

  if (!loginForm.username.trim()) {
    loginErrors.username = '用户名不能为空';
    isValid = false;
  } else {
    loginErrors.username = '';
  }

  if (!loginForm.studentId.trim()) {
    loginErrors.studentId = '学号不能为空';
    isValid = false;
  } else {
    loginErrors.studentId = '';
  }

  if (!loginForm.password) {
    loginErrors.password = '密码不能为空';
    isValid = false;
  } else if (loginForm.password.length < 6) {
    loginErrors.password = '密码长度不能少于6位';
    isValid = false;
  } else {
    loginErrors.password = '';
  }

  return isValid;
};

// 验证注册表单
const validateRegisterForm = () => {
  let isValid = true;

  if (!registerForm.username.trim()) {
    registerErrors.username = '用户名不能为空';
    isValid = false;
  } else {
    registerErrors.username = '';
  }

  if (!registerForm.studentId.trim()) {
    registerErrors.studentId = '学号不能为空';
    isValid = false;
  } else {
    registerErrors.studentId = '';
  }

  if (!registerForm.password) {
    registerErrors.password = '密码不能为空';
    isValid = false;
  } else if (registerForm.password.length < 6) {
    registerErrors.password = '密码长度不能少于6位';
    isValid = false;
  } else {
    registerErrors.password = '';
  }

  if (!registerForm.confirmPassword) {
    registerErrors.confirmPassword = '请再次输入密码';
    isValid = false;
  } else if (registerForm.password !== registerForm.confirmPassword) {
    registerErrors.confirmPassword = '两次输入的密码不一致';
    isValid = false;
  } else {
    registerErrors.confirmPassword = '';
  }

  return isValid;
};

// 处理登录
const handleLogin = async () => {
  if (!validateLoginForm()) return;

  try {
    isSubmitting.value = true;

    // 模拟登录请求
    console.log('登录请求数据:', {...loginForm});

    // 实际项目中替换为真实的API请求
    await new Promise(resolve => setTimeout(resolve, 1000));

    // 登录成功后的处理
    console.log('登录成功');
    // 这里可以添加登录成功后的逻辑，如存储token等
    router.push('/'); // 登录成功后跳转到首页
  } catch (error) {
    console.error('登录失败:', error);
    alert('登录失败，请检查您的信息');
  } finally {
    isSubmitting.value = false;
  }
};

// 处理注册
const handleRegister = async () => {
  if (!validateRegisterForm()) return;

  try {
    isSubmitting.value = true;

    // 模拟注册请求
    console.log('注册请求数据:', {...registerForm});

    // 实际项目中替换为真实的API请求
    await new Promise(resolve => setTimeout(resolve, 1000));

    // 注册成功后的处理
    console.log('注册成功');
    alert('注册成功，请登录');
    switchTab('signin'); // 注册成功后切换到登录标签
  } catch (error) {
    console.error('注册失败:', error);
    alert('注册失败，请稍后再试');
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
.login-page-container {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
  position: relative;
  background: linear-gradient(135deg, #FF99C8 0%, #646EF8 100%);
}

.noise-overlay {
  position: absolute;
  width: 100%;
  height: 100%;
  background: url("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAIAAACRXR/mAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAGElEQVR4nO3BAQ0AAADCoPdPbQ43oAAAAASUVORK5CYII=");
  opacity: 0.12;
  pointer-events: none;
}

/* 登录卡片 */
.login-card {
  width: 100%;
  max-width: 400px;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(12px);
  box-shadow: inset 0 0 16px rgba(255, 255, 255, 0.5), 0 8px 32px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
  position: relative;
  z-index: 1;
}

.login-card:hover {
  transform: translateY(-6px);
  box-shadow: inset 0 0 16px rgba(255, 255, 255, 0.5), 0 12px 40px rgba(0, 0, 0, 0.15);
}

/* 标题样式 */
.card-header {
  padding: 24px;
  font-size: 20px;
  font-weight: 700;
  color: #646EF8;
  border-bottom: 1px solid #F5F5FF;
  text-align: center;
}

/* 标签栏样式 */
.tabs-nav {
  display: flex;
  border-bottom: 1px solid #F5F5FF;
}

.tab-item {
  flex: 1;
  text-align: center;
  padding: 16px 0;
  color: #FF99C8;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.tab-item.active {
  color: #646EF8;
  border-bottom: 3px solid #646EF8;
  font-weight: 600;
}

.tab-item:hover:not(.active) {
  color: #646EF8;
  background-color: rgba(255, 153, 200, 0.05);
}

/* 表单样式 */
.pane-wrapper {
  padding: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #646EF8;
  font-weight: 500;
}

.form-group input {
  width: 100%;
  padding: 12px 15px;
  border: 2px solid #F5F5FF;
  border-radius: 12px;
  background: #FAFAFF;
  outline: none;
  transition: border-color 0.3s ease, box-shadow 0.3s ease;
}

.form-group input:focus {
  border-color: #FF99C8;
  box-shadow: 0 0 0 3px rgba(255, 153, 200, 0.1);
}

.form-group input.invalid {
  border-color: #ff6b6b;
}

.error-message {
  margin: 5px 0 0;
  color: #ff6b6b;
  font-size: 12px;
  height: 16px;
}

/* 按钮样式 */
.btn {
  width: 100%;
  padding: 14px 0;
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

/* 适配移动端 */
@media (max-width: 480px) {
  .login-page-container {
    padding: 15px;
  }

  .login-card {
    border-radius: 16px;
  }

  .card-header {
    padding: 20px;
    font-size: 18px;
  }

  .pane-wrapper {
    padding: 20px;
  }

  .form-group input {
    padding: 11px 14px;
  }

  .btn {
    padding: 13px 0;
    font-size: 15px;
  }
}
</style>
<template>
  <div class="sidebar">
    <div class="sidebar-header">
      <img
          src="../assets/iOS_Club_LOGO.png"
          alt="iOS Club Logo"
          class="sidebar-logo"
          @error="handleImageError"
      />
      <h2 class="sidebar-title">iMember</h2>
    </div>

    <nav class="sidebar-nav">
      <ul>
        <li v-for="item in menuItems" :key="item.name" class="sidebar-item">
          <router-link
              :to="item.path"
              class="sidebar-link"
              :class="{ 'active': $route.path === item.path || ($route.path === '/Centre' && item.path === '/Centre') }"
              @click="closeSidebar"
          >
            <span class="sidebar-icon">{{ item.icon }}</span>
            <span class="sidebar-text">{{ item.name }}</span>
          </router-link>
        </li>
      </ul>
    </nav>

    <div class="sidebar-footer">
      <button @click="logout" class="logout-button">
        <span class="sidebar-icon">🚪</span>
        <span class="sidebar-text">退出登录</span>
      </button>
    </div>
  </div>
</template>

<script setup>
import { useRouter, useRoute } from 'vue-router'
import { useAuthorizationStore } from '../stores/Authorization'

const router = useRouter()
const route = useRoute()
const authorizationStore = useAuthorizationStore()

const menuItems = [
  { name: '主页', path: '/Centre', icon: '🏠' },
  { name: '个人数据', path: '/Centre/PersonalData', icon: '👤' },
  { name: '社团部门', path: '/Centre/Department', icon: '🏢' },
  { name: '社团资源', path: '/Centre/Resources', icon: '📚' },
  { name: '社团文章', path: '/Centre/Article', icon: '📊' },
  { name: '成员数据', path: '/Centre/MemberData', icon: '👥' },
  { name: '其他数据', path: '/Centre/Admin', icon: '🚀' }
]

const logout = () => {
  authorizationStore.logout()
  router.push('/')
}

const closeSidebar = () => {
  // 如果需要在移动设备上关闭侧边栏，可以在这里添加逻辑
}

const handleImageError = (e) => {
  e.target.src = '../assets/default-logo.png'
}
</script>

<style scoped>
.sidebar {
  width: 250px;
  height: 100vh;
  color: black;
  display: flex;
  flex-direction: column;
  position: fixed;
  top: 0;
  left: 0;
  z-index: 1000;
  box-shadow: 2px 0 5px rgba(0,0,0,0.1);
}

.sidebar-header {
  padding: 20px;
  display: flex;
  align-items: center;
  border-bottom: 1px solid rgba(0,0,0,0.1);
}

.sidebar-logo {
  width: 40px;
  height: 40px;
  margin-right: 10px;
}

.sidebar-title {
  font-size: 1.5rem;
  font-weight: bold;
  color: black;
}

.sidebar-nav {
  flex: 1;
  padding: 20px 0;
  overflow-y: auto;
}

.sidebar-nav ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.sidebar-item {
  margin-bottom: 5px;
}

.sidebar-link {
  display: flex;
  align-items: center;
  padding: 12px 20px;
  color: rgba(0,0,0,0.8);
  text-decoration: none;
  transition: all 0.3s ease;
}

.sidebar-link:hover {
  background: rgba(255,255,255,0.3);
  color: black;
}

.sidebar-link.active {
  background: rgba(255,255,255,0.5);
  color: black;
  border-left: 4px solid black;
}

.sidebar-icon {
  font-size: 1.2rem;
  margin-right: 10px;
  width: 24px;
  text-align: center;
}

.sidebar-text {
  font-size: 1rem;
  color: black;
}

.sidebar-footer {
  padding: 20px;
  border-top: 1px solid rgba(0,0,0,0.1);
}

.logout-button {
  width: 100%;
  background: none;
  border: none;
  color: rgba(0,0,0,0.8);
  padding: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.logout-button:hover {
  color: black;
  background: rgba(255,255,255,0.3);
}

@media (max-width: 768px) {
  .sidebar {
    width: 200px;
  }

  .sidebar-text {
    font-size: 0.9rem;
  }
}
</style>

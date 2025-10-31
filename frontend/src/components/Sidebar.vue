<template>
  <div :style="{ marginLeft: layoutStore.showSidebar && !layoutStore.isMobile ? '250px' : '0', transition: 'margin-left 0.3s ease' }"
  >
  <!-- 移动端菜单按钮，仅在小屏幕显示 -->
  <button class="mobile-menu-btn" @click="layoutStore.toggleSidebar()" v-show="layoutStore.isMobile">
    <span class="menu-icon">☰</span>
  </button>

  <div class="sidebar" :class="{ 'sidebar-hidden': !layoutStore.showSidebar && layoutStore.isMobile }">
    <div class="sidebar-header">
      <img
          src="../../public/assets/iOS_Club_LOGO.png"
          alt="iOS Club Logo"
          class="sidebar-logo"
          @error="handleImageError"
      />
      <h2 class="sidebar-title">iMember</h2>
    </div>

    <nav class="sidebar-nav">
      <ul>
        <li v-for="item in filteredMenuItems" :key="item.name" class="sidebar-item">
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
  </div>
</template>

<script setup>
import { computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthorizationStore } from '../stores/Authorization'
import { useLayoutStore } from '../stores/LayoutStore'

const router = useRouter()
const route = useRoute()
const authorizationStore = useAuthorizationStore()
const layoutStore = useLayoutStore()

// 窗口大小变化时更新 isMobile
const handleResize = () => {
  layoutStore.handleResize()
}

onMounted(() => {
  window.addEventListener('resize', handleResize)
  // 初始化时也调用一次
  layoutStore.handleResize()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})

// 关闭侧边栏
const closeSidebar = () => {
  if (layoutStore.isMobile) {
    layoutStore.showSidebar = false
  }
}

// 解析JWT token获取用户身份
const getUserRole = () => {
  const token = authorizationStore.getAuthorization
  if (!token) return null

  try {
    const payload = token.split('.')[1]
    const decodedPayload = atob(payload)
    const userInfo = JSON.parse(decodedPayload)
    return userInfo['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        userInfo.role ||
        null
  } catch (e) {
    console.error('解析token失败:', e)
    return null
  }
}

const menuItems = [
  { name: '主页', path: '/Centre', icon: '🏠' },
  { name: '个人数据', path: '/Centre/PersonalData', icon: '👤' },
  { name: '社团部门', path: '/Centre/Department', icon: '🏢', requiresRole: 'Minister' },
  { name: '社团资源', path: '/Centre/Resources', icon: '📚', requiresRole: 'Minister' },
  { name: '社团文章', path: '/Centre/Article', icon: '📊', requiresRole: 'Minister' },
  { name: '成员数据', path: '/Centre/MemberData', icon: '👥', requiresRole: 'Minister' },
  { name: '其他数据', path: '/Centre/Admin', icon: '🚀', requiresRole: 'Minister' }
]

// 根据用户角色过滤菜单项
const filteredMenuItems = computed(() => {
  const userRole = getUserRole()
  if (!userRole) return menuItems.filter(item => !item.requiresRole)

  // 定义角色层级
  const roleHierarchy = {
    'Member': 1,
    'Department': 2,
    'Minister': 3,
    'President': 4,
    'Founder': 5
  }

  const userRoleLevel = roleHierarchy[userRole] || 0

  return menuItems.filter(item => {
    if (!item.requiresRole) return true

    const requiredRoleLevel = roleHierarchy[item.requiresRole] || 0
    return userRoleLevel >= requiredRoleLevel
  })
})

const logout = () => {
  authorizationStore.logout()
  router.push('/')
}

const handleImageError = (e) => {
  e.target.src = '../assets/default-logo.png'
}
</script>

<style scoped>
/* 移动端菜单按钮样式 */
.mobile-menu-btn {
  position: fixed;
  top: 10px;
  right: 10px;
  z-index: 1001;
  background: white;
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 8px 12px;
  cursor: pointer;
  display: none; /* 默认隐藏，在媒体查询中显示 */
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}
.menu-icon {
  font-size: 1.2rem;
}

/* 侧边栏隐藏样式 */
.sidebar-hidden {
  transform: translateX(-100%);
  transition: transform 0.3s ease;
}

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
  box-shadow: 2px 0 5px rgba(0, 0, 0, 0.1);
  background: linear-gradient(135deg, #f5f7fa 0%, #e4edf9 100%);
}

.sidebar-header {
  padding: 20px;
  display: flex;
  align-items: center;
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  background: rgba(255, 255, 255, 0.7);
}

.sidebar-logo {
  width: 40px;
  height: 40px;
  margin-right: 10px;
  border-radius: 50%;
}

.sidebar-title {
  font-size: 1.5rem;
  font-weight: bold;
  color: #333;
}

.sidebar-nav {
  flex: 1;
  padding: 20px 0;
  overflow-y: auto;
  background: rgba(255, 255, 255, 0.5);
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
  color: rgba(0, 0, 0, 0.8);
  text-decoration: none;
  transition: all 0.3s ease;
  border-left: 4px solid transparent;
}

.sidebar-link:hover {
  background: rgba(255, 255, 255, 0.7);
  color: #333;
  border-left: 4px solid #409eff;
}

.sidebar-link.active {
  background: rgba(255, 255, 255, 0.9);
  color: #333;
  border-left: 4px solid #409eff;
}

.sidebar-icon {
  font-size: 1.2rem;
  margin-right: 10px;
  width: 24px;
  text-align: center;
}

.sidebar-text {
  font-size: 1rem;
  color: #333;
}

.sidebar-footer {
  padding: 20px;
  border-top: 1px solid rgba(0, 0, 0, 0.1);
  background: rgba(255, 255, 255, 0.7);
}

.logout-button {
  width: 100%;
  background: none;
  border: none;
  color: rgba(0, 0, 0, 0.8);
  padding: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  font-size: 1rem;
  transition: all 0.3s ease;
  border-radius: 4px;
}

.logout-button:hover {
  color: #333;
  background: rgba(255, 255, 255, 0.7);
  border-left: 4px solid #f56c6c;
}

/* 媒体查询，移动端显示菜单按钮，调整侧边栏 */
@media (max-width: 768px) {
  .mobile-menu-btn {
    display: block;
  }
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    height: 100vh;
    width: 250px;
    z-index: 1000;
    transition: transform 0.3s ease;
  }

  .sidebar-header,
  .sidebar-nav,
  .sidebar-footer {
    background: rgba(255, 255, 255, 0.7);
  }
}
</style>

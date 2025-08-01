<template>
  <n-layout class="min-h-screen bg-gray-50">
    <n-layout-header
        class="apple-header"
        :class="{ 'scrolled': isScrolled }"
    >
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo & Title -->
          <router-link to="/" class="flex items-center gap-3 group">
            <img
                src="../assets/iOS_Club_LOGO.png"
                alt="iOS Club Logo"
                class="w-10 h-10 transition-transform group-hover:scale-105"
            />
            <div class="text-xl font-semibold text-gray-900 hidden sm:block">
              XAUAT iOS Club
            </div>
          </router-link>

          <!-- 桌面端菜单 -->
          <nav class="hidden md:flex items-center space-x-1">
            <n-dropdown
                trigger="hover"
                :options="aboutUsOptions"
                @select="handleSelect"
                placement="bottom"
            >
              <button class="nav-link">社团简介</button>
            </n-dropdown>
            
            <n-dropdown
                trigger="hover"
                :options="competitionOptions"
                @select="handleSelect"
                placement="bottom"
            >
              <button class="nav-link">竞赛资源</button>
            </n-dropdown>
            
            <n-dropdown
                trigger="hover"
                :options="activityOptions"
                @select="handleSelect"
                placement="bottom"
            >
              <button class="nav-link">社团活动</button>
            </n-dropdown>
            
            <n-dropdown
                trigger="hover"
                :options="historyOptions"
                @select="handleSelect"
                placement="bottom"
            >
              <button class="nav-link">社团历史</button>
            </n-dropdown>
            
            <button
                class="apple-button-primary ml-4"
                @click="() => router.push('/Login')"
            >
              登录/注册
            </button>
          </nav>

          <!-- 移动端菜单按钮 -->
          <button
              class="md:hidden apple-icon-button"
              @click="drawerVisible = !drawerVisible"
          >
            <n-icon size="24">
              <MenuOutline />
            </n-icon>
          </button>
        </div>
      </div>
    </n-layout-header>
    
    <n-layout-content class="pt-16">
      <transition name="slide-down">
        <div v-if="drawerVisible" class="mobile-menu">
          <div class="px-4 py-6 space-y-1">
            <router-link
                to="/"
                class="mobile-menu-item"
                @click="drawerVisible = false"
            >
              首页
            </router-link>
            
            <div class="mobile-menu-section">
              <div class="mobile-menu-section-title">社团简介</div>
              <router-link
                  v-for="item in aboutUsOptions"
                  :key="item.key"
                  :to="item.key"
                  class="mobile-menu-subitem"
                  @click="drawerVisible = false"
              >
                {{ item.label }}
              </router-link>
            </div>
            
            <div class="mobile-menu-section">
              <div class="mobile-menu-section-title">竞赛资源</div>
              <router-link
                  v-for="item in competitionOptions"
                  :key="item.key"
                  :to="item.key"
                  class="mobile-menu-subitem"
                  @click="drawerVisible = false"
              >
                {{ item.label }}
              </router-link>
            </div>
            
            <div class="mobile-menu-section">
              <div class="mobile-menu-section-title">社团活动</div>
              <router-link
                  v-for="item in activityOptions"
                  :key="item.key"
                  :to="item.key"
                  class="mobile-menu-subitem"
                  @click="drawerVisible = false"
              >
                {{ item.label }}
              </router-link>
            </div>
            
            <div class="mobile-menu-section">
              <div class="mobile-menu-section-title">社团历史</div>
              <router-link
                  v-for="item in historyOptions"
                  :key="item.key"
                  :to="item.key"
                  class="mobile-menu-subitem"
                  @click="drawerVisible = false"
              >
                {{ item.label }}
              </router-link>
            </div>

            <div class="pt-4 mt-4 border-t border-gray-200">
              <button
                  class="apple-button-primary w-full"
                  @click="() => { router.push('/Login'); drawerVisible = false }"
              >
                登录/注册
              </button>
            </div>
          </div>
        </div>
      </transition>

      <!-- 页面内容 -->
      <div v-if="!drawerVisible" class="min-h-screen">
        <router-view />
      </div>
    </n-layout-content>

    <!-- 页脚 -->
    <n-layout-footer class="apple-footer">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div class="text-center space-y-4">
          <p class="text-sm text-gray-600">
            Copyright © 2023 - 2024 XAUAT iOS Club
          </p>
          <div class="flex flex-wrap justify-center gap-4 text-sm">
            <a
                href="https://cn.xauat.edu.cn/"
                class="footer-link hidden sm:inline-flex items-center gap-1"
                target="_blank"
                rel="noopener"
            >
              西安建筑科技大学
            </a>
            <span class="text-gray-400 hidden sm:inline">·</span>
            <a
                href="https://beian.miit.gov.cn/"
                class="footer-link"
                target="_blank"
                rel="noopener"
            >
              陕ICP备2024031872号
            </a>
            <span class="text-gray-400 hidden sm:inline">·</span>
            <a
                href="https://gitee.com/XAUATiOSClub"
                class="footer-link hidden sm:inline-flex items-center gap-1"
                target="_blank"
                rel="noopener"
            >
              Gitee
            </a>
          </div>
        </div>
      </div>
    </n-layout-footer>
  </n-layout>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, RouterView } from 'vue-router'
import {
  NLayout,
  NLayoutHeader,
  NLayoutContent,
  NLayoutFooter,
  NDropdown,
  NIcon
} from 'naive-ui'
import { MenuOutline } from '@vicons/ionicons5'

const router = useRouter()
const drawerVisible = ref(false)
const isScrolled = ref(false)

// 处理滚动效果
const handleScroll = () => {
  isScrolled.value = window.scrollY > 10
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})

// 下拉菜单选项
const aboutUsOptions = [
  { label: '关于我们', key: '/About' },
  { label: '社团结构', key: '/Structure' },
  { label: '其他组织', key: '/OtherOrg' }
]

const competitionOptions = [
  { label: '资源支持', key: '/Article/Competitions' },
  { label: '移动应用创新赛', key: '/Article/MobileApplication' },
  { label: 'WWDC-Swift学生挑战赛', key: '/Article/Swift' }
]

const activityOptions = [
  { label: 'iOS Learn', key: '/Article/iOSLearn' },
  { label: '项目开发活动', key: '/Article/TimeToCode' },
  { label: '体验最新产品', key: '/Article/VisionPro' },
  { label: '一起看发布会', key: '/Article/PressConference' }
]

const historyOptions = [
  { label: '总述', key: '/Article/History-Overview' },
  { label: '历届干部', key: '/Article/PreviousCadres' },
  { label: '创社史', key: '/Article/History-Founding' },
  { label: '邵韩之治', key: '/Article/History-Shao Han\'s Reign' }
]

// 处理下拉菜单选择
const handleSelect = (key) => {
  router.push(key)
}
</script>

<style scoped>
/* 玻璃态 Header */
.apple-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 50;
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  height: 64px;
}

.apple-header.scrolled {
  background: rgba(255, 255, 255, 0.95);
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
}

/* 导航链接样式 */
.nav-link {
  padding: 0.5rem 1rem;
  color: #1d1d1f;
  font-size: 0.875rem;
  font-weight: 400;
  border-radius: 0.5rem;
  transition: all 0.2s ease;
  background: transparent;
  border: none;
  cursor: pointer;
}

.nav-link:hover {
  background: rgba(0, 0, 0, 0.05);
}

/* 主按钮样式 */
.apple-button-primary {
  padding: 0.5rem 1.25rem;
  background: #0071e3;
  color: white;
  font-size: 0.875rem;
  font-weight: 400;
  border-radius: 980px;
  border: none;
  cursor: pointer;
  transition: all 0.2s ease;
}

.apple-button-primary:hover {
  background: #0077ed;
  transform: scale(1.02);
}

.apple-button-primary:active {
  transform: scale(0.98);
}

/* 移动端图标按钮 */
.apple-icon-button {
  padding: 0.5rem;
  color: #1d1d1f;
  background: transparent;
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.apple-icon-button:hover {
  background: rgba(0, 0, 0, 0.05);
}

/* 移动端菜单 */
.mobile-menu {
  position: fixed;
  top: 64px;
  left: 0;
  right: 0;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  z-index: 40;
  max-height: calc(100vh - 64px);
  overflow-y: auto;
}

.mobile-menu-item {
  display: block;
  padding: 0.875rem 1rem;
  color: #1d1d1f;
  font-size: 1rem;
  font-weight: 400;
  border-radius: 0.75rem;
  transition: all 0.2s ease;
}

.mobile-menu-item:hover {
  background: rgba(0, 0, 0, 0.05);
}

.mobile-menu-section {
  margin-top: 1.5rem;
}

.mobile-menu-section-title {
  padding: 0.5rem 1rem;
  color: #86868b;
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.mobile-menu-subitem {
  display: block;
  padding: 0.75rem 1rem 0.75rem 2rem;
  color: #1d1d1f;
  font-size: 0.9375rem;
  font-weight: 400;
  border-radius: 0.75rem;
  transition: all 0.2s ease;
}

.mobile-menu-subitem:hover {
  background: rgba(0, 0, 0, 0.05);
}

/* 页脚 */
.apple-footer {
  background: #f5f5f7;
  border-top: 1px solid rgba(0, 0, 0, 0.1);
}

.footer-link {
  color: #0066cc;
  transition: all 0.2s ease;
}

.footer-link:hover {
  color: #0052cc;
  text-decoration: underline;
}

/* 下拉菜单 */
:deep(.n-dropdown-menu) {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(0, 0, 0, 0.1);
  border-radius: 0.75rem;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.12);
  padding: 0.5rem;
  min-width: 180px;
}

:deep(.n-dropdown-option) {
  color: #1d1d1f;
  padding: 0.625rem 1rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 400;
  transition: all 0.2s ease;
}

:deep(.n-dropdown-option:hover) {
  background: rgba(0, 0, 0, 0.05);
}

/* 动画 */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}

.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* 响应式 */
@media (max-width: 640px) {
  .apple-header {
    background: rgba(255, 255, 255, 0.95);
  }
}
</style>
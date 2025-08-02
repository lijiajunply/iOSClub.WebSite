<template>
  <n-layout class="min-h-screen bg-gray-50">
    <!-- Header with glassmorphism effect -->
    <n-layout-header
        class="apple-header"
        :class="{ 'scrolled': isScrolled }"
    >
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo and Title -->
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

          <!-- Desktop Menu -->
          <nav class="hidden md:flex items-center space-x-1">
            <!-- 关于我们 Dropdown -->
            <n-dropdown
                trigger="hover"
                :options="aboutUsOptions"
                @select="handleSelect"
                placement="bottom"
            >
              <button class="nav-link">
                关于我们
              </button>
            </n-dropdown>

            <!-- 社团动态 Dropdown -->
            <n-dropdown
                trigger="hover"
                :options="communityOptions"
                @select="handleSelect"
                placement="bottom"
            >
              <button class="nav-link">
                社团动态
              </button>
            </n-dropdown>

            <!-- Login/Register Button -->
            <button
                class="apple-button-primary ml-4"
                @click="() => router.push('/Login')"
            >
              登录/注册
            </button>
          </nav>

          <!-- Mobile Menu Button -->
          <button
              class="md:hidden apple-icon-button"
              @click="drawerVisible = !drawerVisible"
          >
            <n-icon size="24">
              <MenuOutline/>
            </n-icon>
          </button>
        </div>
      </div>
    </n-layout-header>

    <!-- Main Content -->
    <n-layout-content class="pt-16">
      <!-- Mobile Drawer Menu -->
      <transition name="slide-down">
        <div
            v-if="drawerVisible"
            class="mobile-menu"
        >
          <div class="px-4 py-6 space-y-1">
            <router-link
                to="/"
                class="mobile-menu-item"
                @click="drawerVisible = false"
            >
              首页
            </router-link>

            <!-- About Section -->
            <div class="mobile-menu-section">
              <div class="mobile-menu-section-title">关于我们</div>
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

            <!-- Community Section -->
            <div class="mobile-menu-section">
              <div class="mobile-menu-section-title">社团动态</div>
              <router-link
                  v-for="item in communityOptions"
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

      <!-- Page Content -->
      <div v-if="!drawerVisible" class="min-h-screen">
        <router-view/>
      </div>
    </n-layout-content>

    <!-- Footer -->
    <n-layout-footer class="apple-footer">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div class="text-center space-y-4">
          <p class="text-sm text-gray-600">
            Copyright © 2023 - 2024 XAUAT iOS Club
          </p>
          <div class="flex flex-wrap justify-center gap-4 text-sm">
            <a
                href="https://cn.xauat.edu.cn/"
                class="footer-link hidden sm:inline-flex items-center gap-1 apple-link"
                target="_blank"
                rel="noopener"
            >
              西安建筑科技大学
            </a>
            <span class="text-gray-400 hidden sm:inline">·</span>
            <a
                href="https://beian.miit.gov.cn/"
                class="footer-link apple-link"
                target="_blank"
                rel="noopener"
            >
              陕ICP备2024031872号
            </a>
            <span class="text-gray-400 hidden sm:inline">·</span>
            <a
                href="https://gitee.com/XAUATiOSClub"
                class="footer-link hidden sm:inline-flex items-center gap-1 apple-link"
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
import {ref, onMounted, onUnmounted} from 'vue'
import {useRouter, RouterView} from 'vue-router'
import {
  NLayout, NLayoutHeader, NLayoutContent, NLayoutFooter,
  NDropdown, NIcon
} from 'naive-ui'
import {MenuOutline} from '@vicons/ionicons5'

const router = useRouter()
const drawerVisible = ref(false)
const isScrolled = ref(false)

// Handle scroll effect
const handleScroll = () => {
  isScrolled.value = window.scrollY > 10
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})

// Dropdown options for desktop menu
const aboutUsOptions = [
  {
    label: '社团简介',
    key: '/About'
  },
  {
    label: '社团结构',
    key: '/Structure'
  },
  {
    label: '其他组织',
    key: '/OtherOrg'
  },
  {
    label: '竞赛资源',
    key: '/Article/Competitions'
  },
  {
    label: '社团历史',
    key: '/History'
  }
]

const communityOptions = [
  {
    label: '社团活动',
    key: '/Event'
  },
  {
    label: '技术博客',
    key: '/Articles'
  },
  {
    label: 'iOS App',
    key: '/Tools'
  },
  {
    label: '精品项目',
    key: '/Projects'
  }
]

// Handle dropdown selection
const handleSelect = (key) => {
  router.push(key)
}
</script>

<style scoped>
/* Apple-style header */
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

/* Navigation link styles */
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

/* Apple-style primary button */
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

/* Icon button for mobile */
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

/* Mobile menu styles */
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

/* Footer styles */
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
}

/* Dropdown customization */
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

/* Animations */
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

/* Responsive adjustments */
@media (max-width: 640px) {
  .apple-header {
    background: rgba(255, 255, 255, 0.95);
  }
}
</style>
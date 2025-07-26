<template>
  <n-layout class="min-h-screen">
    <n-layout-header class="bg-white" style="height: auto">
      <div class="flex items-center justify-between px-4 py-3">
        <!-- Logo and Title -->
        <router-link to="/" class="flex items-center gap-4">
          <img
              src="../assets/iOS_Club_LOGO.png"
              alt="iOS Club Logo"
              class="w-10 h-10"
          />
          <div class="text-2xl font-bold text-gray-900 hidden sm:block">
            XAUAT iOS Club
          </div>
        </router-link>

        <!-- Desktop Menu -->
        <n-space size="large" class="hidden sm:flex">
          <!-- 关于我们 Dropdown -->
          <n-dropdown
              trigger="hover"
              :options="aboutUsOptions"
              @select="handleSelect"
          >
            <n-button
                text
                class="rounded-lg ml-5"
                @click="() => router.push('/About')"
            >
              关于我们
            </n-button>
          </n-dropdown>

          <!-- 社团动态 Dropdown -->
          <n-dropdown
              trigger="hover"
              :options="communityOptions"
              @select="handleSelect"
          >
            <n-button
                text
                class="rounded-lg ml-5"
                @click="() => router.push('/Event')"
            >
              社团动态
            </n-button>
          </n-dropdown>

          <!-- Login/Register Button -->
          <n-button
              type="primary"
              class="rounded-lg ml-5"
              @click="() => router.push('/Login')"
          >
            登录/注册
          </n-button>
        </n-space>

        <!-- Mobile Menu Button -->
        <n-button
            text
            size="large"
            class="sm:hidden"
            @click="drawerVisible = !drawerVisible"
        >
          <n-icon size="30">
            <MenuOutline />
          </n-icon>
        </n-button>
      </div>
    </n-layout-header>

    <n-layout-content class="bg-white">
      <!-- Mobile Drawer Menu -->
      <div
          v-if="drawerVisible"
          class="pt-2.5 animate-fade-in"
      >
        <n-menu
            :options="menuOptions"
            accordion
            @update:value="handleMenuSelect"
        />
      </div>

      <!-- Main Content -->
      <div v-else>
        <slot />
      </div>
    </n-layout-content>

    <n-layout-footer class="text-center py-4">
      <p class="mb-2">
        Copyright © 2023 - 2024 XAUAT iOS Club
      </p>
      <n-space :wrap="false">
        <n-divider vertical class="hidden sm:block" />
        <router-link
            to="https://cn.xauat.edu.cn/"
            class="text-blue-600 hover:text-blue-800 hidden sm:inline"
            target="_blank"
        >
          西安建筑科技大学
        </router-link>
        <router-link
            to="https://beian.miit.gov.cn/"
            class="text-blue-600 hover:text-blue-800"
            target="_blank"
        >
          备案号 陕ICP备2024031872号
        </router-link>
        <router-link
            to="https://gitee.com/XAUATiOSClub"
            class="text-blue-600 hover:text-blue-800 hidden sm:inline"
            target="_blank"
        >
          我们的Gitee组织
        </router-link>
      </n-space>
    </n-layout-footer>
  </n-layout>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { NLayout, NLayoutHeader, NLayoutContent, NLayoutFooter,
  NButton, NDropdown, NSpace, NMenu, NIcon, NDivider } from 'naive-ui'
import { MenuOutline } from '@vicons/ionicons5'

const router = useRouter()
const drawerVisible = ref(false)

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

// Mobile menu options
const menuOptions = [
  {
    label: '首页',
    key: '/'
  },
  {
    label: '关于我们',
    key: 'about',
    children: aboutUsOptions
  },
  {
    label: '社团动态',
    key: 'community',
    children: communityOptions
  },
  {
    label: '登录/注册',
    key: '/Login'
  }
]

// Handle dropdown selection
const handleSelect = (key) => {
  router.push(key)
}

// Handle mobile menu selection
const handleMenuSelect = (key) => {
  if (key.startsWith('/')) {
    router.push(key)
    drawerVisible.value = false
  }
}
</script>

<style scoped>
/* Dropdown menu custom styles */
:deep(.n-dropdown-menu) {
  background-color: #ffffff;
  border-radius: 10px;
  box-shadow: 0 0 10px #C0C0C0;
  padding: 20px;
}

:deep(.n-dropdown-option) {
  color: #3c3c43;
  margin: 5px;
  text-align: center;
  min-width: 100px;
  font-size: 15px;
}

/* Gradient text utility class */
.gradient-text {
  background: -webkit-linear-gradient(-64deg, #f9bf65, #ffab6b, #ff9977, #fc8986, #ef7e95, #e47da6, #d37fb5, #bf83c1, #ab8dcf, #9597d8, #7fa0dc, #6ca7da);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  @apply text-5xl font-bold;
}

@media (max-width: 992px) {
  .gradient-text {
    @apply text-center;
  }
}

@media (max-width: 768px) {
  .gradient-text {
    @apply text-4xl text-center;
  }
}

/* Fade in animation */
@keyframes fade-in {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.animate-fade-in {
  animation: fade-in ease-in 0.3s;
}
</style>
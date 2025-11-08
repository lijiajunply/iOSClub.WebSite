<template>
  <n-menu
      :value="activeKey"
      :options="menuOptions"
      :root-indent="24"
      :indent="36"
      @update:value="handleSelect"
      class="apple-menu"
  />
</template>

<script setup>
import {computed, ref} from 'vue'
import {useRoute, useRouter} from 'vue-router'
import {NMenu} from 'naive-ui'

const emit = defineEmits(['menu-item-click'])
const route = useRoute()
const router = useRouter()

const activeKey = computed(() => route.path)

// 用于跟踪是否正在刷新当前页面
const isReloading = ref(false)

const menuOptions = [
  {
    type: 'group',
    label: '社团简介',
    key: 'about-group',
    children: [
      {
        label: '关于我们',
        key: '/About'
      },
      {
        label: '社团结构',
        key: '/Structure'
      },
      {
        label: '其他组织',
        key: '/OtherOrg'
      }
    ]
  },
  {
    type: 'group',
    label: '竞赛资源',
    key: 'competition-group',
    children: [
      {
        label: '资源支持',
        key: '/Article/Competitions'
      },
      {
        label: '移动应用创新赛',
        key: '/Article/MobileApplication'
      },
      {
        label: 'WWDC-Swift学生挑战赛',
        key: '/Article/Swift'
      }
    ]
  },
  {
    type: 'group',
    label: '社团活动',
    key: 'activity-group',
    children: [
      {
        label: 'iOS Learn',
        key: '/Article/iOSLearn'
      },
      {
        label: '项目开发活动',
        key: '/Article/TimeToCode'
      },
      {
        label: '体验最新产品',
        key: '/Article/VisionPro'
      },
      {
        label: '一起看发布会',
        key: '/Article/PressConference'
      }
    ]
  },
  {
    type: 'group',
    label: '社团历史',
    key: 'history-group',
    children: [
      {
        label: '总述',
        key: '/Article/History-Overview'
      },
      {
        label: '历届干部',
        key: '/Article/PreviousCadres'
      },
      {
        label: '创社史',
        key: '/Article/History-Founding'
      },
      {
        label: '邵韩之治',
        key: '/Article/History-Shao Han\'s Reign'
      },
      {
        label: '活动室变迁',
        key: '/Article/History-Room'
      }
    ]
  }
]

const handleSelect = (key) => {
  emit('menu-item-click', key)
  // 如果当前路由与点击的路由相同，则刷新页面
  if (route.path === key) {
    // 设置刷新状态并强制刷新当前页面
    isReloading.value = true
    window.location.reload()
  } else {
    // 跳转到新页面
    router.push(key)
  }
}
</script>

<style>
/* 苹果风格的菜单样式 */
.apple-menu {
  background: transparent !important;
}

.apple-menu .n-menu-item-group-title {
  font-size: 20px;
  font-weight: 600;
  color: #86868b;
  text-transform: uppercase;
  letter-spacing: 0.01em;
  padding: 12px 24px 8px;
  margin-bottom: 4px;
  border-bottom: 1px solid #f0f0f0;
}

.apple-menu .n-menu-item {
  font-size: 15px;
  font-weight: 400;
  color: #1d1d1f;
  border-radius: 8px;
  margin: 2px 12px;
  transition: all 0.2s ease;
}

.apple-menu .n-menu-item:hover {
  background-color: #f5f5f7;
  color: #0071e3;
}

.apple-menu .n-menu-item.n-menu-item--selected {
  background-color: #0071e3;
  color: white;
  font-weight: 500;
}

.dark .apple-menu .n-menu-item {
  background: transparent !important;
}

.apple-menu .n-menu-item.n-menu-item--selected::before {
  display: none;
}

.apple-menu .n-menu-item-content {
  padding: 10px 16px !important;
}

/* 动画效果 */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.apple-menu {
  animation: fadeIn 0.3s ease-in;
}
</style>
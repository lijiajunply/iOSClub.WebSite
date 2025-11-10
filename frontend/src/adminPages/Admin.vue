<template>
  <div class=" transition-colors duration-300">
    <!-- 主内容区域 -->
    <div class="p-4 md:p-6">
      <!-- 页面标题 -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">其他数据</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">社团管理系统数据分析</p>
        </div>

        <div class="flex items-center space-x-2">
          <button
              v-if="isAdmin"
              @click="triggerFileInput"
              class="hidden sm:flex items-center px-4 py-2 bg-blue-500 text-white rounded-full hover:bg-blue-600 transition-colors text-sm"
          >
            <Icon icon="material-symbols:upload" class="mr-1" width="20" height="20"/>
            上传数据
          </button>

          <div v-if="isAdmin" class="relative" ref="dropdownContainer">
            <button
                @click="toggleDropdown"
                class="h-9 w-9 rounded-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors"
            >
              <Icon icon="material-symbols:more-horiz" width="20" height="20" class="text-gray-700 dark:text-gray-300"/>
            </button>

            <div
                v-if="dropdownOpen"
                class="absolute right-0 mt-2 w-48 bg-white dark:bg-gray-700 rounded-lg shadow-lg py-1 z-10 border border-gray-200 dark:border-gray-600"
            >
              <button
                  @click="handleDropdownSelect('download')"
                  class="block w-full text-left px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-600"
              >
                下载所有数据
              </button>
              <button
                  @click="handleDropdownSelect('remove')"
                  class="block w-full text-left px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-gray-100 dark:hover:bg-gray-600"
              >
                删除所有数据
              </button>
            </div>
          </div>

          <input
              ref="fileInput"
              type="file"
              accept=".json"
              multiple
              @change="uploadFiles"
              style="display: none"
          />
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="space-y-8">
        <!-- 数据概览卡片骨架 -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-8">
          <SkeletonLoader v-for="i in 7" :key="i" type="card"/>
        </div>

        <!-- 图表区域骨架 -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          <SkeletonLoader v-for="i in 2" :key="i" type="chart"/>
        </div>
      </div>

      <!-- 数据概览卡片 -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 mb-8">
        <StatCard
            title="社员人数"
            :value="statistics.members"
            icon="ion:people"
            color="blue"
        />
        <StatCard
            title="部员人数"
            :value="statistics.staffs"
            icon="material-symbols:badge"
            color="green"
        />
        <StatCard
            title="项目数"
            :value="statistics.projects"
            icon="material-symbols:folder"
            color="purple"
        />
        <StatCard
            title="任务数"
            :value="statistics.tasks"
            icon="material-symbols:task"
            color="amber"
        />
        <StatCard
            title="资源数"
            :value="statistics.resources"
            icon="material-symbols:folder-open"
            color="pink"
        />
        <StatCard
            title="部门数"
            :value="statistics.departments"
            icon="material-symbols:groups"
            color="indigo"
        />
        <StatCard
            title="待办数"
            :value="statistics.todos"
            icon="ion:checkbox"
            color="red"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, onMounted, onBeforeUnmount, nextTick} from 'vue'
import {useMessage, useDialog} from 'naive-ui'
import {Icon} from '@iconify/vue'
import SkeletonLoader from '../components/SkeletonLoader.vue'
import StatCard from '../components/StatCard.vue'
import {DataCentreService} from "../services/DataCentreService";
import {useAuthorizationStore} from "../stores/Authorization";

const message = useMessage()
const dialog = useDialog()
const authorizationStore = useAuthorizationStore()

// Refs
const fileInput = ref<HTMLInputElement>()
const dropdownContainer = ref<HTMLElement>()
const dropdownOpen = ref(false)
const loading = ref(false)

const statistics = ref({
  members: 0,
  staffs: 0,
  projects: 0,
  tasks: 0,
  resources: 0,
  departments: 0,
  todos: 0
})

const isAdmin = ref(false)

// File handling methods
const triggerFileInput = () => {
  fileInput.value?.click()
}

// Dropdown methods
const toggleDropdown = () => {
  dropdownOpen.value = !dropdownOpen.value
}

const handleDropdownSelect = (key: string) => {
  dropdownOpen.value = false
  switch (key) {
    case 'download':
      downloadAllData()
      break
    case 'remove':
      removeAllData()
      break
    case 'upload':
      triggerFileInput()
      break
  }
}

// Click outside to close dropdown
const handleClickOutside = (event: MouseEvent) => {
  if (dropdownContainer.value && !dropdownContainer.value.contains(event.target as Node)) {
    dropdownOpen.value = false
  }
}

// Upload files
const uploadFiles = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = target.files
  if (!files || !files.length) return

  try {
    for (let i = 0; i < files.length; i++) {
      const file = files[i]
      try {
        await DataCentreService.updateDataFromJson(file)

        message.success(`文件 "${file.name}" 上传成功`)
      } catch (error) {
        console.error('解析文件时出错:', error)
        message.error(`解析文件 "${file.name}" 时出错`)
      }
    }
  } catch (error) {
    console.error('上传文件时出错:', error)
    message.error('上传文件时出错')
  }
}

// Download all data
const downloadAllData = async () => {
  try {
    const token = localStorage.getItem('Authorization')
    if (!token) {
      message.error('未找到认证信息')
      return
    }

    const blob = await DataCentreService.exportJson()
    const url = URL.createObjectURL(blob)

    const a = document.createElement('a')
    a.href = url
    a.download = 'allData.json'
    document.body.appendChild(a)
    a.click()

    setTimeout(() => {
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    }, 100)

    message.success('数据下载成功')
  } catch (error) {
    console.error('下载数据时出错:', error)
    message.error('下载数据时出错')
  }
}

// Remove all data
const removeAllData = () => {
  dialog.warning({
    title: '确认删除',
    content: '确定要删除所有数据吗？此操作不可撤销。',
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        const token = localStorage.getItem('Authorization')
        if (!token) {
          message.error('未找到认证信息')
          return
        }

        const response = await fetch('https://www.xauat.site/api/Admin/RemoveAllData', {
          method: 'DELETE',
          headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
          }
        })

        if (response.ok) {
          message.success('所有数据已删除')
          await fetchData()
        } else {
          message.error('删除数据失败')
        }
      } catch (error) {
        console.error('删除数据时出错:', error)
        message.error('删除数据时出错')
      }
    }
  })
}

// Fetch data
const fetchData = async () => {
  loading.value = true
  try {
    // 并行获取所有数据
    statistics.value = await DataCentreService.getCentreData();

  } catch (error) {
    console.error('获取数据时出错:', error)
    message.error('获取数据时出错')
  } finally {
    loading.value = false
  }
}

// Mount and unmount lifecycle
onMounted(async () => {
  isAdmin.value = authorizationStore.isAdmin()
  await fetchData()
  await nextTick()

  // Add click outside listener
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
/* Custom styles if needed */
</style>
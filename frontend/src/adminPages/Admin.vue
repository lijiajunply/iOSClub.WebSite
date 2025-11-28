<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 页面操作栏 -->
      <div class="flex items-center justify-end space-x-3 mb-8">
        <button v-if="isAdmin" size="small" class="rounded-full bg-blue-500 hover:bg-blue-600 h-9 space-x-1 px-2 flex items-center justify-center text-gray-100"
          @click="triggerFileInput">
          <Icon icon="material-symbols:upload" class="w-4 h-4" />
          <span>上传数据</span>
        </button>

        <div v-if="isAdmin" class="relative" ref="dropdownContainer">
          <button @click="toggleDropdown"
            class="h-9 w-9 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors duration-200">
            <Icon icon="material-symbols:more-horiz" class="w-5 h-5 text-gray-600 dark:text-gray-400" />
          </button>

          <div v-if="dropdownOpen"
            class="absolute right-0 mt-2 w-48 bg-white dark:bg-gray-800 rounded-xl shadow-lg py-2 z-10 border border-gray-200 dark:border-gray-700 transition-all duration-200">
            <button @click="handleDropdownSelect('download')"
              class="block w-full text-left px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-150">
              下载所有数据
            </button>
            <button @click="handleDropdownSelect('remove')"
              class="block w-full text-left px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors duration-150">
              删除所有数据
            </button>
          </div>
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="space-y-8">
        <!-- 数据概览卡片骨架 -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
          <SkeletonLoader v-for="i in 7" :key="i" type="card" />
        </div>
      </div>

      <!-- 数据概览卡片 -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
        <div
          class="p-4 rounded-xl bg-blue-50 dark:bg-blue-900/20 border border-blue-100 dark:border-blue-800/30 hover:shadow-md transition-all duration-200">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-xs text-gray-500 dark:text-gray-400">社员人数</p>
              <p class="text-2xl font-semibold mt-1 text-blue-600 dark:text-blue-400">{{ statistics.members }}</p>
            </div>
            <div class="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-800/50 flex items-center justify-center">
              <Icon icon="ion:people" class="w-4 h-4 text-blue-600 dark:text-blue-400" />
            </div>
          </div>
        </div>

        <div
          class="p-4 rounded-xl bg-green-50 dark:bg-green-900/20 border border-green-100 dark:border-green-800/30 hover:shadow-md transition-all duration-200">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-xs text-gray-500 dark:text-gray-400">部员人数</p>
              <p class="text-2xl font-semibold mt-1 text-green-600 dark:text-green-400">{{ statistics.staffs }}</p>
            </div>
            <div class="w-8 h-8 rounded-full bg-green-100 dark:bg-green-800/50 flex items-center justify-center">
              <Icon icon="material-symbols:badge" class="w-4 h-4 text-green-600 dark:text-green-400" />
            </div>
          </div>
        </div>

        <div
          class="p-4 rounded-xl bg-purple-50 dark:bg-purple-900/20 border border-purple-100 dark:border-purple-800/30 hover:shadow-md transition-all duration-200">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-xs text-gray-500 dark:text-gray-400">项目数</p>
              <p class="text-2xl font-semibold mt-1 text-purple-600 dark:text-purple-400">{{ statistics.projects }}</p>
            </div>
            <div class="w-8 h-8 rounded-full bg-purple-100 dark:bg-purple-800/50 flex items-center justify-center">
              <Icon icon="material-symbols:folder" class="w-4 h-4 text-purple-600 dark:text-purple-400" />
            </div>
          </div>
        </div>

        <div
          class="p-4 rounded-xl bg-amber-50 dark:bg-amber-900/20 border border-amber-100 dark:border-amber-800/30 hover:shadow-md transition-all duration-200">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-xs text-gray-500 dark:text-gray-400">任务数</p>
              <p class="text-2xl font-semibold mt-1 text-amber-600 dark:text-amber-400">{{ statistics.tasks }}</p>
            </div>
            <div class="w-8 h-8 rounded-full bg-amber-100 dark:bg-amber-800/50 flex items-center justify-center">
              <Icon icon="material-symbols:task" class="w-4 h-4 text-amber-600 dark:text-amber-400" />
            </div>
          </div>
        </div>

        <div
          class="p-4 rounded-xl bg-pink-50 dark:bg-pink-900/20 border border-pink-100 dark:border-pink-800/30 hover:shadow-md transition-all duration-200">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-xs text-gray-500 dark:text-gray-400">资源数</p>
              <p class="text-2xl font-semibold mt-1 text-pink-600 dark:text-pink-400">{{ statistics.resources }}</p>
            </div>
            <div class="w-8 h-8 rounded-full bg-pink-100 dark:bg-pink-800/50 flex items-center justify-center">
              <Icon icon="material-symbols:folder-open" class="w-4 h-4 text-pink-600 dark:text-pink-400" />
            </div>
          </div>
        </div>

        <div
          class="p-4 rounded-xl bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-800/30 hover:shadow-md transition-all duration-200">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-xs text-gray-500 dark:text-gray-400">部门数</p>
              <p class="text-2xl font-semibold mt-1 text-indigo-600 dark:text-indigo-400">{{ statistics.departments }}
              </p>
            </div>
            <div class="w-8 h-8 rounded-full bg-indigo-100 dark:bg-indigo-800/50 flex items-center justify-center">
              <Icon icon="material-symbols:groups" class="w-4 h-4 text-indigo-600 dark:text-indigo-400" />
            </div>
          </div>
        </div>

        <div
          class="p-4 rounded-xl bg-red-50 dark:bg-red-900/20 border border-red-100 dark:border-red-800/30 hover:shadow-md transition-all duration-200">
          <div class="flex justify-between items-start">
            <div>
              <p class="text-xs text-gray-500 dark:text-gray-400">待办数</p>
              <p class="text-2xl font-semibold mt-1 text-red-600 dark:text-red-400">{{ statistics.todos }}</p>
            </div>
            <div class="w-8 h-8 rounded-full bg-red-100 dark:bg-red-800/50 flex items-center justify-center">
              <Icon icon="ion:checkbox" class="w-4 h-4 text-red-600 dark:text-red-400" />
            </div>
          </div>
        </div>
      </div>

      <input ref="fileInput" type="file" accept=".json" multiple @change="uploadFiles" style="display: none" />
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useMessage, useDialog, NButton } from 'naive-ui'
import { Icon } from '@iconify/vue'
import SkeletonLoader from '../components/SkeletonLoader.vue'
import { DataCentreService } from "../services/DataCentreService";
import { useAuthorizationStore } from "../stores/Authorization";
import { useLayoutStore } from "../stores/LayoutStore";

const message = useMessage()
const dialog = useDialog()
const authorizationStore = useAuthorizationStore()
const layoutStore = useLayoutStore()

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
  // Set page header
  layoutStore.setPageHeader(
    '其他数据',
    '社团管理系统数据分析'
  )

  // Show page actions
  layoutStore.setShowPageActions(true)

  isAdmin.value = authorizationStore.isAdmin()
  await fetchData()
  await nextTick()

  // Add click outside listener
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  // Clear page header
  layoutStore.clearPageHeader()

  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
/* Custom styles if needed */
</style>
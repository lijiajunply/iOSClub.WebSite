<template>
  <div class="layout-container">
    <n-layout class="h-[calc(100vh-64px)] bg-ios transition-colors duration-300" has-sider>
      <n-layout-sider v-if="!isMobile" collapse-mode="width" :collapsed-width="0" :width="280" :native-scrollbar="false"
        bordered class="glass-sidebar">
        <div class="flex flex-col h-full">
          <!-- Sidebar Header / Search placeholder -->
          <div class="px-5 py-4">
            <div class="flex items-center space-x-2 text-gray-500 dark:text-gray-400">
              <Icon icon="lucide:layout-grid" class="w-5 h-5" />
              <span class="text-sm font-semibold tracking-wide uppercase opacity-70">Blog</span>
            </div>
          </div>

          <!-- Menu Content Wrapper -->
          <n-scrollbar class="flex-1 -mr-2 pr-2">
            <MenuContent @menu-item-click="handleMenuClick" />
          </n-scrollbar>
        </div>
      </n-layout-sider>

      <!-- Main Content Area -->
      <n-layout-content class="bg-transparent">
        <div class="content-card w-full h-full flex flex-col overflow-hidden bg-white dark:bg-[#1e1e1e]">

          <!-- Breadcrumb / Mobile Header Toolbar -->
          <div
            class="h-14 px-6 flex items-center justify-between border-b border-gray-100 dark:border-white/5 shrink-0 glass-header z-10">
            <!-- Mobile Toggle -->
            <button v-if="isMobile" @click="mobileMenuOpen = true"
              class="p-2 -ml-2 rounded-full hover:bg-gray-100 dark:hover:bg-white/10 transition-colors">
              <Icon icon="lucide:menu" class="w-6 h-6 text-gray-600 dark:text-gray-300" />
            </button>

            <!-- Title / Breadcrumb placeholder -->
            <div class="flex items-center space-x-2 text-sm font-medium text-gray-500 dark:text-gray-400">
              <span>Documents</span>
              <!-- Dynamic Paths -->
              <template v-for="(crumb, index) in breadcrumbs" :key="index">
                <Icon icon="lucide:chevron-right" class="w-4 h-4 mx-1 text-gray-300 dark:text-gray-600 shrink-0" />
                <Icon :icon="index === breadcrumbs.length - 1 ? 'lucide:file-text' : 'lucide:folder'"
                  class="w-4 h-4 mx-1 shrink-0" :class="index === breadcrumbs.length - 1
                    ? 'font-semibold text-gray-800 dark:text-gray-100'
                    : 'text-gray-500 dark:text-gray-400'" />
                <span class="truncate" :class="index === breadcrumbs.length - 1
                  ? 'font-semibold text-gray-800 dark:text-gray-100'
                  : 'text-gray-500 dark:text-gray-400'">
                  {{ crumb }}
                </span>
              </template>
            </div>

            <!-- Right Actions -->
            <div class="flex items-center space-x-3">
              <Icon icon="lucide:search"
                class="w-5 h-5 text-gray-400 cursor-pointer hover:text-gray-600 dark:hover:text-gray-200"
                @click="handleSearchClick" />


              <n-dropdown trigger="click" placement="bottom-end" :options="dropdownOptions" @select="handleDropdownSelect">
                <button><Icon icon="lucide:more-horizontal"
                    class="w-5 h-5 text-gray-400 cursor-pointer hover:text-gray-600 dark:hover:text-gray-200" /></button>
              </n-dropdown>
            </div>
          </div>

          <!-- Router View Container -->
          <div class="flex-1 overflow-auto relative scroll-smooth">
            <div class="min-h-full">
              <router-view v-slot="{ Component }">
                <transition name="fade-scale" mode="out-in">
                  <component :is="Component" />
                </transition>
              </router-view>
            </div>
          </div>
        </div>
      </n-layout-content>
    </n-layout>

    <!-- Mobile Sheet/Drawer (iOS Style) -->
    <n-drawer v-model:show="mobileMenuOpen" placement="bottom" height="60vh" class="ios-drawer rounded-t-2xl"
      :trap-focus="false" :block-scroll="false">
      <n-drawer-content body-content-style="padding: 0;" class="bg-gray-50 dark:bg-[#1c1c1e]">
        <template #header>
          <div class="w-full flex justify-center pt-2 pb-4 cursor-grab">
            <div class="w-12 h-1.5 bg-gray-300 dark:bg-gray-600 rounded-full"></div>
          </div>
        </template>

        <div class="px-6 pb-10 h-full overflow-y-auto">
          <h3 class="text-2xl font-bold mb-6 text-gray-900 dark:text-white tracking-tight">Navigation</h3>
          <MenuContent @menu-item-click="handleMenuClick" />
        </div>
      </n-drawer-content>
    </n-drawer>

  </div>
  <!-- Search Modal -->
  <n-modal v-model:show="searchModalVisible" preset="card" :show-icon="false" :show-header="false" :close-on-esc="true"
    :close-on-click-outside="true" :mask-opacity="0.3" :style="{ width: '90%', maxWidth: '560px' }"
    class="ios-search-modal">
    <div class="ios-search-container">
      <!-- Search Input Section -->
      <div class="ios-search-header">
        <div class="ios-search-input-wrapper">
          <Icon icon="lucide:search" class="ios-search-icon" />
          <n-input v-model:value="searchKeyword" placeholder="搜索文章" size="large" autofocus @keyup.enter="handleSearch"
            class="ios-search-input" :bordered="false" />
          <n-button v-if="searchKeyword.trim()" size="small" type="default" :bordered="false"
            @click="searchKeyword = ''" class="ios-search-clear">
            <Icon icon="lucide:x" class="w-4 h-4" />
          </n-button>
        </div>
        <n-button type="default" size="large" :bordered="false" @click="handleSearch" :disabled="isSearching"
          class="ios-search-button">
          <n-spin v-if="isSearching" size="small" class="ios-search-spinner" />
          <span v-else>搜索</span>
        </n-button>
      </div>

      <!-- Search Results Section -->
      <div class="ios-search-results-container">
        <!-- Loading State -->
        <div v-if="isSearching" class="ios-loading-state">
          <n-spin size="large" class="ios-spinner" />
        </div>

        <!-- Error State -->
        <div v-else-if="searchError" class="ios-error-state">
          <Icon icon="lucide:alert-circle" class="ios-error-icon" />
          <p class="ios-error-text">{{ searchError }}</p>
        </div>

        <!-- Results List -->
        <div v-else-if="searchResults.length > 0" class="ios-results-list">
          <div v-for="article in searchResults" :key="article.path" class="ios-result-item"
            @click="handleResultClick(article.path)">
            <div class="ios-result-content">
              <div class="ios-result-title" v-html="article.highlightedTitle || article.title"></div>
              <div class="ios-result-preview" v-html="article.highlightedContent || ''"></div>
              <div class="ios-result-meta">
                <span class="ios-result-date">{{ new Date(article.lastWriteTime).toLocaleDateString() }}</span>
              </div>
            </div>
            <Icon icon="lucide:chevron-right" class="ios-result-arrow" />
          </div>
        </div>

        <!-- Empty State -->
        <div v-else-if="searchKeyword.trim()" class="ios-empty-state">
          <Icon icon="lucide:file-text" class="ios-empty-icon" />
          <p class="ios-empty-text">没有找到匹配的文章</p>
        </div>
      </div>
    </div>
  </n-modal>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { NDrawer, NDrawerContent, NLayout, NLayoutContent, NLayoutSider, NScrollbar, NModal, NInput, NButton, NSpin, NDropdown } from 'naive-ui'
import { Icon } from '@iconify/vue'
import MenuContent from '../components/MenuContent.vue'
import { ArticleService } from '../services/ArticleService'
import { useAuthorizationStore } from '../stores/Authorization'
import type { ArticleModel } from '../models'
import type { ArticleSearchResult } from '../models/ArticleModel'

const router = useRouter()
const route = useRoute()
const mobileMenuOpen = ref(false)
const windowWidth = ref(window.innerWidth)

// Authorization store for user login status and permissions
const authStore = useAuthorizationStore()
const isLoggedIn = computed(() => authStore.isAuthenticated)
const isAdmin = computed(() => authStore.isAdmin())

// Dropdown menu options
const dropdownOptions = computed(() => {
  const options = [
    {
      label: '分享当前URL',
      key: 'share'
    }
  ]
  
  // Add edit option only if user is logged in and has admin permissions
  if (isLoggedIn.value && isAdmin.value) {
    options.push({
      label: '编辑',
      key: 'edit'
    })
  }
  
  return options
})

// Search related state
const searchModalVisible = ref(false)
const searchKeyword = ref('')
const searchResults = ref<ArticleSearchResult[]>([])
const isSearching = ref(false)
const searchError = ref('')

// Breakpoint for mobile view (iPad Mini portrait and below)
const MOBILE_BREAKPOINT = 768
const isMobile = computed(() => windowWidth.value < MOBILE_BREAKPOINT)

const handleResize = () => {
  windowWidth.value = window.innerWidth
  if (!isMobile.value) {
    mobileMenuOpen.value = false
  }
}

const handleMenuClick = (key: string) => {
  if (isMobile.value) {
    mobileMenuOpen.value = false
  }
  router.push(key)
}

// Search handlers
const handleSearchClick = () => {
  searchModalVisible.value = true
}

const handleSearch = async () => {
  if (!searchKeyword.value.trim()) {
    searchResults.value = []
    return
  }

  isSearching.value = true
  searchError.value = ''
  try {
    searchResults.value = await ArticleService.searchArticles(searchKeyword.value)
  } catch (error: any) {
    searchError.value = error.message || '搜索失败'
  } finally {
    isSearching.value = false
  }
}

const handleResultClick = (path: string) => {
  searchModalVisible.value = false
  router.push(`/Article/${path}`)
}

const breadcrumbs = computed(() => {
  const path = route.path
  if (!path) return []

  return decodeURIComponent(path)
    .split('/')
    .filter(segment => segment && segment !== 'Article')
})

onMounted(() => {
  window.addEventListener('resize', handleResize)
})

// Handle dropdown menu selection
const handleDropdownSelect = (key: string) => {
  switch (key) {
    case 'share':
      handleShareUrl()
      break
    case 'edit':
      handleEdit()
      break
  }
}

// Handle URL sharing
const handleShareUrl = async () => {
  try {
    await navigator.clipboard.writeText(window.location.href)
    // You could add a notification here if needed
  } catch (err) {
    console.error('Failed to copy URL:', err)
  }
}

// Handle edit action
const handleEdit = () => {
  // Get the current article path from route
  const path = route.path
  if (path.startsWith('/Article/')) {
    const articlePath = path.replace('/Article/', '')
    router.push(`/Centre/Article/edit/${articlePath}`)
  }
}

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
/* iOS Style Search Modal */
.ios-search-modal :deep(.n-modal-content) {
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  overflow: hidden;
  background-color: #ffffff;
}

.dark .ios-search-modal :deep(.n-modal-content) {
  background-color: #2c2c2e;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.6);
}

/* Search Container */
.ios-search-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}

/* Search Header */
.ios-search-header {
  display: flex;
  align-items: center;
  padding: 16px;
  border-bottom: 1px solid #e5e5ea;
}

.dark .ios-search-header {
  border-bottom-color: #3a3a3c;
}

.ios-search-input-wrapper {
  flex: 1;
  display: flex;
  align-items: center;
  background-color: #f2f2f7;
  border-radius: 12px;
  padding: 0 12px;
  margin-right: 12px;
  transition: background-color 0.2s ease;
}

.dark .ios-search-input-wrapper {
  background-color: #3a3a3c;
}

.ios-search-icon {
  width: 18px;
  height: 18px;
  color: #8e8e93;
  margin-right: 8px;
}

.dark .ios-search-icon {
  color: #8e8e93;
}

.ios-search-input {
  flex: 1;
  font-size: 17px;
  font-weight: 400;
  line-height: 22px;
}

.ios-search-input :deep(.n-input-input) {
  padding: 10px 0;
  color: #000000;
}

.dark .ios-search-input :deep(.n-input-input) {
  color: #ffffff;
}

.ios-search-clear {
  padding: 4px;
  color: #8e8e93;
}

.dark .ios-search-clear {
  color: #8e8e93;
}

.ios-search-button {
  font-size: 17px;
  font-weight: 600;
  color: #007aff;
  padding: 8px 12px;
}

.dark .ios-search-button {
  color: #0a84ff;
}

.ios-search-spinner {
  color: #007aff;
}

.dark .ios-search-spinner {
  color: #0a84ff;
}

/* Search Results Container */
.ios-search-results-container {
  flex: 1;
  overflow-y: auto;
  max-height: 400px;
}

/* Loading State */
.ios-loading-state {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 48px 20px;
}

.ios-spinner {
  color: #007aff;
}

.dark .ios-spinner {
  color: #0a84ff;
}

/* Error State */
.ios-error-state {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 48px 20px;
  text-align: center;
}

.ios-error-icon {
  width: 48px;
  height: 48px;
  color: #ff3b30;
  margin-bottom: 12px;
}

.dark .ios-error-icon {
  color: #ff453a;
}

.ios-error-text {
  font-size: 14px;
  color: #8e8e93;
  margin: 0;
}

.dark .ios-error-text {
  color: #8e8e93;
}

/* Results List */
.ios-results-list {
  padding: 8px 0;
}

.ios-result-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  transition: background-color 0.2s ease;
  cursor: pointer;
}

.ios-result-item:hover {
  background-color: #f2f2f7;
}

.dark .ios-result-item:hover {
  background-color: #3a3a3c;
}

.ios-result-content {
  flex: 1;
  min-width: 0;
}

.ios-result-title {
  font-size: 16px;
  font-weight: 400;
  color: #000000;
  line-height: 20px;
  margin-bottom: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ios-result-preview {
  font-size: 13px;
  color: #6e6e73;
  line-height: 18px;
  margin-bottom: 6px;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

/* 高亮样式 */
.ios-result-title :deep(b),
.ios-result-preview :deep(b) {
  color: #007aff;
  font-weight: 600;
}

.dark .ios-result-title {
  color: #ffffff;
}

.ios-result-meta {
  display: flex;
  align-items: center;
  font-size: 12px;
  color: #8e8e93;
  line-height: 16px;
}

.dark .ios-result-meta {
  color: #8e8e93;
}

.ios-result-category {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ios-result-divider {
  margin: 0 4px;
}

.ios-result-date {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ios-result-arrow {
  width: 16px;
  height: 16px;
  color: #c7c7cc;
  margin-left: 8px;
}

.dark .ios-result-arrow {
  color: #636366;
}

/* Empty State */
.ios-empty-state {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 48px 20px;
  text-align: center;
}

.ios-empty-icon {
  width: 48px;
  height: 48px;
  color: #c7c7cc;
  margin-bottom: 12px;
}

.dark .ios-empty-icon {
  color: #636366;
}

.ios-empty-text {
  font-size: 14px;
  color: #8e8e93;
  margin: 0;
}

.dark .ios-empty-text {
  color: #8e8e93;
}

/* Responsive Design */
@media (max-width: 768px) {
  .ios-search-modal :deep(.n-modal-content) {
    border-radius: 0;
    margin: 0;
    height: 100vh;
  }

  .ios-search-results-container {
    max-height: calc(100vh - 80px);
  }
}

.layout-container {
  /* Set base font stack similar to Apple System */
  font-family: -apple-system, BlinkMacSystemFont, "SF Pro Text", "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
}

/* Light Mode Sidebar Background (Apple Sidebar Grey) */
.bg-ios :deep(.n-layout-sider) {
  background-color: #fbfbfd;
}

/* Dark Mode Sidebar Background (Apple Dark Grey) */
.dark .bg-ios :deep(.n-layout-sider) {
  background-color: #2d2d2d;
  /* Slightly lighter than pure black */
}

/* Sidebar Styling */
.glass-sidebar :deep(.n-layout-sider-scroll-container) {
  /* Ensure content doesn't get cut off */
  min-height: 100%;
}

/* Apply backdrop blur if supported for that frosted glass look */
@supports (backdrop-filter: blur(20px)) {
  .glass-sidebar {
    background-color: rgba(249, 249, 251, 0.8) !important;
    backdrop-filter: blur(20px);
    -webkit-backdrop-filter: blur(20px);
  }

  .dark .bg-ios .glass-sidebar {
    background-color: rgba(45, 45, 45, 0.8) !important;
  }
}

/* The Main "Card" area */
.content-card {
  /* Only round corners on Desktop */
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05),
    0 4px 12px 0 rgba(0, 0, 0, 0.02);

  /* Subtle border for definition */
  border: 1px solid rgba(0, 0, 0, 0.06);
}

.bg-ios-dark .content-card {
  border: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow: 0 4px 24px 0 rgba(0, 0, 0, 0.2);
}

/* Mobile specific adjustments */
@media (max-width: 768px) {
  .content-card {
    border-radius: 0;
    border: none;
  }
}

/* Animations */
.fade-scale-enter-active,
.fade-scale-leave-active {
  transition: opacity 0.25s ease, transform 0.25s cubic-bezier(0.2, 0.0, 0.2, 1);
}

.fade-scale-enter-from {
  opacity: 0;
  transform: scale(0.98) translateY(10px);
}

.fade-scale-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Scrollbar Customization to look like macOS native */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background-color: rgba(0, 0, 0, 0.2);
  border-radius: 10px;
  border: 2px solid transparent;
  /* Creates padding effect */
  background-clip: content-box;
}

::-webkit-scrollbar-thumb:hover {
  background-color: rgba(0, 0, 0, 0.4);
}

.dark ::-webkit-scrollbar-thumb {
  background-color: rgba(255, 255, 255, 0.2);
}

.dark ::-webkit-scrollbar-thumb:hover {
  background-color: rgba(255, 255, 255, 0.4);
}

/* Drawer Styling */
:deep(.n-drawer.ios-drawer) {
  border-top-left-radius: 16px;
  border-top-right-radius: 16px;
}
</style>
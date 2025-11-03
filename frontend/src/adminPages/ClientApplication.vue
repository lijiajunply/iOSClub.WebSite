<template>
  <div class="min-h-screen bg-white dark:bg-gray-800 text-gray-900 dark:text-white transition-colors duration-300">
    <!-- 页面头部 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="text-2xl font-semibold tracking-tight">客户端应用管理</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">管理第三方应用的OAuth客户端</p>
        </div>

        <button @click="openCreateModal"
                class="px-4 py-2 rounded-lg bg-blue-500 hover:bg-blue-600 text-white transition-colors flex items-center justify-center">
          <Icon icon="ion:add" class="mr-1" width="18" height="18"/>
          创建客户端
        </button>
      </div>
    </div>

    <!-- 主内容区 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-12">
      <!-- 搜索和筛选 -->
      <div class="mb-6">
        <div class="relative">
          <input v-model="searchQuery" type="text" placeholder="搜索应用名称或客户端ID..."
                 class="w-full pl-10 pr-4 py-2 rounded-lg bg-gray-100 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"/>
          <Icon icon="ion:search" class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" width="20"
                height="20"/>
        </div>
      </div>

      <!-- 客户端应用列表 -->
      <div
          class="bg-white/90 dark:bg-gray-800/90 backdrop-blur-md rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
            <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th scope="col"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                应用名称
              </th>
              <th scope="col"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                客户端ID
              </th>
              <th scope="col"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                状态
              </th>
              <th scope="col"
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                创建时间
              </th>
              <th scope="col"
                  class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                操作
              </th>
            </tr>
            </thead>
            <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <template v-if="isLoading">
              <tr v-for="i in 5" :key="i" class="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <SkeletonLoader type="avatar" />
                    <div class="ml-4">
                      <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-32 mb-2"></div>
                      <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-24"></div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-32"></div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-16"></div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-24"></div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right">
                  <div class="flex justify-end space-x-2">
                    <div class="h-5 w-5 bg-gray-200 dark:bg-gray-700 rounded"></div>
                    <div class="h-5 w-5 bg-gray-200 dark:bg-gray-700 rounded"></div>
                    <div class="h-5 w-5 bg-gray-200 dark:bg-gray-700 rounded"></div>
                  </div>
                </td>
              </tr>
            </template>
            <template v-else-if="filteredClientApps.length > 0">
              <tr v-for="app in filteredClientApps" :key="app.ClientId"
                  class="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <img v-if="app.LogoUrl" :src="app.LogoUrl" :alt="app.ApplicationName"
                         class="h-8 w-8 rounded-md object-cover"/>
                    <div v-else
                         class="h-8 w-8 rounded-md bg-gray-200 dark:bg-gray-600 flex items-center justify-center">
                      <Icon icon="ion:app" width="20" height="20" class="text-gray-500 dark:text-gray-400"/>
                    </div>
                    <div class="ml-4">
                      <div class="text-sm font-medium text-gray-900 dark:text-white">
                        {{ app.ApplicationName }}
                      </div>
                      <div class="text-xs text-gray-500 dark:text-gray-400 line-clamp-1 max-w-xs">
                        {{ app.Description }}
                      </div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <code
                        class="text-xs text-gray-600 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded font-mono select-all">
                      {{ app.ClientId }}
                    </code>
                    <button @click="copyToClipboard(app.ClientId)"
                            class="ml-2 text-gray-400 hover:text-blue-500 transition-colors" title="复制客户端ID">
                      <Icon icon="ion:copy-outline" width="16" height="16"/>
                    </button>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <span :class="[
                      'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                      app.IsActive
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
                        : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                    ]">
                      {{ app.IsActive ? '启用' : '禁用' }}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                  {{ formatDate(app.CreatedAt) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div class="flex items-center justify-end space-x-2">
                    <button @click="openDetailsModal(app)"
                            class="text-gray-600 dark:text-gray-300 hover:text-blue-500 dark:hover:text-blue-400 transition-colors"
                            title="查看详情">
                      <Icon icon="ion:eye-outline" width="20" height="20"/>
                    </button>
                    <button @click="openEditModal(app)"
                            class="text-gray-600 dark:text-gray-300 hover:text-blue-500 dark:hover:text-blue-400 transition-colors"
                            title="编辑">
                      <Icon icon="ion:pencil-outline" width="20" height="20"/>
                    </button>
                    <button @click="confirmDelete(app)"
                            class="text-gray-600 dark:text-gray-300 hover:text-red-500 dark:hover:text-red-400 transition-colors"
                            title="删除">
                      <Icon icon="ion:trash-outline" width="20" height="20"/>
                    </button>
                  </div>
                </td>
              </tr>
            </template>
            <tr v-else>
              <td colspan="5" class="px-6 py-12 text-center">
                <div class="flex flex-col items-center justify-center text-gray-500 dark:text-gray-400">
                  <Icon icon="ion:search" width="48" height="48" class="mb-4 opacity-30"/>
                  <p class="text-lg font-medium">未找到客户端应用</p>
                  <p class="text-sm mt-2">尝试更改搜索条件或创建新的客户端应用</p>
                </div>
              </td>
            </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- 创建客户端模态框 -->
    <n-modal v-model:show="showCreateModal" title="创建客户端应用" preset="dialog" size="large"
             @close="handleCreateModalClose"
             class="rounded-xl overflow-hidden bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
      <div class="p-4">
        <form @submit.prevent="handleCreateSubmit">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="col-span-1 md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用名称 <span class="text-red-500">*</span>
              </label>
              <input v-model="createForm.ApplicationName" type="text" required
                     class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                     placeholder="输入应用名称"/>
            </div>

            <div class="col-span-1 md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用描述
              </label>
              <textarea v-model="createForm.Description" rows="3"
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors resize-none"
                        placeholder="描述您的应用..."></textarea>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用主页URL <span class="text-red-500">*</span>
              </label>
              <input v-model="createForm.HomepageUrl" type="url" required
                     class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                     placeholder="https://example.com"/>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用图标URL
              </label>
              <input v-model="createForm.LogoUrl" type="url"
                     class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                     placeholder="https://example.com/logo.png"/>
            </div>

            <div class="col-span-1 md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                回调URL（每行一个） <span class="text-red-500">*</span>
              </label>
              <textarea v-model="redirectUrisText" rows="3" required
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors resize-none font-mono text-sm"
                        placeholder="https://example.com/callback\nhttps://example.com/oauth/callback"></textarea>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                请确保回调URL与您应用中配置的完全一致
              </p>
            </div>
          </div>

          <div class="mt-6 flex justify-end space-x-3">
            <button type="button" @click="handleCreateModalClose"
                    class="px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors">
              取消
            </button>
            <button type="submit" :disabled="isSubmitting"
                    class="px-4 py-2 rounded-lg bg-blue-500 text-white hover:bg-blue-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
              {{ isSubmitting ? '保存中...' : '保存' }}
            </button>
          </div>
        </form>
      </div>
    </n-modal>

    <!-- 编辑客户端模态框 -->
    <n-modal v-model:show="showEditModal" title="编辑客户端应用" preset="dialog" size="large"
             @close="handleEditModalClose"
             class="rounded-xl overflow-hidden bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
      <div class="p-4">
        <form @submit.prevent="handleEditSubmit">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="col-span-1 md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用名称 <span class="text-red-500">*</span>
              </label>
              <input v-model="editForm.ApplicationName" type="text" required
                     class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                     placeholder="输入应用名称"/>
            </div>

            <div class="col-span-1 md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用描述
              </label>
              <textarea v-model="editForm.Description" rows="3"
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors resize-none"
                        placeholder="描述您的应用..."></textarea>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用主页URL <span class="text-red-500">*</span>
              </label>
              <input v-model="editForm.HomepageUrl" type="url" required
                     class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                     placeholder="https://example.com"/>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                应用图标URL
              </label>
              <input v-model="editForm.LogoUrl" type="url"
                     class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                     placeholder="https://example.com/logo.png"/>
            </div>

            <div class="col-span-1 md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                回调URL（每行一个） <span class="text-red-500">*</span>
              </label>
              <textarea v-model="editRedirectUrisText" rows="3" required
                        class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors resize-none font-mono text-sm"
                        placeholder="https://example.com/callback\nhttps://example.com/oauth/callback"></textarea>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                请确保回调URL与您应用中配置的完全一致
              </p>
            </div>

            <div class="col-span-1 md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                状态
              </label>
              <div class="flex items-center">
                <input v-model="editForm.IsActive" type="checkbox" id="isActive"
                       class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"/>
                <label for="isActive" class="ml-2 block text-sm text-gray-900 dark:text-white">
                  启用客户端
                </label>
              </div>
            </div>
          </div>

          <div class="mt-6 flex justify-end space-x-3">
            <button type="button" @click="handleEditModalClose"
                    class="px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors">
              取消
            </button>
            <button type="submit" :disabled="isSubmitting"
                    class="px-4 py-2 rounded-lg bg-blue-500 text-white hover:bg-blue-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
              {{ isSubmitting ? '保存中...' : '保存' }}
            </button>
          </div>
        </form>
      </div>
    </n-modal>

    <!-- 详情模态框 -->
    <n-modal v-model:show="showDetailsModal" title="客户端详情" preset="dialog" size="large"
             @close="handleDetailsModalClose"
             class="rounded-xl overflow-hidden bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
      <div class="p-4">
        <div class="flex flex-col items-center mb-6">
          <img v-if="selectedApp?.LogoUrl" :src="selectedApp.LogoUrl" :alt="selectedApp.ApplicationName"
               class="h-24 w-24 rounded-lg object-cover mb-4"/>
          <div v-else class="h-24 w-24 rounded-lg bg-gray-200 dark:bg-gray-600 flex items-center justify-center mb-4">
            <Icon icon="ion:app" width="48" height="48" class="text-gray-500 dark:text-gray-400"/>
          </div>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
            {{ selectedApp?.ApplicationName }}
          </h2>
          <p class="text-gray-500 dark:text-gray-400 mt-1">
            创建于 {{ formatDate(selectedApp?.CreatedAt) }}
          </p>
        </div>

        <div class="space-y-6">
          <div>
            <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              描述
            </h3>
            <p class="text-gray-900 dark:text-white bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
              {{ selectedApp?.Description || '无描述' }}
            </p>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                客户端ID
              </h3>
              <div class="flex items-center">
                <code
                    class="text-sm text-gray-600 dark:text-gray-300 bg-gray-50 dark:bg-gray-700 px-3 py-2 rounded font-mono select-all flex-grow break-all">
                  {{ selectedApp?.ClientId }}
                </code>
                <button @click="copyToClipboard(selectedApp?.ClientId)"
                        class="ml-2 text-gray-400 hover:text-blue-500 transition-colors flex-shrink-0"
                        title="复制客户端ID">
                  <Icon icon="ion:copy-outline" width="20" height="20"/>
                </button>
              </div>

              <div>
                <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 mt-4">
                  应用主页
                </h3>
                <a v-if="selectedApp?.HomepageUrl" :href="selectedApp.HomepageUrl" target="_blank"
                   rel="noopener noreferrer" class="text-blue-500 hover:text-blue-600 transition-colors break-all">
                  {{ selectedApp.HomepageUrl }}
                </a>
                <span v-else class="text-gray-500 dark:text-gray-400">无</span>
              </div>
            </div>

            <div>
              <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                客户端密钥
              </h3>
              <div class="relative">
                <input :value="selectedApp?.ClientSecret" type="password" readonly
                       class="w-full px-3 py-2 rounded-lg bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white focus:outline-none font-mono text-sm"/>
                <button @click="toggleSecretVisibility"
                        class="absolute right-2 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors">
                  <Icon :icon="showSecret ? 'ion:eye-off-outline' : 'ion:eye-outline'" width="20" height="20"/>
                </button>
                <button @click="copyToClipboard(selectedApp?.ClientSecret)"
                        class="absolute right-8 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-blue-500 transition-colors"
                        title="复制客户端密钥">
                  <Icon icon="ion:copy-outline" width="20" height="20"/>
                </button>
              </div>

              <div class="mt-4">
                <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  状态
                </h3>
                <span :class="[
                  'px-2.5 py-0.5 inline-flex text-xs leading-5 font-semibold rounded-full',
                  selectedApp?.IsActive
                    ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
                    : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                ]">
                  {{ selectedApp?.IsActive ? '启用' : '禁用' }}
                </span>
              </div>
            </div>
          </div>

          <div>
            <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              回调URL
            </h3>
            <div class="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
              <div v-for="(uri, index) in selectedApp?.RedirectUris.split(';')" :key="index"
                   class="flex items-start mb-2 last:mb-0">
                <Icon icon="ion:link" width="16" height="16" class="text-gray-400 mr-2 mt-0.5 flex-shrink-0"/>
                <div class="flex-grow">
                  <code class="text-sm text-gray-600 dark:text-gray-300 font-mono break-all">
                    {{ uri }}
                  </code>
                </div>
                <button @click="copyToClipboard(uri)"
                        class="ml-2 text-gray-400 hover:text-blue-500 transition-colors flex-shrink-0" title="复制URL">
                  <Icon icon="ion:copy-outline" width="16" height="16"/>
                </button>
              </div>
            </div>
          </div>

          <div class="flex justify-end space-x-3 mt-6">
            <button @click="handleDetailsModalClose"
                    class="px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors">
              关闭
            </button>
          </div>
        </div>
      </div>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue';
import {useMessage, useDialog, NModal} from 'naive-ui';
import {Icon} from '@iconify/vue';
import SkeletonLoader from '../components/SkeletonLoader.vue';
import {ClientAppService} from '../services/ClientAppService';
import type {ClientApplication, CreateClientAppModel, UpdateClientAppModel, RegenerateSecretResult} from '../models';

const message = useMessage();
const dialog = useDialog();

// 状态管理
const clientApps = ref<ClientApplication[]>([]);
const searchQuery = ref('');
const isLoading = ref(false);
const isSubmitting = ref(false);

// 模态框状态
const showCreateModal = ref(false);
const showEditModal = ref(false);
const showDetailsModal = ref(false);
const selectedApp = ref<ClientApplication | null>(null);
const showSecret = ref(false);

// 创建表单
const createForm = ref<CreateClientAppModel>({
  ApplicationName: '',
  Description: '',
  HomepageUrl: '',
  RedirectUris: [],
  LogoUrl: ''
});
const redirectUrisText = ref('');

// 编辑表单
const editForm = ref<UpdateClientAppModel>({
  ApplicationName: '',
  Description: '',
  HomepageUrl: '',
  RedirectUris: [],
  LogoUrl: '',
  IsActive: true
});
const editRedirectUrisText = ref('');
const currentEditClientId = ref('');

// 计算属性：过滤后的客户端应用列表
const filteredClientApps = computed(() => {
  if (!searchQuery.value.trim()) {
    return clientApps.value;
  }

  const query = searchQuery.value.toLowerCase().trim();
  return clientApps.value.filter(app =>
      app.ApplicationName.toLowerCase().includes(query) ||
      app.ClientId.toLowerCase().includes(query)
  );
});

// 初始化数据
const loadClientApps = async () => {
  isLoading.value = true;
  try {
    clientApps.value = await ClientAppService.getAllClientApplications();
  } catch (error) {
    message.error((error as Error).message || '获取客户端应用列表失败');
  } finally {
    isLoading.value = false;
  }
};

// 格式化日期
const formatDate = (dateString?: string) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};

// 复制到剪贴板
const copyToClipboard = async (text?: string) => {
  if (!text) return;
  try {
    await navigator.clipboard.writeText(text);
    message.success('已复制到剪贴板');
  } catch (error) {
    message.error('复制失败');
  }
};

// 创建模态框
const openCreateModal = () => {
  createForm.value = {
    ApplicationName: '',
    Description: '',
    HomepageUrl: '',
    RedirectUris: [],
    LogoUrl: ''
  };
  redirectUrisText.value = '';
  showCreateModal.value = true;
};

const handleCreateModalClose = () => {
  showCreateModal.value = false;
  // 重置表单
  createForm.value = {
    ApplicationName: '',
    Description: '',
    HomepageUrl: '',
    RedirectUris: [],
    LogoUrl: ''
  };
  redirectUrisText.value = '';
};

const handleCreateSubmit = async () => {
  // 处理回调URL文本为数组
  const redirectUris = redirectUrisText.value
      .split('\n')
      .map(uri => uri.trim())
      .filter(uri => uri);

  if (redirectUris.length === 0) {
    message.warning('请至少添加一个回调URL');
    return;
  }

  createForm.value.RedirectUris = redirectUris;

  isSubmitting.value = true;
  try {
    await ClientAppService.createClientApplication(createForm.value);
    message.success('客户端应用创建成功');
    handleCreateModalClose();
    await loadClientApps();
  } catch (error) {
    message.error((error as Error).message || '创建客户端应用失败');
  } finally {
    isSubmitting.value = false;
  }
};

// 编辑模态框
const openEditModal = (app: ClientApplication) => {
  selectedApp.value = app;
  currentEditClientId.value = app.ClientId;
  editForm.value = {
    ApplicationName: app.ApplicationName,
    Description: app.Description,
    HomepageUrl: app.HomepageUrl,
    RedirectUris: app.RedirectUris.split(';'),
    LogoUrl: app.LogoUrl,
    IsActive: app.IsActive
  };
  editRedirectUrisText.value = app.RedirectUris.split(';').join('\n');
  showEditModal.value = true;
};

const handleEditModalClose = () => {
  showEditModal.value = false;
  selectedApp.value = null;
  currentEditClientId.value = '';
};

const handleEditSubmit = async () => {
  // 处理回调URL文本为数组
  const redirectUris = editRedirectUrisText.value
      .split('\n')
      .map(uri => uri.trim())
      .filter(uri => uri);

  if (redirectUris.length === 0) {
    message.warning('请至少添加一个回调URL');
    return;
  }

  editForm.value.RedirectUris = redirectUris;

  isSubmitting.value = true;
  try {
    await ClientAppService.updateClientApplication(currentEditClientId.value, editForm.value);
    message.success('客户端应用更新成功');
    handleEditModalClose();
    await loadClientApps();
  } catch (error) {
    message.error((error as Error).message || '更新客户端应用失败');
  } finally {
    isSubmitting.value = false;
  }
};

// 详情模态框
const openDetailsModal = (app: ClientApplication) => {
  selectedApp.value = app;
  showSecret.value = false;
  showDetailsModal.value = true;
};

const handleDetailsModalClose = () => {
  showDetailsModal.value = false;
  selectedApp.value = null;
  showSecret.value = false;
};

const toggleSecretVisibility = () => {
  showSecret.value = !showSecret.value;
};

// 删除确认
const confirmDelete = (app: ClientApplication) => {
  dialog.warning({
    title: '确认删除',
    content: `确定要删除客户端应用 "${app.ApplicationName}" 吗？此操作不可撤销。`,
    positiveText: '删除',
    negativeText: '取消',
    positiveButtonProps: {
      type: 'error'
    },
    onPositiveClick: async () => {
      try {
        await ClientAppService.deleteClientApplication(app.ClientId);
        message.success('客户端应用删除成功');
        await loadClientApps();
      } catch (error) {
        message.error((error as Error).message || '删除客户端应用失败');
      }
    }
  });
};

// 重新生成密钥确认
const confirmRegenerateSecret = () => {
  dialog.warning({
    title: '重新生成密钥',
    content: '确定要重新生成客户端密钥吗？这将使当前密钥失效，可能会影响正在使用该应用的用户。',
    positiveText: '生成新密钥',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        const result: RegenerateSecretResult = await ClientAppService.regenerateClientSecret(currentEditClientId.value);

        // 显示新密钥的临时提示框
        dialog.info({
          title: '新的客户端密钥',
          content: `
            <div class="space-y-4">
              <p>请立即保存以下新密钥，它只在本次会话中显示一次。</p>
              <div class="flex">
                <code class="text-sm text-gray-600 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 px-3 py-2 rounded font-mono select-all flex-grow break-all">
                  ${result.NewSecret}
                </code>
                <button
                  id="copy-secret-btn"
                  class="ml-2 px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors flex-shrink-0"
                >
                  复制
                </button>
              </div>
            </div>
          `,
          positiveText: '已保存',
          closable: false,
          onPositiveClick: () => {
            // 可以在这里添加清理逻辑
          }
        });

        // 添加点击复制按钮的事件监听器
        setTimeout(() => {
          const copyBtn = document.getElementById('copy-secret-btn');
          if (copyBtn) {
            copyBtn.onclick = () => {
              copyToClipboard(result.NewSecret);
            };
          }
        }, 100);

        // 更新本地数据
        await loadClientApps();
        // 重新打开编辑模态框，确保显示最新数据
        const updatedApp = clientApps.value.find(app => app.ClientId === currentEditClientId.value);
        if (updatedApp) {
          openEditModal(updatedApp);
        }
      } catch (error) {
        message.error((error as Error).message || '重新生成密钥失败');
      }
    }
  });
};

// 组件挂载时加载数据
onMounted(() => {
  loadClientApps();
});
</script>

<style scoped>
/* 添加一些自定义样式以增强苹果风格的视觉效果 */
.n-modal {
  --n-modal-border-radius: 16px;
}

.n-dialog__header {
  border-bottom: 1px solid var(--n-border-color);
  padding: 16px 24px;
}

.n-dialog__footer {
  border-top: 1px solid var(--n-border-color);
  padding: 16px 24px;
}

/* 动画效果 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-enter-active,
.slide-leave-active {
  transition: transform 0.3s ease;
}

.slide-enter-from {
  transform: translateY(-20px);
}

.slide-leave-to {
  transform: translateY(20px);
}
</style>
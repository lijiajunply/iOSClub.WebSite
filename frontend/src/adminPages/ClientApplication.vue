<template>
  <div class="min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-300">
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 页面标题 -->
      <div class="mb-8">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 class="text-2xl font-semibold tracking-tight">客户端应用管理</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">管理第三方应用的OAuth客户端</p>
          </div>

          <div class="flex items-center gap-3 mt-4 sm:mt-0">
            <button @click="showHelpGuide = true"
                    class="px-4 py-2 rounded-full bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 transition-colors flex items-center justify-center">
              <Icon icon="ion:help-circle-outline" class="mr-1" width="18" height="18"/>
              帮助
            </button>
            <button @click="openCreateModal"
                    class="px-4 py-2 rounded-full bg-blue-500 hover:bg-blue-600 text-white transition-colors flex items-center justify-center">
              <Icon icon="ion:add" class="mr-1" width="18" height="18"/>
              创建客户端
            </button>
          </div>
        </div>
      </div>

      <!-- 搜索和筛选 -->
      <div class="mb-6">
        <div class="relative">
          <input v-model="searchQuery" type="text" placeholder="搜索应用名称或客户端ID..."
                 class="w-full pl-10 pr-4 py-2 rounded-xl bg-gray-100 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"/>
          <Icon icon="ion:search" class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" width="20"
                height="20"/>
        </div>
      </div>

      <!-- 客户端应用列表 -->
      <div
          class="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden transition-all duration-300 hover:shadow-lg">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
            <thead class="bg-gray-50 dark:bg-gray-800">
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
                    <SkeletonLoader type="avatar"/>
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
              <tr v-for="app in filteredClientApps" :key="app.clientId"
                  class="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors duration-150">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <img v-if="app.logoUrl" :src="app.logoUrl" :alt="app.applicationName"
                         class="h-8 w-8 rounded-md object-cover"/>
                    <div v-else
                         class="h-8 w-8 rounded-md bg-gray-200 dark:bg-gray-600 flex items-center justify-center">
                      <Icon icon="lucide:app-window-mac" width="20" height="20"
                            class="text-gray-500 dark:text-gray-400"/>
                    </div>
                    <div class="ml-4">
                      <div class="text-sm font-medium text-gray-900 dark:text-white">
                        {{ app.applicationName }}
                      </div>
                      <div class="text-xs text-gray-500 dark:text-gray-400 line-clamp-1 max-w-xs">
                        {{ app.description }}
                      </div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <code
                        class="text-xs text-gray-600 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded font-mono select-all">
                      {{ app.clientId }}
                    </code>
                    <button @click="copyToClipboard(app.clientId)"
                            class="ml-2 text-gray-400 hover:text-blue-500 transition-colors" title="复制客户端ID">
                      <Icon icon="ion:copy-outline" width="16" height="16"/>
                    </button>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <span :class="[
                      'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                      app.isActive
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
                        : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                    ]">
                      {{ app.isActive ? '启用' : '禁用' }}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                  {{ formatDate(app.createdAt) }}
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
    </main>
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
            <input v-model="createForm.applicationName" type="text" required
                   class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                   placeholder="输入应用名称"/>
          </div>

          <div class="col-span-1 md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              应用描述
            </label>
            <textarea v-model="createForm.description" rows="3"
                      class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors resize-none"
                      placeholder="描述您的应用..."></textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              应用主页URL <span class="text-red-500">*</span>
            </label>
            <input v-model="createForm.homepageUrl" type="url" required
                   class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                   placeholder="https://example.com"/>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              应用图标URL
            </label>
            <input v-model="createForm.logoUrl" type="url"
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

          <div class="col-span-1 md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              高级设置
            </label>
            <div class="flex items-center">
              <input v-model="createForm.supportsPkce" type="checkbox" id="supportsPkceCreate"
                     class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"/>
              <label for="supportsPkceCreate" class="ml-2 block text-sm text-gray-900 dark:text-white">
                支持 PKCE (Proof Key for Code Exchange)
              </label>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
              对于移动应用和单页应用，建议启用 PKCE 以增强安全性
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
            <input v-model="editForm.applicationName" type="text" required
                   class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                   placeholder="输入应用名称"/>
          </div>

          <div class="col-span-1 md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              应用描述
            </label>
            <textarea v-model="editForm.description" rows="3"
                      class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors resize-none"
                      placeholder="描述您的应用..."></textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              应用主页URL <span class="text-red-500">*</span>
            </label>
            <input v-model="editForm.homepageUrl" type="url" required
                   class="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                   placeholder="https://example.com"/>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              应用图标URL
            </label>
            <input v-model="editForm.logoUrl" type="url"
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
              <input v-model="editForm.isActive" type="checkbox" id="isActive"
                     class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"/>
              <label for="isActive" class="ml-2 block text-sm text-gray-900 dark:text-white">
                启用客户端
              </label>
            </div>
          </div>

          <div class="col-span-1 md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              高级设置
            </label>

            <div class="flex items-center">
              <input v-model="editForm.isNeedEMail" type="checkbox" id="isNeedEMailEdit"
                     class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"/>
              <label for="isNeedEMailEdit" class="ml-2 block text-sm text-gray-900 dark:text-white">
                需要邮箱验证
              </label>
            </div>

            <div class="flex items-center mt-2">
              <input v-model="editForm.supportsPkce" type="checkbox" id="supportsPkceEdit"
                     class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"/>
              <label for="supportsPkceEdit" class="ml-2 block text-sm text-gray-900 dark:text-white">
                支持 PKCE (Proof Key for Code Exchange)
              </label>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
              对于移动应用和单页应用，建议启用 PKCE 以增强安全性
            </p>
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
        <img v-if="selectedApp?.logoUrl" :src="selectedApp.logoUrl" :alt="selectedApp.applicationName"
             class="h-24 w-24 rounded-lg object-cover mb-4"/>
        <div v-else class="h-24 w-24 rounded-lg bg-gray-200 dark:bg-gray-600 flex items-center justify-center mb-4">
          <Icon icon="lucide:app-window-mac" width="48" height="48" class="text-gray-500 dark:text-gray-400"/>
        </div>
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
          {{ selectedApp?.applicationName }}
        </h2>
        <p class="text-gray-500 dark:text-gray-400 mt-1">
          创建于 {{ formatDate(selectedApp?.createdAt) }}
        </p>
      </div>

      <div class="space-y-6">
        <div>
          <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            描述
          </h3>
          <p class="text-gray-900 dark:text-white bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
            {{ selectedApp?.description || '无描述' }}
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
                {{ selectedApp?.clientId }}
              </code>
              <button @click="copyToClipboard(selectedApp?.clientId)"
                      class="ml-2 text-gray-400 hover:text-blue-500 transition-colors flex-shrink-0"
                      title="复制客户端ID">
                <Icon icon="ion:copy-outline" width="20" height="20"/>
              </button>
            </div>

            <div class="mt-4">
              <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                邮箱验证
              </h3>
              <span :class="[
                  'px-2.5 py-0.5 inline-flex text-xs leading-5 font-semibold rounded-full',
                  selectedApp?.isNeedEMail
                    ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
                    : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                ]">
                  {{ selectedApp?.isNeedEMail ? '是' : '否' }}
                </span>
            </div>

            <div>
              <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 mt-4">
                应用主页
              </h3>
              <a v-if="selectedApp?.homepageUrl" :href="selectedApp.homepageUrl" target="_blank"
                 rel="noopener noreferrer" class="text-blue-500 hover:text-blue-600 transition-colors break-all">
                {{ selectedApp.homepageUrl }}
              </a>
              <span v-else class="text-gray-500 dark:text-gray-400">无</span>
            </div>
          </div>

          <div>
            <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              客户端密钥
            </h3>
            <div class="relative">
              <input :value="selectedApp?.clientSecret" type="password" readonly
                     class="w-full px-3 py-2 rounded-lg bg-gray-50 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white focus:outline-none font-mono text-sm"/>
              <button @click="copyToClipboard(selectedApp?.clientSecret)"
                      class="ml-2 absolute top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-blue-500 transition-colors"
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
                  selectedApp?.isActive
                    ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300'
                    : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                ]">
                  {{ selectedApp?.isActive ? '启用' : '禁用' }}
                </span>
            </div>

            <div class="mt-4">
              <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                支持 PKCE
              </h3>
              <span :class="[
                  'px-2.5 py-0.5 inline-flex text-xs leading-5 font-semibold rounded-full',
                  selectedApp?.supportsPkce
                    ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300'
                    : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                ]">
                  {{ selectedApp?.supportsPkce ? '是' : '否' }}
                </span>
            </div>
          </div>
        </div>

        <div>
          <h3 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            回调URL
          </h3>
          <div class="bg-gray-50 dark:bg-gray-700 p-3 rounded-lg">
            <div v-for="(uri, index) in selectedApp?.redirectUris.split(';')" :key="index"
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
      </div>
    </div>
  </n-modal>

  <!-- 帮助指南 -->
  <Transition name="fade">
    <HelpGuide
        v-if="showHelpGuide"
        :title="'客户端应用管理帮助'"
        :visible="showHelpGuide"
        @close="handleHelpGuideClose"
    >
      <div class="space-y-6">
        <section>
          <h2 class="text-xl font-medium mb-3">概述</h2>
          <p>客户端应用管理允许您创建和配置用于OAuth
            2.0认证的第三方应用程序。通过此页面，您可以管理应用的基本信息、回调URL以及安全设置。</p>
        </section>

        <section>
          <h2 class="text-xl font-medium mb-3">核心功能</h2>
          <div class="grid gap-4">
            <div class="bg-gray-50 dark:bg-gray-700/50 p-4 rounded-xl">
              <h3 class="text-lg font-medium mb-2 flex items-center">
                <Icon icon="ion:add-circle-outline" class="mr-2 text-blue-500" width="20" height="20"/>
                创建客户端应用
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">
                点击"创建客户端"按钮，填写应用名称、描述、主页URL和回调URL等信息，系统将生成唯一的客户端ID和密钥。</p>
            </div>

            <div class="bg-gray-50 dark:bg-gray-700/50 p-4 rounded-xl">
              <h3 class="text-lg font-medium mb-2 flex items-center">
                <Icon icon="ion:eye-outline" class="mr-2 text-blue-500" width="20" height="20"/>
                查看应用详情
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">
                点击"详情"按钮，查看应用的完整信息，包括客户端ID、客户端密钥和配置详情。</p>
            </div>

            <div class="bg-gray-50 dark:bg-gray-700/50 p-4 rounded-xl">
              <h3 class="text-lg font-medium mb-2 flex items-center">
                <Icon icon="ion:pencil-outline" class="mr-2 text-blue-500" width="20" height="20"/>
                编辑应用配置
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">
                点击"编辑"按钮，可以修改应用的基本信息、回调URL以及启用/禁用状态。</p>
            </div>
          </div>
        </section>

        <section>
          <h2 class="text-xl font-medium mb-3">配置说明</h2>
          <div class="space-y-4">
            <div>
              <h3 class="text-lg font-medium mb-1">回调URL</h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">
                回调URL是OAuth流程完成后重定向用户的地址。请确保URL格式正确，多个URL请每行一个。</p>
            </div>

            <div>
              <h3 class="text-lg font-medium mb-1">PKCE支持</h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">PKCE (Proof Key for Code Exchange)
                提供额外的安全层，推荐移动应用和单页应用启用此选项。</p>
            </div>

            <div>
              <h3 class="text-lg font-medium mb-1">客户端密钥</h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">
                客户端密钥是敏感信息，请妥善保管。如果密钥泄露，请重新生成或更新应用配置。</p>
            </div>
          </div>
        </section>

        <section>
          <h2 class="text-xl font-medium mb-3">权限说明 (Scopes)</h2>
          <div class="space-y-4">
            <div>
              <h3 class="text-lg font-medium mb-1">什么是Scopes</h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">
                Scopes是OAuth
                2.0中的权限范围，定义了第三方应用可以访问的用户数据。系统会根据访问令牌中包含的scope返回相应的用户信息。</p>
            </div>

            <div>
              <h3 class="text-lg font-medium mb-1">权限范围列表</h3>
              <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                  <thead class="bg-gray-50 dark:bg-gray-700">
                  <tr>
                    <th scope="col"
                        class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      Scope
                    </th>
                    <th scope="col"
                        class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      返回字段
                    </th>
                    <th scope="col"
                        class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                      说明
                    </th>
                  </tr>
                  </thead>
                  <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  <tr>
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white"><code>openid</code>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"><code>sub</code>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">用户ID</td>
                  </tr>
                  <tr>
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white"><code>profile</code>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"><code>name</code>,
                      <code>nickname</code>, <code>academy</code>, <code>class</code></td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">用户基本信息</td>
                  </tr>
                  <tr>
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white"><code>email</code>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      <code>email</code></td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">用户邮箱地址</td>
                  </tr>
                  <tr>
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                      <code>read</code>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"><code>role</code>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">用户角色</td>
                  </tr>
                  <tr>
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white"><code>phone</code>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      <code>phone</code></td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">用户电话号码</td>
                  </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <div>
              <h3 class="text-lg font-medium mb-1">权限配置建议</h3>
              <p class="text-sm text-gray-600 dark:text-gray-300">
                应用应该仅请求必要的最小权限范围，遵循最小权限原则，提高安全性。例如，如果应用只需要识别用户身份，只需请求<code>openid</code>
                scope即可。</p>
            </div>
          </div>
        </section>

        <section>
          <h2 class="text-xl font-medium mb-3">安全最佳实践</h2>
          <ul class="list-disc list-inside space-y-2 text-sm text-gray-600 dark:text-gray-300">
            <li>仅在服务器端存储客户端密钥，不要在前端代码中暴露</li>
            <li>使用HTTPS协议的回调URL</li>
            <li>为移动和单页应用启用PKCE</li>
            <li>定期审查和更新您的OAuth配置</li>
            <li>及时禁用不再使用的客户端应用</li>
          </ul>
        </section>
      </div>
    </HelpGuide>
  </Transition>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue';
import {useMessage, useDialog, NModal} from 'naive-ui';
import {Icon} from '@iconify/vue';
import HelpGuide from '../components/HelpGuide.vue';
import SkeletonLoader from '../components/SkeletonLoader.vue';
import {ClientAppService} from '../services/ClientAppService';
import type {ClientApplication, CreateClientAppModel, UpdateClientAppModel} from '../models';

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
const showHelpGuide = ref(false);
const selectedApp = ref<ClientApplication | null>(null);
const showSecret = ref(false);

// 创建表单
const createForm = ref<CreateClientAppModel>({
  applicationName: '',
  description: '',
  homepageUrl: '',
  redirectUris: [],
  logoUrl: '',
  supportsPkce: false,
  isNeedEMail: false
});
const redirectUrisText = ref('');

// 编辑表单
const editForm = ref<UpdateClientAppModel>({
  applicationName: '',
  description: '',
  homepageUrl: '',
  redirectUris: [],
  logoUrl: '',
  isActive: true,
  supportsPkce: false,
  isNeedEMail: false
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
      app.applicationName.toLowerCase().includes(query) ||
      app.clientId.toLowerCase().includes(query)
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
    applicationName: '',
    description: '',
    homepageUrl: '',
    redirectUris: [],
    logoUrl: '',
    supportsPkce: false,
    isNeedEMail: false
  };
  redirectUrisText.value = '';
  showCreateModal.value = true;
};

const handleCreateModalClose = () => {
  showCreateModal.value = false;
  // 重置表单
  createForm.value = {
    applicationName: '',
    description: '',
    homepageUrl: '',
    redirectUris: [],
    logoUrl: '',
    supportsPkce: false,
    isNeedEMail: false
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

  createForm.value.redirectUris = redirectUris;

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
  currentEditClientId.value = app.clientId;
  editForm.value = {
    applicationName: app.applicationName,
    description: app.description,
    homepageUrl: app.homepageUrl,
    redirectUris: app.redirectUris.split(';'),
    logoUrl: app.logoUrl,
    isActive: app.isActive,
    supportsPkce: app.supportsPkce || false,
    isNeedEMail: app.isNeedEMail || false
  };
  editRedirectUrisText.value = app.redirectUris.split(';').join('\n');
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

  editForm.value.redirectUris = redirectUris;

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

const handleHelpGuideClose = () => {
  showHelpGuide.value = false;
};

// 删除确认
const confirmDelete = (app: ClientApplication) => {
  dialog.warning({
    title: '确认删除',
    content: `确定要删除客户端应用 "${app.applicationName}" 吗？此操作不可撤销。`,
    positiveText: '删除',
    negativeText: '取消',
    positiveButtonProps: {
      type: 'error'
    },
    onPositiveClick: async () => {
      try {
        await ClientAppService.deleteClientApplication(app.clientId);
        message.success('客户端应用删除成功');
        await loadClientApps();
      } catch (error) {
        message.error((error as Error).message || '删除客户端应用失败');
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
:deep(.n-modal) {
  --n-modal-border-radius: 16px;
}

:deep(.n-dialog__header) {
  border-bottom: 1px solid var(--n-border-color);
  padding: 16px 24px;
}

:deep(.n-dialog__footer) {
  border-top: 1px solid var(--n-border-color);
  padding: 16px 24px;
}

/* 动画效果 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
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

/* 帮助按钮动画 */
.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}
</style>
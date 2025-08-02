<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 to-blue-50">
    <n-layout class="min-h-screen">
      <n-layout has-sider class="flex-1">
        <!-- 左侧文章列表 -->
        <n-layout-sider
            bordered
            collapse-mode="width"
            :collapsed-width="0"
            :native-scrollbar="false"
            class="bg-white/60 backdrop-blur-sm"
            v-if="selectedArticle"
        >
          <div class="p-4">
            <n-menu :options="menuItems"/>
          </div>
        </n-layout-sider>

        <!-- 右侧文章详情 -->
        <n-layout-content class="relative">
          <div v-if="selectedArticle">
            <!-- 文章头部 -->
            <div class="p-8 mb-6">
              <div class="mb-6">
                <div class="text-xl lg:text-4xl font-bold text-gray-900 leading-tight mb-4">
                  {{ selectedArticle.title }}
                </div>
                <div class="flex items-center space-x-6 text-gray-500 text-sm">
                  <span class="flex items-center">
                    <n-icon size="16" class="mr-2">
                      <CalendarOutlined/>
                    </n-icon>
                    {{ selectedArticle.date }}
                  </span>
                  <span class="flex items-center">
                    <n-icon size="16" class="mr-2">
                      <EyeFilled/>
                    </n-icon>
                    {{ selectedArticle.watch }} 次阅读
                  </span>
                </div>
              </div>

              <div class="p-8">
                <div class="prose prose-lg max-w-none">
                  <MarkdownComponent :content="selectedArticle.content"/>
                </div>
              </div>
            </div>
          </div>

          <!-- 空状态 -->
          <div v-else class="flex items-center justify-center h-full">
            <div class="text-center">
              <n-icon size="64" class="text-gray-300 mb-4">
                <svg viewBox="0 0 24 24">
                  <path fill="currentColor"
                        d="M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.89 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm4 18H6V4h7v5h5v11z"/>
                </svg>
              </n-icon>
              <p class="text-gray-500 text-lg">请选择一篇文章阅读</p>
            </div>
          </div>
        </n-layout-content>
      </n-layout>
    </n-layout>
  </div>
</template>

<script setup lang="ts">
import {ref} from 'vue'
import {
  NLayout,
  NLayoutSider,
  NLayoutContent,
  NMenu,
  NIcon,
  type MenuOption,
} from 'naive-ui'

// import {ArticleService, type ArticleModel} from "../services/ArticleService.js";
import {CalendarOutlined, EyeFilled} from "@vicons/antd";
import MarkdownComponent from "../components/MarkdownComponent.vue";

const menuItems = ref<MenuOption[]>([])

const selectedArticle = ref<any>({
  title: '',
  date: '',
  watch: 0,
  content: ``,
})
</script>

<style scoped>

</style>
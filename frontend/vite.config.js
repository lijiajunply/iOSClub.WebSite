import {defineConfig} from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
    plugins: [vue(), tailwindcss()],
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    // 将较大的依赖或特定模块分离到固定名称的块中
                    'client-application': ['./src/adminPages/ClientApplication.vue'],
                    // 对较大的第三方库进行单独打包
                    'naive-ui': ['naive-ui'],
                    'echarts-core': ['echarts/core'],
                    'echarts-charts': ['echarts/charts'],
                    'echarts-components': ['echarts/components'],
                    'markdown-it': ['markdown-it'],
                    'prismjs': ['prismjs']
                }
            }
        },
        // 调整块大小警告限制
        chunkSizeWarningLimit: 1500
    }
});
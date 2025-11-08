import {defineConfig} from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
    plugins: [vue(), tailwindcss()],
    server: {
        proxy: {
            '/api': {
                target: 'https://api.xauat.site',
                changeOrigin: true,
                rewrite: (path) => path.replace(/^\/api/, '')
            }
        }
    },
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    // 对较大的第三方库进行单独打包
                    'naive-ui': ['naive-ui'],
                    'echarts-core': ['echarts/core'],
                    'echarts-charts': ['echarts/charts'],
                    'echarts-components': ['echarts/components'],
                    'markdown-it': ['markdown-it'],
                    'prismjs': ['prismjs']
                    // 移除了 'client-application' 的手动分块，避免循环依赖问题
                }
            }
        },
        // 调整块大小警告限制
        chunkSizeWarningLimit: 1500
    }
});
// https://vitepress.dev/guide/custom-theme
import { h } from 'vue'
import type { Theme } from 'vitepress'
import DefaultTheme from 'vitepress/theme'
import { onMounted, watch, nextTick } from 'vue'
import { useRoute } from 'vitepress'
import mediumZoom from 'medium-zoom'
import './style.css'
import Tools from '../components/Tools.vue'
import ArticlesPage from '../components/ArticlesPage.vue'
import '//at.alicdn.com/t/c/font_4612528_md4hjwjgcb.js'
import IconFont from '../components/IconFont.vue'
import Login from '../components/Login.vue'
import Signup from '../components/Signup.vue'

export default {
  extends: DefaultTheme,
  Layout: () => {
    return h(DefaultTheme.Layout, null, {
      // https://vitepress.dev/guide/extending-default-theme#layout-slots
    })
  },
  enhanceApp({ app, router, siteData }) {
    // ...
    app.component('Tools', Tools)
    app.component('ArticlesPage',ArticlesPage)
    app.component('IconFont',IconFont)
    app.component('Login',Login)
    app.component('Signup',Signup)
  },
  setup() {
    const route = useRoute() 
    const initZoom = () => {
      // 不显式添加{data-zoomable}的情况下为所有图像启用此功能
      mediumZoom('.main img', { background: 'var(--vp-c-bg)' })
    }
    onMounted(() => {
      initZoom()
    })
    watch(
      () => route.path,
      () => nextTick(() => initZoom())
    )
  },
} satisfies Theme

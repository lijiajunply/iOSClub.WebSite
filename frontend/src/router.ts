// src/router.ts
import { createRouter, createWebHistory, type NavigationGuardNext, type RouteLocationNormalized } from 'vue-router'
import { useAuthorizationStore } from './stores/Authorization'

const routes = [
  {
    path: '/',
    name: 'main',
    component: () => import('./layouts/MainLayout.vue'),
    children: [
      {
        path: '',
        name: 'Home',
        meta: { title: '首页 - 西建大 iOS Club' },
        component: () => import('./pages/Home.vue')
      },
      {
        path: '/about',
        name: 'About',
        meta: { title: '关于我们 - 西建大 iOS Club' },
        component: () => import('./pages/About.vue')
      },
      {
        path: '/structure',
        name: 'Structure',
        meta: { title: '社团结构 - 西建大 iOS Club' },
        component: () => import('./pages/Structure.vue')
      },
      {
        path: '/articles',
        name: 'Articles',
        meta: { title: '社团动态 - 西建大 iOS Club' },
        component: () => import('./pages/Articles.vue')
      },
      {
        path: '/article/:id',
        name: 'ArticleDetail',
        meta: { title: '文章详情 - 西建大 iOS Club' },
        component: () => import('./pages/ArticleDetail.vue')
      },
      {
        path: '/projects',
        name: 'Projects',
        meta: { title: '精品项目 - 西建大 iOS Club' },
        component: () => import('./pages/Projects.vue')
      },
      {
        path: '/event',
        name: 'Event',
        meta: { title: '社团活动 - 西建大 iOS Club' },
        component: () => import('./pages/Event.vue')
      },
      {
        path: '/tools',
        name: 'Tools',
        meta: { title: '精品资源 - 西建大 iOS Club' },
        component: () => import('./pages/Tools.vue')
      },
      {
        path: '/login',
        name: 'Login',
        meta: { title: '登录到您的iMember - 西建大 iOS Club' },
        component: () => import('./pages/Login.vue')
      }
    ]
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    meta: { isNeedAdmin: true },
    component: () => import('./pages/AdminDashboard.vue'),
    children: []
  },
  {
    path: '/:catchAll(.*)',
    name: 'NotFound',
    meta: { title: '未能找到该页面' },
    component: () => import('./pages/NotFount.vue')
  }
]

const router = createRouter({
  history: createWebHistory(''),
  routes
})

router.beforeEach((to: RouteLocationNormalized, _: RouteLocationNormalized, next: NavigationGuardNext) => {
  if (to.meta.title && typeof to.meta.title === 'string') {
    document.title = to.meta.title
  }

  const authorizationStore = useAuthorizationStore()
  const isAdmin = authorizationStore.isAuthenticated

  if (typeof to.meta.isNeedAdmin === 'boolean' && to.meta.isNeedAdmin && !isAdmin) {
    next({ path: '/NotFound' })
  } else {
    next()
  }
})

export default router
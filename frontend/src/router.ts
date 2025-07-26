import {createRouter, createWebHistory} from 'vue-router';
import {useAuthorizationStore} from "./stores/Authorization.ts";

const routes = [
    {
        path: "/",
        name: "main",
        component: () => import("./layouts/MainLayout.vue"),   // 布局文件
        children: [
            {
                path: '',
                name: 'Home',
                meta: {title: "首页 - 西建大 iOS Club"},
                component: () => import('./pages/Home.vue'),
            },
            {
                path: '',
                name: 'Home',
                meta: {title: "首页 - 西建大 iOS Club"},
                component: () => import('./pages/Home.vue'),
            },
            {
                path: '/login',
                name: 'Login',
                meta: {title: "登录到您的iMember - 西建大 iOS Club"},
                component: () => import('./pages/Login.vue'),
            }
        ]
    },
    {
        path: '/admin',
        name: 'admin',
        meta: {isNeedAdmin: true},
        component: () => import('./layouts/AdminLayout.vue'),
        children: [

        ]
    },
    {
        path: '/:catchAll(.*)',
        name: 'NotFound',
        meta: {title: "未能找到该页面"},
        component: () => import('./pages/NotFount.vue'),
    },
];

const router = createRouter({
    history: createWebHistory(""),
    routes,
});

router.beforeEach((to, _, next) => {
    if (to.meta.title && typeof to.meta.title === 'string') {//判断是否有标题
        document.title = to.meta.title
    }

    const authorizationStore = useAuthorizationStore()

    const isAdmin = authorizationStore.isAuthenticated
    if (typeof to.meta.isNeedAdmin === 'boolean' && to.meta.isNeedAdmin && !isAdmin) {
        next({path: '/NotFound'})
    }
    next()
})


export default router;
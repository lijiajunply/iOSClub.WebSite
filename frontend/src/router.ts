import { createRouter, createWebHistory } from 'vue-router';

const routes = [
    {
        path: "/",
        name: "main",
        component: () => import("./layouts/MainLayout.vue"),   // 布局文件
        children: [
            {
                path: '',
                name: 'Home',
                meta: {title: ""},
                component: () => import('./pages/Home.vue'),
            },
        ]
    },

    {
        path: '/Login',
        name: 'Login',
        component: () => import('./pages/Login.vue')
    },
    {
        path: '/admin',
        name: 'admin',
        meta: {isNeedAdmin: true},
        component: () => import('./layouts/AdminLayout.vue'),
        children: [
            // 管理后台子路由
        ]
    },
    {
        // 通配符路由
        path: '/:catchAll(.*)',
        name: 'NotFound',
        meta: {title: "未能找到该页面"},
        component: () => import('./pages/NotFount.vue')
    }
];

const router = createRouter({
    history: createWebHistory(""),
    routes,
});

router.beforeEach((to, _, next) => {
    if (to.meta.title && typeof to.meta.title === 'string') {
        document.title = to.meta.title;
    }

    const isAdmin = true; // 实际项目中应从存储中获取
    if (typeof to.meta.isNeedAdmin === 'boolean' && to.meta.isNeedAdmin && !isAdmin) {
        next({path: '/NotFound'});
    } else {
        next();
    }
});

export default router;
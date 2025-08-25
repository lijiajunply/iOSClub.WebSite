import {createRouter, createWebHistory} from 'vue-router';
import {useAuthorizationStore} from "./stores/Authorization.ts";

const routes = [
    {
        path: "",
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
                path: 'projects',
                name: 'Projects',
                meta: {title: "社团项目 - 西建大 iOS Club"},
                component: () => import('./pages/Projects.vue'),
            },
            {
                path: 'login',
                name: 'Login',
                meta: {title: "登录到您的iMember - 西建大 iOS Club"},
                component: () => import('./pages/Login.vue'),
            },
            {
                path: 'signup',
                name: 'Signup',
                meta: {title: "注册账号 - 西建大 iOS Club"},
                component: () => import('./pages/Signup.vue'),
            },
            {
                path: 'forgotpassword',
                name: 'ForgotPassword',
                meta: {title: "找回密码 - 西建大 iOS Club"},
                component: () => import('./pages/ForgotPassword.vue'),
            },
            {
                path: 'event',
                name: 'Event',
                meta: {title: "社团活动 - 西建大 iOS Club"},
                component: () => import('./pages/Event.vue'),
            },
            {
                path: 'History',
                name: 'History',
                meta: {title: "社团历史 - 西建大 iOS Club"},
                component: () => import('./pages/History.vue'),
            },
            {
                path: 'Tools',
                name: 'Tools',
                meta: {title: "社团应用 - 西建大 iOS Club"},
                component: () => import('./pages/Tools.vue'),
            },
            {
                path: 'Blog',
                name: 'Blog',
                meta: {title: "社团文章 - 西建大 iOS Club"},
                component: () => import('./pages/Blog.vue'),
            },
            {
                path: '/',
                component: () => import('./layouts/WordLayout.vue'),
                children: [
                    {
                        path: 'article/:id',
                        name: 'Article',
                        component: () => import('./pages/Article.vue'),
                    },
                    {
                        path: 'About',
                        name: 'About',
                        meta: {title: "关于我们 - 西建大 iOS Club"},
                        component: () => import('./pages/Articles/About.vue'),
                    },
                    {
                        path: 'OtherOrg',
                        name: 'OtherOrg',
                        component: () => import('./pages/Articles/OtherOrg.vue'),
                        meta: {
                            title: '其他组织 - 西建大iOS Club'
                        }
                    },
                    {
                        path: 'Structure',
                        name: 'Structure',
                        meta: {title: "社团结构 - 西建大 iOS Club"},
                        component: () => import('./pages/Articles/Structure.vue'),
                    }
                ]
            },
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
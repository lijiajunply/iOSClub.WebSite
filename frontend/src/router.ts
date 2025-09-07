import {createRouter, createWebHistory} from 'vue-router';
import {useAuthorizationStore} from "./stores/Authorization.ts";
import MainLayout from "./layouts/MainLayout.vue";
import CentreLayout from "./layouts/CentreLayout.vue";

const routes = [
    {
        path: "",
        name: "main",
        component: MainLayout,
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
                name: 'SignUp',
                meta: {title: "注册账号 - 西建大 iOS Club"},
                component: () => import('./pages/SignUp.vue'),
            },
            {
                path: 'forgotPassword',
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
                path: 'Centre',
                name: 'Centre',
                meta: {title: "您的iMember中心 - 西建大 iOS Club", requiresAuth: true},
                component: () => import('./pages/Centre.vue'),
            }
        ]
    },
    {
        path: '/Centre',
        component: CentreLayout,
        meta: {requiresAuth: true},
        children: [
            {
                path: 'PersonalData',
                name: 'PersonalData',
                meta: {title: "个人数据 - 西建大 iOS Club"},
                component: () => import('./pages/PersonalData.vue'),
            },
            {
                path: 'MemberData',
                name: 'MemberData',
                meta: {title: "成员数据 - 西建大 iOS Club"},
                component: () => import('./pages/MemberData.vue'),
            },
            {
                path: 'Department',
                name: 'Department',
                meta: {title: "部门管理 - 西建大 iOS Club"},
                component: () => import('./pages/Department.vue'),
            },
            {
                path: 'Projects',
                name: 'ProjectsData',
                meta: {title: "项目管理 - 西建大 iOS Club"},
                component: () => import('./pages/ProjectsData.vue'),
            },
            {
                path: 'Resources',
                name: 'Resources',
                meta: {title: "资源管理 - 西建大 iOS Club"},
                component: () => import('./pages/Resources.vue'),
            },
            {
                path: 'Admin',
                name: 'Admin',
                meta: {title: "其他数据 - 西建大 iOS Club"},
                component: () => import('./pages/Admin.vue'),
            },
            {
                path: 'Article',
                name: 'Article',
                meta: {title: "社团文章 - 西建大 iOS Club"},
                component: () => import('./pages/CenterArticle.vue'),
            }
        ]
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

// Navigation guard
router.beforeEach((to, from, next) => {
    const authorizationStore = useAuthorizationStore();

    // Set page title
    document.title = to.meta.title || "西建大 iOS Club";

    // Check if route requires authentication
    if (to.meta.requiresAuth && !authorizationStore.isAuthenticated) {
        next('/login');
    } else {
        next();
    }
});

export default router;

import { defineStore } from 'pinia';

export const useLayoutStore = defineStore('layout', {
    state: () => ({
        showSidebar: true, // 侧边栏是否显示
        isMobile: typeof window !== 'undefined' ? window.innerWidth < 768 : false, // 是否为移动端
        isSidebarCollapsed: false, // 侧边栏是否折叠
    }),
    actions: {
        toggleSidebar() {
            this.showSidebar = !this.showSidebar;
        },
        toggleSidebarCollapse() {
            this.isSidebarCollapsed = !this.isSidebarCollapsed;
        },
        handleResize() {
            this.isMobile = typeof window !== 'undefined' ? window.innerWidth < 768 : false;
            if (this.isMobile) {
            } else {
                this.showSidebar = true;
            }
        },
    },
});
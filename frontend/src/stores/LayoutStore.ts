import { defineStore } from 'pinia';

export const useLayoutStore = defineStore('layout', {
    state: () => ({
        showSidebar: true, // 侧边栏是否显示
        isMobile: typeof window !== 'undefined' ? window.innerWidth < 768 : false, // 是否为移动端
    }),
    actions: {
        toggleSidebar() {
            this.showSidebar = !this.showSidebar;
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

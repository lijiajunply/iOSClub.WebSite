import {createApp} from 'vue'

import App from './App.vue'
import {createPinia} from 'pinia'
import router from './router'

// @ts-ignore
import 'vfonts/Lato.css'
// 等宽字体
// @ts-ignore
import 'vfonts/FiraCode.css'
// @ts-ignore
import './style.css'

const pinia = createPinia()

const app = createApp(App)

app.use(pinia)
app.use(router)

app.mount('#app')
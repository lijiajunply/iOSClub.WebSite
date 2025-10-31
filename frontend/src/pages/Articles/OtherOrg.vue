<script setup>
import { ref } from 'vue'
import {
  NImage,
  NImageGroup,
  NDivider,
  NAnchor,
  NAnchorLink,
  NAffix,
  NBreadcrumb,
  NBreadcrumbItem
} from 'naive-ui'
import xiaomiImg from '/assets/other/xiaomi.jpg'
import huaweiImg from '/assets/other/huawei.jpg'
import aircraftImg from '/assets/other/aircraft.jpg'
import tesheImg from '/assets/other/teshe.jpg'
import PageTitle from "../../components/PageTitle.vue"

const visible = ref(false)

// 组织数据
const organizations = [
  {
    id: 'xiaomi',
    title: '米粉俱乐部',
    image: xiaomiImg,
    description: [
      '欢迎来到西建大米粉俱乐部——一个充满创新和科技热情的社区！我们是由小米公司在西安建筑科技大学创办的科技类俱乐部。这里，我们共同探索科技的无限可能，分享智能生活的每一个精彩瞬间。',
      '作为米粉俱乐部的一员，你将有机会参与到最前沿的科技讨论，体验最新的小米产品，甚至参与产品测试和反馈。我们的活动涵盖了科技讲座、工作坊、创新竞赛等多种形式，旨在激发每一位成员的创造力和潜能。',
      '我们诚挚邀请每一位对科技充满热情的朋友加入我们的大家庭。无论你是科技爱好者，还是寻求创新灵感的学者，西建大米粉俱乐部都是你理想的归宿。在这里，你可以与志同道合的伙伴交流思想，共同成长。',
      '加入我们，一起在科技的道路上，启航新征程！我们期待着你的加入，一起创造属于我们的未来。立即联系我们，开启你的米粉之旅吧。'
    ]
  },
  {
    id: 'huawei',
    title: '花粉俱乐部',
    image: huaweiImg,
    description: [
      '欢迎来到西建大花粉俱乐部! 我们是由华为公司在西安建筑科技大学创办的科技类俱乐部。这里，我们共同探索科技的无限可能，分享智能生活的每一个精彩瞬间。',
      '我们诚挚邀请每一位对科技充满热情的朋友加入我们的大家庭。无论你是科技爱好者，还是寻求创新灵感的学者，西建大花粉俱乐部都是你理想的归宿。在这里，你可以与志同道合的伙伴交流思想，共同成长。',
      '加入我们，一起在科技的道路上，启航新征程！我们期待着你的加入，一起创造属于我们的未来。立即联系我们，开启你的花粉之旅吧。'
    ]
  },
  {
    id: 'aircraft',
    title: '航模社',
    image: aircraftImg,
    description: [
      '航模社是一个充满激情与创新精神的学生社团，致力于航空模型和无人机的设计、制作与飞行。航模社不仅为学生提供了一个展示自己技术和创意的平台，还培养了团队合作和解决问题的能力。',
      '如果你对航空模型感兴趣，欢迎加入西安建筑科技大学航模社。无论你是新手还是有经验的模型爱好者，这里都有适合你的活动和项目。让我们一起在蓝天中翱翔，探索航空科技的无限可能！'
    ]
  }
]

// 校外组织
const offCampusOrgs = [
  { name: '西建大仙建协会【MC社】', url: 'https://skin.xauatcraft.com/' },
  { name: '西安邮电大学Linux兴趣小组', url: 'https://www.xiyoulinux.com/' },
  { name: '西安邮电大学MC兴趣交流团体【邮立方】', url: 'https://cop.cooo.site/' }
]
</script>

<template>
  <div class="min-h-screen  transition-colors duration-500">
    <!-- 页面标题 -->
    <div class="container mx-auto px-4 py-8">
      <div class="flex flex-col lg:flex-row gap-8">
        <!-- 主内容区 -->
        <div class="w-full">
          <!-- 介绍部分 -->
          <section id="other" class="mb-12 bg-white dark:bg-zinc-800 p-6 rounded-xl shadow-sm">
            <h2 class="text-2xl font-semibold text-zinc-800 dark:text-white mb-4">其他组织</h2>
            <p class="text-zinc-600 dark:text-zinc-300 leading-relaxed">
              iOS Club除了iOS Club，还许多分社团和挂靠组织。同时也和校内外许多社团和组织有许多交流。
            </p>
          </section>
          
          <!-- 组织列表 -->
          <div class="space-y-8">
            <template v-for="org in organizations" :key="org.id">
              <section :id="org.id" class="bg-white dark:bg-zinc-800 rounded-xl overflow-hidden shadow-sm transition-all duration-300 hover:shadow-md">
                <div class="flex flex-col md:flex-row">
                  <!-- 图片部分 -->
                  <div class="md:w-1/3 p-6 flex items-center justify-center">
                    <NImage
                      class="w-full max-w-[200px] rounded-lg shadow-md"
                      :src="org.image"
                      :preview-disabled="false"
                      @click="visible = true"
                      object-fit="cover"
                      height="200"
                    />
                  </div>
                  
                  <!-- 内容部分 -->
                  <div class="md:w-2/3 p-6">
                    <h2 class="text-xl font-semibold text-zinc-800 dark:text-white mb-4">{{ org.title }}</h2>
                    <div class="space-y-3">
                      <p v-for="(paragraph, index) in org.description" :key="index" class="text-zinc-600 dark:text-zinc-300 leading-relaxed">
                        {{ paragraph }}
                      </p>
                    </div>
                  </div>
                </div>
              </section>
              
              <!-- 分隔线 -->
              <NDivider v-if="org.id !== organizations[organizations.length - 1]" class="my-8" />
            </template>
          </div>
          
          <!-- 校外组织 -->
          <section id="off-campus" class="mt-12 bg-white dark:bg-zinc-800 p-6 rounded-xl shadow-sm">
            <h3 class="text-xl font-semibold text-zinc-800 dark:text-white mb-4">校内外兄弟组织</h3>
            <div class="space-y-3">
              <a 
                v-for="(org, index) in offCampusOrgs" 
                :key="index"
                :href="org.url" 
                target="_blank"
                class="block p-3 rounded-lg bg-zinc-50 dark:bg-zinc-700 hover:bg-zinc-100 dark:hover:bg-zinc-600 transition-colors flex items-center justify-between"
              >
                <span class="text-zinc-700 dark:text-zinc-200">{{ org.name }}</span>
                <svg class="w-5 h-5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"></path>
                </svg>
              </a>
            </div>
          </section>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 苹果风格的过渡动画 */
section {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

section:hover {
  transform: translateY(-2px);
}

/* 链接样式 */
a {
  transition: all 0.2s ease;
}

/* 图片样式 */
.n-image {
  transition: transform 0.3s ease, filter 0.3s ease;
}

.n-image:hover {
  transform: scale(1.02);
  filter: brightness(1.05);
}

/* 响应式调整 */
@media (max-width: 768px) {
  .container {
    padding: 16px;
  }
  
  section {
    padding: 16px;
  }
  
  h2 {
    font-size: 1.5rem;
  }
}

/* 平滑滚动 */
html {
  scroll-behavior: smooth;
}
</style>
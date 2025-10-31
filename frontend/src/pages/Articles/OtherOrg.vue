<script setup>
import { ref } from 'vue'
import {
  NGrid,
  NGridItem,
  NImage,
  NImageGroup,
  NDivider,
  NAnchor,
  NAnchorLink,
  NAffix,
} from 'naive-ui'
import xiaomiImg from '../../../public/assets/other/xiaomi.jpg'
import huaweiImg from '../../../public/assets/other/huawei.jpg'
import aircraftImg from '../../../public/assets/other/aircraft.jpg'
import tesheImg from '../../../public/assets/other/teshe.jpg'
const visible = ref(false)
</script>


<template>
  <div class="min-h-screen bg-gray-50 dark:bg-neutral-900 transition-colors duration-300">
    <PageTitle class="text-gray-900 dark:text-gray-100">其他组织 - 西建大iOS Club</PageTitle>
    <n-breadcrumb style="margin-bottom: 10px" class="desktop-phone">
      <n-breadcrumb-item>
        <router-link to="/About">社团简介</router-link>
      </n-breadcrumb-item>
      <n-breadcrumb-item>
        <router-link to="/OtherOrg">其他组织</router-link>
      </n-breadcrumb-item>
    </n-breadcrumb>
    <n-grid x-gap="16" y-gap="16" cols="24">
      <n-grid-item span="20">
        <section id="other" class="mb-10">
          <h2 class="text-2xl font-bold mb-4 text-gray-800 dark:text-gray-100">其他组织</h2>
          <p class="mb-4 leading-8 indent-8 text-gray-700 dark:text-gray-300">
            iOS Club除了iOS Club，还许多分社团和挂靠组织。同时也和校内外许多社团和组织有许多交流。
          </p>
        </section>

        <section id="xiaomi" style="margin-top: 32px;">
          <n-grid x-gap="24" cols="24">
            <n-grid-item span="16">
              <h2>米粉俱乐部</h2>
              <p>欢迎来到西建大米粉俱乐部——一个充满创新和科技热情的社区！我们是由小米公司在西安建筑科技大学创办的科技类俱乐部。这里，我们共同探索科技的无限可能，分享智能生活的每一个精彩瞬间。</p>
              <p>作为米粉俱乐部的一员，你将有机会参与到最前沿的科技讨论，体验最新的小米产品，甚至参与产品测试和反馈。我们的活动涵盖了科技讲座、工作坊、创新竞赛等多种形式，旨在激发每一位成员的创造力和潜能。</p>
              <p>我们诚挚邀请每一位对科技充满热情的朋友加入我们的大家庭。无论你是科技爱好者，还是寻求创新灵感的学者，西建大米粉俱乐部都是你理想的归宿。在这里，你可以与志同道合的伙伴交流思想，共同成长。</p>
              <p>加入我们，一起在科技的道路上，启航新征程！我们期待着你的加入，一起创造属于我们的未来。立即联系我们，开启你的米粉之旅吧</p>
            </n-grid-item>
            <n-grid-item span="8" class="flex items-center justify-center">
                <n-image
                  width="200"
                  :preview-disabled="true"
                  :src="xiaomiImg"
                  @click="visible = true"
                  style="cursor:pointer"
                />
            </n-grid-item>
          </n-grid>
        </section>

        <n-divider />

        <section id="huawei">
          <n-grid x-gap="24" cols="24">
            <n-grid-item span="16">
              <h2>花粉俱乐部</h2>
              <p>欢迎来到西建大花粉俱乐部! 我们是由华为公司在西安建筑科技大学创办的科技类俱乐部。这里，我们共同探索科技的无限可能，分享智能生活的每一个精彩瞬间。</p>
              <p>我们诚挚邀请每一位对科技充满热情的朋友加入我们的大家庭。无论你是科技爱好者，还是寻求创新灵感的学者，西建大花粉俱乐部都是你理想的归宿。在这里，你可以与志同道合的伙伴交流思想，共同成长。</p>
              <p>加入我们，一起在科技的道路上，启航新征程！我们期待着你的加入，一起创造属于我们的未来。立即联系我们，开启你的花粉之旅吧</p>
            </n-grid-item>
            <n-grid-item span="8" class="flex items-center justify-center">
              <n-image
                width="200"
                :preview-disabled="true"
                :src="huaweiImg"
                @click="visible = true"
                style="cursor:pointer"
              />
            </n-grid-item>
          </n-grid>
        </section>

        <n-divider />

        <section id="aircraft">
          <n-grid x-gap="24" cols="24">
            <n-grid-item span="16">
              <h2>航模社</h2>
              <p>航模社是一个充满激情与创新精神的学生社团，致力于航空模型和无人机的设计、制作与飞行。航模社不仅为学生提供了一个展示自己技术和创意的平台，还培养了团队合作和解决问题的能力。</p>
              <p>加入我们</p>
              <p>如果你对航空模型感兴趣，欢迎加入西安建筑科技大学航模社。无论你是新手还是有经验的模型爱好者，这里都有适合你的活动和项目。让我们一起在蓝天中翱翔，探索航空科技的无限可能！</p>
            </n-grid-item>
            <n-grid-item span="8" class="flex items-center justify-center">
              <n-image
                width="200"
                :preview-disabled="true"
                :src="aircraftImg"
                @click="visible = true"
                style="cursor:pointer"
              />
            </n-grid-item>
          </n-grid>
        </section>

        <n-divider />

        <section id="SpecialEffects">
          <n-grid x-gap="24" cols="24">
            <n-grid-item span="16">
              <h2>仙建特摄同好群</h2>
              <p>🌟 特摄爱好者的集结号！</p>
              <p>我们是一个专注于特摄艺术的校园社团，隶属于iOS摄影部。特摄，即特殊摄影，是一种结合了模型、特效、动作捕捉等技术的电影拍摄手法，创造出令人惊叹的视觉效果。有名的特摄作品有假面骑士、超级战队、奥特曼以及各类型的特效变身类作品</p>
              <p>📱 iOS Club 附属社团 📱</p>
              <p>作为iOS摄影部的附属社团，我们不仅追求摄影的纯粹艺术，更致力于探索特摄的无限可能。在这里，你可以学习到如何将现实与想象完美结合，创造出属于自己的奇幻世界。</p>
              <p>🎥 特摄的魅力 🎥</p>
              <p>特摄不仅仅是电影制作的一部分，它是一种文化，一种艺术形式，更是一种对生活的热爱和对梦想的追求。我们相信，每个人都能通过特摄表达自己独特的视角和创意。</p>
              <p>🔗 加入我们 🔗</p>
              <p>无论你是特摄的狂热粉丝，还是对特效摄影充满好奇的新手，这里都是你展示才华、交流心得、共同进步的平台。加入我们，一起探索特摄的奥秘，用你充满童心的热情和真挚的热爱来推动社团的发展。</p>
            </n-grid-item>
            <n-grid-item span="8" class="flex items-center justify-center">
              <n-image
                width="200"
                :preview-disabled="true"
                :src="tesheImg"
                @click="visible = true"
                style="cursor:pointer"
              />
            </n-grid-item>
          </n-grid>
        </section>

        <n-divider />

        <section id="off-campus">
          <h4>校内外兄弟组织</h4>
          <ul style="font-size: 16px;">
            <li style="margin-bottom: 10px">
              <a href="https://skin.xauatcraft.com/" target="_blank">西建大仙建协会【MC社】</a>
            </li>
            <li style="margin-bottom: 10px">
              <a href="https://www.xiyoulinux.com/" target="_blank">西安邮电大学Linux兴趣小组</a>
            </li>
            <li style="margin-bottom: 10px">
              <a href="https://cop.cooo.site/" target="_blank">西安邮电大学MC兴趣交流团体【邮立方】</a>
            </li>
          </ul>
        </section>
      </n-grid-item>

      <n-grid-item span="4" class="desktop-phone">
        <n-affix>
          <n-anchor>
            <n-anchor-link href="#other" title="其他组织" />
            <n-anchor-link href="#xiaomi" title="米粉俱乐部" />
            <n-anchor-link href="#huawei" title="花粉俱乐部" />
            <n-anchor-link href="#aircraft" title="航模社" />
            <n-anchor-link href="#SpecialEffects" title="仙建特摄同好群" />
            <n-anchor-link href="#off-campus" title="校外组织" />
          </n-anchor>
        </n-affix>
      </n-grid-item>
    </n-grid>

    <n-image-group v-model:show="visible" style="display:none;">
      <n-image :src="xiaomiImg" />
      <n-image :src="huaweiImg" />
      <n-image :src="aircraftImg" />
      <n-image :src="tesheImg" />
    </n-image-group>
  </div>
</template>

<style scoped>
section {
  margin-bottom: 36px;
}
section h2 {
  font-size: 1.6rem;
  font-weight: 700;
  margin-bottom: 12px;
  color: var(--n-text-color, #222);
}
section p {
  margin: 10px 0 10px 0;
  line-height: 1.9;
  text-indent: 2em;
  font-size: 1.08rem;
}
section ul {
  margin: 12px 0 0 0;
  padding-left: 1.5em;
}
section li {
  margin-bottom: 10px;
  line-height: 1.7;
}
.desktop-phone {
  margin-bottom: 18px;
}
.n-divider {
  margin: 32px 0 32px 0;
}
.n-image {
  border-radius: 10px;
  box-shadow: 0 2px 12px 0 rgba(0,0,0,0.08);
}
@media (max-width: 900px) {
  section h2 {
    font-size: 1.2rem;
  }
  section p {
    font-size: 1rem;
  }
}
</style>
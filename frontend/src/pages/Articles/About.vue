<template>
  <div>
    <MarkdownComponent v-if="formattedArticle.content" :content="formattedArticle"/>
    <div v-else class="about-us p-4 md:p-8">
      <h1 class="about-title"><strong>关于我们</strong></h1>

      <section class="intro">
        <p>西建大iOS众创空间俱乐部（别称为西建大iOS
          Club），是受信息与控制工程学院指导的创新创业类社团。成立于2019年9月。目前是全校较大和较为知名的科技类社团。</p>
        <p>
          西建大iOS众创空间俱乐部没有设备要求，或者说没有任何限制——只要你喜欢数码，热爱编程，或者想要学习编程开发搞项目，就可以加入到西建大iOS众创空间俱乐部。</p>
      </section>

      <section class="mission">
        <h2>我们的使命是：</h2>
        <ul>
          <li>助力高校创新创业项目，搭建学生创新创业平台，丰富教学内容，深化校企合作发展。</li>
          <li>培养俱乐部成员的创新创业意识及软件开发、项目编程能力，提升学生自我竞争力，为毕业后服务社会奠定良好基础。</li>
          <li>探讨学科前沿，提升学校在相关学科的技术开发水平和业界影响力。</li>
          <li>帮助扶持创业团队，引入社会项目进入高校，同时助力学生的创新成果转化为社会有效财富。</li>
        </ul>
      </section>

      <section class="business">
        <h2>社团业务</h2>
        <p>作为学术性社团，我们担负着学校创新创业项目深化、发展的重担。当然要好好学习了，参加竞赛啦！</p>
        <p>其中主要参加的两项比赛分别是：</p>
        <ul>
          <li>
            <strong>WWDC Scholarship（苹果全球开发者大会学生奖学金）</strong>
            <p>
              WWDC是全球iOS开发者的盛会，苹果会在大会上公布新的软、硬件，同时在大会上设立奖学金，每年通过选拔在全球范围内遴选350名优秀开发者参加盛会，Apple公司提供WWDC的门票以及来回机票和住宿。</p>
          </li>
          <li>
            <strong>C4（全国高校计算机大赛移动应用创新赛）</strong>
            <p>C4 是国家级高校计算机能力应用的大赛，其中大赛主要分为四个模块，苹果公司赞助了移动应用创新赛这一模块。</p>
          </li>
        </ul>
      </section>

      <section class="activities">
        <h2>社团活动</h2>
        <ul>
          <li>每周一次免费iOS开发基础知识培训（由Apple专门讲师进行）</li>
          <li>每周两次免费基础设备使用（Mac、MacBook、iPad、iPhone）</li>
          <li>有机会免费参加Apple组织的青少年开发者冬、夏令营培训。</li>
          <li>定期举行的技术讲座以及区域性学生开发论坛活动。</li>
          <li>当然，你也可以结交到很多优秀的学长学姐，以及你的开发者同好们，组建一个快乐温馨的大家庭！</li>
        </ul>
      </section>

      <section class="media">
        <h2>社团媒体</h2>
        <ul>
          <li>Gitee 组织：<a href="https://gitee.com/XAUATiOSClub" target="_blank"
                            rel="noopener">西安建筑科技大学iOS众创空间官网</a></li>
          <li>Github 组织：<a href="https://github.com/XAUATiOSClub" target="_blank"
                             rel="noopener">西安建筑科技大学iOS众创空间官网</a></li>
          <li>B站：<a href="https://space.bilibili.com/1283819651?spm_id_from=333.337.search-card.all.click"
                     target="_blank" rel="noopener">西建大iOSClub</a></li>
          <li>知乎：<a href="https://www.zhihu.com/people/83-49-80-67" target="_blank" rel="noopener">西建大iOS Club</a>
          </li>
          <li>掘金：<a href="https://juejin.cn/user/939445846031627" target="_blank"
                      rel="noopener">西安建筑科技大学iOS众创空间俱乐部</a></li>
          <li>抖音：<a
              href="https://www.douyin.com/user/MS4wLjABAAAA042crZROwmNV69KtvhQSGtyLEZZATt4z1ewoXk-dLxCmt2S_DNus5c2KP_5S2CTY?_sw=4167573395234171&enter_from=general_search&enter_method=general_search&extra_params=%7B%22search_params%22%3A%7B%22search_result_id%22%3A%221612337042431609%22%2C%22relation_tag%22%3A0%2C%22log_pb%22%3A%7B%22impr_id%22%3A%2220241107211419AA5B58FD61D13326A614%22%7D%2C%22search_type%22%3A%22general%22%2C%22impr_id%22%3A%2220241107211419AA5B58FD61D13326A614%22%2C%22search_id%22%3A%2220241107211419AA5B58FD61D13326A614%22%2C%22search_keyword%22%3A%22%20%E8%A5%BF%E5%BB%BA%E5%A4%A7iOS%20Club%22%2C%22token_type%22%3A%22discover_list%22%7D%7D&from_tab_name=main"
              target="_blank" rel="noopener">西建大iOS Club</a></li>
          <li>微信公众号：微信搜索 西建大iOS众创空间俱乐部</li>
          <li>欢迎给我们写邮箱意见！邮箱号：<a href="mailto:iosclubxauat@163.com">iosclubxauat@163.com</a></li>
        </ul>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from 'vue'
import MarkdownComponent from "../../components/MarkdownComponent.vue";
import {ArticleService} from "../../services/ArticleService";
import {ArticleModel} from "../../models";

const roomArticle = ref<ArticleModel>({
  path: "",
  title: '',
  lastWriteTime: '',
  watch: 0,
  content: ''
})

const formattedArticle = computed(() => {
  return {
    title: roomArticle.value.title,
    date: roomArticle.value.lastWriteTime,
    watch: roomArticle.value.watch || 0,
    content: roomArticle.value.content
  };
});

onMounted(async () => {
  roomArticle.value = await ArticleService.getArticle('About')
})
</script>

<style scoped>

/* 卡片样式 */
.about-us {
  background: var(--card-bg, #fff);
  color: var(--text-color, #111827);
  padding: ;
}

/* 标题/段落/列表风格 */
.about-title {
  font-size: 1.75rem;
  margin-bottom: 1rem;
  color: var(--title-color, #111827);
  font-weight: 700;
}

.about-us h2 {
  font-size: 1.125rem;
  margin-top: 1.25rem;
  margin-bottom: 0.75rem;
  color: var(--title-color, #111827);
}

.about-us p {
  line-height: 1.75;
  margin-bottom: 0.9rem;
  color: inherit;
}

.about-us ul {
  margin-left: 1.25rem;
  margin-bottom: 0.9rem;
  list-style: disc;
}

.about-us li p {
  margin: 0.5rem 0 0 0;
}

/* 链接优先可见，明暗模式都适配 */
.about-us a {
  color: #2563eb;
  text-decoration: underline;
  position: relative;
  z-index: 10;
  word-break: break-word;
}

:deep(.dark) .about-us a {
  color: #7dd3fc;
}

/* 暗夜模式支持 */
:deep(.dark) .about-us {
  background: #0f1720;
  color: #e6eef6;
  box-shadow: none;
}

/* 响应式 */
@media (max-width: 768px) {
  .about-us {
    padding: 18px;
  }

  .about-title {
    font-size: 1.5rem;
  }
}
</style>
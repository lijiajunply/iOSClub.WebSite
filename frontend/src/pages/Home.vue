<template>
  <!-- 隐藏的音频播放器 -->
  <audio
      ref="audioPlayer"
      class="hidden"
      @ended="onAudioEnded"
  >
    <source src="../assets/other/纳新录音.m4a" type="audio/mpeg"/>
  </audio>

  <div @wheel="handleFirstWheel" class="">
    <div class="md:min-h-screen">
      <n-grid x-gap="12" cols="8" class="p-10" item-responsive>
        <n-gi span="8 700:3" class="flex items-center justify-center">
          <div class="w-3/4 text-center">
            <img
                alt="Logo"
                src="../assets/iOS_Club_LOGO.png"
                class="w-full"
            />
            <div class="hidden lg:block">
              <n-progress
                  :percentage="percent"
                  :show-indicator="false"
                  class="my-2.5"
              />
              <n-space justify="center">
                <n-button text @click="previousLyric">
                  <n-icon size="32" :component="CaretBack"/>
                </n-button>
                <n-button text @click="togglePlay">
                  <n-icon size="32" :component="isPlaying ? PauseCircleOutline : PlayCircleOutline"/>
                </n-button>
                <n-button text @click="nextLyric">
                  <n-icon size="32" :component="CaretForward"/>
                </n-button>
              </n-space>
            </div>
          </div>
        </n-gi>

        <n-gi span="8 700:5" class="flex flex-col justify-center">
          <div class="gradient-text text-4xl lg:text-5xl font-bold text-center lg:text-left mb-4 pt-2">
            iOS Club of XAUAT
          </div>
          <div class="block lg:hidden">
            <h3 class="text-xl text-center text-gray-700">
              一个跨专业与课堂的数码开发爱好者社团
            </h3>
            <p class="text-lg text-center text-gray-500 mt-2">
              "Stay hungry, stay foolish"
            </p>
          </div>
          <div
              class="text-2xl lg:text-4xl font-bold text-center lg:text-left text-gray-700 mt-4 lg:mt-0 hidden md:block">
            西安建筑科技大学iOS众创空间俱乐部
          </div>
          <div class="hidden lg:block mt-8">
            <div
                v-for="(lyric, index) in lyrics[lyricIndex]"
                :key="index"
                :class="[
                'lyric-item',
                isLyricActive(index) ? 'lyric-active' : ''
              ]"
            >
              {{ lyric }}
            </div>
          </div>
        </n-gi>
      </n-grid>
    </div>

    <div
        :class="[
          'transition-opacity duration-300',
          'block min-h-screen',
          isMobile || percent >= 70 ? 'opacity-100' : ''
        ]"
        :style="{ opacity: !isMobile ? (percent - 30) / 30 : 1 }"
    >
      <div class="text-3xl font-bold text-center mb-8 hidden md:block">关于我们</div>
      <br/>
      <br/>
      <n-grid x-gap="20" y-gap="24" cols="3" class="px-10 pb-10" item-responsive>
        <n-gi
            v-for="(card, index) in cards"
            :key="index"
            span="3 800:1"
        >
          <a
              :href="card.url"
              target="_blank"
              class="block"
          >
            <div class="card-hover h-52 bg-gray-100">
              <div class="text-2xl mb-3">{{ card.icon }}</div>
              <h3 class="text-lg font-semibold mb-2 text-gray-800">{{ card.title }}</h3>
              <p class="text-gray-600">{{ card.content }}</p>
            </div>
          </a>
        </n-gi>
      </n-grid>
    </div>
  </div>
</template>

<script setup>
import {ref, onMounted, onUnmounted} from 'vue'
import {NGrid, NGi, NProgress, NSpace, NButton, NIcon} from 'naive-ui'

import {
  PlayCircleOutline,
  PauseCircleOutline,
  CaretBack,
  CaretForward
} from '@vicons/ionicons5'

// 状态管理
const percent = ref(30)
const lyricIndex = ref(0)
const isPlaying = ref(false)
const isMobile = ref(false)
let audioPlayer = null

// 歌词数据
const lyrics = [
  [
    "这里是",
    "梦想家们改变世界的起点",
    "一个跨专业与课堂的数码开发爱好者社团",
    "跨越学科，体验编程与开发的魅力",
    "创造人生，开发非同凡响的APP"
  ],
  [
    "探讨学科前沿",
    "帮助扶持创业团队",
    "助力高校创新创业项目，搭建学生创新创业平台",
    "培养俱乐部成员的创新创业意识及软件开发能力",
    "丰富教学内容，深化校企合作发展"
  ]
]

// 卡片数据
const cards = [
  {
    icon: "🍎",
    title: "我们是谁?",
    content: "我们是由Apple公司资金支持，受学管和西安建筑科技大学大学生创新创业实践中心指导的创新创业类社团。",
    url: "/About"
  },
  {
    icon: "🤝",
    title: "和iOS Club一起结伴同行",
    content: "不管是零基础的小白还是大神，只要你有兴趣，这里就是你的天堂",
    url: "/Blog"
  },
  {
    icon: "🌐",
    title: "iOS Club,不止iOS",
    content: "我们不止只有iOS，西建大iOS Club是一个跨专业与课堂的数码编程爱好者社团",
    url: "/OtherOrg"
  },
  {
    icon: "😀",
    title: "Apple ✖️ 西建大",
    content: "苹果公司每个学期都会在学校举办各种活动，俱乐部成员可以参与合作软件开发，共同创造世界",
    url: "/Event"
  },
  {
    icon: "👩🏻‍💻",
    title: "合作软件开发",
    content: "加入我们，和志同道合的iMember一起合作开发，一起创造世界",
    url: "https://gitee.com/XAUATiOSClub"
  },
  {
    icon: "📈",
    title: "全校数码编程爱好者,联合起来！",
    content: "我们意图打造全校最大的科技社区，快来加入我们成为一名iMember吧",
    url: "/Login"
  }
]

// 方法
const handleFirstWheel = (event) => {
  if (isMobile.value ||
      (Math.abs(percent.value - 100) < 0.01 && event.deltaY > 0) ||
      (Math.abs(percent.value - 30) < 0.01 && event.deltaY < 0)) {
    return
  }

  percent.value += event.deltaY * 0.1
  if (percent.value < 30) percent.value = 30
  if (percent.value > 100) percent.value = 100
}

const nextLyric = () => {
  lyricIndex.value++
  if (lyricIndex.value >= lyrics.length) {
    lyricIndex.value = 0
  }
}

const previousLyric = () => {
  lyricIndex.value--
  if (lyricIndex.value < 0) {
    lyricIndex.value = lyrics.length - 1
  }
}

const togglePlay = async () => {
  isPlaying.value = !isPlaying.value
  if (isPlaying.value) {
    await audioPlayer?.play()
  } else {
    audioPlayer?.pause()
  }
}

const onAudioEnded = () => {
  audioPlayer?.play() // 自动重播
}

const isLyricActive = (index) => {
  return Math.abs(index * 10 + 30 - percent.value + 10) < 5
}

// 响应式处理
const handleResize = () => {
  isMobile.value = window.innerWidth <= 768
}

onMounted(() => {
  handleResize()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
@reference 'tailwindcss';

.gradient-text {
  background: -webkit-linear-gradient(-64deg, #f9bf65, #ffab6b, #ff9977, #fc8986, #ef7e95, #e47da6, #d37fb5, #bf83c1, #ab8dcf, #9597d8, #7fa0dc, #6ca7da);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.lyric-item {
  @apply w-full rounded-lg text-xl font-normal transition-all duration-200 p-2 mb-1;
}

.lyric-item:hover {
  @apply scale-[1.02] bg-gray-100 font-bold pl-4;
}

.lyric-active {
  @apply scale-[1.02] bg-gray-100 font-bold px-4 py-2;
}

.card-hover {
  @apply transition-transform duration-200 p-6 rounded-lg;
}

.card-hover:hover {
  @apply transform scale-[1.02];
}
</style>
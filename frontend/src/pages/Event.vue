<template>
  <div class="event-page">
    <h1 class="page-title">社团活动 - 西建大iOS Club</h1>

    <div class="header-section text-center">
      <img src="../assets/Centre/AppleLogo.jpg" alt="iOS Club Logo" class="logo-image mx-auto" />
      <div class="flex justify-center items-center">
        <div class="gradient-text">iOS Club 社团活动</div>
      </div>
      <p class="secondary-text font-22">Think Different</p>
    </div>

    <div class="flex justify-center items-center">
      <div class="block grid grid-cols-1 md:grid-cols-2 gap-6 w-4/5 my-11">
        <div
            v-for="(item, index) in cards"
            :key="index"
            class="col-span-1"
        >
          <a :href="item.url" target="_blank">
            <div class="card hover:shadow-lg transition-shadow duration-300 rounded-lg bg-white p-6 h-52 flex flex-col items-center text-center">
              <img :src="item.imageUrl" alt="Event Icon" class="w-16 h-14 mb-3" />
              <h2 class="mt-1 mb-3 text-xl font-bold">
                {{ item.title }}
              </h2>
              <p class="text-gray-600">
                {{ item.content }}
              </p>
            </div>
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import visionProImage from '../assets/other/vision_pro.jpg';
import launchEventImage from '../assets/other/launch_event.jpg';
import classImage from '../assets/other/class.jpg';
import partyImage from '../assets/other/you_yuan_hui.jpg';

export default {
  name: 'EventPage',
  data() {
    return {
      cards: [
        {
          imageUrl: visionProImage,
          title: 'iOS Club和你一起体验最新产品',
          content: 'iOS Club与许多企业进行合作，我们将带您体验最新的设备与最新应用',
          // url: '/Article/VisionPro'
        },
        {
          imageUrl: launchEventImage,
          title: 'iOS Club和你一起看发布会',
          content: 'iOS Club和你一起见证未来。未来已来，你来不来？',
          // url: '/Article/PressConference'
        },
        {
          imageUrl: classImage,
          title: 'iOS Club和你一起夜校培训',
          content: 'iOS Club携手专业讲师，和你一起开启夜校培训，带你深入学习前沿知识',
          // url: '/Article/Class'
        },
        {
          imageUrl: partyImage,
          title: 'iOS Club和你一起进行游园会',
          content: 'iOS Club和你一起漫步校园，在游园会中打卡互动、畅享欢乐时光',
          // url: '/Article/Party'
        }
      ]
    }
  }
}
</script>

<style scoped>
.page-title {
  display: none;
}

.header-section {
  margin: 30px 0;
}

.logo-image {
  width: 40%;
  max-width: 300px;
}

.gradient-text {
  background: linear-gradient(90deg, #ff6b00, #ff0066);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  font-size: 36px;
  font-weight: bold;
  margin: 20px 0;
}

.secondary-text {
  color: #666;
}

.font-22 {
  font-size: 22px;
}

.card {
  margin: 10px;
  border-radius: 10px;
  background: #ffffff;
  transition: all 0.3s ease;
}

.card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0,0,0,0.1);
}
</style>    
---
title: 社团活动 - 西建大iOS Club
layout: page
---

  <div class="page-container">
    <!-- 顶部标题部分 -->
    <div class="hero-section">
      <img 
        src="/Centre/AppleLogo.jpg"
        class="logo"
        :style="{ width: '40%' }"
      />
      <div class="gradient-text">iOS Club 社团活动</div>
      <p class="subtitle">Think Different</p>
    </div>
    <!-- 卡片列表部分 -->
    <div class="cards-container">
      <div 
        v-for="card in cards" 
        :key="card.title"
        class="card-col"
      >
        <a :href="card.url" target="_blank">
          <div class="card">
            <img 
              :src="card.imageUrl" 
              width="65" 
              height="55" 
              :alt="card.title"
            />
            <h1 class="card-title">{{ card.title }}</h1>
            <p class="card-content">{{ card.content }}</p>
          </div>
        </a>
      </div>
    </div>
  </div>

<script setup>
import { ref } from 'vue'

const cards = ref([
  {
    imageUrl: "https://developer.apple.com/wwdc24/images/icons/glow/state_2x.png",
    title: "iOS Club和你一起体验最新产品",
    content: "iOS Club与许多企业进行合作，我们将带您体验最新的设备与最新应用",
    url: "/Article/VisionPro"
  },
  {
    imageUrl: "https://developer.apple.com/wwdc24/images/icons/glow/play_2x.png",
    title: "iOS Club和你一起看发布会",
    content: "iOS Club和你一起见证未来。未来已来，你来不来？",
    url: "/Article/PressConference"
  }
])
</script>

<style scoped>
.page-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.hero-section {
  text-align: center;
  padding: 40px 0;
}

.logo {
  width: 40%;
  max-width: 300px;
}

.gradient-text {
  font-size: 32px;
  font-weight: bold;
  margin: 20px 0;
  background: linear-gradient(to right, #007AFF, #5AD8A6);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.subtitle {
  font-size: 22px;
  color: #555;
  margin-bottom: 30px;
}

.cards-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  padding: 45px 0;
}

.card-col {
  padding: 10px;
}

.card {
  border-radius: 10px;
  background: #ffffff;
  padding: 20px;
  height: 200px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  transition: transform 0.2s ease;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.card:hover {
  transform: translateY(-5px);
}

.card img {
  display: block;
  margin: 0 auto;
}

.card-title {
  margin: 15px 0 10px;
  font-size: 18px;
  text-align: center;
}

.card-content {
  color: #666;
  text-align: center;
  font-size: 14px;
}
</style>
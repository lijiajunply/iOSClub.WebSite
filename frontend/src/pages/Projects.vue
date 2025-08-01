<template>
  <div class="project-page">
    <div class="header-section text-center">
      <img
          src="@/assets/Centre/AppleLogo.jpg"
          alt="iOS Club Logo"
          class="logo-image mx-auto"
      />
      <div class="flex justify-center items-center">
        <div class="gradient-text">iOS Club 社团项目</div>
      </div>
      <p class="secondary-text font-22">创造世界的新方式</p>
    </div>
    
    <div class="flex justify-center items-center">
      <div class="block grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 w-4/5 my-11">
        <div
            v-for="(item, index) in cards"
            :key="item.title"
        class="col-span-1"
        >
        <a :href="item.url" target="_blank">
          <div class="card hover:shadow-lg transition-all duration-300 rounded-lg bg-white p-6 h-52 flex flex-col">
            <h2 class="text-xl md:text-2xl font-bold text-gray-800 
                transition-colors 
                group-hover:text-transparent 
                group-hover:bg-clip-text 
                group-hover:bg-gradient-to-r 
                group-hover:from-pink-500 
                group-hover:to-teal-500"
            >
              {{ item.title }}
            </h2>
            <p class="mt-2 text-gray-600 line-clamp-3 transition-opacity group-hover:opacity-80">
              {{ item.content }}
            </p>
          </div>
        </a>
      </div>
    </div>
  </div>
  </div>
</template>

<script setup>
const cards = [
  {
    title: "Old8Lang",
    content: "西建大iOS Club出品的脚本语言解释器 使用C#开发",
    url: "https://gitee.com/XAUATiOSClub/Old8Lang"
  },
  {
    title: "西建大iOS Club官网",
    content: "西建大iOS Club官网，使用Blazor开发",
    url: "https://gitee.com/XAUATiOSClub/iOSClub.Web"
  },
  {
    title: "代码综合平台",
    content: "将在线编辑器，编译器，OJ系统结合起来的代码综合平台，使用FastApi+Vue开发",
    url: "https://gitee.com/XAUATiOSClub/LetCoding"
  },
  {
    title: "文档生成平台",
    content: "将各式各样的文档进行生成，使用WPF+Asp.net Webapi开发",
    url: "https://gitee.com/XAUATiOSClub/DocumentMaking"
  },
  {
    title: "滑稽账本",
    content: "用.NET MAUI开发的账本App",
    url: "https://gitee.com/XAUATiOSClub/huaji-ledger"
  },
  {
    title: "LuckyFish.Json",
    content: "用C#开发的json解析器",
    url: "https://gitee.com/XAUATiOSClub/LuckyFish.Json"
  },
  {
    title: "建大Wiki小程序",
    content: "采用 Uni - app 框架构建的Wiki小程序",
    url: "https://gitee.com/XAUATiOSClub/uniapp-ioswiki"
  },
  {
    title: "新生代培养计划",
    content: "基于前端技术栈的培训方案",
    url: "https://gitee.com/XAUATiOSClub/uniapp-ioswiki"
  },
  {
    title: "XAUAT 自动评教脚本",
    content: "基于 Tampermonkey 的用户脚本",
    url: "https://gitee.com/XAUATiOSClub/Xauat-automatic-teaching-evaluation"
  }
];
</script>

<style scoped>
.project-page {
  padding: 2rem 1rem;
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
  height: 200px; 
  border-radius: 10px;
  background: #ffffff;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06); 
  transition: all 0.3s ease; 
  will-change: transform, box-shadow;
  transform: translateZ(0); 
}

.card:hover {
  transform: translateY(-5px); 
  box-shadow: 0 10px 20px rgba(0,0,0,0.1); 
  border-color: #e0fbfc;
}


@media (min-width: 768px) {
  .grid-cols-2 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
@media (min-width: 1024px) {
  .grid-cols-3 {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}
</style>
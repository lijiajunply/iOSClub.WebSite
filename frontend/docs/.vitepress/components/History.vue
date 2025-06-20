<template>
    <div class="flex flex-col min-h-screen">
        <div class="text-center py-12">
            <!-- 历史图片 -->
            <img src="/Centre/History.jpg" alt="iOS Club历史" class="w-[25%] max-w-xl mx-auto" />
            <!-- 渐变标题 -->
            <h1 class="font-bold text-4xl mb-4 mt-4">
                <span class="text-clip text-transparent text-3xl md:text-4xl lg:text-5xl font-bold mb-4">
                    iOS Club 社团历史
                </span>
            </h1>
            <!-- 引用语 -->
            <p class="text-xl text-gray-500 font-medium">
                "历史是时代的见证"
            </p>
        </div>
        <!-- 历史卡片网格 -->
        <div class="flex justify-center items-center pb-16">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 w-[80%] max-w-6xl my-12">
                <a v-for="card in cards" :key="card.title" :href="card.url"
                    class="transition-transform duration-300 hover:scale-[1.02]">
                    <div class="card bg-white dark:bg-gray-800 rounded-xl shadow-md p-6">
                        <h2 class="text-xl font-bold text-gray-800 dark:text-gray-100 mb-4">{{ card.title }}</h2>
                        <p class="text-gray-500">{{ card.content }}</p>
                    </div>
                </a>
            </div>
        </div>
    </div>
</template>

<script setup>
const cards = [
    {
        title: "总述",
        content: "我们社团从何而来，又要到哪里去，这是一个值得思考的问题",
        url: "/article/History/Overview"
    },
    {
        title: "创社时代",
        content: "社团的开始总是有着一段传奇故事",
        url: "/article/History/Founding"
    },
    {
        title: "邵韩之治",
        content: "解决完社团的初步建设，接下来就要进行社团的发展了",
        url: "/article/History/Shao Han's Reign"
    }
];
</script>
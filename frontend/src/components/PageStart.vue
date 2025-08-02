<script setup lang="ts">
defineProps([
  'title',
  'subtitle',
  'img',
  'gradientClass'
])
</script>

<template>
  <div class="min-h-screen grid grid-cols-1 content-around gap-4">
    <div class="text-center mb-20">
      <!-- Logo 区域 -->
      <div class="mb-8 animate-fade-in">
        <img
            :src="img"
            alt="iOS Club Logo"
            class="w-48 h-48 mx-auto rounded-4xl shadow-xl hover:shadow-2xl transition-all duration-500 hover:scale-105"
        />
      </div>

      <!-- 标题区域 -->
      <div class="space-y-4 ">
        <h1 :class="['text-5xl md:text-6xl font-semibold',gradientClass,'bg-clip-text text-transparent animate-gradient']">
          {{ title }}
        </h1>
        <p class="text-xl text-gray-600 tracking-wide font-semibold">
          {{ subtitle }}
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.animate-fade-in {
  animation: fade-in 0.8s ease-out;
}

.animate-gradient {
  background-size: 200% 200%;
  animation: gradient 3s ease infinite;
}

@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slide-up {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes gradient {
  0%, 100% {
    background-position: 0 50%;
  }
  50% {
    background-position: 100% 50%;
  }
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px);
  }
  50% {
    transform: translateY(-20px);
  }
}
</style>
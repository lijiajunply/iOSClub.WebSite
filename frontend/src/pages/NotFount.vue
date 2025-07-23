<template>
  <n-layout>
    <n-layout-header>
    </n-layout-header>
    <n-layout-content>
      <n-space justify="center" align="center" class="h-screen">
        <div class="text-center">
          <n-result status="404" title="未能找到该页面"/>
          <br/>
          <n-button type="primary" @click="goHome">
            回到主页
          </n-button>
        </div>
      </n-space>
    </n-layout-content>
    <n-layout-footer>
    </n-layout-footer>
  </n-layout>
</template>

<script setup>
import {useRouter} from 'vue-router';
import {NSpace, NResult, NButton} from 'naive-ui'

const router = useRouter();
const goHome = () => {
  router.push('/');
};
</script>

<style scoped>

</style>
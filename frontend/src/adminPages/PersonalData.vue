<template>
  <div class="flex-1 flex flex-col min-h-screen bg-gray-50 dark:bg-black transition-colors duration-300">
    <!-- 顶部导航栏 -->
    <div class="sticky top-0 z-10 bg-white/90 dark:bg-gray-800/90 backdrop-blur-md border-b border-gray-200 dark:border-gray-800">
      <div class="px-4 py-3 flex justify-between items-center">
        <h1 class="text-xl font-semibold text-gray-900 dark:text-white">个人信息</h1>
        <div class="flex items-center gap-2">
          <div class="h-8 w-8 rounded-full bg-gradient-to-r from-blue-500 to-purple-500 flex items-center justify-center text-white">
            <Icon icon="lucide:user" size="18" />
          </div>
          <span class="text-sm font-medium text-gray-900 dark:text-white">{{ userInfo.userName || '未登录' }}</span>
        </div>
      </div>
    </div>

    <!-- 主内容区域 -->
    <main class="flex-1 overflow-y-auto p-4 md:p-6">
      <!-- 加载状态 -->
      <div v-if="loading" class="py-20 flex flex-col items-center justify-center">
        <div class="h-12 w-12 rounded-full border-4 border-blue-500 border-t-transparent animate-spin"></div>
        <p class="mt-4 text-gray-500 dark:text-gray-400">加载中...</p>
      </div>

      <!-- 表单卡片 -->
      <div v-else class="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden max-w-4xl mx-auto">
        <!-- 卡片头部 -->
        <div class="px-6 py-5 border-b border-gray-200 dark:border-gray-700">
          <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3">
            <h2 class="text-lg font-medium text-gray-900 dark:text-white">基本信息</h2>
            <div class="flex items-center">
              <span class="text-sm text-gray-500 dark:text-gray-400 mr-2">当前身份:</span>
              <span 
                class="px-3 py-1 text-xs rounded-full font-medium"
                :class="getIdentityBadgeClass(userInfo.identity)"
              >
                {{ identityMap[userInfo.identity] || userInfo.identity }}
              </span>
            </div>
          </div>
        </div>

        <!-- 表单内容 -->
        <div class="p-6">
          <n-form
            :model="userInfo"
            :rules="rules"
            ref="formRef"
            label-placement="top"
            require-mark-placement="right"
          >
            <!-- 表单行：姓名和性别 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <n-form-item label="姓名" path="userName" class="mb-0">
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="lucide:user" class="text-gray-400" size="18" />
                  </div>
                  <n-input 
                    v-model:value="userInfo.userName"
                    placeholder="请输入姓名"
                    class="pl-10 bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                  />
                </div>
              </n-form-item>
              
              <n-form-item label="性别" path="gender" class="mb-0">
                <div class="flex gap-4 mt-1">
                  <label 
                    v-for="gender in genderOptions" 
                    :key="gender"
                    class="flex items-center space-x-2 cursor-pointer"
                  >
                    <input 
                      type="radio" 
                      :value="gender" 
                      v-model="userInfo.gender"
                      class="peer h-4 w-4 text-blue-500 border-gray-300 focus:ring-blue-500 dark:border-gray-600"
                    />
                    <span class="text-sm text-gray-700 dark:text-gray-300 peer-checked:text-blue-500 transition-colors">{{ gender }}</span>
                  </label>
                </div>
              </n-form-item>
            </div>

            <!-- 表单行：学号和学院 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <n-form-item label="学号" path="userId" class="mb-0">
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="lucide:id-badge" class="text-gray-400" size="18" />
                  </div>
                  <n-input 
                    v-model:value="userInfo.userId"
                    placeholder="请输入10位学号"
                    class="pl-10 bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                  />
                </div>
              </n-form-item>
              
              <n-form-item label="学院" path="academy" class="mb-0">
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="lucide:building" class="text-gray-400" size="18" />
                  </div>
                  <n-select 
                    v-model:value="userInfo.academy"
                    :options="academyOptions"
                    placeholder="请选择学院"
                    filterable
                    class="pl-10 bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                  />
                </div>
              </n-form-item>
            </div>

            <!-- 表单行：政治面貌和专业班级 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <n-form-item label="政治面貌" path="politicalLandscape" class="mb-0">
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="lucide:flag" class="text-gray-400" size="18" />
                  </div>
                  <n-select 
                    v-model:value="userInfo.politicalLandscape"
                    :options="politicalLandscapeOptions"
                    placeholder="请选择政治面貌"
                    class="pl-10 bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                  />
                </div>
              </n-form-item>
              
              <n-form-item label="专业班级" path="className" class="mb-0">
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="lucide:users" class="text-gray-400" size="18" />
                  </div>
                  <n-input 
                    v-model:value="userInfo.className"
                    placeholder="请输入专业班级"
                    class="pl-10 bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                  />
                </div>
              </n-form-item>
            </div>

            <!-- 表单行：手机号 -->
            <div class="mb-6">
              <n-form-item label="手机号" path="phoneNum" class="mb-0">
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="lucide:phone" class="text-gray-400" size="18" />
                  </div>
                  <n-input 
                    v-model:value="userInfo.phoneNum"
                    placeholder="请输入手机号"
                    class="pl-10 bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                  />
                </div>
              </n-form-item>
            </div>

            <!-- 操作按钮 -->
            <div class="flex justify-center pt-4">
              <n-button
                type="primary"
                @click="handleSubmit"
                :loading="confirmLoading"
                size="large"
                class="w-full md:w-auto px-8 py-2.5 bg-blue-500 hover:bg-blue-600 text-white font-medium rounded-xl shadow-sm transition-all"
              >
                <template #icon>
                  <Icon icon="lucide:save" size="18" />
                </template>
                保存修改
              </n-button>
            </div>
          </n-form>
        </div>
      </div>

      <!-- 数据统计卡片 -->
      <div class="mt-8 grid grid-cols-1 md:grid-cols-3 gap-4 max-w-4xl mx-auto">
        <div class="bg-white dark:bg-gray-800 rounded-xl p-4 border border-gray-200 dark:border-gray-700 shadow-sm">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400">成员类型</h3>
            <div class="h-8 w-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
              <Icon icon="lucide:user-check" class="text-blue-500" size="16" />
            </div>
          </div>
          <p class="text-2xl font-semibold text-gray-900 dark:text-white">{{ identityMap[userInfo.identity] || '未设置' }}</p>
        </div>
        <div class="bg-white dark:bg-gray-800 rounded-xl p-4 border border-gray-200 dark:border-gray-700 shadow-sm">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400">学院</h3>
            <div class="h-8 w-8 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
              <Icon icon="lucide:building-2" class="text-green-500" size="16" />
            </div>
          </div>
          <p class="text-2xl font-semibold text-gray-900 dark:text-white truncate">{{ userInfo.academy || '未设置' }}</p>
        </div>
        <div class="bg-white dark:bg-gray-800 rounded-xl p-4 border border-gray-200 dark:border-gray-700 shadow-sm">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-medium text-gray-500 dark:text-gray-400">班级</h3>
            <div class="h-8 w-8 rounded-full bg-purple-100 dark:bg-purple-900/30 flex items-center justify-center">
              <Icon icon="lucide:users-2" class="text-purple-500" size="16" />
            </div>
          </div>
          <p class="text-2xl font-semibold text-gray-900 dark:text-white truncate">{{ userInfo.className || '未设置' }}</p>
        </div>
      </div>

      <!-- 图表区域 -->
      <div class="mt-8 bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden max-w-4xl mx-auto">
        <div class="px-6 py-5 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-lg font-medium text-gray-900 dark:text-white">个人数据统计</h2>
        </div>
        <div class="p-6">
          <div ref="chartRef" class="h-64 w-full"></div>
        </div>
      </div>
    </main>

    <!-- 确认对话框 -->
    <n-modal
      v-model:show="showModal"
      preset="dialog"
      title="确认保存"
      content="确定要保存修改吗？"
      positive-text="确认"
      negative-text="取消"
      :loading="confirmLoading"
      @positive-click="handleConfirm"
      class="rounded-xl"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, watch } from 'vue';
import { NForm, NFormItem, NInput, NSelect, NButton, NModal, useMessage } from 'naive-ui';
import { useRouter } from 'vue-router';
import { useAuthorizationStore } from '../stores/Authorization';
import { UserService } from '../services/UserService';
import { MemberModel } from '../models';
import { Icon } from '@iconify/vue';
import * as echarts from 'echarts';

const message = useMessage();
const router = useRouter();
const authorizationStore = useAuthorizationStore();
const chartRef = ref<HTMLElement>();
let chartInstance: echarts.ECharts | null = null;

// 表单引用
const formRef = ref();

// 加载状态
const loading = ref(false);
const confirmLoading = ref(false);
const showModal = ref(false);

// 身份映射
const identityMap: Record<string, string> = {
  'Founder': '创始人',
  'President': '社长/团支书',
  'Minister': '部长/副部长',
  'Department': '部员',
  'Member': '普通成员'
};

// 根据身份获取徽章样式
const getIdentityBadgeClass = (identity: string) => {
  switch (identity) {
    case 'Founder':
      return 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400';
    case 'President':
      return 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400';
    case 'Minister':
      return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
    case 'Department':
      return 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400';
    case 'Member':
      return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
    default:
      return 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300';
  }
};

// 性别选项
const genderOptions = ['男', '女'];

// 政治面貌选项
const politicalLandscapeOptions = [
  '群众',
  '共青团员',
  '中共党员',
  '中共预备党员'
].map(item => ({ label: item, value: item }));

// 学院选项
const academyOptions = [
  '信息与控制工程学院',
  '理学院',
  '机电工程学院',
  '管理学院',
  '土木工程学院',
  '环境与市政工程学院',
  '建筑设备科学与工程学院',
  '材料科学与工程学院',
  '冶金工程学院',
  '资源工程学院',
  '城市发展与现代交通学院',
  '文学院',
  '艺术学院',
  '建筑学院',
  '马克思主义学院',
  '公共管理学院',
  '化学与化工学院',
  '体育学院',
  '安德学院',
  '未来技术学院',
  '国际教育学院'
].map(academy => ({ label: academy, value: academy }));

// 表单数据
const userInfo = reactive<MemberModel>({
  userName: '',
  gender: '男',
  userId: '',
  academy: '',
  politicalLandscape: '群众',
  className: '',
  phoneNum: '',
  identity: '',
  joinTime: '',
  passwordHash: '',
  eMail: null
});

// 表单验证规则
const rules = {
  userName: [
    {
      required: true,
      message: '请输入姓名',
      trigger: 'blur'
    },
    {
      pattern: /^[\u4E00-\u9FA5A-Za-z\s]+(·[\u4E00-\u9FA5A-Za-z]+)*$/,
      message: '姓名格式不正确',
      trigger: 'blur'
    }
  ],
  gender: {
    required: true,
    message: '请选择性别',
    trigger: 'change'
  },
  userId: [
    {
      required: true,
      message: '请输入学号',
      trigger: 'blur'
    },
    {
      len: 10,
      message: '学号应为10位数字',
      trigger: 'blur'
    },
    {
      pattern: /(19|20|21|22|23|24)([0-9]{8})/,
      message: '学号格式不正确',
      trigger: 'blur'
    }
  ],
  academy: {
    required: true,
    message: '请选择学院',
    trigger: 'change'
  },
  politicalLandscape: {
    required: true,
    message: '请选择政治面貌',
    trigger: 'change'
  },
  className: [
    {
      required: true,
      message: '请输入专业班级',
      trigger: 'blur'
    },
    {
      pattern: /[\u4e00-\u9fa5|()（）]+[0-9]{4}(.*)/,
      message: '班级名称格式不正确',
      trigger: 'blur'
    }
  ],
  phoneNum: [
    {
      required: true,
      message: '请输入手机号',
      trigger: 'blur'
    },
    {
      pattern: /^1\d{10}$/,
      message: '请输入正确的手机号',
      trigger: 'blur'
    }
  ]
};

// 初始化图表
const initChart = () => {
  if (!chartRef.value) return;
  
  chartInstance = echarts.init(chartRef.value);
  
  const option = {
    tooltip: {
      trigger: 'item'
    },
    legend: {
      orient: 'horizontal',
      bottom: 10,
      data: ['政治面貌', '成员身份']
    },
    series: [
      {
        name: '个人信息统计',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 18,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: [
          {
            value: 1,
            name: userInfo.politicalLandscape,
            itemStyle: { color: '#165DFF' }
          },
          {
            value: 1,
            name: identityMap[userInfo.identity] || userInfo.identity,
            itemStyle: { color: '#00B42A' }
          },
          {
            value: 1,
            name: userInfo.gender,
            itemStyle: { color: '#FF7D00' }
          },
          {
            value: 1,
            name: userInfo.academy ? '已设置学院' : '未设置学院',
            itemStyle: { color: '#86909C' }
          }
        ]
      }
    ],
    color: ['#165DFF', '#00B42A', '#FF7D00', '#86909C']
  };
  
  chartInstance.setOption(option);
  
  // 响应式调整
  const handleResize = () => {
    chartInstance?.resize();
  };
  
  window.addEventListener('resize', handleResize);
  
  // 在组件卸载时清理事件监听器
  onUnmounted(() => {
    window.removeEventListener('resize', handleResize);
  });
};

// 获取用户信息
const fetchUserInfo = async () => {
  try {
    loading.value = true;
    const data = await UserService.getUserData();
    Object.assign(userInfo, data);
    
    // 延迟初始化图表，确保DOM已渲染
    setTimeout(() => {
      initChart();
    }, 100);
  } catch (error: any) {
    console.error('获取用户信息时出错:', error);
    message.error('获取用户信息时出错: ' + (error instanceof Error ? error.message : String(error)));
    
    if (error.message.includes('认证已过期')) {
      authorizationStore.logout();
      await router.push('/login');
    }
  } finally {
    loading.value = false;
  }
};

// 提交表单
const handleSubmit = () => {
  formRef.value?.validate((errors: any) => {
    if (!errors) {
      showModal.value = true;
    } else {
      message.error('请检查填写内容');
    }
  });
};

// 确认更改
const handleConfirm = async () => {
  try {
    confirmLoading.value = true;

    // 使用 UserService 更新用户信息
    await UserService.updateProfile({
      userName: userInfo.userName,
      gender: userInfo.gender,
      userId: userInfo.userId,
      academy: userInfo.academy,
      politicalLandscape: userInfo.politicalLandscape,
      className: userInfo.className,
      phoneNum: userInfo.phoneNum,
      identity: userInfo.identity,
      joinTime: userInfo.joinTime,
      passwordHash: userInfo.passwordHash,
      eMail: userInfo.eMail
    });

    message.success('信息更新成功');
    showModal.value = false;
    
    // 更新图表数据
    if (chartInstance) {
      chartInstance.setOption({
        series: [{
          data: [
            {
              value: 1,
              name: userInfo.politicalLandscape,
              itemStyle: { color: '#165DFF' }
            },
            {
              value: 1,
              name: identityMap[userInfo.identity] || userInfo.identity,
              itemStyle: { color: '#00B42A' }
            },
            {
              value: 1,
              name: userInfo.gender,
              itemStyle: { color: '#FF7D00' }
            },
            {
              value: 1,
              name: userInfo.academy ? '已设置学院' : '未设置学院',
              itemStyle: { color: '#86909C' }
            }
          ]
        }]
      });
    }
  } catch (error: any) {
    console.error('信息更新时出错:', error);
    message.error('信息更新失败: ' + (error instanceof Error ? error.message : String(error)));
    
    if (error.message.includes('认证已过期')) {
      authorizationStore.logout();
      await router.push('/login');
    }
  } finally {
    confirmLoading.value = false;
  }
};

// 监听暗黑模式变化，更新图表样式
watch(() => document.documentElement.classList.contains('dark'), (isDark) => {
  if (chartInstance) {
    chartInstance.dispose();
    initChart();
  }
});

// 组件挂载时获取用户信息
onMounted(() => {
  fetchUserInfo();
});

// 组件卸载时清理图表
onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose();
    chartInstance = null;
  }
});
</script>

<style scoped>
/* 添加全局样式重置 */
:deep(.n-form-item-label) {
  font-weight: 500;
}

:deep(.n-input__input) {
  transition: all 0.2s ease;
}

:deep(.n-input__input:focus) {
  background-color: rgba(255, 255, 255, 0.9);
}

:deep(.dark .n-input__input:focus) {
  background-color: rgba(55, 65, 81, 0.9);
}

/* 动画效果 */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
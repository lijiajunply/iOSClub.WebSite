<template>
  <div class="flex-1 flex flex-col transition-colors duration-300">
    <!-- 主内容区域 -->
    <main class="flex-1 overflow-y-auto p-4 md:p-6">
      <!-- 加载状态 -->
      <div v-if="loading" class="py-6">
        <div
            class="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden max-w-4xl mx-auto">
          <!-- 卡片头部骨架 -->
          <div class="px-6 py-5 border-b border-gray-200 dark:border-gray-700">
            <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3">
              <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-24"></div>
              <div class="flex items-center">
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-16 mr-2"></div>
                <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded-full w-20"></div>
              </div>
            </div>
          </div>

          <!-- 表单内容骨架 -->
          <div class="p-6">
            <!-- 表单行：姓名和性别 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-12 mb-2"></div>
                <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl"></div>
              </div>

              <div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-12 mb-2"></div>
                <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl"></div>
              </div>
            </div>

            <!-- 表单行：学号和学院 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-12 mb-2"></div>
                <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl"></div>
              </div>

              <div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-12 mb-2"></div>
                <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl"></div>
              </div>
            </div>

            <!-- 表单行：政治面貌和专业班级 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-16 mb-2"></div>
                <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl"></div>
              </div>

              <div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-16 mb-2"></div>
                <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl"></div>
              </div>
            </div>

            <!-- 表单行：手机号 -->
            <div class="mb-6">
              <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-12 mb-2"></div>
              <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl"></div>
            </div>

            <!-- 操作按钮 -->
            <div class="flex justify-center pt-4">
              <div class="h-12 bg-gray-200 dark:bg-gray-700 rounded-xl w-40"></div>
            </div>
          </div>
        </div>

        <!-- 数据统计卡片骨架 -->
        <div class="mt-8 grid grid-cols-1 md:grid-cols-3 gap-4 max-w-4xl mx-auto">
          <SkeletonLoader v-for="i in 3" :key="i" type="card"/>
        </div>
      </div>

      <!-- 表单卡片 -->
      <div v-else
           class="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden max-w-4xl mx-auto">
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
                <n-input
                    v-model:value="userInfo.userName"
                    placeholder="请输入姓名"
                    class="bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                >
                  <template #prefix>
                    <Icon icon="lucide:user" class="text-gray-700 dark:text-gray-300" size="18"/>
                  </template>
                </n-input>
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
                <n-input
                    v-model:value="userInfo.userId"
                    placeholder="请输入10位学号"
                    class=" bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                >
                  <template #prefix>
                    <Icon icon="iconoir:hashtag" class="text-gray-700 dark:text-gray-300" size="18"/>
                  </template>
                </n-input>
              </n-form-item>

              <n-form-item label="学院" path="academy" class="mb-0">
                <div class="relative">
                  <div class="absolute z-10 inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="iconoir:graduation-cap" class="text-gray-700 dark:text-gray-300" size="18"/>
                  </div>
                  <n-select
                      v-model:value="userInfo.academy"
                      :options="academyOptions"
                      placeholder="请选择学院"
                      filterable
                      class="pl-10 bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-700 rounded-xl"
                      :bordered="false"
                  />
                </div>
              </n-form-item>
            </div>

            <!-- 表单行：政治面貌和专业班级 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <n-form-item label="政治面貌" path="politicalLandscape" class="mb-0">
                <div class="relative">
                  <div class="absolute z-10 inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                    <Icon icon="lucide:flag" class="text-gray-700 dark:text-gray-300" size="18"/>
                  </div>
                  <n-select
                      v-model:value="userInfo.politicalLandscape"
                      :options="politicalLandscapeOptions"
                      placeholder="请选择政治面貌"
                      class="pl-10 bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-700 rounded-xl"
                      :bordered="false"
                  />
                </div>
              </n-form-item>

              <n-form-item label="专业班级" path="className" class="mb-0">
                <n-input
                    v-model:value="userInfo.className"
                    placeholder="请输入专业班级"
                    class="bg-gray-50 dark:bg-gray-700 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                >
                  <template #prefix>
                    <Icon icon="lucide:users" class="text-gray-700 dark:text-gray-300" size="18"/>
                  </template>
                </n-input>
              </n-form-item>
            </div>

            <!-- 表单行：手机号 -->
            <div class="mb-6 grid grid-cols-1 md:grid-cols-2 gap-6">
              <n-form-item label="手机号" path="phoneNum" class="mb-0">
                <n-input
                    v-model:value="userInfo.phoneNum"
                    placeholder="请输入手机号"
                    class="bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                >
                  <template #prefix>
                    <Icon icon="lucide:phone" class="text-gray-700 dark:text-gray-300" size="18"/>
                  </template>
                </n-input>
              </n-form-item>
              <n-form-item label="邮箱" path="eMail" class="mb-0">
                <n-input
                    v-model:value="userInfo.eMail"
                    placeholder="请输入邮箱地址"
                    class="bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                    :bordered="false"
                >
                  <template #prefix>
                    <Icon icon="lucide:mail" class="text-gray-700 dark:text-gray-300" size="18"/>
                  </template>
                </n-input>
              </n-form-item>
            </div>

            <!-- 操作按钮 -->
            <div class="flex flex-col md:flex-row justify-center gap-4 pt-4">
              <n-button
                  type="primary"
                  @click="handleSubmit"
                  :loading="confirmLoading"
                  size="large"
                  class="w-full md:w-auto px-8 py-2.5 bg-blue-500 hover:bg-blue-600 text-white font-medium rounded-xl shadow-sm transition-all"
              >
                <template #icon>
                  <Icon icon="lucide:save" size="18"/>
                </template>
                保存修改
              </n-button>
              <n-button
                  @click="handlePasswordSubmit"
                  size="large"
                  class="w-full md:w-auto px-8 py-2.5 bg-purple-500 hover:bg-purple-600 text-white font-medium rounded-xl shadow-sm transition-all"
              >
                <template #icon>
                  <Icon icon="lucide:lock" size="18"/>
                </template>
                修改密码
              </n-button>
            </div>
          </n-form>
        </div>
      </div>
    </main>

    <!-- 确认对话框 -->
    <n-modal v-model:show="showModal" preset="dialog" title="确认更改" positive-text="确认" negative-text="取消"
             @positive-click="handleConfirm" :loading="confirmLoading">
      <div class="space-y-2">
        <p class="text-gray-600 dark:text-gray-300">您确定要保存这些更改吗？</p>
        <p class="text-sm text-gray-500 dark:text-gray-400">请仔细核对您的个人信息</p>
      </div>
    </n-modal>

    <!-- 密码修改对话框 -->
    <n-modal v-model:show="showPasswordModal" preset="dialog" title="修改密码" positive-text="确认" negative-text="取消"
             @positive-click="confirmPasswordChange" :loading="passwordLoading">
      <div class="space-y-4">
        <n-form :model="passwordForm" :rules="passwordRules" ref="passwordFormRef">
          <n-form-item path="oldPassword" label="旧密码">
            <n-input
                v-model:value="passwordForm.oldPassword"
                type="password"
                show-password-on="click"
                placeholder="请输入旧密码"
                class="bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                :bordered="false"
            />
          </n-form-item>
          <n-form-item path="newPassword" label="新密码">
            <n-input
                v-model:value="passwordForm.newPassword"
                type="password"
                show-password-on="click"
                placeholder="请输入新密码（至少6位）"
                class="bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                :bordered="false"
            />
          </n-form-item>
          <n-form-item path="confirmPassword" label="确认新密码">
            <n-input
                v-model:value="passwordForm.confirmPassword"
                type="password"
                show-password-on="click"
                placeholder="请再次输入新密码"
                class="bg-gray-50 dark:bg-gray-750 border-gray-200 dark:border-gray-700 rounded-xl"
                :bordered="false"
            />
          </n-form-item>
        </n-form>
      </div>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import {ref, reactive, onMounted} from 'vue';
import {useMessage, NButton, NForm, NFormItem, NInput, NSelect, NModal} from 'naive-ui';
import {Icon} from '@iconify/vue';
import SkeletonLoader from '../components/SkeletonLoader.vue';
import {useAuthorizationStore} from '../stores/Authorization';
import {UserService} from '../services/UserService';
import {AuthService} from '../services/AuthService';
import type {MemberModel} from '../models';
import {useRouter} from 'vue-router';

const message = useMessage();
const authorizationStore = useAuthorizationStore();
const router = useRouter();

const loading = ref(false);
const confirmLoading = ref(false);
const showModal = ref(false);
const formRef = ref<InstanceType<typeof NForm> | null>(null);

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
].map(item => ({label: item, value: item}));

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
].map(academy => ({label: academy, value: academy}));

// 密码修改相关状态
const showPasswordModal = ref(false);
const passwordLoading = ref(false);
const passwordForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
});
const passwordRules = {
  oldPassword: {
    required: true,
    message: '请输入旧密码',
    trigger: 'blur'
  },
  newPassword: [
    {
      required: true,
      message: '请输入新密码',
      trigger: 'blur'
    },
    {
      min: 6,
      message: '密码长度至少为6位',
      trigger: 'blur'
    }
  ],
  confirmPassword: [
    {
      required: true,
      message: '请确认新密码',
      trigger: 'blur'
    },
    {
      validator: (_: any, value: string) => {
        return value === passwordForm.newPassword;
      },
      message: '两次输入的密码不一致',
      trigger: 'blur'
    }
  ]
};

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
  ],
  eMail: [
    {
      required: false,
      message: '请输入正确的邮箱地址',
      trigger: 'blur'
    },
    {
      pattern: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
      message: '请输入正确的邮箱格式',
      trigger: 'blur'
    }
  ]
};

// 获取用户信息
const fetchUserInfo = async () => {
  try {
    loading.value = true;
    const data = await UserService.getUserData();
    Object.assign(userInfo, data);
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

// 处理密码修改
const handlePasswordSubmit = () => {
  showPasswordModal.value = true;
};

// 确认修改密码
const confirmPasswordChange = async () => {
  try {
    passwordLoading.value = true;
    await AuthService.changePassword(userInfo.userId, passwordForm.oldPassword, passwordForm.newPassword);
    message.success('密码修改成功');
    showPasswordModal.value = false;

    // 重置密码表单
    passwordForm.oldPassword = '';
    passwordForm.newPassword = '';
    passwordForm.confirmPassword = '';
  } catch (error: any) {
    console.error('密码修改失败:', error);
    message.error('密码修改失败: ' + (error instanceof Error ? error.message : String(error)));
  } finally {
    passwordLoading.value = false;
  }
};

// 组件挂载时获取用户信息
onMounted(() => {
  fetchUserInfo();
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
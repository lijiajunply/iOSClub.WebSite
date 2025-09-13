<template>
  <!-- 主内容区：使用CSS类而不是内联样式 -->
  <div
      class="main-content"
      :class="{ 'with-sidebar': layoutStore.showSidebar && !layoutStore.isMobile }"
  >
    <n-card title="我的信息" class="mb-6">
      <template #header-extra>
        <n-tag type="info">当前身份: {{ identityMap[userInfo.identity] || userInfo.identity }}</n-tag>
      </template>

      <n-form
          :model="userInfo"
          :rules="rules"
          ref="formRef"
          label-placement="left"
          label-width="120"
          require-mark-placement="right-hanging"
      >
        <n-form-item label="姓名" path="userName">
          <n-input v-model:value="userInfo.userName" />
        </n-form-item>

        <n-form-item label="性别" path="gender">
          <n-radio-group v-model:value="userInfo.gender">
            <n-space>
              <n-radio
                  v-for="gender in genderOptions"
                  :key="gender"
                  :value="gender"
              >
                {{ gender }}
              </n-radio>
            </n-space>
          </n-radio-group>
        </n-form-item>

        <n-form-item label="学号" path="userId">
          <n-input v-model:value="userInfo.userId" />
        </n-form-item>

        <n-form-item label="学院" path="academy">
          <n-select
              v-model:value="userInfo.academy"
              :options="academyOptions"
              filterable
          />
        </n-form-item>

        <n-form-item label="政治面貌" path="politicalLandscape">
          <n-radio-group v-model:value="userInfo.politicalLandscape">
            <n-space>
              <n-radio
                  v-for="political in politicalLandscapeOptions"
                  :key="political"
                  :value="political"
              >
                {{ political }}
              </n-radio>
            </n-space>
          </n-radio-group>
        </n-form-item>

        <n-form-item label="专业班级" path="className">
          <n-input v-model:value="userInfo.className" />
        </n-form-item>

        <n-form-item label="手机号" path="phoneNum">
          <n-input v-model:value="userInfo.phoneNum" />
        </n-form-item>

        <div class="flex justify-center">
          <n-button
              type="primary"
              @click="handleSubmit"
              :loading="confirmLoading"
              size="large"
          >
            更改
          </n-button>
        </div>
      </n-form>
    </n-card>

    <n-modal
        v-model:show="showModal"
        preset="dialog"
        title="确认更改"
        content="确定提交?"
        positive-text="确认"
        negative-text="取消"
        :loading="confirmLoading"
        @positive-click="handleConfirm"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import {
  NCard,
  NTag,
  NForm,
  NFormItem,
  NInput,
  NRadioGroup,
  NRadio,
  NSpace,
  NSelect,
  NButton,
  NModal,
  useMessage
} from 'naive-ui';
import { useRouter } from 'vue-router';
import { useAuthorizationStore } from '../stores/Authorization';
import { useLayoutStore } from '../stores/LayoutStore';

const message = useMessage();
const router = useRouter();
const authorizationStore = useAuthorizationStore();
const layoutStore = useLayoutStore();

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

// 性别选项
const genderOptions = ['男', '女'];

// 政治面貌选项
const politicalLandscapeOptions = [
  '群众',
  '共青团员',
  '中共党员',
  '中共预备党员'
];

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
interface UserInfo {
  userName: string;
  gender: string;
  userId: string;
  academy: string;
  politicalLandscape: string;
  className: string;
  phoneNum: string;
  identity: string;
}

const userInfo = reactive<UserInfo>({
  userName: '',
  gender: '男',
  userId: '',
  academy: '',
  politicalLandscape: '群众',
  className: '',
  phoneNum: '',
  identity: ''
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
      pattern: /[\u4e00-\u9fa5|(|)|（|）]+[0-9]{4}(.*)/,
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

// 获取用户信息
const fetchUserInfo = async () => {
  try {
    loading.value = true;

    const token = localStorage.getItem('Authorization');
    if (!token) {
      message.error('未找到认证信息');
      router.push('/login');
      return;
    }

    const response = await fetch('https://www.xauat.site/api/Member/GetData', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const data = await response.json();
      Object.assign(userInfo, {
        userName: data.userName,
        gender: data.gender,
        userId: data.userId,
        academy: data.academy,
        politicalLandscape: data.politicalLandscape,
        className: data.className,
        phoneNum: data.phoneNum,
        identity: data.identity
      });
    } else if (response.status === 401) {
      message.error('认证已过期，请重新登录');
      authorizationStore.logout();
      router.push('/login');
    } else {
      message.error('获取用户信息失败');
    }
  } catch (error) {
    console.error('获取用户信息时出错:', error);
    message.error('获取用户信息时出错');
  } finally {
    loading.value = false;
  }
};

// 提交表单
const handleSubmit = (e: MouseEvent) => {
  e.preventDefault();
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

    const token = localStorage.getItem('Authorization');
    if (!token) {
      message.error('未找到认证信息');
      return Promise.reject();
    }

    // 调用API更新用户信息
    const response = await fetch('https://www.xauat.site/api/Member/UpdatePersonalData', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        userName: userInfo.userName,
        gender: userInfo.gender,
        userId: userInfo.userId,
        academy: userInfo.academy,
        politicalLandscape: userInfo.politicalLandscape,
        className: userInfo.className,
        phoneNum: userInfo.phoneNum
      })
    });

    if (response.ok) {
      message.success('信息更新成功');
      showModal.value = false;
      return Promise.resolve();
    } else if (response.status === 401) {
      message.error('认证已过期，请重新登录');
      authorizationStore.logout();
      router.push('/login');
      return Promise.reject();
    } else {
      const errorData = await response.json();
      message.error(errorData.message || '信息更新失败');
      return Promise.reject();
    }
  } catch (error) {
    console.error('信息更新时出错:', error);
    message.error('信息更新失败');
    return Promise.reject();
  } finally {
    confirmLoading.value = false;
  }
};

// 组件挂载时获取用户信息
onMounted(() => {
  fetchUserInfo();
});
</script>

<style scoped>
.main-content {
  padding: 1.5rem;
  width: 100%;
  transition: margin-left 0.3s ease;
}

.n-card {
  transition: box-shadow 0.3s ease;
}

.n-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

:deep(.n-form-item-label) {
  font-weight: 500;
}

/* 移动端适配 */
@media (max-width: 768px) {
  .main-content {
    padding: 6px;
  }

  .main-content.with-sidebar {
    margin-left: 0;
  }
}
</style>

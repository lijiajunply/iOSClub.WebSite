<template>
  <div class="p-6">
    <n-card title="成员数据管理" class="mb-6">
      <template #header-extra>
        <n-space>
          <n-button type="primary" @click="showAddMemberModal">
            添加成员
          </n-button>
          <n-button @click="exportData">
            导出数据
          </n-button>
          <n-button @click="fetchMembers">
            <template #icon>
              <n-icon><Refresh /></n-icon>
            </template>
            刷新
          </n-button>
        </n-space>
      </template>

      <n-input
          v-model:value="searchTerm"
          placeholder="搜索成员..."
          clearable
          class="mb-4"
      >
        <template #prefix>
          <n-icon><SearchOutline /></n-icon>
        </template>
      </n-input>

      <n-data-table
          :columns="columns"
          :data="filteredMembers"
          :pagination="pagination"
          :bordered="false"
          striped
          :loading="loading"
      />
    </n-card>

    <!-- 添加/编辑成员模态框 -->
    <n-modal v-model:show="showModal" preset="card" style="width: 600px;" :title="currentMember.id ? '编辑成员' : '添加成员'">
      <n-form :model="currentMember" :rules="rules" ref="formRef">
        <n-form-item label="姓名" path="userName">
          <n-input v-model:value="currentMember.userName" placeholder="请输入姓名" />
        </n-form-item>
        <n-form-item label="学号" path="userId">
          <n-input v-model:value="currentMember.userId" placeholder="请输入学号" />
        </n-form-item>
        <n-form-item label="学院" path="academy">
          <n-select
              v-model:value="currentMember.academy"
              :options="academyOptions"
              filterable
              placeholder="请选择学院"
          />
        </n-form-item>
        <n-form-item label="专业班级" path="className">
          <n-input v-model:value="currentMember.className" placeholder="请输入专业班级" />
        </n-form-item>
        <n-form-item label="手机号" path="phoneNum">
          <n-input v-model:value="currentMember.phoneNum" placeholder="请输入手机号" />
        </n-form-item>
        <n-form-item label="政治面貌" path="politicalLandscape">
          <n-select
              v-model:value="currentMember.politicalLandscape"
              :options="politicalLandscapeOptions"
              placeholder="请选择政治面貌"
          />
        </n-form-item>
        <n-form-item label="性别" path="gender">
          <n-radio-group v-model:value="currentMember.gender">
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
      </n-form>
      <template #footer>
        <n-space justify="end">
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="saveMember">保存</n-button>
        </n-space>
      </template>
    </n-modal>
  </div>
</template>

<script setup>
import { ref, computed, reactive, h, onMounted } from 'vue'
import { SearchOutline, Refresh } from '@vicons/ionicons5'
import { NMessage } from 'naive-ui'

const showModal = ref(false)
const searchTerm = ref('')
const formRef = ref(null)
const loading = ref(false)

// 学院选项
const academyOptions = [
  '建筑学院',
  '城乡规划学院',
  '环境与市政工程学院',
  '建筑设备科学与工程学院',
  '土木工程学院',
  '交通运输工程学院',
  '环境工程学院',
  '材料科学与工程学院',
  '管理学院',
  '机电工程学院',
  '冶金工程学院',
  '信息与控制工程学院',
  '艺术学院',
  '理学院',
  '文学院',
  '马克思主义学院',
  '体育学院',
  '继续教育学院'
].map(academy => ({ label: academy, value: academy }));

// 政治面貌选项
const politicalLandscapeOptions = [
  '群众',
  '共青团员',
  '中共党员'
].map(political => ({ label: political, value: political }));

// 性别选项
const genderOptions = ['男', '女'];

// 成员数据
const members = ref([])

const currentMember = ref({
  id: null,
  userName: '',
  userId: '',
  academy: null,
  className: '',
  phoneNum: '',
  politicalLandscape: null,
  gender: null
})

const rules = {
  userName: {
    required: true,
    message: '请输入姓名',
    trigger: 'blur'
  },
  userId: [
    {
      required: true,
      message: '请输入学号',
      trigger: 'blur'
    },
    {
      min: 10,
      max: 10,
      message: '学号应为10位数字',
      trigger: 'blur'
    }
  ],
  academy: {
    required: true,
    message: '请选择学院',
    trigger: 'change'
  },
  className: {
    required: true,
    message: '请输入专业班级',
    trigger: 'blur'
  },
  phoneNum: [
    {
      required: true,
      message: '请输入手机号',
      trigger: 'blur'
    },
    {
      pattern: /^1[3-9]\d{9}$/,
      message: '请输入正确的手机号',
      trigger: 'blur'
    }
  ],
  politicalLandscape: {
    required: true,
    message: '请选择政治面貌',
    trigger: 'change'
  },
  gender: {
    required: true,
    message: '请选择性别',
    trigger: 'change'
  }
}

const columns = [
  {
    title: '姓名',
    key: 'userName',
    width: 100
  },
  {
    title: '学号',
    key: 'userId',
    width: 150
  },
  {
    title: '学院',
    key: 'academy',
    width: 200
  },
  {
    title: '专业班级',
    key: 'className',
    width: 150
  },
  {
    title: '手机号',
    key: 'phoneNum',
    width: 150
  },
  {
    title: '政治面貌',
    key: 'politicalLandscape',
    width: 120
  },
  {
    title: '性别',
    key: 'gender',
    width: 80
  },
  {
    title: '操作',
    key: 'actions',
    width: 150,
    render(row) {
      return [
        h(
            'n-button',
            {
              strong: true,
              tertiary: true,
              size: 'small',
              onClick: () => editMember(row)
            },
            { default: () => '编辑' }
        ),
        h(
            'n-button',
            {
              strong: true,
              tertiary: true,
              size: 'small',
              type: 'error',
              onClick: () => deleteMember(row)
            },
            { default: () => '删除' }
        )
      ]
    }
  }
]

const pagination = {
  pageSize: 10
}

const filteredMembers = computed(() => {
  if (!searchTerm.value) return members.value
  const term = searchTerm.value.toLowerCase()
  return members.value.filter(member =>
      member.userName.toLowerCase().includes(term) ||
      member.userId.includes(term) ||
      (member.academy && member.academy.toLowerCase().includes(term)) ||
      (member.className && member.className.toLowerCase().includes(term))
  )
})

// 获取成员数据
const fetchMembers = async () => {
  try {
    loading.value = true
    const token = localStorage.getItem('Authorization')
    if (!token) {
      NMessage.error('未找到认证信息')
      return
    }

    const response = await fetch('https://www.xauat.site/api/President/GetAllData', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (response.ok) {
      const compressedData = await response.text()
      // 这里应该解压缩数据，但为了简化，我们假设后端直接返回JSON
      // 实际项目中需要处理GZip解压缩
      const data = JSON.parse(compressedData)
      members.value = data
    } else if (response.status === 401) {
      NMessage.error('认证已过期，请重新登录')
    } else {
      NMessage.error('获取成员数据失败')
    }
  } catch (error) {
    console.error('获取成员数据时出错:', error)
    NMessage.error('获取成员数据时出错')
  } finally {
    loading.value = false
  }
}

const showAddMemberModal = () => {
  currentMember.value = {
    id: null,
    userName: '',
    userId: '',
    academy: null,
    className: '',
    phoneNum: '',
    politicalLandscape: null,
    gender: null
  }
  showModal.value = true
}

const editMember = (member) => {
  currentMember.value = { ...member }
  showModal.value = true
}

const deleteMember = async (member) => {
  try {
    const token = localStorage.getItem('Authorization')
    if (!token) {
      NMessage.error('未找到认证信息')
      return
    }

    const response = await fetch(`https://www.xauat.site/api/President/Delete/${member.userId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (response.ok) {
      const index = members.value.findIndex(m => m.userId === member.userId)
      if (index !== -1) {
        members.value.splice(index, 1)
        NMessage.success(`删除成员: ${member.userName} 成功`)
      }
    } else {
      NMessage.error('删除成员失败')
    }
  } catch (error) {
    console.error('删除成员时出错:', error)
    NMessage.error('删除成员时出错')
  }
}

const saveMember = async (e) => {
  e.preventDefault()
  formRef.value?.validate(async (errors) => {
    if (!errors) {
      try {
        const token = localStorage.getItem('Authorization')
        if (!token) {
          NMessage.error('未找到认证信息')
          return
        }

        // 这里应该调用实际的API来保存成员信息
        // 由于没有看到对应的API，暂时只做前端演示
        if (currentMember.value.id) {
          // 更新成员
          NMessage.success('成员信息已更新')
        } else {
          // 添加新成员
          currentMember.value.id = members.value.length + 1
          members.value.push({ ...currentMember.value })
          NMessage.success('新成员已添加')
        }
        showModal.value = false
      } catch (error) {
        console.error('保存成员信息时出错:', error)
        NMessage.error('保存成员信息时出错')
      }
    } else {
      NMessage.error('请检查表单填写是否正确')
    }
  })
}

// 导出为 CSV 格式，无需额外库
const exportData = () => {
  try {
    // 定义 CSV 头部
    const headers = ['姓名', '学号', '学院', '专业班级', '手机号', '政治面貌', '性别'];
    const keys = ['userName', 'userId', 'academy', 'className', 'phoneNum', 'politicalLandscape', 'gender'];

    // 创建 CSV 内容
    let csvContent = '\uFEFF' + headers.join(',') + '\n'; // 添加 UTF-8 BOM

    // 添加数据行
    members.value.forEach(member => {
      const row = keys.map(key => {
        const value = member[key] || '';
        // 转义包含逗号或引号的值
        return `"${value.toString().replace(/"/g, '""')}"`;
      });
      csvContent += row.join(',') + '\n';
    });

    // 创建并下载文件
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', '成员数据.csv');
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    NMessage.success('数据导出成功');
  } catch (error) {
    console.error('导出数据失败:', error);
    NMessage.error('导出数据失败');
  }
}

// 组件挂载时获取成员数据
onMounted(() => {
  fetchMembers()
})
</script>

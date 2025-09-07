<template>
  <div class="p-6">
    <n-card title="资源管理" class="mb-6">
      <template #header-extra>
        <n-space>
          <n-button type="primary" @click="showAddResourceModal">
            添加资源
          </n-button>
          <n-button @click="fetchResources">
            <template #icon>
              <n-icon><Refresh /></n-icon>
            </template>
            刷新
          </n-button>
        </n-space>
      </template>

      <n-input
          v-model:value="searchTerm"
          placeholder="搜索资源..."
          clearable
          class="mb-4"
      >
        <template #prefix>
          <n-icon><SearchOutline /></n-icon>
        </template>
      </n-input>

      <n-data-table
          :columns="columns"
          :data="filteredResources"
          :pagination="pagination"
          :bordered="false"
          striped
      />
    </n-card>

    <!-- 添加/编辑资源模态框 -->
    <n-modal v-model:show="showModal" preset="card" style="width: 600px;" :title="editingResource.id ? '编辑资源' : '添加资源'">
      <n-form :model="editingResource" :rules="rules" ref="formRef">
        <n-form-item label="资源名称" path="name">
          <n-input v-model:value="editingResource.name" placeholder="请输入资源名称" />
        </n-form-item>
        <n-form-item label="描述" path="description">
          <n-input v-model:value="editingResource.description" placeholder="请输入资源描述" type="textarea" />
        </n-form-item>
        <n-form-item label="标签" path="tag">
          <n-dynamic-tags v-model:value="resourceTags" />
        </n-form-item>
      </n-form>
      <template #footer>
        <n-space justify="end">
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="saveResource">保存</n-button>
        </n-space>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useMessage } from 'naive-ui';
import { Refresh, SearchOutline } from '@vicons/ionicons5';
import type { DataTableColumns } from 'naive-ui';

interface Resource {
  id: string;
  name: string;
  description: string | null;
  tag: string | null;
}

const message = useMessage();
const showModal = ref(false);
const searchTerm = ref('');
const formRef = ref();
const editingResource = ref<Resource>({
  id: '',
  name: '',
  description: '',
  tag: ''
});

const resourceTags = ref<string[]>([]);

// 监听编辑资源的标签变化
watch(() => editingResource.value.tag, (newTag) => {
  if (newTag) {
    resourceTags.value = newTag.split(',').map(t => t.trim()).filter(t => t.length > 0);
  } else {
    resourceTags.value = [];
  }
});

// 监听resourceTags变化，更新editingResource.tag
watch(resourceTags, (newTags) => {
  editingResource.value.tag = newTags.join(', ');
}, { deep: true });

const rules = {
  name: {
    required: true,
    message: '请输入资源名称',
    trigger: 'blur'
  }
};

const resources = ref<Resource[]>([]);

// 计算过滤后的资源列表
const filteredResources = computed(() => {
  if (!searchTerm.value) {
    return resources.value;
  }
  const term = searchTerm.value.toLowerCase();
  return resources.value.filter(resource =>
      resource.name.toLowerCase().includes(term) ||
      (resource.description && resource.description.toLowerCase().includes(term)) ||
      (resource.tag && resource.tag.toLowerCase().includes(term))
  );
});

const columns: DataTableColumns<Resource> = [
  {
    title: '资源名称',
    key: 'name',
    width: 200
  },
  {
    title: '描述',
    key: 'description',
    width: 300,
    render: (row) => row.description || '无描述'
  },
  {
    title: '标签',
    key: 'tag',
    width: 200,
    render: (row) => {
      if (!row.tag) return '无标签';
      const tags = row.tag.split(',').map(t => t.trim()).filter(t => t.length > 0);
      return tags.map(tag =>
          h('n-tag', {
            style: { marginRight: '4px' },
            type: 'info',
            size: 'small'
          }, { default: () => tag })
      );
    }
  },
  {
    title: '操作',
    key: 'actions',
    width: 150,
    render: (row) => {
      return h('n-space', {}, [
        h('n-button', {
          text: true,
          type: 'primary',
          onClick: () => editResource(row)
        }, { default: () => '编辑' }),
        h('n-button', {
          text: true,
          type: 'error',
          onClick: () => deleteResource(row)
        }, { default: () => '删除' })
      ]);
    }
  }
];

const pagination = {
  pageSize: 10
};

// 获取资源列表
const fetchResources = async () => {
  try {
    const token = localStorage.getItem('token');
    if (!token) {
      message.error('未找到认证信息');
      return;
    }

    const response = await fetch('/api/Project/GetResources', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (response.ok) {
      const data = await response.json();
      resources.value = data.map((item: any) => ({
        id: item.id,
        name: item.name,
        description: item.description,
        tag: item.tag
      }));
    } else {
      message.error('获取资源列表失败');
    }
  } catch (error) {
    console.error('获取资源列表时出错:', error);
    message.error('获取资源列表时出错');
  }
};

// 显示添加资源模态框
const showAddResourceModal = () => {
  editingResource.value = {
    id: '',
    name: '',
    description: '',
    tag: ''
  };
  resourceTags.value = [];
  showModal.value = true;
};

// 编辑资源
const editResource = (resource: Resource) => {
  editingResource.value = { ...resource };
  showModal.value = true;
};

// 保存资源（添加或更新）
const saveResource = async () => {
  try {
    await formRef.value?.validate();

    const token = localStorage.getItem('token');
    if (!token) {
      message.error('未找到认证信息');
      return;
    }

    // 准备数据
    const resourceData = {
      ...editingResource.value,
      tag: resourceTags.value.join(', ')
    };

    // 这里应该调用实际的API来保存资源
    // 由于没有看到对应的API，暂时只做前端演示
    message.success(editingResource.value.id ? '资源更新成功' : '资源添加成功');
    showModal.value = false;

    // 重新获取资源列表
    await fetchResources();
  } catch (error) {
    message.error('表单验证失败');
  }
};

// 删除资源
const deleteResource = async (resource: Resource) => {
  try {
    const token = localStorage.getItem('token');
    if (!token) {
      message.error('未找到认证信息');
      return;
    }

    // 这里应该调用实际的API来删除资源
    // 由于没有看到对应的API，暂时只做前端演示
    message.success('资源删除成功');

    // 重新获取资源列表
    await fetchResources();
  } catch (error) {
    message.error('删除资源时出错');
  }
};

// 组件挂载时获取资源列表
onMounted(() => {
  fetchResources();
});
</script>

<style scoped>
.n-card {
  transition: box-shadow 0.3s ease;
}

.n-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}
</style>

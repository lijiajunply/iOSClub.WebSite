<template>
  <div class="p-6">
    <n-card title="部门管理" class="mb-6">
      <template #header-extra>
        <n-button type="primary" @click="showAddDepartmentModal">
          添加部门
        </n-button>
      </template>

      <n-grid :cols="1" :md="2" :lg="3" :x-gap="12" :y-gap="12">
        <n-grid-item v-for="department in departments" :key="department.id">
          <n-card hoverable>
            <div class="flex justify-between items-start">
              <div>
                <h3 class="text-xl font-semibold">{{ department.name }}</h3>
                <p class="text-gray-600 mt-1">{{ department.description }}</p>
                <p class="text-sm text-gray-500 mt-2">{{ department.memberCount }} 名成员</p>
              </div>
              <n-dropdown :options="dropdownOptions" @select="(key) => handleSelect(key, department)">
                <n-button quaternary circle>
                  <template #icon>
                    <n-icon><EllipsisVertical /></n-icon>
                  </template>
                </n-button>
              </n-dropdown>
            </div>

            <div class="mt-4">
              <n-tag v-for="member in department.members.slice(0, 3)" :key="member.id" class="mr-2 mb-2">
                {{ member.name }}
              </n-tag>
              <n-tag v-if="department.members.length > 3" type="info">
                +{{ department.members.length - 3 }} 更多
              </n-tag>
            </div>
          </n-card>
        </n-grid-item>
      </n-grid>
    </n-card>

    <!-- 添加/编辑部门模态框 -->
    <n-modal v-model:show="showModal" preset="card" style="width: 600px;" :title="editingDepartment.id ? '编辑部门' : '添加部门'">
      <n-form :model="editingDepartment" :rules="rules" ref="formRef">
        <n-form-item label="部门名称" path="name">
          <n-input v-model:value="editingDepartment.name" placeholder="请输入部门名称" />
        </n-form-item>
        <n-form-item label="部门描述" path="description">
          <n-input v-model:value="editingDepartment.description" placeholder="请输入部门描述" type="textarea" />
        </n-form-item>
      </n-form>

      <template #footer>
        <n-space justify="end">
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="saveDepartment">保存</n-button>
        </n-space>
      </template>
    </n-modal>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { EllipsisVertical } from '@vicons/ionicons5'

const showModal = ref(false)
const formRef = ref(null)

const departments = ref([
  {
    id: 1,
    name: '技术部',
    description: '负责技术开发和项目实施',
    memberCount: 15,
    members: [
      { id: 1, name: '张三' },
      { id: 2, name: '李四' },
      { id: 3, name: '王五' },
      { id: 4, name: '赵六' }
    ]
  },
  {
    id: 2,
    name: '设计部',
    description: '负责UI/UX设计和视觉创意',
    memberCount: 10,
    members: [
      { id: 5, name: '钱七' },
      { id: 6, name: '孙八' },
      { id: 7, name: '周九' }
    ]
  },
  {
    id: 3,
    name: '运营部',
    description: '负责社团运营和活动策划',
    memberCount: 8,
    members: [
      { id: 8, name: '吴十' },
      { id: 9, name: '郑一' },
      { id: 10, name: '王二' }
    ]
  }
])

const editingDepartment = ref({
  id: null,
  name: '',
  description: ''
})

const rules = {
  name: {
    required: true,
    message: '请输入部门名称',
    trigger: 'blur'
  },
  description: {
    required: true,
    message: '请输入部门描述',
    trigger: 'blur'
  }
}

const dropdownOptions = [
  {
    label: '编辑',
    key: 'edit'
  },
  {
    label: '删除',
    key: 'delete'
  }
]

const handleSelect = (key, department) => {
  switch (key) {
    case 'edit':
      editDepartment(department)
      break
    case 'delete':
      deleteDepartment(department)
      break
  }
}

const showAddDepartmentModal = () => {
  editingDepartment.value = {
    id: null,
    name: '',
    description: ''
  }
  showModal.value = true
}

const editDepartment = (department) => {
  editingDepartment.value = { ...department }
  showModal.value = true
}

const deleteDepartment = (department) => {
  window.$dialog.warning({
    title: '确认删除',
    content: `确定要删除 "${department.name}" 部门吗？`,
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: () => {
      const index = departments.value.findIndex(d => d.id === department.id)
      if (index !== -1) {
        departments.value.splice(index, 1)
        window.$message.success('部门已删除')
      }
    }
  })
}

const saveDepartment = (e) => {
  e.preventDefault()
  formRef.value?.validate((errors) => {
    if (!errors) {
      if (editingDepartment.value.id) {
        // 更新部门
        const index = departments.value.findIndex(d => d.id === editingDepartment.value.id)
        if (index !== -1) {
          departments.value[index] = { ...editingDepartment.value, memberCount: departments.value[index].memberCount, members: departments.value[index].members }
          window.$message.success('部门信息已更新')
        }
      } else {
        // 添加新部门
        const newDepartment = {
          ...editingDepartment.value,
          id: Math.max(0, ...departments.value.map(d => d.id)) + 1,
          memberCount: 0,
          members: []
        }
        departments.value.push(newDepartment)
        window.$message.success('新部门已添加')
      }
      showModal.value = false
    } else {
      window.$message.error('请检查表单填写是否正确')
    }
  })
}
</script>

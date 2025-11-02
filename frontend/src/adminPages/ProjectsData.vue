<template>
  <div class="p-6">
    <n-card title="项目管理" class="mb-6">
      <template #header-extra>
        <n-button type="primary" @click="showAddProjectModal">
          添加项目
        </n-button>
      </template>

      <n-tabs type="line" animated>
        <n-tab-pane name="active" tab="进行中">
          <n-list>
            <n-list-item v-for="project in activeProjects" :key="project.id">
              <n-thing :title="project.name" :description="project.description">
                <template #avatar>
                  <n-avatar>
                    <Icon icon="ion:folder-outline" />
                  </n-avatar>
                </template>
                <template #header-extra>
                  <n-tag :type="getProjectStatusType(project.status)">
                    {{ project.status }}
                  </n-tag>
                </template>
                <template #description>
                  <div class="flex items-center mt-1">
                    <span class="text-sm text-gray-500">{{ project.startDate }} - {{ project.endDate }}</span>
                    <n-progress
                        class="ml-4 flex-1"
                        type="line"
                        :percentage="project.progress"
                        :show-indicator="false"
                        style="max-width: 200px;"
                    />
                    <span class="text-sm text-gray-500 ml-2">{{ project.progress }}%</span>
                  </div>
                </template>
              </n-thing>

              <template #suffix>
                <n-button-group>
                  <n-button strong secondary @click="editProject(project)">
                    <template #icon>
                      <Icon icon="ion:create-outline" />
                    </template>
                  </n-button>
                  <n-button strong secondary @click="viewProjectDetails(project)">
                    <template #icon>
                      <Icon icon="ion:eye-outline" />
                    </template>
                  </n-button>
                </n-button-group>
              </template>
            </n-list-item>
          </n-list>
        </n-tab-pane>

        <n-tab-pane name="completed" tab="已完成">
          <n-list>
            <n-list-item v-for="project in completedProjects" :key="project.id">
              <n-thing :title="project.name" :description="project.description">
                <template #avatar>
                  <n-avatar>
                    <n-icon><FolderOutline /></n-icon>
                  </n-avatar>
                </template>
                <template #header-extra>
                  <n-tag type="success">
                    已完成
                  </n-tag>
                </template>
                <template #description>
                  <div class="flex items-center mt-1">
                    <span class="text-sm text-gray-500">{{ project.startDate }} - {{ project.endDate }}</span>
                  </div>
                </template>
              </n-thing>

              <template #suffix>
                <n-button strong secondary @click="viewProjectDetails(project)">
                  <template #icon>
                    <n-icon><EyeOutline /></n-icon>
                  </template>
                </n-button>
              </template>
            </n-list-item>
          </n-list>
        </n-tab-pane>
      </n-tabs>
    </n-card>

    <!-- 添加/编辑项目模态框 -->
    <n-modal v-model:show="showModal" preset="card" style="width: 600px;" :title="editingProject.id ? '编辑项目' : '添加项目'">
      <n-form :model="editingProject" :rules="rules" ref="formRef">
        <n-form-item label="项目名称" path="name">
          <n-input v-model:value="editingProject.name" placeholder="请输入项目名称" />
        </n-form-item>
        <n-form-item label="项目描述" path="description">
          <n-input v-model:value="editingProject.description" placeholder="请输入项目描述" type="textarea" />
        </n-form-item>
        <n-form-item label="开始日期" path="startDate">
          <n-date-picker v-model:value="editingProject.startDate" type="date" />
        </n-form-item>
        <n-form-item label="结束日期" path="endDate">
          <n-date-picker v-model:value="editingProject.endDate" type="date" />
        </n-form-item>
        <n-form-item label="状态" path="status">
          <n-select v-model:value="editingProject.status" :options="statusOptions" />
        </n-form-item>
        <n-form-item label="进度" path="progress">
          <n-slider v-model:value="editingProject.progress" :step="1" />
        </n-form-item>
      </n-form>

      <template #footer>
        <n-space justify="end">
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="saveProject">保存</n-button>
        </n-space>
      </template>
    </n-modal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useMessage } from 'naive-ui'
import { NButton, NCard, NList, NListItem, NThing, NAvatar, NTag, NProgress, NModal, NForm, NFormItem, NInput, NButtonGroup } from 'naive-ui'
import { Icon } from '@iconify/vue'
import { ProjectService } from '../services/ProjectService'

const message = useMessage()
const showModal = ref(false)
const formRef = ref(null)
const loading = ref(false)

const projects = ref([])

const editingProject = ref({
  id: null,
  name: '',
  description: '',
  startDate: null,
  endDate: null,
  status: '进行中',
  progress: 0
})

const rules = {
  name: {
    required: true,
    message: '请输入项目名称',
    trigger: 'blur'
  },
  description: {
    required: true,
    message: '请输入项目描述',
    trigger: 'blur'
  },
  startDate: {
    required: true,
    message: '请选择开始日期',
    trigger: 'change'
  },
  endDate: {
    required: true,
    message: '请选择结束日期',
    trigger: 'change'
  },
  status: {
    required: true,
    message: '请选择项目状态',
    trigger: 'change'
  }
}

const statusOptions = [
  { label: '规划中', value: '规划中' },
  { label: '进行中', value: '进行中' },
  { label: '已完成', value: '已完成' },
  { label: '已暂停', value: '已暂停' }
]

const activeProjects = computed(() => {
  return projects.value.filter(project => project.status !== '已完成')
})

const completedProjects = computed(() => {
  return projects.value.filter(project => project.status === '已完成')
})

const getProjectStatusType = (status) => {
  switch (status) {
    case '规划中': return 'default'
    case '进行中': return 'info'
    case '已完成': return 'success'
    case '已暂停': return 'warning'
    default: return 'default'
  }
}

const showAddProjectModal = () => {
  editingProject.value = {
    id: null,
    name: '',
    description: '',
    startDate: null,
    endDate: null,
    status: '进行中',
    progress: 0
  }
  showModal.value = true
}

const editProject = (project) => {
  editingProject.value = { ...project }
  showModal.value = true
}

const viewProjectDetails = (project) => {
  message.info(`查看项目 "${project.name}" 的详细信息`)
  // 实际项目中可以跳转到项目详情页
}

const saveProject = async (e) => {
  e.preventDefault()
  formRef.value?.validate(async (errors) => {
    if (!errors) {
      try {
        loading.value = true
        const projectData = {
          id: editingProject.value.id || undefined,
          name: editingProject.value.name,
          description: editingProject.value.description,
          startTime: editingProject.value.startDate ? new Date(editingProject.value.startDate).toISOString() : null,
          endTime: editingProject.value.endDate ? new Date(editingProject.value.endDate).toISOString() : null,
          status: editingProject.value.status,
          // 进度字段需要根据实际API调整
        }

        if (editingProject.value.id) {
          // 更新项目
          await ProjectService.createOrUpdateProject(projectData)
          message.success('项目信息已更新')
        } else {
          // 添加新项目
          await ProjectService.createOrUpdateProject(projectData)
          message.success('新项目已添加')
        }
        
        showModal.value = false
        await fetchProjects()
      } catch (error) {
        console.error('保存项目时出错:', error)
        message.error('保存项目失败: ' + error.message)
      } finally {
        loading.value = false
      }
    } else {
      message.error('请检查表单填写是否正确')
    }
  })
}

// 获取项目列表
const fetchProjects = async () => {
  try {
    loading.value = true
    const data = await ProjectService.getAllProjects()
    projects.value = data.map(project => ({
      id: project.id,
      name: project.name,
      description: project.description,
      startDate: project.startTime,
      endDate: project.endTime,
      status: project.status || '进行中',
      progress: 0 // 需要根据实际数据计算
    }))
  } catch (error) {
    console.error('获取项目列表时出错:', error)
    message.error('获取项目列表失败: ' + error.message)
  } finally {
    loading.value = false
  }
}

// 组件挂载时获取项目数据
onMounted(() => {
  fetchProjects()
})
</script>

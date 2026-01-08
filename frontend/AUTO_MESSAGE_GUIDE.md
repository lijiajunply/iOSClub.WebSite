# 自动消息系统使用指南

## 概述

本系统提供了一种灵活的 API 响应消息显示方式，允许开发者选择是否自动显示成功/错误消息，或者手动控制消息显示。

## 核心特性

- ✅ **默认不显示消息** - 保持现有代码行为不变
- ✅ **按需启用** - 在需要的地方添加配置即可自动显示
- ✅ **细粒度控制** - 可以分别控制成功和错误消息
- ✅ **完全向后兼容** - 现有代码无需修改即可正常工作

## 使用方式

### 方式1: 默认行为（不自动显示消息）

```typescript
import { ArticleService } from '../services/ArticleService'

// 不显示任何消息，需要手动处理
try {
  await ArticleService.createArticle(dto)
} catch (error) {
  message.error('操作失败')
}
```

### 方式2: 开启所有消息

```typescript
import { ArticleService } from '../services/ArticleService'

// 自动显示成功和错误消息
await ArticleService.createArticle(dto, { showMessage: true })

// 相当于手动调用：
// message.success('操作成功')  或  message.error('错误消息')
```

### 方式3: 细粒度控制

```typescript
// 只显示成功消息
await ArticleService.createArticle(dto, { showSuccess: true })

// 只显示错误消息
await ArticleService.createArticle(dto, { showError: true })

// 同时控制成功和错误
await ArticleService.createArticle(dto, {
  showSuccess: true,
  showError: true
})
```

### 方式4: 自定义消息

```typescript
// 自定义成功和错误消息
await ArticleService.createArticle(dto, {
  showSuccess: true,
  successMessage: '文章发布成功！',
  showError: true,
  errorMessage: '发布失败，请重试'
})

// 如果不提供自定义消息，将使用后端返回的 message 字段
```

## 在服务层使用

### 修改服务方法支持自动消息

```typescript
import { apiRequest, ApiRequestConfig } from './ApiService'

export class ArticleService {
    static async createArticle(dto: CreateArticleDto, config?: ApiRequestConfig): Promise<ArticleModel> {
        return await apiRequest<ArticleModel>({
            url: `${url}/Article`,
            method: 'POST',
            requiresAuth: true,
            body: dto,
            // 添加消息配置
            showMessage: config?.showMessage ?? false,
            showSuccess: config?.showSuccess,
            showError: config?.showError,
            ...config
        });
    }
}
```

### 批量启用服务消息

```typescript
export class ArticleService {
    static async createArticle(dto: CreateArticleDto): Promise<ArticleModel> {
        return await apiRequest<ArticleModel>({
            url: `${url}/Article`,
            method: 'POST',
            requiresAuth: true,
            body: dto,
            showSuccess: true,  // 默认显示成功消息
            showError: true      // 默认显示错误消息
        });
    }

    static async updateArticle(dto: UpdateArticleDto): Promise<ArticleModel> {
        return await apiRequest<ArticleModel>({
            url: `${url}/Article/${dto.path}`,
            method: 'PUT',
            requiresAuth: true,
            body: dto,
            showSuccess: true,
            showError: true
        });
    }

    static async deleteArticle(path: string): Promise<void> {
        return await apiRequest<void>({
            url: `${url}/Article/${path}`,
            method: 'DELETE',
            requiresAuth: true,
            showSuccess: true,
            showError: true
        });
    }
}
```

## 在组件中使用

### 简化后的组件代码

**之前（需要手动处理消息）：**

```vue
<script setup lang="ts">
import { useMessage } from 'naive-ui'
import { ArticleService } from '../services/ArticleService'

const message = useMessage()

const createArticle = async () => {
  try {
    await ArticleService.createArticle(dto)
    message.success('已发布')
  } catch (error) {
    message.error('保存失败: ' + error.message)
  }
}
</script>
```

**现在（自动显示消息）：**

```vue
<script setup lang="ts">
import { ArticleService } from '../services/ArticleService'

const createArticle = async () => {
  // 自动显示成功/错误消息
  await ArticleService.createArticle(dto, {
    showMessage: true
  })
}
</script>
```

### 混合使用场景

```vue
<script setup lang="ts">
import { useMessage } from 'naive-ui'
import { ArticleService } from '../services/ArticleService'

const message = useMessage()

const handleCreate = async () => {
  // 使用自动消息
  await ArticleService.createArticle(dto, {
    showMessage: true
  })
}

const handleDelete = async () => {
  // 手动处理错误（例如：需要确认对话框）
  try {
    await ArticleService.deleteArticle(id, { showError: true })
    message.success('删除成功')
  } catch (error) {
    // 已经自动显示了错误消息，这里可以添加额外的逻辑
    await refreshList()
  }
}
</script>
```

## 迁移指南

### 渐进式迁移策略

1. **第一步：选择简单场景**
   - 优先对简单的 CRUD 操作启用自动消息
   - 保留复杂场景（如删除确认）的手动控制

2. **第二步：修改服务方法**
   - 在服务方法中添加默认的 `showSuccess: true`
   - 或者在组件调用时传递配置

3. **第三步：简化组件代码**
   - 移除组件中的 `message.success/error` 调用
   - 简化 try-catch 逻辑

4. **第四步：验证功能**
   - 测试成功和错误场景
   - 确认消息显示符合预期

### 完全迁移示例

**服务层（ArticleService.ts）：**

```typescript
export class ArticleService {
    // 修改前
    static async createArticle(dto: CreateArticleDto): Promise<ArticleModel> {
        return await apiRequest<ArticleModel>({
            url: `${url}/Article`,
            method: 'POST',
            requiresAuth: true,
            body: dto
        });
    }

    // 修改后
    static async createArticle(dto: CreateArticleDto): Promise<ArticleModel> {
        return await apiRequest<ArticleModel>({
            url: `${url}/Article`,
            method: 'POST',
            requiresAuth: true,
            body: dto,
            showSuccess: true,  // 添加这行
            showError: true      // 添加这行
        });
    }
}
```

**组件层（ArticleEditor.vue）：**

```vue
<script setup lang="ts">
// 修改前
import { useMessage } from 'naive-ui'
const message = useMessage()

const saveArticle = async () => {
  try {
    await ArticleService.createArticle(dto)
    message.success('已发布')
  } catch (error) {
    message.error('保存失败: ' + error.message)
  }
}

// 修改后
const saveArticle = async () => {
  await ArticleService.createArticle(dto)
  // 自动显示消息，无需手动处理
}
</script>
```

## 最佳实践

### 1. 读取操作（GET）不显示成功消息

```typescript
// ✅ 推荐：不显示成功消息，仅显示错误
await ArticleService.getAllArticles({ showError: true })

// ❌ 不推荐：显示成功消息会干扰用户体验
await ArticleService.getAllArticles({ showMessage: true })
```

### 2. 写操作（POST/PUT/DELETE）显示成功消息

```typescript
// ✅ 推荐：写操作显示成功消息
await ArticleService.createArticle(dto, { showMessage: true })
await ArticleService.updateArticle(dto, { showMessage: true })
await ArticleService.deleteArticle(id, { showMessage: true })
```

### 3. 批量操作显示加载提示

```typescript
import MessageService from './MessageService'

const loading = MessageService.loading('正在处理...')

try {
  await BatchService.processBatch(items)
  loading.destroy()  // 手动关闭加载提示
} catch (error) {
  loading.destroy()
}
```

### 4. 保留手动控制复杂场景

```typescript
// 需要确认对话框的场景 - 保留手动控制
const confirmDelete = async () => {
  dialog.warning({
    title: '确认删除',
    content: '删除后无法恢复，是否继续？',
    positiveText: '删除',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        await ArticleService.deleteArticle(id)
        message.success('删除成功')
        await refreshList()
      } catch (error) {
        // 错误已由 service 自动显示
      }
    }
  })
}
```

## API 参考

### MessageService

独立的消息服务，可在任何地方调用。

```typescript
import MessageService from './MessageService'

// 显示成功消息（3秒）
MessageService.success('操作成功')

// 显示错误消息（5秒）
MessageService.error('操作失败')

// 显示警告消息（4秒）
MessageService.warning('警告信息')

// 显示信息消息（3秒）
MessageService.info('提示信息')

// 显示加载消息（不自动消失）
const loading = MessageService.loading('加载中...')
loading.destroy()  // 手动关闭
```

### ApiRequestConfig

```typescript
interface ApiRequestConfig extends Omit<RequestInit, 'body'> {
    url: string;
    requiresAuth?: boolean;
    body?: any;
    showMessage?: boolean;      // 是否显示所有消息（默认 false）
    showSuccess?: boolean;     // 是否显示成功消息
    showError?: boolean;        // 是否显示错误消息
}
```

## 常见问题

### Q: 默认行为是什么？

A: 默认不显示任何消息，保持向后兼容。需要在调用时显式配置 `showMessage`、`showSuccess` 或 `showError`。

### Q: 可以在服务层设置默认值吗？

A: 可以。在服务方法中设置默认值，调用时不需要每次都传递：

```typescript
static async createArticle(dto: CreateArticleDto): Promise<ArticleModel> {
    return await apiRequest<ArticleModel>({
        url: `${url}/Article`,
        method: 'POST',
        requiresAuth: true,
        body: dto,
        showSuccess: true,  // 服务层默认值
        showError: true
    });
}
```

### Q: 后端返回的 message 字段会显示吗？

A: 会。如果配置了自动消息，系统会优先使用：
1. 自定义的消息（`successMessage` / `errorMessage`）
2. 后端返回的 `message` 字段
3. 默认消息（"操作成功" / "操作失败"）

### Q: 会影响现有的手动消息显示吗？

A: 不会。默认行为不变，现有代码无需修改。只有当显式配置了 `showMessage`、`showSuccess` 或 `showError` 时才会自动显示消息。

### Q: 如何禁用自动消息？

A: 不传递任何消息配置参数即可：

```typescript
// 不自动显示消息
await ArticleService.createArticle(dto)
```

## 总结

自动消息系统提供了灵活的方式来管理 API 响应消息：

- **简单场景**：使用 `showMessage: true` 自动处理所有消息
- **复杂场景**：继续使用手动控制（如删除确认对话框）
- **渐进迁移**：选择性地为某些操作启用自动消息

这样既减少了重复代码，又保持了足够的灵活性。

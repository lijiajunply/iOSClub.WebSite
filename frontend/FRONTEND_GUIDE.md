# IP 黑名单前端管理页面

## 📱 界面设计

### 苹果风格特性
- ✅ **简约设计**：采用 macOS/iOS 系统设计语言
- ✅ **圆角卡片**：使用 rounded-2xl 实现柔和视觉效果
- ✅ **柔和阴影**：subtle shadow 增强层次感
- ✅ **流畅动画**：transition-all duration-300 实现丝滑过渡
- ✅ **响应式布局**：完美适配桌面、平板、手机

### 暗黑模式支持
- 🌓 自动适配系统暗黑模式
- 🎨 深色背景 + 高对比度文字
- 📊 图表配色自适应
- 💡 实时切换无闪烁

---

## 🎯 功能模块

### 1. 统计概览
```
┌─────────────┬─────────────┬─────────────┐
│  精确 IP    │  CIDR 范围  │  拦截次数   │
│     5       │      2      │    42       │
└─────────────┴─────────────┴─────────────┘
```

**数据卡片**：
- 精确IP数量：显示黑名单中的单个IP数
- CIDR范围数量：显示IP段数量
- 拦截次数：显示成功拦截的请求数
- 图标动画：Hover 时图标放大 1.1 倍

### 2. 缓存性能图表

**ECharts 环形图**：
- 🟢 绿色：缓存命中
- 🔴 红色：缓存未命中
- 📊 显示命中率百分比
- 🎨 自动适应暗黑模式

### 3. 添加黑名单

**表单字段**：
- IP 地址或 CIDR：支持精确IP和IP段
  - 示例：`192.168.1.100`
  - 示例：`192.168.1.0/24`
- 封禁原因：可选，记录操作原因

**验证规则**：
- IP地址必填
- CIDR 格式自动校验

### 4. 移除黑名单

**表单字段**：
- IP 地址或 CIDR：输入要移除的IP
- 移除原因：可选，记录操作原因

### 5. IP检查

**功能**：
- 输入IP地址
- 实时检查是否在黑名单
- 显示检查结果和时间

**结果样式**：
- ✅ 绿色：不在黑名单
- ❌ 红色：已在黑名单

### 6. 操作按钮

- **刷新缓存**：手动刷新Redis缓存
- **重新加载统计**：刷新页面统计数据

---

## 🎨 颜色方案

### 亮色模式
```css
背景：白色 #FFFFFF
文字：深灰 #111827
卡片：白色 + 浅灰边框
阴影：柔和灰色阴影
```

### 暗黑模式
```css
背景：深灰 #1F2937
文字：白色 #FFFFFF
卡片：深灰 #374151 + 深边框
阴影：深色阴影
```

### 状态颜色
| 状态 | 颜色 | 用途 |
|------|------|------|
| 成功 | 绿色 #10B981 | 正常状态、成功操作 |
| 警告 | 橙色 #F59E0B | 警告信息 |
| 危险 | 红色 #EF4444 | 危险操作、错误 |
| 信息 | 蓝色 #3B82F6 | 信息提示 |
| 紫色 | #8B5CF6 | 图表、特殊功能 |

---

## 📐 布局结构

```
IP 黑名单管理
├── 页面标题
├── 统计概览（3列网格）
│   ├── 精确IP
│   ├── CIDR范围
│   └── 拦截次数
├── 缓存性能图表（ECharts）
├── 操作区域（2列网格）
│   ├── 添加黑名单
│   └── 移除黑名单
├── IP检查
└── 操作按钮组
    ├── 刷新缓存
    └── 重新加载统计
```

---

## 🚀 使用方法

### 访问页面
```
URL: /Centre/IpBlacklist
权限要求: Admin 或 Founder 角色
```

### 添加IP到黑名单
1. 在"添加黑名单"卡片中输入IP
2. 填写封禁原因（可选）
3. 点击"添加到黑名单"按钮
4. 成功后自动刷新统计数据

### 移除IP
1. 在"移除黑名单"卡片中输入IP
2. 填写移除原因（可选）
3. 点击"从黑名单移除"按钮
4. 成功后自动刷新统计数据

### 检查IP状态
1. 在"检查 IP"输入框输入IP地址
2. 点击"检查"按钮或按回车
3. 查看检查结果

### 刷新缓存
1. 点击"刷新缓存"按钮
2. 等待操作完成
3. 自动刷新统计数据

---

## 🎯 响应式断点

### 桌面 (lg: 1024px+)
- 统计卡片：3列网格
- 操作区域：2列网格
- 完整功能显示

### 平板 (sm: 640px - 1023px)
- 统计卡片：2列网格
- 操作区域：1列堆叠
- 图标和文字适当缩小

### 手机 (< 640px)
- 统计卡片：1列堆叠
- 操作区域：1列堆叠
- 圆角从 2xl 降至 xl
- 间距适当减小

---

## 💻 技术栈

### 核心框架
- **Vue 3.5.24**：Composition API + TypeScript
- **Naive UI 2.43.2**：UI 组件库
- **Tailwind CSS 4.x**：原子化CSS
- **ECharts 5.x**：数据可视化

### 工具库
- **@iconify/vue**：图标系统
- **@vueuse/core**：组合式工具集
- **TypeScript**：类型安全

### 图标
使用 Iconify 图标，无需导入 n-icon：
```vue
<Icon icon="mdi:shield-check" />
```

常用图标：
- `mdi:shield-check` - 安全检查
- `mdi:ip-network` - IP网络
- `mdi:chart-line` - 图表
- `mdi:plus-circle` - 添加
- `mdi:minus-circle` - 移除
- `mdi:magnify` - 搜索
- `mdi:refresh` - 刷新

---

## 📊 图表配置

### ECharts 环形图
```typescript
{
  type: 'pie',
  radius: ['40%', '70%'],  // 环形图内外半径
  itemStyle: {
    borderRadius: 10,      // 扇区圆角
    borderWidth: 2         // 扇区间隔
  },
  colors: ['#10b981', '#ef4444']  // 绿色/红色
}
```

### 响应式调整
- 监听窗口 resize 事件
- 自动调整图表大小
- 暗黑模式自动切换配色

---

## 🎭 动画效果

### 卡片 Hover
```css
transition: all 300ms ease
hover: shadow-md (阴影加深)
       scale-110 (图标放大)
```

### 按钮交互
```css
transition: all 200ms ease
hover: shadow-md
active: scale-95
```

### 加载状态
- Skeleton 骨架屏
- 脉冲动画 (animate-pulse)
- Loading spinner

---

## ⚡ 性能优化

### 1. 懒加载
- 路由组件懒加载
- 图表按需引入

### 2. 防抖节流
- 输入框防抖
- 窗口 resize 节流

### 3. 缓存策略
- 统计数据缓存
- 避免重复请求

---

## 🐛 错误处理

### API 错误
```typescript
try {
  await IpBlacklistService.addToBlacklist(data);
  message.success('添加成功');
} catch (error: any) {
  message.error(error.message || '操作失败');
}
```

### 表单验证
- 必填项验证
- 格式校验
- 实时反馈

---

## 📱 界面截图说明

### 亮色模式
- 纯白背景
- 柔和阴影
- 清晰文字
- 简洁图标

### 暗黑模式
- 深色背景
- 高对比文字
- 柔和光晕
- 护眼配色

---

## 🔧 开发指南

### 本地运行
```bash
cd frontend
pnpm install
pnpm dev
```

### 访问页面
```
http://localhost:5173/Centre/IpBlacklist
```

### 构建生产
```bash
pnpm build
```

---

## 📝 文件结构

```
frontend/src/
├── services/
│   └── IpBlacklistService.ts       # IP黑名单服务
├── adminPages/
│   └── IpBlacklist.vue             # 管理页面
└── router.ts                        # 路由配置
```

---

## 🎨 样式定制

### 自定义卡片样式
```css
.apple-card {
  @apply bg-white dark:bg-gray-800
         rounded-2xl shadow-sm hover:shadow-md
         transition-all duration-300
         border border-gray-100 dark:border-gray-700;
}
```

### 自定义按钮样式
```css
.apple-button {
  @apply rounded-xl font-medium
         transition-all duration-200 shadow-sm;
}
```

---

## 🌐 API 集成

### 服务方法
```typescript
IpBlacklistService.getStats()           // 获取统计
IpBlacklistService.addToBlacklist()     // 添加IP
IpBlacklistService.removeFromBlacklist() // 移除IP
IpBlacklistService.checkIp()            // 检查IP
IpBlacklistService.refreshBlacklist()   // 刷新缓存
```

### 返回格式
```typescript
{
  code: 200,
  errorCode: 0,
  message: "成功",
  data: { /* 数据 */ }
}
```

---

## 🎓 最佳实践

1. **保持简洁**：避免过度设计
2. **响应式优先**：移动端体验同样重要
3. **暗黑模式适配**：确保所有元素可读
4. **错误提示**：友好的错误信息
5. **加载状态**：明确的加载反馈
6. **性能优化**：避免不必要的重渲染

---

## 📞 支持

如有问题或建议：
- 前端代码：[IpBlacklist.vue](src/adminPages/IpBlacklist.vue)
- 服务代码：[IpBlacklistService.ts](src/services/IpBlacklistService.ts)
- 路由配置：[router.ts](src/router.ts)

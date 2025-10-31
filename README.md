# 西建大iOS Club社团官网重制版

## 项目简介

本项目是西安建筑科技大学iOS Club社团官网的重制版，旨在提供一个现代化、功能完善的社团展示和管理平台。项目采用前后端分离架构，前端使用Vue.js/TypeScript构建，后端使用ASP.NET WebAPI提供数据支持。

## 技术栈

### 前端
- Vue.js 3
- TypeScript
- Vite (构建工具)
- Vue Router (路由管理)
- Pinia (状态管理)
- Vue Components (组件库)

### 后端
- ASP.NET WebAPI
- Entity Framework Core
- SQL Server

## 项目结构

```
iOS-Club-Website/
├── frontend/          # 前端项目目录
│   ├── src/           # 前端源码
│   ├── public/        # 静态资源
│   ├── package.json   # 前端依赖配置
│   └── vite.config.js # Vite配置文件
├── rearend/           # 后端项目目录
│   └── iOSClub.WebAPI/ # ASP.NET WebAPI项目
└── README.md          # 项目说明文档
```

## 功能特性

### 前端功能
- 社团介绍与展示
- 活动公告与报名
- 成员展示与介绍
- 作品展示
- 招新信息发布
- 后台管理系统

### 后端功能
- 用户认证与授权
- 数据管理API
- 活动报名处理
- 文件上传下载

## 开发环境要求

### 前端
- Node.js 16.x 或更高版本
- npm 8.x 或更高版本
- VS Code 或其他支持Vue.js的IDE

### 后端
- .NET 6.0 SDK 或更高版本
- Visual Studio 2022 或其他支持ASP.NET的IDE
- SQL Server 2019 或更高版本

## 安装与运行

### 前端安装

1. 进入前端目录
```bash
cd frontend
```

2. 安装依赖
```bash
npm install
```

3. 启动开发服务器
```bash
npm run dev
```

### 后端安装

1. 使用Visual Studio打开`rearend/iOSClub.WebAPI/iOSClub.WebAPI.sln`解决方案

2. 还原NuGet包

3. 更新数据库连接字符串（在appsettings.json中）

4. 运行数据库迁移
```bash
dotnet ef database update
```

5. 启动后端服务

## 部署

### 前端部署

1. 构建生产版本
```bash
npm run build
```

2. 将构建产物部署到Web服务器（如Nginx、Apache等）

### 后端部署

1. 发布ASP.NET WebAPI项目

2. 部署到IIS或其他支持.NET的Web服务器

3. 配置数据库连接

## 贡献指南

1. Fork本仓库

2. 创建你的功能分支
```bash
git checkout -b feature/amazing-feature
```

3. 提交你的更改
```bash
git commit -m 'Add some amazing feature'
```

4. 推送到分支
```bash
git push origin feature/amazing-feature
```

5. 开启一个Pull Request

## 许可证

本项目采用MIT许可证 - 详情请查看[LICENSE](LICENSE)文件

## 联系方式

如有问题或建议，请联系：
- 项目负责人：[负责人姓名]
- 社团邮箱：[社团邮箱地址]
- GitHub Issues：[项目Issues页面]

---

© 2024 西建大iOS Club. 保留所有权利。
# iOS Club 社团官网后端

## 项目简介

本项目是西安建筑科技大学iOS Club社团官网的后端部分，采用ASP.NET WebAPI构建，提供了完整的数据管理和API服务。

## 技术栈

- ASP.NET WebAPI
- Entity Framework Core
- CQRS设计模式
- PostgreSQL (推荐，需安装中文插件，用于主要业务数据)
- SQLite (用于日志存储)
- JWT认证
- FluentValidation (数据验证)

## 项目结构

```
iOSClub.WebAPI/
├── iOSClub.Data/         # 数据访问层
│   ├── DataModels/       # 数据模型
│   ├── Migrations/       # 数据库迁移
│   └── ClubContext.cs    # 数据库上下文
├── iOSClub.DataApi/      # 业务逻辑层
│   ├── CQRS/             # CQRS模式实现
│   │   ├── Commands/     # 命令处理
│   │   ├── Queries/      # 查询处理
│   │   └── Handlers/     # 命令/查询处理器
│   ├── Repositories/     # 数据访问仓库
│   └── Services/         # 业务服务
├── iOSClub.Tests/        # 测试项目
│   ├── IntegrationTests/ # 集成测试
│   ├── RepositoryTests/  # 仓库测试
│   └── ServiceTests/     # 服务测试
└── iOSClub.WebAPI/       # WebAPI项目
    ├── Controllers/      # API控制器
    ├── Middleware/       # 中间件
    └── Program.cs        # 应用入口
```

## 架构设计

本项目采用了分层架构和CQRS设计模式：

1. **数据访问层 (iOSClub.Data)**
   - 负责数据库连接和数据模型定义
   - 使用Entity Framework Core进行数据访问
   - 包含数据库迁移和上下文管理

2. **业务逻辑层 (iOSClub.DataApi)**
   - 实现CQRS模式，分离命令和查询
   - 命令处理：负责数据的创建、更新和删除
   - 查询处理：负责数据的读取和查询
   - 包含业务规则验证和服务实现

3. **API层 (iOSClub.WebAPI)**
   - 提供RESTful API接口
   - 处理HTTP请求和响应
   - 包含认证授权、中间件和路由配置

## 核心功能

### 认证与授权
- JWT令牌生成与验证
- 角色基于的权限控制
- 安全的密码存储

### 数据管理
- 文章管理 (CRUD操作)
- 分类管理
- 成员管理
- 项目管理
- 资源管理
- 客户端应用管理

### 业务功能
- 数据统计与分析
- 邮件发送服务
- 数据访问日志

## 开发命令

### 还原依赖
```bash
dotnet restore
```

### 运行项目
```bash
dotnet run --project iOSClub.WebAPI.csproj
```

### 运行测试
```bash
dotnet test
```

### 数据库迁移
```bash
dotnet ef database update --project iOSClub.Data --startup-project iOSClub.WebAPI
```

## 部署

1. 发布WebAPI项目：
```bash
dotnet publish -c Release
```

2. 部署到IIS或其他支持.NET的Web服务器

3. 配置数据库连接字符串

## 贡献指南

1. 确保安装了.NET 10 SDK
2. 克隆仓库并还原依赖
3. 创建功能分支
4. 开发功能并编写测试
5. 运行测试确保通过
6. 提交代码并创建Pull Request

## 许可证

MIT License
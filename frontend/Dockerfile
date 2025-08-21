# 前端专用 Dockerfile - 适用于 Zeabur 部署

# 阶段 1: 构建环境
FROM node:22 AS build
WORKDIR /app

# STEP 1: 优先复制关键配置文件
COPY .npmrc package.json package-lock.json ./

# STEP 2: 显式设置淘宝镜像源（双重保障）
RUN npm config set registry https://registry.npmmirror.com \
    && npm config set progress false \
    && npm config set audit false \
    && npm config set fund false

# STEP 3: 升级 npm（可选）
RUN npm update -g npm

# STEP 4: ✅ 修复后的安装命令（使用 --prefer-offline）
RUN npm install --verbose --prefer-offline --no-fund --no-audit

# STEP 5: 复制其余源代码
COPY . .

# STEP 6: 构建生产版本
RUN npm run build

# 阶段 2: 运行环境
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s \
  CMD wget --spider http://localhost/ || exit 1
# 阶段 1: 构建环境
FROM node:22 AS build
WORKDIR /app

# STEP 1: 优先复制关键配置文件
COPY .npmrc package.json package-lock.json ./

# STEP 2: 显式设置淘宝镜像源
RUN npm config set registry https://registry.npmmirror.com \
    && npm config set progress false \
    && npm config set audit false \
    && npm config set fund false

# STEP 3: 升级 npm
RUN npm update -g npm

# STEP 4: 安装重试工具
RUN npm install -g retry-cli

# STEP 5: 使用重试机制安装依赖（✅ 修正选项）
RUN npx retry-cli --retries 3 --delay 10000 npm install --verbose --no-fund --no-audit

# STEP 6: 复制其余源代码
COPY . .

# STEP 7: 构建生产版本
RUN npm run build

# 阶段 2: 运行环境
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
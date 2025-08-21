FROM node:22 AS build
WORKDIR /app

COPY .npmrc package.json package-lock.json* ./

RUN npm config set registry https://registry.npmmirror.com \
    && npm config set progress false \
    && npm config set audit false \
    && npm config set fund false

RUN npm update -g npm

RUN npm install --verbose --network-concurrency=1 --no-fund --no-audit

COPY . .

RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s \
  CMD wget --spider http://localhost/ || exit 1
# Build context is the repository root (see docker-compose.yml).
FROM node:22-alpine AS build
WORKDIR /app

# Install first so packages are cached until package.json or the lock file changes.
COPY src/Web/package.json src/Web/package-lock.json ./
RUN npm ci

COPY src/Web/ .
RUN npm run build

FROM nginx:stable-alpine AS final
COPY src/Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80

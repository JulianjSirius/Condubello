# Contexto de construcción: raíz del repositorio
FROM node:22-alpine AS build
WORKDIR /web
COPY Condubello.Web/package*.json ./
RUN npm ci --no-audit --no-fund
COPY Condubello.Web/ ./
RUN npx ng build --configuration production

FROM nginx:1.27-alpine
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /web/dist/Condubello.Web/browser /usr/share/nginx/html
EXPOSE 80

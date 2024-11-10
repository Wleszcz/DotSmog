FROM node:21-alpine AS builder
RUN npm install -g @angular/cli@18.2.0
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build --configuration=production

FROM nginx:alpine
COPY --from=builder /app/dist/dot-smog-ng/browser /usr/share/nginx/html

EXPOSE 80

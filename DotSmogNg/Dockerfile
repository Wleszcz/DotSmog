FROM node:latest as build
WORKDIR /app
COPY package*.json ./
RUN npm install
RUN npm install -g @angular/cli
COPY . .
RUN npm run build --configuration=production

FROM nginx:alpine
COPY --from=build /app/dist/dot-smog-ng/browser /usr/share/nginx/html

EXPOSE 80

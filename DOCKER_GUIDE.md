# 🐳 K-POP FLAC Player - Docker Deployment Guide

## 🚀 Быстрый старт с Docker

### Способ 1: Docker Compose (РЕКОМЕНДУЕТСЯ)

```bash
# 1. Распакуйте проект
unzip kpop-flac-player-full.zip

# 2. Добавьте музыку
mkdir music
cp ~/Music/*.flac music/
# Переименуйте файлы согласно README

# 3. Запустите всё одной командой
docker-compose up -d

# 4. Откройте браузер
# http://localhost:3000
```

**Готово!** 🎉 Frontend и Backend запущены в контейнерах.

---

## 📦 Что включено

Docker Compose запускает:
- ✅ **Backend** (ASP.NET Core) на порту 5000
- ✅ **Frontend** (Next.js) на порту 3000
- ✅ **Network** для связи между контейнерами
- ✅ **Health checks** для мониторинга
- ✅ **Auto-restart** при падении

---

## 🛠️ Управление контейнерами

### Запуск
```bash
# Запустить в фоновом режиме
docker-compose up -d

# Запустить с логами
docker-compose up

# Пересобрать и запустить
docker-compose up -d --build
```

### Остановка
```bash
# Остановить контейнеры
docker-compose stop

# Остановить и удалить
docker-compose down

# Остановить и удалить с volumes
docker-compose down -v
```

### Логи
```bash
# Все логи
docker-compose logs

# Следить за логами в реальном времени
docker-compose logs -f

# Логи конкретного сервиса
docker-compose logs -f backend
docker-compose logs -f frontend
```

### Перезапуск
```bash
# Перезапустить всё
docker-compose restart

# Перезапустить конкретный сервис
docker-compose restart backend
docker-compose restart frontend
```

### Статус
```bash
# Проверить статус
docker-compose ps

# Проверить здоровье
docker-compose ps --all
```

---

## 📁 Структура проекта для Docker

```
kpop-flac-player-full/
├── docker-compose.yml          # Главная конфигурация
├── music/                      # 📁 Ваши FLAC файлы здесь
│   ├── dynamite.flac
│   ├── hylt.flac
│   └── ...
├── kpop-backend/
│   ├── Dockerfile
│   ├── Program.cs
│   └── ...
└── kpop-player/
    ├── Dockerfile
    ├── .dockerignore
    ├── next.config.js
    └── ...
```

---

## 🔧 Конфигурация

### Изменение портов

Отредактируйте `docker-compose.yml`:

```yaml
services:
  backend:
    ports:
      - "8080:5000"  # Изменить 8080 на нужный внешний порт
  
  frontend:
    ports:
      - "8000:3000"  # Изменить 8000 на нужный внешний порт
```

### Environment Variables

```yaml
services:
  backend:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      # Добавьте свои переменные
  
  frontend:
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:5000
      - NODE_ENV=production
      # Добавьте свои переменные
```

### Увеличение лимитов памяти

```yaml
services:
  backend:
    deploy:
      resources:
        limits:
          memory: 512M
        reservations:
          memory: 256M
```

---

## 📊 Мониторинг

### Health Checks

Проверьте здоровье сервисов:

```bash
# Backend
curl http://localhost:5000/api/health

# Frontend
curl http://localhost:3000
```

### Docker Stats

```bash
# Реалтайм статистика контейнеров
docker stats

# Статистика конкретного контейнера
docker stats kpop-backend
docker stats kpop-frontend
```

### Логи ошибок

```bash
# Последние 100 строк логов backend
docker-compose logs --tail=100 backend

# Логи с временными метками
docker-compose logs -t frontend
```

---

## 🔒 Безопасность

### Запуск от non-root пользователя

Уже настроено в Dockerfile:

**Backend:**
```dockerfile
RUN useradd -m appuser && chown -R appuser /app
USER appuser
```

**Frontend:**
```dockerfile
RUN adduser --system --uid 1001 nextjs
USER nextjs
```

### Read-only музыка

```yaml
volumes:
  - ./music:/app/music:ro  # :ro = read-only
```

### Network изоляция

Контейнеры изолированы в отдельной сети `kpop-network`.

---

## 🚀 Production Deployment

### С SSL/TLS (HTTPS)

Добавьте Nginx reverse proxy:

```yaml
# docker-compose.yml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - frontend
      - backend
    networks:
      - kpop-network
```

Пример `nginx.conf`:

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    location / {
        proxy_pass http://frontend:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    location /api/ {
        proxy_pass http://backend:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Использование Docker Hub

```bash
# Создать образы
docker-compose build

# Тегировать
docker tag kpop-backend yourusername/kpop-backend:latest
docker tag kpop-frontend yourusername/kpop-frontend:latest

# Загрузить на Docker Hub
docker push yourusername/kpop-backend:latest
docker push yourusername/kpop-frontend:latest
```

Затем на сервере:

```yaml
# docker-compose.yml
services:
  backend:
    image: yourusername/kpop-backend:latest
  
  frontend:
    image: yourusername/kpop-frontend:latest
```

---

## 🔄 Обновление

### Обновить код

```bash
# Остановить контейнеры
docker-compose down

# Обновить код (git pull или замените файлы)

# Пересобрать и запустить
docker-compose up -d --build
```

### Обновить только Frontend

```bash
docker-compose up -d --build frontend
```

### Обновить только Backend

```bash
docker-compose up -d --build backend
```

---

## 🐛 Troubleshooting

### Контейнер не запускается

```bash
# Проверьте логи
docker-compose logs backend
docker-compose logs frontend

# Проверьте статус
docker-compose ps
```

### Порт занят

```bash
# Найти процесс на порту
lsof -i :3000  # macOS/Linux
netstat -ano | findstr :3000  # Windows

# Или измените порт в docker-compose.yml
```

### Frontend не может подключиться к Backend

Проверьте переменные окружения:

```yaml
frontend:
  environment:
    - BACKEND_URL=http://backend:5000  # Используйте имя сервиса, не localhost
```

### Музыка не играет

```bash
# Проверьте volume mapping
docker-compose exec backend ls -la /app/music

# Проверьте права доступа
chmod -R 755 ./music
```

### Не хватает памяти

```bash
# Проверьте использование
docker stats

# Увеличьте лимиты в docker-compose.yml
```

### Очистка Docker

```bash
# Удалить неиспользуемые образы
docker image prune -a

# Удалить неиспользуемые volumes
docker volume prune

# Полная очистка
docker system prune -a --volumes
```

---

## 🎯 Полезные команды

### Вход в контейнер

```bash
# Backend
docker-compose exec backend /bin/bash

# Frontend
docker-compose exec frontend /bin/sh
```

### Проверка сети

```bash
# Список сетей
docker network ls

# Информация о сети
docker network inspect kpop-flac-player-full_kpop-network
```

### Экспорт/Импорт образов

```bash
# Сохранить образ
docker save -o kpop-backend.tar kpop-backend
docker save -o kpop-frontend.tar kpop-frontend

# Загрузить образ
docker load -i kpop-backend.tar
docker load -i kpop-frontend.tar
```

---

## 📈 Масштабирование

### Несколько реплик Backend

```yaml
services:
  backend:
    deploy:
      replicas: 3  # 3 инстанса backend
```

### С балансировкой нагрузки

```yaml
services:
  nginx-lb:
    image: nginx:alpine
    volumes:
      - ./nginx-lb.conf:/etc/nginx/nginx.conf
    depends_on:
      - backend
    scale: 1
  
  backend:
    scale: 3
```

---

## 🌍 CI/CD Integration

### GitHub Actions

```yaml
# .github/workflows/docker.yml
name: Docker Build and Push

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Build images
        run: docker-compose build
      
      - name: Push to registry
        run: |
          docker-compose push
```

---

## 📝 Best Practices

1. **Всегда используйте specific versions** вместо `latest`
2. **Multi-stage builds** для меньшего размера образов (уже реализовано)
3. **Health checks** для auto-recovery (уже настроено)
4. **Non-root users** для безопасности (уже реализовано)
5. **Read-only volumes** где возможно
6. **Ограничения ресурсов** в production

---

## 🎉 Готово!

Теперь у вас есть полностью контейнеризованное приложение!

```bash
# Запустить всё
docker-compose up -d

# Проверить статус
docker-compose ps

# Открыть в браузере
http://localhost:3000
```

**Наслаждайтесь K-POP музыкой в Docker! 🎵🐳**

---

## 📚 Дополнительные ресурсы

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Next.js Docker Documentation](https://nextjs.org/docs/deployment#docker-image)
- [ASP.NET Core Docker Documentation](https://docs.microsoft.com/aspnet/core/host-and-deploy/docker/)

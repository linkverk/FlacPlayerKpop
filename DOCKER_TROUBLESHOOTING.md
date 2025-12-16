# 🐛 Docker Build - Решение проблем

## ❌ Ошибка: "npm ci" failed

### Проблема
```
RUN npm ci
process "/bin/sh -c npm ci" did not complete successfully: exit code: 1
```

### Решение

#### Вариант 1: Сгенерировать package-lock.json

```bash
# Перейдите в папку frontend
cd kpop-player

# Сгенерируйте package-lock.json
npm install --package-lock-only

# Пересоберите Docker
cd ..
docker-compose build
```

#### Вариант 2: Использовать npm install

Dockerfile уже настроен на автоматическую обработку! Он проверяет наличие package-lock.json:

```dockerfile
RUN if [ -f package-lock.json ]; then npm ci; else npm install; fi
```

Если проблема всё равно есть, попробуйте очистить кеш:

```bash
# Очистите Docker кеш
docker system prune -a

# Пересоберите
docker-compose build --no-cache
```

---

## ❌ Ошибка: Port already in use

### Проблема
```
Error: bind: address already in use
```

### Решение

```bash
# Найдите процесс на порту
lsof -i :3000  # macOS/Linux
netstat -ano | findstr :3000  # Windows

# Остановите процесс
kill -9 <PID>

# Или измените порт в docker-compose.yml
ports:
  - "3001:3000"  # Внешний:Внутренний
```

---

## ❌ Ошибка: Cannot find module

### Проблема
```
Error: Cannot find module 'next'
```

### Решение

```bash
# Удалите node_modules
rm -rf kpop-player/node_modules

# Пересоберите образ
docker-compose build --no-cache frontend
```

---

## ❌ Ошибка: EACCES permission denied

### Проблема
```
npm error EACCES: permission denied
```

### Решение

```bash
# Исправьте права
sudo chown -R $USER:$USER kpop-player

# Или запустите с sudo (не рекомендуется)
sudo docker-compose up
```

---

## ❌ Ошибка: Network timeout

### Проблема
```
npm error network timeout
```

### Решение

```bash
# Увеличьте таймаут в Dockerfile
RUN npm install --network-timeout=600000

# Или используйте другой registry
RUN npm config set registry https://registry.npmmirror.com
RUN npm install
```

---

## 🔧 Общие команды для отладки

### Очистка Docker

```bash
# Остановить все контейнеры
docker-compose down

# Удалить образы
docker-compose down --rmi all

# Полная очистка
docker system prune -a --volumes

# Пересоздать всё с нуля
docker-compose build --no-cache
docker-compose up -d
```

### Логи

```bash
# Посмотреть логи сборки
docker-compose build frontend 2>&1 | tee build.log

# Логи контейнера
docker-compose logs frontend
docker-compose logs backend

# Следить за логами
docker-compose logs -f
```

### Вход в контейнер

```bash
# Войти в контейнер frontend
docker-compose exec frontend sh

# Проверить файлы
ls -la
cat package.json

# Попробовать установить вручную
npm install
```

---

## ✅ Проверка перед запуском

### Чек-лист

- [ ] Docker установлен и запущен
- [ ] Порты 3000 и 5000 свободны
- [ ] `package.json` существует в `kpop-player/`
- [ ] `package-lock.json` существует (или будет создан автоматически)
- [ ] Интернет соединение стабильно
- [ ] Достаточно места на диске (минимум 2GB)

### Команды проверки

```bash
# Проверить Docker
docker --version
docker-compose --version

# Проверить порты
lsof -i :3000
lsof -i :5000

# Проверить файлы
ls -la kpop-player/package.json
ls -la kpop-player/package-lock.json

# Проверить место
df -h
```

---

## 🚀 Альтернативный запуск (без Docker)

Если Docker не работает, запустите без контейнеров:

### Frontend

```bash
cd kpop-player
npm install
npm run dev
```

### Backend

```bash
cd kpop-backend
dotnet run
```

Откройте http://localhost:3000

---

## 📞 Помощь

Если проблема не решается:

1. Проверьте логи: `docker-compose logs`
2. Попробуйте запустить без Docker
3. Проверьте требования системы
4. Убедитесь что все файлы распакованы правильно

---

## 💡 Полезные ссылки

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Troubleshooting Docker](https://docs.docker.com/config/daemon/troubleshoot/)

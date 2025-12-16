.PHONY: help up down restart logs build clean status

# Default target
help:
	@echo "🎵 K-POP FLAC Player - Docker Commands"
	@echo ""
	@echo "Usage:"
	@echo "  make up          - Запустить все сервисы"
	@echo "  make down        - Остановить все сервисы"
	@echo "  make restart     - Перезапустить все сервисы"
	@echo "  make logs        - Показать логи"
	@echo "  make logs-f      - Следить за логами"
	@echo "  make build       - Пересобрать образы"
	@echo "  make clean       - Удалить контейнеры и образы"
	@echo "  make status      - Проверить статус"
	@echo "  make backend-logs - Логи backend"
	@echo "  make frontend-logs - Логи frontend"
	@echo "  make shell-backend - Войти в backend контейнер"
	@echo "  make shell-frontend - Войти в frontend контейнер"

# Start all services
up:
	docker-compose up -d
	@echo "✅ Сервисы запущены!"
	@echo "🌐 Frontend: http://localhost:3000"
	@echo "🔧 Backend: http://localhost:5000"

# Stop all services
down:
	docker-compose down
	@echo "✅ Сервисы остановлены!"

# Restart all services
restart:
	docker-compose restart
	@echo "✅ Сервисы перезапущены!"

# View logs
logs:
	docker-compose logs

# Follow logs
logs-f:
	docker-compose logs -f

# Backend logs
backend-logs:
	docker-compose logs -f backend

# Frontend logs
frontend-logs:
	docker-compose logs -f frontend

# Build images
build:
	docker-compose build
	@echo "✅ Образы собраны!"

# Clean everything
clean:
	docker-compose down -v
	docker system prune -f
	@echo "✅ Очистка завершена!"

# Check status
status:
	docker-compose ps

# Shell into backend
shell-backend:
	docker-compose exec backend /bin/bash

# Shell into frontend
shell-frontend:
	docker-compose exec frontend /bin/sh

# Rebuild and restart
rebuild: down build up
	@echo "✅ Пересборка и перезапуск завершены!"

# View stats
stats:
	docker stats

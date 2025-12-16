#!/bin/bash

# K-POP FLAC Player - Docker Quick Start Script

echo "╔════════════════════════════════════════════════════╗"
echo "║   🎵 K-POP FLAC Player - Docker Quick Start      ║"
echo "╚════════════════════════════════════════════════════╝"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker не установлен!"
    echo "   Установите Docker: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose не установлен!"
    echo "   Установите Docker Compose: https://docs.docker.com/compose/install/"
    exit 1
fi

echo "✅ Docker установлен"
echo "✅ Docker Compose установлен"
echo ""

# Check if music directory exists
if [ ! -d "music" ]; then
    echo "📁 Создание папки для музыки..."
    mkdir -p music
    echo "⚠️  Добавьте FLAC файлы в папку music/"
    echo "   - dynamite.flac"
    echo "   - hylt.flac"
    echo "   - nextlevel.flac"
    echo "   - butter.flac"
    echo "   - eleven.flac"
    echo "   - savage.flac"
    echo "   - pinkvenom.flac"
    echo "   - spicy.flac"
    echo ""
fi

# Check if music files exist
MUSIC_COUNT=$(find music -name "*.flac" 2>/dev/null | wc -l)
if [ "$MUSIC_COUNT" -eq 0 ]; then
    echo "⚠️  Папка music/ пуста!"
    echo "   Добавьте FLAC файлы перед запуском"
    read -p "   Продолжить без музыки? (y/n) " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    echo "✅ Найдено $MUSIC_COUNT FLAC файлов"
fi

echo ""
echo "🚀 Запуск Docker контейнеров..."
echo ""

# Build and start containers
docker-compose up -d --build

# Check if containers started successfully
if [ $? -eq 0 ]; then
    echo ""
    echo "╔════════════════════════════════════════════════════╗"
    echo "║            🎉 Успешно запущено!                   ║"
    echo "╚════════════════════════════════════════════════════╝"
    echo ""
    echo "🌐 Frontend:  http://localhost:3000"
    echo "🔧 Backend:   http://localhost:5000"
    echo "📊 API Docs:  http://localhost:5000/api/music"
    echo ""
    echo "📝 Полезные команды:"
    echo "   docker-compose logs -f        - Просмотр логов"
    echo "   docker-compose ps             - Статус контейнеров"
    echo "   docker-compose down           - Остановить"
    echo "   docker-compose restart        - Перезапустить"
    echo ""
    echo "   или используйте: make help"
    echo ""
    echo "🎵 Откройте http://localhost:3000 в браузере!"
    echo ""
else
    echo ""
    echo "❌ Ошибка при запуске контейнеров"
    echo "   Проверьте логи: docker-compose logs"
    exit 1
fi

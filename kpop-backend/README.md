# 🎵 K-POP FLAC Music Server - ASP.NET Core Backend

Мощный backend API для стриминга высококачественной музыки в формате FLAC, построенный на ASP.NET Core 8.0.

## ✨ Возможности

- 🌊 **Потоковое воспроизведение** с поддержкой Range requests
- 🔍 **Поиск** по названию и исполнителю
- 📊 **Статистика** библиотеки
- 👥 **Группировка** по артистам
- 🎚️ **Форматы** аудио
- ⬇️ **Скачивание** треков
- 🔒 **Безопасность** - path validation
- 🌐 **CORS** настроен для фронтенда
- 📝 **Типизация** с моделями
- ⚡ **Производительность** с буферизацией

## 🚀 Быстрый старт

### Требования

- .NET 8.0 SDK
- Windows/Linux/macOS

### Установка и запуск

```bash
# Перейдите в папку проекта
cd kpop-backend

# Восстановите зависимости
dotnet restore

# Запустите сервер
dotnet run
```

Сервер запустится на **http://localhost:5000**

### Добавление музыки

```bash
# Создайте папку для музыки (создастся автоматически)
mkdir music

# Скопируйте FLAC файлы
# Переименуйте согласно библиотеке:
cp ~/Music/song1.flac music/dynamite.flac
cp ~/Music/song2.flac music/hylt.flac
# и т.д.
```

## 📁 Структура проекта

```
kpop-backend/
├── Program.cs                      # Основной файл с API endpoints
├── KPopFlacMusicServer.csproj     # Конфигурация проекта
├── appsettings.json               # Настройки приложения
├── Models/
│   └── Models.cs                  # Модели данных
├── music/                         # 📁 Папка для FLAC файлов
└── README.md
```

## 🌐 API Endpoints

### 📚 Основные endpoints

#### `GET /`
Информация о сервере

**Response:**
```json
{
  "message": "K-POP FLAC Music Server (ASP.NET Core)",
  "version": "2.0.0",
  "status": "online",
  "endpoints": { ... },
  "musicDirectory": "/path/to/music",
  "serverTime": "2024-12-16T12:00:00Z"
}
```

#### `GET /api/music`
Список всех треков

**Response:**
```json
{
  "success": true,
  "tracks": [
    {
      "id": 1,
      "title": "Dynamite",
      "artist": "BTS",
      "filename": "dynamite.flac",
      "format": "FLAC 24bit/96kHz",
      "emoji": "💥",
      "duration": 199
    }
  ],
  "availableCount": 8,
  "totalCount": 8,
  "timestamp": "2024-12-16T12:00:00Z"
}
```

#### `GET /api/stream/{filename}`
Стриминг аудио файла

**Headers:**
- `Range: bytes=0-1024` - для частичной загрузки (поддерживается)

**Response:**
- `Content-Type: audio/flac`
- `Accept-Ranges: bytes`
- `Content-Range: bytes 0-1024/5242880` (при Range request)

#### `GET /api/track/{id}`
Детальная информация о треке

**Response:**
```json
{
  "id": 1,
  "title": "Dynamite",
  "artist": "BTS",
  "filename": "dynamite.flac",
  "format": "FLAC 24bit/96kHz",
  "emoji": "💥",
  "duration": 199,
  "available": true,
  "fileSize": 52428800,
  "lastModified": "2024-12-16T12:00:00Z",
  "streamUrl": "/api/stream/dynamite.flac"
}
```

### 🔍 Поиск и фильтрация

#### `GET /api/search?q={query}`
Поиск треков

**Example:** `/api/search?q=bts`

**Response:**
```json
{
  "success": true,
  "query": "bts",
  "count": 2,
  "results": [...]
}
```

#### `GET /api/artists`
Список всех артистов

**Response:**
```json
{
  "success": true,
  "count": 3,
  "artists": [
    {
      "name": "BTS",
      "trackCount": 2,
      "tracks": [
        { "id": 1, "title": "Dynamite" },
        { "id": 4, "title": "Butter" }
      ]
    }
  ]
}
```

#### `GET /api/formats`
Информация о форматах

**Response:**
```json
{
  "success": true,
  "formats": [
    {
      "format": "FLAC 24bit/96kHz",
      "count": 8
    }
  ]
}
```

### 📊 Статистика

#### `GET /api/stats`
Статистика библиотеки

**Response:**
```json
{
  "success": true,
  "stats": {
    "totalTracks": 8,
    "availableTracks": 8,
    "unavailableTracks": 0,
    "totalDurationSeconds": 1550,
    "totalDurationFormatted": "00:25:50",
    "uniqueArtists": 3,
    "averageTrackDuration": 193
  }
}
```

### ⬇️ Дополнительные endpoints

#### `GET /api/download/{filename}`
Скачивание файла (не стриминг)

**Response:**
- File download with `Content-Disposition: attachment`

#### `GET /api/health`
Health check endpoint

**Response:**
```json
{
  "status": "healthy",
  "uptime": "2024-12-16T12:00:00Z",
  "version": "2.0.0"
}
```

## 🔧 Конфигурация

### appsettings.json

```json
{
  "MusicSettings": {
    "MusicDirectory": "music",
    "MaxFileSize": 104857600,
    "AllowedExtensions": [".flac", ".mp3", ".wav"],
    "BufferSize": 81920
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  }
}
```

### Изменение порта

В `Program.cs`:
```csharp
app.Run("http://0.0.0.0:5000");  // Измените 5000 на нужный порт
```

Или через командную строку:
```bash
dotnet run --urls "http://localhost:8080"
```

## 🔒 Безопасность

### Path Traversal Protection
```csharp
var fullPath = Path.GetFullPath(filepath);
if (!fullPath.StartsWith(musicPath))
{
    return Results.Json(new { error = "Доступ запрещен" }, statusCode: 403);
}
```

### CORS
Настроен для работы только с указанными origins (Next.js фронтенд).

### File Validation
Проверка существования файлов перед отправкой.

## ⚡ Производительность

### Буферизация
- Размер буфера: **80KB** (81920 bytes)
- Оптимизирован для потокового воспроизведения

### Range Requests
Поддержка HTTP Range requests для:
- Перемотки треков
- Частичной загрузки
- Экономии трафика

### Асинхронность
Все I/O операции асинхронные для максимальной производительности.

## 🧪 Тестирование API

### С помощью curl

```bash
# Получить список треков
curl http://localhost:5000/api/music

# Информация о треке
curl http://localhost:5000/api/track/1

# Поиск
curl "http://localhost:5000/api/search?q=bts"

# Статистика
curl http://localhost:5000/api/stats

# Стриминг с Range
curl -H "Range: bytes=0-1024" http://localhost:5000/api/stream/dynamite.flac
```

### С помощью Postman

Импортируйте коллекцию или создайте запросы вручную:
- Base URL: `http://localhost:5000`
- Все endpoints GET
- Не требуют аутентификации

## 🔄 Интеграция с Next.js фронтендом

### Настройка фронтенда

В Next.js проекте создайте `.env.local`:

```bash
NEXT_PUBLIC_API_URL=http://localhost:5000
```

### Пример запроса

```typescript
// В компоненте Next.js
const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/music`);
const data = await response.json();
```

### Замена встроенных API Routes

Замените `/app/api/*` routes на прокси к ASP.NET:

```typescript
// app/api/music/route.ts
export async function GET() {
  const response = await fetch('http://localhost:5000/api/music');
  const data = await response.json();
  return Response.json(data);
}
```

## 📝 Добавление новых треков

Отредактируйте `Program.cs`:

```csharp
var musicLibrary = new[]
{
    new { 
        id = 9, 
        title = "Ваша песня", 
        artist = "Исполнитель", 
        filename = "your-song.flac", 
        format = "FLAC 24bit/96kHz", 
        emoji = "🎤", 
        duration = 200 
    },
    // ... остальные треки
};
```

## 🐛 Troubleshooting

### Сервер не запускается

```bash
# Проверьте версию .NET
dotnet --version  # Должна быть 8.0+

# Очистите проект
dotnet clean
dotnet restore
dotnet run
```

### Порт занят

```bash
# Измените порт
dotnet run --urls "http://localhost:5001"
```

### CORS ошибки

Убедитесь, что фронтенд URL добавлен в `appsettings.json`:
```json
"AllowedOrigins": [
  "http://localhost:3000",
  "http://localhost:3001"
]
```

### Файлы не найдены

```bash
# Проверьте путь
ls music/

# Проверьте имена файлов (регистр важен!)
```

## 🚀 Production Deployment

### Создание Release build

```bash
dotnet publish -c Release -o ./publish
```

### Docker (опционально)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
EXPOSE 5000
ENTRYPOINT ["dotnet", "KPopFlacMusicServer.dll"]
```

### Systemd Service (Linux)

```ini
[Unit]
Description=K-POP FLAC Music Server

[Service]
WorkingDirectory=/var/www/kpop-server
ExecStart=/usr/bin/dotnet KPopFlacMusicServer.dll
Restart=always

[Install]
WantedBy=multi-user.target
```

## 📄 Лицензия

MIT License

## 🎵 Поддержка

Наслаждайтесь высококачественной музыкой с ASP.NET Core! 🎧

---

Made with ❤️ for K-POP and Hi-Res audio lovers

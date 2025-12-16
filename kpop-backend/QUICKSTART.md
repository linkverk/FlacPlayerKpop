# 🚀 K-POP FLAC Backend - Быстрый старт

## ⚡ Самый быстрый способ

```bash
# 1. Перейдите в папку
cd kpop-backend

# 2. Запустите сервер
dotnet run

# 3. Откройте в браузере
# http://localhost:5000
```

## 📁 Добавление музыки

```bash
# Скопируйте FLAC файлы в папку music/
cp ~/Music/song1.flac music/dynamite.flac
cp ~/Music/song2.flac music/hylt.flac
cp ~/Music/song3.flac music/nextlevel.flac
cp ~/Music/song4.flac music/butter.flac
cp ~/Music/song5.flac music/eleven.flac
cp ~/Music/song6.flac music/savage.flac
cp ~/Music/song7.flac music/pinkvenom.flac
cp ~/Music/song8.flac music/spicy.flac
```

## 🧪 Тестирование

```bash
# Получить список треков
curl http://localhost:5000/api/music | json_pp

# Информация о треке
curl http://localhost:5000/api/track/1 | json_pp

# Статистика
curl http://localhost:5000/api/stats | json_pp

# Поиск
curl "http://localhost:5000/api/search?q=bts" | json_pp
```

## 🔌 Подключение Next.js фронтенда

### Вариант 1: Использовать ASP.NET бэкенд напрямую

В Next.js создайте `.env.local`:
```bash
NEXT_PUBLIC_API_URL=http://localhost:5000
```

Обновите API запросы:
```typescript
// Вместо '/api/music'
const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/music`);
```

### Вариант 2: Проксировать через Next.js

Измените Next.js API routes:

```typescript
// app/api/music/route.ts
export async function GET() {
  const response = await fetch('http://localhost:5000/api/music');
  const data = await response.json();
  return Response.json(data);
}

// app/api/stream/[filename]/route.ts
export async function GET(
  request: NextRequest,
  { params }: { params: { filename: string } }
) {
  const range = request.headers.get('range');
  const headers: HeadersInit = range ? { 'Range': range } : {};
  
  const response = await fetch(
    `http://localhost:5000/api/stream/${params.filename}`,
    { headers }
  );
  
  return new Response(response.body, {
    status: response.status,
    headers: response.headers,
  });
}
```

## 🐳 Docker запуск

```bash
# Собрать и запустить
docker-compose up -d

# Проверить логи
docker-compose logs -f

# Остановить
docker-compose down
```

## 🌐 API Endpoints

| Метод | Endpoint | Описание |
|-------|----------|----------|
| GET | `/api/music` | Список всех треков |
| GET | `/api/stream/{filename}` | Стриминг аудио |
| GET | `/api/track/{id}` | Инфо о треке |
| GET | `/api/search?q={query}` | Поиск треков |
| GET | `/api/artists` | Список артистов |
| GET | `/api/formats` | Форматы аудио |
| GET | `/api/stats` | Статистика |
| GET | `/api/download/{filename}` | Скачать трек |
| GET | `/api/health` | Health check |

## 🔧 Изменение порта

```bash
# Через командную строку
dotnet run --urls "http://localhost:8080"

# Или в Program.cs
app.Run("http://0.0.0.0:8080");
```

## 📝 Добавление новых треков в код

Отредактируйте `Program.cs` и добавьте в массив `musicLibrary`:

```csharp
new { 
    id = 9, 
    title = "Название", 
    artist = "Исполнитель", 
    filename = "file.flac", 
    format = "FLAC 24bit/96kHz", 
    emoji = "🎤", 
    duration = 200 
}
```

## 🏗️ Production Build

```bash
# Создать release
dotnet publish -c Release -o ./publish

# Запустить
cd publish
dotnet KPopFlacMusicServer.dll
```

## 💡 Полезные команды

```bash
# Проверить версию .NET
dotnet --version

# Очистить проект
dotnet clean

# Восстановить пакеты
dotnet restore

# Собрать проект
dotnet build

# Запустить с watch (автоперезагрузка)
dotnet watch run
```

## 🐛 Решение проблем

### Порт занят
```bash
# Найти процесс на порту 5000
lsof -i :5000  # macOS/Linux
netstat -ano | findstr :5000  # Windows

# Изменить порт
dotnet run --urls "http://localhost:5001"
```

### Файлы не стримятся
- Проверьте что файлы в папке `music/`
- Проверьте расширение `.flac`
- Проверьте права доступа к файлам

### CORS ошибки
В `appsettings.json` добавьте origin фронтенда:
```json
"AllowedOrigins": ["http://localhost:3000"]
```

## 🎵 Готово!

Backend запущен и готов обслуживать высококачественную музыку! 🎧

---

**Следующий шаг:** Запустите Next.js фронтенд и наслаждайтесь!

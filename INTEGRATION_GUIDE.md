# 🎵 K-POP FLAC Player - Полное руководство по интеграции

## 📦 Что вы получили

1. **Frontend (Next.js 14)** - `kpop-player.tar.gz`
   - Современный React UI с TypeScript
   - Framer Motion анимации
   - Tailwind CSS дизайн
   - Встроенные API routes

2. **Backend (ASP.NET Core 8.0)** - `kpop-backend.tar.gz`
   - Мощный API сервер
   - Стриминг FLAC файлов
   - Поиск и статистика
   - CORS настроен

## 🚀 Быстрый старт (оба компонента)

### Вариант 1: Frontend со встроенным API (проще)

```bash
# 1. Распакуйте и установите frontend
tar -xzf kpop-player.tar.gz
cd kpop-player
npm install

# 2. Создайте папку для музыки
mkdir -p public/music

# 3. Добавьте FLAC файлы
cp ~/Music/*.flac public/music/
# Переименуйте согласно списку в README

# 4. Запустите
npm run dev

# Готово! http://localhost:3000
```

**Плюсы:**
- ✅ Всё в одном проекте
- ✅ Проще в настройке
- ✅ Подходит для личного использования

**Минусы:**
- ❌ Next.js API routes медленнее для больших файлов
- ❌ Нет расширенных возможностей (поиск, статистика)

### Вариант 2: Frontend + Backend раздельно (рекомендуется)

```bash
# Терминал 1 - Backend
tar -xzf kpop-backend.tar.gz
cd kpop-backend
dotnet run  # Запустится на :5000

# Терминал 2 - Frontend
tar -xzf kpop-player.tar.gz
cd kpop-player
npm install
npm run dev  # Запустится на :3000
```

**Плюсы:**
- ✅ Быстрый стриминг через ASP.NET Core
- ✅ Расширенные возможности (поиск, артисты, статистика)
- ✅ Лучше для production
- ✅ Можно масштабировать отдельно

**Минусы:**
- ❌ Два процесса для запуска
- ❌ Требует интеграцию

## 🔌 Интеграция Frontend + Backend

### Шаг 1: Подготовка Backend

```bash
cd kpop-backend

# Добавьте музыку
mkdir music
cp ~/Music/*.flac music/

# Переименуйте файлы:
mv music/song1.flac music/dynamite.flac
mv music/song2.flac music/hylt.flac
# ... и т.д.

# Запустите
dotnet run
# Backend запущен на http://localhost:5000
```

### Шаг 2: Настройка Frontend

#### Способ A: Прямое подключение (проще)

Создайте `.env.local` в папке `kpop-player`:

```bash
NEXT_PUBLIC_API_URL=http://localhost:5000
```

Обновите компоненты для использования внешнего API:

```typescript
// app/page.tsx
const loadMusicLibrary = async () => {
  try {
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || '';
    const response = await fetch(`${apiUrl}/api/music`);
    const data = await response.json();
    // ...
  } catch (error) {
    console.error('Failed to load music library:', error);
  }
};
```

```typescript
// components/AudioPlayer.tsx
useEffect(() => {
  if (playlist.length > 0 && currentTrackIndex >= 0) {
    const track = playlist[currentTrackIndex];
    const apiUrl = process.env.NEXT_PUBLIC_API_URL || '';
    
    if (audioRef.current) {
      audioRef.current.src = `${apiUrl}/api/stream/${track.filename}`;
      audioRef.current.load();
    }
  }
}, [currentTrackIndex, playlist]);
```

#### Способ B: Проксирование через Next.js (гибче)

Оставьте Next.js API routes как есть, но проксируйте к ASP.NET:

```typescript
// app/api/music/route.ts
export async function GET() {
  const backendUrl = process.env.BACKEND_URL || 'http://localhost:5000';
  const response = await fetch(`${backendUrl}/api/music`);
  const data = await response.json();
  return Response.json(data);
}
```

```typescript
// app/api/stream/[filename]/route.ts
import { NextRequest, NextResponse } from 'next/server';

export async function GET(
  request: NextRequest,
  { params }: { params: { filename: string } }
) {
  const backendUrl = process.env.BACKEND_URL || 'http://localhost:5000';
  const range = request.headers.get('range');
  
  const headers: HeadersInit = {};
  if (range) {
    headers['Range'] = range;
  }
  
  const response = await fetch(
    `${backendUrl}/api/stream/${params.filename}`,
    { headers }
  );
  
  return new NextResponse(response.body, {
    status: response.status,
    headers: response.headers,
  });
}
```

Создайте `.env.local`:
```bash
BACKEND_URL=http://localhost:5000
```

### Шаг 3: Запуск

```bash
# Терминал 1 - Backend
cd kpop-backend
dotnet run

# Терминал 2 - Frontend
cd kpop-player
npm run dev

# Откройте http://localhost:3000
```

## 📁 Структура файлов

```
project/
├── kpop-backend/          # ASP.NET Core backend
│   ├── music/            # 📁 FLAC файлы здесь
│   │   ├── dynamite.flac
│   │   ├── hylt.flac
│   │   └── ...
│   ├── Program.cs
│   └── ...
│
└── kpop-player/          # Next.js frontend
    ├── app/
    ├── components/
    ├── public/
    └── ...
```

## 🎵 Список файлов для библиотеки

Переименуйте ваши FLAC файлы:

1. `dynamite.flac` - BTS - Dynamite
2. `hylt.flac` - BLACKPINK - How You Like That
3. `nextlevel.flac` - aespa - Next Level
4. `butter.flac` - BTS - Butter
5. `eleven.flac` - IVE - ELEVEN
6. `savage.flac` - aespa - Savage
7. `pinkvenom.flac` - BLACKPINK - Pink Venom
8. `spicy.flac` - aespa - Spicy

## 🔧 Дополнительные возможности Backend API

### Поиск треков
```bash
curl "http://localhost:5000/api/search?q=bts"
```

### Список артистов
```bash
curl http://localhost:5000/api/artists
```

### Статистика
```bash
curl http://localhost:5000/api/stats
```

### Форматы
```bash
curl http://localhost:5000/api/formats
```

## 🐳 Docker развертывание

### Backend
```bash
cd kpop-backend
docker-compose up -d
```

### Frontend
Создайте `Dockerfile` в `kpop-player`:

```dockerfile
FROM node:20-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci

FROM node:20-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV production
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

EXPOSE 3000
CMD ["node", "server.js"]
```

## 🌐 Production развертывание

### Backend на Linux сервере

```bash
# Собрать
dotnet publish -c Release -o ./publish

# Создать systemd service
sudo nano /etc/systemd/system/kpop-backend.service
```

```ini
[Unit]
Description=K-POP FLAC Backend

[Service]
WorkingDirectory=/var/www/kpop-backend
ExecStart=/usr/bin/dotnet /var/www/kpop-backend/KPopFlacMusicServer.dll
Restart=always
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable kpop-backend
sudo systemctl start kpop-backend
```

### Frontend на Vercel

```bash
cd kpop-player
vercel deploy
```

Добавьте environment variable:
- `BACKEND_URL` = `https://your-backend.com`

## 🔒 Безопасность для Production

### Backend

1. **HTTPS**: Используйте обратный прокси (nginx)
2. **Rate Limiting**: Добавьте ограничения запросов
3. **Аутентификация**: Для приватного доступа

```csharp
// Добавьте в Program.cs
builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("api", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 60;
    });
});

app.UseRateLimiter();
```

### Frontend

1. **Environment Variables**: Не коммитьте `.env.local`
2. **CORS**: Настройте правильные origins
3. **CSP Headers**: Content Security Policy

## 📊 Мониторинг

### Backend Health Check

```bash
curl http://localhost:5000/api/health
```

### Frontend Monitoring

Используйте Vercel Analytics или добавьте:
- Sentry для ошибок
- Google Analytics для метрик

## 🎨 Кастомизация

### Добавление новых треков

**Backend** (`Program.cs`):
```csharp
new { 
    id = 9, 
    title = "Ваш трек", 
    artist = "Артист", 
    filename = "file.flac",
    format = "FLAC 24bit/96kHz", 
    emoji = "🎵", 
    duration = 200 
}
```

**Frontend**: Автоматически подтянется из API!

### Изменение дизайна

Редактируйте `tailwind.config.js`:
```javascript
colors: {
  neon: {
    pink: '#FF10F0',    // Ваш цвет
    cyan: '#00F0FF',    // Ваш цвет
  },
}
```

## 🐛 Troubleshooting

### Backend не запускается
```bash
dotnet --version  # Проверьте версию
dotnet clean && dotnet restore
```

### Frontend не подключается к Backend
- Проверьте CORS в `appsettings.json`
- Проверьте URL в `.env.local`
- Проверьте firewall

### Треки не воспроизводятся
- Проверьте формат файлов (FLAC)
- Проверьте имена файлов
- Проверьте права доступа

### CORS ошибки
В `kpop-backend/appsettings.json`:
```json
"AllowedOrigins": [
  "http://localhost:3000",
  "https://your-frontend.vercel.app"
]
```

## 💡 Советы

1. **Разработка**: Используйте оба раздельно
2. **Production**: Разместите Backend на сервере, Frontend на Vercel/Netlify
3. **Производительность**: ASP.NET Core стримит быстрее Next.js
4. **Масштабируемость**: Легко добавить CDN для статики

## 📚 Дополнительные ресурсы

- [Next.js Documentation](https://nextjs.org/docs)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [FLAC Audio Format](https://xiph.org/flac/)

---

## 🎉 Готово!

Теперь у вас есть полноценная система для стриминга высококачественной K-POP музыки!

**Рекомендация**: Используйте Вариант 2 (раздельный Frontend + Backend) для лучшей производительности и гибкости.

Наслаждайтесь музыкой! 🎵🎧

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// CORS для работы с Next.js фронтендом
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Range", "Accept-Ranges");
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors("AllowNextJS");
app.UseRouting();

// Путь к папке с музыкой
var musicPath = Path.Combine(Directory.GetCurrentDirectory(), "music");
if (!Directory.Exists(musicPath))
{
    Directory.CreateDirectory(musicPath);
    Console.WriteLine($"📁 Создана папка для музыки: {musicPath}");
}

// Функция для извлечения артиста и названия из имени файла
string ExtractArtistAndTitle(string filename)
{
    // Убираем расширение
    var nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
    
    // Паттерны для парсинга: "Artist - Title" или "Artist-Title"
    var patterns = new[]
    {
        @"^(.+?)\s*-\s*(.+)$",  // "Artist - Title"
        @"^(.+?)_(.+)$",         // "Artist_Title"
    };
    
    foreach (var pattern in patterns)
    {
        var match = Regex.Match(nameWithoutExt, pattern);
        if (match.Success)
        {
            return $"{match.Groups[1].Value.Trim()}|{match.Groups[2].Value.Trim()}";
        }
    }
    
    // Если паттерн не подошел, используем имя файла как название
    return $"Unknown Artist|{nameWithoutExt}";
}

// Функция для определения эмодзи на основе артиста
string GetEmojiForArtist(string artist)
{
    var lowerArtist = artist.ToLower();
    
    if (lowerArtist.Contains("bts")) return "💥";
    if (lowerArtist.Contains("blackpink")) return "🖤";
    if (lowerArtist.Contains("aespa")) return "🚀";
    if (lowerArtist.Contains("ive")) return "🎯";
    if (lowerArtist.Contains("newjeans")) return "🐰";
    if (lowerArtist.Contains("twice")) return "🍭";
    if (lowerArtist.Contains("red velvet")) return "🍰";
    if (lowerArtist.Contains("itzy")) return "⚡";
    if (lowerArtist.Contains("txt") || lowerArtist.Contains("tomorrow")) return "🌟";
    if (lowerArtist.Contains("stray kids")) return "🐺";
    if (lowerArtist.Contains("seventeen")) return "💎";
    if (lowerArtist.Contains("nct")) return "🌱";
    if (lowerArtist.Contains("exo")) return "🌙";
    
    return "🎵"; // По умолчанию
}

// Функция для получения длительности (приблизительно по размеру файла)
int EstimateDuration(long fileSize)
{
    // FLAC ~1MB = ~6-7 секунд (приблизительно)
    // Для точности нужна библиотека для чтения метаданных
    var megabytes = fileSize / (1024.0 * 1024.0);
    return (int)(megabytes * 6.5);
}

// Автоматическое сканирование музыкальной папки
List<dynamic> ScanMusicLibrary()
{
    var tracks = new List<dynamic>();
    
    if (!Directory.Exists(musicPath))
    {
        return tracks;
    }
    
    var flacFiles = Directory.GetFiles(musicPath, "*.flac");
    var id = 1;
    
    foreach (var filepath in flacFiles)
    {
        var filename = Path.GetFileName(filepath);
        var fileInfo = new FileInfo(filepath);
        
        // Извлекаем артиста и название
        var artistTitle = ExtractArtistAndTitle(filename);
        var parts = artistTitle.Split('|');
        var artist = parts[0];
        var title = parts[1];
        
        tracks.Add(new
        {
            id = id++,
            title = title,
            artist = artist,
            filename = filename,
            format = "FLAC 24bit/96kHz",
            emoji = GetEmojiForArtist(artist),
            duration = EstimateDuration(fileInfo.Length),
            fileSize = fileInfo.Length,
            lastModified = fileInfo.LastWriteTimeUtc
        });
    }
    
    return tracks;
}

// ========================================
// API ROUTES
// ========================================

// Корневой маршрут - информация о сервере
app.MapGet("/", () => Results.Ok(new
{
    message = "K-POP FLAC Music Server (ASP.NET Core)",
    version = "2.1.0",
    status = "online",
    features = new[] { "Auto-scan music directory", "No rename required" },
    endpoints = new
    {
        musicList = "/api/music",
        stream = "/api/stream/{filename}",
        trackInfo = "/api/track/{id}",
        search = "/api/search?q={query}",
        artists = "/api/artists",
        formats = "/api/formats",
        rescan = "/api/rescan"
    },
    musicDirectory = musicPath,
    serverTime = DateTime.UtcNow
}));

// Список всех треков (автоматическое сканирование)
app.MapGet("/api/music", () =>
{
    var tracks = ScanMusicLibrary();
    
    return Results.Ok(new
    {
        success = true,
        tracks = tracks,
        availableCount = tracks.Count,
        totalCount = tracks.Count,
        timestamp = DateTime.UtcNow,
        autoScanned = true
    });
});

// Пересканировать папку с музыкой
app.MapGet("/api/rescan", () =>
{
    var tracks = ScanMusicLibrary();
    
    return Results.Ok(new
    {
        success = true,
        message = "Музыкальная библиотека пересканирована",
        tracksFound = tracks.Count,
        tracks = tracks
    });
});

// Стриминг аудио с поддержкой Range requests
app.MapGet("/api/stream/{filename}", async (string filename, HttpContext context) =>
{
    var filepath = Path.Combine(musicPath, filename);

    // Проверка безопасности - файл должен быть в папке музыки
    var fullPath = Path.GetFullPath(filepath);
    if (!fullPath.StartsWith(musicPath))
    {
        return Results.Json(new { error = "Доступ запрещен" }, statusCode: 403);
    }

    // Проверка существования файла
    if (!File.Exists(filepath))
    {
        return Results.Json(new
        {
            error = "Файл не найден",
            message = $"Файл {filename} не найден в папке музыки",
            hint = "Добавьте FLAC файлы в папку music/"
        }, statusCode: 404);
    }

    var fileInfo = new FileInfo(filepath);
    var rangeHeader = context.Request.Headers["Range"].ToString();

    // Если есть Range header - отправляем частичный контент
    if (!string.IsNullOrEmpty(rangeHeader))
    {
        var range = rangeHeader.Replace("bytes=", "").Split('-');
        var start = long.Parse(range[0]);
        var end = range.Length > 1 && !string.IsNullOrEmpty(range[1])
            ? long.Parse(range[1])
            : fileInfo.Length - 1;

        var length = end - start + 1;

        context.Response.StatusCode = 206; // Partial Content
        context.Response.Headers["Content-Range"] = $"bytes {start}-{end}/{fileInfo.Length}";
        context.Response.Headers["Accept-Ranges"] = "bytes";
        context.Response.Headers["Content-Length"] = length.ToString();
        context.Response.ContentType = "audio/flac";

        using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(start, SeekOrigin.Begin);

        var buffer = new byte[81920]; // 80KB buffer для плавного стриминга
        var bytesToRead = length;

        while (bytesToRead > 0)
        {
            var bytesRead = await stream.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, bytesToRead));
            if (bytesRead == 0) break;

            await context.Response.Body.WriteAsync(buffer, 0, bytesRead);
            bytesToRead -= bytesRead;
        }

        return Results.Empty;
    }
    else
    {
        // Полная отправка файла
        context.Response.Headers["Content-Length"] = fileInfo.Length.ToString();
        context.Response.Headers["Accept-Ranges"] = "bytes";
        context.Response.ContentType = "audio/flac";

        await context.Response.SendFileAsync(filepath);
        return Results.Empty;
    }
});

// Информация о конкретном треке
app.MapGet("/api/track/{id:int}", (int id) =>
{
    var tracks = ScanMusicLibrary();
    var track = tracks.FirstOrDefault(t => t.id == id);

    if (track == null)
    {
        return Results.Json(new { error = "Трек не найден" }, statusCode: 404);
    }

    return Results.Ok(track);
});

// Поиск треков
app.MapGet("/api/search", (string? q) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.Ok(new
        {
            success = false,
            message = "Введите поисковый запрос",
            results = Array.Empty<object>()
        });
    }

    var tracks = ScanMusicLibrary();
    var query = q.ToLower();
    var results = tracks.Where(track =>
        track.title.ToLower().Contains(query) ||
        track.artist.ToLower().Contains(query) ||
        track.filename.ToLower().Contains(query)
    ).ToList();

    return Results.Ok(new
    {
        success = true,
        query = q,
        count = results.Count,
        results
    });
});

// Список артистов
app.MapGet("/api/artists", () =>
{
    var tracks = ScanMusicLibrary();
    var artists = tracks
        .GroupBy(t => t.artist)
        .Select(g => new
        {
            name = g.Key,
            trackCount = g.Count(),
            tracks = g.Select(t => new { t.id, t.title })
        })
        .OrderBy(a => a.name)
        .ToList();

    return Results.Ok(new
    {
        success = true,
        count = artists.Count,
        artists
    });
});

// Информация о форматах
app.MapGet("/api/formats", () =>
{
    var tracks = ScanMusicLibrary();
    var formats = tracks
        .GroupBy(t => t.format)
        .Select(g => new
        {
            format = g.Key,
            count = g.Count()
        })
        .ToList();

    return Results.Ok(new
    {
        success = true,
        formats
    });
});

// Статистика библиотеки
app.MapGet("/api/stats", () =>
{
    var tracks = ScanMusicLibrary();
    var totalDuration = tracks.Sum(t => (int)t.duration);
    var artists = tracks.Select(t => (string)t.artist).Distinct().Count();

    return Results.Ok(new
    {
        success = true,
        stats = new
        {
            totalTracks = tracks.Count,
            availableTracks = tracks.Count,
            unavailableTracks = 0,
            totalDurationSeconds = totalDuration,
            totalDurationFormatted = TimeSpan.FromSeconds(totalDuration).ToString(@"hh\:mm\:ss"),
            uniqueArtists = artists,
            averageTrackDuration = tracks.Count > 0 ? totalDuration / tracks.Count : 0
        }
    });
});

// Скачивание трека (не стриминг, а полная загрузка)
app.MapGet("/api/download/{filename}", async (string filename, HttpContext context) =>
{
    var filepath = Path.Combine(musicPath, filename);
    var fullPath = Path.GetFullPath(filepath);

    if (!fullPath.StartsWith(musicPath))
    {
        return Results.Json(new { error = "Доступ запрещен" }, statusCode: 403);
    }

    if (!File.Exists(filepath))
    {
        return Results.Json(new { error = "Файл не найден" }, statusCode: 404);
    }

    context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{filename}\"";
    await context.Response.SendFileAsync(filepath);
    return Results.Empty;
});

// Health check
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    uptime = DateTime.UtcNow,
    version = "2.1.0",
    features = new[] { "auto-scan", "no-rename" }
}));

// ========================================
// STARTUP
// ========================================

var initialTracks = ScanMusicLibrary();

Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║     🎵 K-POP FLAC Music Server (ASP.NET Core)     ║");
Console.WriteLine("║         AUTO-SCAN MODE (No Rename Required)        ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"🌐 Server:          http://localhost:5000");
Console.WriteLine($"📁 Music Directory: {musicPath}");
Console.WriteLine($"📊 Tracks Found:    {initialTracks.Count}");
Console.WriteLine();
Console.WriteLine("💡 API Endpoints:");
Console.WriteLine("   GET  /api/music              - Список всех треков (авто-сканирование)");
Console.WriteLine("   GET  /api/rescan             - Пересканировать папку");
Console.WriteLine("   GET  /api/stream/{filename}  - Стриминг аудио");
Console.WriteLine("   GET  /api/track/{id}         - Информация о треке");
Console.WriteLine("   GET  /api/search?q={query}   - Поиск треков");
Console.WriteLine("   GET  /api/artists            - Список артистов");
Console.WriteLine("   GET  /api/formats            - Форматы аудио");
Console.WriteLine("   GET  /api/stats              - Статистика библиотеки");
Console.WriteLine("   GET  /api/download/{filename}- Скачать трек");
Console.WriteLine("   GET  /api/health             - Health check");
Console.WriteLine();
Console.WriteLine("✨ Просто добавьте .flac файлы в папку music/");
Console.WriteLine("   Формат: 'Artist - Title.flac' или любое имя");
Console.WriteLine("🚀 Сервер запущен и готов к работе!");
Console.WriteLine();

if (initialTracks.Count > 0)
{
    Console.WriteLine("🎵 Найденные треки:");
    foreach (var track in initialTracks.Take(5))
    {
        Console.WriteLine($"   {track.emoji} {track.artist} - {track.title}");
    }
    if (initialTracks.Count > 5)
    {
        Console.WriteLine($"   ... и ещё {initialTracks.Count - 5} треков");
    }
    Console.WriteLine();
}

app.Run("http://0.0.0.0:5000");
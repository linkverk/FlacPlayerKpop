using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Linq;
using System.Text.Json;

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

// Музыкальная библиотека
var musicLibrary = new[]
{
    new { id = 1, title = "Dynamite", artist = "BTS", filename = "dynamite.flac", format = "FLAC 24bit/96kHz", emoji = "💥", duration = 199 },
    new { id = 2, title = "How You Like That", artist = "BLACKPINK", filename = "hylt.flac", format = "FLAC 24bit/96kHz", emoji = "🖤", duration = 182 },
    new { id = 3, title = "Next Level", artist = "aespa", filename = "nextlevel.flac", format = "FLAC 24bit/96kHz", emoji = "🚀", duration = 210 },
    new { id = 4, title = "Butter", artist = "BTS", filename = "butter.flac", format = "FLAC 24bit/96kHz", emoji = "🧈", duration = 164 },
    new { id = 5, title = "ELEVEN", artist = "IVE", filename = "eleven.flac", format = "FLAC 24bit/96kHz", emoji = "🎯", duration = 179 },
    new { id = 6, title = "Savage", artist = "aespa", filename = "savage.flac", format = "FLAC 24bit/96kHz", emoji = "😈", duration = 234 },
    new { id = 7, title = "Pink Venom", artist = "BLACKPINK", filename = "pinkvenom.flac", format = "FLAC 24bit/96kHz", emoji = "🐍", duration = 187 },
    new { id = 8, title = "Spicy", artist = "aespa", filename = "spicy.flac", format = "FLAC 24bit/96kHz", emoji = "🌶️", duration = 195 }
};

// ========================================
// API ROUTES
// ========================================

// Корневой маршрут - информация о сервере
app.MapGet("/", () => Results.Ok(new
{
    message = "K-POP FLAC Music Server (ASP.NET Core)",
    version = "2.0.0",
    status = "online",
    endpoints = new
    {
        musicList = "/api/music",
        stream = "/api/stream/{filename}",
        trackInfo = "/api/track/{id}",
        search = "/api/search?q={query}",
        artists = "/api/artists",
        formats = "/api/formats"
    },
    musicDirectory = musicPath,
    serverTime = DateTime.UtcNow
}));

// Список всех треков
app.MapGet("/api/music", () =>
{
    var availableTracks = musicLibrary.Where(track =>
    {
        var filepath = Path.Combine(musicPath, track.filename);
        return File.Exists(filepath);
    }).ToList();

    return Results.Ok(new
    {
        success = true,
        tracks = musicLibrary,
        availableCount = availableTracks.Count,
        totalCount = musicLibrary.Length,
        timestamp = DateTime.UtcNow
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
    var track = musicLibrary.FirstOrDefault(t => t.id == id);

    if (track == null)
    {
        return Results.Json(new { error = "Трек не найден" }, statusCode: 404);
    }

    var filepath = Path.Combine(musicPath, track.filename);
    var exists = File.Exists(filepath);

    FileInfo? fileInfo = exists ? new FileInfo(filepath) : null;

    return Results.Ok(new
    {
        track.id,
        track.title,
        track.artist,
        track.filename,
        track.format,
        track.emoji,
        track.duration,
        available = exists,
        fileSize = fileInfo?.Length,
        lastModified = fileInfo?.LastWriteTimeUtc,
        streamUrl = exists ? $"/api/stream/{track.filename}" : null
    });
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

    var query = q.ToLower();
    var results = musicLibrary.Where(track =>
        track.title.ToLower().Contains(query) ||
        track.artist.ToLower().Contains(query)
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
    var artists = musicLibrary
        .Select(t => t.artist)
        .Distinct()
        .OrderBy(a => a)
        .Select(artist => new
        {
            name = artist,
            trackCount = musicLibrary.Count(t => t.artist == artist),
            tracks = musicLibrary.Where(t => t.artist == artist).Select(t => new { t.id, t.title })
        })
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
    var formats = musicLibrary
        .Select(t => t.format)
        .Distinct()
        .Select(format => new
        {
            format,
            count = musicLibrary.Count(t => t.format == format)
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
    var availableCount = musicLibrary.Count(track =>
    {
        var filepath = Path.Combine(musicPath, track.filename);
        return File.Exists(filepath);
    });

    var totalDuration = musicLibrary.Sum(t => t.duration);
    var artists = musicLibrary.Select(t => t.artist).Distinct().Count();

    return Results.Ok(new
    {
        success = true,
        stats = new
        {
            totalTracks = musicLibrary.Length,
            availableTracks = availableCount,
            unavailableTracks = musicLibrary.Length - availableCount,
            totalDurationSeconds = totalDuration,
            totalDurationFormatted = TimeSpan.FromSeconds(totalDuration).ToString(@"hh\:mm\:ss"),
            uniqueArtists = artists,
            averageTrackDuration = totalDuration / musicLibrary.Length
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
    version = "2.0.0"
}));

// ========================================
// STARTUP
// ========================================

Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║     🎵 K-POP FLAC Music Server (ASP.NET Core)     ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"🌐 Server:          http://localhost:5000");
Console.WriteLine($"📁 Music Directory: {musicPath}");
Console.WriteLine($"📊 Tracks:          {musicLibrary.Length}");
Console.WriteLine();
Console.WriteLine("💡 API Endpoints:");
Console.WriteLine("   GET  /api/music              - Список всех треков");
Console.WriteLine("   GET  /api/stream/{filename}  - Стриминг аудио");
Console.WriteLine("   GET  /api/track/{id}         - Информация о треке");
Console.WriteLine("   GET  /api/search?q={query}   - Поиск треков");
Console.WriteLine("   GET  /api/artists            - Список артистов");
Console.WriteLine("   GET  /api/formats            - Форматы аудио");
Console.WriteLine("   GET  /api/stats              - Статистика библиотеки");
Console.WriteLine("   GET  /api/download/{filename}- Скачать трек");
Console.WriteLine("   GET  /api/health             - Health check");
Console.WriteLine();
Console.WriteLine("⚠️  Добавьте FLAC файлы в папку music/");
Console.WriteLine("🚀 Сервер запущен и готов к работе!");
Console.WriteLine();

app.Run("http://0.0.0.0:5000");

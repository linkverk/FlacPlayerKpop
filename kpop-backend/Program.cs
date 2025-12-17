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

// ============================================
// MUSIC DIRECTORY CONFIGURATION
// ============================================
// Получаем путь из переменной окружения
var musicDirEnv = Environment.GetEnvironmentVariable("MUSIC_DIR");

if (string.IsNullOrEmpty(musicDirEnv))
{
    Console.WriteLine("⚠️  MUSIC_DIR не установлен!");
    Console.WriteLine("   Установите MUSIC_DIR в .env файле");
    Console.WriteLine("   Пример: MUSIC_DIR=C:/Users/YourName/Music");
    musicDirEnv = "./music"; // Fallback
}

Console.WriteLine($"📝 MUSIC_DIR environment variable: {musicDirEnv}");

// Обработка разных типов путей
string musicPath;

if (Path.IsPathRooted(musicDirEnv))
{
    // Абсолютный путь (Linux: /home/user/Music, Windows: C:/Music)
    musicPath = musicDirEnv;
    Console.WriteLine($"✓ Detected absolute path");
}
else if (musicDirEnv.StartsWith("~/"))
{
    // Домашняя папка (Linux/Mac: ~/Music)
    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    musicPath = Path.Combine(home, musicDirEnv.Substring(2));
    Console.WriteLine($"✓ Detected home directory path: {home}");
}
else
{
    // Относительный путь (./music, ../Music)
    musicPath = Path.Combine(Directory.GetCurrentDirectory(), musicDirEnv);
    Console.WriteLine($"✓ Detected relative path from: {Directory.GetCurrentDirectory()}");
}

// Нормализация пути
musicPath = Path.GetFullPath(musicPath);

// Создаем папку если не существует
if (!Directory.Exists(musicPath))
{
    try
    {
        Directory.CreateDirectory(musicPath);
        Console.WriteLine($"📁 Created music directory: {musicPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to create directory: {ex.Message}");
    }
}

Console.WriteLine($"🎵 Using music directory: {musicPath}");

// Функция для извлечения артиста и названия из имени файла
string ExtractArtistAndTitle(string filenameWithExt)
{
    // Убираем расширение
    var nameWithoutExt = Path.GetFileNameWithoutExtension(filenameWithExt);
    
    // Паттерны для парсинга: "Artist - Title" или "Artist_Title"
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
    if (lowerArtist.Contains("babymonster")) return "🔥";
    if (lowerArtist.Contains("fifty fifty")) return "✨";
    if (lowerArtist.Contains("le sserafim") || lowerArtist.Contains("lesserafim")) return "👑";
    if (lowerArtist.Contains("meovv")) return "😺";
    
    return "🎵"; // По умолчанию
}

// Функция для получения длительности (приблизительно по размеру файла)
int EstimateDuration(long fileSize)
{
    // FLAC ~1MB = ~6-7 секунд (приблизительно)
    var megabytes = fileSize / (1024.0 * 1024.0);
    return (int)(megabytes * 6.5);
}

// Автоматическое сканирование музыкальной папки (ВКЛЮЧАЯ ВСЕ ПОДПАПКИ)
List<dynamic> ScanMusicLibrary()
{
    var tracks = new List<dynamic>();
    
    if (!Directory.Exists(musicPath))
    {
        Console.WriteLine($"⚠️  Music directory not found: {musicPath}");
        Console.WriteLine($"   Please set MUSIC_DIR in .env file");
        Console.WriteLine($"   Example: MUSIC_DIR=C:/Users/YourName/Music");
        return tracks;
    }
    
    Console.WriteLine($"📂 Scanning directory recursively: {musicPath}");
    
    // Сначала покажем структуру папок
    try
    {
        var subdirs = Directory.GetDirectories(musicPath, "*", SearchOption.AllDirectories);
        Console.WriteLine($"📁 Found {subdirs.Length} subdirectories:");
        foreach (var dir in subdirs.Take(10))
        {
            var relativeDirPath = Path.GetRelativePath(musicPath, dir);
            Console.WriteLine($"   📂 {relativeDirPath}");
        }
        if (subdirs.Length > 10)
        {
            Console.WriteLine($"   ... и ещё {subdirs.Length - 10} папок");
        }
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Warning: Could not list subdirectories: {ex.Message}");
    }
    
    // ✨ КЛЮЧЕВОЕ: SearchOption.AllDirectories - сканирует ВСЕ подпапки рекурсивно!
    Console.WriteLine($"🔍 Searching for FLAC files in all subdirectories...");
    
    string[] flacFiles;
    try
    {
        flacFiles = Directory.GetFiles(musicPath, "*.flac", SearchOption.AllDirectories);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error scanning for FLAC files: {ex.Message}");
        return tracks;
    }
    
    Console.WriteLine($"🎵 Found {flacFiles.Length} FLAC files total");
    Console.WriteLine();
    
    var id = 1;
    
    foreach (var filepath in flacFiles)
    {
        try
        {
            var fileBasename = Path.GetFileName(filepath);
            var fileInfo = new FileInfo(filepath);
            
            // Получаем относительный путь от папки music (с подпапками)
            var relativePath = Path.GetRelativePath(musicPath, filepath);
            
            // Получаем название родительской папки (для группировки)
            var parentDir = Path.GetFileName(Path.GetDirectoryName(filepath));
            
            // Извлекаем артиста и название
            var artistTitle = ExtractArtistAndTitle(fileBasename);
            var parts = artistTitle.Split('|');
            var artist = parts[0];
            var title = parts[1];
            
            // Детальное логирование первых 20 файлов
            if (id <= 20)
            {
                Console.WriteLine($"   ✓ {id}. {artist} - {title}");
                Console.WriteLine($"      📂 Relative path: {relativePath}");
                Console.WriteLine($"      📁 Parent folder: {parentDir}");
                Console.WriteLine($"      💾 Size: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");
            }
            
            tracks.Add(new
            {
                id = id++,
                title = title,
                artist = artist,
                filename = fileBasename,  // Только имя файла
                relativePath = relativePath.Replace("\\", "/"),  // Полный путь с подпапками
                parentFolder = parentDir, // Родительская папка
                format = "FLAC 24bit/96kHz",
                emoji = GetEmojiForArtist(artist),
                duration = EstimateDuration(fileInfo.Length),
                fileSize = fileInfo.Length,
                lastModified = fileInfo.LastWriteTimeUtc
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️  Warning: Could not process file {filepath}: {ex.Message}");
        }
    }
    
    if (tracks.Count > 20)
    {
        Console.WriteLine($"   ... и ещё {tracks.Count - 20} треков");
    }
    
    Console.WriteLine();
    
    // Статистика по папкам
    if (tracks.Count > 0)
    {
        var folderStats = tracks
            .GroupBy(t => t.parentFolder)
            .OrderByDescending(g => g.Count())
            .Take(10);
        
        Console.WriteLine("📊 Top folders by track count:");
        foreach (var folder in folderStats)
        {
            Console.WriteLine($"   📁 {folder.Key}: {folder.Count()} tracks");
        }
        Console.WriteLine();
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
    version = "2.6.0",
    status = "online",
    features = new[] { "Recursive subdirectory scanning", "All depth levels supported", "Auto-scan", "Custom music path from .env" },
    endpoints = new
    {
        musicList = "/api/music",
        stream = "/api/stream/{**filepath}",
        trackInfo = "/api/track/{id}",
        search = "/api/search?q={query}",
        artists = "/api/artists",
        formats = "/api/formats",
        rescan = "/api/rescan",
        stats = "/api/stats",
        folders = "/api/folders"
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
        autoScanned = true,
        recursiveScan = true,
        musicDirectory = musicPath
    });
});

// Новый эндпоинт: список папок с треками
app.MapGet("/api/folders", () =>
{
    var tracks = ScanMusicLibrary();
    
    var folders = tracks
        .GroupBy(t => t.parentFolder)
        .Select(g => new
        {
            name = g.Key,
            trackCount = g.Count(),
            tracks = g.Select(t => new { t.id, t.title, t.artist }).ToList()
        })
        .OrderBy(f => f.name)
        .ToList();
    
    return Results.Ok(new
    {
        success = true,
        folderCount = folders.Count,
        folders = folders
    });
});

// Пересканировать папку с музыкой
app.MapGet("/api/rescan", () =>
{
    var tracks = ScanMusicLibrary();
    
    return Results.Ok(new
    {
        success = true,
        message = "Музыкальная библиотека пересканирована (все подпапки рекурсивно)",
        tracksFound = tracks.Count,
        musicDirectory = musicPath,
        tracks = tracks
    });
});

// Стриминг аудио с поддержкой Range requests и подпапок
app.MapGet("/api/stream/{**filepath}", async (string filepath, HttpContext context) =>
{
    // Декодируем путь (на случай если есть URL-кодирование)
    filepath = Uri.UnescapeDataString(filepath);
    
    // Ищем файл - сначала по относительному пути, потом по имени
    var fullFilePath = Path.Combine(musicPath, filepath);
    
    // Если файл не найден по пути, ищем по имени рекурсивно во всех подпапках
    if (!File.Exists(fullFilePath))
    {
        var filenameToSearch = Path.GetFileName(filepath);
        
        try
        {
            // Ищем во всех подпапках
            var allFiles = Directory.GetFiles(musicPath, filenameToSearch, SearchOption.AllDirectories);
            
            if (allFiles.Length == 0)
            {
                return Results.Json(new
                {
                    error = "Файл не найден",
                    message = $"Файл {filenameToSearch} не найден во всех подпапках",
                    searchedPath = filepath,
                    musicDirectory = musicPath,
                    hint = $"Проверьте что MUSIC_DIR установлен правильно в .env файле"
                }, statusCode: 404);
            }
            
            fullFilePath = allFiles[0]; // Берём первое совпадение
            Console.WriteLine($"🔍 Found file by name search: {Path.GetRelativePath(musicPath, fullFilePath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error searching for file: {ex.Message}");
            return Results.Json(new
            {
                error = "Ошибка поиска файла",
                message = ex.Message
            }, statusCode: 500);
        }
    }
    
    var fullPath = Path.GetFullPath(fullFilePath);
    
    // Проверка безопасности - файл должен быть в папке музыки
    if (!fullPath.StartsWith(Path.GetFullPath(musicPath)))
    {
        return Results.Json(new { error = "Доступ запрещен" }, statusCode: 403);
    }

    var fileInfo = new FileInfo(fullFilePath);
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

        using var stream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(start, SeekOrigin.Begin);

        var buffer = new byte[81920]; // 80KB buffer
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

        await context.Response.SendFileAsync(fullFilePath);
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
        track.filename.ToLower().Contains(query) ||
        track.relativePath.ToLower().Contains(query) ||
        track.parentFolder.ToLower().Contains(query)
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
    var folders = tracks.Select(t => (string)t.parentFolder).Distinct().Count();

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
            uniqueFolders = folders,
            averageTrackDuration = tracks.Count > 0 ? totalDuration / tracks.Count : 0,
            musicDirectory = musicPath
        }
    });
});

// Скачивание трека
app.MapGet("/api/download/{**filepath}", async (string filepath, HttpContext context) =>
{
    filepath = Uri.UnescapeDataString(filepath);
    var fullFilePath = Path.Combine(musicPath, filepath);
    
    if (!File.Exists(fullFilePath))
    {
        var filenameToSearch = Path.GetFileName(filepath);
        var allFiles = Directory.GetFiles(musicPath, filenameToSearch, SearchOption.AllDirectories);
        
        if (allFiles.Length == 0)
        {
            return Results.Json(new { error = "Файл не найден" }, statusCode: 404);
        }
        
        fullFilePath = allFiles[0];
    }
    
    var fullPath = Path.GetFullPath(fullFilePath);

    if (!fullPath.StartsWith(Path.GetFullPath(musicPath)))
    {
        return Results.Json(new { error = "Доступ запрещен" }, statusCode: 403);
    }

    var downloadFilename = Path.GetFileName(fullFilePath);
    context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{downloadFilename}\"";
    await context.Response.SendFileAsync(fullFilePath);
    return Results.Empty;
});

// Health check
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    uptime = DateTime.UtcNow,
    version = "2.6.0",
    features = new[] { "recursive-scan", "all-depth-levels", "auto-scan", "env-music-dir" },
    musicDirectory = musicPath,
    musicDirectoryExists = Directory.Exists(musicPath)
}));

// ========================================
// STARTUP
// ========================================

var initialTracks = ScanMusicLibrary();

Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║     🎵 K-POP FLAC Music Server (ASP.NET Core)     ║");
Console.WriteLine("║       RECURSIVE SUBDIRECTORY SCANNING             ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"🌐 Server:          http://localhost:5000");
Console.WriteLine($"📁 Music Directory: {musicPath}");
Console.WriteLine($"📊 Tracks Found:    {initialTracks.Count}");
Console.WriteLine($"📂 Scan Mode:       Recursive (All Subdirectories)");
Console.WriteLine();
Console.WriteLine("💡 API Endpoints:");
Console.WriteLine("   GET  /api/music              - List all tracks");
Console.WriteLine("   GET  /api/folders            - List folders with tracks");
Console.WriteLine("   GET  /api/rescan             - Rescan music directory");
Console.WriteLine("   GET  /api/stream/{**path}    - Stream audio file");
Console.WriteLine("   GET  /api/track/{id}         - Track info");
Console.WriteLine("   GET  /api/search?q={query}   - Search tracks");
Console.WriteLine("   GET  /api/artists            - List artists");
Console.WriteLine("   GET  /api/stats              - Library statistics");
Console.WriteLine();

if (initialTracks.Count == 0)
{
    Console.WriteLine("⚠️  Музыкальные файлы не найдены!");
    Console.WriteLine($"   Установите MUSIC_DIR в .env файле");
    Console.WriteLine($"   Пример для Windows: MUSIC_DIR=C:/Users/YourName/Music");
    Console.WriteLine($"   Пример для Linux:   MUSIC_DIR=/home/username/Music");
    Console.WriteLine($"   Текущий путь: {musicPath}");
    Console.WriteLine();
    Console.WriteLine("   💡 Совет: Избегайте кириллицы в путях!");
    Console.WriteLine();
}

app.Run("http://0.0.0.0:5000");
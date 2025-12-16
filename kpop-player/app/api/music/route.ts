import { NextRequest, NextResponse } from 'next/server';
import fs from 'fs';
import path from 'path';

// Функция для извлечения артиста и названия из имени файла
function extractArtistAndTitle(filename: string): { artist: string; title: string } {
  // Убираем расширение
  const nameWithoutExt = filename.replace(/\.flac$/i, '');
  
  // Паттерны для парсинга
  const patterns = [
    /^(.+?)\s*-\s*(.+)$/, // "Artist - Title"
    /^(.+?)_(.+)$/,        // "Artist_Title"
  ];
  
  for (const pattern of patterns) {
    const match = nameWithoutExt.match(pattern);
    if (match) {
      return {
        artist: match[1].trim(),
        title: match[2].trim(),
      };
    }
  }
  
  // Если паттерн не подошел
  return {
    artist: 'Unknown Artist',
    title: nameWithoutExt,
  };
}

// Функция для определения эмодзи на основе артиста
function getEmojiForArtist(artist: string): string {
  const lowerArtist = artist.toLowerCase();
  
  if (lowerArtist.includes('bts')) return '💥';
  if (lowerArtist.includes('blackpink')) return '🖤';
  if (lowerArtist.includes('aespa')) return '🚀';
  if (lowerArtist.includes('ive')) return '🎯';
  if (lowerArtist.includes('newjeans')) return '🐰';
  if (lowerArtist.includes('twice')) return '🍭';
  if (lowerArtist.includes('red velvet')) return '🍰';
  if (lowerArtist.includes('itzy')) return '⚡';
  if (lowerArtist.includes('txt') || lowerArtist.includes('tomorrow')) return '🌟';
  if (lowerArtist.includes('stray kids')) return '🐺';
  if (lowerArtist.includes('seventeen')) return '💎';
  if (lowerArtist.includes('nct')) return '🌱';
  if (lowerArtist.includes('exo')) return '🌙';
  
  return '🎵';
}

// Автоматическое сканирование папки music
function scanMusicLibrary() {
  const musicPath = path.join(process.cwd(), 'public', 'music');

  // Создаем папку если не существует
  if (!fs.existsSync(musicPath)) {
    fs.mkdirSync(musicPath, { recursive: true });
    return [];
  }

  // Получаем все .flac файлы
  const files = fs.readdirSync(musicPath).filter(file => 
    file.toLowerCase().endsWith('.flac')
  );

  // Создаем объекты треков
  const tracks = files.map((filename, index) => {
    const { artist, title } = extractArtistAndTitle(filename);
    const filepath = path.join(musicPath, filename);
    const stats = fs.statSync(filepath);
    
    // Приблизительная длительность по размеру файла
    const megabytes = stats.size / (1024 * 1024);
    const estimatedDuration = Math.round(megabytes * 6.5);

    return {
      id: index + 1,
      title,
      artist,
      filename,
      format: 'FLAC 24bit/96kHz',
      emoji: getEmojiForArtist(artist),
      duration: estimatedDuration,
      available: true,
      fileSize: stats.size,
      lastModified: stats.mtime,
    };
  });

  return tracks;
}

export async function GET() {
  const tracks = scanMusicLibrary();

  return NextResponse.json({
    success: true,
    tracks,
    availableCount: tracks.length,
    totalCount: tracks.length,
    autoScanned: true,
  });
}
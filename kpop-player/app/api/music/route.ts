import { NextRequest, NextResponse } from 'next/server';
import fs from 'fs';
import path from 'path';

// Музыкальная библиотека
const musicLibrary = [
  {
    id: 1,
    title: "Dynamite",
    artist: "BTS",
    filename: "dynamite.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "💥"
  },
  {
    id: 2,
    title: "How You Like That",
    artist: "BLACKPINK",
    filename: "hylt.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "🖤"
  },
  {
    id: 3,
    title: "Next Level",
    artist: "aespa",
    filename: "nextlevel.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "🚀"
  },
  {
    id: 4,
    title: "Butter",
    artist: "BTS",
    filename: "butter.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "🧈"
  },
  {
    id: 5,
    title: "ELEVEN",
    artist: "IVE",
    filename: "eleven.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "🎯"
  },
  {
    id: 6,
    title: "Savage",
    artist: "aespa",
    filename: "savage.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "😈"
  },
  {
    id: 7,
    title: "Pink Venom",
    artist: "BLACKPINK",
    filename: "pinkvenom.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "🐍"
  },
  {
    id: 8,
    title: "Spicy",
    artist: "aespa",
    filename: "spicy.flac",
    format: "FLAC 24bit/96kHz",
    emoji: "🌶️"
  }
];

export async function GET() {
  const musicPath = path.join(process.cwd(), 'public', 'music');

  // Создаем папку если не существует
  if (!fs.existsSync(musicPath)) {
    fs.mkdirSync(musicPath, { recursive: true });
  }

  // Проверяем доступность файлов
  const availableTracks = musicLibrary.filter(track => {
    const filepath = path.join(musicPath, track.filename);
    return fs.existsSync(filepath);
  });

  return NextResponse.json({
    success: true,
    tracks: musicLibrary,
    availableCount: availableTracks.length,
    totalCount: musicLibrary.length
  });
}

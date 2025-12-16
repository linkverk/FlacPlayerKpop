import { NextRequest, NextResponse } from 'next/server';
import fs from 'fs';
import path from 'path';

export async function GET(
  request: NextRequest,
  { params }: { params: { filename: string } }
) {
  const filename = params.filename;
  const musicPath = path.join(process.cwd(), 'public', 'music');
  const filepath = path.join(musicPath, filename);

  // Проверка безопасности
  const resolvedPath = path.resolve(filepath);
  if (!resolvedPath.startsWith(musicPath)) {
    return NextResponse.json(
      { error: 'Доступ запрещен' },
      { status: 403 }
    );
  }

  // Проверка существования файла
  if (!fs.existsSync(filepath)) {
    return NextResponse.json(
      { 
        error: 'Файл не найден',
        message: `Файл ${filename} не найден в папке музыки`
      },
      { status: 404 }
    );
  }

  const stat = fs.statSync(filepath);
  const fileSize = stat.size;
  const range = request.headers.get('range');

  // Поддержка Range requests для стриминга
  if (range) {
    const parts = range.replace(/bytes=/, '').split('-');
    const start = parseInt(parts[0], 10);
    const end = parts[1] ? parseInt(parts[1], 10) : fileSize - 1;
    const chunksize = (end - start) + 1;

    const stream = fs.createReadStream(filepath, { start, end });
    
    return new NextResponse(stream as any, {
      status: 206,
      headers: {
        'Content-Range': `bytes ${start}-${end}/${fileSize}`,
        'Accept-Ranges': 'bytes',
        'Content-Length': chunksize.toString(),
        'Content-Type': 'audio/flac',
      },
    });
  } else {
    const stream = fs.createReadStream(filepath);
    
    return new NextResponse(stream as any, {
      status: 200,
      headers: {
        'Content-Length': fileSize.toString(),
        'Content-Type': 'audio/flac',
      },
    });
  }
}

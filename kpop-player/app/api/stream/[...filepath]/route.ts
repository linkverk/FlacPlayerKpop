import { NextRequest, NextResponse } from 'next/server';

export async function GET(
  request: NextRequest,
  { params }: { params: { filepath: string[] } }
) {
  // Собираем полный путь из всех сегментов (поддержка подпапок)
  const filepath = params.filepath.join('/');
  const backendUrl = process.env.BACKEND_URL || 'http://localhost:5000';
  
  console.log('🎵 Stream request:', {
    filepath: filepath,
    segments: params.filepath,
    backendUrl: backendUrl
  });
  
  try {
    const range = request.headers.get('range');
    const headers: HeadersInit = {};
    
    if (range) {
      headers['Range'] = range;
    }
    
    // ИСПРАВЛЕНИЕ: НЕ кодируем путь вручную!
    // fetch() автоматически закодирует URL правильно
    const backendApiUrl = `${backendUrl}/api/stream/${filepath}`;
    
    console.log('📡 Fetching from backend:', {
      originalPath: filepath,
      fullUrl: backendApiUrl
    });
    
    const response = await fetch(backendApiUrl, { headers });
    
    if (!response.ok) {
      console.error('❌ Backend returned error:', {
        status: response.status,
        filepath: filepath,
        url: backendApiUrl
      });
      
      return NextResponse.json(
        { 
          error: 'Файл не найден',
          message: `Не удалось получить файл ${filepath} с backend`,
          status: response.status,
          originalPath: filepath,
          backendUrl: backendApiUrl
        },
        { status: response.status }
      );
    }
    
    // Копируем все заголовки от backend
    const responseHeaders = new Headers();
    response.headers.forEach((value, key) => {
      responseHeaders.set(key, value);
    });
    
    console.log('✅ Stream success:', {
      filepath: filepath,
      contentType: response.headers.get('content-type'),
      contentLength: response.headers.get('content-length'),
      status: response.status
    });
    
    return new NextResponse(response.body, {
      status: response.status,
      headers: responseHeaders,
    });
  } catch (error) {
    console.error('❌ Stream proxy error:', error);
    return NextResponse.json(
      { 
        error: 'Backend недоступен',
        message: 'Не удалось подключиться к серверу музыки',
        filepath: filepath,
        details: String(error)
      },
      { status: 503 }
    );
  }
}
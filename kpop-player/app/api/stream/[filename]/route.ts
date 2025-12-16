import { NextRequest, NextResponse } from 'next/server';

export async function GET(
  request: NextRequest,
  { params }: { params: { filename: string } }
) {
  const filename = params.filename;
  const backendUrl = process.env.BACKEND_URL || 'http://localhost:5000';
  
  try {
    // Проксируем запрос к backend
    const range = request.headers.get('range');
    const headers: HeadersInit = {};
    
    if (range) {
      headers['Range'] = range;
    }
    
    const response = await fetch(
      `${backendUrl}/api/stream/${encodeURIComponent(filename)}`,
      { headers }
    );
    
    if (!response.ok) {
      return NextResponse.json(
        { 
          error: 'Файл не найден',
          message: `Не удалось получить файл ${filename} с backend`
        },
        { status: response.status }
      );
    }
    
    // Копируем все заголовки от backend
    const responseHeaders = new Headers();
    response.headers.forEach((value, key) => {
      responseHeaders.set(key, value);
    });
    
    return new NextResponse(response.body, {
      status: response.status,
      headers: responseHeaders,
    });
  } catch (error) {
    console.error('Stream proxy error:', error);
    return NextResponse.json(
      { 
        error: 'Backend недоступен',
        message: 'Не удалось подключиться к серверу музыки'
      },
      { status: 503 }
    );
  }
}
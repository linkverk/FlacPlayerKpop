import { NextRequest, NextResponse } from 'next/server';

export async function GET() {
  try {
    // ИСПРАВЛЕНИЕ: Используем правильный backend URL
    // Внутри Docker: http://backend:5000
    // Из браузера через Next.js proxy: используем переменную окружения
    const backendUrl = process.env.BACKEND_URL || 'http://localhost:5000';
    
    console.log('🎵 Fetching music from backend:', backendUrl);
    
    const response = await fetch(`${backendUrl}/api/music`, {
      // Добавляем кэш для ускорения
      cache: 'no-store'
    });
    
    if (!response.ok) {
      console.error('❌ Backend returned error:', response.status);
      throw new Error(`Backend returned ${response.status}`);
    }
    
    const data = await response.json();
    
    console.log('✅ Music loaded successfully:', {
      tracks: data.tracks?.length || 0,
      availableCount: data.availableCount
    });
    
    return NextResponse.json(data);
  } catch (error) {
    console.error('❌ Failed to fetch from backend:', error);
    
    // Возвращаем пустой список если backend недоступен
    return NextResponse.json({
      success: false,
      tracks: [],
      availableCount: 0,
      totalCount: 0,
      error: 'Backend unavailable',
      message: String(error)
    });
  }
}
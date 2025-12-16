import { NextRequest, NextResponse } from 'next/server';

export async function GET() {
  try {
    // Используем backend API вместо локальных файлов
    const backendUrl = process.env.BACKEND_URL || 'http://localhost:5000';
    const response = await fetch(`${backendUrl}/api/music`);
    
    if (!response.ok) {
      throw new Error(`Backend returned ${response.status}`);
    }
    
    const data = await response.json();
    return NextResponse.json(data);
  } catch (error) {
    console.error('Failed to fetch from backend:', error);
    
    // Возвращаем пустой список если backend недоступен
    return NextResponse.json({
      success: false,
      tracks: [],
      availableCount: 0,
      totalCount: 0,
      error: 'Backend unavailable'
    });
  }
}
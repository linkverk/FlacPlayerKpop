import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'K-POP FLAC Player | Hi-Res Audio Streaming',
  description: 'Stream your favorite K-POP tracks in high-resolution FLAC format',
  keywords: ['k-pop', 'music', 'flac', 'hi-res', 'audio', 'streaming'],
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="ru">
      <body>{children}</body>
    </html>
  );
}

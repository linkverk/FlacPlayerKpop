'use client';

import { useState, useEffect } from 'react';
import { Track } from '@/types';
import AudioPlayer from '@/components/AudioPlayer';
import TrackCard from '@/components/TrackCard';
import ServerStatus from '@/components/ServerStatus';
import { motion } from 'framer-motion';
import { Music2, Sparkles } from 'lucide-react';

export default function Home() {
  const [playlist, setPlaylist] = useState<Track[]>([]);
  const [currentTrackIndex, setCurrentTrackIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [serverConnected, setServerConnected] = useState(false);

  useEffect(() => {
    loadMusicLibrary();
  }, []);

  const loadMusicLibrary = async () => {
    try {
      const response = await fetch('/api/music');
      const data = await response.json();
      
      if (data.success) {
        setPlaylist(data.tracks);
        setServerConnected(data.availableCount > 0);
      }
    } catch (error) {
      console.error('Failed to load music library:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleTrackSelect = (index: number) => {
    setCurrentTrackIndex(index);
  };

  return (
    <div className="min-h-screen cosmic-bg pb-40">
      {/* Animated background orbs */}
      <div className="fixed inset-0 overflow-hidden pointer-events-none">
        <motion.div
          animate={{
            x: [0, 100, 0],
            y: [0, -100, 0],
            scale: [1, 1.2, 1],
          }}
          transition={{
            duration: 20,
            repeat: Infinity,
            ease: "easeInOut"
          }}
          className="absolute top-20 left-20 w-96 h-96 rounded-full bg-neon-pink/20 blur-3xl"
        />
        <motion.div
          animate={{
            x: [0, -100, 0],
            y: [0, 100, 0],
            scale: [1, 1.3, 1],
          }}
          transition={{
            duration: 25,
            repeat: Infinity,
            ease: "easeInOut"
          }}
          className="absolute bottom-20 right-20 w-96 h-96 rounded-full bg-neon-cyan/20 blur-3xl"
        />
        <motion.div
          animate={{
            x: [0, 50, 0],
            y: [0, -50, 0],
            scale: [1, 1.1, 1],
          }}
          transition={{
            duration: 15,
            repeat: Infinity,
            ease: "easeInOut"
          }}
          className="absolute top-1/2 left-1/2 w-96 h-96 rounded-full bg-neon-purple/20 blur-3xl"
        />
      </div>

      <div className="relative z-10 max-w-7xl mx-auto px-4 sm:px-6 py-8">
        {/* Header */}
        <motion.header
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center mb-12"
        >
          <motion.div
            animate={{
              rotate: [0, 5, -5, 0],
            }}
            transition={{
              duration: 4,
              repeat: Infinity,
              ease: "easeInOut"
            }}
            className="inline-flex items-center gap-3 mb-4"
          >
            <Music2 className="w-12 h-12 text-neon-pink" />
            <Sparkles className="w-8 h-8 text-neon-cyan" />
          </motion.div>

          <h1 className="font-display text-6xl md:text-8xl font-black mb-4">
            <span className="holographic bg-clip-text text-transparent neon-text">
              K-POP FLAC
            </span>
          </h1>
          
          <p className="text-xl md:text-2xl text-gray-300 mb-2 font-body">
            Hi-Resolution Audio Streaming
          </p>
          
          <motion.div
            animate={{
              opacity: [0.5, 1, 0.5],
            }}
            transition={{
              duration: 3,
              repeat: Infinity,
            }}
            className="inline-flex items-center gap-2 text-neon-cyan text-sm"
          >
            <div className="w-2 h-2 rounded-full bg-neon-cyan animate-pulse-glow" />
            <span className="font-semibold">24bit / 96kHz Audiophile Quality</span>
          </motion.div>
        </motion.header>

        {/* Server Status */}
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ delay: 0.2 }}
          className="mb-8"
        >
          <ServerStatus 
            connected={serverConnected} 
            serverUrl="Локальный сервер" 
          />
        </motion.div>

        {/* Loading State */}
        {loading ? (
          <div className="flex flex-col items-center justify-center py-20">
            <div className="spinner mb-4" />
            <p className="text-gray-400 text-lg">Загрузка библиотеки...</p>
          </div>
        ) : (
          <>
            {/* Stats */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 0.3 }}
              className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8"
            >
              <div className="glass rounded-xl p-4 text-center">
                <div className="text-3xl font-display font-bold holographic bg-clip-text text-transparent mb-1">
                  {playlist.length}
                </div>
                <div className="text-gray-400 text-sm">Треков</div>
              </div>
              <div className="glass rounded-xl p-4 text-center">
                <div className="text-3xl font-display font-bold text-neon-cyan mb-1">
                  FLAC
                </div>
                <div className="text-gray-400 text-sm">Формат</div>
              </div>
              <div className="glass rounded-xl p-4 text-center">
                <div className="text-3xl font-display font-bold text-neon-pink mb-1">
                  24bit
                </div>
                <div className="text-gray-400 text-sm">Глубина</div>
              </div>
              <div className="glass rounded-xl p-4 text-center">
                <div className="text-3xl font-display font-bold text-neon-purple mb-1">
                  96kHz
                </div>
                <div className="text-gray-400 text-sm">Частота</div>
              </div>
            </motion.div>

            {/* Track Grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-8">
              {playlist.map((track, index) => (
                <TrackCard
                  key={track.id}
                  track={track}
                  isActive={index === currentTrackIndex}
                  onClick={() => handleTrackSelect(index)}
                  index={index}
                />
              ))}
            </div>

            {/* Info Card */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.5 }}
              className="glass rounded-2xl p-6 text-center"
            >
              <h3 className="text-white font-display text-xl font-bold mb-2">
                🎧 Инструкция
              </h3>
              <p className="text-gray-400 text-sm max-w-2xl mx-auto">
                Добавьте ваши FLAC файлы в папку <code className="px-2 py-1 bg-white/10 rounded">public/music/</code> 
                {' '}и обновите страницу. Переименуйте файлы согласно именам в API (dynamite.flac, hylt.flac и т.д.)
              </p>
            </motion.div>
          </>
        )}
      </div>

      {/* Audio Player */}
      {playlist.length > 0 && (
        <AudioPlayer
          playlist={playlist}
          currentTrackIndex={currentTrackIndex}
          setCurrentTrackIndex={setCurrentTrackIndex}
        />
      )}
    </div>
  );
}

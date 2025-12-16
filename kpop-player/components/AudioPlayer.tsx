'use client';

import { useEffect, useRef, useState } from 'react';
import { Track, PlayerState } from '@/types';
import { Play, Pause, SkipBack, SkipForward, Volume2, VolumeX } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

interface AudioPlayerProps {
  playlist: Track[];
  currentTrackIndex: number;
  setCurrentTrackIndex: (index: number) => void;
}

export default function AudioPlayer({ 
  playlist, 
  currentTrackIndex, 
  setCurrentTrackIndex 
}: AudioPlayerProps) {
  const audioRef = useRef<HTMLAudioElement>(null);
  const [playerState, setPlayerState] = useState<PlayerState>({
    currentTrack: null,
    isPlaying: false,
    currentTime: 0,
    duration: 0,
    volume: 1,
    currentTrackIndex: 0,
  });

  const [isMuted, setIsMuted] = useState(false);
  const [visualizerBars, setVisualizerBars] = useState<number[]>(
    Array(10).fill(0).map(() => Math.random())
  );

  useEffect(() => {
    if (playlist.length > 0 && currentTrackIndex >= 0) {
      const track = playlist[currentTrackIndex];
      setPlayerState(prev => ({ ...prev, currentTrack: track, currentTrackIndex }));
      
      if (audioRef.current) {
        audioRef.current.src = `/api/stream/${track.filename}`;
        audioRef.current.load();
      }
    }
  }, [currentTrackIndex, playlist]);

  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (playerState.isPlaying) {
      interval = setInterval(() => {
        setVisualizerBars(Array(10).fill(0).map(() => Math.random()));
      }, 150);
    }
    return () => clearInterval(interval);
  }, [playerState.isPlaying]);

  const togglePlay = async () => {
    if (!audioRef.current || !playerState.currentTrack) return;

    try {
      if (playerState.isPlaying) {
        audioRef.current.pause();
      } else {
        await audioRef.current.play();
      }
      setPlayerState(prev => ({ ...prev, isPlaying: !prev.isPlaying }));
    } catch (error) {
      console.error('Playback error:', error);
    }
  };

  const handleTimeUpdate = () => {
    if (audioRef.current) {
      setPlayerState(prev => ({
        ...prev,
        currentTime: audioRef.current!.currentTime,
        duration: audioRef.current!.duration || 0,
      }));
    }
  };

  const handleSeek = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!audioRef.current) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const percent = (e.clientX - rect.left) / rect.width;
    audioRef.current.currentTime = percent * audioRef.current.duration;
  };

  const handleVolumeChange = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!audioRef.current) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const percent = (e.clientX - rect.left) / rect.width;
    const newVolume = Math.max(0, Math.min(1, percent));
    audioRef.current.volume = newVolume;
    setPlayerState(prev => ({ ...prev, volume: newVolume }));
    setIsMuted(newVolume === 0);
  };

  const toggleMute = () => {
    if (!audioRef.current) return;
    if (isMuted) {
      audioRef.current.volume = playerState.volume || 0.5;
      setIsMuted(false);
    } else {
      audioRef.current.volume = 0;
      setIsMuted(true);
    }
  };

  const previousTrack = () => {
    const newIndex = currentTrackIndex > 0 ? currentTrackIndex - 1 : playlist.length - 1;
    setCurrentTrackIndex(newIndex);
  };

  const nextTrack = () => {
    const newIndex = currentTrackIndex < playlist.length - 1 ? currentTrackIndex + 1 : 0;
    setCurrentTrackIndex(newIndex);
  };

  const handleEnded = () => {
    nextTrack();
  };

  const formatTime = (seconds: number): string => {
    if (isNaN(seconds)) return '0:00';
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const progress = (playerState.currentTime / playerState.duration) * 100 || 0;

  return (
    <motion.div
      initial={{ y: 100, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      className="fixed bottom-0 left-0 right-0 z-50 backdrop-blur-2xl bg-black/80 border-t border-white/10"
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 py-4">
        {/* Visualizer */}
        <div className="flex items-end justify-center gap-1 h-16 mb-4">
          {visualizerBars.map((height, i) => (
            <motion.div
              key={i}
              className="w-1 rounded-full visualizer-bar"
              animate={{
                height: playerState.isPlaying ? `${height * 50 + 10}px` : '4px',
              }}
              transition={{ duration: 0.3 }}
              style={{
                background: 'linear-gradient(to top, #FF10F0, #00F0FF)',
              }}
            />
          ))}
        </div>

        {/* Now Playing Info */}
        <AnimatePresence mode="wait">
          {playerState.currentTrack && (
            <motion.div
              key={playerState.currentTrack.id}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="flex items-center gap-4 mb-4"
            >
              <div className="w-14 h-14 rounded-xl holographic flex items-center justify-center text-2xl">
                {playerState.currentTrack.emoji}
              </div>
              <div className="flex-1 min-w-0">
                <h3 className="text-white font-display font-bold text-lg truncate">
                  {playerState.currentTrack.title}
                </h3>
                <p className="text-gray-400 text-sm truncate">
                  {playerState.currentTrack.artist}
                </p>
              </div>
              <div className="hidden sm:block px-3 py-1 rounded-lg bg-gradient-to-r from-neon-pink/20 to-neon-cyan/20 border border-neon-cyan/30">
                <span className="text-xs text-neon-cyan font-semibold">
                  {playerState.currentTrack.format}
                </span>
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Controls */}
        <div className="flex items-center gap-4 mb-3">
          <button
            type="button"
            onClick={previousTrack}
            className="control-btn p-2 rounded-full glass hover:bg-white/10 transition-colors"
            title="Previous Track"
            aria-label="Previous Track"
          >
            <SkipBack className="w-5 h-5 text-white" />
          </button>

          <button
            type="button"
            onClick={togglePlay}
            disabled={!playerState.currentTrack}
            className="control-btn p-4 rounded-full holographic disabled:opacity-50 disabled:cursor-not-allowed"
            title={playerState.isPlaying ? "Pause" : "Play"}
            aria-label={playerState.isPlaying ? "Pause" : "Play"}
          >
            {playerState.isPlaying ? (
              <Pause className="w-6 h-6 text-white" fill="white" />
            ) : (
              <Play className="w-6 h-6 text-white" fill="white" />
            )}
          </button>

          <button
            type="button"
            onClick={nextTrack}
            className="control-btn p-2 rounded-full glass hover:bg-white/10 transition-colors"
            title="Next Track"
            aria-label="Next Track"
          >
            <SkipForward className="w-5 h-5 text-white" />
          </button>

          {/* Progress Bar */}
          <div className="flex-1 progress-container">
            <div 
              className="progress-bar cursor-pointer"
              onClick={handleSeek}
            >
              <div 
                className="progress-fill"
                style={{ width: `${progress}%` }}
              />
            </div>
            <div className="flex justify-between text-xs text-gray-400 mt-1">
              <span>{formatTime(playerState.currentTime)}</span>
              <span>{formatTime(playerState.duration)}</span>
            </div>
          </div>

          {/* Volume Control */}
          <div className="hidden md:flex items-center gap-2">
            <button
              type="button"
              onClick={toggleMute}
              className="p-2 rounded-full hover:bg-white/10 transition-colors"
              title={isMuted || playerState.volume === 0 ? "Unmute" : "Mute"}
              aria-label={isMuted || playerState.volume === 0 ? "Unmute" : "Mute"}
            >
              {isMuted || playerState.volume === 0 ? (
                <VolumeX className="w-5 h-5 text-white" />
              ) : (
                <Volume2 className="w-5 h-5 text-white" />
              )}
            </button>
            <div 
              className="w-24 h-1 rounded-full bg-white/20 cursor-pointer"
              onClick={handleVolumeChange}
            >
              <div 
                className="h-full rounded-full bg-gradient-to-r from-neon-pink to-neon-cyan"
                style={{ width: `${isMuted ? 0 : playerState.volume * 100}%` }}
              />
            </div>
          </div>
        </div>
      </div>

      <audio
        ref={audioRef}
        onTimeUpdate={handleTimeUpdate}
        onEnded={handleEnded}
        onPlay={() => setPlayerState(prev => ({ ...prev, isPlaying: true }))}
        onPause={() => setPlayerState(prev => ({ ...prev, isPlaying: false }))}
      />
    </motion.div>
  );
}

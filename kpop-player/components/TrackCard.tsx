'use client';

import { Track } from '@/types';
import { motion } from 'framer-motion';
import { Play } from 'lucide-react';

interface TrackCardProps {
  track: Track;
  isActive: boolean;
  onClick: () => void;
  index: number;
}

export default function TrackCard({ track, isActive, onClick, index }: TrackCardProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: index * 0.05 }}
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
      onClick={onClick}
      className={`
        track-card glass rounded-2xl p-6 cursor-pointer
        ${isActive ? 'ring-2 ring-neon-cyan shadow-lg shadow-neon-cyan/50' : ''}
      `}
    >
      {/* Album Art */}
      <div className="relative mb-4 aspect-square rounded-xl overflow-hidden holographic flex items-center justify-center text-6xl">
        {track.emoji}
        
        {/* Play overlay on hover */}
        <motion.div
          initial={{ opacity: 0 }}
          whileHover={{ opacity: 1 }}
          className="absolute inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center"
        >
          <motion.div
            whileHover={{ scale: 1.1 }}
            className="w-16 h-16 rounded-full bg-white/20 flex items-center justify-center"
          >
            <Play className="w-8 h-8 text-white" fill="white" />
          </motion.div>
        </motion.div>
      </div>

      {/* Track Info */}
      <div className="space-y-2">
        <h3 className="text-white font-display font-bold text-lg truncate">
          {track.title}
        </h3>
        <p className="text-gray-400 text-sm truncate">
          {track.artist}
        </p>
        
        {/* Format Badge */}
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-lg bg-gradient-to-r from-neon-pink/20 to-neon-purple/20 border border-neon-pink/30">
          <div className="w-2 h-2 rounded-full bg-neon-pink animate-pulse-glow" />
          <span className="text-xs text-white font-semibold">
            {track.format}
          </span>
        </div>
      </div>

      {/* Active indicator */}
      {isActive && (
        <motion.div
          initial={{ width: 0 }}
          animate={{ width: '100%' }}
          className="h-1 bg-gradient-to-r from-neon-pink via-neon-purple to-neon-cyan rounded-full mt-4"
        />
      )}
    </motion.div>
  );
}

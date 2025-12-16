'use client';

import { motion } from 'framer-motion';
import { Wifi, WifiOff } from 'lucide-react';

interface ServerStatusProps {
  connected: boolean;
  serverUrl: string;
}

export default function ServerStatus({ connected, serverUrl }: ServerStatusProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: -10 }}
      animate={{ opacity: 1, y: 0 }}
      className="glass rounded-2xl p-4 flex items-center gap-3"
    >
      <motion.div
        animate={{
          scale: connected ? [1, 1.2, 1] : 1,
        }}
        transition={{
          duration: 2,
          repeat: connected ? Infinity : 0,
        }}
      >
        {connected ? (
          <Wifi className="w-5 h-5 text-neon-cyan" />
        ) : (
          <WifiOff className="w-5 h-5 text-red-400" />
        )}
      </motion.div>

      <div className="flex-1">
        <div className="flex items-center gap-2">
          <motion.div
            className={`w-2 h-2 rounded-full ${
              connected ? 'bg-neon-cyan' : 'bg-red-400'
            }`}
            animate={{
              opacity: connected ? [1, 0.3, 1] : 1,
            }}
            transition={{
              duration: 2,
              repeat: connected ? Infinity : 0,
            }}
          />
          <span className="text-white text-sm font-semibold">
            {connected ? 'Подключено' : 'Автономный режим'}
          </span>
        </div>
        <p className="text-gray-400 text-xs mt-1">
          {connected ? serverUrl : 'Загрузите FLAC файлы для работы'}
        </p>
      </div>
    </motion.div>
  );
}

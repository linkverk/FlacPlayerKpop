export interface Track {
  id: number;
  title: string;
  artist: string;
  filename: string;
  format: string;
  emoji: string;
  duration?: number;
  available?: boolean;
}

export interface PlayerState {
  currentTrack: Track | null;
  isPlaying: boolean;
  currentTime: number;
  duration: number;
  volume: number;
  currentTrackIndex: number;
}

export interface ServerConfig {
  url: string;
  connected: boolean;
}

export interface MusicLibraryResponse {
  success: boolean;
  tracks: Track[];
  availableCount: number;
  totalCount: number;
}

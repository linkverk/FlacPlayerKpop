namespace KPopFlacMusicServer.Models;

public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public int Duration { get; set; }
    public bool Available { get; set; }
    public long? FileSize { get; set; }
    public DateTime? LastModified { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class MusicLibraryResponse
{
    public bool Success { get; set; }
    public Track[] Tracks { get; set; } = Array.Empty<Track>();
    public int AvailableCount { get; set; }
    public int TotalCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class SearchResult
{
    public bool Success { get; set; }
    public string Query { get; set; } = string.Empty;
    public int Count { get; set; }
    public Track[] Results { get; set; } = Array.Empty<Track>();
}

public class LibraryStats
{
    public int TotalTracks { get; set; }
    public int AvailableTracks { get; set; }
    public int UnavailableTracks { get; set; }
    public int TotalDurationSeconds { get; set; }
    public string TotalDurationFormatted { get; set; } = string.Empty;
    public int UniqueArtists { get; set; }
    public int AverageTrackDuration { get; set; }
}

public class ArtistInfo
{
    public string Name { get; set; } = string.Empty;
    public int TrackCount { get; set; }
    public List<TrackSummary> Tracks { get; set; } = new();
}

public class TrackSummary
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class FormatInfo
{
    public string Format { get; set; } = string.Empty;
    public int Count { get; set; }
}

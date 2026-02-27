namespace DominoShared.Models;

/// <summary>
/// Represents a game room.
/// Server maintains instances, clients receive summaries.
/// </summary>
public class Room
{
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public int MaxPlayers { get; set; } = 2;
    public int WinningScore { get; set; } = 100; // Points limit to win
    public List<Player> Players { get; set; } = new();
    public List<Player> Watchers { get; set; } = new();
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool CanJoin() => Players.Count < MaxPlayers && Status == GameStatus.Waiting;
    public bool CanWatch() => Status == GameStatus.Playing;
    public bool IsFull() => Players.Count >= MaxPlayers;
}

public enum GameStatus
{
    Waiting,
    Playing,
    Finished
}

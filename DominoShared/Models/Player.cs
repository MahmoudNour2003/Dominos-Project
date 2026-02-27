namespace DominoShared.Models;

/// <summary>
/// Represents a player in the game system.
/// Dev 2 will extend this for game-specific properties.
/// </summary>
public class Player
{
    public string Username { get; set; } = string.Empty;
    public int TotalScore { get; set; } = 0;
    public bool IsConnected { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

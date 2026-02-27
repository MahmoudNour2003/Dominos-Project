using DominoShared.Models;

namespace DominoShared.Engine;

/// <summary>
/// Complete game state that gets synchronized to all players and watchers.
/// Dev 2 will refine this with additional game logic properties.
/// This is the single source of truth for the current game state.
/// </summary>
public class GameState
{
    /// <summary>
    /// Which room this game belongs to
    /// </summary>
    public string RoomName { get; set; } = string.Empty;

    /// <summary>
    /// All players in the game (in turn order)
    /// </summary>
    public List<PlayerGameState> Players { get; set; } = new();

    /// <summary>
    /// Index of the player whose turn it is (0-based)
    /// </summary>
    public int CurrentPlayerIndex { get; set; } = 0;

    /// <summary>
    /// Cards currently on the table
    /// </summary>
    public List<DominoCard> TableCards { get; set; } = new();

    /// <summary>
    /// Number of cards remaining in the side deck
    /// </summary>
    public int SideDeckCount { get; set; } = 0;

    /// <summary>
    /// Current scores for each player (username -> score)
    /// </summary>
    public Dictionary<string, int> CurrentScores { get; set; } = new();

    /// <summary>
    /// Total accumulated scores for each player (username -> total)
    /// </summary>
    public Dictionary<string, int> TotalScores { get; set; } = new();

    /// <summary>
    /// Cards in each player's hand (only the current player sees all cards)
    /// Other players see only the count
    /// </summary>
    public Dictionary<string, List<DominoCard>> PlayerHands { get; set; } = new();

    /// <summary>
    /// Card count per player (visible to all for fairness)
    /// </summary>
    public Dictionary<string, int> PlayerCardCounts { get; set; } = new();

    /// <summary>
    /// Is the game currently active
    /// </summary>
    public bool IsGameActive { get; set; } = false;

    /// <summary>
    /// Has the current round finished
    /// </summary>
    public bool IsRoundFinished { get; set; } = false;

    /// <summary>
    /// Who won (null if game still active)
    /// </summary>
    public string? Winner { get; set; }

    /// <summary>
    /// Server timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Get current active player
    /// </summary>
    public string GetCurrentPlayerUsername()
    {
        if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < Players.Count)
        {
            return Players[CurrentPlayerIndex].Username;
        }
        return string.Empty;
    }
}

/// <summary>
/// Player info in a game (different from general Player class)
/// </summary>
public class PlayerGameState
{
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false; // Currently can play
    public int CardsInHand { get; set; } = 0;
    public bool PassedThisRound { get; set; } = false;
}

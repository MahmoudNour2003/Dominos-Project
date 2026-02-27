using DominoShared.Models;

namespace DominoShared.Engine;

/// <summary>
/// Interface for game rules engine.
/// Dev 2 implements this with the actual domino game logic.
/// Dev 1 calls these methods to validate moves.
/// </summary>
public interface IRulesEngine
{
    /// <summary>
    /// Check if a card can be played on the current table.
    /// A valid move is when card matches either end of the table.
    /// </summary>
    bool IsValidMove(DominoCard card, List<DominoCard> tableCards);

    /// <summary>
    /// Check if a player can pass (only when side deck is empty)
    /// </summary>
    bool CanPass(int sideDeckCount);

    /// <summary>
    /// Calculate points for a single round based on remaining cards in hand.
    /// Sum of all card values (left + right) for each card.
    /// </summary>
    int CalculateRoundPoints(List<DominoCard> hand);

    /// <summary>
    /// Determine which end of the table a card can match.
    /// Returns: "LEFT", "RIGHT", or "BOTH"
    /// </summary>
    string GetValidEnd(DominoCard card, List<DominoCard> tableCards);
}

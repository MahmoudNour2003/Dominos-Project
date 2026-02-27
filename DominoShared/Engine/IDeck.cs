using DominoShared.Models;

namespace DominoShared.Engine;

/// <summary>
/// Interface for deck management.
/// Dev 2 implements this with actual card shuffling and dealing logic.
/// Dev 1 calls these methods at game start and when drawing.
/// </summary>
public interface IDeck
{
    /// <summary>
    /// Generate a standard domino deck (28 cards: 0-0 through 6-6)
    /// </summary>
    List<DominoCard> GenerateDeck();

    /// <summary>
    /// Shuffle the deck randomly
    /// </summary>
    void Shuffle();

    /// <summary>
    /// Draw a specific number of cards from the deck
    /// </summary>
    List<DominoCard> DrawCards(int count);

    /// <summary>
    /// Get remaining cards in deck
    /// </summary>
    int RemainingCards { get; }

    /// <summary>
    /// Reset the deck (for new round)
    /// </summary>
    void Reset();
}

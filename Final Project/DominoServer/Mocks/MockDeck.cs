using DominoShared.Engine;
using DominoShared.Models;

namespace DominoServer.Mocks;

/// <summary>
/// Temporary mock implementation of IDeck for testing.
/// Replace with Dev 2's real implementation.
/// </summary>
public class MockDeck : IDeck
{
    private List<DominoCard> _cards = new();
    private int _nextCardIndex = 0;

    public int RemainingCards => _cards.Count - _nextCardIndex;

    public List<DominoCard> GenerateDeck()
    {
        var deck = new List<DominoCard>();
        
        // Create a standard domino set (0-0 to 6-6)
        for (int i = 0; i <= 6; i++)
        {
            for (int j = i; j <= 6; j++)
            {
                deck.Add(new DominoCard { LeftValue = i, RightValue = j });
            }
        }
        
        _cards = deck;
        return deck;
    }

    public void Shuffle()
    {
        var random = new Random();
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
        _nextCardIndex = 0;
    }

    public List<DominoCard> DrawCards(int count)
    {
        var drawnCards = new List<DominoCard>();
        for (int i = 0; i < count && _nextCardIndex < _cards.Count; i++)
        {
            drawnCards.Add(_cards[_nextCardIndex++]);
        }
        return drawnCards;
    }

    public void Reset()
    {
        _nextCardIndex = 0;
        GenerateDeck();
        Shuffle();
    }
}

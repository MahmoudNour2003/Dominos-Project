using DominoShared.Engine;
using DominoShared.Models;

namespace DominoServer.Mocks;

/// <summary>
/// Temporary mock implementation of IRulesEngine for testing.
/// Replace with Dev 2's real implementation.
/// </summary>
public class MockRulesEngine : IRulesEngine
{
    public bool IsValidMove(DominoCard card, List<DominoCard> tableCards)
    {
        // If board is empty, any card is valid
        if (tableCards == null || tableCards.Count == 0)
            return true;

        // Get the ends of the domino chain
        var leftEnd = tableCards.First().LeftValue;
        var rightEnd = tableCards.Last().RightValue;

        // Check if the card can connect to either end
        return card.LeftValue == leftEnd || card.RightValue == leftEnd ||
               card.LeftValue == rightEnd || card.RightValue == rightEnd;
    }

    public bool CanPass(int sideDeckCount)
    {
        // Player can only pass if side deck is empty
        return sideDeckCount == 0;
    }

    public int CalculateRoundPoints(List<DominoCard> hand)
    {
        if (hand == null)
            return 0;

        return hand.Sum(card => card.LeftValue + card.RightValue);
    }

    public string GetValidEnd(DominoCard card, List<DominoCard> tableCards)
    {
        if (tableCards == null || tableCards.Count == 0)
            return "BOTH";

        var leftEnd = tableCards.First().LeftValue;
        var rightEnd = tableCards.Last().RightValue;

        bool canPlayLeft = card.LeftValue == leftEnd || card.RightValue == leftEnd;
        bool canPlayRight = card.LeftValue == rightEnd || card.RightValue == rightEnd;

        if (canPlayLeft && canPlayRight)
            return "BOTH";
        else if (canPlayLeft)
            return "LEFT";
        else if (canPlayRight)
            return "RIGHT";
        else
            return "NONE";
    }
}

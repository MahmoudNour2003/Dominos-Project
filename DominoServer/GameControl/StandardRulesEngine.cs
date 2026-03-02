using DominoShared.Engine;
using DominoShared.Models;

namespace DominoServer.GameControl;

/// <summary>
/// Concrete implementation of IRulesEngine with standard domino rules.
/// This is a conservative, well-documented first pass implementing common domino behavior
/// required by the GameOrchestrator: move validation, pass policy and simple scoring.
/// </summary>
public class StandardRulesEngine : IRulesEngine
{
    public bool IsValidMove(DominoCard card, List<DominoCard> tableCards)
    {
        if (card == null) return false;

        // If board is empty any card is valid
        if (tableCards == null || tableCards.Count == 0)
            return true;

        var leftEnd = tableCards.First().LeftValue;
        var rightEnd = tableCards.Last().RightValue;

        // A card is valid if one of its ends matches either end of the chain
        return card.LeftValue == leftEnd || card.RightValue == leftEnd ||
               card.LeftValue == rightEnd || card.RightValue == rightEnd;
    }

    public bool CanPass(int sideDeckCount)
    {
        // Standard simple rule: players can only pass when there are no cards left in the side deck.
        return sideDeckCount == 0;
    }

    public int CalculateRoundPoints(List<DominoCard> hand)
    {
        if (hand == null) return 0;

        return hand.Sum(c => c.LeftValue + c.RightValue);
    }

    public string GetValidEnd(DominoCard card, List<DominoCard> tableCards)
    {
        if (card == null) return "NONE";
        if (tableCards == null || tableCards.Count == 0) return "BOTH";

        var leftEnd = tableCards.First().LeftValue;
        var rightEnd = tableCards.Last().RightValue;

        bool canPlayLeft = card.LeftValue == leftEnd || card.RightValue == leftEnd;
        bool canPlayRight = card.LeftValue == rightEnd || card.RightValue == rightEnd;

        if (canPlayLeft && canPlayRight) return "BOTH";
        if (canPlayLeft) return "LEFT";
        if (canPlayRight) return "RIGHT";
        return "NONE";
    }
}

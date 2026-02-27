namespace DominoShared.Models;

/// <summary>
/// Represents a single domino card with two sides.
/// Dev 2 will complete the full implementation.
/// Dev 1 & Dev 3 use this for client/server communication.
/// </summary>
public class DominoCard
{
    public int LeftValue { get; set; }
    public int RightValue { get; set; }

    public DominoCard() { }

    public DominoCard(int left, int right)
    {
        LeftValue = left;
        RightValue = right;
    }

    public override string ToString() => $"[{LeftValue}|{RightValue}]";

    /// <summary>
    /// Get the sum of both sides (used for scoring)
    /// </summary>
    public int GetValue() => LeftValue + RightValue;
}

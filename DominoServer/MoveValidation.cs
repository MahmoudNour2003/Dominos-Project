namespace DominoServer;

public interface IMoveValidator
{
    bool IsMoveValid(Room room, Player player, string action, string payload, out string reason);
}

public class PassThroughMoveValidator : IMoveValidator
{
    public bool IsMoveValid(Room room, Player player, string action, string payload, out string reason)
    {
        reason = string.Empty;
        return true;
    }
}

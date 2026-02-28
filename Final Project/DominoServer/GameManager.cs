using System.Text;

namespace DominoServer;

public class GameManager(IMoveValidator moveValidator)
{
    private readonly IMoveValidator _moveValidator = moveValidator;

    public bool StartGame(Room room, out string reason)
    {
        if (room.IsGameStarted)
        {
            reason = "Game already started.";
            return false;
        }

        if (room.Players.Count < 2)
        {
            reason = "Need at least 2 players to start.";
            return false;
        }

        room.IsGameStarted = true;
        room.CurrentTurnIndex = 0;
        room.BoardState = "[]";
        room.MovesInRound = 0;
        room.ConsecutivePasses = 0;
        reason = string.Empty;
        return true;
    }

    public bool TryHandleAction(Room room, Player player, string action, string payload, out string reason, out bool roundEnded, out bool gameEnded)
    {
        reason = string.Empty;
        roundEnded = false;
        gameEnded = false;

        if (!room.IsGameStarted)
        {
            reason = "Game not started.";
            return false;
        }

        if (player.IsWatcher)
        {
            reason = "Watchers cannot play.";
            return false;
        }

        var current = room.GetCurrentPlayer();
        if (current is null || !current.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Not your turn.";
            return false;
        }

        if (!_moveValidator.IsMoveValid(room, player, action, payload, out reason))
        {
            return false;
        }

        ApplyAction(room, player, action, payload);

        roundEnded = IsRoundEnded(room);
        if (roundEnded)
        {
            ApplyRoundScore(room);
            room.MovesInRound = 0;
            room.ConsecutivePasses = 0;
            room.BoardState = "[]";
        }

        gameEnded = IsGameEnded(room);
        return true;
    }

    public string BuildGameState(Room room)
    {
        var current = room.GetCurrentPlayer();
        var turnText = current is null ? "None" : current.Name;
        var players = string.Join(',', room.Players.Select(p => p.Name));
        var watchers = string.Join(',', room.Watchers.Select(w => w.Name));
        var scores = string.Join(',', room.Scores.Select(s => $"{s.Key}:{s.Value}"));

        return $"GAME_STATE|{room.RoomName}|Turn:{turnText}|Board:{room.BoardState}|Players:{players}|Watchers:{watchers}|Scores:{scores}";
    }

    private static void ApplyAction(Room room, Player player, string action, string payload)
    {
        room.MovesInRound++;

        switch (action)
        {
            case "PLAY_CARD":
                room.BoardState = AppendBoard(room.BoardState, $"{player.Name}:{payload}");
                room.ConsecutivePasses = 0;
                break;

            case "DRAW":
                room.BoardState = AppendBoard(room.BoardState, $"{player.Name}:DRAW");
                room.ConsecutivePasses = 0;
                break;

            case "PASS":
                room.BoardState = AppendBoard(room.BoardState, $"{player.Name}:PASS");
                room.ConsecutivePasses++;
                break;
        }

        room.NextTurn();
    }

    private static bool IsRoundEnded(Room room)
    {
        if (room.Players.Count == 0)
        {
            return false;
        }

        if (room.ConsecutivePasses >= room.Players.Count)
        {
            return true;
        }

        return room.MovesInRound >= room.Players.Count * 8;
    }

    private static void ApplyRoundScore(Room room)
    {
        if (room.Players.Count == 0)
        {
            return;
        }

        var bonus = 10;
        foreach (var player in room.Players)
        {
            room.Scores[player.Name] = room.Scores.GetValueOrDefault(player.Name) + bonus;
        }
    }

    private static bool IsGameEnded(Room room)
    {
        return room.Scores.Values.Any(score => score >= 100);
    }

    public static string BuildEndGameMessage(Room room)
    {
        var winner = room.Scores.OrderByDescending(x => x.Value).FirstOrDefault();
        var winnerName = string.IsNullOrWhiteSpace(winner.Key) ? "NoWinner" : winner.Key;
        return $"END_GAME|{room.RoomName}|Winner:{winnerName}";
    }

    public static string BuildResultText(Room room)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Room: {room.RoomName}");
        foreach (var score in room.Scores.OrderBy(s => s.Key))
        {
            builder.AppendLine($"{score.Key}: {score.Value}");
        }

        builder.AppendLine("---");
        return builder.ToString();
    }

    private static string AppendBoard(string boardState, string item)
    {
        if (boardState == "[]")
        {
            return $"[{item}]";
        }

        return boardState.TrimEnd(']') + $",{item}]";
    }
}

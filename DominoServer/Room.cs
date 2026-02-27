namespace DominoServer;

public class Room
{
    private readonly object _lock = new();

    public Room(string roomName, int maxPlayers = 4)
    {
        RoomName = roomName;
        MaxPlayers = maxPlayers;
    }

    public string RoomName { get; }
    public int MaxPlayers { get; }
    public List<Player> Players { get; } = [];
    public List<Player> Watchers { get; } = [];
    public bool IsGameStarted { get; set; }
    public int CurrentTurnIndex { get; set; }
    public int MovesInRound { get; set; }
    public int ConsecutivePasses { get; set; }
    public Dictionary<string, int> Scores { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string BoardState { get; set; } = "[]";

    public bool AddPlayer(Player player)
    {
        lock (_lock)
        {
            if (IsGameStarted || Players.Count >= MaxPlayers || Players.Any(p => p.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            Players.Add(player);
            if (!Scores.ContainsKey(player.Name))
            {
                Scores[player.Name] = 0;
            }

            player.CurrentRoom = this;
            player.IsWatcher = false;
            return true;
        }
    }

    public bool AddWatcher(Player player)
    {
        lock (_lock)
        {
            if (Watchers.Any(w => w.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            Watchers.Add(player);
            player.CurrentRoom = this;
            player.IsWatcher = true;
            return true;
        }
    }

    public void RemoveParticipant(Player player)
    {
        lock (_lock)
        {
            Players.Remove(player);
            Watchers.Remove(player);
            player.CurrentRoom = null;
            player.IsWatcher = false;

            if (CurrentTurnIndex >= Players.Count)
            {
                CurrentTurnIndex = 0;
            }
        }
    }

    public Player? GetCurrentPlayer()
    {
        lock (_lock)
        {
            if (Players.Count == 0 || CurrentTurnIndex < 0 || CurrentTurnIndex >= Players.Count)
            {
                return null;
            }

            return Players[CurrentTurnIndex];
        }
    }

    public void NextTurn()
    {
        lock (_lock)
        {
            if (Players.Count == 0)
            {
                CurrentTurnIndex = 0;
                return;
            }

            CurrentTurnIndex = (CurrentTurnIndex + 1) % Players.Count;
        }
    }

    public string ToRoomListItem()
    {
        lock (_lock)
        {
            var status = IsGameStarted ? "Started" : "Waiting";
            return $"{RoomName}({Players.Count}/{MaxPlayers})[{Watchers.Count}W]-{status}";
        }
    }
}

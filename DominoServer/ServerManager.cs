using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DominoServer;

public class ServerManager
{
    private readonly object _lock = new();
    private readonly List<Player> _connectedPlayers = [];
    private readonly List<Room> _rooms = [];
    private readonly Dictionary<TcpClient, ClientHandler> _handlers = [];
    private readonly GameManager _gameManager;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    public ServerManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public event Action<string>? Log;

    public IReadOnlyList<Player> ConnectedPlayers => _connectedPlayers;
    public IReadOnlyList<Room> Rooms => _rooms;

    public async Task StartAsync(int port)
    {
        if (_listener is not null)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        WriteLog($"Server started on port {port}");

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);
                var handler = new ClientHandler(tcpClient, this);
                lock (_lock)
                {
                    _handlers[tcpClient] = handler;
                }

                _ = handler.RunAsync(_cts.Token);
                WriteLog("Client connected.");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                WriteLog($"Accept error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _cts?.Cancel();

        lock (_lock)
        {
            foreach (var handler in _handlers.Values.ToList())
            {
                handler.Close();
            }

            _handlers.Clear();
            _connectedPlayers.Clear();
            _rooms.Clear();
        }

        _listener?.Stop();
        _listener = null;
        WriteLog("Server stopped.");
    }

    public bool TryLogin(TcpClient client, string playerName, out Player? player, out string reason)
    {
        player = null;
        reason = string.Empty;

        lock (_lock)
        {
            if (_connectedPlayers.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
            {
                reason = "Name already connected.";
                return false;
            }

            player = new Player { Name = playerName, Client = client };
            _connectedPlayers.Add(player);
        }

        WriteLog($"Player logged in: {playerName}");
        BroadcastPlayerList();
        BroadcastRoomList();
        return true;
    }

    public void HandleDisconnect(Player? player)
    {
        if (player is null)
        {
            return;
        }

        lock (_lock)
        {
            player.IsConnected = false;
            _connectedPlayers.Remove(player);

            if (player.CurrentRoom is not null)
            {
                var room = player.CurrentRoom;
                room.RemoveParticipant(player);
                if (room.Players.Count == 0 && room.Watchers.Count == 0)
                {
                    _rooms.Remove(room);
                }
                else
                {
                    BroadcastToRoom(room, _gameManager.BuildGameState(room));
                }
            }
        }

        WriteLog($"Player disconnected: {player.Name}");
        BroadcastPlayerList();
        BroadcastRoomList();
    }

    public bool TryCreateRoom(Player player, string roomName, out string reason)
    {
        reason = string.Empty;
        Room room;

        lock (_lock)
        {
            if (_rooms.Any(r => r.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase)))
            {
                reason = "Room already exists.";
                return false;
            }

            room = new Room(roomName);
            _rooms.Add(room);
            room.AddPlayer(player);
        }

        WriteLog($"Room created: {roomName} by {player.Name}");
        BroadcastRoomList();
        BroadcastToRoom(room, _gameManager.BuildGameState(room));
        return true;
    }

    public bool TryJoinRoom(Player player, string roomName, bool asWatcher, out string reason)
    {
        reason = string.Empty;
        Room? room;

        lock (_lock)
        {
            room = _rooms.FirstOrDefault(r => r.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase));
            if (room is null)
            {
                reason = "Room not found.";
                return false;
            }

            player.CurrentRoom?.RemoveParticipant(player);

            var joined = asWatcher ? room.AddWatcher(player) : room.AddPlayer(player);
            if (!joined)
            {
                reason = asWatcher ? "Cannot watch room." : "Cannot join room.";
                return false;
            }
        }

        WriteLog($"{player.Name} {(asWatcher ? "watching" : "joined")} room {roomName}");
        BroadcastRoomList();
        BroadcastToRoom(room, _gameManager.BuildGameState(room));
        return true;
    }

    public bool TryStartGame(Player player, out string reason)
    {
        reason = string.Empty;
        var room = player.CurrentRoom;
        if (room is null)
        {
            reason = "Join room first.";
            return false;
        }

        if (!_gameManager.StartGame(room, out reason))
        {
            return false;
        }

        WriteLog($"Game started in room {room.RoomName}");
        BroadcastToRoom(room, _gameManager.BuildGameState(room));
        BroadcastRoomList();
        return true;
    }

    public bool TryHandleAction(Player player, string action, string payload, out string reason)
    {
        reason = string.Empty;
        var room = player.CurrentRoom;
        if (room is null)
        {
            reason = "Join room first.";
            return false;
        }

        if (!_gameManager.TryHandleAction(room, player, action, payload, out reason, out var roundEnded, out var gameEnded))
        {
            return false;
        }

        BroadcastToRoom(room, _gameManager.BuildGameState(room));

        if (roundEnded)
        {
            BroadcastToRoom(room, $"GAME_STATE|{room.RoomName}|Round ended");
        }

        if (gameEnded)
        {
            var endMessage = GameManager.BuildEndGameMessage(room);
            BroadcastToRoom(room, endMessage);
            SaveResults(room);
            room.IsGameStarted = false;
            BroadcastRoomList();
        }

        return true;
    }

    public void UnregisterClient(TcpClient client)
    {
        lock (_lock)
        {
            _handlers.Remove(client);
        }
    }

    public void SendToPlayer(Player player, string message)
    {
        try
        {
            if (!player.Client.Connected)
            {
                return;
            }

            var stream = player.Client.GetStream();
            var bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            stream.Write(bytes, 0, bytes.Length);
        }
        catch
        {
            HandleDisconnect(player);
        }
    }

    public void BroadcastPlayerList()
    {
        string message;
        lock (_lock)
        {
            var names = string.Join(',', _connectedPlayers.Select(p => p.Name));
            message = $"PLAYER_LIST|{names}";
        }

        BroadcastToAll(message);
    }

    public void BroadcastRoomList()
    {
        string message;
        lock (_lock)
        {
            var items = _rooms.Select(r => r.ToRoomListItem());
            message = $"ROOM_LIST|{string.Join(',', items)}";
        }

        BroadcastToAll(message);
    }

    public void BroadcastToRoom(Room room, string message)
    {
        List<Player> targets;
        lock (_lock)
        {
            targets = room.Players.Concat(room.Watchers).Distinct().ToList();
        }

        foreach (var player in targets)
        {
            SendToPlayer(player, message);
        }
    }

    private void BroadcastToAll(string message)
    {
        List<Player> snapshot;
        lock (_lock)
        {
            snapshot = _connectedPlayers.ToList();
        }

        foreach (var player in snapshot)
        {
            SendToPlayer(player, message);
        }
    }

    private void SaveResults(Room room)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "ServerResults.txt");
        var text = GameManager.BuildResultText(room);
        File.AppendAllText(filePath, text);
        WriteLog($"Results saved for room {room.RoomName}");
    }

    private void WriteLog(string message)
    {
        Log?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}

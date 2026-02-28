using DominoShared.DTOs;
using NetClientHandler = DominoServer.Networking.ClientHandler;
using NetServerManager = DominoServer.Networking.ServerManager;
using SharedPlayer = DominoShared.Models.Player;

namespace DominoServer.Managers;

/// <summary>
/// Manages all connected players in the system.
/// Tracks login/logout, maintains player list, broadcasts updates.
/// </summary>
public class PlayerManager
{
    private readonly Dictionary<string, SharedPlayer> _players = new();
    private readonly Dictionary<string, NetClientHandler> _playerToClient = new();
    private readonly NetServerManager _serverManager;

    // Events for player changes
    public event Action<SharedPlayer>? OnPlayerJoined;
    public event Action<string>? OnPlayerLeft;

    public PlayerManager(NetServerManager serverManager)
    {
        _serverManager = serverManager;

        // Hook into server events
        _serverManager.OnMessageReceived += HandleMessageAsync;
        _serverManager.OnClientDisconnected += HandleClientDisconnected;
    }

    /// <summary>
    /// Add a player to the system when they login.
    /// </summary>
    public void LoginPlayer(string username, NetClientHandler clientHandler)
    {
        lock (_players)
        {
            if (_players.ContainsKey(username))
            {
                Console.WriteLine($"[PlayerManager] {username} already logged in");
                return;
            }

            var player = new SharedPlayer { Username = username };
            _players[username] = player;
            _playerToClient[username] = clientHandler;

            Console.WriteLine($"[PlayerManager] {username} logged in. Total players: {_players.Count}");
            OnPlayerJoined?.Invoke(player);
        }
    }

    /// <summary>
    /// Remove a player when they logout/disconnect.
    /// </summary>
    public void LogoutPlayer(string username)
    {
        lock (_players)
        {
            if (_players.Remove(username))
            {
                _playerToClient.Remove(username);
                Console.WriteLine($"[PlayerManager] {username} logged out. Total players: {_players.Count}");
                OnPlayerLeft?.Invoke(username);
            }
        }
    }

    /// <summary>
    /// Get all connected players.
    /// </summary>
    public List<SharedPlayer> GetAllPlayers()
    {
        lock (_players)
        {
            return new List<SharedPlayer>(_players.Values);
        }
    }

    /// <summary>
    /// Check if player exists.
    /// </summary>
    public bool PlayerExists(string username)
    {
        lock (_players)
        {
            return _players.ContainsKey(username);
        }
    }

    /// <summary>
    /// Broadcast current player list to all clients.
    /// </summary>
    public async Task BroadcastPlayerListAsync()
    {
        var playerList = GetAllPlayers();
        var message = new NetworkMessage
        {
            Action = "PLAYER_LIST",
            Data = System.Text.Json.JsonSerializer.Serialize(playerList),
            Timestamp = DateTime.UtcNow
        };

        await _serverManager.BroadcastAsync(message);
    }

    /// <summary>
    /// Route LOGIN messages to this manager.
    /// </summary>
    private void HandleMessageAsync(NetClientHandler handler, NetworkMessage message)
    {
        if (message.Action == "LOGIN")
        {
            LoginPlayer(message.Username, handler);
            _ = BroadcastPlayerListAsync();
        }
    }

    /// <summary>
    /// Handle client disconnect by logging out the player.
    /// </summary>
    private void HandleClientDisconnected(NetClientHandler handler)
    {
        if (handler.Username != null)
        {
            LogoutPlayer(handler.Username);
            _ = BroadcastPlayerListAsync();
        }
    }
}

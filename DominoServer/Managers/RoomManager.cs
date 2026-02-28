using System.Text.Json;
using DominoShared.DTOs;
using SharedGameStatus = DominoShared.Models.GameStatus;
using SharedPlayer = DominoShared.Models.Player;
using SharedRoom = DominoShared.Models.Room;
using NetClientHandler = DominoServer.Networking.ClientHandler;
using NetServerManager = DominoServer.Networking.ServerManager;

namespace DominoServer.Managers;

/// <summary>
/// Manages all game rooms in the system.
/// Handles room creation, player joining, watching, and room state synchronization.
/// Phase C responsibility - coordinates with Phase D (GameOrchestrator)
/// </summary>
public class RoomManager
{
    private readonly Dictionary<string, SharedRoom> _rooms = new();
    private readonly Dictionary<string, string> _playerToRoom = new(); // Maps username to room name
    private readonly NetServerManager _serverManager;

    // Events for room changes
    public event Action<SharedRoom>? OnRoomCreated;
    public event Action<string>? OnRoomDeleted;
    public event Action<SharedRoom, string>? OnPlayerJoinedRoom; // Room, Username
    public event Action<SharedRoom, string>? OnPlayerLeftRoom;   // Room, Username
    public event Action<SharedRoom>? OnRoomStatusChanged;

    public RoomManager(NetServerManager serverManager)
    {
        _serverManager = serverManager;
        _serverManager.OnMessageReceived += HandleMessageAsync;
        _serverManager.OnClientDisconnected += HandleClientDisconnected;
    }

    /// <summary>
    /// Create a new room.
    /// Only room owner can start the game.
    /// </summary>
    public SharedRoom? CreateRoom(string roomName, string ownerUsername, int maxPlayers, int winningScore)
    {
        lock (_rooms)
        {
            // Check if room name already exists
            if (_rooms.ContainsKey(roomName))
            {
                Console.WriteLine($"[RoomManager] Room '{roomName}' already exists");
                return null;
            }

            // Check if player is already in a room
            if (_playerToRoom.ContainsKey(ownerUsername))
            {
                Console.WriteLine($"[RoomManager] Player {ownerUsername} is already in a room");
                return null;
            }

            var room = new SharedRoom
            {
                Name = roomName,
                Owner = ownerUsername,
                MaxPlayers = maxPlayers,
                WinningScore = winningScore,
                Status = SharedGameStatus.Waiting
            };

            // Add owner as first player
            room.Players.Add(new SharedPlayer { Username = ownerUsername });
            _rooms[roomName] = room;
            _playerToRoom[ownerUsername] = roomName;

            Console.WriteLine($"[RoomManager] Room '{roomName}' created by {ownerUsername}. Max: {maxPlayers}, Winning Score: {winningScore}");
            OnRoomCreated?.Invoke(room);

            return room;
        }
    }

    /// <summary>
    /// Player joins an existing room.
    /// </summary>
    public bool JoinRoom(string roomName, string username)
    {
        lock (_rooms)
        {
            // Check if room exists
            if (!_rooms.TryGetValue(roomName, out var room))
            {
                Console.WriteLine($"[RoomManager] Room '{roomName}' not found");
                return false;
            }

            // Check if player is already in a room
            if (_playerToRoom.ContainsKey(username))
            {
                Console.WriteLine($"[RoomManager] Player {username} is already in room '{_playerToRoom[username]}'");
                return false;
            }

            // Check if room can accept players
            if (!room.CanJoin())
            {
                Console.WriteLine($"[RoomManager] Room '{roomName}' is full or game is in progress");
                return false;
            }

            // Add player to room
            room.Players.Add(new SharedPlayer { Username = username });
            _playerToRoom[username] = roomName;

            Console.WriteLine($"[RoomManager] Player {username} joined room '{roomName}'. Players: {room.Players.Count}/{room.MaxPlayers}");
            OnPlayerJoinedRoom?.Invoke(room, username);

            return true;
        }
    }

    /// <summary>
    /// Player watches an active game in a room.
    /// </summary>
    public bool WatchRoom(string roomName, string username)
    {
        lock (_rooms)
        {
            // Check if room exists
            if (!_rooms.TryGetValue(roomName, out var room))
            {
                Console.WriteLine($"[RoomManager] Room '{roomName}' not found");
                return false;
            }

            // Check if game is actually playing
            if (room.Status != SharedGameStatus.Playing)
            {
                Console.WriteLine($"[RoomManager] Room '{roomName}' is not currently playing");
                return false;
            }

            // Check if player is already in a room
            if (_playerToRoom.ContainsKey(username))
            {
                Console.WriteLine($"[RoomManager] Player {username} is already in room '{_playerToRoom[username]}'");
                return false;
            }

            // Add as watcher
            room.Watchers.Add(new SharedPlayer { Username = username });
            _playerToRoom[username] = roomName;

            Console.WriteLine($"[RoomManager] Player {username} started watching room '{roomName}'");

            return true;
        }
    }

    /// <summary>
    /// Player leaves a room.
    /// </summary>
    public void LeaveRoom(string username)
    {
        lock (_rooms)
        {
            if (!_playerToRoom.TryGetValue(username, out var roomName))
            {
                return; // Player not in any room
            }

            if (!_rooms.TryGetValue(roomName, out var room))
            {
                _playerToRoom.Remove(username);
                return;
            }

            // Remove from players
            var playerToRemove = room.Players.FirstOrDefault(p => p.Username == username);
            if (playerToRemove != null)
            {
                room.Players.Remove(playerToRemove);
                Console.WriteLine($"[RoomManager] Player {username} left room '{roomName}'");
                OnPlayerLeftRoom?.Invoke(room, username);
            }

            // Remove from watchers
            var watcherToRemove = room.Watchers.FirstOrDefault(p => p.Username == username);
            if (watcherToRemove != null)
            {
                room.Watchers.Remove(watcherToRemove);
                Console.WriteLine($"[RoomManager] Watcher {username} left room '{roomName}'");
            }

            // Delete room if owner left and no one is playing
            if (room.Owner == username && room.Status != SharedGameStatus.Playing)
            {
                _rooms.Remove(roomName);
                Console.WriteLine($"[RoomManager] Room '{roomName}' deleted (owner left)");
                OnRoomDeleted?.Invoke(roomName);
            }

            _playerToRoom.Remove(username);
        }
    }

    /// <summary>
    /// Set room status (called by GameOrchestrator when game starts/ends)
    /// </summary>
    public void SetRoomStatus(string roomName, SharedGameStatus status)
    {
        lock (_rooms)
        {
            if (_rooms.TryGetValue(roomName, out var room))
            {
                room.Status = status;
                OnRoomStatusChanged?.Invoke(room);
                Console.WriteLine($"[RoomManager] Room '{roomName}' status changed to {status}");
            }
        }
    }

    /// <summary>
    /// Get all available rooms for client display.
    /// </summary>
    public List<SharedRoom> GetAllRooms()
    {
        lock (_rooms)
        {
            return new List<SharedRoom>(_rooms.Values);
        }
    }

    /// <summary>
    /// Get a specific room by name.
    /// </summary>
    public SharedRoom? GetRoom(string roomName)
    {
        lock (_rooms)
        {
            _rooms.TryGetValue(roomName, out var room);
            return room;
        }
    }

    /// <summary>
    /// Get the room a player is in (playing or watching).
    /// </summary>
    public string? GetPlayerRoom(string username)
    {
        lock (_rooms)
        {
            _playerToRoom.TryGetValue(username, out var roomName);
            return roomName;
        }
    }

    /// <summary>
    /// Broadcast current room list to all clients.
    /// </summary>
    public async Task BroadcastRoomListAsync()
    {
        var rooms = GetAllRooms();
        var message = new NetworkMessage
        {
            Action = "ROOM_LIST",
            Data = JsonSerializer.Serialize(rooms),
            Timestamp = DateTime.UtcNow
        };

        await _serverManager.BroadcastAsync(message);
    }

    /// <summary>
    /// Route room-related messages to this manager.
    /// </summary>
    private void HandleMessageAsync(NetClientHandler handler, NetworkMessage message)
    {
        switch (message.Action)
        {
            case "CREATE_ROOM":
                HandleCreateRoom(handler, message);
                break;
            case "JOIN_ROOM":
                HandleJoinRoom(handler, message);
                break;
            case "WATCH_ROOM":
                HandleWatchRoom(handler, message);
                break;
            case "LEAVE_ROOM":
                HandleLeaveRoom(handler, message);
                break;
        }
    }

    private void HandleCreateRoom(NetClientHandler handler, NetworkMessage message)
    {
        try
        {
            var data = JsonSerializer.Deserialize<CreateRoomRequest>(message.Data ?? "{}");
            if (data == null || string.IsNullOrEmpty(data.RoomName))
            {
                SendErrorToClient(handler, "Invalid room creation request");
                return;
            }

            var room = CreateRoom(data.RoomName, message.Username, data.MaxPlayers, data.WinningScore);
            if (room != null)
            {
                SendSuccessToClient(handler, "Room created successfully");
                _ = BroadcastRoomListAsync();
            }
            else
            {
                SendErrorToClient(handler, "Failed to create room");
            }
        }
        catch (Exception ex)
        {
            SendErrorToClient(handler, $"Error creating room: {ex.Message}");
        }
    }

    private void HandleJoinRoom(NetClientHandler handler, NetworkMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.RoomName))
            {
                SendErrorToClient(handler, "Room name not specified");
                return;
            }

            if (JoinRoom(message.RoomName, message.Username))
            {
                SendSuccessToClient(handler, "Joined room successfully");
                _ = BroadcastRoomListAsync();
            }
            else
            {
                SendErrorToClient(handler, "Failed to join room");
            }
        }
        catch (Exception ex)
        {
            SendErrorToClient(handler, $"Error joining room: {ex.Message}");
        }
    }

    private void HandleWatchRoom(NetClientHandler handler, NetworkMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.RoomName))
            {
                SendErrorToClient(handler, "Room name not specified");
                return;
            }

            if (WatchRoom(message.RoomName, message.Username))
            {
                SendSuccessToClient(handler, "Started watching room");
            }
            else
            {
                SendErrorToClient(handler, "Failed to watch room");
            }
        }
        catch (Exception ex)
        {
            SendErrorToClient(handler, $"Error watching room: {ex.Message}");
        }
    }

    private void HandleLeaveRoom(NetClientHandler handler, NetworkMessage message)
    {
        LeaveRoom(message.Username);
        _ = BroadcastRoomListAsync();
    }

    /// <summary>
    /// Handle player disconnect by removing them from all rooms.
    /// </summary>
    private void HandleClientDisconnected(NetClientHandler handler)
    {
        if (handler.Username != null)
        {
            LeaveRoom(handler.Username);
            _ = BroadcastRoomListAsync();
        }
    }

    private void SendSuccessToClient(NetClientHandler handler, string message)
    {
        var response = new NetworkMessage
        {
            Success = true,
            ErrorMessage = message,
            Timestamp = DateTime.UtcNow
        };
        _ = handler.SendAsync(response);
    }

    private void SendErrorToClient(NetClientHandler handler, string errorMessage)
    {
        var response = new NetworkMessage
        {
            Success = false,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow
        };
        _ = handler.SendAsync(response);
    }

    /// <summary>
    /// DTO for CREATE_ROOM action data.
    /// </summary>
    public class CreateRoomRequest
    {
        public string RoomName { get; set; } = string.Empty;
        public int MaxPlayers { get; set; } = 2;
        public int WinningScore { get; set; } = 100;
    }
}

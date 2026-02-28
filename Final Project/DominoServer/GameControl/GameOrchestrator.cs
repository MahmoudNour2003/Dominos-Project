using System.Text.Json;
using DominoShared.DTOs;
using DominoShared.Engine;
using DominoShared.Models;
using DominoServer.Managers;
using DominoServer.Storage;
using SharedGameStatus = DominoShared.Models.GameStatus;
using SharedRoom = DominoShared.Models.Room;
using NetClientHandler = DominoServer.Networking.ClientHandler;
using NetServerManager = DominoServer.Networking.ServerManager;

namespace DominoServer.GameControl;

/// <summary>
/// Orchestrates the game flow for each room.
/// Manages turns, validates moves, updates game state, broadcasts updates.
/// This is the CORE of the server-side game logic coordination.
/// Phase D + E: Game orchestration and persistence
/// </summary>
public class GameOrchestrator
{
    private readonly NetServerManager _serverManager;
    private readonly RoomManager _roomManager;
    private readonly FileStorage _fileStorage;
    private readonly Dictionary<string, GameState> _gameStates = new();
    private readonly Dictionary<string, IDeck> _gameDecks = new();
    private readonly Dictionary<string, IRulesEngine> _gameRules = new();

    // Allow injection of Dev 2's implementations
    private readonly Func<IDeck> _deckFactory;
    private readonly Func<IRulesEngine> _rulesFactory;

    public event Action<string, GameState>? OnGameStateUpdated; // RoomName, GameState
    public event Action<string, string>? OnGameEnded;           // RoomName, Winner

    public GameOrchestrator(
        NetServerManager serverManager,
        RoomManager roomManager,
        Func<IDeck>? deckFactory = null,
        Func<IRulesEngine>? rulesFactory = null,
        FileStorage? fileStorage = null)
    {
        _serverManager = serverManager;
        _roomManager = roomManager;
        _fileStorage = fileStorage ?? new FileStorage();
        _deckFactory = deckFactory ?? (() => throw new NotImplementedException("Deck not implemented by Dev 2"));
        _rulesFactory = rulesFactory ?? (() => throw new NotImplementedException("RulesEngine not implemented by Dev 2"));

        // Hook into room and server events
        _roomManager.OnPlayerJoinedRoom += HandlePlayerJoinedRoom;
        _roomManager.OnPlayerLeftRoom += HandlePlayerLeftRoom;
        _serverManager.OnMessageReceived += HandleGameMessage;
    }

    /// <summary>
    /// Start a game when a room has enough players
    /// </summary>
    public async Task StartGameAsync(string roomName)
    {
        try
        {
            var room = _roomManager.GetRoom(roomName);
            if (room == null)
            {
                Console.WriteLine($"[GameOrchestrator] Room '{roomName}' not found");
                return;
            }

            if (room.Players.Count < 2)
            {
                Console.WriteLine($"[GameOrchestrator] Room '{roomName}' needs at least 2 players to start");
                return;
            }

            lock (_gameStates)
            {
                if (_gameStates.ContainsKey(roomName))
                {
                    Console.WriteLine($"[GameOrchestrator] Game already running in '{roomName}'");
                    return;
                }

                // Create deck and rules engine for this game
                var deck = _deckFactory();
                var rules = _rulesFactory();
                
                // Generate and shuffle the deck
                deck.GenerateDeck();
                deck.Shuffle();
                
                _gameDecks[roomName] = deck;
                _gameRules[roomName] = rules;

                // Initialize game state
                var gameState = new GameState
                {
                    RoomName = roomName,
                    IsGameActive = true,
                    CurrentPlayerIndex = 0,
                    SideDeckCount = 0
                };

                // Setup players in turn order
                foreach (var player in room.Players)
                {
                    gameState.Players.Add(new PlayerGameState { Username = player.Username });
                    gameState.CurrentScores[player.Username] = 0;
                    gameState.TotalScores[player.Username] = 0;
                    gameState.PlayerCardCounts[player.Username] = 0;
                    gameState.PlayerHands[player.Username] = new();
                }

                // Distribute cards (7 per player, or less if not enough cards)
                const int cardsPerPlayer = 7;
                foreach (var player in gameState.Players)
                {
                    var hand = deck.DrawCards(cardsPerPlayer);
                    gameState.PlayerHands[player.Username] = hand;
                    gameState.PlayerCardCounts[player.Username] = hand.Count;
                }

                // Remaining cards go to side deck
                gameState.SideDeckCount = deck.RemainingCards;

                // First player is active
                if (gameState.Players.Count > 0)
                {
                    gameState.Players[0].IsActive = true;
                }

                _gameStates[roomName] = gameState;
                Console.WriteLine($"[GameOrchestrator] Game started in '{roomName}' with {gameState.Players.Count} players");
                Console.WriteLine($"[GameOrchestrator] Each player has {cardsPerPlayer} cards, {gameState.SideDeckCount} cards remaining in deck");

                // Notify room status changed
                _roomManager.SetRoomStatus(roomName, GameStatus.Playing);
            }

            // Broadcast initial game state
            await BroadcastGameStateAsync(roomName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameOrchestrator] Error starting game in '{roomName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Handle PLAY_CARD message
    /// </summary>
    private async Task HandlePlayCardAsync(NetClientHandler handler, NetworkMessage message)
    {
        if (string.IsNullOrEmpty(message.RoomName))
        {
            await handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Room not specified" });
            return;
        }

        try
        {
            var cardData = JsonSerializer.Deserialize<PlayCardRequest>(message.Data ?? "{}");
            if (cardData == null)
            {
                await handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Invalid card data" });
                return;
            }

            lock (_gameStates)
            {
                if (!_gameStates.TryGetValue(message.RoomName, out var gameState))
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Game not found" });
                    return;
                }

                // Validate it's this player's turn
                var currentPlayer = gameState.GetCurrentPlayerUsername();
                if (currentPlayer != message.Username)
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Not your turn" });
                    return;
                }

                // Get player's hand
                if (!gameState.PlayerHands.TryGetValue(message.Username, out var hand))
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Player hand not found" });
                    return;
                }

                // Find the card in hand
                var card = hand.FirstOrDefault(c => c.LeftValue == cardData.LeftValue && c.RightValue == cardData.RightValue);
                if (card == null)
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Card not in hand" });
                    return;
                }

                // Validate the move with rules engine
                var rules = _gameRules[message.RoomName];
                if (!rules.IsValidMove(card, gameState.TableCards))
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Invalid move" });
                    return;
                }

                // Play the card
                hand.Remove(card);
                gameState.TableCards.Add(card);
                gameState.PlayerCardCounts[message.Username] = hand.Count;

                // Check if player finished
                if (hand.Count == 0)
                {
                    gameState.IsRoundFinished = true;
                    Console.WriteLine($"[GameOrchestrator] Player '{message.Username}' finished round in '{message.RoomName}'");
                }
                else
                {
                    // Move to next player
                    gameState.Players[gameState.CurrentPlayerIndex].IsActive = false;
                    gameState.CurrentPlayerIndex = (gameState.CurrentPlayerIndex + 1) % gameState.Players.Count;
                    gameState.Players[gameState.CurrentPlayerIndex].IsActive = true;
                }

                _ = handler.SendAsync(new NetworkMessage { Success = true, ErrorMessage = "Card played" });
            }

            await BroadcastGameStateAsync(message.RoomName);
            await CheckRoundEndAsync(message.RoomName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameOrchestrator] Error playing card: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle PASS message
    /// </summary>
    private async Task HandlePassAsync(NetClientHandler handler, NetworkMessage message)
    {
        if (string.IsNullOrEmpty(message.RoomName))
        {
            await handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Room not specified" });
            return;
        }

        try
        {
            lock (_gameStates)
            {
                if (!_gameStates.TryGetValue(message.RoomName, out var gameState))
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Game not found" });
                    return;
                }

                // Validate it's this player's turn
                var currentPlayer = gameState.GetCurrentPlayerUsername();
                if (currentPlayer != message.Username)
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Not your turn" });
                    return;
                }

                // Check if player can pass
                var rules = _gameRules[message.RoomName];
                if (!rules.CanPass(gameState.SideDeckCount))
                {
                    _ = handler.SendAsync(new NetworkMessage { Success = false, ErrorMessage = "Cannot pass - side deck still has cards" });
                    return;
                }

                // Mark player as passed
                gameState.Players[gameState.CurrentPlayerIndex].PassedThisRound = true;

                // Move to next player
                gameState.Players[gameState.CurrentPlayerIndex].IsActive = false;
                gameState.CurrentPlayerIndex = (gameState.CurrentPlayerIndex + 1) % gameState.Players.Count;
                gameState.Players[gameState.CurrentPlayerIndex].IsActive = true;

                _ = handler.SendAsync(new NetworkMessage { Success = true, ErrorMessage = "Passed" });
            }

            await BroadcastGameStateAsync(message.RoomName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameOrchestrator] Error handling pass: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if round has finished (someone played all their cards or all players passed)
    /// </summary>
    private async Task CheckRoundEndAsync(string roomName)
    {
        lock (_gameStates)
        {
            if (!_gameStates.TryGetValue(roomName, out var gameState))
                return;

            if (!gameState.IsRoundFinished)
                return;

            // Calculate round points for remaining cards
            var rules = _gameRules[roomName];
            foreach (var player in gameState.Players)
            {
                if (gameState.PlayerHands.TryGetValue(player.Username, out var hand))
                {
                    var points = rules.CalculateRoundPoints(hand);
                    gameState.CurrentScores[player.Username] = points;
                    gameState.TotalScores[player.Username] += points;
                }
            }

            // Check if anyone reached winning score
            var room = _roomManager.GetRoom(roomName);
            if (room != null)
            {
                var winner = gameState.TotalScores.FirstOrDefault(x => x.Value >= room.WinningScore);
                if (!string.IsNullOrEmpty(winner.Key))
                {
                    gameState.Winner = winner.Key;
                    gameState.IsGameActive = false;
                    Console.WriteLine($"[GameOrchestrator] Game finished in '{roomName}'. Winner: {winner.Key}");
                    
                    // Save game result to file (Phase E)
                    _ = _fileStorage.SaveGameResultAsync(gameState, room);
                    
                    OnGameEnded?.Invoke(roomName, winner.Key);
                }
            }
        }

        await BroadcastGameStateAsync(roomName);
    }

    /// <summary>
    /// Broadcast current game state to all players in the room
    /// </summary>
    private async Task BroadcastGameStateAsync(string roomName)
    {
        GameState? gameState;
        lock (_gameStates)
        {
            if (!_gameStates.TryGetValue(roomName, out gameState))
                return;
        }

        var message = new NetworkMessage
        {
            Action = "GAME_STATE",
            RoomName = roomName,
            Data = JsonSerializer.Serialize(gameState),
            Timestamp = DateTime.UtcNow
        };

        // Send to all players in the room
        var room = _roomManager.GetRoom(roomName);
        if (room != null)
        {
            var tasks = new List<Task>();
            foreach (var player in room.Players.Concat(room.Watchers))
            {
                tasks.Add(_serverManager.SendToPlayerAsync(player.Username, message));
            }
            await Task.WhenAll(tasks);
        }

        OnGameStateUpdated?.Invoke(roomName, gameState);
    }

    /// <summary>
    /// When player joins room, check if we should start game
    /// </summary>
    private void HandlePlayerJoinedRoom(SharedRoom room, string username)
    {
        // If room now has enough players, start the game
        if (room.Players.Count >= room.MaxPlayers && room.Status == SharedGameStatus.Waiting)
        {
            _ = StartGameAsync(room.Name);
        }
    }

    /// <summary>
    /// When player leaves room during game, handle disconnection
    /// </summary>
    private void HandlePlayerLeftRoom(SharedRoom room, string username)
    {
        if (room.Status == SharedGameStatus.Playing)
        {
            Console.WriteLine($"[GameOrchestrator] Player {username} left room '{room.Name}' during game");
            // TODO: Handle mid-game disconnect (forfeit? pause?)
        }
    }

    /// <summary>
    /// Route game messages to appropriate handlers
    /// </summary>
    private void HandleGameMessage(NetClientHandler handler, NetworkMessage message)
    {
        switch (message.Action)
        {
            case "PLAY_CARD":
                _ = HandlePlayCardAsync(handler, message);
                break;
            case "PASS":
                _ = HandlePassAsync(handler, message);
                break;
        }
    }

    /// <summary>
    /// DTO for PLAY_CARD action
    /// </summary>
    public class PlayCardRequest
    {
        public int LeftValue { get; set; }
        public int RightValue { get; set; }
    }
}

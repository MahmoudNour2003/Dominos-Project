using DominoServer.GameControl;
using DominoServer.Managers;
using DominoServer.Storage;
using NetServerManager = DominoServer.Networking.ServerManager;

namespace DominoServer;

internal static class Program
{
    private static NetServerManager? _serverManager;
    private static PlayerManager? _playerManager;
    private static RoomManager? _roomManager;
    private static GameOrchestrator? _gameOrchestrator;
    private static FileStorage? _fileStorage;

    [STAThread]
    static void Main()
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║    DOMINO GAME SERVER - DEV 1        ║");
        Console.WriteLine("║    Phase A: TCP Infrastructure       ║");
        Console.WriteLine("║    Phase B: Player Management        ║");
        Console.WriteLine("║    Phase C: Room Management          ║");
        Console.WriteLine("║    Phase D: Game Orchestration       ║");
        Console.WriteLine("║    Phase E: Persistence              ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();

        // Initialize file storage first (Phase E)
        _fileStorage = new FileStorage();

        // Initialize managers in order
        _serverManager = new NetServerManager();
        _playerManager = new PlayerManager(_serverManager);
        _roomManager = new RoomManager(_serverManager);
        
        // GameOrchestrator with FileStorage integrated
        _gameOrchestrator = new GameOrchestrator(_serverManager, _roomManager, fileStorage: _fileStorage);

        // Wire up player events
        _playerManager.OnPlayerJoined += player =>
        {
            Console.WriteLine($"✓ Player joined: {player.Username}");
        };

        _playerManager.OnPlayerLeft += username =>
        {
            Console.WriteLine($"✗ Player left: {username}");
        };

        // Wire up room events
        _roomManager.OnRoomCreated += room =>
        {
            Console.WriteLine($"✓ Room created: {room.Name} (Owner: {room.Owner}, Max: {room.MaxPlayers})");
        };

        _roomManager.OnRoomDeleted += roomName =>
        {
            Console.WriteLine($"✗ Room deleted: {roomName}");
        };

        _roomManager.OnPlayerJoinedRoom += (room, username) =>
        {
            Console.WriteLine($"  → {username} joined room '{room.Name}' ({room.Players.Count}/{room.MaxPlayers})");
        };

        _roomManager.OnPlayerLeftRoom += (room, username) =>
        {
            Console.WriteLine($"  ← {username} left room '{room.Name}'");
        };

        // Wire up game events
        _gameOrchestrator.OnGameStateUpdated += (roomName, gameState) =>
        {
            Console.WriteLine($"  [Game] State updated in '{roomName}' - Turn: {gameState.GetCurrentPlayerUsername()}");
        };

        _gameOrchestrator.OnGameEnded += (roomName, winner) =>
        {
            Console.WriteLine($"  [Game] Finished in '{roomName}' - Winner: {winner}");
            Console.WriteLine($"  [File] Results saved to: {_fileStorage?.GetResultsDirectory()}");
        };

        // Start server on background thread
        var serverTask = Task.Run(() => _serverManager.StartAsync());

        Console.WriteLine("\n[Server] Ready for clients. Press Ctrl+C to stop...");
        Console.WriteLine($"[Storage] Results directory: {_fileStorage?.GetResultsDirectory()}\n");

        // Keep console alive
        Console.CancelKeyPress += (sender, args) =>
        {
            args.Cancel = true;
            _serverManager?.Stop();
            Console.WriteLine("\n[Server] Shutting down...");
            Environment.Exit(0);
        };

        serverTask.Wait();
    }

    /// <summary>
    /// Called by Dev 1 after Dev 2 delivers implementations
    /// to inject the concrete Deck and RulesEngine factories
    /// </summary>
    public static void SetGameImplementations(
        Func<DominoShared.Engine.IDeck> deckFactory,
        Func<DominoShared.Engine.IRulesEngine> rulesFactory)
    {
        if (_gameOrchestrator != null)
        {
            // Recreate GameOrchestrator with proper implementations
            _gameOrchestrator = new GameOrchestrator(_serverManager!, _roomManager!, deckFactory, rulesFactory, _fileStorage);
            Console.WriteLine("[Server] Game implementations registered");
        }
    }
}
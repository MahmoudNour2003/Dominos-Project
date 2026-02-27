using System.Net;
using System.Net.Sockets;
using DominoShared.DTOs;

namespace DominoServer.Networking;

/// <summary>
/// Main TCP server manager for the Domino game server.
/// Listens for client connections, maintains client registry, and broadcasts messages.
/// This is the central networking hub that Phase B, C, D will hook into.
/// </summary>
public class ServerManager
{
    private const int PORT = 5000;
    private TcpListener? _listener;
    private bool _isRunning = false;
    private readonly Dictionary<string, ClientHandler> _connectedClients = new();
    private int _clientCounter = 0;

    // Event for when a message is received from any client
    public event Action<ClientHandler, NetworkMessage>? OnMessageReceived;
    public event Action<ClientHandler>? OnClientConnected;
    public event Action<ClientHandler>? OnClientDisconnected;

    /// <summary>
    /// Start the TCP server and begin accepting connections.
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, PORT);
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"[Server] Listening on port {PORT}...");

            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientConnectionAsync(client);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle a newly connected client.
    /// </summary>
    private async Task HandleClientConnectionAsync(TcpClient client)
    {
        var clientId = $"Client_{++_clientCounter}";
        var handler = new ClientHandler(client, clientId);

        lock (_connectedClients)
        {
            _connectedClients[clientId] = handler;
        }

        Console.WriteLine($"[Server] {clientId} connected. Total: {_connectedClients.Count}");

        // Wire up events
        handler.OnMessageReceived += (h, msg) => OnMessageReceived?.Invoke(h, msg);
        handler.OnDisconnected += (h) =>
        {
            RemoveClient(clientId);
            OnClientDisconnected?.Invoke(h);
        };

        OnClientConnected?.Invoke(handler);

        // Start listening for messages from this client (blocks until disconnect)
        await handler.StartListeningAsync();
    }

    /// <summary>
    /// Remove a disconnected client from the registry.
    /// </summary>
    private void RemoveClient(string clientId)
    {
        lock (_connectedClients)
        {
            if (_connectedClients.Remove(clientId))
            {
                Console.WriteLine($"[Server] {clientId} disconnected. Total: {_connectedClients.Count}");
            }
        }
    }

    /// <summary>
    /// Broadcast a message to all connected clients.
    /// </summary>
    public async Task BroadcastAsync(NetworkMessage message)
    {
        List<ClientHandler> handlers;
        lock (_connectedClients)
        {
            handlers = new List<ClientHandler>(_connectedClients.Values);
        }

        var tasks = new List<Task>();
        foreach (var handler in handlers)
        {
            tasks.Add(handler.SendAsync(message));
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Send a message to a specific client.
    /// </summary>
    public async Task SendToClientAsync(string clientId, NetworkMessage message)
    {
        ClientHandler? handler = null;
        lock (_connectedClients)
        {
            _connectedClients.TryGetValue(clientId, out handler);
        }

        if (handler != null)
        {
            await handler.SendAsync(message);
        }
    }

    /// <summary>
    /// Send to a specific player by username.
    /// </summary>
    public async Task SendToPlayerAsync(string username, NetworkMessage message)
    {
        ClientHandler? handler = null;
        lock (_connectedClients)
        {
            handler = _connectedClients.Values.FirstOrDefault(h => h.Username == username);
        }

        if (handler != null)
        {
            await handler.SendAsync(message);
        }
    }

    /// <summary>
    /// Get all connected client handlers.
    /// </summary>
    public List<ClientHandler> GetAllClients()
    {
        lock (_connectedClients)
        {
            return new List<ClientHandler>(_connectedClients.Values);
        }
    }

    /// <summary>
    /// Stop the server.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
    }
}

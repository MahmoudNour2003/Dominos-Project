using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DominoShared.DTOs;

namespace DominoServer.Networking;

/// <summary>
/// Handles communication with a single connected client.
/// Each client connection gets its own ClientHandler instance running on a background thread.
/// </summary>
public class ClientHandler
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly string _clientId;
    public string? Username { get; set; }

    // Event raised when message is received from this client
    public event Action<ClientHandler, NetworkMessage>? OnMessageReceived;
    public event Action<ClientHandler>? OnDisconnected;

    public ClientHandler(TcpClient client, string clientId)
    {
        _client = client;
        _clientId = clientId;
        _stream = client.GetStream();
    }

    /// <summary>
    /// Start listening for messages from this client asynchronously.
    /// This should be called on a background task.
    /// </summary>
    public async Task StartListeningAsync()
    {
        try
        {
            using var reader = new StreamReader(_stream, Encoding.UTF8);
            while (_client.Connected)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null) break; // Client disconnected

                try
                {
                    var message = JsonSerializer.Deserialize<NetworkMessage>(line);
                    if (message != null)
                    {
                        Username = message.Username;
                        OnMessageReceived?.Invoke(this, message);
                    }
                }
                catch (JsonException)
                {
                    Console.WriteLine($"[{_clientId}] Invalid JSON received");
                }
            }
        }
        catch (IOException)
        {
            // Client disconnected
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_clientId}] Error: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    /// <summary>
    /// Send a message to this client asynchronously.
    /// </summary>
    public async Task SendAsync(NetworkMessage message)
    {
        try
        {
            if (!_client.Connected) return;

            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json + "\n");
            await _stream.WriteAsync(buffer, 0, buffer.Length);
            await _stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_clientId}] Send error: {ex.Message}");
        }
    }

    /// <summary>
    /// Close the connection and cleanup.
    /// </summary>
    public void Disconnect()
    {
        _client?.Close();
        _stream?.Dispose();
        OnDisconnected?.Invoke(this);
    }

    public bool IsConnected => _client?.Connected ?? false;
}

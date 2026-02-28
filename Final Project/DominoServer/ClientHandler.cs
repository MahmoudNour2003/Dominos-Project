using System.Net.Sockets;

namespace DominoServer;

public class ClientHandler(TcpClient client, ServerManager server)
{
    private readonly TcpClient _client = client;
    private readonly ServerManager _server = server;
    private Player? _player;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var stream = _client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested && _client.Connected)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var response = HandleCommand(line);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    await writer.WriteLineAsync(response);
                }
            }
        }
        catch
        {
        }
        finally
        {
            _server.HandleDisconnect(_player);
            _server.UnregisterClient(_client);
            _client.Close();
        }
    }

    public void Close()
    {
        try
        {
            _client.Close();
        }
        catch
        {
        }
    }

    private string HandleCommand(string message)
    {
        var parts = message.Split('|');
        var command = parts[0].Trim().TrimStart('\uFEFF').ToUpperInvariant();
        var payload = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        return command switch
        {
            "LOGIN" => HandleLogin(payload),
            "CREATE_ROOM" => HandleCreateRoom(payload),
            "JOIN_ROOM" => HandleJoinRoom(payload),
            "WATCH_ROOM" => HandleWatchRoom(payload),
            "START_GAME" => HandleStartGame(),
            "PLAY_CARD" => HandleAction("PLAY_CARD", payload),
            "PASS" => HandleAction("PASS", string.Empty),
            "DRAW" => HandleAction("DRAW", string.Empty),
            _ => "ERROR|Unknown command"
        };
    }

    private string HandleLogin(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return "ERROR|Name required";
        }

        if (_player is not null)
        {
            return "ERROR|Already logged in";
        }

        if (!_server.TryLogin(_client, playerName, out var player, out var reason) || player is null)
        {
            return $"ERROR|{reason}";
        }

        _player = player;
        return $"LOGIN|OK|{playerName}";
    }

    private string HandleCreateRoom(string roomName)
    {
        if (_player is null)
        {
            return "ERROR|Login first";
        }

        if (!_server.TryCreateRoom(_player, roomName, out var reason))
        {
            return $"ERROR|{reason}";
        }

        return "CREATE_ROOM|OK";
    }

    private string HandleJoinRoom(string roomName)
    {
        if (_player is null)
        {
            return "ERROR|Login first";
        }

        if (!_server.TryJoinRoom(_player, roomName, asWatcher: false, out var reason))
        {
            return $"ERROR|{reason}";
        }

        return "JOIN_ROOM|OK";
    }

    private string HandleWatchRoom(string roomName)
    {
        if (_player is null)
        {
            return "ERROR|Login first";
        }

        if (!_server.TryJoinRoom(_player, roomName, asWatcher: true, out var reason))
        {
            return $"ERROR|{reason}";
        }

        return "WATCH_ROOM|OK";
    }

    private string HandleStartGame()
    {
        if (_player is null)
        {
            return "ERROR|Login first";
        }

        if (!_server.TryStartGame(_player, out var reason))
        {
            return $"ERROR|{reason}";
        }

        return "START_GAME|OK";
    }

    private string HandleAction(string action, string payload)
    {
        if (_player is null)
        {
            return "ERROR|Login first";
        }

        if (!_server.TryHandleAction(_player, action, payload, out var reason))
        {
            return $"ERROR|{reason}";
        }

        return $"{action}|OK";
    }
}

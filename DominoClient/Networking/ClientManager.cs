using System.Net.Sockets;
using System.Text;

namespace DominoClient.Networking;

public sealed class ClientManager : IAsyncDisposable
{
    private static readonly UTF8Encoding Utf8NoBom = new(false);

    private TcpClient? _tcpClient;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _receiveCts;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public event Action<string>? OnMessageReceived;
    public event Action? OnDisconnected;

    public bool IsConnected => _tcpClient?.Connected == true;

    public async Task ConnectAsync(string host, int port)
    {
        if (IsConnected)
        {
            return;
        }

        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port);

        var stream = _tcpClient.GetStream();
        _reader = new StreamReader(stream, Utf8NoBom);
        _writer = new StreamWriter(stream, Utf8NoBom) { AutoFlush = true };

        _receiveCts = new CancellationTokenSource();
        _ = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token));
    }

    public async Task SendAsync(string message)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        await _sendLock.WaitAsync();
        try
        {
            await _writer.WriteLineAsync(message);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _reader is not null)
            {
                var line = await _reader.ReadLineAsync(cancellationToken);
                if (line is null)
                {
                    break;
                }

                OnMessageReceived?.Invoke(line.TrimStart('\uFEFF'));
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
        finally
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        _receiveCts?.Cancel();
        _reader?.Dispose();
        _writer?.Dispose();
        _tcpClient?.Close();

        _reader = null;
        _writer = null;
        _tcpClient = null;

        OnDisconnected?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        Disconnect();
        _sendLock.Dispose();
        await Task.CompletedTask;
    }
}

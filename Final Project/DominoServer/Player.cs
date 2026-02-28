using System.Net.Sockets;

namespace DominoServer;

public class Player
{
    public required string Name { get; init; }
    public required TcpClient Client { get; init; }
    public Room? CurrentRoom { get; set; }
    public bool IsWatcher { get; set; }
    public bool IsConnected { get; set; } = true;
}

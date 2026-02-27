namespace DominoShared.DTOs;

/// <summary>
/// Standard message format for all TCP communication between server and clients.
/// Ensures consistent JSON serialization across the entire system.
/// </summary>
public class NetworkMessage
{
    /// <summary>
    /// Action type: LOGIN, LOGOUT, CREATE_ROOM, JOIN_ROOM, PLAY_CARD, PASS, etc.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Username of the sender
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Room name context (null if not room-specific)
    /// </summary>
    public string? RoomName { get; set; }

    /// <summary>
    /// JSON-serialized payload (game state, card data, error messages, etc.)
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Server timestamp for message ordering
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Success/failure indicator
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if Success is false
    /// </summary>
    public string? ErrorMessage { get; set; }
}

using DominoShared.Engine;
using SharedRoom = DominoShared.Models.Room;

namespace DominoServer.Storage;

/// <summary>
/// Handles persisting game results to disk.
/// Saves game results in the required format when games end.
/// </summary>
public class FileStorage
{
    private readonly string _resultsDirectory;

    public FileStorage(string? resultsDirectory = null)
    {
        // Use custom path or default to Results folder in server directory
        _resultsDirectory = resultsDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Results");

        // Create Results directory if it doesn't exist
        try
        {
            if (!Directory.Exists(_resultsDirectory))
            {
                Directory.CreateDirectory(_resultsDirectory);
                Console.WriteLine($"[FileStorage] Created results directory: {_resultsDirectory}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileStorage] Error creating results directory: {ex.Message}");
        }
    }

    /// <summary>
    /// Save game result to file when game ends.
    /// Format:
    /// Room_Name = "RoomName"
    ///     Player Name = "PlayerName", Player Points = 100
    ///     Player Name = "PlayerName", Player Points = 95
    /// </summary>
    public async Task SaveGameResultAsync(string roomName, GameState gameState, SharedRoom room)
    {
        try
        {
            if (gameState == null || room == null)
            {
                Console.WriteLine("[FileStorage] Invalid game state or room for saving result");
                return;
            }

            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var filename = $"{roomName}_{timestamp}.txt";
            var filepath = Path.Combine(_resultsDirectory, filename);

            // Build file content
            var lines = new List<string>
            {
                $"Room_Name = \"{roomName}\""
            };

            // Add each player's final score
            foreach (var player in gameState.Players)
            {
                if (gameState.TotalScores.TryGetValue(player.Username, out var points))
                {
                    lines.Add($"    Player Name = \"{player.Username}\", Player Points = {points}");
                }
            }

            // Add game metadata
            lines.Add("");
            lines.Add($"Game Date = {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            lines.Add($"Winner = \"{gameState.Winner}\"");
            lines.Add($"Max Players = {room.MaxPlayers}");
            lines.Add($"Winning Score Limit = {room.WinningScore}");

            // Write to file asynchronously
            await File.WriteAllLinesAsync(filepath, lines);
            Console.WriteLine($"[FileStorage] Game result saved: {filepath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileStorage] Error saving game result: {ex.Message}");
        }
    }

    /// <summary>
    /// Save game result (overload for easier calling)
    /// </summary>
    public async Task SaveGameResultAsync(GameState gameState, SharedRoom room)
    {
        await SaveGameResultAsync(gameState.RoomName, gameState, room);
    }

    /// <summary>
    /// Get all saved game results
    /// </summary>
    public List<string> GetAllResults()
    {
        try
        {
            if (!Directory.Exists(_resultsDirectory))
                return new List<string>();

            return Directory.GetFiles(_resultsDirectory, "*.txt")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileStorage] Error reading results: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Read a specific game result file
    /// </summary>
    public async Task<string?> ReadGameResultAsync(string filename)
    {
        try
        {
            var filepath = Path.Combine(_resultsDirectory, filename);

            if (!File.Exists(filepath))
            {
                Console.WriteLine($"[FileStorage] Result file not found: {filepath}");
                return null;
            }

            return await File.ReadAllTextAsync(filepath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileStorage] Error reading result: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Delete a game result file
    /// </summary>
    public void DeleteGameResult(string filename)
    {
        try
        {
            var filepath = Path.Combine(_resultsDirectory, filename);

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
                Console.WriteLine($"[FileStorage] Result deleted: {filepath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileStorage] Error deleting result: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the results directory path
    /// </summary>
    public string GetResultsDirectory() => _resultsDirectory;
}

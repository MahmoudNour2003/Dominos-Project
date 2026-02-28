using System.Text.Json;
using DominoClient.Controls;
using DominoClient.Networking;
using DominoShared.DTOs;

namespace DominoClient;

public class GameForm : Form
{
    private readonly ClientManager _clientManager;
    private readonly string _username;
    private readonly string _roomName;

    private readonly Panel _topPanel;
    private readonly Panel _boardPanel;
    private readonly FlowLayoutPanel _handPanel;
    private readonly Panel _bottomPanel;

    private readonly Label _lblCurrentTurn;
    private readonly Label _lblScore;
    private readonly Button _btnLeaveGame;

    public GameForm(ClientManager clientManager, string username, DominoShared.Engine.GameState initialGameState)
    {
        _clientManager = clientManager;
        _username = username;
        _roomName = initialGameState.RoomName;

        _topPanel = new Panel();
        _boardPanel = new Panel();
        _handPanel = new FlowLayoutPanel();
        _bottomPanel = new Panel();

        _lblCurrentTurn = new Label();
        _lblScore = new Label();
        _btnLeaveGame = new Button();

        InitializeComponent();
        SetupLayout();
        SetupMessageHandlers();
        
        // Process initial game state immediately
        ProcessGameState(initialGameState);
    }

    private void InitializeComponent()
    {
        Text = $"Domino Game - {_roomName}";
        Size = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(800, 600);

        FormClosing += GameForm_FormClosing;
    }

    private void SetupMessageHandlers()
    {
        _clientManager.OnMessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => OnMessageReceived(message));
            return;
        }

        try
        {
            var netMsg = JsonSerializer.Deserialize<NetworkMessage>(message);
            if (netMsg == null) return;

            HandleServerMessage(netMsg);
        }
        catch (JsonException)
        {
            // Invalid JSON, ignore
        }
    }

    /// <summary>
    /// Handles all incoming server messages and routes them to appropriate handlers.
    /// Processes: RoomListUpdated, GameStarted, HandReceived, BoardUpdated, TurnChanged, GameEnded
    /// </summary>
    private void HandleServerMessage(NetworkMessage msg)
    {
        switch (msg.Action)
        {
            case "GAME_STATE":
                HandleGameState(msg);
                break;
            case "PLAY_CARD":
                HandlePlayCardResponse(msg);
                break;
            case "ROOM_LIST_UPDATED":
                HandleRoomListUpdated(msg);
                break;
            case "GAME_STARTED":
                HandleGameStarted(msg);
                break;
            case "HAND_RECEIVED":
                HandleHandReceived(msg);
                break;
            case "BOARD_UPDATED":
                HandleBoardUpdated(msg);
                break;
            case "TURN_CHANGED":
                HandleTurnChanged(msg);
                break;
            case "GAME_ENDED":
                HandleGameEnded(msg);
                break;
            default:
                // Unknown action, log it
                Console.WriteLine($"[GameForm] Unknown message action: {msg.Action}");
                break;
        }
    }

    /// <summary>
    /// Handles ROOM_LIST_UPDATED message - notifies when room availability changes.
    /// Used primarily by LobbyForm but can be handled here if needed.
    /// </summary>
    private void HandleRoomListUpdated(NetworkMessage msg)
    {
        try
        {
            if (msg.Data == null) return;

            var rooms = JsonSerializer.Deserialize<List<DominoShared.Models.Room>>(msg.Data);
            if (rooms == null) return;

            Console.WriteLine($"[GameForm] Room list updated: {rooms.Count} rooms available");
            // In GameForm context, this is informational only
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameForm] Error processing ROOM_LIST_UPDATED: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles GAME_STARTED message - initializes game UI when the game begins.
    /// Called when all players are ready and the game officially starts.
    /// </summary>
    private void HandleGameStarted(NetworkMessage msg)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => HandleGameStarted(msg));
            return;
        }

        try
        {
            if (msg.Data == null) return;

            var gameState = JsonSerializer.Deserialize<DominoShared.Engine.GameState>(msg.Data);
            if (gameState == null) return;

            Console.WriteLine($"[GameForm] Game started in room '{gameState.RoomName}'");
            
            // Process complete game state
            ProcessGameState(gameState);
            
            // Show game started notification
            MessageBox.Show(
                "Game has started! Good luck!",
                "Game Started",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameForm] Error processing GAME_STARTED: {ex.Message}");
            MessageBox.Show(
                $"Error starting game: {ex.Message}",
                "Game Start Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    /// <summary>
    /// Handles HAND_RECEIVED message - updates the current player's hand.
    /// Sent after a card is drawn or when game state is updated.
    /// </summary>
    private void HandleHandReceived(NetworkMessage msg)
    {
        try
        {
            if (msg.Data == null) return;

            var hand = JsonSerializer.Deserialize<List<DominoShared.Models.DominoCard>>(msg.Data);
            if (hand == null) return;

            Console.WriteLine($"[GameForm] Hand updated: {hand.Count} cards");
            
            // Update player's hand display
            DisplayHand(hand);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameForm] Error processing HAND_RECEIVED: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles BOARD_UPDATED message - refreshes the playing board with current tiles.
    /// Called after any tile is played or removed from the board.
    /// </summary>
    private void HandleBoardUpdated(NetworkMessage msg)
    {
        try
        {
            if (msg.Data == null) return;

            var boardTiles = JsonSerializer.Deserialize<List<DominoShared.Models.DominoCard>>(msg.Data);
            if (boardTiles == null) return;

            Console.WriteLine($"[GameForm] Board updated: {boardTiles.Count} tiles on board");
            
            // Render the board with the updated tiles
            RenderBoard(boardTiles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameForm] Error processing BOARD_UPDATED: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles TURN_CHANGED message - updates whose turn it is.
    /// Called after each player's turn to indicate the next active player.
    /// </summary>
    private void HandleTurnChanged(NetworkMessage msg)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => HandleTurnChanged(msg));
            return;
        }

        try
        {
            if (msg.Data == null) return;

            var turnData = JsonSerializer.Deserialize<TurnChangedData>(msg.Data);
            if (turnData == null) return;

            Console.WriteLine($"[GameForm] Turn changed to: {turnData.CurrentPlayerUsername}");
            
            // Update the current turn display
            UpdateCurrentTurn(turnData.CurrentPlayerUsername);
            
            // Optional: Show notification if it's the current player's turn
            if (turnData.CurrentPlayerUsername == _username)
            {
                // Flash or highlight to indicate it's player's turn
                _lblCurrentTurn.BackColor = Color.FromArgb(0, 180, 0);
            }
            else
            {
                _lblCurrentTurn.BackColor = Color.Transparent;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameForm] Error processing TURN_CHANGED: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles GAME_ENDED message - processes game completion and displays results.
    /// Called when a player wins or the game ends in any other condition.
    /// </summary>
    private void HandleGameEnded(NetworkMessage msg)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => HandleGameEnded(msg));
            return;
        }

        try
        {
            if (msg.Data == null) return;

            var endData = JsonSerializer.Deserialize<GameEndedData>(msg.Data);
            if (endData == null) return;

            Console.WriteLine($"[GameForm] Game ended. Winner: {endData.Winner}");
            
            // Display game end results
            string resultMessage = $"Game Over!\n\nWinner: {endData.Winner}";
            
            // Add final scores if available
            if (endData.FinalScores != null && endData.FinalScores.Count > 0)
            {
                resultMessage += "\n\nFinal Scores:\n";
                foreach (var score in endData.FinalScores)
                {
                    resultMessage += $"  {score.Key}: {score.Value}\n";
                }
            }

            MessageBox.Show(
                resultMessage,
                "Game Ended",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // Return to lobby or close game form
            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameForm] Error processing GAME_ENDED: {ex.Message}");
            MessageBox.Show(
                $"Error ending game: {ex.Message}",
                "Game End Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void HandlePlayCardResponse(NetworkMessage message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => HandlePlayCardResponse(message));
            return;
        }

        // Re-enable hand
        SetHandEnabled(true);

        if (!message.Success)
        {
            MessageBox.Show(
                message.ErrorMessage ?? "Failed to play card",
                "Invalid Move",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        // If successful, the GAME_STATE update will reflect the change
    }

    private void HandleGameState(NetworkMessage message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => HandleGameState(message));
            return;
        }

        if (message.Data == null) return;

        try
        {
            var gameState = JsonSerializer.Deserialize<DominoShared.Engine.GameState>(message.Data);
            if (gameState == null) return;

            ProcessGameState(gameState);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to process game state: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ProcessGameState(DominoShared.Engine.GameState gameState)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ProcessGameState(gameState));
            return;
        }

        // Update current turn
        var currentPlayer = gameState.GetCurrentPlayerUsername();
        UpdateCurrentTurn(currentPlayer);

        // Update score
        if (gameState.TotalScores.TryGetValue(_username, out var score))
        {
            UpdateScore(score);
        }

        // Update player's hand - use DisplayHand with DominoCard objects directly
        if (gameState.PlayerHands.TryGetValue(_username, out var hand))
        {
            DisplayHand(hand);
        }

        // Update board
        var boardStrings = gameState.TableCards.Select(c => $"{c.LeftValue}-{c.RightValue}").ToList();
        UpdateBoard(boardStrings);
    }

    private void SetupLayout()
    {
        // Top Panel - Game Info
        _topPanel.Dock = DockStyle.Top;
        _topPanel.Height = 80;
        _topPanel.Padding = new Padding(15);
        _topPanel.BackColor = Color.FromArgb(240, 240, 240);

        var topLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(5)
        };

        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
        topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));

        // Current Turn Label
        _lblCurrentTurn.Text = "Current Turn: Waiting...";
        _lblCurrentTurn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _lblCurrentTurn.TextAlign = ContentAlignment.MiddleLeft;
        _lblCurrentTurn.Dock = DockStyle.Fill;
        _lblCurrentTurn.ForeColor = Color.FromArgb(0, 120, 215);

        // Room Name Label (center)
        var lblRoomName = new Label
        {
            Text = _roomName,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(50, 50, 50)
        };

        // Score Label
        _lblScore.Text = "Score: 0";
        _lblScore.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _lblScore.TextAlign = ContentAlignment.MiddleRight;
        _lblScore.Dock = DockStyle.Fill;
        _lblScore.ForeColor = Color.FromArgb(0, 120, 215);

        topLayout.Controls.Add(_lblCurrentTurn, 0, 0);
        topLayout.Controls.Add(lblRoomName, 1, 0);
        topLayout.Controls.Add(_lblScore, 2, 0);

        _topPanel.Controls.Add(topLayout);

        // Board Panel - Domino Playing Area (Center)
        _boardPanel.Dock = DockStyle.Fill;
        _boardPanel.BackColor = Color.FromArgb(34, 139, 34); // Forest Green
        _boardPanel.Padding = new Padding(10);
        _boardPanel.AutoScroll = true;

        var lblBoardPlaceholder = new Label
        {
            Text = "Waiting for game to start...",
            Font = new Font("Segoe UI", 14F, FontStyle.Italic),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 20),
            BackColor = Color.Transparent
        };
        _boardPanel.Controls.Add(lblBoardPlaceholder);

        // Hand Panel - Player's Cards (Bottom)
        _handPanel.Dock = DockStyle.Fill;
        _handPanel.FlowDirection = FlowDirection.LeftToRight;
        _handPanel.AutoScroll = true;
        _handPanel.BackColor = Color.FromArgb(245, 245, 245);
        _handPanel.Padding = new Padding(10);
        _handPanel.WrapContents = false;

        var lblHandPlaceholder = new Label
        {
            Text = "Waiting for cards...",
            Font = new Font("Segoe UI", 10F, FontStyle.Italic),
            ForeColor = Color.Gray,
            AutoSize = true,
            Margin = new Padding(10)
        };
        _handPanel.Controls.Add(lblHandPlaceholder);

        // Bottom Panel - Controls
        _bottomPanel.Dock = DockStyle.Bottom;
        _bottomPanel.Height = 180;
        _bottomPanel.BackColor = Color.FromArgb(250, 250, 250);

        var bottomSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 120,
            IsSplitterFixed = true
        };

        bottomSplit.Panel1.Controls.Add(_handPanel);

        // Control buttons panel
        var controlPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10, 5, 10, 5)
        };

        _btnLeaveGame.Text = "Leave Game";
        _btnLeaveGame.Size = new Size(120, 40);
        _btnLeaveGame.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _btnLeaveGame.BackColor = Color.FromArgb(220, 53, 69); // Red
        _btnLeaveGame.ForeColor = Color.White;
        _btnLeaveGame.FlatStyle = FlatStyle.Flat;
        _btnLeaveGame.Margin = new Padding(5);
        _btnLeaveGame.Click += BtnLeaveGame_Click;

        var lblPlayerInfo = new Label
        {
            Text = $"Player: {_username}",
            Font = new Font("Segoe UI", 10F),
            AutoSize = true,
            Margin = new Padding(10, 12, 5, 5),
            ForeColor = Color.FromArgb(100, 100, 100)
        };

        controlPanel.Controls.Add(_btnLeaveGame);
        controlPanel.Controls.Add(lblPlayerInfo);

        bottomSplit.Panel2.Controls.Add(controlPanel);

        _bottomPanel.Controls.Add(bottomSplit);

        // Add all panels to form
        Controls.Add(_boardPanel);
        Controls.Add(_topPanel);
        Controls.Add(_bottomPanel);
    }

    public void UpdateCurrentTurn(string playerName)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateCurrentTurn(playerName));
            return;
        }

        _lblCurrentTurn.Text = $"Current Turn: {playerName}";
        _lblCurrentTurn.ForeColor = playerName == _username 
            ? Color.FromArgb(0, 180, 0) // Green if it's your turn
            : Color.FromArgb(0, 120, 215); // Blue otherwise
    }

    public void UpdateScore(int score)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateScore(score));
            return;
        }

        _lblScore.Text = $"Score: {score}";
    }

    public void UpdatePlayerHand(List<string> cards)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdatePlayerHand(cards));
            return;
        }

        _handPanel.Controls.Clear();

        if (cards == null || cards.Count == 0)
        {
            var lblEmpty = new Label
            {
                Text = "No cards in hand",
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Margin = new Padding(10)
            };
            _handPanel.Controls.Add(lblEmpty);
            return;
        }

        foreach (var card in cards)
        {
            // Parse card format "3-5"
            var parts = card.Split('-');
            if (parts.Length != 2) continue;

            if (!int.TryParse(parts[0], out int leftValue) || 
                !int.TryParse(parts[1], out int rightValue))
                continue;

            var dominoTile = new DominoTileControl
            {
                LeftValue = leftValue,
                RightValue = rightValue,
                Margin = new Padding(5),
                Tag = card
            };

            dominoTile.TileClicked += DominoTile_Clicked;

            _handPanel.Controls.Add(dominoTile);
        }
    }

    /// <summary>
    /// Display domino tiles in the hand panel.
    /// Clears existing tiles and adds new DominoTileControl for each tile.
    /// </summary>
    public void DisplayHand(List<DominoShared.Models.DominoCard> tiles)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => DisplayHand(tiles));
            return;
        }

        // Clear existing tiles
        _handPanel.Controls.Clear();

        if (tiles == null || tiles.Count == 0)
        {
            var lblEmpty = new Label
            {
                Text = "No cards in hand",
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Margin = new Padding(10)
            };
            _handPanel.Controls.Add(lblEmpty);
            return;
        }

        // Add DominoTileControl for each tile
        foreach (var tile in tiles)
        {
            var dominoTile = new DominoTileControl
            {
                LeftValue = tile.LeftValue,
                RightValue = tile.RightValue,
                Margin = new Padding(5),
                Tag = tile // Store the actual tile object for later use
            };

            dominoTile.TileClicked += DominoTile_Clicked;

            _handPanel.Controls.Add(dominoTile);
        }
    }

    private void DominoTile_Clicked(object? sender, EventArgs e)
    {
        if (sender is not DominoTileControl tile) return;

        // Check if it's the current player's turn
        if (_lblCurrentTurn.Text.Contains(_username) && _lblCurrentTurn.ForeColor == Color.FromArgb(0, 180, 0))
        {
            // Deselect all other tiles
            foreach (Control control in _handPanel.Controls)
            {
                if (control is DominoTileControl otherTile && otherTile != tile)
                {
                    otherTile.IsSelected = false;
                }
            }

            // Toggle selection on clicked tile
            tile.IsSelected = !tile.IsSelected;

            if (tile.IsSelected)
            {
                // Ask for confirmation
                var result = MessageBox.Show(
                    $"Play domino {tile.LeftValue}-{tile.RightValue}?",
                    "Play Card",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    _ = PlayCardAsync(tile.LeftValue, tile.RightValue);
                }
                else
                {
                    tile.IsSelected = false;
                }
            }
        }
        else
        {
            MessageBox.Show("It's not your turn!", "Wait", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async Task PlayCardAsync(int leftValue, int rightValue)
    {
        try
        {
            // Disable hand while waiting for response
            SetHandEnabled(false);

            var playCardRequest = new
            {
                LeftValue = leftValue,
                RightValue = rightValue
            };

            var message = new NetworkMessage
            {
                Action = "PLAY_CARD",
                Username = _username,
                RoomName = _roomName,
                Data = JsonSerializer.Serialize(playCardRequest),
                Timestamp = DateTime.UtcNow
            };

            await _clientManager.SendAsync(JsonSerializer.Serialize(message));
        }
        catch (Exception ex)
        {
            SetHandEnabled(true);
            MessageBox.Show($"Failed to play card: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetHandEnabled(bool enabled)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetHandEnabled(enabled));
            return;
        }

        foreach (Control control in _handPanel.Controls)
        {
            if (control is DominoTileControl tile)
            {
                tile.Enabled = enabled;
                tile.Cursor = enabled ? Cursors.Hand : Cursors.WaitCursor;
            }
        }

        // Change panel background color to indicate disabled state
        _handPanel.BackColor = enabled 
            ? Color.FromArgb(245, 245, 245) 
            : Color.FromArgb(220, 220, 220);
    }

    public void UpdateBoard(List<string> boardCards)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateBoard(boardCards));
            return;
        }

        _boardPanel.Controls.Clear();

        if (boardCards == null || boardCards.Count == 0)
        {
            var lblEmpty = new Label
            {
                Text = "Board is empty\nPlay the first domino!",
                Font = new Font("Segoe UI", 14F, FontStyle.Italic),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };
            _boardPanel.Controls.Add(lblEmpty);
            return;
        }

        var tiles = new List<DominoShared.Models.DominoCard>();
        foreach (var card in boardCards)
        {
            var parts = card.Split('-');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], out int leftValue) && 
                int.TryParse(parts[1], out int rightValue))
            {
                tiles.Add(new DominoShared.Models.DominoCard(leftValue, rightValue));
            }
        }

        RenderBoard(tiles);
    }

    /// <summary>
    /// Renders domino tiles horizontally in the board panel with support for left and right placement.
    /// Displays tiles in two rows: left side tiles and right side tiles, indicating play direction.
    /// </summary>
    /// <param name="boardTiles">List of domino tiles to render on the board</param>
    public void RenderBoard(List<DominoShared.Models.DominoCard> boardTiles)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => RenderBoard(boardTiles));
            return;
        }

        _boardPanel.Controls.Clear();

        if (boardTiles == null || boardTiles.Count == 0)
        {
            var lblEmpty = new Label
            {
                Text = "Board is empty\nPlay the first domino!",
                Font = new Font("Segoe UI", 14F, FontStyle.Italic),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };
            _boardPanel.Controls.Add(lblEmpty);
            return;
        }

        // Create main container
        var mainContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(10),
            BackColor = Color.Transparent,
            AutoScroll = true
        };

        // Set row styles
        mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Left label
        mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // Tiles
        mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Right section

        // Left side label
        var lblLeft = new Label
        {
            Text = "← Left Side",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Dock = DockStyle.Left,
            Margin = new Padding(5),
            BackColor = Color.Transparent
        };
        mainContainer.Controls.Add(lblLeft, 0, 0);

        // Left tiles flow panel
        var leftFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding = new Padding(5),
            WrapContents = false,
            Height = 130
        };

        // Right tiles flow panel
        var rightFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding = new Padding(5),
            WrapContents = false,
            Height = 130
        };

        // Determine middle index (first tile goes in center)
        int middleIndex = boardTiles.Count > 0 ? 0 : -1;

        // Add tiles
        for (int i = 0; i < boardTiles.Count; i++)
        {
            var tile = boardTiles[i];
            var dominoTile = new DominoTileControl
            {
                LeftValue = tile.LeftValue,
                RightValue = tile.RightValue,
                Margin = new Padding(3),
                Size = new Size(80, 120)
            };

            dominoTile.Enabled = false;

            if (i < middleIndex)
            {
                // Tiles before middle go to left (added in reverse order)
                leftFlow.Controls.Add(dominoTile);
            }
            else if (i == middleIndex)
            {
                // Middle tile (first placed) - add to both but highlight
                var centerTile = new DominoTileControl
                {
                    LeftValue = tile.LeftValue,
                    RightValue = tile.RightValue,
                    Margin = new Padding(3),
                    Size = new Size(80, 120)
                };
                centerTile.Enabled = false;
                leftFlow.Controls.Add(centerTile);
            }
            else
            {
                // Tiles after middle go to right
                rightFlow.Controls.Add(dominoTile);
            }
        }

        mainContainer.Controls.Add(leftFlow, 0, 1);

        // Right section container
        var rightSection = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = Color.Transparent,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        rightSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Right label
        rightSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // Tiles

        var lblRight = new Label
        {
            Text = "Right Side →",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Dock = DockStyle.Left,
            Margin = new Padding(5),
            BackColor = Color.Transparent
        };

        rightSection.Controls.Add(lblRight, 0, 0);
        rightSection.Controls.Add(rightFlow, 0, 1);

        mainContainer.Controls.Add(rightSection, 0, 2);

        _boardPanel.Controls.Add(mainContainer);
    }

    private void BtnLeaveGame_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to leave the game?",
            "Leave Game",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            Close();
        }
    }

    private void GameForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _clientManager.OnMessageReceived -= OnMessageReceived;
        // TODO: Send leave game message to server
    }
}

/// <summary>
/// Data structure for TURN_CHANGED message payload.
/// Contains information about whose turn it is in the game.
/// </summary>
public class TurnChangedData
{
    public string CurrentPlayerUsername { get; set; } = string.Empty;
    public int CurrentPlayerIndex { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Data structure for GAME_ENDED message payload.
/// Contains game results including winner and final scores.
/// </summary>
public class GameEndedData
{
    public string Winner { get; set; } = string.Empty;
    public Dictionary<string, int> FinalScores { get; set; } = new();
    public string? RoomName { get; set; }
    public DateTime Timestamp { get; set; }
}

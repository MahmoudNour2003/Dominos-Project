using System.Text.Json;
using DominoClient.Networking;
using DominoShared.DTOs;

namespace DominoClient;

public class LobbyForm : Form
{
    private readonly ClientManager _clientManager;
    private readonly string _username;
    private GameForm? _currentGameForm; // Track current game form to prevent duplicates

    private readonly Label _lblUsername;
    private readonly ListBox _lstRooms;
    private readonly Button _btnCreateRoom;
    private readonly Button _btnJoinRoom;
    private readonly Button _btnRefresh;
    private readonly Panel _topPanel;
    private readonly Panel _bottomPanel;

    public LobbyForm(ClientManager clientManager, string username)
    {
        _clientManager = clientManager;
        _username = username;

        _lblUsername = new Label();
        _lstRooms = new ListBox();
        _btnCreateRoom = new Button();
        _btnJoinRoom = new Button();
        _btnRefresh = new Button();
        _topPanel = new Panel();
        _bottomPanel = new Panel();

        InitializeComponent();
        SetupLayout();
        SetupMessageHandlers();
    }

    private void InitializeComponent()
    {
        Text = "Domino Lobby";
        Size = new Size(600, 500);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(500, 400);

        FormClosing += LobbyForm_FormClosing;
    }

    private void SetupMessageHandlers()
    {
        _clientManager.OnMessageReceived += OnMessageReceived;
    }

    private void SetupLayout()
    {
        // Top Panel - Username Display
        _topPanel.Dock = DockStyle.Top;
        _topPanel.Height = 50;
        _topPanel.Padding = new Padding(10);
        _topPanel.BackColor = Color.FromArgb(240, 240, 240);

        _lblUsername.Text = $"Connected as: {_username}";
        _lblUsername.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _lblUsername.AutoSize = true;
        _lblUsername.Location = new Point(10, 12);
        _lblUsername.ForeColor = Color.FromArgb(0, 120, 215);

        _topPanel.Controls.Add(_lblUsername);

        // Room ListBox
        _lstRooms.Dock = DockStyle.Fill;
        _lstRooms.Font = new Font("Consolas", 10F);
        _lstRooms.ItemHeight = 20;
        _lstRooms.SelectionMode = SelectionMode.One;
        _lstRooms.DoubleClick += LstRooms_DoubleClick;

        // Bottom Panel - Action Buttons
        _bottomPanel.Dock = DockStyle.Bottom;
        _bottomPanel.Height = 70;
        _bottomPanel.Padding = new Padding(10);

        var buttonLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(5),
            WrapContents = false
        };

        _btnCreateRoom.Text = "Create Room";
        _btnCreateRoom.Size = new Size(120, 40);
        _btnCreateRoom.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _btnCreateRoom.BackColor = Color.FromArgb(0, 120, 215);
        _btnCreateRoom.ForeColor = Color.White;
        _btnCreateRoom.FlatStyle = FlatStyle.Flat;
        _btnCreateRoom.Margin = new Padding(5);
        _btnCreateRoom.Click += BtnCreateRoom_Click;

        _btnJoinRoom.Text = "Join Room";
        _btnJoinRoom.Size = new Size(120, 40);
        _btnJoinRoom.Font = new Font("Segoe UI", 10F);
        _btnJoinRoom.Margin = new Padding(5);
        _btnJoinRoom.Click += BtnJoinRoom_Click;

        _btnRefresh.Text = "Refresh";
        _btnRefresh.Size = new Size(120, 40);
        _btnRefresh.Font = new Font("Segoe UI", 10F);
        _btnRefresh.Margin = new Padding(5);
        _btnRefresh.Click += BtnRefresh_Click;

        buttonLayout.Controls.Add(_btnCreateRoom);
        buttonLayout.Controls.Add(_btnJoinRoom);
        buttonLayout.Controls.Add(_btnRefresh);

        _bottomPanel.Controls.Add(buttonLayout);

        // Add all panels to form
        Controls.Add(_lstRooms);
        Controls.Add(_topPanel);
        Controls.Add(_bottomPanel);

        // Initial refresh
        LoadRooms();
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

            switch (netMsg.Action)
            {
                case "ROOM_LIST":
                    HandleRoomList(netMsg);
                    break;
                case "CREATE_ROOM":
                    HandleCreateRoomResponse(netMsg);
                    break;
                case "JOIN_ROOM":
                    HandleJoinRoomResponse(netMsg);
                    break;
                case "GAME_STARTED":
                    HandleGameStarted(netMsg);
                    break;
                case "GAME_STATE":
                    // Also handle GAME_STATE in case that's what triggers the game
                    HandleGameState(netMsg);
                    break;
            }
        }
        catch (JsonException)
        {
            // Invalid JSON, ignore
        }
    }

    private void HandleGameState(NetworkMessage message)
    {
        if (message.Data == null) return;

        try
        {
            var gameState = JsonSerializer.Deserialize<DominoShared.Engine.GameState>(message.Data);
            if (gameState != null && gameState.IsGameActive)
            {
                // Only open game form if game is active and we haven't already opened it
                OpenGameForm(gameState);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LobbyForm] Error processing GAME_STATE: {ex.Message}");
        }
    }

    private void HandleGameStarted(NetworkMessage message)
    {
        if (message.Data == null) return;

        try
        {
            var gameState = JsonSerializer.Deserialize<DominoShared.Engine.GameState>(message.Data);
            if (gameState != null)
            {
                OpenGameForm(gameState);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load game state: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleJoinRoomResponse(NetworkMessage message)
    {
        _btnJoinRoom.Enabled = true;

        if (message.Success)
        {
            MessageBox.Show("Joined room successfully! Waiting for game to start...", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show(message.ErrorMessage ?? "Failed to join room", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleRoomList(NetworkMessage message)
    {
        if (message.Data == null)
        {
            UpdateRoomList(new List<string>());
            return;
        }

        try
        {
            var rooms = JsonSerializer.Deserialize<List<DominoShared.Models.Room>>(message.Data);
            if (rooms != null)
            {
                var roomDisplayList = rooms.Select(r => 
                    $"{r.Name} ({r.Players.Count}/{r.MaxPlayers}) - {r.Status}").ToList();
                UpdateRoomList(roomDisplayList);
            }
        }
        catch
        {
            UpdateRoomList(new List<string>());
        }
    }

    private void HandleCreateRoomResponse(NetworkMessage message)
    {
        _btnCreateRoom.Enabled = true;

        if (message.Success)
        {
            MessageBox.Show("Room created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RequestRoomList();
        }
        else
        {
            MessageBox.Show(message.ErrorMessage ?? "Failed to create room", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadRooms()
    {
        _lstRooms.Items.Clear();
        _lstRooms.Items.Add("Loading rooms...");
        RequestRoomList();
    }

    private async void RequestRoomList()
    {
        try
        {
            var message = new NetworkMessage
            {
                Action = "GET_ROOMS",
                Username = _username,
                Timestamp = DateTime.UtcNow
            };

            await _clientManager.SendAsync(JsonSerializer.Serialize(message));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to request room list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void UpdateRoomList(List<string> rooms)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateRoomList(rooms));
            return;
        }

        _lstRooms.Items.Clear();

        if (rooms == null || rooms.Count == 0)
        {
            _lstRooms.Items.Add("No rooms available");
            return;
        }

        foreach (var room in rooms)
        {
            _lstRooms.Items.Add(room);
        }
    }

    private async void BtnCreateRoom_Click(object? sender, EventArgs e)
    {
        var roomSettings = ShowCreateRoomDialog();
        if (roomSettings == null)
        {
            return;
        }

        _btnCreateRoom.Enabled = false;

        try
        {
            var createRoomRequest = new
            {
                RoomName = roomSettings.RoomName,
                MaxPlayers = roomSettings.MaxPlayers,
                WinningScore = roomSettings.WinningScore
            };

            var message = new NetworkMessage
            {
                Action = "CREATE_ROOM",
                Username = _username,
                Data = JsonSerializer.Serialize(createRoomRequest),
                Timestamp = DateTime.UtcNow
            };

            await _clientManager.SendAsync(JsonSerializer.Serialize(message));
        }
        catch (Exception ex)
        {
            _btnCreateRoom.Enabled = true;
            MessageBox.Show($"Failed to send create room request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private RoomSettings? ShowCreateRoomDialog()
    {
        using var dialog = new Form
        {
            Text = "Create Room",
            Size = new Size(400, 250),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 4
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

        // Room Name
        var lblRoomName = new Label { Text = "Room Name:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        var txtRoomName = new TextBox { Dock = DockStyle.Fill };

        // Max Players
        var lblMaxPlayers = new Label { Text = "Max Players:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        var numMaxPlayers = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 2, Maximum = 4, Value = 2 };

        // Winning Score
        var lblWinScore = new Label { Text = "Winning Score:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        var numWinScore = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 50, Maximum = 500, Value = 100, Increment = 10 };

        // Buttons
        var btnOk = new Button { Text = "Create", DialogResult = DialogResult.OK, Dock = DockStyle.Fill };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Dock = DockStyle.Fill };

        layout.Controls.Add(lblRoomName, 0, 0);
        layout.Controls.Add(txtRoomName, 1, 0);
        layout.Controls.Add(lblMaxPlayers, 0, 1);
        layout.Controls.Add(numMaxPlayers, 1, 1);
        layout.Controls.Add(lblWinScore, 0, 2);
        layout.Controls.Add(numWinScore, 1, 2);
        layout.Controls.Add(btnCancel, 0, 3);
        layout.Controls.Add(btnOk, 1, 3);

        dialog.Controls.Add(layout);
        dialog.AcceptButton = btnOk;
        dialog.CancelButton = btnCancel;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var roomName = txtRoomName.Text.Trim();
            if (string.IsNullOrWhiteSpace(roomName))
            {
                MessageBox.Show("Please enter a room name", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return new RoomSettings(
                roomName,
                (int)numMaxPlayers.Value,
                (int)numWinScore.Value
            );
        }

        return null;
    }

    private async void BtnJoinRoom_Click(object? sender, EventArgs e)
    {
        if (_lstRooms.SelectedItem == null)
        {
            MessageBox.Show("Please select a room to join", "No Room Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var roomDisplayText = _lstRooms.SelectedItem.ToString();
        if (string.IsNullOrEmpty(roomDisplayText)) return;

        // Extract room name from display format: "RoomName (2/4) - Waiting"
        var roomName = ExtractRoomName(roomDisplayText);

        _btnJoinRoom.Enabled = false;

        try
        {
            var message = new NetworkMessage
            {
                Action = "JOIN_ROOM",
                Username = _username,
                RoomName = roomName,
                Timestamp = DateTime.UtcNow
            };

            await _clientManager.SendAsync(JsonSerializer.Serialize(message));
        }
        catch (Exception ex)
        {
            _btnJoinRoom.Enabled = true;
            MessageBox.Show($"Failed to send join room request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string ExtractRoomName(string displayText)
    {
        // Format: "RoomName (2/4) - Waiting"
        var openParenIndex = displayText.IndexOf('(');
        if (openParenIndex > 0)
        {
            return displayText.Substring(0, openParenIndex).Trim();
        }
        return displayText.Trim();
    }

    private void OpenGameForm(DominoShared.Engine.GameState initialGameState)
    {
        // Don't open multiple game forms
        if (_currentGameForm != null && !_currentGameForm.IsDisposed)
        {
            _currentGameForm.Activate(); // Bring existing form to front
            return;
        }

        // Unsubscribe from messages since GameForm will handle them
        _clientManager.OnMessageReceived -= OnMessageReceived;
        
        // Create and track the game form
        _currentGameForm = new GameForm(_clientManager, _username, initialGameState);
        
        // Hide lobby and show game form
        Hide();
        _currentGameForm.FormClosed += (s, e) =>
        {
            // When game form closes, resubscribe to messages and show lobby again
            _clientManager.OnMessageReceived += OnMessageReceived;
            _currentGameForm = null;
            Show();
            RequestRoomList();
        };
        _currentGameForm.Show();
    }

    private void BtnRefresh_Click(object? sender, EventArgs e)
    {
        LoadRooms();
    }

    private void LstRooms_DoubleClick(object? sender, EventArgs e)
    {
        if (_lstRooms.SelectedItem != null)
        {
            BtnJoinRoom_Click(sender, e);
        }
    }

    private void LobbyForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _clientManager.OnMessageReceived -= OnMessageReceived;
        _clientManager?.Disconnect();
    }

    private record RoomSettings(string RoomName, int MaxPlayers, int WinningScore);
}

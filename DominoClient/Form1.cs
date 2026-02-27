using System.ComponentModel;

namespace DominoClient
{
    public partial class Form1 : Form
    {
        private readonly BindingList<RoomViewModel> _rooms = new();
        private readonly List<DominoTileView> _handTiles = new();
        private readonly List<DominoTileView> _boardTiles = new();

        public Form1()
        {
            InitializeComponent();
            WireEvents();
            ConfigureRoomGrid();
            SeedUiMockData();
            UpdateStatus("Disconnected. Configure host and connect.");
        }

        private void WireEvents()
        {
            btnConnect.Click += (_, _) => OnConnectClicked();
            btnRefreshRooms.Click += (_, _) => RefreshRooms();
            btnCreateRoom.Click += (_, _) => CreateRoom();
            btnJoinRoom.Click += (_, _) => JoinSelectedRoom(false);
            btnWatchRoom.Click += (_, _) => JoinSelectedRoom(true);
            btnPlaySelected.Click += (_, _) => PlaySelectedTile();
            btnDraw.Click += (_, _) => UpdateStatus("Draw request sent (placeholder).", true);
            btnPass.Click += (_, _) => UpdateStatus("Pass request sent (placeholder).", true);
        }

        private void ConfigureRoomGrid()
        {
            roomsGrid.AutoGenerateColumns = false;
            roomsGrid.Columns.Clear();
            roomsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RoomViewModel.RoomName), HeaderText = "Room" });
            roomsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RoomViewModel.Host), HeaderText = "Host" });
            roomsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RoomViewModel.PlayerCount), HeaderText = "Players" });
            roomsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RoomViewModel.State), HeaderText = "State" });
            roomsGrid.DataSource = _rooms;
        }

        private void SeedUiMockData()
        {
            _rooms.Clear();
            _rooms.Add(new RoomViewModel("Beginners", "Alice", "1/2", "Waiting"));
            _rooms.Add(new RoomViewModel("Pro Table", "Hassan", "3/4", "Waiting"));
            _rooms.Add(new RoomViewModel("Final Match", "Nour", "4/4", "Running"));

            LoadPlayers(new[]
            {
                ("You", "Active", "15"),
                ("Alice", "Waiting", "8"),
                ("Hassan", "Watching", "0")
            });

            DrawBoard(new[] { "6|6", "6|5", "5|2" });
            DrawHand(new[] { "4|4", "4|2", "2|1", "1|1", "0|1", "0|4", "6|1" });
            lblCurrentTurn.Text = "Turn: You";
        }

        private void OnConnectClicked()
        {
            var player = txtPlayerName.Text.Trim();
            if (string.IsNullOrWhiteSpace(player))
            {
                UpdateStatus("Please enter player name before connecting.", true);
                return;
            }

            UpdateStatus($"Connected as {player} to {txtServerHost.Text}:{nudServerPort.Value}.");
        }

        private void RefreshRooms()
        {
            UpdateStatus("Room list refreshed (placeholder).", true);
        }

        private void CreateRoom()
        {
            var roomName = string.IsNullOrWhiteSpace(txtRoomName.Text) ? "Unnamed Room" : txtRoomName.Text.Trim();
            var maxPlayers = (int)nudMaxPlayers.Value;
            _rooms.Add(new RoomViewModel(roomName, txtPlayerName.Text.Trim(), $"1/{maxPlayers}", "Waiting"));
            UpdateStatus($"Room '{roomName}' created. Point limit: {nudPointLimit.Value}.");
        }

        private void JoinSelectedRoom(bool watching)
        {
            if (roomsGrid.CurrentRow?.DataBoundItem is not RoomViewModel room)
            {
                UpdateStatus("Select a room first.", true);
                return;
            }

            var mode = watching ? "watching" : "playing";
            UpdateStatus($"Joined '{room.RoomName}' for {mode} (placeholder).", true);
        }

        private void DrawBoard(IEnumerable<string> tiles)
        {
            _boardTiles.Clear();
            boardPanel.Controls.Clear();

            foreach (var tile in tiles)
            {
                var control = new DominoTileView(tile, false);
                _boardTiles.Add(control);
                boardPanel.Controls.Add(control);
            }
        }

        private void DrawHand(IEnumerable<string> tiles)
        {
            _handTiles.Clear();
            handPanel.Controls.Clear();

            foreach (var tile in tiles)
            {
                var control = new DominoTileView(tile, true);
                _handTiles.Add(control);
                handPanel.Controls.Add(control);
            }
        }

        private void PlaySelectedTile()
        {
            var selected = _handTiles.FirstOrDefault(x => x.Selected);
            if (selected is null)
            {
                UpdateStatus("Select a tile to play.", true);
                return;
            }

            selected.Selected = false;
            _handTiles.Remove(selected);
            handPanel.Controls.Remove(selected);

            var boardTile = new DominoTileView(selected.TileText, false);
            _boardTiles.Add(boardTile);
            boardPanel.Controls.Add(boardTile);

            UpdateStatus($"Played tile {selected.TileText}.");
        }

        private void LoadPlayers(IEnumerable<(string Name, string Status, string Points)> players)
        {
            lstPlayers.Items.Clear();
            foreach (var player in players)
            {
                var item = new ListViewItem(player.Name);
                item.SubItems.Add(player.Status);
                item.SubItems.Add(player.Points);
                lstPlayers.Items.Add(item);
            }
        }

        private void UpdateStatus(string message, bool isInformational = false)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = isInformational ? Color.DarkSlateBlue : Color.DarkGreen;
        }

        private sealed record RoomViewModel(string RoomName, string Host, string PlayerCount, string State);

        private sealed class DominoTileView : Button
        {
            public DominoTileView(string text, bool canSelect)
            {
                TileText = text;
                CanSelect = canSelect;
                Text = canSelect ? text : "■";
                Margin = new Padding(6);
                Width = 74;
                Height = 104;
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 1;
                BackColor = Color.White;

                if (canSelect)
                {
                    Click += (_, _) => Selected = !Selected;
                }
            }

            public string TileText { get; }

            public bool CanSelect { get; }

            public bool Selected
            {
                get => BackColor == Color.LightBlue;
                set
                {
                    if (!CanSelect)
                    {
                        return;
                    }

                    BackColor = value ? Color.LightBlue : Color.White;
                }
            }
        }
    }
}

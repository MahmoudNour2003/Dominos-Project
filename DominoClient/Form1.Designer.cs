namespace DominoClient
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            var mainLayout = new TableLayoutPanel();
            lobbyGroup = new GroupBox();
            roomsLayout = new TableLayoutPanel();
            roomsGrid = new DataGridView();
            roomActionsPanel = new FlowLayoutPanel();
            btnRefreshRooms = new Button();
            btnJoinRoom = new Button();
            btnWatchRoom = new Button();
            creationGroup = new GroupBox();
            createLayout = new TableLayoutPanel();
            txtRoomName = new TextBox();
            nudMaxPlayers = new NumericUpDown();
            nudPointLimit = new NumericUpDown();
            btnCreateRoom = new Button();
            loginGroup = new GroupBox();
            loginLayout = new TableLayoutPanel();
            txtPlayerName = new TextBox();
            txtServerHost = new TextBox();
            nudServerPort = new NumericUpDown();
            btnConnect = new Button();
            gameGroup = new GroupBox();
            gameLayout = new TableLayoutPanel();
            lblCurrentTurn = new Label();
            boardPanel = new FlowLayoutPanel();
            handPanel = new FlowLayoutPanel();
            actionPanel = new FlowLayoutPanel();
            btnPlaySelected = new Button();
            btnDraw = new Button();
            btnPass = new Button();
            lstPlayers = new ListView();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)roomsGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMaxPlayers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPointLimit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudServerPort).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 145F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            // 
            // loginGroup
            // 
            loginGroup.Dock = DockStyle.Fill;
            loginGroup.Text = "Connection";
            loginGroup.Controls.Add(loginLayout);
            mainLayout.Controls.Add(loginGroup, 0, 0);
            // 
            // loginLayout
            // 
            loginLayout.ColumnCount = 2;
            loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
            loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            loginLayout.Dock = DockStyle.Fill;
            loginLayout.RowCount = 4;
            loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            loginLayout.Controls.Add(new Label { Text = "Player", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            loginLayout.Controls.Add(new Label { Text = "Server", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            loginLayout.Controls.Add(new Label { Text = "Port", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            loginLayout.Controls.Add(txtPlayerName, 1, 0);
            loginLayout.Controls.Add(txtServerHost, 1, 1);
            loginLayout.Controls.Add(nudServerPort, 1, 2);
            loginLayout.Controls.Add(btnConnect, 1, 3);
            txtPlayerName.Dock = DockStyle.Fill;
            txtPlayerName.PlaceholderText = "Enter your nickname";
            txtServerHost.Dock = DockStyle.Fill;
            txtServerHost.Text = "127.0.0.1";
            nudServerPort.Dock = DockStyle.Left;
            nudServerPort.Maximum = 65535;
            nudServerPort.Minimum = 1000;
            nudServerPort.Value = 5000;
            btnConnect.AutoSize = true;
            btnConnect.Text = "Connect";
            // 
            // lobbyGroup
            // 
            lobbyGroup.Dock = DockStyle.Fill;
            lobbyGroup.Text = "Rooms";
            lobbyGroup.Controls.Add(roomsLayout);
            mainLayout.Controls.Add(lobbyGroup, 0, 1);
            mainLayout.SetRowSpan(lobbyGroup, 2);
            // 
            // roomsLayout
            // 
            roomsLayout.ColumnCount = 1;
            roomsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            roomsLayout.Dock = DockStyle.Fill;
            roomsLayout.RowCount = 3;
            roomsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            roomsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            roomsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            roomsLayout.Controls.Add(roomsGrid, 0, 0);
            roomsLayout.Controls.Add(roomActionsPanel, 0, 1);
            roomsLayout.Controls.Add(creationGroup, 0, 2);
            // 
            // roomsGrid
            // 
            roomsGrid.AllowUserToAddRows = false;
            roomsGrid.AllowUserToDeleteRows = false;
            roomsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            roomsGrid.Dock = DockStyle.Fill;
            roomsGrid.MultiSelect = false;
            roomsGrid.ReadOnly = true;
            roomsGrid.RowHeadersVisible = false;
            roomsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            // 
            // roomActionsPanel
            // 
            roomActionsPanel.Dock = DockStyle.Fill;
            roomActionsPanel.Controls.AddRange(new Control[] { btnRefreshRooms, btnJoinRoom, btnWatchRoom });
            btnRefreshRooms.Text = "Refresh";
            btnJoinRoom.Text = "Join Room";
            btnWatchRoom.Text = "Watch";
            // 
            // creationGroup
            // 
            creationGroup.Dock = DockStyle.Fill;
            creationGroup.Text = "Create Room";
            creationGroup.Controls.Add(createLayout);
            // 
            // createLayout
            // 
            createLayout.ColumnCount = 2;
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
            createLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            createLayout.Dock = DockStyle.Fill;
            createLayout.RowCount = 4;
            createLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            createLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            createLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            createLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            createLayout.Controls.Add(new Label { Text = "Name", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            createLayout.Controls.Add(new Label { Text = "Players", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            createLayout.Controls.Add(new Label { Text = "Point Limit", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            createLayout.Controls.Add(txtRoomName, 1, 0);
            createLayout.Controls.Add(nudMaxPlayers, 1, 1);
            createLayout.Controls.Add(nudPointLimit, 1, 2);
            createLayout.Controls.Add(btnCreateRoom, 1, 3);
            txtRoomName.Dock = DockStyle.Fill;
            txtRoomName.PlaceholderText = "Room name";
            nudMaxPlayers.Minimum = 2;
            nudMaxPlayers.Maximum = 4;
            nudMaxPlayers.Value = 2;
            nudPointLimit.Minimum = 50;
            nudPointLimit.Maximum = 300;
            nudPointLimit.Value = 100;
            btnCreateRoom.AutoSize = true;
            btnCreateRoom.Text = "Create";
            // 
            // gameGroup
            // 
            gameGroup.Dock = DockStyle.Fill;
            gameGroup.Text = "Game Table";
            gameGroup.Controls.Add(gameLayout);
            mainLayout.Controls.Add(gameGroup, 1, 0);
            mainLayout.SetRowSpan(gameGroup, 3);
            // 
            // gameLayout
            // 
            gameLayout.ColumnCount = 2;
            gameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            gameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            gameLayout.Dock = DockStyle.Fill;
            gameLayout.RowCount = 4;
            gameLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            gameLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            gameLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
            gameLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            gameLayout.Controls.Add(lblCurrentTurn, 0, 0);
            gameLayout.SetColumnSpan(lblCurrentTurn, 2);
            gameLayout.Controls.Add(boardPanel, 0, 1);
            gameLayout.Controls.Add(handPanel, 0, 2);
            gameLayout.Controls.Add(actionPanel, 0, 3);
            gameLayout.Controls.Add(lstPlayers, 1, 1);
            gameLayout.SetRowSpan(lstPlayers, 3);
            lblCurrentTurn.Dock = DockStyle.Fill;
            lblCurrentTurn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCurrentTurn.Text = "Turn: -";
            lblCurrentTurn.TextAlign = ContentAlignment.MiddleLeft;
            boardPanel.Dock = DockStyle.Fill;
            boardPanel.AutoScroll = true;
            boardPanel.BorderStyle = BorderStyle.FixedSingle;
            handPanel.Dock = DockStyle.Fill;
            handPanel.AutoScroll = true;
            handPanel.BorderStyle = BorderStyle.FixedSingle;
            actionPanel.Dock = DockStyle.Fill;
            actionPanel.Controls.AddRange(new Control[] { btnPlaySelected, btnDraw, btnPass });
            btnPlaySelected.Text = "Play Selected";
            btnDraw.Text = "Draw";
            btnPass.Text = "Pass";
            lstPlayers.Dock = DockStyle.Fill;
            lstPlayers.View = View.Details;
            lstPlayers.Columns.Add("Player", 120);
            lstPlayers.Columns.Add("Status", 80);
            lstPlayers.Columns.Add("Points", 60);
            lstPlayers.FullRowSelect = true;
            // 
            // statusStrip
            // 
            statusStrip.Items.Add(statusLabel);
            statusStrip.Dock = DockStyle.Bottom;
            statusLabel.Text = "Ready";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1300, 760);
            Controls.Add(mainLayout);
            Controls.Add(statusStrip);
            MinimumSize = new Size(1200, 700);
            Name = "Form1";
            Text = "Domino Client";
            ((System.ComponentModel.ISupportInitialize)roomsGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMaxPlayers).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPointLimit).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudServerPort).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox lobbyGroup;
        private DataGridView roomsGrid;
        private Button btnRefreshRooms;
        private Button btnJoinRoom;
        private Button btnWatchRoom;
        private GroupBox creationGroup;
        private TextBox txtRoomName;
        private NumericUpDown nudMaxPlayers;
        private NumericUpDown nudPointLimit;
        private Button btnCreateRoom;
        private GroupBox loginGroup;
        private TextBox txtPlayerName;
        private TextBox txtServerHost;
        private NumericUpDown nudServerPort;
        private Button btnConnect;
        private GroupBox gameGroup;
        private Label lblCurrentTurn;
        private FlowLayoutPanel boardPanel;
        private FlowLayoutPanel handPanel;
        private Button btnPlaySelected;
        private Button btnDraw;
        private Button btnPass;
        private ListView lstPlayers;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private TableLayoutPanel roomsLayout;
        private FlowLayoutPanel roomActionsPanel;
        private TableLayoutPanel createLayout;
        private TableLayoutPanel loginLayout;
        private TableLayoutPanel gameLayout;
        private FlowLayoutPanel actionPanel;
    }
}

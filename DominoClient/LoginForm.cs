using System.Text.Json;
using DominoClient.Networking;
using DominoShared.DTOs;

namespace DominoClient;

public class LoginForm : Form
{
    private readonly TableLayoutPanel _mainLayout;
    private readonly TextBox _txtUsername;
    private readonly TextBox _txtServerIP;
    private readonly TextBox _txtPort;
    private readonly Button _btnConnect;
    private readonly Label _lblStatus;

    private ClientManager? _clientManager;
    private TaskCompletionSource<bool>? _loginTcs;

    public LoginForm()
    {
        InitializeComponent();
        _mainLayout = new TableLayoutPanel();
        _txtUsername = new TextBox();
        _txtServerIP = new TextBox();
        _txtPort = new TextBox();
        _btnConnect = new Button();
        _lblStatus = new Label();
        
        SetupLayout();
    }

    private void InitializeComponent()
    {
        Text = "Domino Game - Login";
        Size = new Size(400, 300);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        FormClosing += LoginForm_FormClosing;
    }

    private void SetupLayout()
    {
        _mainLayout.Dock = DockStyle.Fill;
        _mainLayout.Padding = new Padding(20);
        _mainLayout.ColumnCount = 2;
        _mainLayout.RowCount = 5;
        _mainLayout.AutoSize = true;

        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

        var lblUsername = new Label
        {
            Text = "Username:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false
        };

        _txtUsername.Dock = DockStyle.Fill;
        _txtUsername.Font = new Font(_txtUsername.Font.FontFamily, 10F);

        var lblServerIP = new Label
        {
            Text = "Server IP:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false
        };

        _txtServerIP.Text = "127.0.0.1";
        _txtServerIP.Dock = DockStyle.Fill;
        _txtServerIP.Font = new Font(_txtServerIP.Font.FontFamily, 10F);

        var lblPort = new Label
        {
            Text = "Port:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false
        };

        _txtPort.Text = "5000";
        _txtPort.Dock = DockStyle.Fill;
        _txtPort.Font = new Font(_txtPort.Font.FontFamily, 10F);

        _btnConnect.Text = "Connect";
        _btnConnect.Dock = DockStyle.Fill;
        _btnConnect.Font = new Font(_btnConnect.Font.FontFamily, 10F, FontStyle.Bold);
        _btnConnect.Height = 40;
        _btnConnect.Click += BtnConnect_Click;

        _lblStatus.Text = "Ready to connect";
        _lblStatus.Dock = DockStyle.Fill;
        _lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        _lblStatus.ForeColor = Color.Gray;
        _lblStatus.AutoSize = false;

        _mainLayout.Controls.Add(lblUsername, 0, 0);
        _mainLayout.Controls.Add(_txtUsername, 1, 0);
        _mainLayout.Controls.Add(lblServerIP, 0, 1);
        _mainLayout.Controls.Add(_txtServerIP, 1, 1);
        _mainLayout.Controls.Add(lblPort, 0, 2);
        _mainLayout.Controls.Add(_txtPort, 1, 2);
        _mainLayout.SetColumnSpan(_btnConnect, 2);
        _mainLayout.Controls.Add(_btnConnect, 0, 3);
        _mainLayout.SetColumnSpan(_lblStatus, 2);
        _mainLayout.Controls.Add(_lblStatus, 0, 4);

        Controls.Add(_mainLayout);
    }

    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        var username = _txtUsername.Text.Trim();
        var serverIP = _txtServerIP.Text.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            UpdateStatus("Please enter a username", Color.Red);
            return;
        }

        if (string.IsNullOrWhiteSpace(serverIP))
        {
            UpdateStatus("Please enter server IP", Color.Red);
            return;
        }

        if (!int.TryParse(_txtPort.Text, out int port) || port <= 0 || port > 65535)
        {
            UpdateStatus("Please enter a valid port (1-65535)", Color.Red);
            return;
        }

        UpdateStatus("Connecting to server...", Color.Blue);
        _btnConnect.Enabled = false;

        try
        {
            _clientManager = new ClientManager();
            _clientManager.OnMessageReceived += OnMessageReceived;
            _clientManager.OnDisconnected += OnDisconnected;

            await _clientManager.ConnectAsync(serverIP, port);
            UpdateStatus("Sending login request...", Color.Blue);

            _loginTcs = new TaskCompletionSource<bool>();

            var loginMessage = new NetworkMessage
            {
                Action = "LOGIN",
                Username = username,
                Timestamp = DateTime.UtcNow
            };

            await _clientManager.SendAsync(JsonSerializer.Serialize(loginMessage));

            var loginSuccess = await _loginTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

            if (loginSuccess)
            {
                UpdateStatus("Login successful!", Color.Green);
                await Task.Delay(500);
                
                Hide();
                var lobbyForm = new LobbyForm(_clientManager, username);
                lobbyForm.FormClosed += (s, args) => Close();
                lobbyForm.Show();
            }
            else
            {
                UpdateStatus("Login failed", Color.Red);
                _btnConnect.Enabled = true;
                _clientManager?.Disconnect();
            }
        }
        catch (TimeoutException)
        {
            UpdateStatus("Login timeout - no response from server", Color.Red);
            _btnConnect.Enabled = true;
            _clientManager?.Disconnect();
        }
        catch (Exception ex)
        {
            UpdateStatus($"Connection failed: {ex.Message}", Color.Red);
            _btnConnect.Enabled = true;
            _clientManager?.Disconnect();
        }
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

            if (netMsg.Action == "PLAYER_LIST" && netMsg.Success)
            {
                _loginTcs?.TrySetResult(true);
            }
            else if (!netMsg.Success)
            {
                UpdateStatus(netMsg.ErrorMessage ?? "Login failed", Color.Red);
                _loginTcs?.TrySetResult(false);
            }
        }
        catch (JsonException)
        {
        }
    }

    private void OnDisconnected()
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => OnDisconnected());
            return;
        }

        if (_loginTcs != null && !_loginTcs.Task.IsCompleted)
        {
            UpdateStatus("Disconnected from server", Color.Red);
            _loginTcs.TrySetResult(false);
        }

        _btnConnect.Enabled = true;
    }

    private void LoginForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _clientManager?.Disconnect();
    }

    private void UpdateStatus(string message, Color color)
    {
        _lblStatus.Text = message;
        _lblStatus.ForeColor = color;
    }

    public string Username => _txtUsername.Text.Trim();
    public string ServerIP => _txtServerIP.Text.Trim();
    public int Port => int.TryParse(_txtPort.Text, out int port) ? port : 5000;
}

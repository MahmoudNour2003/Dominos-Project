namespace DominoServer
{
    public partial class Form1 : Form
    {
        private ServerManager? _server;

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPort.Text, out var port) || port <= 0)
            {
                MessageBox.Show("Invalid port.");
                return;
            }

            var validator = new PassThroughMoveValidator();
            var gameManager = new GameManager(validator);
            _server = new ServerManager(gameManager);
            _server.Log += OnServerLog;

            btnStart.Enabled = false;
            btnStop.Enabled = true;

            await Task.Run(() => _server.StartAsync(port));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }

        private void StopServer()
        {
            if (_server is null)
            {
                return;
            }

            _server.Stop();
            _server.Log -= OnServerLog;
            _server = null;

            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void OnServerLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnServerLog(message));
                return;
            }

            txtLogs.AppendText(message + Environment.NewLine);
        }
    }
}

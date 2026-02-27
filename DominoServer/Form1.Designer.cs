namespace DominoServer
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
            txtLogs = new TextBox();
            btnStart = new Button();
            btnStop = new Button();
            txtPort = new TextBox();
            lblPort = new Label();
            SuspendLayout();
            // 
            // txtLogs
            // 
            txtLogs.Location = new Point(12, 51);
            txtLogs.Multiline = true;
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.ScrollBars = ScrollBars.Vertical;
            txtLogs.Size = new Size(776, 387);
            txtLogs.TabIndex = 0;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(215, 12);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(94, 29);
            btnStart.TabIndex = 1;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(315, 12);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(94, 29);
            btnStop.TabIndex = 2;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(56, 14);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(141, 27);
            txtPort.TabIndex = 3;
            txtPort.Text = "5000";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(12, 17);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(38, 20);
            lblPort.TabIndex = 4;
            lblPort.Text = "Port";
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblPort);
            Controls.Add(txtPort);
            Controls.Add(btnStop);
            Controls.Add(btnStart);
            Controls.Add(txtLogs);
            Name = "Form1";
            Text = "Domino Server";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLogs;
        private Button btnStart;
        private Button btnStop;
        private TextBox txtPort;
        private Label lblPort;
    }
}

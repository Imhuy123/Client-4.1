namespace Client_4._1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtServerDNS;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox rtbMessages;
        private System.Windows.Forms.Label lblConnectionStatus;





        private void InitializeComponent()
        {
            txtUserName = new TextBox();
            txtServerDNS = new TextBox();
            txtPort = new TextBox();
            lstUsers = new ListBox();
            btnConnect = new Button();
            rtbMessages = new RichTextBox();
            lstMessagedUsers = new ListBox();
            SuspendLayout();
            // 
            // txtUserName
            // 
            txtUserName.Location = new Point(17, 18);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderText = "Enter your username";
            txtUserName.Size = new Size(150, 27);
            txtUserName.TabIndex = 0;
            // 
            // txtServerDNS
            // 
            txtServerDNS.Location = new Point(17, 70);
            txtServerDNS.Name = "txtServerDNS";
            txtServerDNS.Size = new Size(150, 27);
            txtServerDNS.TabIndex = 1;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(17, 114);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(150, 27);
            txtPort.TabIndex = 2;
            // 
            // lstUsers
            // 
            lstUsers.Location = new Point(232, 182);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(155, 244);
            lstUsers.TabIndex = 3;
            lstUsers.DoubleClick += lstUsers_DoubleClick;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(249, 18);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(101, 85);
            btnConnect.TabIndex = 5;
            btnConnect.Text = "Connect";
            btnConnect.Click += btnConnect_Click;
            // 
            // rtbMessages
            // 
            rtbMessages.Location = new Point(58, 457);
            rtbMessages.Name = "rtbMessages";
            rtbMessages.Size = new Size(292, 69);
            rtbMessages.TabIndex = 10;
            rtbMessages.Text = "";
            // 
            // lstMessagedUsers
            // 
            lstMessagedUsers.Location = new Point(12, 182);
            lstMessagedUsers.Name = "lstMessagedUsers";
            lstMessagedUsers.Size = new Size(155, 244);
            lstMessagedUsers.TabIndex = 9;
            lstMessagedUsers.SelectedIndexChanged += lstMessagedUsers_SelectedIndexChanged;
            // 
            // Form1
            // 
            ClientSize = new Size(419, 583);
            Controls.Add(lstMessagedUsers);
            Controls.Add(txtUserName);
            Controls.Add(txtServerDNS);
            Controls.Add(txtPort);
            Controls.Add(lstUsers);
            Controls.Add(btnConnect);
            Controls.Add(rtbMessages);
            Name = "Form1";
            Text = "Socket Client GUI";
            ResumeLayout(false);
            PerformLayout();
        }

        private ListBox lstMessagedUsers;

    }
}

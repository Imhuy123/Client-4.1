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
        private System.Windows.Forms.Label lblStatus;

        private void InitializeComponent()
        {
            txtUserName = new TextBox();
            txtServerDNS = new TextBox();
            txtPort = new TextBox();
            lstUsers = new ListBox();
            txtMessage = new TextBox();
            btnConnect = new Button();
            btnSend = new Button();
            rtbMessages = new RichTextBox();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // txtUserName
            // 
            txtUserName.Location = new Point(12, 12);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderText = "Enter your username";
            txtUserName.Size = new Size(150, 23);
            txtUserName.TabIndex = 0;
            // 
            // txtServerDNS
            // 
            txtServerDNS.Location = new Point(12, 38);
            txtServerDNS.Name = "txtServerDNS";
            txtServerDNS.Size = new Size(150, 23);
            txtServerDNS.TabIndex = 1;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(12, 64);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(150, 23);
            txtPort.TabIndex = 2;
            // 
            // lstUsers
            // 
            lstUsers.ItemHeight = 15;
            lstUsers.Location = new Point(12, 100);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(150, 199);
            lstUsers.TabIndex = 3;
            lstUsers.DoubleClick += lstUsers_DoubleClick;
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(180, 100);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(150, 23);
            txtMessage.TabIndex = 4;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(180, 12);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(75, 23);
            btnConnect.TabIndex = 5;
            btnConnect.Text = "Connect";
            btnConnect.Click += btnConnect_Click;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(180, 130);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 6;
            btnSend.Text = "Send";
            // 
            // rtbMessages
            // 
            rtbMessages.Location = new Point(180, 160);
            rtbMessages.Name = "rtbMessages";
            rtbMessages.Size = new Size(300, 140);
            rtbMessages.TabIndex = 7;
            rtbMessages.Text = "";
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(12, 320);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(300, 23);
            lblStatus.TabIndex = 8;
            // 
            // Form1
            // 
            ClientSize = new Size(500, 350);
            Controls.Add(txtUserName);
            Controls.Add(txtServerDNS);
            Controls.Add(txtPort);
            Controls.Add(lstUsers);
            Controls.Add(txtMessage);
            Controls.Add(btnConnect);
            Controls.Add(btnSend);
            Controls.Add(rtbMessages);
            Controls.Add(lblStatus);
            Name = "Form1";
            Text = "Socket Client GUI";
            ResumeLayout(false);
            PerformLayout();
            // btb label 
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatus.Location = new System.Drawing.Point(12, 120);
            this.lblStatus.Size = new System.Drawing.Size(200, 23);
            this.lblStatus.Text = "Disconnected";
            this.Controls.Add(this.lblStatus);
        }
    }
}

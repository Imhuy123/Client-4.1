
namespace Client_4._1
{
    partial class ChatForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RichTextBox rtbChatHistory;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            rtbChatHistory = new RichTextBox();
            txtMessage = new TextBox();
            btnSend = new Button();
            rtbLog = new RichTextBox();
            btnVideoCall = new Button();
            SuspendLayout();
            // 
            // rtbChatHistory
            // 
            rtbChatHistory.Location = new Point(13, 79);
            rtbChatHistory.Margin = new Padding(4, 5, 4, 5);
            rtbChatHistory.Name = "rtbChatHistory";
            rtbChatHistory.ReadOnly = true;
            rtbChatHistory.Size = new Size(492, 195);
            rtbChatHistory.TabIndex = 0;
            rtbChatHistory.Text = "";
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(13, 445);
            txtMessage.Margin = new Padding(4, 5, 4, 5);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(372, 27);
            txtMessage.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(400, 445);
            btnSend.Margin = new Padding(4, 5, 4, 5);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(100, 35);
            btnSend.TabIndex = 2;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // rtbLog
            // 
            rtbLog.Location = new Point(21, 296);
            rtbLog.Name = "rtbLog";
            rtbLog.Size = new Size(484, 113);
            rtbLog.TabIndex = 3;
            rtbLog.Text = "";
            // 
            // btnVideoCall
            // 
            btnVideoCall.Location = new Point(374, 22);
            btnVideoCall.Name = "btnVideoCall";
            btnVideoCall.Size = new Size(126, 29);
            btnVideoCall.TabIndex = 4;
            btnVideoCall.Text = "CALL";
            btnVideoCall.UseVisualStyleBackColor = true;
            btnVideoCall.Click += viewCall_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(527, 501);
            Controls.Add(btnVideoCall);
            Controls.Add(rtbLog);
            Controls.Add(btnSend);
            Controls.Add(txtMessage);
            Controls.Add(rtbChatHistory);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ChatForm";
            Text = "Chat";
            ResumeLayout(false);
            PerformLayout();
        }

        private void rtbChatHistory_TextChanged(object sender, EventArgs e)
        {
            // Tự động cuộn xuống cuối khi có tin nhắn mới
            rtbChatHistory.SelectionStart = rtbChatHistory.Text.Length;
            rtbChatHistory.ScrollToCaret();
        }


        private RichTextBox rtbLog;
        private Button btnVideoCall;
    }
}

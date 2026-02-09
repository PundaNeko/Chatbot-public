namespace Simple_Chatbot_Remake
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
            richTextBox1 = new RichTextBox();
            textBox1 = new TextBox();
            button1 = new Button();
            RecordButton = new Button();
            BotReplayButton = new Button();
            InputReplayButton = new Button();
            DebugText = new RichTextBox();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 12);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(963, 534);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 552);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(963, 49);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox1.KeyDown += textBox1_KeyDown;
            // 
            // button1
            // 
            button1.Location = new Point(981, 552);
            button1.Name = "button1";
            button1.Size = new Size(127, 49);
            button1.TabIndex = 2;
            button1.Text = "Send Input";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            button1.KeyDown += button1_KeyDown;
            // 
            // RecordButton
            // 
            RecordButton.Location = new Point(1114, 552);
            RecordButton.Name = "RecordButton";
            RecordButton.Size = new Size(52, 49);
            RecordButton.TabIndex = 3;
            RecordButton.Text = "Record Voice";
            RecordButton.UseVisualStyleBackColor = true;
            RecordButton.Click += button2_Click;
            // 
            // BotReplayButton
            // 
            BotReplayButton.Location = new Point(981, 469);
            BotReplayButton.Name = "BotReplayButton";
            BotReplayButton.Size = new Size(75, 77);
            BotReplayButton.TabIndex = 4;
            BotReplayButton.Text = "Tell the bot to speak";
            BotReplayButton.UseVisualStyleBackColor = true;
            BotReplayButton.Click += button2_Click_1;
            // 
            // InputReplayButton
            // 
            InputReplayButton.Location = new Point(981, 386);
            InputReplayButton.Name = "InputReplayButton";
            InputReplayButton.Size = new Size(75, 77);
            InputReplayButton.TabIndex = 5;
            InputReplayButton.Text = "Listen to yourself";
            InputReplayButton.UseVisualStyleBackColor = true;
            // 
            // DebugText
            // 
            DebugText.Location = new Point(981, 12);
            DebugText.Name = "DebugText";
            DebugText.Size = new Size(710, 368);
            DebugText.TabIndex = 6;
            DebugText.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1703, 624);
            Controls.Add(DebugText);
            Controls.Add(InputReplayButton);
            Controls.Add(BotReplayButton);
            Controls.Add(RecordButton);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(richTextBox1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBox1;
        private TextBox textBox1;
        private Button button1;
        private Button RecordButton;
        private Button BotReplayButton;
        private Button InputReplayButton;
        private RichTextBox DebugText;
    }
}

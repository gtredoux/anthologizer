namespace AnthologizerClient
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.connectBtn = new System.Windows.Forms.Button();
            this.myDataGrid = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.myStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.myNextBtn = new System.Windows.Forms.Button();
            this.myStopBtn = new System.Windows.Forms.Button();
            this.myBackBtn = new System.Windows.Forms.Button();
            this.myServerLabel = new System.Windows.Forms.Label();
            this.myAnthologyLabel = new System.Windows.Forms.Label();
            this.myAnthologyName = new System.Windows.Forms.TextBox();
            this.myURL = new System.Windows.Forms.TextBox();
            this.myAnthologySection = new System.Windows.Forms.ComboBox();
            this.myAnthologyCount = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.playBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.myDataGrid)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // connectBtn
            // 
            this.connectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.connectBtn.Location = new System.Drawing.Point(546, 46);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(87, 26);
            this.connectBtn.TabIndex = 3;
            this.connectBtn.Text = "connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // myDataGrid
            // 
            this.myDataGrid.AllowUserToAddRows = false;
            this.myDataGrid.AllowUserToDeleteRows = false;
            this.myDataGrid.AllowUserToResizeRows = false;
            this.myDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.myDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.myDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.myDataGrid.Location = new System.Drawing.Point(13, 81);
            this.myDataGrid.Name = "myDataGrid";
            this.myDataGrid.RowTemplate.Height = 24;
            this.myDataGrid.Size = new System.Drawing.Size(620, 500);
            this.myDataGrid.TabIndex = 4;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.myStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 599);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(645, 22);
            this.statusStrip1.TabIndex = 5;
            // 
            // myStatus
            // 
            this.myStatus.Name = "myStatus";
            this.myStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // myNextBtn
            // 
            this.myNextBtn.BackgroundImage = global::AnthologizerClient.Properties.Resources.ios7_fastforward;
            this.myNextBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.myNextBtn.Location = new System.Drawing.Point(214, 12);
            this.myNextBtn.Name = "myNextBtn";
            this.myNextBtn.Size = new System.Drawing.Size(42, 30);
            this.myNextBtn.TabIndex = 8;
            this.myNextBtn.UseVisualStyleBackColor = true;
            this.myNextBtn.Click += new System.EventHandler(this.myNextBtn_Click);
            // 
            // myStopBtn
            // 
            this.myStopBtn.BackgroundImage = global::AnthologizerClient.Properties.Resources.stop;
            this.myStopBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.myStopBtn.Location = new System.Drawing.Point(175, 12);
            this.myStopBtn.Name = "myStopBtn";
            this.myStopBtn.Size = new System.Drawing.Size(33, 30);
            this.myStopBtn.TabIndex = 7;
            this.myStopBtn.UseVisualStyleBackColor = true;
            this.myStopBtn.Click += new System.EventHandler(this.myStopBtn_Click);
            // 
            // myBackBtn
            // 
            this.myBackBtn.BackgroundImage = global::AnthologizerClient.Properties.Resources.arrow_up_c;
            this.myBackBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.myBackBtn.Location = new System.Drawing.Point(69, 12);
            this.myBackBtn.Name = "myBackBtn";
            this.myBackBtn.Size = new System.Drawing.Size(50, 30);
            this.myBackBtn.TabIndex = 6;
            this.myBackBtn.UseVisualStyleBackColor = true;
            this.myBackBtn.Click += new System.EventHandler(this.myBackBtn_Click);
            // 
            // myServerLabel
            // 
            this.myServerLabel.AutoSize = true;
            this.myServerLabel.Location = new System.Drawing.Point(13, 51);
            this.myServerLabel.Name = "myServerLabel";
            this.myServerLabel.Size = new System.Drawing.Size(50, 17);
            this.myServerLabel.TabIndex = 10;
            this.myServerLabel.Text = "Server";
            // 
            // myAnthologyLabel
            // 
            this.myAnthologyLabel.AutoSize = true;
            this.myAnthologyLabel.Location = new System.Drawing.Point(260, 17);
            this.myAnthologyLabel.Name = "myAnthologyLabel";
            this.myAnthologyLabel.Size = new System.Drawing.Size(71, 17);
            this.myAnthologyLabel.TabIndex = 12;
            this.myAnthologyLabel.Text = "Anthology";
            // 
            // myAnthologyName
            // 
            this.myAnthologyName.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AnthologizerClient.Properties.Settings.Default, "AnthologyName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.myAnthologyName.Location = new System.Drawing.Point(464, 14);
            this.myAnthologyName.Name = "myAnthologyName";
            this.myAnthologyName.Size = new System.Drawing.Size(147, 22);
            this.myAnthologyName.TabIndex = 11;
            this.myAnthologyName.Text = global::AnthologizerClient.Properties.Settings.Default.AnthologyName;
            this.myAnthologyName.TextChanged += new System.EventHandler(this.myAnthologyName_TextChanged);
            // 
            // myURL
            // 
            this.myURL.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::AnthologizerClient.Properties.Settings.Default, "url", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.myURL.Location = new System.Drawing.Point(69, 48);
            this.myURL.Name = "myURL";
            this.myURL.Size = new System.Drawing.Size(471, 22);
            this.myURL.TabIndex = 1;
            this.myURL.Text = global::AnthologizerClient.Properties.Settings.Default.url;
            this.myURL.TextChanged += new System.EventHandler(this.myURL_TextChanged);
            // 
            // myAnthologySection
            // 
            this.myAnthologySection.FormattingEnabled = true;
            this.myAnthologySection.Items.AddRange(new object[] {
            "assorted",
            "by-artist",
            "by-theme"});
            this.myAnthologySection.Location = new System.Drawing.Point(337, 14);
            this.myAnthologySection.Name = "myAnthologySection";
            this.myAnthologySection.Size = new System.Drawing.Size(121, 24);
            this.myAnthologySection.TabIndex = 13;
            this.myAnthologySection.SelectedIndexChanged += new System.EventHandler(this.myAnthologySection_SelectedIndexChanged);
            // 
            // myAnthologyCount
            // 
            this.myAnthologyCount.AutoSize = true;
            this.myAnthologyCount.Location = new System.Drawing.Point(617, 17);
            this.myAnthologyCount.Name = "myAnthologyCount";
            this.myAnthologyCount.Size = new System.Drawing.Size(16, 17);
            this.myAnthologyCount.TabIndex = 14;
            this.myAnthologyCount.Text = "0";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AnthologizerClient.Properties.Resources.exit;
            this.pictureBox1.Location = new System.Drawing.Point(5, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(51, 30);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 15;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.powerBtn_Click);
            // 
            // playBtn
            // 
            this.playBtn.BackgroundImage = global::AnthologizerClient.Properties.Resources.play;
            this.playBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.playBtn.Location = new System.Drawing.Point(136, 12);
            this.playBtn.Name = "playBtn";
            this.playBtn.Size = new System.Drawing.Size(33, 30);
            this.playBtn.TabIndex = 16;
            this.playBtn.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(645, 621);
            this.Controls.Add(this.playBtn);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.myAnthologyCount);
            this.Controls.Add(this.myAnthologySection);
            this.Controls.Add(this.myAnthologyLabel);
            this.Controls.Add(this.myAnthologyName);
            this.Controls.Add(this.myServerLabel);
            this.Controls.Add(this.myNextBtn);
            this.Controls.Add(this.myStopBtn);
            this.Controls.Add(this.myBackBtn);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.myDataGrid);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.myURL);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Anthologizer";
            ((System.ComponentModel.ISupportInitialize)(this.myDataGrid)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox myURL;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.DataGridView myDataGrid;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel myStatus;
        private System.Windows.Forms.Button myBackBtn;
        private System.Windows.Forms.Button myNextBtn;
        private System.Windows.Forms.Button myStopBtn;
        private System.Windows.Forms.Label myServerLabel;
        private System.Windows.Forms.TextBox myAnthologyName;
        private System.Windows.Forms.Label myAnthologyLabel;
        private System.Windows.Forms.ComboBox myAnthologySection;
        private System.Windows.Forms.Label myAnthologyCount;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button playBtn;
    }
}


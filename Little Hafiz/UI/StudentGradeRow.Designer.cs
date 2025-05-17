namespace Little_Hafiz
{
    partial class StudentGradeRow
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.prevLevel = new System.Windows.Forms.Label();
            this.currentLevel = new System.Windows.Forms.Label();
            this.compDate = new System.Windows.Forms.Label();
            this.scoreLabel = new System.Windows.Forms.Label();
            this.rankLabel = new System.Windows.Forms.Label();
            this.stdScore = new Guna.UI2.WinForms.Guna2NumericUpDown();
            this.stdRank = new Guna.UI2.WinForms.Guna2NumericUpDown();
            this.stdCode = new System.Windows.Forms.Label();
            this.saveBtn = new Guna.UI2.WinForms.Guna2CircleButton();
            this.deleteBtn = new Guna.UI2.WinForms.Guna2CircleButton();
            ((System.ComponentModel.ISupportInitialize)(this.stdScore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stdRank)).BeginInit();
            this.SuspendLayout();
            // 
            // prevLevel
            // 
            this.prevLevel.BackColor = System.Drawing.Color.Transparent;
            this.prevLevel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.prevLevel.Location = new System.Drawing.Point(617, 0);
            this.prevLevel.Name = "prevLevel";
            this.prevLevel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.prevLevel.Size = new System.Drawing.Size(100, 60);
            this.prevLevel.TabIndex = 2;
            this.prevLevel.Text = "المستوى السابق";
            this.prevLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // currentLevel
            // 
            this.currentLevel.BackColor = System.Drawing.Color.Transparent;
            this.currentLevel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.currentLevel.Location = new System.Drawing.Point(511, 0);
            this.currentLevel.Name = "currentLevel";
            this.currentLevel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.currentLevel.Size = new System.Drawing.Size(100, 60);
            this.currentLevel.TabIndex = 3;
            this.currentLevel.Text = "المستوى الحالي";
            this.currentLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // compDate
            // 
            this.compDate.BackColor = System.Drawing.Color.Transparent;
            this.compDate.Font = new System.Drawing.Font("Tahoma", 12F);
            this.compDate.Location = new System.Drawing.Point(326, 0);
            this.compDate.Name = "compDate";
            this.compDate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.compDate.Size = new System.Drawing.Size(179, 60);
            this.compDate.TabIndex = 4;
            this.compDate.Text = "تاريخ المسابقة";
            this.compDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.compDate.DoubleClick += new System.EventHandler(this.CompDate_DoubleClick);
            // 
            // scoreLabel
            // 
            this.scoreLabel.BackColor = System.Drawing.Color.Transparent;
            this.scoreLabel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.scoreLabel.Location = new System.Drawing.Point(197, 0);
            this.scoreLabel.Name = "scoreLabel";
            this.scoreLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.scoreLabel.Size = new System.Drawing.Size(123, 60);
            this.scoreLabel.TabIndex = 5;
            this.scoreLabel.Text = "الدرجة";
            this.scoreLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.scoreLabel.Visible = false;
            // 
            // rankLabel
            // 
            this.rankLabel.BackColor = System.Drawing.Color.Transparent;
            this.rankLabel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.rankLabel.Location = new System.Drawing.Point(69, 0);
            this.rankLabel.Name = "rankLabel";
            this.rankLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.rankLabel.Size = new System.Drawing.Size(120, 60);
            this.rankLabel.TabIndex = 6;
            this.rankLabel.Text = "المركز";
            this.rankLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rankLabel.Visible = false;
            // 
            // stdScore
            // 
            this.stdScore.BackColor = System.Drawing.Color.Transparent;
            this.stdScore.BorderRadius = 3;
            this.stdScore.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.stdScore.DecimalPlaces = 2;
            this.stdScore.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.stdScore.Location = new System.Drawing.Point(203, 9);
            this.stdScore.Name = "stdScore";
            this.stdScore.Size = new System.Drawing.Size(111, 43);
            this.stdScore.TabIndex = 49;
            this.stdScore.UpDownButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            // 
            // stdRank
            // 
            this.stdRank.BackColor = System.Drawing.Color.Transparent;
            this.stdRank.BorderRadius = 3;
            this.stdRank.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.stdRank.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.stdRank.Location = new System.Drawing.Point(74, 9);
            this.stdRank.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.stdRank.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.stdRank.Name = "stdRank";
            this.stdRank.Size = new System.Drawing.Size(109, 43);
            this.stdRank.TabIndex = 50;
            this.stdRank.UpDownButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.stdRank.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // stdCode
            // 
            this.stdCode.BackColor = System.Drawing.Color.Transparent;
            this.stdCode.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdCode.Location = new System.Drawing.Point(723, 0);
            this.stdCode.Name = "stdCode";
            this.stdCode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdCode.Size = new System.Drawing.Size(94, 60);
            this.stdCode.TabIndex = 51;
            this.stdCode.Text = "الكود";
            this.stdCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // saveBtn
            // 
            this.saveBtn.BackColor = System.Drawing.Color.Transparent;
            this.saveBtn.BackgroundImage = global::Little_Hafiz.Properties.Resources.check_mark_button;
            this.saveBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.saveBtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.saveBtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.saveBtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.saveBtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.saveBtn.FillColor = System.Drawing.Color.Transparent;
            this.saveBtn.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.saveBtn.ForeColor = System.Drawing.Color.White;
            this.saveBtn.Location = new System.Drawing.Point(38, 18);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.saveBtn.Size = new System.Drawing.Size(25, 25);
            this.saveBtn.TabIndex = 52;
            this.saveBtn.Visible = false;
            this.saveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
            // 
            // deleteBtn
            // 
            this.deleteBtn.BackColor = System.Drawing.Color.Transparent;
            this.deleteBtn.BackgroundImage = global::Little_Hafiz.Properties.Resources.no_entry;
            this.deleteBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.deleteBtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.deleteBtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.deleteBtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.deleteBtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.deleteBtn.FillColor = System.Drawing.Color.Transparent;
            this.deleteBtn.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.deleteBtn.ForeColor = System.Drawing.Color.White;
            this.deleteBtn.Location = new System.Drawing.Point(7, 18);
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.deleteBtn.Size = new System.Drawing.Size(25, 25);
            this.deleteBtn.TabIndex = 53;
            this.deleteBtn.Click += new System.EventHandler(this.DeleteBtn_Click);
            // 
            // StudentGradeRow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.deleteBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.stdCode);
            this.Controls.Add(this.stdRank);
            this.Controls.Add(this.stdScore);
            this.Controls.Add(this.rankLabel);
            this.Controls.Add(this.scoreLabel);
            this.Controls.Add(this.compDate);
            this.Controls.Add(this.currentLevel);
            this.Controls.Add(this.prevLevel);
            this.Name = "StudentGradeRow";
            this.Size = new System.Drawing.Size(820, 60);
            ((System.ComponentModel.ISupportInitialize)(this.stdScore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stdRank)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label prevLevel;
        private System.Windows.Forms.Label currentLevel;
        private System.Windows.Forms.Label compDate;
        private System.Windows.Forms.Label scoreLabel;
        private System.Windows.Forms.Label rankLabel;
        private Guna.UI2.WinForms.Guna2NumericUpDown stdScore;
        private Guna.UI2.WinForms.Guna2NumericUpDown stdRank;
        private System.Windows.Forms.Label stdCode;
        private Guna.UI2.WinForms.Guna2CircleButton saveBtn;
        private Guna.UI2.WinForms.Guna2CircleButton deleteBtn;
    }
}

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
            this.prevLevel.Size = new System.Drawing.Size(100, 72);
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
            this.currentLevel.Size = new System.Drawing.Size(100, 72);
            this.currentLevel.TabIndex = 3;
            this.currentLevel.Text = "المستوى الحالي";
            this.currentLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // compDate
            // 
            this.compDate.BackColor = System.Drawing.Color.Transparent;
            this.compDate.Font = new System.Drawing.Font("Tahoma", 12F);
            this.compDate.Location = new System.Drawing.Point(315, 0);
            this.compDate.Name = "compDate";
            this.compDate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.compDate.Size = new System.Drawing.Size(190, 72);
            this.compDate.TabIndex = 4;
            this.compDate.Text = "تاريخ المسابقة";
            this.compDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // scoreLabel
            // 
            this.scoreLabel.BackColor = System.Drawing.Color.Transparent;
            this.scoreLabel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.scoreLabel.Location = new System.Drawing.Point(159, 0);
            this.scoreLabel.Name = "scoreLabel";
            this.scoreLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.scoreLabel.Size = new System.Drawing.Size(150, 72);
            this.scoreLabel.TabIndex = 5;
            this.scoreLabel.Text = "الدرجة";
            this.scoreLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.scoreLabel.Visible = false;
            // 
            // rankLabel
            // 
            this.rankLabel.BackColor = System.Drawing.Color.Transparent;
            this.rankLabel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.rankLabel.Location = new System.Drawing.Point(3, 0);
            this.rankLabel.Name = "rankLabel";
            this.rankLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.rankLabel.Size = new System.Drawing.Size(150, 72);
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
            this.stdScore.Location = new System.Drawing.Point(171, 14);
            this.stdScore.Name = "stdScore";
            this.stdScore.Size = new System.Drawing.Size(128, 43);
            this.stdScore.TabIndex = 49;
            this.stdScore.UpDownButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(255)))), ((int)(((byte)(220)))));
            // 
            // stdRank
            // 
            this.stdRank.BackColor = System.Drawing.Color.Transparent;
            this.stdRank.BorderRadius = 3;
            this.stdRank.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.stdRank.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.stdRank.Location = new System.Drawing.Point(12, 14);
            this.stdRank.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.stdRank.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.stdRank.Name = "stdRank";
            this.stdRank.Size = new System.Drawing.Size(128, 43);
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
            this.stdCode.Size = new System.Drawing.Size(94, 72);
            this.stdCode.TabIndex = 51;
            this.stdCode.Text = "الكود";
            this.stdCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StudentGradeRow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.stdCode);
            this.Controls.Add(this.stdRank);
            this.Controls.Add(this.stdScore);
            this.Controls.Add(this.rankLabel);
            this.Controls.Add(this.scoreLabel);
            this.Controls.Add(this.compDate);
            this.Controls.Add(this.currentLevel);
            this.Controls.Add(this.prevLevel);
            this.Name = "StudentGradeRow";
            this.Size = new System.Drawing.Size(820, 72);
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
    }
}

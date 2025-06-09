namespace Little_Hafiz
{
    partial class StudentRankRow
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
            this.stdCode = new System.Windows.Forms.Label();
            this.stdName = new System.Windows.Forms.Label();
            this.StudentRank = new Guna.UI2.WinForms.Guna2NumericUpDown();
            this.stdScore = new System.Windows.Forms.Label();
            this.stdRankLabel = new System.Windows.Forms.Label();
            this.compDate = new System.Windows.Forms.Label();
            this.countLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.StudentRank)).BeginInit();
            this.SuspendLayout();
            // 
            // stdCode
            // 
            this.stdCode.BackColor = System.Drawing.Color.Transparent;
            this.stdCode.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdCode.Location = new System.Drawing.Point(713, 0);
            this.stdCode.Name = "stdCode";
            this.stdCode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdCode.Size = new System.Drawing.Size(94, 40);
            this.stdCode.TabIndex = 52;
            this.stdCode.Text = "الكود";
            this.stdCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stdName
            // 
            this.stdName.BackColor = System.Drawing.Color.Transparent;
            this.stdName.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdName.Location = new System.Drawing.Point(309, 0);
            this.stdName.Name = "stdName";
            this.stdName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdName.Size = new System.Drawing.Size(398, 40);
            this.stdName.TabIndex = 53;
            this.stdName.Text = "اسم الطالب";
            this.stdName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StudentRank
            // 
            this.StudentRank.BackColor = System.Drawing.Color.Transparent;
            this.StudentRank.BorderRadius = 3;
            this.StudentRank.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.StudentRank.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.StudentRank.Location = new System.Drawing.Point(3, 3);
            this.StudentRank.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.StudentRank.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.StudentRank.Name = "StudentRank";
            this.StudentRank.Size = new System.Drawing.Size(103, 34);
            this.StudentRank.TabIndex = 55;
            this.StudentRank.UpDownButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.StudentRank.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // stdScore
            // 
            this.stdScore.BackColor = System.Drawing.Color.Transparent;
            this.stdScore.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdScore.Location = new System.Drawing.Point(112, 0);
            this.stdScore.Name = "stdScore";
            this.stdScore.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdScore.Size = new System.Drawing.Size(85, 40);
            this.stdScore.TabIndex = 56;
            this.stdScore.Text = "الدرجة";
            this.stdScore.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stdRankLabel
            // 
            this.stdRankLabel.BackColor = System.Drawing.Color.Transparent;
            this.stdRankLabel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdRankLabel.Location = new System.Drawing.Point(3, 0);
            this.stdRankLabel.Name = "stdRankLabel";
            this.stdRankLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdRankLabel.Size = new System.Drawing.Size(103, 40);
            this.stdRankLabel.TabIndex = 57;
            this.stdRankLabel.Text = "المركز";
            this.stdRankLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.stdRankLabel.Visible = false;
            // 
            // compDate
            // 
            this.compDate.BackColor = System.Drawing.Color.Transparent;
            this.compDate.Font = new System.Drawing.Font("Tahoma", 12F);
            this.compDate.Location = new System.Drawing.Point(203, 0);
            this.compDate.Name = "compDate";
            this.compDate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.compDate.Size = new System.Drawing.Size(100, 40);
            this.compDate.TabIndex = 58;
            this.compDate.Text = "التاريخ";
            this.compDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // countLabel
            // 
            this.countLabel.BackColor = System.Drawing.Color.Transparent;
            this.countLabel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.countLabel.Location = new System.Drawing.Point(813, 0);
            this.countLabel.Name = "countLabel";
            this.countLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.countLabel.Size = new System.Drawing.Size(39, 40);
            this.countLabel.TabIndex = 59;
            this.countLabel.Text = "م";
            this.countLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StudentRankRow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.countLabel);
            this.Controls.Add(this.compDate);
            this.Controls.Add(this.stdScore);
            this.Controls.Add(this.StudentRank);
            this.Controls.Add(this.stdName);
            this.Controls.Add(this.stdCode);
            this.Controls.Add(this.stdRankLabel);
            this.Name = "StudentRankRow";
            this.Size = new System.Drawing.Size(855, 40);
            ((System.ComponentModel.ISupportInitialize)(this.StudentRank)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label stdCode;
        private System.Windows.Forms.Label stdName;
        private System.Windows.Forms.Label stdScore;
        private System.Windows.Forms.Label stdRankLabel;
        public Guna.UI2.WinForms.Guna2NumericUpDown StudentRank;
        private System.Windows.Forms.Label compDate;
        private System.Windows.Forms.Label countLabel;
    }
}

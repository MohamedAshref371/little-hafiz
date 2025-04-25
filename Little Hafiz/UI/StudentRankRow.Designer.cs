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
            this.stdRank = new Guna.UI2.WinForms.Guna2NumericUpDown();
            this.stdScore = new System.Windows.Forms.Label();
            this.stdRankLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.stdRank)).BeginInit();
            this.SuspendLayout();
            // 
            // stdCode
            // 
            this.stdCode.BackColor = System.Drawing.Color.Transparent;
            this.stdCode.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdCode.Location = new System.Drawing.Point(723, 0);
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
            this.stdName.Location = new System.Drawing.Point(241, 0);
            this.stdName.Name = "stdName";
            this.stdName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdName.Size = new System.Drawing.Size(476, 40);
            this.stdName.TabIndex = 53;
            this.stdName.Text = "إسم الطالب";
            this.stdName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stdRank
            // 
            this.stdRank.BackColor = System.Drawing.Color.Transparent;
            this.stdRank.BorderRadius = 3;
            this.stdRank.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.stdRank.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.stdRank.Location = new System.Drawing.Point(3, 3);
            this.stdRank.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.stdRank.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.stdRank.Name = "stdRank";
            this.stdRank.Size = new System.Drawing.Size(113, 34);
            this.stdRank.TabIndex = 55;
            this.stdRank.UpDownButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.stdRank.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // stdScore
            // 
            this.stdScore.BackColor = System.Drawing.Color.Transparent;
            this.stdScore.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdScore.Location = new System.Drawing.Point(122, 3);
            this.stdScore.Name = "stdScore";
            this.stdScore.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdScore.Size = new System.Drawing.Size(113, 34);
            this.stdScore.TabIndex = 56;
            this.stdScore.Text = "الدرجة";
            this.stdScore.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stdRankLabel
            // 
            this.stdRankLabel.BackColor = System.Drawing.Color.Transparent;
            this.stdRankLabel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdRankLabel.Location = new System.Drawing.Point(3, 3);
            this.stdRankLabel.Name = "stdRankLabel";
            this.stdRankLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdRankLabel.Size = new System.Drawing.Size(113, 34);
            this.stdRankLabel.TabIndex = 57;
            this.stdRankLabel.Text = "المركز";
            this.stdRankLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.stdRankLabel.Visible = false;
            // 
            // StudentRankRow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.stdScore);
            this.Controls.Add(this.stdRank);
            this.Controls.Add(this.stdName);
            this.Controls.Add(this.stdCode);
            this.Controls.Add(this.stdRankLabel);
            this.Name = "StudentRankRow";
            this.Size = new System.Drawing.Size(820, 40);
            ((System.ComponentModel.ISupportInitialize)(this.stdRank)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label stdCode;
        private System.Windows.Forms.Label stdName;
        private Guna.UI2.WinForms.Guna2NumericUpDown stdRank;
        private System.Windows.Forms.Label stdScore;
        private System.Windows.Forms.Label stdRankLabel;
    }
}

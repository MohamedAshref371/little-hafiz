namespace Little_Hafiz
{
    partial class StudentSearchRow
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
            this.stdName = new System.Windows.Forms.Label();
            this.compLevel = new System.Windows.Forms.Label();
            this.compDate = new System.Windows.Forms.Label();
            this.stdRank = new System.Windows.Forms.Label();
            this.studentBtn = new Guna.UI2.WinForms.Guna2Button();
            this.gradesBtn = new Guna.UI2.WinForms.Guna2Button();
            this.SuspendLayout();
            // 
            // stdName
            // 
            this.stdName.BackColor = System.Drawing.Color.Transparent;
            this.stdName.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdName.Location = new System.Drawing.Point(532, 0);
            this.stdName.Name = "stdName";
            this.stdName.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.stdName.Size = new System.Drawing.Size(365, 72);
            this.stdName.TabIndex = 0;
            this.stdName.Text = "إسم الطالب";
            this.stdName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // compLevel
            // 
            this.compLevel.BackColor = System.Drawing.Color.Transparent;
            this.compLevel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.compLevel.Location = new System.Drawing.Point(407, 0);
            this.compLevel.Name = "compLevel";
            this.compLevel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.compLevel.Size = new System.Drawing.Size(119, 72);
            this.compLevel.TabIndex = 1;
            this.compLevel.Text = "المستوى";
            this.compLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // compDate
            // 
            this.compDate.BackColor = System.Drawing.Color.Transparent;
            this.compDate.Font = new System.Drawing.Font("Tahoma", 12F);
            this.compDate.Location = new System.Drawing.Point(282, 0);
            this.compDate.Name = "compDate";
            this.compDate.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.compDate.Size = new System.Drawing.Size(119, 72);
            this.compDate.TabIndex = 2;
            this.compDate.Text = "تاريخ المسابقة";
            this.compDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stdRank
            // 
            this.stdRank.BackColor = System.Drawing.Color.Transparent;
            this.stdRank.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdRank.Location = new System.Drawing.Point(157, 0);
            this.stdRank.Name = "stdRank";
            this.stdRank.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.stdRank.Size = new System.Drawing.Size(119, 72);
            this.stdRank.TabIndex = 3;
            this.stdRank.Text = "مركزه";
            this.stdRank.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // studentBtn
            // 
            this.studentBtn.BackColor = System.Drawing.Color.Transparent;
            this.studentBtn.BorderRadius = 15;
            this.studentBtn.BorderThickness = 1;
            this.studentBtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.studentBtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.studentBtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.studentBtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.studentBtn.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.studentBtn.Font = new System.Drawing.Font("Segoe UI", 15F);
            this.studentBtn.ForeColor = System.Drawing.Color.Black;
            this.studentBtn.Location = new System.Drawing.Point(3, 1);
            this.studentBtn.Name = "studentBtn";
            this.studentBtn.Size = new System.Drawing.Size(148, 34);
            this.studentBtn.TabIndex = 4;
            this.studentBtn.Text = "نظرة خاطفة";
            // 
            // gradesBtn
            // 
            this.gradesBtn.BackColor = System.Drawing.Color.Transparent;
            this.gradesBtn.BorderRadius = 15;
            this.gradesBtn.BorderThickness = 1;
            this.gradesBtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.gradesBtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.gradesBtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.gradesBtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.gradesBtn.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(200)))));
            this.gradesBtn.Font = new System.Drawing.Font("Segoe UI", 15F);
            this.gradesBtn.ForeColor = System.Drawing.Color.White;
            this.gradesBtn.Location = new System.Drawing.Point(3, 36);
            this.gradesBtn.Name = "gradesBtn";
            this.gradesBtn.Size = new System.Drawing.Size(148, 34);
            this.gradesBtn.TabIndex = 5;
            this.gradesBtn.Text = "الدرجات";
            // 
            // StudentSearchRow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.gradesBtn);
            this.Controls.Add(this.studentBtn);
            this.Controls.Add(this.stdRank);
            this.Controls.Add(this.compDate);
            this.Controls.Add(this.compLevel);
            this.Controls.Add(this.stdName);
            this.Name = "StudentSearchRow";
            this.Size = new System.Drawing.Size(900, 72);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label stdName;
        private System.Windows.Forms.Label compLevel;
        private System.Windows.Forms.Label compDate;
        private System.Windows.Forms.Label stdRank;
        private Guna.UI2.WinForms.Guna2Button studentBtn;
        private Guna.UI2.WinForms.Guna2Button gradesBtn;
    }
}

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
            this.totalRows = new System.Windows.Forms.Label();
            this.gradesBtn = new Guna.UI2.WinForms.Guna2Button();
            this.SuspendLayout();
            // 
            // stdName
            // 
            this.stdName.BackColor = System.Drawing.Color.Transparent;
            this.stdName.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdName.Location = new System.Drawing.Point(512, 0);
            this.stdName.Name = "stdName";
            this.stdName.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.stdName.Size = new System.Drawing.Size(385, 40);
            this.stdName.TabIndex = 0;
            this.stdName.Text = "اسم الطالب";
            this.stdName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // compLevel
            // 
            this.compLevel.BackColor = System.Drawing.Color.Transparent;
            this.compLevel.Font = new System.Drawing.Font("Tahoma", 12F);
            this.compLevel.Location = new System.Drawing.Point(387, 0);
            this.compLevel.Name = "compLevel";
            this.compLevel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.compLevel.Size = new System.Drawing.Size(119, 40);
            this.compLevel.TabIndex = 1;
            this.compLevel.Text = "المستوى";
            this.compLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // compDate
            // 
            this.compDate.BackColor = System.Drawing.Color.Transparent;
            this.compDate.Font = new System.Drawing.Font("Tahoma", 12F);
            this.compDate.Location = new System.Drawing.Point(262, 0);
            this.compDate.Name = "compDate";
            this.compDate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.compDate.Size = new System.Drawing.Size(119, 40);
            this.compDate.TabIndex = 2;
            this.compDate.Text = "تاريخ المسابقة";
            this.compDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stdRank
            // 
            this.stdRank.BackColor = System.Drawing.Color.Transparent;
            this.stdRank.Font = new System.Drawing.Font("Tahoma", 12F);
            this.stdRank.Location = new System.Drawing.Point(137, 0);
            this.stdRank.Name = "stdRank";
            this.stdRank.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.stdRank.Size = new System.Drawing.Size(119, 40);
            this.stdRank.TabIndex = 3;
            this.stdRank.Text = "مركزه";
            this.stdRank.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // studentBtn
            // 
            this.studentBtn.BackColor = System.Drawing.Color.Transparent;
            this.studentBtn.BorderRadius = 15;
            this.studentBtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.studentBtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.studentBtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.studentBtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.studentBtn.FillColor = System.Drawing.Color.Transparent;
            this.studentBtn.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.studentBtn.ForeColor = System.Drawing.Color.Black;
            this.studentBtn.Image = global::Little_Hafiz.Properties.Resources.board;
            this.studentBtn.ImageSize = new System.Drawing.Size(40, 40);
            this.studentBtn.Location = new System.Drawing.Point(73, 0);
            this.studentBtn.Name = "studentBtn";
            this.studentBtn.Size = new System.Drawing.Size(40, 40);
            this.studentBtn.TabIndex = 4;
            this.studentBtn.SizeChanged += new System.EventHandler(this.StudentBtn_SizeChanged);
            // 
            // totalRows
            // 
            this.totalRows.BackColor = System.Drawing.Color.Transparent;
            this.totalRows.Font = new System.Drawing.Font("Tahoma", 12F);
            this.totalRows.Location = new System.Drawing.Point(3, 0);
            this.totalRows.Name = "totalRows";
            this.totalRows.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.totalRows.Size = new System.Drawing.Size(128, 40);
            this.totalRows.TabIndex = 6;
            this.totalRows.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.totalRows.Visible = false;
            // 
            // gradesBtn
            // 
            this.gradesBtn.BackColor = System.Drawing.Color.Transparent;
            this.gradesBtn.BorderRadius = 15;
            this.gradesBtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.gradesBtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.gradesBtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.gradesBtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.gradesBtn.FillColor = System.Drawing.Color.Transparent;
            this.gradesBtn.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.gradesBtn.ForeColor = System.Drawing.Color.White;
            this.gradesBtn.Image = global::Little_Hafiz.Properties.Resources.emoji100;
            this.gradesBtn.ImageSize = new System.Drawing.Size(40, 40);
            this.gradesBtn.Location = new System.Drawing.Point(21, 0);
            this.gradesBtn.Name = "gradesBtn";
            this.gradesBtn.Size = new System.Drawing.Size(40, 40);
            this.gradesBtn.TabIndex = 5;
            this.gradesBtn.SizeChanged += new System.EventHandler(this.GradesBtn_SizeChanged);
            // 
            // StudentSearchRow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.gradesBtn);
            this.Controls.Add(this.studentBtn);
            this.Controls.Add(this.stdRank);
            this.Controls.Add(this.compDate);
            this.Controls.Add(this.compLevel);
            this.Controls.Add(this.stdName);
            this.Controls.Add(this.totalRows);
            this.Name = "StudentSearchRow";
            this.Size = new System.Drawing.Size(900, 40);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label stdName;
        private System.Windows.Forms.Label compLevel;
        private System.Windows.Forms.Label compDate;
        private System.Windows.Forms.Label stdRank;
        private Guna.UI2.WinForms.Guna2Button studentBtn;
        private Guna.UI2.WinForms.Guna2Button gradesBtn;
        private System.Windows.Forms.Label totalRows;
    }
}

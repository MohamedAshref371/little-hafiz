using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static Little_Hafiz.StaticMembers;

namespace Little_Hafiz
{
    public partial class StudentSearchRow : UserControl
    {

        public event EventHandler StudentButtonClick
        {
            add => studentBtn.Click += value;
            remove => studentBtn.Click -= value;
        }

        public event EventHandler GradesButtonClick
        {
            add => gradesBtn.Click += value;
            remove => gradesBtn.Click -= value;
        }

        public StudentSearchRow(StudentSearchRowData data)
        {
            InitializeComponent();

            studentBtn.Tag = data.NationalNumber;
            gradesBtn.Tag = data.NationalNumber;
            stdName.Text = data.FullName;
            compLevel.Text = ConvertNumberToRank(data.CompetitionLevel);
            compDate.Text = data.CompetitionDate;
            stdRank.Text = ConvertNumberToRank(data.Rank);
        }

        #region Border Radius
        private readonly int borderRadius = 20;

        private GraphicsPath GetRoundedPath()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, borderRadius, borderRadius, 180, 90);
            path.AddArc(Width - borderRadius, 0, borderRadius, borderRadius, 270, 90);
            path.AddArc(Width - borderRadius, Height - borderRadius, borderRadius, borderRadius, 0, 90);
            path.AddArc(0, Height - borderRadius, borderRadius, borderRadius, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.Region = new Region(GetRoundedPath());
        }
        #endregion
    }
}

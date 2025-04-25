using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public partial class StudentRankRow : UserControl
    {
        public StudentRankRow()
        {
            InitializeComponent();

            stdRank.Visible = false;
            stdRankLabel.Visible = true;
        }

        readonly CompetitionRankData data;
        public StudentRankRow(CompetitionRankData data)
        {
            InitializeComponent();
            this.data = data;

            stdCode.Text = data.StudentCode.ToString();
            stdName.Text = data.StudentName;
            stdScore.Text = data.Score.ToString();
            stdRank.Text = data.Rank.ToString();

            stdRank.ValueChanged += StdRank_ValueChanged;
        }

        private void StdRank_ValueChanged(object sender, EventArgs e)
        {
            data.Rank = (int)stdRank.Value;
            DatabaseHelper.UpdateStudentRank(data);
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

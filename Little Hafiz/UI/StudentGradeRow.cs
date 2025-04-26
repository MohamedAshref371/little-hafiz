using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public partial class StudentGradeRow : UserControl
    {
        public StudentGradeRow()
        {
            InitializeComponent();

            stdScore.Visible = false;
            stdRank.Visible = false;
            scoreLabel.Visible = true;
            rankLabel.Visible = true;
        }

        readonly CompetitionGradeData data;
        public StudentGradeRow(CompetitionGradeData data)
        {
            InitializeComponent();
            this.data = data;

            stdCode.Text = data.StudentCode.ToString();
            prevLevel.Text = Ranks.ConvertNumberToRank(data.PreviousLevel);
            currentLevel.Text = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            compDate.Text = data.CompetitionDate;
            stdScore.Value = (decimal)data.Score;
            stdRank.Value = data.Rank;

            stdScore.ValueChanged += StdScore_ValueChanged;
            stdRank.ValueChanged += StdRank_ValueChanged;
        }

        private void StdScore_ValueChanged(object sender, EventArgs e)
        {
            data.Score = (float)stdScore.Value;
            DatabaseHelper.UpdateStudentGrade(data);
        }

        private void StdRank_ValueChanged(object sender, EventArgs e)
        {
            data.Rank = (int)stdRank.Value;
            DatabaseHelper.UpdateStudentGrade(data);
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

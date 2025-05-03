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
            deleteBtn.Visible = false;
            scoreLabel.Visible = true;
            rankLabel.Visible = true;
        }

        public readonly CompetitionGradeData CompetitionGradeData;
        public StudentGradeRow(CompetitionGradeData data)
        {
            InitializeComponent();
            this.CompetitionGradeData = data;

            stdCode.Text = data.StudentCode.ToString();
            prevLevel.Text = Ranks.ConvertNumberToRank(data.PreviousLevel);
            currentLevel.Text = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            compDate.Text = data.CompetitionDate;
            stdScore.Value = (decimal)data.Score;
            stdRank.Value = data.Rank;

            if (DatabaseHelper.CurrentOffice != 0)
                deleteBtn.Visible = false;

            stdScore.ValueChanged += StdScore_ValueChanged;
            stdRank.ValueChanged += StdRank_ValueChanged;
        }

        private void CompDate_DoubleClick(object sender, EventArgs e)
        {
            if (CompetitionGradeData.Notes != "")
                MessageBox.Show(CompetitionGradeData.Notes);
        }
        
        private void StdScore_ValueChanged(object sender, EventArgs e)
        {
            if (DatabaseHelper.CurrentOffice == 0)
                saveBtn.Visible = true;
        }
        
        private void StdRank_ValueChanged(object sender, EventArgs e)
        {
            if (DatabaseHelper.CurrentOffice == 0)
                saveBtn.Visible = true;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("لا يمكن للنسخ الفرعية تعديل المسابقات");
                return;
            }
            
            CompetitionGradeData.Score = (float)stdScore.Value;
            CompetitionGradeData.Rank = (int)stdRank.Value;

            if (DatabaseHelper.UpdateStudentGrade(CompetitionGradeData) != -1)
            {
                saveBtn.Visible = false;
                Program.Form.UpdateRankOfStudentRow(this, CompetitionGradeData.Rank);
            }
            else
                MessageBox.Show("حدث خطأ غير معروف", "خطأ !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("لا يمكن للنسخ الفرعية حذف المسابقات");
                return;
            }

            Program.Form.DeleteStudentGradeRow(this);
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

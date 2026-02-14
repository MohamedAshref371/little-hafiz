using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static ClosedXML.Excel.XLPredefinedFormat;

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

        public StudentSearchRowData StudentSearchRowData;
        public StudentSearchRow()
        {
            InitializeComponent();
        }

        public void SetData(int total)
        {
            studentBtn.Visible = false;
            gradesBtn.Visible = false;

            if (total >= 3)
            {
                totalRows.Visible = true;
                totalRows.Text = total.ToString() + (total <= 10 ? " طلاب" : " طالب");
            }
            else
                totalRows.Visible = false;
        }

        public int Idx = -1;
        public void SetData(StudentSearchRowData data, int idx = -1)
        {
            studentBtn.Visible = true;
            gradesBtn.Visible = true;

            if (idx >= 0)
            {
                Idx = idx;
                num.Text = (idx + 1).ToString();
            }
            StudentSearchRowData = data;

            stdName.Text = data.FullName;
            compLevel.Text = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            compDate.Text = data.CompetitionDate;
            stdRank.Text = Ranks.ConvertNumberToRank(data.Rank);
        }

        public void SetRank(int rank)
        {
            StudentSearchRowData.Rank = rank;
            stdRank.Text = Ranks.ConvertNumberToRank(rank);
        }

        private void StudentBtn_SizeChanged(object sender, EventArgs e)
        {
            int minSize = Math.Min(studentBtn.Width, studentBtn.Height);
            studentBtn.ImageSize = new Size(minSize, minSize);
        }

        private void GradesBtn_SizeChanged(object sender, EventArgs e)
        {
            int minSize = Math.Min(gradesBtn.Width, gradesBtn.Height);
            gradesBtn.ImageSize = new Size(minSize, minSize);
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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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

        public StudentSearchRow()
        {
            InitializeComponent();

            studentBtn.Visible = false;
            gradesBtn.Visible = false;
        }

        public StudentSearchRowData StudentSearchRowData;
        public StudentSearchRow(StudentSearchRowData data)
        {
            InitializeComponent();

            StudentSearchRowData = data;

            stdName.Text = data.FullName;
            compLevel.Text = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            compDate.Text = data.CompetitionDate;
            stdRank.Text = Ranks.ConvertNumberToRank(data.Rank);
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

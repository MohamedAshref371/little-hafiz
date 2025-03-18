using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
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

        private static string ConvertNumberToRank(int? i)
        {
            switch (i)
            {
                case 0: return "لا يوجد";
                case 1: return "الأول";
                case 2: return "الثاني";
                case 3: return "الثالث";
                case 4: return "الرابع";
                case 5: return "الخامس";
                case 6: return "السادس";
                case 7: return "السابع";
                case 8: return "الثامن";
                case 9: return "التاسع";
                case 10: return "العاشر";
                case null: return "غير معروف";
                default: return i.ToString();
            }
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

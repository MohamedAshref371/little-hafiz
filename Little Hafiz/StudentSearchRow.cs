using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public partial class StudentSearchRow : UserControl
    {
        public event EventHandler ButtonClick
        {
            add => showStudentBtn.Click += value;
            remove => showStudentBtn.Click -= value;
        }

        public StudentSearchRow(StudentSearchRowData data)
        {
            InitializeComponent();
            showStudentBtn.Tag = data.NationalNumber;
            stdName.Text = data.FullName;
            compLevel.Text = ConvertNumberToRank(data.CompetitionLevel);
            compDate.Text = data.CompetitionDate;
            stdRank.Text = ConvertNumberToRank(data.Rank);
        }

        private static string ConvertNumberToRank(int? i)
        {
            switch (i)
            {
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
    }
}

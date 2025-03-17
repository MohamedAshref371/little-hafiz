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
            compLevel.Text = data.CompetitionLevel.ToString();
            compDate.Text = data.CompetitionDate;
            stdRank.Text = data.Rank.ToString();
        }
    }
}

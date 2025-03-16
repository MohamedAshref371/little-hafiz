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
        private string nationalNumber;
        public StudentSearchRow(StudentSearchRowData data)
        {
            InitializeComponent();
            nationalNumber = data.NationalNumber;
            stdName.Text = data.FullName;
            compLevel.Text = data.CompetitionLevel.ToString();
            compDate.Text = data.CompetitionDate;
            stdRank.Text = data.Rank.ToString();
        }

        private void ShowStudentBtn_Click(object sender, EventArgs e)
        {

        }
    }
}

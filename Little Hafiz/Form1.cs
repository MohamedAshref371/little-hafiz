using DocumentFormat.OpenXml.Office2010.Excel;
using Guna.UI2.WinForms;
using QuranKareem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public partial class Form1 : Form
    {
        private readonly int SizeX = 950, SizeY = 700;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Timer scrollTimer = new Timer { Interval = 100 };
            scrollTimer.Tick += (sender1, e1) => {
                scrollTimer.Stop();
                if (studentDataPanel.Visible)
                    studentDataPanel.Invalidate();

            };
            studentDataPanel.Scroll += (sender1, e1) => { scrollTimer.Stop(); scrollTimer.Start(); };
            studentDataPanel.MouseWheel += (sender1, e1) => { scrollTimer.Stop(); scrollTimer.Start(); };
        }

        private void CloseBtn_Click(object sender, EventArgs e)
            => Close();

        private void MinimizeBtn_Click(object sender, EventArgs e)
            => WindowState = FormWindowState.Minimized;

        private void MaximizeBtn_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            FormSize fs = new FormSize(SizeX, SizeY, Size.Width, Size.Height);
            fs.SetControls(Controls);
            maximizeBtn.Visible = false;
            minimizeBtn.Location = new Point(maximizeBtn.Location.X + maximizeBtn.Size.Width - minimizeBtn.Size.Width, minimizeBtn.Location.Y); 
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            var students = DatabaseHelper.SelectStudents
                (
                    undoubtedName: stdNameCheckBox.Checked ? stdNameSearch.Text : null,
                    nationalNumber: stdNationalCheckBox.Checked ? stdNationalSearch.Text : null,
                    phoneNumber: stdPhoneCheckBox.Checked ? stdPhoneSearch.Text : null,
                    email: stdEmailCheckBox.Checked ? stdEmailSearch.Text : null
                );
            // note: display all students in UI
        }
    }
}

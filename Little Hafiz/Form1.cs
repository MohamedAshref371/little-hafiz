using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
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
            //if (!Directory.Exists("excels"))
            //    Directory.CreateDirectory("excels");

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

        FormSize fs = null;
        private void MaximizeBtn_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            fs = new FormSize(SizeX, SizeY, Size.Width, Size.Height);
            fs.SetControls(Controls);
            maximizeBtn.Visible = false;
            minimizeBtn.Location = new Point(maximizeBtn.Location.X + maximizeBtn.Size.Width - minimizeBtn.Size.Width, minimizeBtn.Location.Y); 
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            StudentSearchRowData[] students = DatabaseHelper.SelectStudents
                    (
                        undoubtedName: stdNameCheckBox.Checked ? stdNameSearch.Text : null,
                        nationalNumber: stdNationalCheckBox.Checked ? stdNationalSearch.Text : null,
                        phoneNumber: stdPhoneCheckBox.Checked ? stdPhoneSearch.Text : null,
                        email: stdEmailCheckBox.Checked ? stdEmailSearch.Text : null
                    );

            studentsListPanel.Controls.Clear();
            StudentSearchRow stdRow;
            for (int i = 0; i < students?.Length; i++)
            {
                stdRow = new StudentSearchRow(students[i]);
                stdRow.Location = new Point(9, (stdRow.Size.Height + 3) * i + 9);
                stdRow.showStudentBtn.Click += ShowStudentBtn_Click;
                studentsListPanel.Controls.Add(stdRow);
            }
        }

        private void ShowStudentBtn_Click(object sender, EventArgs e)
        {
            openAddStudentBtn.Visible = false;
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            var srudent = DatabaseHelper.SelectStudent((string)((Guna2Button)sender).Tag);
            // studentDataPanel set data
            studentDataPanel.Visible = true;
        }

        private void OpenAddStudentBtn_Click(object sender, EventArgs e)
        {
            openAddStudentBtn.Visible = false;
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            studentDataPanel.Visible = true;
        }

        #region Student Data Panel
        private void National_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void National_TextChanged(object sender, EventArgs e)
        {
            string newText = new string(stdNational.Text.Where(char.IsDigit).ToArray());
            if (newText.Length > 14)
                newText = newText.Substring(0, 14);

            if (stdNational.Text != newText)
            {
                int cursorPosition = stdNational.SelectionStart - (stdNational.Text.Length - newText.Length);
                stdNational.Text = newText;
                stdNational.SelectionStart = Math.Max(0, cursorPosition);
            }

            if (stdNational.Text.Length == 14)
            {
                DateTime? date = GetDateFromNationalNumber(stdNational.Text);
                if (date.HasValue)
                    stdBirthDate.Value = (DateTime)date;
                wrongValueLabel.Visible = !date.HasValue;
            }
        }

        private DateTime? GetDateFromNationalNumber(string natNum)
        {
            int year = 100 * (natNum[0] - '0' + 17)
                + int.Parse(natNum.Substring(1, 2));
            int month = int.Parse(natNum.Substring(3, 2));
            int day = int.Parse(natNum.Substring(5, 2));

            try { return new DateTime(year, month, day); }
            catch { return null; }
        }

        private void PhoneNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '+' && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void StdBrothers_ValueChanged(object sender, EventArgs e)
            => stdArrangement.Maximum = stdBrothers.Value + 1;

        private void StdImageSelectorBtn_Click(object sender, EventArgs e)
        {
            if (selectImageDialog.ShowDialog() == DialogResult.OK)
            {
                stdImagePath.Text = selectImageDialog.FileName;
                Bitmap bitmap = new Bitmap(new Bitmap(stdImagePath.Text), studentImage.Size.Width, studentImage.Size.Height);
                studentImage.Image = bitmap;
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            studentDataPanel.Visible = false;
            openAddStudentBtn.Visible = true;
            studentSearchPanel.Visible = true;
            studentsListPanel.Visible = true;
        }

        private void AddStudentBtn_Click(object sender, EventArgs e)
        {
            if (stdNational.Text.Length != 14 || wrongValueLabel.Visible)
            {
                MessageBox.Show("أدخل الرقم القومي الصحيح");
                return;
            }

            if (DatabaseHelper.AddStudent(GetStudentData()) != -1)
                CancelBtn_Click(null, null);
        }

        private StudentData GetStudentData()
            => new StudentData
            {
                FullName = stdName.Text,
                NationalNumber = stdNational.Text,
                BirthDate = stdBirthDate.Value.ToString("yyyy/MM/dd"),
                Job = stdJob.Text,
                FatherQualification = fatherQuali.Text,
                MotherQualification = motherQuali.Text,
                FatherJob = fatherJob.Text,
                MotherJob = motherJob.Text,
                FatherPhone = fatherPhone.Text,
                MotherPhone = motherPhone.Text,
                GuardianName = guardianName.Text,
                GuardianLink = guardianLink.Text,
                GuardianBirth = guardianBirth.Value.ToString("yyy/MM/dd"),
                PhoneNumber = stdPhone.Text,
                Address = stdAddress.Text,
                Email = stdEmail.Text,
                Facebook = stdFacebook.Text,
                School = stdSchool.Text,
                Class = stdClass.Text,
                BrothersCount = (int)stdBrothers.Value,
                ArrangementBetweenBrothers = (int)stdArrangement.Value,
                Level = (int)stdLevel.Value,
                MemorizationAmount = stdMemo.Text,
                StudentMashaykh = stdMashaykh.Text,
                MashaykhPlaces = stdMashaykhPlaces.Text,
                JoiningDate = stdJoiningDate.Value.ToString("yyy/MM/dd"),
                FirstConclusionDate = stdFirstConclusion.Value.ToString("yyy/MM/dd"),
                Certificates = stdCertificates.Text,
                Ijazah = stdIjazah.Text,
                Courses = stdCourses.Text,
                Skills = stdSkills.Text,
                Hobbies = stdHobbies.Text,
                Image = stdImagePath.Text
            };

        private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("\\/:*?\"<>|".Contains(e.KeyChar))
                e.Handled = true;
        }

        private enum StudentPanelState
        {
            Add = 0,
            Update = 1,
            Read = 2,
        }
        #endregion
    }
}

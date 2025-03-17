using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Guna.UI2.WinForms;
using QuranKareem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
                stdRow.ButtonClick += ShowStudentBtn_Click;
                studentsListPanel.Controls.Add(stdRow);
            }
        }

        private void ShowStudentBtn_Click(object sender, EventArgs e)
        {
            openAddStudentBtn.Visible = false;
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;

            string national = (string)((Guna2Button)sender).Tag;
            StudentData stdData = DatabaseHelper.SelectStudent(national);
            CompetitionGrade[] grades = DatabaseHelper.SelectStudentGrades(national);

            SetStudentData(stdData, grades);

            stdNational.ReadOnly = true;
            studentPanelState = StudentPanelState.Update;
            studentDataPanel.Visible = true;
        }

        private void OpenAddStudentBtn_Click(object sender, EventArgs e)
        {
            openAddStudentBtn.Visible = false;
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;

            SetStudentData(null, null);

            stdNational.ReadOnly = false;
            studentPanelState = StudentPanelState.Add;
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
        {
            return new StudentData
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
        }

        private void SetStudentData(StudentData stdData, CompetitionGrade[] grades)
        {
            if (stdData is null)
                SetStudentDataAtStudentDataIsNull();
            else
                SetStudentDataAtStudentDataIsNotNull(stdData);

            wrongValueLabel.Visible = false;

        }

        private void SetStudentDataAtStudentDataIsNull()
        {
            stdName.Text = "";
            stdNational.Text = "";
            stdBirthDate.Value = DateTime.Now.AddYears(-10);
            stdJob.Text = "";
            fatherQuali.Text = "";
            motherQuali.Text = "";
            fatherJob.Text = "";
            motherJob.Text = "";
            fatherPhone.Text = "";
            motherPhone.Text = "";
            guardianName.Text = "";
            guardianLink.Text = "";
            guardianBirth.Value = DateTime.Now.AddYears(-20);
            stdPhone.Text = "";
            stdAddress.Text = "";
            stdEmail.Text = "";
            stdFacebook.Text = "";
            stdSchool.Text = "";
            stdClass.Text = "";
            stdBrothers.Value = 0;
            //stdArrangement.Value = 1;
            stdLevel.Value = 0;
            stdMemo.Text = "";
            stdMashaykh.Text = "";
            stdMashaykhPlaces.Text = "";
            stdJoiningDate.Value = DateTime.Now.AddYears(-5);
            stdFirstConclusion.Value = DateTime.Now.AddYears(-2);
            stdCertificates.Text = "";
            stdIjazah.Text = "";
            stdCourses.Text = "";
            stdSkills.Text = "";
            stdHobbies.Text = "";
            stdImagePath.Text = "";
        }

        private void SetStudentDataAtStudentDataIsNotNull(StudentData stdData)
        {
            stdName.Text = stdData.FullName;
            stdNational.Text = stdData.NationalNumber;
            stdBirthDate.Value = ParseExact(stdData.BirthDate);
            stdJob.Text = stdData.Job;
            fatherQuali.Text = stdData.FatherQualification;
            motherQuali.Text = stdData.MotherQualification;
            fatherJob.Text = stdData.FatherJob;
            motherJob.Text = stdData.MotherJob;
            fatherPhone.Text = stdData.FatherPhone;
            motherPhone.Text = stdData.MotherPhone;
            guardianName.Text = stdData.GuardianName;
            guardianLink.Text = stdData.GuardianLink;
            guardianBirth.Value = ParseExact(stdData.GuardianBirth);
            stdPhone.Text = stdData.PhoneNumber;
            stdAddress.Text = stdData.Address;
            stdEmail.Text = stdData.Email;
            stdFacebook.Text = stdData.Facebook;
            stdSchool.Text = stdData.School;
            stdClass.Text = stdData.Class;
            stdBrothers.Value = stdData.BrothersCount;
            stdArrangement.Value = stdData.ArrangementBetweenBrothers;
            stdLevel.Value = stdData.Level;
            stdMemo.Text = stdData.MemorizationAmount;
            stdMashaykh.Text = stdData.StudentMashaykh;
            stdMashaykhPlaces.Text = stdData.MashaykhPlaces;
            stdJoiningDate.Value = ParseExact(stdData.JoiningDate);
            stdFirstConclusion.Value = ParseExact(stdData.FirstConclusionDate);
            stdCertificates.Text = stdData.Certificates;
            stdIjazah.Text = stdData.Ijazah;
            stdCourses.Text = stdData.Courses;
            stdSkills.Text = stdData.Skills;
            stdHobbies.Text = stdData.Hobbies;
            stdImagePath.Text = stdData.Image;
        }

        private DateTime ParseExact(string date) => DateTime.ParseExact(date, "yyyy/MM/dd", DateTimeFormatInfo.CurrentInfo);

        private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("\\/:*?\"<>|".Contains(e.KeyChar))
                e.Handled = true;
        }

        StudentPanelState studentPanelState = StudentPanelState.Read;
        private enum StudentPanelState
        {
            Add = 0,
            Update = 1,
            Read = 2,
        }
        #endregion
    }
}

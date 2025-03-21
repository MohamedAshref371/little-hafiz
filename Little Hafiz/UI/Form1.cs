﻿using ClosedXML.Excel;
using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();

        #region Form1
        private readonly int SizeX = 950, SizeY = 700;

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private void Form1_Load(object sender, EventArgs e)
        {
            AddTitleInStudentsListPanel();

            Timer timer = new Timer { Interval = 10 };
            timer.Tick += (s, e1) =>
            {
                timer.Stop();
                if (studentDataPanel.Visible)
                    studentDataPanel.Invalidate();

                if (studentsListPanel.Visible)
                    studentsListPanel.Invalidate();

                if (studentGradesListPanel.Visible)
                    studentGradesListPanel.Invalidate();

                if (Control.MouseButtons == MouseButtons.None)
                    this.Opacity = 1.0;
            };

            MouseEventHandler meh = (s, e1) =>
            {
                if (WindowState != FormWindowState.Maximized && e1.Button == MouseButtons.Left)
                {
                    this.Opacity = 0.9;
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);

                    timer.Start();
                }
            };
            headerPanel.MouseDown += meh;
            formTitle.MouseDown += meh;
            formImage.MouseDown += meh;

            studentDataPanel.Scroll += (s, e1) => { timer.Stop(); timer.Start(); };
            studentDataPanel.MouseWheel += (s, e1) => { timer.Stop(); timer.Start(); };

            studentsListPanel.Scroll += (s, e1) => { timer.Stop(); timer.Start(); };
            studentsListPanel.MouseWheel += (s, e1) => { timer.Stop(); timer.Start(); };

            studentGradesListPanel.Scroll += (s, e1) => { timer.Stop(); timer.Start(); };
            studentGradesListPanel.MouseWheel += (s, e1) => { timer.Stop(); timer.Start(); };
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
            
            if (studentDataPanel.Visible) SetStudentImage();
            if (studentGradesPanel.Visible) SetStudentImage2();
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }
        #endregion

        #region Two Serach Panels
        private void AddTitleInStudentsListPanel()
        {
            studentsListPanel.Controls.Clear();
            studentsListPanel.Controls.Add(new StudentSearchRow { Location = new Point(9, 9) });
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

            AddTitleInStudentsListPanel();

            StudentSearchRow stdRow;
            for (int i = 0; i < students?.Length; i++)
            {
                stdRow = new StudentSearchRow(students[i]);
                stdRow.Location = new Point(9, (stdRow.Size.Height + 3) * (i + 1) + 9);
                stdRow.StudentButtonClick += ShowStudentBtn_Click;
                stdRow.GradesButtonClick += ShowGradesBtn_Click;
                studentsListPanel.Controls.Add(stdRow);
            }
            fs?.SetControls(studentsListPanel.Controls);
        }

        private void ShowStudentBtn_Click(object sender, EventArgs e)
        {
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            footerPanel.Visible = false;

            string national = ((StudentSearchRow)((Guna2Button)sender).Parent).StudentSearchRowData.NationalNumber;
            StudentData stdData = DatabaseHelper.SelectStudent(national);

            SetStudentData(stdData);

            addStudentBtn.Text = "تعديل";
            stdNational.Enabled = false;
            studentPanelState = StudentPanelState.Update;
            studentDataPanel.Visible = true;
        }

        private void ShowGradesBtn_Click(object sender, EventArgs e)
        {
            var data = ((StudentSearchRow)((Guna2Button)sender).Parent).StudentSearchRowData;

            OpenStudentGradesPanel(data, DatabaseHelper.SelectStudentGrades(data.NationalNumber));
        }
        #endregion

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
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        private void StdBrothers_ValueChanged(object sender, EventArgs e)
            => stdArrangement.Maximum = stdBrothers.Value + 1;

        private void StdImageSelectorBtn_Click(object sender, EventArgs e)
        {
            if (selectImageDialog.ShowDialog() == DialogResult.OK)
            {
                stdImagePath.Text = selectImageDialog.FileName;
                SetStudentImage();
            }
        }

        private void SetStudentImage()
        {
            if (File.Exists(stdImagePath.Text))
                studentImage.Image = new Bitmap(new Bitmap(stdImagePath.Text), studentImage.Size.Width, studentImage.Size.Height);
            else
                studentImage.Image = null;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            studentDataPanel.Visible = false;
            studentSearchPanel.Visible = true;
            studentsListPanel.Visible = true;
            footerPanel.Visible = true;
        }

        private void AddStudentBtn_Click(object sender, EventArgs e)
        {
            if (stdNational.Text.Length != 14 || wrongValueLabel.Visible)
            {
                MessageBox.Show("أدخل الرقم القومي الصحيح");
                return;
            }

            if (studentPanelState == StudentPanelState.Add && DatabaseHelper.AddStudent(GetStudentData()) != -1)
            {
                stdNationalCheckBox.Checked = true;
                stdNationalSearch.Text = stdNational.Text;
                stdNameCheckBox.Checked = false;
                stdPhoneCheckBox.Checked = false;
                stdEmailCheckBox.Checked = false;
                CancelBtn_Click(null, null);
                SearchBtn_Click(null, null);
            }
            else if (studentPanelState == StudentPanelState.Update && DatabaseHelper.UpdateStudent(GetStudentData()) != -1)
            {
                CancelBtn_Click(null, null);
                SearchBtn_Click(null, null);
            }
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
                MemorizePlaces = stdMemoPlaces.Text,
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

        private void SetStudentData(StudentData stdData)
        {
            if (stdData is null)
                SetStudentDataAtStudentDataIsNull();
            else
                SetStudentDataAtStudentDataIsNotNull(stdData);

            wrongValueLabel.Visible = false;
            SetStudentImage();
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
            stdMemoPlaces.Text = "";
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
            stdMemoPlaces.Text = stdData.MemorizePlaces;
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
            if ("\\/:*?\"<>|'".Contains(e.KeyChar))
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

        #region Student Grades Panel
        private void OpenStudentGradesPanel(StudentSearchRowData data, CompetitionGradeData[] grades)
        {
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            footerPanel.Visible = false;

            stdName2.Text = data.FullName;
            stdNational2.Text = data.NationalNumber;
            stdImagePath2.Text = data.Image;
            SetStudentImage2();

            prevLevel.Minimum = 0; prevLevel.Maximum = 10;
            prevLevel.Value = data.CompetitionLevel ?? 0;
            SetPrevLevelMinMax();

            studentGradesListPanel.Controls.Clear();
            studentGradesListPanel.Controls.Add(new StudentGradeRow { Location = new Point(30, 9) });

            StudentGradeRow stdRow;
            for (int i = 0; i < grades.Length; i++)
            {
                stdRow = new StudentGradeRow(grades[i]);
                stdRow.Location = new Point(30, (stdRow.Size.Height + 3) * (i + 1) + 9);
                studentGradesListPanel.Controls.Add(stdRow);
            }
            fs?.SetControls(studentGradesListPanel.Controls);
            studentGradesPanel.Visible = true;
        }

        private void SetPrevLevelMinMax()
        {
            if (prevLevel.Value != 0)
            {
                prevLevel.Minimum = 1;
                prevLevel.Maximum = prevLevel.Value;
            }
        }

        private void SetCurrentLevelMaximum()
        {
            if (prevLevel.Value == 0)
                currentLevel.Maximum = 10;
            else if (prevLevel.Value == 1)
                currentLevel.Maximum = 1;
            else
                currentLevel.Maximum = prevLevel.Value - 1;
        }

        private void AddGradeBtn_Click(object sender, EventArgs e)
        {
            CompetitionGradeData data = new CompetitionGradeData
            {
                NationalNumber = stdNational2.Text,
                StudentCode = (int)stdCode.Value,
                PreviousLevel = (int)prevLevel.Value,
                CompetitionLevel = (int)currentLevel.Value,
                CompetitionDate = compDate.Value.ToString("yyyy/MM"),
                Score = (float)stdScore.Value,
                Rank = (int)stdRank.Value,
            };

            if (DatabaseHelper.AddGrade(data) != -1)
            {
                StudentGradeRow stdRow = new StudentGradeRow(data);
                stdRow.Location = new Point(30, (stdRow.Size.Height + 3) * studentGradesListPanel.Controls.Count + 9);
                fs?.SetControl(stdRow);
                fs?.SetControls(stdRow.Controls);
                studentGradesListPanel.Controls.Add(stdRow);
                prevLevel.Value = currentLevel.Value;
                SetPrevLevelMinMax();
            }
        }

        private void CancelBtn2_Click(object sender, EventArgs e)
        {
            studentGradesPanel.Visible = false;
            studentSearchPanel.Visible = true;
            studentsListPanel.Visible = true;
            footerPanel.Visible = true;
        }

        private void PrevLevel_ValueChanged(object sender, EventArgs e)
        {
            SetCurrentLevelMaximum();
            prevLevelExplain.Text = Ranks.RanksText[(int)prevLevel.Value];
        }
        
        private void CurrentLevel_ValueChanged(object sender, EventArgs e)
            => currentLevelExplain.Text = Ranks.RanksText[(int)currentLevel.Value];
        
        private void SetStudentImage2()
        {
            if (File.Exists(stdImagePath2.Text))
                studentImage2.Image = new Bitmap(new Bitmap(stdImagePath2.Text), studentImage2.Size.Width, studentImage2.Size.Height);
            else
                studentImage2.Image = null;
        }
        #endregion

        #region Footer Panel
        private void OpenAddStudentBtn_Click(object sender, EventArgs e)
        {
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            footerPanel.Visible = false;

            SetStudentData(null);

            addStudentBtn.Text = "إضافة";
            stdNational.Enabled = true;
            studentPanelState = StudentPanelState.Add;
            studentDataPanel.Visible = true;
        }


        private void ExcelRowsFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            excelDateFilter.Visible = false;
            int index = excelRowsFilter.SelectedIndex;
            if (index == 2)
            {
                excelDateFilter.CustomFormat = "yyyy";
                excelDateFilter.Visible = true;
            }
            else if (index == 4)
            {
                excelDateFilter.CustomFormat = "yyyy MMM";
                excelDateFilter.Visible = true;
            }
        }

        private void ExtractExcelBtn_Click(object sender, EventArgs e)
        {
            int year = 0, month = 0;
            int index = excelRowsFilter.SelectedIndex;
            DateTime dt;

            if (index == 2 || index == 4)
            {
                dt = excelDateFilter.Value;
                year = dt.Year;
                if (index == 4) month = dt.Month;
            }
            else if (index == 1 || index == 3)
            {
                dt = DateTime.Now;
                year = dt.Year;
                if (index == 3) month = dt.Month;
            }

            if (saveExcelFileDialog.ShowDialog() != DialogResult.OK) return;

            ExcelRowData[] rows = DatabaseHelper.SelectExcelRowData(year, month);
            if (rows is null) return;

            using (var workbook = new XLWorkbook())
            {
                workbook.RightToLeft = true;
                IXLWorksheet[] sheets = new IXLWorksheet[11]
                {
                    workbook.Worksheets.Add("جميع المستويات"),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[1]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[2]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[3]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[4]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[5]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[6]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[7]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[8]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[9]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[10]),
                };
                
                for (int i = 0; i < sheets.Length; i++)
                    SetTitlesOnExcelFile(sheets[i]);

                int[] sheetsRowIndexes = new int[11] {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
                for (int i = 0; i < rows.Length; i++)
                {
                    int sheetIndex = rows[i].CompetitionLevel;
                    SetDataOnExcelFile(sheets[sheetIndex], ref sheetsRowIndexes[sheetIndex], rows[i]);
                }

                workbook.SaveAs(saveExcelFileDialog.FileName);
            }
        }

        private void SetTitlesOnExcelFile(IXLWorksheet sheet)
        {
            sheet.Range("A1:L1").Merge().Value = "الحافظ الصغير- المسابقة القرآنية الرمضانية";

            sheet.Cell(2, 1).Value = "م";
            sheet.Cell(2, 2).Value = "الكود";
            sheet.Cell(2, 3).Value = "الاسم";
            sheet.Cell(2, 4).Value = "تاريخ الميلاد";
            sheet.Cell(2, 5).Value = "رقم التليفون";
            sheet.Cell(2, 6).Value = "الحالي";
            sheet.Cell(2, 7).Value = "السابق";
            sheet.Cell(2, 8).Value = "الصف";
            sheet.Cell(2, 9).Value = "العنوان";
            sheet.Cell(2, 10).Value = "مكان الحفظ";
            sheet.Cell(2, 11).Value = "المركز";
            sheet.Cell(2, 12).Value = "تاريخ إضافة المسابقة";
        }

        private void SetDataOnExcelFile(IXLWorksheet sheet, ref int row, ExcelRowData data)
        {
            sheet.Cell(row, 1).Value = (row - 2).ToString();
            sheet.Cell(row, 2).Value = data.StudentCode;
            sheet.Cell(row, 3).Value = data.FullName;
            sheet.Cell(row, 4).Value = data.BirthDate;
            sheet.Cell(row, 5).Value = data.PhoneNumber;
            sheet.Cell(row, 6).Value = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            sheet.Cell(row, 7).Value = Ranks.ConvertNumberToRank(data.PreviousLevel);
            sheet.Cell(row, 8).Value = data.Class;
            sheet.Cell(row, 9).Value = data.Address;
            sheet.Cell(row, 10).Value = data.MemoPlace;
            sheet.Cell(row, 11).Value = data.Rank;
            sheet.Cell(row, 12).Value = data.CompetitionAddedDate;
            row++;
        }
        #endregion

        private void ReleasesLatestBtn_Click(object sender, EventArgs e)
            => Process.Start("https://github.com/MohamedAshref371/little-hafiz/releases/latest");
    }
}

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace Little_Hafiz
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();

        #region Form1
        private readonly int SizeX = 950, SizeY = 700;
        private int NewSizeX = 950, NewSizeY = 700;

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

                if (ranksListPanel.Visible)
                    ranksListPanel.Invalidate();

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
            //formImage.MouseDown += meh;

            studentDataPanel.Scroll += (s, e1) => { timer.Stop(); timer.Start(); };
            studentDataPanel.MouseWheel += (s, e1) => { timer.Stop(); timer.Start(); };

            studentsListPanel.Scroll += (s, e1) => { timer.Stop(); timer.Start(); };
            studentsListPanel.MouseWheel += (s, e1) => { timer.Stop(); timer.Start(); };

            studentGradesListPanel.Scroll += (s, e1) => { timer.Stop(); timer.Start(); };
            studentGradesListPanel.MouseWheel += (s, e1) => { timer.Stop(); timer.Start(); };

            ranksListPanel.Scroll += (s, e1) => { timer.Stop(); timer.Start(); };
            ranksListPanel.MouseWheel += (s, e1) => { timer.Stop(); timer.Start(); };

            dataRecorderCheckBox.Checked = Program.Record;
            disableAtAll.Visible = Program.Record;
            dataRecorderCheckBox.CheckedChanged += DataRecorderCheckBox_CheckedChanged;

            GetOffice();

            if (Properties.Settings.Default.CheckUpdate)
            {
                Timer timer2 = new Timer { Interval = 1 };
                timer2.Tick += (s, e1) =>
                {
                    timer2.Stop();
                    DownloadUpdate();
                    timer2.Dispose();
                };
                timer2.Start();
            }
        }

        string[] offices;
        private void GetOffice()
        {
            offices = DatabaseHelper.GetOffices();
            if (offices is null)
            {
                MessageBox.Show("خطأ، سيتم إغلاق البرنامج", "خطأ !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            int ofc = DatabaseHelper.CurrentOffice;

            int wdth = formTitle.Size.Width;
            formTitle.Text = offices[ofc];
            formTitle.Location = new Point(formTitle.Location.X + wdth - formTitle.Size.Width, formTitle.Location.Y);
            
            stdOffice.Items.Clear();
            stdOffice.Items.AddRange(offices);
            stdOffice.SelectedIndex = ofc;

            stdOfficeSearch.Items.Clear();
            stdOfficeSearch.Items.AddRange(offices);
            stdOfficeSearch.SelectedIndex = ofc;

            officeComboBox.Items.Clear();
            officeComboBox.Items.AddRange(offices);
            officeComboBox.SelectedIndex = ofc;

            if (ofc != 0)
            {
                stdOffice.Enabled = false;
                stdOfficeCheckBox.Enabled = false;
                stdOfficeCheckBox.Checked = true;
                stdOfficeSearch.Enabled = false;
            }
        }

        private void FormImage_DoubleClick(object sender, EventArgs e)
        {
            if (officeComboBox.Visible)
            {
                if (officeComboBox.SelectedIndex >= 0)
                    DatabaseHelper.UpdateMetadataOffice(officeComboBox.SelectedIndex);
                GetOffice();
                officeComboBox.Visible = false;
                formTitle.Visible = true;
                return;
            }
            if (DatabaseHelper.CurrentOffice != 0)
                MessageBox.Show("لا يمكن للنسخ الفرعية استعمال هذه الخاصية");
            
            if (DatabaseHelper.CurrentOffice != 0 && (!File.Exists("password.log") || ComputeSha256Hash(File.ReadAllText("password.log")) != Secret.HashPassword))
                return;

            if (MessageBox.Show("هل انت متأكد أنك تريد تحويل هذه النسخة إلى نسخة فرعية ؟", "؟!?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            formTitle.Visible = false;
            officeComboBox.Visible = true;
        }

        private void StudentPanelTitle_DoubleClick(object sender, EventArgs e)
        {
            bool check = Properties.Settings.Default.CheckUpdate;
            Properties.Settings.Default.CheckUpdate = !check;
            Properties.Settings.Default.Save();
            MessageBox.Show("تم " + (check ? "تعطيل" : "تفعيل") + " فحص التحديثات عند فتح البرنامج");
        }

        static void DownloadUpdate()
        {
            GetAppUpdate update = new GetAppUpdate();
            bool hasUpdate = update.CheckForUpdates();
            if (hasUpdate && MessageBox.Show("هناك تحديث متوفر، هل تريد تحميله ؟", "🥳", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                bool updateDownloaded = update.GetTheUpdate();
                if (updateDownloaded) MessageBox.Show("تم تحميل التحديث بنجاح");
                else MessageBox.Show("لم يتم تحميل التحديث");
            }
        }

        private void CloseBtn_Click(object sender, EventArgs e)
            => Close();

        private void MinimizeBtn_Click(object sender, EventArgs e)
            => WindowState = FormWindowState.Minimized;

        FormSize fs = null;
        private void MaximizeBtn_Click(object sender, EventArgs e)
        {
            studentDataPanel.VerticalScroll.Value = 0;
            studentsListPanel.VerticalScroll.Value = 0;
            studentGradesListPanel.VerticalScroll.Value = 0;
            ranksListPanel.VerticalScroll.Value = 0;

            studentDataPanel.PerformLayout();
            studentsListPanel.PerformLayout();
            studentGradesListPanel.PerformLayout();
            ranksListPanel.PerformLayout();

            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                fs = new FormSize(NewSizeX, NewSizeY, SizeX, SizeY);
                fs.SetControls(Controls);
                fs = null;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                NewSizeX = Size.Width; NewSizeY = Size.Height;

                fs = new FormSize(SizeX, SizeY, NewSizeX, NewSizeY);
                fs.SetControls(Controls);

                //maximizeBtn.Visible = false;
                //minimizeBtn.Location = new Point(maximizeBtn.Location.X - maximizeBtn.Size.Width + minimizeBtn.Size.Width, minimizeBtn.Location.Y);
            }
            if (WindowState != FormWindowState.Minimized)
            {
                if (studentDataPanel.Visible) SetStudentImage();
                if (studentGradesPanel.Visible) SetStudentImage2();
            }
        }

        private void DataRecorderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!dataRecorderCheckBox.Checked && MessageBox.Show("هل انت متأكد أنك الجهاز الرئيسي الذي ستؤول إليه كل التسجيلات ؟", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) == DialogResult.No)
                dataRecorderCheckBox.Checked = true;
            else
            {
                Program.Record = dataRecorderCheckBox.Checked;
                if (!disableAtAll.Visible)
                {
                    Properties.Settings.Default.RecordEnabled = true;
                    Properties.Settings.Default.Save();
                }
                disableAtAll.Visible = true;
                disableAtAllCounter = 0;
            }
        }

        int disableAtAllCounter = 0;
        private void DisableAtAll_Click(object sender, EventArgs e)
        {
            if (Program.Record) return;
            disableAtAllCounter++;
            if (disableAtAllCounter < 5) return;
            disableAtAllCounter = 0;
            if (MessageBox.Show("هل تريد ابقاء هذا التعطيل مع كل مرة تفتح فيها البرنامج؟", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) == DialogResult.Yes)
            {
                Properties.Settings.Default.RecordEnabled = false;
                Properties.Settings.Default.Save();
                disableAtAll.Visible = false;
            }
        }

        private void ReadRecordsBtn_Click(object sender, EventArgs e)
        {
            if (selectDataFolderDialog.ShowDialog() != DialogResult.OK) return;

            string[] err = DatabaseHelper.ReadRecords(selectDataFolderDialog.SelectedPath);
            if (err is null)
                MessageBox.Show("لا يمكن تنفيذ هذا الأمر، أغلق البرنامج وأعد المحاولة مجددا", "تحذير !!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (err.Length == 0)
                MessageBox.Show("انتهى البرنامج بلا أخطاء والحمد لله", "🥳", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (err.Length > 0)
                MessageBox.Show($"الملفات التي حدث فيها أخطاء أثناء التنفيذ\n{string.Join("\n", err)}", "خطأ !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\'')
                e.Handled = true;
        }

        private void ErrorMessage() => MessageBox.Show("حدث خطأ غير معروف", "خطأ !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        #endregion

        #region Two Serach Panels
        private void AddTitleInStudentsListPanel()
        {
            studentsListPanel.Controls.Clear();
            studentsListPanel.Controls.Add(new StudentSearchRow { Location = new Point(9, 9) });
        }

        private void SearchPanelTitle_DoubleClick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.BackupEnabled)
            {
                Properties.Settings.Default.BackupEnabled = false;
                MessageBox.Show("تم تعطيل النسخ الاحتياطي");
            }
            else
            {
                Properties.Settings.Default.BackupEnabled = true;
                MessageBox.Show("تم تفعيل النسخ الاحتياطي");
            }
            Properties.Settings.Default.Save();
        }

        private void StdBirthDateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            stdBirthDateFromCheckBox.Enabled = stdBirthDateCheckBox.Checked;
            stdBirthDateToCheckBox.Enabled = stdBirthDateCheckBox.Checked;
            stdBirthDateSearch.Enabled = stdBirthDateCheckBox.Checked;

            if (!stdBirthDateCheckBox.Checked)
            {
                stdBirthDateFromCheckBox.Checked = false;
                stdBirthDateToCheckBox.Checked = false;
            }
        }

        private void StdBirthDateFromCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (stdBirthDateCheckBox.Checked)
                stdBirthDateSearch.Enabled = stdBirthDateFromCheckBox.Checked || !stdBirthDateToCheckBox.Checked;
        }

        private void StdBirthDateToCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            stdBirthDateToSearch.Enabled = stdBirthDateToCheckBox.Checked;
            StdBirthDateFromCheckBox_CheckedChanged(null, null);
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            string birth = null;
            if (stdBirthDateCheckBox.Checked)
            {
                if (stdBirthDateFromCheckBox.Checked || !stdBirthDateToCheckBox.Checked) birth = stdBirthDateSearch.Value.ToString("yyyy/MM/dd");
                if (stdBirthDateFromCheckBox.Checked || stdBirthDateToCheckBox.Checked) birth += "|";
                if (stdBirthDateToCheckBox.Checked) birth += stdBirthDateToSearch.Value.ToString("yyyy/MM/dd");
            }
            StudentSearchRowData[] students = DatabaseHelper.SelectStudents
                    (
                        undoubtedName: stdNameCheckBox.Checked ? stdNameSearch.Text : null,
                        nationalNumber: stdNationalCheckBox.Checked ? stdNationalSearch.Text : null,
                        birthDate: birth,
                        phoneNumber: stdPhoneCheckBox.Checked ? stdPhoneSearch.Text : null,
                        email: stdEmailCheckBox.Checked ? stdEmailSearch.Text : null
                    );

            if (students is null)
            {
                ErrorMessage();
                return;
            }

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

        private void UpdateStudentRow()
        {
            if (currentStudent != null)
            {
                StudentSearchRowData[] student = DatabaseHelper.SelectStudents(nationalNumber: currentStudent.StudentSearchRowData.NationalNumber);

                if (student.Length > 0)
                    currentStudent.SetData(student[0]);
            }
        }

        public void UpdateRankOfStudentRow(StudentGradeRow row, int rank)
        {
            var panel = studentGradesListPanel.Controls;
            if (panel.IndexOf(row) == panel.Count - 1)
                currentStudent.SetRank(rank);
        }

        StudentSearchRow currentStudent;
        private void ShowStudentBtn_Click(object sender, EventArgs e)
        {
            currentStudent = (StudentSearchRow)((Guna2Button)sender).Parent;
            string national = currentStudent.StudentSearchRowData.NationalNumber;
            StudentData stdData = DatabaseHelper.SelectStudent(national);
            if (stdData is null)
            {
                ErrorMessage();
                return;
            }

            SetStudentData(stdData);

            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            footerPanel.Visible = false;

            addStudentBtn.Text = "تعديل";
            stdNational.Enabled = false;
            stdBirthDate.Enabled = currentStudent.StudentSearchRowData.CompetitionDate is null;
            studentPanelState = StudentPanelState.Update;
            studentDataPanel.Visible = true;
        }

        private void ShowGradesBtn_Click(object sender, EventArgs e)
        {
            currentStudent = (StudentSearchRow)((Guna2Button)sender).Parent;
            StudentSearchRowData data = currentStudent.StudentSearchRowData;

            CompetitionGradeData[] gradesData = DatabaseHelper.SelectStudentGrades(data.NationalNumber);
            if (gradesData is null)
            {
                ErrorMessage();
                return;
            }

            OpenStudentGradesPanel(data, gradesData);
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
                alreadyExistsLabel.Visible = !wrongValueLabel.Visible && DatabaseHelper.SelectStudent(stdNational.Text) != null;
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

        private void StdImageLabel_DoubleClick(object sender, EventArgs e)
        {
            if (DatabaseHelper.RemoveOldImages())
                MessageBox.Show("تم تنظيف الصور القديمة");
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
            if (stdNational.Text.Length != 14 || wrongValueLabel.Visible || alreadyExistsLabel.Visible)
            {
                MessageBox.Show("أدخل رقم قومي صحيح");
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
                UpdateStudentRow();
            }
            else
                ErrorMessage();
        }

        private StudentData GetStudentData()
        {
            string frstConcDate = stdFirstConclusionCheckBox.Checked ?
                stdFirstConclusion.Value.ToString("yyyy/MM/dd") : "1900/01/01";

            return new StudentData
            {
                OfficeId = stdOffice.SelectedIndex,
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
                GuardianBirth = guardianBirth.Value.ToString("yyyy/MM/dd"),
                PhoneNumber = stdPhone.Text,
                Address = stdAddress.Text,
                Email = stdEmail.Text,
                Facebook = stdFacebook.Text,
                School = stdSchool.Text,
                Class = stdClass.Text,
                BrothersCount = (int)stdBrothers.Value,
                ArrangementBetweenBrothers = (int)stdArrangement.Value,
                MaritalStatus = stdMaritalStatus.Text,
                MemorizationAmount = stdMemo.Text,
                StudentMashaykh = stdMashaykh.Text,
                MemorizePlaces = stdMemoPlaces.Text,
                JoiningDate = stdJoiningDate.Value.ToString("yyyy/MM/dd"),
                FirstConclusionDate = frstConcDate,
                Certificates = stdCertificates.Text,
                Ijazah = stdIjazah.Text,
                Courses = stdCourses.Text,
                Skills = stdSkills.Text,
                Hobbies = stdHobbies.Text,
                Notes = stdNotes.Text,
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
            alreadyExistsLabel.Visible = false;

            SetStudentImage();
        }

        private void SetStudentDataAtStudentDataIsNull()
        {
            if (DatabaseHelper.CurrentOffice != 0)
                stdOffice.SelectedIndex = DatabaseHelper.CurrentOffice;

            stdName.Text = "";
            stdNational.Text = "";
            stdBirthDate.Value = DateTime.Now.AddYears(-10);
            stdBirthDate.Enabled = true;
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
            stdMaritalStatus.Text = "";
            stdMemo.Text = "";
            stdMashaykh.Text = "";
            stdMemoPlaces.Text = "";
            stdJoiningDate.Value = DateTime.Now.AddYears(-5);
            stdFirstConclusion.Checked = false;
            stdFirstConclusion.Value = DateTime.Now.AddYears(-2);
            stdCertificates.Text = "";
            stdIjazah.Text = "";
            stdCourses.Text = "";
            stdSkills.Text = "";
            stdHobbies.Text = "";
            stdNotes.Text = "";
            stdImagePath.Text = "";
        }

        private void SetStudentDataAtStudentDataIsNotNull(StudentData stdData)
        {
            stdOffice.SelectedIndex = stdData.OfficeId;
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
            stdMaritalStatus.Text = stdData.MaritalStatus;
            stdMemo.Text = stdData.MemorizationAmount;
            stdMashaykh.Text = stdData.StudentMashaykh;
            stdMemoPlaces.Text = stdData.MemorizePlaces;
            stdJoiningDate.Value = ParseExact(stdData.JoiningDate);
            stdFirstConclusion.Value = ParseExact(stdData.FirstConclusionDate);
            stdFirstConclusionCheckBox.Checked = stdFirstConclusion.Value > stdBirthDate.Value;
            stdCertificates.Text = stdData.Certificates;
            stdIjazah.Text = stdData.Ijazah;
            stdCourses.Text = stdData.Courses;
            stdSkills.Text = stdData.Skills;
            stdHobbies.Text = stdData.Hobbies;
            stdNotes.Text = stdData.Notes;
            stdImagePath.Text = stdData.Image;
        }

        private DateTime ParseExact(string date) => DateTime.ParseExact(date, "yyyy/MM/dd", DateTimeFormatInfo.CurrentInfo);

        private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ("\\/:*?\"<>|'".Contains(e.KeyChar))
                e.Handled = true;
        }

        private void StdFirstConclusionCheckBox_CheckedChanged(object sender, EventArgs e)
            => stdFirstConclusion.Enabled = stdFirstConclusionCheckBox.Checked;

        StudentPanelState studentPanelState = StudentPanelState.Read;

        private enum StudentPanelState
        {
            Add = 0,
            Update = 1,
            Read = 2,
        }
        #endregion

        #region Student Grades Panel

        public void DeleteStudentGradeRow(StudentGradeRow row)
        {
            if (MessageBox.Show("هل انت متأكد أنك تريد حذف هذه المسابقة لهذا الطالب ؟", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) != DialogResult.Yes) return;

            if (DatabaseHelper.DeleteStudentGrade(row.CompetitionGradeData) != -1)
            {
                var panel = studentGradesListPanel.Controls;
                int idx = panel.IndexOf(row);

                for (int i = idx + 1; i < panel.Count; i++)
                    if (panel[i].Top > panel[idx].Top)
                        panel[i].Top -= panel[idx].Height + (fs?.GetNewY(3) ?? 3);

                panel.RemoveAt(idx);

                compCount.Text = (int.Parse(compCount.Text) - 1).ToString();
                if (idx == panel.Count)
                {
                    UpdateStudentRow();
                    PrevCurrLevel();
                }
            }
            else
                MessageBox.Show("حدث خطأ غير معروف", "خطأ !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void PrevCurrLevel()
        {
            prevLevel.Minimum = 0; prevLevel.Maximum = 10;
            prevLevel.Value = currentStudent.StudentSearchRowData.CompetitionLevel ?? 0;
            SetPrevLevelMinMax();

            if (prevLevel.Value == 0) currentLevel.Value = 10;
            else if (prevLevel.Value > 2) currentLevel.Value = prevLevel.Value - 1;

            if (stdCodeState == 0)
                stdCode.Value = 0;
        }

        private void OpenStudentGradesPanel(StudentSearchRowData data, CompetitionGradeData[] grades)
        {
            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            footerPanel.Visible = false;

            compCount.Text = grades.Length.ToString();
            compDate.Value = DateTime.Now;

            stdName2.Text = data.FullName;
            stdNational2.Text = data.NationalNumber;

            stdImagePath2.Text = data.Image;
            SetStudentImage2();

            PrevCurrLevel();

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

        private void CompDateLabel_DoubleClick(object sender, EventArgs e)
            => compDate.Value = DateTime.Now;

        private void CompDate_ValueChanged(object sender, EventArgs e)
        {
            float year = 0;
            if (DateTime.TryParseExact(currentStudent.StudentSearchRowData.BirthDate, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthDate))
                stdAge.Text = AgeCalculator.GetAgeDescription(birthDate, compDate.Value, out year);
            else
                stdAge.Text = "تاريخ الميلاد غير صالح";
            stdAge.Tag = year;
        }

        bool showMessageAtStdCodeIsZero = true, showSureMessage = true;
        private void AddGradeBtn_Click(object sender, EventArgs e)
        {
            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("لا يمكن إضافة مسابقات إلا من النسخة الرئيسية");
                return;
            }

            string date = currentStudent.StudentSearchRowData.CompetitionDate;
            string newDate = compDate.Value.ToString("yyyy/MM");
            bool? allowedDate;
            if (date is null)
                allowedDate = true;
            else if (date.CompareTo(newDate) > 0)
                allowedDate = (bool?)null;
            else
                allowedDate = date != newDate;

            CompetitionGradeData gData; int topRank = 0;
            for (int i = 1; i < studentGradesListPanel.Controls.Count; i++)
            {
                gData = ((StudentGradeRow)studentGradesListPanel.Controls[i]).CompetitionGradeData;
                if (gData.Rank >= 1 && gData.Rank <= 3 && gData.CompetitionLevel == 1) topRank += 1;
            }

            if (allowedDate is null)
            {
                MessageBox.Show("هذا الطالب أضيفت له مسابقة تاريخها أكبر من هذا التاريخ", "تحذير !!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!(bool)allowedDate)
            {
                MessageBox.Show("لقد أضفت بالفعل مسابقة لهذا الطالب في هذا الشهر");
                return;
            }
            if ((float)stdAge.Tag < 0)
            {
                MessageBox.Show("هذا الطالب لم يُولَد بعد");
                return;
            }
            if ((float)stdAge.Tag < 1f)
            {
                MessageBox.Show("هذا الطالب وُلِد للتو");
                return;
            }

            if (topRank >= 2 && MessageBox.Show("حصل هذا الطالب في المستوى الأول على أحد المراكز الثلاثة الأولى أكثر من مرة", "؟!?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel) return;

            if ((float)stdAge.Tag > 35 && MessageBox.Show("هذا الطالب عمره أكبر من 35 عاما", "؟!?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel) return;

            if ((float)stdAge.Tag > 25 && currentLevel.Value != 1 && MessageBox.Show("هذا الطالب عمره أكبر من 25 عاما ومستوى المسابقة أقل من الأول", "؟!?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel) return;

            if (stdCode.Value == 0 && showMessageAtStdCodeIsZero)
            {
                if (MessageBox.Show("هل أنت متأكد أن كود المسابقة صفر", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) == DialogResult.Yes)
                {
                    if (showSureMessage)
                    {
                        DialogResult res = MessageBox.Show("هل تريد تذكر هذا الاختيار ؟", "تنبيه !!!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                        if (res == DialogResult.Yes)
                            showMessageAtStdCodeIsZero = false;
                        else if (res == DialogResult.No)
                            showSureMessage = false;
                        else if (res == DialogResult.Cancel)
                            return;
                    }
                }
                else
                    return;
            }

            CompetitionGradeData data = new CompetitionGradeData
            {
                NationalNumber = stdNational2.Text,
                StudentCode = (int)stdCode.Value,
                PreviousLevel = (int)prevLevel.Value,
                CompetitionLevel = (int)currentLevel.Value,
                CompetitionDate = compDate.Value.ToString("yyyy/MM"),
                Score = (float)stdScore.Value,
                Rank = (int)stdRank.Value,
                Notes = compNotes.Text
            };

            if (DatabaseHelper.AddGrade(data) == -1)
            {
                ErrorMessage();
                return;
            }

            Control lastControl = studentGradesListPanel.Controls[studentGradesListPanel.Controls.Count - 1];
            StudentGradeRow stdRow = new StudentGradeRow(data) { Location = new Point((fs?.GetNewX(30) ?? 30), lastControl.Bottom + (fs?.GetNewY(3) ?? 3)) };
            fs?.SetControl(stdRow, loc: false);
            fs?.SetControls(stdRow.Controls);
            studentGradesListPanel.Controls.Add(stdRow);
            compCount.Text = (int.Parse(compCount.Text) + 1).ToString();
            UpdateStudentRow();
            PrevCurrLevel();
            if (stdCodeState == 2)
                stdCode.Value += 1;
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

        int stdCodeState = 0;
        private void StdCodeLabel_DoubleClick(object sender, EventArgs e)
        {
            if (stdCodeState == 0)
            {
                DialogResult res = MessageBox.Show("إذا أردت أن يزيد الكود تسلسليا بعد كل اضافة اضغط نعم\nإذا أردت فقط منع تصفير رقم الكود كل مرة اضغط لا", "الخيار خيارك", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button3);
                if (res == DialogResult.No)
                    stdCodeState = 1;
                else if (res == DialogResult.Yes)
                    stdCodeState = 2;
            }
            else
            {
                stdCodeState = 0;
                MessageBox.Show("تم استعادة الخيار الافتراضي");
            }
        }
        #endregion

        #region Ranks Calculator Panel
        private void CloseBtn2_Click(object sender, EventArgs e)
        {
            ranksCalculatorPanel.Visible = false;

            studentSearchPanel.Visible = true;
            studentsListPanel.Visible = true;
            footerPanel.Visible = true;
        }

        private void GetGradesDataBtn_Click(object sender, EventArgs e)
        {
            CompetitionRankData[] ranks = DatabaseHelper.SelectCompetitionRanks((int)compLevel.Value, compDateFrom.Value.ToString("yyyy/MM"), compDateTo.Value.ToString("yyyy/MM"));
            if (ranks is null)
            {
                ErrorMessage();
                return;
            }

            levelCompCount.Text = ranks.Length.ToString();

            wrongThingLabel.Visible = false;
            wrongThing2Label.Visible = false;

            for (int i = 0; i < ranks.Length - 1; i++)
            {
                for (int j = i + 1; j < ranks.Length; j++)
                {
                    if (ranks[i].NationalNumber == ranks[j].NationalNumber)
                    {
                        wrongThingLabel.Visible = true;
                        break;
                    }
                    if (ranks[i].StudentCode != 0 && ranks[i].StudentCode == ranks[j].StudentCode)
                    {
                        wrongThing2Label.Visible = true;
                        break;
                    }
                }
            }

            ranksListPanel.Controls.Clear();
            ranksListPanel.Controls.Add(new StudentRankRow { Location = new Point(30, 9) });

            StudentRankRow stdRow;
            for (int i = 0; i < ranks.Length; i++)
            {
                stdRow = new StudentRankRow(ranks[i]);
                stdRow.Location = new Point(30, (stdRow.Size.Height + 3) * (i + 1) + 9);
                ranksListPanel.Controls.Add(stdRow);
            }
            fs?.SetControls(ranksListPanel.Controls);
        }

        private void SetRanksBtn_Click(object sender, EventArgs e)
        {
            if (wrongThingLabel.Visible || wrongThing2Label.Visible)
            {
                MessageBox.Show("لا يمكن استعمال هذا الزر مع وجود تكرار طلبة أو أكواد في القائمة", "تنبيه !!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            float scr = 99999; int rnk = 0; StudentRankRow row;
            for (int i = 1; i < ranksListPanel.Controls.Count; i++)
            {
                row = (StudentRankRow)ranksListPanel.Controls[i];

                if (row.CompetitionRankData.Score < scr)
                {
                    ++rnk;
                    scr = row.CompetitionRankData.Score;
                }
                if (row.CompetitionRankData.Score == scr)
                    row.StudentRank.Value = rnk;
            }
        }
        #endregion

        #region Footer Panel
        private void OpenAddStudentBtn_Click(object sender, EventArgs e)
        {
            SetStudentData(null);

            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            footerPanel.Visible = false;

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
            if (rows is null)
            {
                ErrorMessage();
                return;
            }

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

                int[] sheetsRowIndexes = new int[11] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
                for (int i = 0; i < rows.Length; i++)
                {
                    int sheetIndex = rows[i].CompetitionLevel;
                    SetDataOnExcelFile(sheets[sheetIndex], ref sheetsRowIndexes[sheetIndex], rows[i]);
                    SetDataOnExcelFile(sheets[0], ref sheetsRowIndexes[0], rows[i]);
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

        private void RankCalcBtn_Click(object sender, EventArgs e)
        {
            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("لا يمكن الدخول على هذه الشاشة إلا من النسخة الرئيسية");
                return;
            }

            int count = DatabaseHelper.GetStudentCount();
            if (count == -1)
            {
                ErrorMessage();
                return;
            }
            studentCount.Text = count.ToString();

            studentSearchPanel.Visible = false;
            studentsListPanel.Visible = false;
            footerPanel.Visible = false;

            ranksListPanel.Controls.Clear();
            ranksListPanel.Controls.Add(new StudentRankRow { Location = new Point(30, 9) });
            fs?.SetControls(ranksListPanel.Controls);

            compDateFrom.Value = DateTime.Now;
            compDateTo.Value = DateTime.Now;
            wrongThingLabel.Visible = false;
            wrongThing2Label.Visible = false;
            levelCompCount.Text = "0";

            ranksCalculatorPanel.Visible = true;
        }

        private void HideOfficeTools()
        {
            officeTextBox.Visible = false;
            officeEnterBtn.Visible = false;
            releasesLatestBtn.Visible = true;
            excelDateFilter.Visible = excelRowsFilter.SelectedIndex == 2 || excelRowsFilter.SelectedIndex == 4;
            excelRowsFilter.Visible = true;
            extractExcelBtn.Visible = true;
        }

        private void OfficeHelperBtn_Click(object sender, EventArgs e)
        {
            if (officeEnterBtn.Visible)
            {
                HideOfficeTools();
                return;
            }

            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("لا يمكن للنسخ الفرعية استعمال هذه الخاصية");
                return;
            }

            if (MessageBox.Show("هل انت متأكد أنك على النسخة الرئيسية ؟", "؟!?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            releasesLatestBtn.Visible = false;
            excelDateFilter.Visible = false;
            excelRowsFilter.Visible = false;
            extractExcelBtn.Visible = false;
            officeTextBox.Visible = true;
            officeEnterBtn.Visible = true;
        }

        private void OfficeEnterBtn_Click(object sender, EventArgs e)
        {
            if (officeTextBox.Text == "")
                return;

            if (offices.Contains(officeTextBox.Text))
            {
                MessageBox.Show("تم إدخال هذه المكتبة من قبل");
                return;
            }

            DatabaseHelper.AddOffice(officeTextBox.Text, "");

            GetOffice();
            officeTextBox.Text = "";
        }

        private void ReleasesLatestBtn_Click(object sender, EventArgs e)
            => Process.Start("https://github.com/MohamedAshref371/little-hafiz/releases/latest");
        #endregion


        static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
    }
}

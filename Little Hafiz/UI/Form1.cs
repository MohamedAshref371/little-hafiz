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

            bool isExists = Directory.Exists("data");
            if (isExists)
                Program.Record = Properties.Settings.Default.RecordEnabled;

            dataRecorderCheckBox.Checked = Program.Record;
            disableAtAll.Visible = Program.Record;
            dataRecorderCheckBox.CheckedChanged += DataRecorderCheckBox_CheckedChanged;

            if (isExists)
                GetOffice();
            else
            {
                offices = new string[] { Application.ProductName };
                AfterGetOffice(0);
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

            AfterGetOffice(DatabaseHelper.CurrentOffice);
        }

        private void AfterGetOffice(int ofc)
        {
            formTitle.Text = offices[ofc];

            offices[0] = "اختر من القائمة";
            stdOffice.Items.Clear();
            stdOffice.Items.AddRange(offices);
            stdOffice.SelectedIndex = ofc;

            stdOfficeSearch.Items.Clear();
            stdOfficeSearch.Items.AddRange(offices);
            stdOfficeSearch.SelectedIndex = ofc;

            officeComboBox.Items.Clear();
            officeComboBox.Items.AddRange(offices);
            officeComboBox.SelectedIndex = ofc;

            officeRank.Items.Clear();
            officeRank.Items.AddRange(offices);
            officeRank.SelectedIndex = ofc;

            bool equalZero = ofc == 0;
            stdOffice.Enabled = equalZero;
            stdOfficeCheckBox.Checked = !equalZero;
            stdOfficeCheckBox.Enabled = equalZero;
            stdOfficeSearch.Enabled = equalZero;
            officeRank.Enabled = equalZero;
            if (!equalZero)
            {
                if (ranksCalculatorPanel.Visible)
                    CloseBtn2_Click(null, null);

                if (officeEnterBtn.Visible)
                    HideOfficeTools();
            }
        }

        private void FormImage_DoubleClick(object sender, EventArgs e)
        {
            if (officeComboBox.Visible)
            {
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

            if (MessageBox.Show("هل انت متأكد أنك تريد تحويل هذه النسخة إلى نسخة فرعية ؟\nلن تستطيع تحويلها الى نسخة رئيسية مجددا", "؟!?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            formTitle.Visible = false;
            officeComboBox.Visible = true;
        }

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

            int height = 30;
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                fs = new FormSize(NewSizeX, NewSizeY, SizeX, SizeY);
                fs.SetControls(Controls);
                fs = null;
                ResetComponent();
            }
            else if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                NewSizeX = Size.Width; NewSizeY = Size.Height;
                fs = new FormSize(SizeX, SizeY, NewSizeX, NewSizeY);
                fs.SetControls(Controls);
                height = fs.GetNewY(30);
            }
            if (WindowState != FormWindowState.Minimized)
            {
                officeComboBox.ItemHeight = height;
                stdOffice.ItemHeight = height;
                stdOfficeSearch.ItemHeight = height;
                officeRank.ItemHeight = height;
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
            if (MessageBox.Show("هل انت متأكد أنك الجهاز الرئيسي الذي ستؤول إليه كل التسجيلات ؟", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) == DialogResult.No)
                return;
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
                        email: stdEmailCheckBox.Checked ? stdEmailSearch.Text : null,
                        office: stdOfficeCheckBox.Checked ? stdOfficeSearch.SelectedIndex : (int?)null
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
            deleteStudentBtn.Visible = stdBirthDate.Enabled && DatabaseHelper.CurrentOffice == 0;
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
            studentDataPanel.VerticalScroll.Value = 0;
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
            if (stdOffice.SelectedIndex == 0 && MessageBox.Show("إذا لم تقم باختيار مكتب للطالب، سيتم وضعه في مكتب مزيف\nهل تريد الاستمرار ؟", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) == DialogResult.No)
                return;
            
            if (studentPanelState == StudentPanelState.Add && DatabaseHelper.AddStudent(GetStudentData()) != -1)
            {
                stdNationalCheckBox.Checked = true;
                stdNationalSearch.Text = stdNational.Text;
                stdNameCheckBox.Checked = false;
                stdPhoneCheckBox.Checked = false;
                stdEmailCheckBox.Checked = false;
                stdBirthDateCheckBox.Checked = false;
                if (DatabaseHelper.CurrentOffice == 0)
                    stdOfficeCheckBox.Checked = false;
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

        private void DeleteStudentBtn_Click(object sender, EventArgs e)
        {
            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("حذف الطلاب مسموح فقط للنسخة الرئيسية");
                return;
            }
            if (stdNational.Enabled)
                return;
            if (!stdBirthDate.Enabled)
            {
                MessageBox.Show("هذا الطالب له مسابقات مسجلة في البرنامج");
                return;
            }
            if (MessageBox.Show("هل انت متأكد أنك تريد حذف هذا الطالب ؟", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) != DialogResult.Yes)
                return;

            if (DatabaseHelper.DeleteStudentPermanently(stdNational.Text) != -1)
            {
                MessageBox.Show("تم حذف الطالب بنجاح");
                CancelBtn_Click(null, null);
                currentStudent.Enabled = false;
            }
            else
                ErrorMessage();
        }

        private void PrintStudentBtn_Click(object sender, EventArgs e)
        {
            StudentFormData data = new StudentFormData
            {
                PaperTitle = studentPanelTitle.Text,

                FullName = stdNameLabel.Text + stdName.Text,
                NationalNumber = stdNationalLabel.Text + stdNational.Text,
                BirthDate = birthLabel.Text + stdBirthDate.Value.ToString("yyyy/MM/dd"),
                Job = jobLabel.Text + stdJob.Text,

                FatherQualification = fatherQualiLabel.Text + fatherQuali.Text,
                MotherQualification = motherQualiLabel.Text + motherQuali.Text,
                FatherJob = fatherJobLabel.Text + fatherJob.Text,
                MotherJob = motherJobLabel.Text + motherJob.Text,
                FatherPhone = fatherPhoneLabel.Text + fatherPhone.Text,
                MotherPhone = motherPhoneLabel.Text + motherPhone.Text,
                GuardianName = guardianNameLabel.Text + guardianName.Text,
                GuardianLink = guardianLinkLabel.Text + guardianLink.Text,
                GuardianBirth = guardianBirthLabel.Text + guardianBirth.Value.ToString("yyyy/MM/dd"),

                PhoneNumber = stdPhoneLabel.Text + stdPhone.Text,
                Address = stdAddressLabel.Text + stdAddress.Text,
                Email = stdEmailLabel.Text + stdEmail.Text,
                Facebook = stdFacebookLabel.Text + stdFacebook.Text,
                School = stdSchoolLabel.Text + stdSchool.Text,
                Class = stdClassLabel.Text + stdClass.Text,

                BrothersCount = stdBrothersLabel.Text + Ranks.ConvertNumberToCount((int)stdBrothers.Value),
                ArrangementBetweenBrothers = stdArrangementLabel.Text + Ranks.ConvertNumberToRank((int)stdArrangement.Value),
                MaritalStatus = stdMaritalStatusLabel.Text + stdMaritalStatus.Text,

                MemorizationAmount = stdMemoLabel.Text + stdMemo.Text,
                OfficeName = stdOfficeLabel.Text + stdOffice.Text,
                JoiningDate = stdJoiningDateLabel.Text + stdJoiningDate.Value.ToString("yyyy/MM/dd"),
                FirstConclusionDate = stdFirstConclusionCheckBox.Text + (stdFirstConclusionCheckBox.Checked ? stdFirstConclusion.Value.ToString("yyyy/MM/dd") : "لا توجد"),

                StudentMashaykh = stdMashaykhLabel.Text + stdMashaykh.Text,
                MemorizePlaces = stdMemoPlacesLabel.Text + stdMemoPlaces.Text,
                Certificates = stdCertificatesLabel.Text + stdCertificates.Text,
                Ijazah = stdIjazahLabel.Text + stdIjazah.Text,
                Courses = stdCoursesLabel.Text + stdCourses.Text,
                Skills = stdSkillsLabel.Text + stdSkills.Text,
                Hobbies = stdHobbiesLabel.Text + stdHobbies.Text,
                Notes = stdNotesLabel.Text + stdNotes.Text,

                StudentImage = studentImage.Image ?? new Bitmap(studentImage.Size.Width, studentImage.Size.Height)
            };

            StudentFormPrinter printer = new StudentFormPrinter(data);
            printer.ShowPreview();
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
            deleteStudentBtn.Visible = false;
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
            setRanksBtn.Enabled = officeRank.SelectedIndex == 0;

            CompetitionRankData[] ranks = DatabaseHelper.SelectCompetitionRanks((int)compLevel.Value, compDateFrom.Value.ToString("yyyy/MM"), compDateTo.Value.ToString("yyyy/MM"), officeRank.SelectedIndex);
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

            StudentRankRow.ReadOnly = DatabaseHelper.CurrentOffice != 0;
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

        private void OfficeRank_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ranksCalculatorPanel.Visible) return;
            int idx = officeRank.SelectedIndex;
            int count = DatabaseHelper.GetStudentCount(idx);
            studentCountLabel.Text = idx == 0 ? "عدد الطلبة الكلي : " : "عدد طلاب المكتب : ";
            studentCount.Text = count.ToString();
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

        private void ExtractExcelBtn_Click(object sender, EventArgs e)
        {
            if (!File.Exists("ClosedXML.dll"))
            {
                MessageBox.Show("مكتبات الايكسل غير موجودة");
                return;
            }
            if (saveExcelFileDialog.ShowDialog() != DialogResult.OK) return;

            ExcelRowData[] rows = DatabaseHelper.SelectExcelRowData(DateTime.Now.Year, 0, stdOfficeSearch.SelectedIndex);
            if (rows is null)
            {
                ErrorMessage();
                return;
            }

            ExtractExcel(rows);
        }

        private void ExtractExcel(ExcelRowData[] rows)
        {
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
            sheet.Range("A1:M1").Merge().Value = "الحافظ الصغير- المسابقة القرآنية الرمضانية";
            sheet.Row(1).Height = 30;
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 16;
            sheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Column(1).Width = 7;
            sheet.Cell(2, 1).Value = "م";

            sheet.Column(2).Width = 7;
            sheet.Cell(2, 2).Value = "الكود";

            sheet.Column(3).Width = 20;
            sheet.Cell(2, 3).Value = "الاسم";

            sheet.Column(4).Width = 15;
            sheet.Cell(2, 4).Value = "الرقم القومي";

            sheet.Column(5).Width = 15;
            sheet.Cell(2, 5).Value = "تاريخ الميلاد";

            sheet.Column(6).Width = 15;
            sheet.Cell(2, 6).Value = "رقم التليفون";

            sheet.Column(7).Width = 7;
            sheet.Cell(2, 7).Value = "الحالي";

            sheet.Column(8).Width = 7;
            sheet.Cell(2, 8).Value = "السابق";

            sheet.Column(9).Width = 15;
            sheet.Cell(2, 9).Value = "الصف";

            sheet.Column(10).Width = 15;
            sheet.Cell(2, 10).Value = "العنوان";

            sheet.Column(11).Width = 15;
            sheet.Cell(2, 11).Value = "المكتب";

            sheet.Column(12).Width = 7;
            sheet.Cell(2, 12).Value = "المركز";

            sheet.Column(13).Width = 14;
            sheet.Cell(2, 13).Value = "تاريخ إضافة المسابقة";
        }

        private void SetDataOnExcelFile(IXLWorksheet sheet, ref int row, ExcelRowData data)
        {
            sheet.Cell(row, 1).Value = (row - 2).ToString();
            sheet.Cell(row, 2).Value = data.StudentCode;
            sheet.Cell(row, 3).Value = data.FullName;
            sheet.Cell(row, 4).Value = data.NationalNumber;
            sheet.Cell(row, 5).Value = data.BirthDate;
            sheet.Cell(row, 6).Value = data.PhoneNumber;
            sheet.Cell(row, 7).Value = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            sheet.Cell(row, 8).Value = Ranks.ConvertNumberToRank(data.PreviousLevel);
            sheet.Cell(row, 9).Value = data.Class;
            sheet.Cell(row, 10).Value = data.Address;
            sheet.Cell(row, 11).Value = data.Office == 0 ? "غير معروف" : offices[data.Office];
            sheet.Cell(row, 12).Value = data.Rank;
            sheet.Cell(row, 13).Value = data.CompetitionAddedDate;
            row++;
        }

        private void RankCalcBtn_Click(object sender, EventArgs e)
        {
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
            OfficeRank_SelectedIndexChanged(null, null);
        }

        private void HideOfficeTools()
        {
            officeTextBox.Visible = false;
            officeEnterBtn.Visible = false;
            checkUpdateBtn.Visible = true;
            releasesLatestBtn.Visible = true;
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

            checkUpdateBtn.Visible = false;
            releasesLatestBtn.Visible = false;
            extractExcelBtn.Visible = false;
            officeTextBox.Visible = true;
            officeEnterBtn.Visible = true;
        }

        private void OfficeEnterBtn_Click(object sender, EventArgs e)
        {
            if (officeTextBox.Text == "")
                return;

            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("لا يمكن للنسخ الفرعية إضافة مكاتب");
                return;
            }
            
            if (offices.Contains(officeTextBox.Text))
            {
                MessageBox.Show("تم إدخال هذا المكتب من قبل");
                return;
            }

            DatabaseHelper.AddOffice(officeTextBox.Text, "");

            GetOffice();
            officeTextBox.Text = "";
        }

        private void CheckUpdateBtn_Click(object sender, EventArgs e)
        {
            GetAppUpdate update = new GetAppUpdate();
            bool? hasUpdate = update.CheckForUpdates();

            if (hasUpdate is null)
            {
                MessageBox.Show("تحقق من الاتصال بالانترنت");
                return;
            }
            if (!(bool)hasUpdate)
            {
                MessageBox.Show("البرنامج محدث");
                return;
            }

            if (MessageBox.Show("هناك تحديث متوفر، هل تريد تحميله ؟", "🥳", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                bool? updateDownloaded = update.GetTheUpdate();
                if (updateDownloaded is null) MessageBox.Show("فشل تحميل ملف التحديث");
                else if (!(bool)updateDownloaded) MessageBox.Show("هناك ملف موجود بنفس الإسم، لا يمكن التحميل");
                else MessageBox.Show("تم تحميل ملف التحديث بنجاح");
            }
        }

        private void ReleasesLatestBtn_Click(object sender, EventArgs e)
            => Process.Start("https://github.com/MohamedAshref371/little-hafiz/releases/latest");
        #endregion

        #region Reset Component
        private void ResetComponent()
        {
            closeBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            closeBtn.Location = new Point(15, 4);
            closeBtn.Size = new System.Drawing.Size(32, 30);
            minimizeBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            minimizeBtn.Location = new Point(86, 12);
            minimizeBtn.Size = new System.Drawing.Size(35, 14);
            studentDataPanel.Location = new Point(12, 37);
            studentDataPanel.Size = new System.Drawing.Size(934, 656);
            deleteStudentBtn.Font = new Font("Segoe UI", 16F);
            deleteStudentBtn.Location = new Point(18, 1401);
            deleteStudentBtn.Size = new System.Drawing.Size(158, 45);
            scrollHelperLabel.Location = new Point(718, 1440);
            scrollHelperLabel.Size = new System.Drawing.Size(0, 13);
            printStudentBtn.Font = new Font("Segoe UI", 16F);
            printStudentBtn.Location = new Point(255, 1401);
            printStudentBtn.Size = new System.Drawing.Size(140, 45);
            stdOffice.Font = new Font("Segoe UI", 10F);
            stdOffice.Location = new Point(536, 140);
            stdOffice.Size = new System.Drawing.Size(257, 36);
            stdNotes.Font = new Font("Segoe UI", 12F);
            stdNotes.Location = new Point(13, 1291);
            stdNotes.Size = new System.Drawing.Size(822, 96);
            stdNotesLabel.Font = new Font("Tahoma", 12F);
            stdNotesLabel.Location = new Point(832, 1297);
            stdNotesLabel.Size = new System.Drawing.Size(85, 19);
            studentImage.Location = new Point(43, 6);
            studentImage.Size = new System.Drawing.Size(140, 180);
            stdImageSelectorBtn.Font = new Font("Segoe UI", 12F);
            stdImageSelectorBtn.Location = new Point(204, 103);
            stdImageSelectorBtn.Size = new System.Drawing.Size(108, 32);
            stdImageLabel.Font = new Font("Tahoma", 12F);
            stdImageLabel.Location = new Point(191, 58);
            stdImageLabel.Size = new System.Drawing.Size(145, 19);
            stdSkills.Font = new Font("Segoe UI", 12F);
            stdSkills.Location = new Point(481, 1187);
            stdSkills.Size = new System.Drawing.Size(354, 96);
            stdHobbies.Font = new Font("Segoe UI", 12F);
            stdHobbies.Location = new Point(13, 1187);
            stdHobbies.Size = new System.Drawing.Size(354, 96);
            stdHobbiesLabel.Font = new Font("Tahoma", 12F);
            stdHobbiesLabel.Location = new Point(368, 1194);
            stdHobbiesLabel.Size = new System.Drawing.Size(76, 19);
            stdIjazah.Font = new Font("Segoe UI", 12F);
            stdIjazah.Location = new Point(481, 1083);
            stdIjazah.Size = new System.Drawing.Size(354, 96);
            stdCourses.Font = new Font("Segoe UI", 12F);
            stdCourses.Location = new Point(13, 1083);
            stdCourses.Size = new System.Drawing.Size(354, 96);
            stdSkillsLabel.Font = new Font("Tahoma", 12F);
            stdSkillsLabel.Location = new Point(837, 1194);
            stdSkillsLabel.Size = new System.Drawing.Size(76, 19);
            stdCertificates.Font = new Font("Segoe UI", 12F);
            stdCertificates.Location = new Point(13, 979);
            stdCertificates.Size = new System.Drawing.Size(723, 96);
            stdCertificatesLabel.Font = new Font("Tahoma", 12F);
            stdCertificatesLabel.Location = new Point(733, 985);
            stdCertificatesLabel.Size = new System.Drawing.Size(179, 19);
            stdFirstConclusion.Font = new Font("Segoe UI", 9F);
            stdFirstConclusion.Location = new Point(41, 698);
            stdFirstConclusion.Size = new System.Drawing.Size(193, 36);
            stdJoiningDate.Font = new Font("Segoe UI", 9F);
            stdJoiningDate.Location = new Point(540, 698);
            stdJoiningDate.Size = new System.Drawing.Size(193, 36);
            stdJoiningDateLabel.Font = new Font("Tahoma", 12F);
            stdJoiningDateLabel.Location = new Point(735, 707);
            stdJoiningDateLabel.Size = new System.Drawing.Size(169, 19);
            stdMemoPlaces.Font = new Font("Segoe UI", 12F);
            stdMemoPlaces.Location = new Point(13, 875);
            stdMemoPlaces.Size = new System.Drawing.Size(722, 96);
            stdMashaykh.Font = new Font("Segoe UI", 12F);
            stdMashaykh.Location = new Point(13, 771);
            stdMashaykh.Size = new System.Drawing.Size(722, 96);
            stdMemoPlacesLabel.Font = new Font("Tahoma", 12F);
            stdMemoPlacesLabel.Location = new Point(730, 881);
            stdMemoPlacesLabel.Size = new System.Drawing.Size(175, 19);
            stdMashaykhLabel.Font = new Font("Tahoma", 12F);
            stdMashaykhLabel.Location = new Point(735, 778);
            stdMashaykhLabel.Size = new System.Drawing.Size(170, 19);
            stdMemo.Font = new Font("Segoe UI", 12F);
            stdMemo.Location = new Point(43, 654);
            stdMemo.Size = new System.Drawing.Size(257, 32);
            stdMemoLabel.Font = new Font("Tahoma", 12F);
            stdMemoLabel.Location = new Point(316, 659);
            stdMemoLabel.Size = new System.Drawing.Size(104, 19);
            stdMaritalStatus.Font = new Font("Segoe UI", 12F);
            stdMaritalStatus.Location = new Point(536, 654);
            stdMaritalStatus.Size = new System.Drawing.Size(257, 32);
            stdMaritalStatusLabel.Font = new Font("Tahoma", 12F);
            stdMaritalStatusLabel.Location = new Point(783, 659);
            stdMaritalStatusLabel.Size = new System.Drawing.Size(134, 19);
            stdArrangementLabel.Font = new Font("Tahoma", 12F);
            stdArrangementLabel.Location = new Point(276, 614);
            stdArrangementLabel.Size = new System.Drawing.Size(144, 19);
            stdArrangement.Font = new Font("Segoe UI", 12F);
            stdArrangement.Location = new Point(117, 607);
            stdArrangement.Size = new System.Drawing.Size(100, 32);
            stdBrothersLabel.Font = new Font("Tahoma", 12F);
            stdBrothersLabel.Location = new Point(811, 614);
            stdBrothersLabel.Size = new System.Drawing.Size(95, 19);
            stdBrothers.Font = new Font("Segoe UI", 12F);
            stdBrothers.Location = new Point(615, 607);
            stdBrothers.Size = new System.Drawing.Size(100, 32);
            stdSchool.Font = new Font("Segoe UI", 12F);
            stdSchool.Location = new Point(536, 568);
            stdSchool.Size = new System.Drawing.Size(257, 32);
            stdClass.Font = new Font("Segoe UI", 12F);
            stdClass.Location = new Point(43, 568);
            stdClass.Size = new System.Drawing.Size(257, 32);
            stdClassLabel.Font = new Font("Tahoma", 12F);
            stdClassLabel.Location = new Point(294, 575);
            stdClassLabel.Size = new System.Drawing.Size(126, 19);
            stdEmail.Font = new Font("Segoe UI", 12F);
            stdEmail.Location = new Point(536, 528);
            stdEmail.Size = new System.Drawing.Size(257, 32);
            stdFacebook.Font = new Font("Segoe UI", 12F);
            stdFacebook.Location = new Point(43, 528);
            stdFacebook.Size = new System.Drawing.Size(257, 32);
            stdFacebookLabel.Font = new Font("Tahoma", 12F);
            stdFacebookLabel.Location = new Point(306, 535);
            stdFacebookLabel.Size = new System.Drawing.Size(114, 19);
            stdEmailLabel.Font = new Font("Tahoma", 12F);
            stdEmailLabel.Location = new Point(834, 535);
            stdEmailLabel.Size = new System.Drawing.Size(71, 19);
            stdAddress.Font = new Font("Segoe UI", 12F);
            stdAddress.Location = new Point(43, 488);
            stdAddress.Size = new System.Drawing.Size(257, 32);
            stdAddressLabel.Font = new Font("Tahoma", 12F);
            stdAddressLabel.Location = new Point(350, 495);
            stdAddressLabel.Size = new System.Drawing.Size(70, 19);
            stdPhone.Font = new Font("Segoe UI", 12F);
            stdPhone.Location = new Point(536, 488);
            stdPhone.Size = new System.Drawing.Size(257, 32);
            guardianLink.Font = new Font("Segoe UI", 12F);
            guardianLink.Location = new Point(330, 431);
            guardianLink.Size = new System.Drawing.Size(86, 32);
            guardianBirth.Font = new Font("Segoe UI", 9F);
            guardianBirth.Location = new Point(43, 431);
            guardianBirth.Size = new System.Drawing.Size(136, 32);
            guardianName.Font = new Font("Segoe UI", 12F);
            guardianName.Location = new Point(536, 431);
            guardianName.Size = new System.Drawing.Size(257, 32);
            motherPhone.Font = new Font("Segoe UI", 12F);
            motherPhone.Location = new Point(43, 387);
            motherPhone.Size = new System.Drawing.Size(257, 32);
            fatherPhone.Font = new Font("Segoe UI", 12F);
            fatherPhone.Location = new Point(536, 387);
            fatherPhone.Size = new System.Drawing.Size(257, 32);
            motherJob.Font = new Font("Segoe UI", 12F);
            motherJob.Location = new Point(43, 347);
            motherJob.Size = new System.Drawing.Size(257, 32);
            motherQuali.Font = new Font("Segoe UI", 12F);
            motherQuali.Location = new Point(536, 347);
            motherQuali.Size = new System.Drawing.Size(257, 32);
            fatherJob.Font = new Font("Segoe UI", 12F);
            fatherJob.Location = new Point(43, 307);
            fatherJob.Size = new System.Drawing.Size(257, 32);
            fatherQuali.Font = new Font("Segoe UI", 12F);
            fatherQuali.Location = new Point(536, 307);
            fatherQuali.Size = new System.Drawing.Size(257, 32);
            cancelBtn.Font = new Font("Segoe UI", 12F);
            cancelBtn.Location = new Point(565, 1401);
            cancelBtn.Size = new System.Drawing.Size(92, 45);
            addStudentBtn.Font = new Font("Segoe UI", 16F);
            addStudentBtn.Location = new Point(401, 1401);
            addStudentBtn.Size = new System.Drawing.Size(158, 45);
            stdJob.Font = new Font("Segoe UI", 12F);
            stdJob.Location = new Point(43, 255);
            stdJob.Size = new System.Drawing.Size(257, 32);
            wrongValueLabel.Location = new Point(117, 230);
            wrongValueLabel.Size = new System.Drawing.Size(113, 13);
            alreadyExistsLabel.Location = new Point(108, 230);
            alreadyExistsLabel.Size = new System.Drawing.Size(131, 13);
            stdBirthDate.Font = new Font("Segoe UI", 9F);
            stdBirthDate.Location = new Point(536, 248);
            stdBirthDate.Size = new System.Drawing.Size(257, 36);
            stdNational.Font = new Font("Segoe UI", 12F);
            stdNational.Location = new Point(43, 194);
            stdNational.Size = new System.Drawing.Size(257, 32);
            studentPanelTitle.Font = new Font("Tahoma", 18F);
            studentPanelTitle.Location = new Point(615, 50);
            studentPanelTitle.Size = new System.Drawing.Size(230, 29);
            stdName.Font = new Font("Segoe UI", 12F);
            stdName.Location = new Point(536, 194);
            stdName.Size = new System.Drawing.Size(257, 32);
            guardianLinkLabel.Font = new Font("Tahoma", 12F);
            guardianLinkLabel.Location = new Point(412, 440);
            guardianLinkLabel.Size = new System.Drawing.Size(113, 19);
            guardianBirthLabel.Font = new Font("Tahoma", 12F);
            guardianBirthLabel.Location = new Point(195, 440);
            guardianBirthLabel.Size = new System.Drawing.Size(100, 19);
            guardianNameLabel.Font = new Font("Tahoma", 12F);
            guardianNameLabel.Location = new Point(818, 440);
            guardianNameLabel.Size = new System.Drawing.Size(87, 19);
            motherPhoneLabel.Font = new Font("Tahoma", 12F);
            motherPhoneLabel.Location = new Point(325, 397);
            motherPhoneLabel.Size = new System.Drawing.Size(95, 19);
            fatherPhoneLabel.Font = new Font("Tahoma", 12F);
            fatherPhoneLabel.Location = new Point(806, 397);
            fatherPhoneLabel.Size = new System.Drawing.Size(99, 19);
            motherJobLabel.Font = new Font("Tahoma", 12F);
            motherJobLabel.Location = new Point(326, 357);
            motherJobLabel.Size = new System.Drawing.Size(94, 19);
            motherQualiLabel.Font = new Font("Tahoma", 12F);
            motherQualiLabel.Location = new Point(814, 357);
            motherQualiLabel.Size = new System.Drawing.Size(91, 19);
            fatherJobLabel.Font = new Font("Tahoma", 12F);
            fatherJobLabel.Location = new Point(322, 317);
            fatherJobLabel.Size = new System.Drawing.Size(98, 19);
            fatherQualiLabel.Font = new Font("Tahoma", 12F);
            fatherQualiLabel.Location = new Point(810, 317);
            fatherQualiLabel.Size = new System.Drawing.Size(95, 19);
            jobLabel.Font = new Font("Tahoma", 12F);
            jobLabel.Location = new Point(345, 265);
            jobLabel.Size = new System.Drawing.Size(75, 19);
            birthLabel.Font = new Font("Tahoma", 12F);
            birthLabel.Location = new Point(802, 255);
            birthLabel.Size = new System.Drawing.Size(103, 19);
            stdNationalLabel.Font = new Font("Tahoma", 12F);
            stdNationalLabel.Location = new Point(308, 201);
            stdNationalLabel.Size = new System.Drawing.Size(112, 19);
            stdNameLabel.Font = new Font("Tahoma", 12F);
            stdNameLabel.Location = new Point(839, 201);
            stdNameLabel.Size = new System.Drawing.Size(66, 19);
            stdPhoneLabel.Font = new Font("Tahoma", 12F);
            stdPhoneLabel.Location = new Point(799, 498);
            stdPhoneLabel.Size = new System.Drawing.Size(112, 19);
            stdSchoolLabel.Font = new Font("Tahoma", 12F);
            stdSchoolLabel.Location = new Point(798, 575);
            stdSchoolLabel.Size = new System.Drawing.Size(119, 19);
            stdImagePath.Location = new Point(211, 154);
            stdImagePath.Size = new System.Drawing.Size(89, 13);
            stdFirstConclusionCheckBox.Font = new Font("Tahoma", 12F);
            stdFirstConclusionCheckBox.Location = new Point(234, 704);
            stdFirstConclusionCheckBox.Size = new System.Drawing.Size(203, 29);
            stdIjazahLabel.Font = new Font("Tahoma", 12F);
            stdIjazahLabel.Location = new Point(832, 1090);
            stdIjazahLabel.Size = new System.Drawing.Size(78, 19);
            stdCoursesLabel.Font = new Font("Tahoma", 12F);
            stdCoursesLabel.Location = new Point(371, 1090);
            stdCoursesLabel.Size = new System.Drawing.Size(73, 19);
            stdOfficeLabel.Font = new Font("Tahoma", 12F);
            stdOfficeLabel.Location = new Point(829, 149);
            stdOfficeLabel.Size = new System.Drawing.Size(74, 19);
            formTitle.Font = new Font("Tahoma", 20F);
            formTitle.Location = new Point(457, 1);
            formTitle.Size = new System.Drawing.Size(461, 33);
            maximizeBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            maximizeBtn.Location = new Point(51, 7);
            maximizeBtn.Size = new System.Drawing.Size(30, 24);
            studentSearchPanel.Location = new Point(12, 37);
            studentSearchPanel.Size = new System.Drawing.Size(926, 198);
            stdOfficeCheckBox.Font = new Font("Tahoma", 12F);
            stdOfficeCheckBox.Location = new Point(375, 148);
            stdOfficeCheckBox.Size = new System.Drawing.Size(91, 32);
            stdOfficeSearch.Font = new Font("Segoe UI", 10F);
            stdOfficeSearch.Location = new Point(23, 146);
            stdOfficeSearch.Size = new System.Drawing.Size(257, 36);
            stdBirthDateToSearch.Font = new Font("Segoe UI", 9F);
            stdBirthDateToSearch.Location = new Point(10, 51);
            stdBirthDateToSearch.Size = new System.Drawing.Size(120, 36);
            stdBirthDateSearch.Font = new Font("Segoe UI", 9F);
            stdBirthDateSearch.Location = new Point(186, 51);
            stdBirthDateSearch.Size = new System.Drawing.Size(120, 36);
            searchBtn.Font = new Font("Segoe UI", 16F);
            searchBtn.Location = new Point(570, 143);
            searchBtn.Size = new System.Drawing.Size(158, 45);
            stdEmailSearch.Font = new Font("Segoe UI", 12F);
            stdEmailSearch.Location = new Point(23, 98);
            stdEmailSearch.Size = new System.Drawing.Size(257, 32);
            stdPhoneSearch.Font = new Font("Segoe UI", 12F);
            stdPhoneSearch.Location = new Point(521, 98);
            stdPhoneSearch.Size = new System.Drawing.Size(257, 32);
            stdNationalSearch.Font = new Font("Segoe UI", 12F);
            stdNationalSearch.Location = new Point(23, 9);
            stdNationalSearch.Size = new System.Drawing.Size(257, 32);
            stdNameSearch.Font = new Font("Segoe UI", 12F);
            stdNameSearch.Location = new Point(521, 52);
            stdNameSearch.Size = new System.Drawing.Size(257, 32);
            searchPanelTitle.Font = new Font("Tahoma", 18F);
            searchPanelTitle.Location = new Point(790, 6);
            searchPanelTitle.Size = new System.Drawing.Size(133, 29);
            stdEmailCheckBox.Font = new Font("Tahoma", 12F);
            stdEmailCheckBox.Location = new Point(381, 99);
            stdEmailCheckBox.Size = new System.Drawing.Size(85, 32);
            stdPhoneCheckBox.Font = new Font("Tahoma", 12F);
            stdPhoneCheckBox.Location = new Point(785, 99);
            stdPhoneCheckBox.Size = new System.Drawing.Size(124, 32);
            stdNationalCheckBox.Font = new Font("Tahoma", 12F);
            stdNationalCheckBox.Location = new Point(340, 9);
            stdNationalCheckBox.Size = new System.Drawing.Size(126, 32);
            stdNameCheckBox.Font = new Font("Tahoma", 12F);
            stdNameCheckBox.Location = new Point(823, 53);
            stdNameCheckBox.Size = new System.Drawing.Size(85, 32);
            stdBirthDateToCheckBox.Font = new Font("Tahoma", 12F);
            stdBirthDateToCheckBox.Location = new Point(127, 53);
            stdBirthDateToCheckBox.Size = new System.Drawing.Size(55, 32);
            stdBirthDateFromCheckBox.Font = new Font("Tahoma", 12F);
            stdBirthDateFromCheckBox.Location = new Point(302, 53);
            stdBirthDateFromCheckBox.Size = new System.Drawing.Size(50, 32);
            stdBirthDateCheckBox.Font = new Font("Tahoma", 12F);
            stdBirthDateCheckBox.Location = new Point(349, 53);
            stdBirthDateCheckBox.Size = new System.Drawing.Size(117, 32);
            studentsListPanel.Location = new Point(12, 238);
            studentsListPanel.Size = new System.Drawing.Size(926, 403);
            openAddStudentBtn.Font = new Font("Segoe UI", 16F);
            openAddStudentBtn.Location = new Point(751, 5);
            openAddStudentBtn.Size = new System.Drawing.Size(165, 39);
            footerPanel.Location = new Point(12, 644);
            footerPanel.Size = new System.Drawing.Size(926, 49);
            checkUpdateBtn.Font = new Font("Segoe UI", 12F);
            checkUpdateBtn.Location = new Point(267, 5);
            checkUpdateBtn.Size = new System.Drawing.Size(79, 39);
            officeHelperBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            officeHelperBtn.Location = new Point(449, 5);
            officeHelperBtn.Size = new System.Drawing.Size(145, 39);
            rankCalcBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            rankCalcBtn.Location = new Point(600, 5);
            rankCalcBtn.Size = new System.Drawing.Size(145, 39);
            releasesLatestBtn.Font = new Font("Segoe UI", 12F);
            releasesLatestBtn.Location = new Point(184, 5);
            releasesLatestBtn.Size = new System.Drawing.Size(79, 39);
            extractExcelBtn.Font = new Font("Segoe UI", 12F);
            extractExcelBtn.Location = new Point(6, 5);
            extractExcelBtn.Size = new System.Drawing.Size(79, 39);
            officeTextBox.Font = new Font("Segoe UI", 12F);
            officeTextBox.Location = new Point(91, 7);
            officeTextBox.Size = new System.Drawing.Size(351, 36);
            officeEnterBtn.Font = new Font("Segoe UI", 12F);
            officeEnterBtn.Location = new Point(6, 5);
            officeEnterBtn.Size = new System.Drawing.Size(79, 39);
            headerPanel.Location = new Point(-1, -1);
            headerPanel.Size = new System.Drawing.Size(951, 701);
            formImage.Location = new Point(910, 4);
            formImage.Size = new System.Drawing.Size(30, 30);
            readRecordsBtn.Font = new Font("Segoe UI", 12F);
            readRecordsBtn.Location = new Point(138, 4);
            readRecordsBtn.Size = new System.Drawing.Size(145, 30);
            dataRecorderCheckBox.Font = new Font("Tahoma", 12F);
            dataRecorderCheckBox.Location = new Point(289, 5);
            dataRecorderCheckBox.Size = new System.Drawing.Size(171, 29);
            studentGradesPanel.Location = new Point(12, 37);
            studentGradesPanel.Size = new System.Drawing.Size(926, 656);
            compNotes.Font = new Font("Segoe UI", 12F);
            compNotes.Location = new Point(277, 488);
            compNotes.Size = new System.Drawing.Size(405, 32);
            compCount.Font = new Font("Tahoma", 12F);
            compCount.Location = new Point(189, 174);
            compCount.Size = new System.Drawing.Size(18, 19);
            stdAge.Font = new Font("Tahoma", 12F);
            stdAge.Location = new Point(242, 149);
            stdAge.Size = new System.Drawing.Size(537, 19);
            stdAgeLabel.Font = new Font("Tahoma", 12F);
            stdAgeLabel.Location = new Point(785, 148);
            stdAgeLabel.Size = new System.Drawing.Size(96, 19);
            addGradeBtn.Font = new Font("Segoe UI", 16F);
            addGradeBtn.Location = new Point(44, 516);
            addGradeBtn.Size = new System.Drawing.Size(158, 45);
            compDate.Font = new Font("Segoe UI", 9F);
            compDate.Location = new Point(277, 527);
            compDate.Size = new System.Drawing.Size(131, 36);
            stdRank.Font = new Font("Segoe UI", 12F);
            stdRank.Location = new Point(308, 612);
            stdRank.Size = new System.Drawing.Size(100, 32);
            stdScore.Font = new Font("Segoe UI", 12F);
            stdScore.Location = new Point(308, 571);
            stdScore.Size = new System.Drawing.Size(100, 32);
            currentLevel.Font = new Font("Segoe UI", 12F);
            currentLevel.Location = new Point(635, 612);
            currentLevel.Size = new System.Drawing.Size(100, 32);
            prevLevel.Font = new Font("Segoe UI", 12F);
            prevLevel.Location = new Point(635, 572);
            prevLevel.Size = new System.Drawing.Size(100, 32);
            stdCode.Font = new Font("Segoe UI", 12F);
            stdCode.Location = new Point(635, 531);
            stdCode.Size = new System.Drawing.Size(100, 32);
            stdRankLabel.Font = new Font("Tahoma", 12F);
            stdRankLabel.Location = new Point(476, 620);
            stdRankLabel.Size = new System.Drawing.Size(61, 19);
            stdScoreLabel.Font = new Font("Tahoma", 12F);
            stdScoreLabel.Location = new Point(473, 578);
            stdScoreLabel.Size = new System.Drawing.Size(64, 19);
            stdCodeLabel.Font = new Font("Tahoma", 12F);
            stdCodeLabel.Location = new Point(829, 537);
            stdCodeLabel.Size = new System.Drawing.Size(52, 19);
            compDateLabel.Font = new Font("Tahoma", 12F);
            compDateLabel.Location = new Point(420, 537);
            compDateLabel.Size = new System.Drawing.Size(117, 19);
            currentLevelLabel.Font = new Font("Tahoma", 12F);
            currentLevelLabel.Location = new Point(747, 620);
            currentLevelLabel.Size = new System.Drawing.Size(134, 19);
            prevLevelLabel.Font = new Font("Tahoma", 12F);
            prevLevelLabel.Location = new Point(744, 578);
            prevLevelLabel.Size = new System.Drawing.Size(137, 19);
            cancelBtn2.Font = new Font("Segoe UI", 14F);
            cancelBtn2.Location = new Point(72, 599);
            cancelBtn2.Size = new System.Drawing.Size(92, 45);
            competitionsListLabel.Font = new Font("Tahoma", 14F);
            competitionsListLabel.Location = new Point(741, 174);
            competitionsListLabel.Size = new System.Drawing.Size(140, 23);
            addCompetitionLabel.Font = new Font("Tahoma", 15F);
            addCompetitionLabel.Location = new Point(699, 488);
            addCompetitionLabel.Size = new System.Drawing.Size(202, 24);
            studentGradesListPanel.Location = new Point(18, 190);
            studentGradesListPanel.Size = new System.Drawing.Size(890, 289);
            stdImagePath2.Location = new Point(273, 68);
            stdImagePath2.Size = new System.Drawing.Size(0, 13);
            stdNational2.Font = new Font("Tahoma", 12F);
            stdNational2.Location = new Point(605, 118);
            stdNational2.Size = new System.Drawing.Size(163, 19);
            stdName2.Font = new Font("Tahoma", 12F);
            stdName2.Location = new Point(345, 88);
            stdName2.Size = new System.Drawing.Size(423, 19);
            stdNationalLabel2.Font = new Font("Tahoma", 12F);
            stdNationalLabel2.Location = new Point(774, 118);
            stdNationalLabel2.Size = new System.Drawing.Size(107, 19);
            stdNameLabel2.Font = new Font("Tahoma", 12F);
            stdNameLabel2.Location = new Point(780, 88);
            stdNameLabel2.Size = new System.Drawing.Size(101, 19);
            studentImage2.Location = new Point(23, 5);
            studentImage2.Size = new System.Drawing.Size(140, 180);
            gradesPanelTitle.Font = new Font("Tahoma", 18F);
            gradesPanelTitle.Location = new Point(672, 25);
            gradesPanelTitle.Size = new System.Drawing.Size(239, 29);
            currentLevelExplain.Font = new Font("Tahoma", 8F);
            currentLevelExplain.Location = new Point(590, 625);
            currentLevelExplain.Size = new System.Drawing.Size(29, 13);
            prevLevelExplain.Font = new Font("Tahoma", 8F);
            prevLevelExplain.Location = new Point(590, 583);
            prevLevelExplain.Size = new System.Drawing.Size(38, 13);
            ranksCalculatorPanel.Location = new Point(12, 37);
            ranksCalculatorPanel.Size = new System.Drawing.Size(926, 656);
            officeRank.Font = new Font("Segoe UI", 10F);
            officeRank.Location = new Point(681, 5);
            officeRank.Size = new System.Drawing.Size(237, 36);
            compDateTo.Font = new Font("Segoe UI", 9F);
            compDateTo.Location = new Point(259, 83);
            compDateTo.Size = new System.Drawing.Size(131, 36);
            compDateFrom.Font = new Font("Segoe UI", 9F);
            compDateFrom.Location = new Point(439, 83);
            compDateFrom.Size = new System.Drawing.Size(131, 36);
            levelCompCount.Font = new Font("Tahoma", 10F);
            levelCompCount.Location = new Point(392, 118);
            levelCompCount.Size = new System.Drawing.Size(16, 17);
            compsLabel.Font = new Font("Tahoma", 10F);
            compsLabel.Location = new Point(834, 118);
            compsLabel.Size = new System.Drawing.Size(69, 17);
            closeBtn2.Font = new Font("Segoe UI", 12F);
            closeBtn2.Location = new Point(27, 18);
            closeBtn2.Size = new System.Drawing.Size(91, 39);
            ranksListPanel.Location = new Point(10, 129);
            ranksListPanel.Size = new System.Drawing.Size(908, 521);
            setRanksBtn.Font = new Font("Segoe UI", 12F);
            setRanksBtn.Location = new Point(11, 83);
            setRanksBtn.Size = new System.Drawing.Size(117, 39);
            getGradesDataBtn.Font = new Font("Segoe UI", 12F);
            getGradesDataBtn.Location = new Point(132, 83);
            getGradesDataBtn.Size = new System.Drawing.Size(117, 39);
            dateToLabel.Font = new Font("Tahoma", 12F);
            dateToLabel.Location = new Point(396, 90);
            dateToLabel.Size = new System.Drawing.Size(34, 19);
            dateFromLabel.Font = new Font("Tahoma", 12F);
            dateFromLabel.Location = new Point(576, 90);
            dateFromLabel.Size = new System.Drawing.Size(96, 19);
            compLevel.Font = new Font("Segoe UI", 12F);
            compLevel.Location = new Point(691, 83);
            compLevel.Size = new System.Drawing.Size(77, 32);
            compLevelLabel.Font = new Font("Tahoma", 12F);
            compLevelLabel.Location = new Point(771, 90);
            compLevelLabel.Size = new System.Drawing.Size(144, 19);
            studentCount.Font = new Font("Tahoma", 12F);
            studentCount.Location = new Point(677, 53);
            studentCount.Size = new System.Drawing.Size(96, 19);
            studentCountLabel.Font = new Font("Tahoma", 12F);
            studentCountLabel.Location = new Point(773, 52);
            studentCountLabel.Size = new System.Drawing.Size(141, 19);
            wrongThingLabel.Font = new Font("Tahoma", 10F);
            wrongThingLabel.Location = new Point(336, 64);
            wrongThingLabel.Size = new System.Drawing.Size(166, 17);
            wrongThing2Label.Font = new Font("Tahoma", 10F);
            wrongThing2Label.Location = new Point(336, 64);
            wrongThing2Label.Size = new System.Drawing.Size(156, 17);
            ranksPanelTitle.Font = new Font("Tahoma", 12F);
            ranksPanelTitle.Location = new Point(132, 5);
            ranksPanelTitle.Size = new System.Drawing.Size(537, 57);
            disableAtAll.Location = new Point(455, 11);
            disableAtAll.Size = new System.Drawing.Size(8, 15);
            officeComboBox.Font = new Font("Segoe UI", 10F);
            officeComboBox.Location = new Point(540, 1);
            officeComboBox.Size = new System.Drawing.Size(356, 36);
        }
        #endregion

    }
}

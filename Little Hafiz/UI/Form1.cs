using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Drawing;
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
            AddTitleInStudentsListPanel(0);

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
            {
                Program.Record = Properties.Settings.Default.RecordEnabled;
                colorState = Properties.Settings.Default.ColorState;
                if (colorState != 0) SetColor();
            }

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

            compDateFrom.Value = DateTime.Now;
            compDateTo.Value = DateTime.Now;
            versionLabel.Text = "v" + string.Join(".", Application.ProductVersion.Split('.'), 0, 2);

            zxing = File.Exists("zxing.dll");
            if (!zxing) qrcodeCheckBox.Visible = false;
            aForge = File.Exists("AForge.dll");
            if (!aForge)
            {
                cameraCheckBox.Visible = false;
                openCompsCheckBox.Visible = false;
            }
            SetLogoImage();
        }

        private bool zxing, aForge;

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
        {
            if (cameraCheckBox.Checked) cameraCheckBox.Checked = false;
            Close();
        }

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
                originalHeight = 40;
                expandedHeight = 104;
                animationStep = 64;
                ResetComponent();
            }
            else if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                NewSizeX = Size.Width; NewSizeY = Size.Height;
                fs = new FormSize(SizeX, SizeY, NewSizeX, NewSizeY);
                fs.SetControls(Controls);
                originalHeight = fs.GetNewY(40);
                expandedHeight = fs.GetNewY(104);
                animationStep = fs.GetNewY(64);
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
                SetLogoImage();
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
        private void AddTitleInStudentsListPanel(int total)
        {
            studentsListPanel.Controls.Clear();
            studentsListPanel.Controls.Add(new StudentSearchRow(total) { Location = new Point(9, 9) });
        }

        private void VersionLabel_DoubleClick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.BackupEnabled)
            {
                if (MessageBox.Show("سيتم تعطيل النسخ الاحتياطي", ">_<", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    Properties.Settings.Default.BackupEnabled = false;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                Properties.Settings.Default.BackupEnabled = true;
                Properties.Settings.Default.Save();
                MessageBox.Show("تم تفعيل النسخ الاحتياطي", ":D");
            }
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
                if (stdBirthDateFromCheckBox.Checked || !stdBirthDateToCheckBox.Checked) birth = stdBirthDateSearch.Value.ToStandardString();
                if (stdBirthDateFromCheckBox.Checked || stdBirthDateToCheckBox.Checked) birth += "|";
                if (stdBirthDateToCheckBox.Checked) birth += stdBirthDateToSearch.Value.ToStandardString();
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

            AddStudentRowsInSearchPanel(students);
        }

        private void AddStudentRowsInSearchPanel(StudentSearchRowData[] students)
        {
            AddTitleInStudentsListPanel(students.Length);
            if (students is null) return;

            StudentSearchRow stdRow;
            for (int i = 0; i < students.Length; i++)
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
            if (sender is StudentSearchRow ssr)
                currentStudent = ssr;
            else if (sender is Guna2Button g2b)
                currentStudent = (StudentSearchRow)g2b.Parent;
            else
                return;

            string national = currentStudent.StudentSearchRowData.NationalNumber;
            StudentData stdData = DatabaseHelper.SelectStudent(national);
            if (stdData is null)
            {
                ErrorMessage();
                return;
            }

            SetStudentData(stdData);

            studentSearchPanel.Visible = false;
            int verticalScroll = studentsListPanel.VerticalScroll.Value;
            studentsListPanel.Visible = false;
            studentsListPanel.VerticalScroll.Value = verticalScroll;
            footerPanel.Visible = false;

            addStudentBtn.Text = "تعديل";
            stdNational.Enabled = false;
            stdBirthDate.Enabled = currentStudent.StudentSearchRowData.CompetitionDate is null;
            deleteStudentBtn.Visible = stdBirthDate.Enabled && DatabaseHelper.CurrentOffice == 0;
            copyStdDataBtn.Visible = DatabaseHelper.CurrentOffice == 0;
            studentPanelState = StudentPanelState.Update;
            studentDataPanel.Visible = true;
            cancel1Btn.Focus();
        }

        private void ShowGradesBtn_Click(object sender, EventArgs e)
        {
            if (sender is StudentSearchRow ssr)
                currentStudent = ssr;
            else if (sender is Guna2Button g2b)
                currentStudent = (StudentSearchRow)g2b.Parent;
            else
                return;

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

        #region QrCode
        bool isQrCode;
        private void NationalSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                NationalEnter();
            }
            else if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        public void NationalEnter()
        {
            if (stdNationalSearch.Text.Length != 14) return;
            stdNationalCheckBox.Checked = true;
            stdNameCheckBox.Checked = false;
            stdPhoneCheckBox.Checked = false;
            stdEmailCheckBox.Checked = false;
            stdBirthDateCheckBox.Checked = false;
            if (DatabaseHelper.CurrentOffice == 0)
                stdOfficeCheckBox.Checked = false;
            SearchBtn_Click(null, null);
            if (studentsListPanel.Controls.Count == 2)
            {
                stdNationalSearch.Text = "";
                if (openCompsCheckBox.Checked)
                    ShowGradesBtn_Click(studentsListPanel.Controls[1], null);
                else
                    ShowStudentBtn_Click(studentsListPanel.Controls[1], null);
                isQrCode = true;
            }
            else
            {
                stdEmailSearch.Text = stdNationalSearch.Text;
                stdNationalSearch.Text = "";
            }
        }

        BarcodeScanner barcodeScanner;
        private void StdNationalSearch_Enter(object sender, EventArgs e)
        {
            barcodeScanner?.Resume();
        }

        private void StdNationalSearch_Leave(object sender, EventArgs e)
        {
            barcodeScanner?.Pause();
        }

        bool checkBoxIsChangedByCode;
        private void CameraCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (cameraCheckBox.Checked)
            {
                if (!aForge)
                {
                    MessageBox.Show("AForge.dll file is missing.");
                    checkBoxIsChangedByCode = true;
                    cameraCheckBox.Checked = false;
                    return;
                }
                barcodeScanner = new BarcodeScanner();
                string[] names = barcodeScanner.Init();
                if (names.Length == 0)
                {
                    MessageBox.Show("لا توجد كاميرا");
                    barcodeScanner = null;
                    checkBoxIsChangedByCode = true;
                    cameraCheckBox.Checked = false;
                    return;
                }
                else if (names.Length == 1) barcodeScanner.Start();
                else
                {
                    DialogResult res = MessageBox.Show($"1. {names[0]}\n2. {names[1]}\n3. {(names.Length > 2 ? names[2] : "")}", "الكاميرات", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (res == DialogResult.Yes) barcodeScanner.Start(0);
                    else if (res == DialogResult.No) barcodeScanner.Start(1);
                    else if (res == DialogResult.Cancel && names.Length > 2) barcodeScanner.Start(2);
                    else
                    {
                        barcodeScanner = null;
                        checkBoxIsChangedByCode = true;
                        cameraCheckBox.Checked = false;
                    }
                }
                stdNationalSearch.Enter += StdNationalSearch_Enter;
                stdNationalSearch.Leave += StdNationalSearch_Leave;
            }
            else if (!checkBoxIsChangedByCode)
            {
                stdNationalSearch.Enter -= StdNationalSearch_Enter;
                stdNationalSearch.Leave -= StdNationalSearch_Leave;
                barcodeScanner?.Stop();
                barcodeScanner = null;
            }
            checkBoxIsChangedByCode = false;
        }

        public void AfterQRCodeScanner(string s)
        {
            string nat, level = null, code = null;
            if (stdNationalSearch.Visible && s.Length >= 14)
            {
                nat = s.Substring(0, 14);
                if (s.Length > 14)
                    level = s.Substring(14, 1);
                if (s.Length > 15)
                    code = s.Substring(15);
            }
            else return;

            Invoke(new Action(() =>
            {
                stdNationalSearch.Text = nat;
                NationalEnter();
                if (stdCode.Visible)
                {
                    if (level != null && byte.TryParse(level, out byte val1))
                    {
                        if (val1 == 0) val1 = 10;
                        if (val1 >= currentLevel.Minimum && val1 <= currentLevel.Maximum)
                            currentLevel.Value = val1;
                    }
                    if (code != null && short.TryParse(code, out short val2)) stdCode.Value = val2;
                }
            }));
        }
        #endregion

        #region Student Data Panel
        private void CopyStdDataBtn_Click(object sender, EventArgs e)
        {
            copyStdDataBtn.Visible = false;
            studentPanelState = StudentPanelState.Add;
            addStudentBtn.Text = "إضافة";
            stdNational.Enabled = true;
            stdNational.Text = stdNational.Text.Substring(0, stdNational.Text.Length - 1);
            stdBirthDate.Enabled = true;
            deleteStudentBtn.Visible = false;
        }

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

        private void SetLogoImage()
        {
            string[] exts = { ".jpg", ".jpeg", ".png", ".bmp" };
            string path = exts.Select(ext => Path.Combine("logo" + ext)).FirstOrDefault(File.Exists);

            logo.Image = path != null ? new Bitmap(new Bitmap(path), logo.Size.Width, logo.Size.Height) : null;
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
            if (isQrCode)
                stdNationalSearch.Focus();
            else
                openAddStudentBtn.Focus();
            isQrCode = false;
        }

        private void Cancel1Btn_Click(object sender, EventArgs e)
        {
            CancelBtn_Click(null, null);
        }

        bool isSure;
        private void AddStudentBtn_Click(object sender, EventArgs e)
        {
            if (stdNational.Text.Length != 14 || wrongValueLabel.Visible || alreadyExistsLabel.Visible)
            {
                MessageBox.Show("أدخل رقم قومي صحيح");
                return;
            }
            if (!isSure && stdOffice.SelectedIndex == 0 && stdOffice.Enabled && MessageBox.Show("إذا لم تقم باختيار مكتب للطالب، سيتم وضعه في مكتب مزيف\nهل تريد الاستمرار ؟", "تنبيه !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) == DialogResult.No)
                return;

            isSure = true;
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
                NationalID = stdNational.Text,

                FullName = stdNameLabel.Text + stdName.Text,
                NationalNumber = stdNationalLabel.Text + stdNational.Text,
                BirthDate = birthLabel.Text + stdBirthDate.Value.ToStandardString(),
                Job = jobLabel.Text + stdJob.Text,

                FatherQualification = fatherQualiLabel.Text + fatherQuali.Text,
                MotherQualification = motherQualiLabel.Text + motherQuali.Text,
                FatherJob = fatherJobLabel.Text + fatherJob.Text,
                MotherJob = motherJobLabel.Text + motherJob.Text,
                FatherPhone = fatherPhoneLabel.Text + fatherPhone.Text,
                MotherPhone = motherPhoneLabel.Text + motherPhone.Text,
                GuardianName = guardianNameLabel.Text + guardianName.Text,
                GuardianLink = guardianLinkLabel.Text + guardianLink.Text,
                GuardianBirth = guardianBirthLabel.Text + guardianBirth.Value.ToStandardString(),

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
                OfficeName = stdOfficeLabel.Text + (stdOffice.SelectedIndex == 0 ? "لا يوجد" : stdOffice.Text),
                JoiningDate = stdJoiningDateLabel.Text + stdJoiningDate.Value.ToStandardString(),
                FirstConclusionDate = stdFirstConclusionCheckBox.Text + (stdFirstConclusionCheckBox.Checked ? stdFirstConclusion.Value.ToStandardString() : "لا توجد"),

                StudentMashaykh = stdMashaykhLabel.Text + stdMashaykh.Text,
                MemorizePlaces = stdMemoPlacesLabel.Text + stdMemoPlaces.Text,
                Certificates = stdCertificatesLabel.Text + stdCertificates.Text,
                Ijazah = stdIjazahLabel.Text + stdIjazah.Text,
                Courses = stdCoursesLabel.Text + stdCourses.Text,
                Skills = stdSkillsLabel.Text + stdSkills.Text,
                Hobbies = stdHobbiesLabel.Text + stdHobbies.Text,
                StdComps = stdCompsLabel.Text + stdComps.Text,
                Notes = stdNotesLabel.Text + stdNotes.Text,

                StudentImage = studentImage.Image ?? new Bitmap(140, 180)
            };

            if (copyStdNameCheckBox.Checked)
                try {
                    Clipboard.Clear();
                    Clipboard.SetText(stdName.Text);
                } catch { }

            StudentFormPrinter printer = new StudentFormPrinter(data, zxing && qrcodeCheckBox.Checked, arrangeBigFieldsCheckBox.Checked);
            printer.ShowPreview();
        }

        private StudentData GetStudentData()
        {
            return new StudentData
            {
                OfficeId = stdOffice.SelectedIndex,
                FullName = stdName.Text,
                NationalNumber = stdNational.Text,
                BirthDate = stdBirthDate.Value.ToStandardString(),
                Job = stdJob.Text,
                FatherQualification = fatherQuali.Text,
                MotherQualification = motherQuali.Text,
                FatherJob = fatherJob.Text,
                MotherJob = motherJob.Text,
                FatherPhone = fatherPhone.Text,
                MotherPhone = motherPhone.Text,
                GuardianName = guardianName.Text,
                GuardianLink = guardianLink.Text,
                GuardianBirth = guardianBirth.Value.ToStandardString(),
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
                JoiningDate = stdJoiningDate.Value.ToStandardString(),
                FirstConclusionDate = (stdFirstConclusionCheckBox.Checked ? stdFirstConclusion.Value : stdBirthDate.Value.AddYears(-20)).ToStandardString(),
                Certificates = stdCertificates.Text,
                Ijazah = stdIjazah.Text,
                Courses = stdCourses.Text,
                Skills = stdSkills.Text,
                Hobbies = stdHobbies.Text,
                StdComps = stdComps.Text,
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
            stdFirstConclusionCheckBox.Checked = false;
            stdFirstConclusion.Value = DateTime.Now.AddYears(-2);
            stdCertificates.Text = "";
            stdIjazah.Text = "";
            stdCourses.Text = "";
            stdSkills.Text = "";
            stdHobbies.Text = "";
            stdComps.Text = "";
            stdNotes.Text = "";
            stdImagePath.Text = "";
            deleteStudentBtn.Visible = false;
            copyStdDataBtn.Visible = false;
        }

        private void SetStudentDataAtStudentDataIsNotNull(StudentData stdData)
        {
            stdOffice.SelectedIndex = stdData.OfficeId;
            stdName.Text = stdData.FullName;
            stdNational.Text = stdData.NationalNumber;
            stdBirthDate.Value = stdData.BirthDate.ToStandardDateTime();
            stdJob.Text = stdData.Job;
            fatherQuali.Text = stdData.FatherQualification;
            motherQuali.Text = stdData.MotherQualification;
            fatherJob.Text = stdData.FatherJob;
            motherJob.Text = stdData.MotherJob;
            fatherPhone.Text = stdData.FatherPhone;
            motherPhone.Text = stdData.MotherPhone;
            guardianName.Text = stdData.GuardianName;
            guardianLink.Text = stdData.GuardianLink;
            guardianBirth.Value = stdData.GuardianBirth.ToStandardDateTime();
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
            stdJoiningDate.Value = stdData.JoiningDate.ToStandardDateTime();
            stdFirstConclusion.Value = stdData.FirstConclusionDate.ToStandardDateTime();
            stdFirstConclusionCheckBox.Checked = stdFirstConclusion.Value > stdBirthDate.Value;
            stdCertificates.Text = stdData.Certificates;
            stdIjazah.Text = stdData.Ijazah;
            stdCourses.Text = stdData.Courses;
            stdSkills.Text = stdData.Skills;
            stdHobbies.Text = stdData.Hobbies;
            stdComps.Text = stdData.StdComps;
            stdNotes.Text = stdData.Notes;
            stdImagePath.Text = stdData.Image;
        }

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

        int originalHeight = 40, expandedHeight = 104, animationStep = 64;
        private void BigTextBox_Enter(object sender, EventArgs e)
        {
            AnimateTextBox((Guna2TextBox)sender, true);
        }

        private void BigTextBox_Leave(object sender, EventArgs e)
        {
            AnimateTextBox((Guna2TextBox)sender, false);
        }

        private void AnimateTextBox(Guna2TextBox baseControl, bool enter)
        {
            if (enter && baseControl.Height < expandedHeight)
            {
                MoveControlsBelow(baseControl, animationStep);
                baseControl.Height += animationStep;
            }
            else if (!enter && baseControl.Height > originalHeight)
            {
                MoveControlsBelow(baseControl, -animationStep);
                baseControl.Height -= animationStep;
            }
        }

        private void MoveControlsBelow(Guna2TextBox baseControl, int offset)
        {
            foreach (Control ctrl in studentDataPanel.Controls)
                if (ctrl.Top > baseControl.Bottom)
                    ctrl.Top += offset;
        }
        #endregion

        #region Fields Helper
        private void StudentDataNameField_KeyUp(object sender, KeyEventArgs e)
        {
            Guna2TextBox box = (Guna2TextBox)sender;
            if (e.KeyCode == Keys.F1 && DatabaseHelper.CurrentOffice == 0)
                FieldHelp(box, (TargetField)box.Tag);
        }

        private void StudentDataGuardianNameField_KeyUp(object sender, KeyEventArgs e)
        {
            Guna2TextBox box = (Guna2TextBox)sender;
            if (e.KeyCode == Keys.F1 && DatabaseHelper.CurrentOffice == 0)
                FieldHelp(box, (TargetField)box.Tag);
            else if (e.KeyCode == Keys.F2 && DatabaseHelper.CurrentOffice == 0)
                GetStudentNameFromFieldData(box, (TargetField)box.Tag);
            else if (e.KeyCode == Keys.F3)
                SearchWithFieldData(box, (TargetField)box.Tag);
        }

        private void StudentDataField_KeyUp(object sender, KeyEventArgs e)
        {
            Guna2TextBox box = (Guna2TextBox)sender;
            if (e.KeyCode == Keys.F1)
                FieldHelp(box, (TargetField)box.Tag);
            else if (e.KeyCode == Keys.F2 && DatabaseHelper.CurrentOffice == 0)
                GetStudentNameFromFieldData(box, (TargetField)box.Tag);
            else if (e.KeyCode == Keys.F3)
                SearchWithFieldData(box, (TargetField)box.Tag);
        }

        private void FieldHelp(Guna2TextBox textbox, TargetField target)
        {
            FieldData[] data = DatabaseHelper.FieldSearch(target);
            if (data is null) return;
            ListViewDialog lvd = new ListViewDialog(GetColumnTitle(target), data);
            if (lvd.ShowDialog() != DialogResult.OK || lvd.SelectedIndex == -1) return;

            textbox.Text = data[lvd.SelectedIndex].Text;
        }

        private void GetStudentNameFromFieldData(Guna2TextBox textbox, TargetField target)
        {
            FieldData[] data = DatabaseHelper.FieldSearch(target, textbox.Text);
            if (data is null) return;
            ListViewDialog lvd = new ListViewDialog(GetColumnTitle(target) + ": " + textbox.Text, data);
            if (lvd.ShowDialog() != DialogResult.OK || lvd.SelectedIndex == -1) return;

            stdNameSearch.Text = data[lvd.SelectedIndex].Text;

            stdNationalCheckBox.Checked = false;
            stdNameCheckBox.Checked = true;
            stdPhoneCheckBox.Checked = false;
            stdEmailCheckBox.Checked = false;
            stdBirthDateCheckBox.Checked = false;
            if (DatabaseHelper.CurrentOffice == 0) stdOfficeCheckBox.Checked = false;
            CancelBtn_Click(null, null);
            SearchBtn_Click(null, null);
        }

        private void SearchWithFieldData(Guna2TextBox textbox, TargetField target)
        {
            StudentSearchRowData[] data = DatabaseHelper.SelectStudents(target, textbox.Text, stdOfficeCheckBox.Checked ? stdOfficeSearch.SelectedIndex : 0);
            if (data is null || data.Length == 0) return;

            stdNationalCheckBox.Checked = false;
            stdNameCheckBox.Checked = false;
            stdPhoneCheckBox.Checked = false;
            stdEmailCheckBox.Checked = false;
            stdBirthDateCheckBox.Checked = false;
            if (stdOfficeSearch.SelectedIndex == 0) stdOfficeCheckBox.Checked = false;
            CancelBtn_Click(null, null);
            AddStudentRowsInSearchPanel(data);
        }

        private static string GetColumnTitle(TargetField target)
        {
            switch (target)
            {
                case TargetField.StudentName: return "اسم الطالب";
                case TargetField.StudentJob: return "وظيفة الطالب";
                case TargetField.FatherQualification: return "مؤهل الأب";
                case TargetField.MotherQualification: return "مؤهل الأم";
                case TargetField.FatherJob: return "وظيفة الأب";
                case TargetField.MotherJob: return "وظيفة الأم";
                case TargetField.GuardianName: return "ولي الأمر";
                case TargetField.GuardianLink: return "الصلة بالطالب";
                case TargetField.School: return "المدرسة/الكلية";
                case TargetField.Class: return "الفصل الدراسي";
                case TargetField.MaritalStatus: return "الحالة الاجتماعية";
                default: return null;
            }
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
            int verticalScroll = studentsListPanel.VerticalScroll.Value;
            studentsListPanel.Visible = false;
            studentsListPanel.VerticalScroll.Value = verticalScroll;
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
            cancelBtn2.Focus();
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
            float year = -371;
            DateTime birthDate = currentStudent.StudentSearchRowData.BirthDate.ToStandardDateTime();
            if (birthDate != DateTime.MinValue)
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
            string newDate = compDate.Value.ToStandardStringWithoutDay();
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
            if ((float)stdAge.Tag == -371)
            {
                MessageBox.Show("تاريخ الميلاد غير صالح");
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
                CompetitionDate = compDate.Value.ToStandardStringWithoutDay(),
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
            if (isQrCode)
                stdNationalSearch.Focus();
            else
                openAddStudentBtn.Focus();
            isQrCode = false;
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

        private void AddCompetitionLabel_DoubleClick(object sender, EventArgs e)
        {
            if (compNotes.Text.Trim() != "")
            {
                compNotes.Tag = compNotes.Text;
                compNotes.Text = "";
            }
            else
                compNotes.Text = (string)compNotes.Tag;
        }
        #endregion

        #region Ranks Calculator Panel
        private void CloseBtn2_Click(object sender, EventArgs e)
        {
            ranksCalculatorPanel.Visible = false;
            studentSearchPanel.Visible = true;
            studentsListPanel.Visible = true;
            footerPanel.Visible = true;
            rankCalcBtn.Focus();
        }

        private void DateFromLabel_DoubleClick(object sender, EventArgs e)
            => compDateFrom.Value = DateTime.Now;

        private void DateToLabel_DoubleClick(object sender, EventArgs e)
            => compDateTo.Value = compDateFrom.Value;

        private void GetGradesDataBtn_Click(object sender, EventArgs e)
        {
            setRanksBtn.Enabled = officeRank.SelectedIndex == 0;

            CompetitionRankData[] ranks = DatabaseHelper.SelectCompetitionRanks((int)compLevel.Value, compDateFrom.Value.ToStandardStringWithoutDay(), compDateTo.Value.ToStandardStringWithoutDay(), officeRank.SelectedIndex);
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
            cancel1Btn.Focus();
        }

        private void ExtractExcelBtn_Click(object sender, EventArgs e)
        {
            if (!File.Exists("ClosedXML.dll"))
            {
                MessageBox.Show("مكتبات الايكسل غير موجودة");
                return;
            }
            if (saveExcelFileDialog.ShowDialog() != DialogResult.OK) return;

            ExcelRowData[] rows = DatabaseHelper.SelectExcelRowData(0, 0, stdOfficeSearch.SelectedIndex);
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
                    if (sheetIndex != 0)
                        SetDataOnExcelFile(sheets[0], ref sheetsRowIndexes[0], rows[i]);
                }

                workbook.SaveAs(saveExcelFileDialog.FileName);
            }
        }

        private void SetTitlesOnExcelFile(IXLWorksheet sheet)
        {
            sheet.Range("A1:M1").Merge().Value = "المسابقة القرآنية الرمضانية";
            sheet.Row(1).Height = 30;
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 16;
            sheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Column(1).Width = 7;
            sheet.Cell(2, 1).Value = "م";

            sheet.Column(2).Width = 20;
            sheet.Cell(2, 2).Value = "الاسم";

            sheet.Column(3).Width = 15;
            sheet.Cell(2, 3).Value = "الرقم القومي";

            sheet.Column(4).Width = 15;
            sheet.Cell(2, 4).Value = "تاريخ الميلاد";

            sheet.Column(5).Width = 15;
            sheet.Cell(2, 5).Value = "رقم التليفون";

            sheet.Column(6).Width = 15;
            sheet.Cell(2, 6).Value = "العنوان";


            sheet.Column(7).Width = 10;
            sheet.Cell(2, 7).Value = "الوظيفة";

            sheet.Column(8).Width = 10;
            sheet.Cell(2, 8).Value = "وظيفة الأب";

            sheet.Column(9).Width = 10;
            sheet.Cell(2, 9).Value = "المدرسة/الكلية";

            sheet.Column(10).Width = 10;
            sheet.Cell(2, 10).Value = "الصف";

            sheet.Column(11).Width = 15;
            sheet.Cell(2, 11).Value = "مقدار الحفظ";


            sheet.Column(12).Width = 15;
            sheet.Cell(2, 12).Value = "المكتب";


            sheet.Column(13).Width = 7;
            sheet.Cell(2, 13).Value = "الكود";

            sheet.Column(14).Width = 7;
            sheet.Cell(2, 14).Value = "السابق";

            sheet.Column(15).Width = 7;
            sheet.Cell(2, 15).Value = "الحالي";

            sheet.Column(16).Width = 14;
            sheet.Cell(2, 16).Value = "تاريخ المسابقة";

            sheet.Column(17).Width = 7;
            sheet.Cell(2, 17).Value = "المركز";
        }

        private void SetDataOnExcelFile(IXLWorksheet sheet, ref int row, ExcelRowData data)
        {
            sheet.Cell(row, 1).Value = (row - 2).ToString();
            
            sheet.Cell(row, 2).Value = data.FullName;
            sheet.Cell(row, 3).Value = data.NationalNumber;
            sheet.Cell(row, 4).Value = data.BirthDate;
            sheet.Cell(row, 5).Value = data.PhoneNumber;
            sheet.Cell(row, 6).Value = data.Address;

            sheet.Cell(row, 7).Value = data.Job;
            sheet.Cell(row, 8).Value = data.FatherJob;
            sheet.Cell(row, 9).Value = data.School;
            sheet.Cell(row, 10).Value = data.Class;
            sheet.Cell(row, 11).Value = data.MemoAmount;

            sheet.Cell(row, 12).Value = data.Office == 0 ? "غير معروف" : offices[data.Office];

            sheet.Cell(row, 13).Value = data.StudentCode;
            sheet.Cell(row, 14).Value = Ranks.ConvertNumberToRank(data.PreviousLevel);
            sheet.Cell(row, 15).Value = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            sheet.Cell(row, 16).Value = data.CompetitionDate;
            sheet.Cell(row, 17).Value = data.Rank;
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

            wrongThingLabel.Visible = false;
            wrongThing2Label.Visible = false;
            levelCompCount.Text = "0";

            ranksCalculatorPanel.Visible = true;
            OfficeRank_SelectedIndexChanged(null, null);
            closeBtn2.Focus();
        }

        private void HideOfficeTools()
        {
            officeTextBox.Visible = false;
            officeEnterBtn.Visible = false;
            (officeHelperBtn.FillColor, officeHelperBtn.FillColor2) = (officeHelperBtn.FillColor2, officeHelperBtn.FillColor);
            officeHelperBtn.Text = "إضافة مكتب";
            checkUpdateBtn.Visible = true;
            releasesLatestBtn.Visible = true;
            extractExcelBtn.Visible = true;
            colorBtn.Visible = true;
        }

        private bool isMasterCopy;
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

            if (!isMasterCopy && MessageBox.Show("هل انت متأكد أنك على النسخة الرئيسية ؟", "؟!?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            isMasterCopy = true;
            checkUpdateBtn.Visible = false;
            releasesLatestBtn.Visible = false;
            extractExcelBtn.Visible = false;
            colorBtn.Visible = false;
            (officeHelperBtn.FillColor, officeHelperBtn.FillColor2) = (officeHelperBtn.FillColor2, officeHelperBtn.FillColor);
            officeHelperBtn.Text = "إلغاء";
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
            => Process.Start("https://github.com/MohamedAshref371/little-hafiz");
        #endregion

        #region Colors
        int colorState = 0;
        private void ColorBtn_Click(object sender, EventArgs e)
        {
            if (colorState == 1)
            {
                SetTextBoxColor(Color.FromArgb(220, 255, 255));
                SetTextBoxSearchColor(Color.FromArgb(220, 255, 220));
                SetCompoBoxColor(Color.FromArgb(192, 255, 192));
                StudentSearchRow.StudentButtonColor = Color.Empty;
                StudentSearchRow.GradesButtonColor = Color.Empty;
                SetRedTextColor(Color.FromArgb(192, 0, 0));
            }
            colorState = (colorState + 1) % 5;
            colorBtn.Text = "ألوان " + colorState.ToString();
            SetColor();
            Properties.Settings.Default.ColorState = colorState;
            Properties.Settings.Default.Save();
        }

        private void SetColor()
        {
            if (colorState == 0)
            {
                ForeColor = Color.Black;
                headerPanel.FillColor = Color.FromArgb(192, 192, 255);
                headerPanel.FillColor2 = Color.FromArgb(128, 128, 255);
                studentSearchPanel.FillColor = Color.FromArgb(255, 192, 255);
                studentSearchPanel.FillColor2 = Color.FromArgb(192, 192, 255);
                studentsListPanel.FillColor = Color.FromArgb(192, 255, 255);
                studentsListPanel.FillColor2 = Color.FromArgb(192, 192, 255);
                studentDataPanel.FillColor = Color.FromArgb(255, 224, 224);
                studentDataPanel.FillColor2 = Color.LightYellow;
                studentGradesPanel.FillColor = Color.FromArgb(100, 200, 150);
                studentGradesPanel.FillColor2 = Color.FromArgb(50, 100, 70);
                ranksCalculatorPanel.FillColor = Color.FromArgb(192, 220, 220);
                ranksCalculatorPanel.FillColor2 = Color.FromArgb(128, 128, 255);
                footerPanel.FillColor = Color.FromArgb(255, 255, 220);
                footerPanel.FillColor2 = Color.FromArgb(220, 220, 255);
                colorBtn.FillColor = Color.Red;
                colorBtn.FillColor2 = Color.Blue;
                return;
            }

            switch (colorState)
            {
                case 1:
                    Color01Execute();
                    return;
                case 2:
                    ForeColor = Color.White;
                    SetColor(Color.FromArgb(47, 149, 180), Color.FromArgb(29, 119, 144));
                    return;
                case 3:
                    ForeColor = Color.Black;
                    SetColor(Color.FromArgb(192, 220, 220), Color.FromArgb(128, 128, 255));
                    return;
                case 4:
                    ForeColor = Color.White;
                    SetColor(Color.FromArgb(79, 82, 104), Color.FromArgb(90, 110, 130));
                    return;
            }
        }

        private void Color01Execute()
        {
            if (File.Exists("Color01.txt"))
            {
                string[] arr = string.Join("", File.ReadAllText("Color01.txt").Where(c => c == '|' || c == ',' || c == '#' || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))).Split('|');
                if (arr.Length == 9)
                {
                    SetColor(ParseColor(arr[0]), ParseColor(arr[1]));
                    ForeColor = ParseColor(arr[2]);
                    SetTextBoxColor(ParseColor(arr[3]));
                    SetTextBoxSearchColor(ParseColor(arr[4]));
                    SetCompoBoxColor(ParseColor(arr[5]));
                    StudentSearchRow.StudentButtonColor = ParseColor(arr[6]);
                    StudentSearchRow.GradesButtonColor = ParseColor(arr[7]);
                    SetRedTextColor(ParseColor(arr[8]));
                    return;
                }
            }

            SetColor(Color.FromArgb(79, 163, 55), Color.FromArgb(64, 140, 43));
            ForeColor = Color.Black;
            SetTextBoxColor(Color.FromArgb(255, 255, 60));
            SetTextBoxSearchColor(Color.FromArgb(240, 255, 60));
            SetCompoBoxColor(Color.FromArgb(255, 255, 180));
            StudentSearchRow.StudentButtonColor = Color.LightYellow;
            StudentSearchRow.GradesButtonColor = Color.Green;
            SetRedTextColor(Color.Maroon);
        }

        private void ColorBtn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1 && colorState == 1)
                Color01Execute();
        }

        private void Form1_ForeColorChanged(object sender, EventArgs e)
        {
            copyStdDataBtn.ForeColor = ForeColor;
            cancelBtn.ForeColor = ForeColor;
            cancel1Btn.ForeColor = ForeColor;
            cancelBtn2.ForeColor = ForeColor;
        }

        private void SetColor(Color clr1, Color clr2)
        {
            headerPanel.FillColor = clr1;
            headerPanel.FillColor2 = clr2;
            studentSearchPanel.FillColor = clr1;
            studentSearchPanel.FillColor2 = clr2;
            studentsListPanel.FillColor = clr1;
            studentsListPanel.FillColor2 = clr2;
            studentDataPanel.FillColor = clr1;
            studentDataPanel.FillColor2 = clr2;
            studentGradesPanel.FillColor = clr1;
            studentGradesPanel.FillColor2 = clr2;
            ranksCalculatorPanel.FillColor = clr1;
            ranksCalculatorPanel.FillColor2 = clr2;
            footerPanel.FillColor = clr1;
            footerPanel.FillColor2 = clr2;
            colorBtn.FillColor = clr1;
            colorBtn.FillColor2 = clr2;
        }

        private void SetTextBoxColor(Color clr)
        {
            foreach (Control ctrl in studentDataPanel.Controls)
                if (ctrl is Guna2TextBox gCtrl)
                    gCtrl.FillColor = clr;

            compNotes.FillColor = clr;

            stdBirthDate.FillColor = clr;
            guardianBirth.FillColor = clr;
            stdJoiningDate.FillColor = clr;
            stdFirstConclusion.FillColor = clr;
            compDate.FillColor = clr;
            compDateFrom.FillColor = clr;
            compDateTo.FillColor = clr;
        }

        private void SetTextBoxSearchColor(Color clr)
        {
            stdNationalSearch.FillColor = clr;
            stdNameSearch.FillColor = clr;
            stdPhoneSearch.FillColor = clr;
            stdEmailSearch.FillColor = clr;
            stdBirthDateSearch.FillColor = clr;
            stdBirthDateToSearch.FillColor = clr;
        }

        private void SetCompoBoxColor(Color clr)
        {
            stdOffice.FillColor = clr;
            officeComboBox.FillColor = clr;
            stdOfficeSearch.FillColor = clr;
            officeRank.FillColor = clr;

            //stdBrothers.FillColor = clr;
            //stdArrangement.FillColor = clr;
            //stdCode.FillColor = clr;
            //prevLevel.FillColor = clr;
            //currentLevel.FillColor = clr;
            //compLevel.FillColor = clr;
            //stdScore.FillColor = clr;
            //stdRank.FillColor = clr;
        }

        private void SetRedTextColor(Color clr)
        {
            searchPanelTitle.ForeColor = clr;
            wrongThingLabel.ForeColor = clr;
            wrongThing2Label.ForeColor = clr;
            wrongValueLabel.ForeColor = clr;
            alreadyExistsLabel.ForeColor = clr;
        }

        public static Color ParseColor(string input)
        {
            if (input == "") return Color.Black;
            
            if (input.Contains(","))
            {
                string[] parts = input.Split(',');

                byte r = 0, g = 0, b = 0;
                bool isValidRgb = parts.Length == 3 &&
                byte.TryParse(parts[0], out r) &&
                byte.TryParse(parts[1], out g) &&
                byte.TryParse(parts[2], out b);

                return isValidRgb ? Color.FromArgb(r, g, b) : Color.Black;
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(input, @"^#?[A-Fa-f0-9]{6}$"))
            {
                input = input.TrimStart('#');
                return Color.FromArgb(
                    int.Parse(input.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(input.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(input.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)
                    );
            }

            Color known = Color.FromName(input);
            if (known.IsKnownColor) return known;

            return Color.Black;
        }
        #endregion

        #region Reset Component - It will be deleted at any time.
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
            cancel1Btn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            cancel1Btn.Location = new Point(832, 9);
            cancel1Btn.Size = new System.Drawing.Size(76, 39);
            copyStdDataBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            copyStdDataBtn.Location = new Point(750, 9);
            copyStdDataBtn.Size = new System.Drawing.Size(76, 39);
            stdComps.Font = new Font("Segoe UI", 12F);
            stdComps.Location = new Point(11, 1011);
            stdComps.Size = new System.Drawing.Size(822, 40);
            stdCompsLabel.Font = new Font("Tahoma", 12F);
            stdCompsLabel.Location = new Point(830, 1015);
            stdCompsLabel.Size = new System.Drawing.Size(85, 25);
            deleteStudentBtn.Font = new Font("Segoe UI", 16F);
            deleteStudentBtn.Location = new Point(11, 1135);
            deleteStudentBtn.Size = new System.Drawing.Size(158, 45);
            scrollHelperLabel.Location = new Point(680, 1198);
            scrollHelperLabel.Size = new System.Drawing.Size(0, 13);
            printStudentBtn.Font = new Font("Segoe UI", 16F);
            printStudentBtn.Location = new Point(265, 1135);
            printStudentBtn.Size = new System.Drawing.Size(140, 45);
            stdOffice.Font = new Font("Segoe UI", 10F);
            stdOffice.Location = new Point(536, 140);
            stdOffice.Size = new System.Drawing.Size(257, 36);
            stdNotes.Font = new Font("Segoe UI", 12F);
            stdNotes.Location = new Point(11, 1059);
            stdNotes.Size = new System.Drawing.Size(822, 40);
            stdNotesLabel.Font = new Font("Tahoma", 12F);
            stdNotesLabel.Location = new Point(823, 1063);
            stdNotesLabel.Size = new System.Drawing.Size(92, 25);
            studentImage.Location = new Point(43, 6);
            studentImage.Size = new System.Drawing.Size(140, 180);
            stdImageSelectorBtn.Font = new Font("Segoe UI", 12F);
            stdImageSelectorBtn.Location = new Point(204, 103);
            stdImageSelectorBtn.Size = new System.Drawing.Size(108, 32);
            stdImageLabel.Font = new Font("Tahoma", 12F);
            stdImageLabel.Location = new Point(191, 58);
            stdImageLabel.Size = new System.Drawing.Size(145, 19);
            stdSkills.Font = new Font("Segoe UI", 12F);
            stdSkills.Location = new Point(479, 963);
            stdSkills.Size = new System.Drawing.Size(354, 40);
            stdHobbies.Font = new Font("Segoe UI", 12F);
            stdHobbies.Location = new Point(11, 963);
            stdHobbies.Size = new System.Drawing.Size(354, 40);
            stdHobbiesLabel.Font = new Font("Tahoma", 12F);
            stdHobbiesLabel.Location = new Point(366, 969);
            stdHobbiesLabel.Size = new System.Drawing.Size(76, 25);
            stdIjazah.Font = new Font("Segoe UI", 12F);
            stdIjazah.Location = new Point(479, 915);
            stdIjazah.Size = new System.Drawing.Size(354, 40);
            stdCourses.Font = new Font("Segoe UI", 12F);
            stdCourses.Location = new Point(11, 915);
            stdCourses.Size = new System.Drawing.Size(354, 40);
            stdSkillsLabel.Font = new Font("Tahoma", 12F);
            stdSkillsLabel.Location = new Point(823, 968);
            stdSkillsLabel.Size = new System.Drawing.Size(88, 25);
            stdCertificates.Font = new Font("Segoe UI", 12F);
            stdCertificates.Location = new Point(8, 867);
            stdCertificates.Size = new System.Drawing.Size(722, 40);
            stdCertificatesLabel.Font = new Font("Tahoma", 12F);
            stdCertificatesLabel.Location = new Point(719, 871);
            stdCertificatesLabel.Size = new System.Drawing.Size(189, 25);
            stdFirstConclusion.Font = new Font("Segoe UI", 9F);
            stdFirstConclusion.Location = new Point(41, 698);
            stdFirstConclusion.Size = new System.Drawing.Size(193, 36);
            stdJoiningDate.Font = new Font("Segoe UI", 9F);
            stdJoiningDate.Location = new Point(540, 698);
            stdJoiningDate.Size = new System.Drawing.Size(193, 36);
            stdJoiningDateLabel.Font = new Font("Tahoma", 12F);
            stdJoiningDateLabel.Location = new Point(730, 704);
            stdJoiningDateLabel.Size = new System.Drawing.Size(179, 25);
            stdMemoPlaces.Font = new Font("Segoe UI", 12F);
            stdMemoPlaces.Location = new Point(8, 819);
            stdMemoPlaces.Size = new System.Drawing.Size(722, 40);
            stdMashaykh.Font = new Font("Segoe UI", 12F);
            stdMashaykh.Location = new Point(8, 771);
            stdMashaykh.Size = new System.Drawing.Size(722, 40);
            stdMemoPlacesLabel.Font = new Font("Tahoma", 12F);
            stdMemoPlacesLabel.Location = new Point(728, 823);
            stdMemoPlacesLabel.Size = new System.Drawing.Size(180, 25);
            stdMashaykhLabel.Font = new Font("Tahoma", 12F);
            stdMashaykhLabel.Location = new Point(730, 776);
            stdMashaykhLabel.Size = new System.Drawing.Size(175, 25);
            stdMemo.Font = new Font("Segoe UI", 12F);
            stdMemo.Location = new Point(43, 654);
            stdMemo.Size = new System.Drawing.Size(257, 32);
            stdMemoLabel.Font = new Font("Tahoma", 12F);
            stdMemoLabel.Location = new Point(316, 659);
            stdMemoLabel.Size = new System.Drawing.Size(104, 25);
            stdMaritalStatus.Font = new Font("Segoe UI", 12F);
            stdMaritalStatus.Location = new Point(536, 654);
            stdMaritalStatus.Size = new System.Drawing.Size(257, 32);
            stdMaritalStatusLabel.Font = new Font("Tahoma", 12F);
            stdMaritalStatusLabel.Location = new Point(783, 658);
            stdMaritalStatusLabel.Size = new System.Drawing.Size(134, 25);
            stdArrangementLabel.Font = new Font("Tahoma", 12F);
            stdArrangementLabel.Location = new Point(276, 611);
            stdArrangementLabel.Size = new System.Drawing.Size(144, 25);
            stdArrangement.Font = new Font("Segoe UI", 12F);
            stdArrangement.Location = new Point(117, 607);
            stdArrangement.Size = new System.Drawing.Size(100, 32);
            stdBrothersLabel.Font = new Font("Tahoma", 12F);
            stdBrothersLabel.Location = new Point(811, 611);
            stdBrothersLabel.Size = new System.Drawing.Size(95, 25);
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
            stdClassLabel.Location = new Point(294, 572);
            stdClassLabel.Size = new System.Drawing.Size(126, 25);
            stdEmail.Font = new Font("Segoe UI", 12F);
            stdEmail.Location = new Point(536, 528);
            stdEmail.Size = new System.Drawing.Size(257, 32);
            stdFacebook.Font = new Font("Segoe UI", 12F);
            stdFacebook.Location = new Point(43, 528);
            stdFacebook.Size = new System.Drawing.Size(257, 32);
            stdFacebookLabel.Font = new Font("Tahoma", 12F);
            stdFacebookLabel.Location = new Point(306, 532);
            stdFacebookLabel.Size = new System.Drawing.Size(114, 25);
            stdEmailLabel.Font = new Font("Tahoma", 12F);
            stdEmailLabel.Location = new Point(834, 532);
            stdEmailLabel.Size = new System.Drawing.Size(71, 25);
            stdAddress.Font = new Font("Segoe UI", 12F);
            stdAddress.Location = new Point(43, 488);
            stdAddress.Size = new System.Drawing.Size(257, 32);
            stdAddressLabel.Font = new Font("Tahoma", 12F);
            stdAddressLabel.Location = new Point(350, 492);
            stdAddressLabel.Size = new System.Drawing.Size(70, 25);
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
            cancelBtn.Location = new Point(575, 1135);
            cancelBtn.Size = new System.Drawing.Size(92, 45);
            addStudentBtn.Font = new Font("Segoe UI", 16F);
            addStudentBtn.Location = new Point(411, 1135);
            addStudentBtn.Size = new System.Drawing.Size(158, 45);
            stdJob.Font = new Font("Segoe UI", 12F);
            stdJob.Location = new Point(43, 255);
            stdJob.Size = new System.Drawing.Size(257, 32);
            stdBirthDate.Font = new Font("Segoe UI", 9F);
            stdBirthDate.Location = new Point(536, 250);
            stdBirthDate.Size = new System.Drawing.Size(257, 36);
            stdNational.Font = new Font("Segoe UI", 12F);
            stdNational.Location = new Point(43, 194);
            stdNational.Size = new System.Drawing.Size(257, 32);
            studentPanelTitle.Font = new Font("Tahoma", 18F);
            studentPanelTitle.Location = new Point(570, 62);
            studentPanelTitle.Size = new System.Drawing.Size(230, 29);
            stdName.Font = new Font("Segoe UI", 12F);
            stdName.Location = new Point(536, 194);
            stdName.Size = new System.Drawing.Size(257, 32);
            guardianLinkLabel.Font = new Font("Tahoma", 12F);
            guardianLinkLabel.Location = new Point(412, 435);
            guardianLinkLabel.Size = new System.Drawing.Size(113, 25);
            guardianBirthLabel.Font = new Font("Tahoma", 12F);
            guardianBirthLabel.Location = new Point(195, 435);
            guardianBirthLabel.Size = new System.Drawing.Size(100, 25);
            guardianNameLabel.Font = new Font("Tahoma", 12F);
            guardianNameLabel.Location = new Point(818, 435);
            guardianNameLabel.Size = new System.Drawing.Size(87, 25);
            motherPhoneLabel.Font = new Font("Tahoma", 12F);
            motherPhoneLabel.Location = new Point(325, 394);
            motherPhoneLabel.Size = new System.Drawing.Size(95, 25);
            fatherPhoneLabel.Font = new Font("Tahoma", 12F);
            fatherPhoneLabel.Location = new Point(806, 394);
            fatherPhoneLabel.Size = new System.Drawing.Size(99, 25);
            motherJobLabel.Font = new Font("Tahoma", 12F);
            motherJobLabel.Location = new Point(326, 354);
            motherJobLabel.Size = new System.Drawing.Size(94, 25);
            motherQualiLabel.Font = new Font("Tahoma", 12F);
            motherQualiLabel.Location = new Point(814, 354);
            motherQualiLabel.Size = new System.Drawing.Size(91, 25);
            fatherJobLabel.Font = new Font("Tahoma", 12F);
            fatherJobLabel.Location = new Point(322, 314);
            fatherJobLabel.Size = new System.Drawing.Size(98, 25);
            fatherQualiLabel.Font = new Font("Tahoma", 12F);
            fatherQualiLabel.Location = new Point(810, 314);
            fatherQualiLabel.Size = new System.Drawing.Size(95, 25);
            jobLabel.Font = new Font("Tahoma", 12F);
            jobLabel.Location = new Point(345, 260);
            jobLabel.Size = new System.Drawing.Size(75, 25);
            birthLabel.Font = new Font("Tahoma", 12F);
            birthLabel.Location = new Point(802, 255);
            birthLabel.Size = new System.Drawing.Size(103, 25);
            stdNationalLabel.Font = new Font("Tahoma", 12F);
            stdNationalLabel.Location = new Point(308, 198);
            stdNationalLabel.Size = new System.Drawing.Size(112, 25);
            stdNameLabel.Font = new Font("Tahoma", 12F);
            stdNameLabel.Location = new Point(839, 198);
            stdNameLabel.Size = new System.Drawing.Size(66, 25);
            stdPhoneLabel.Font = new Font("Tahoma", 12F);
            stdPhoneLabel.Location = new Point(788, 492);
            stdPhoneLabel.Size = new System.Drawing.Size(124, 25);
            stdSchoolLabel.Font = new Font("Tahoma", 12F);
            stdSchoolLabel.Location = new Point(788, 572);
            stdSchoolLabel.Size = new System.Drawing.Size(129, 25);
            stdImagePath.Location = new Point(211, 154);
            stdImagePath.Size = new System.Drawing.Size(89, 13);
            stdFirstConclusionCheckBox.Font = new Font("Tahoma", 12F);
            stdFirstConclusionCheckBox.Location = new Point(227, 703);
            stdFirstConclusionCheckBox.Size = new System.Drawing.Size(210, 29);
            stdIjazahLabel.Font = new Font("Tahoma", 12F);
            stdIjazahLabel.Location = new Point(833, 920);
            stdIjazahLabel.Size = new System.Drawing.Size(78, 25);
            stdCoursesLabel.Font = new Font("Tahoma", 12F);
            stdCoursesLabel.Location = new Point(369, 920);
            stdCoursesLabel.Size = new System.Drawing.Size(73, 25);
            stdOfficeLabel.Font = new Font("Tahoma", 12F);
            stdOfficeLabel.Location = new Point(829, 147);
            stdOfficeLabel.Size = new System.Drawing.Size(74, 25);
            copyStdNameCheckBox.Font = new Font("Tahoma", 12F);
            copyStdNameCheckBox.Location = new Point(244, 1108);
            copyStdNameCheckBox.Size = new System.Drawing.Size(159, 29);
            qrcodeCheckBox.Font = new Font("Tahoma", 12F);
            qrcodeCheckBox.Location = new Point(63, 1179);
            qrcodeCheckBox.Size = new System.Drawing.Size(161, 29);
            arrangeBigFieldsCheckBox.Font = new Font("Tahoma", 12F);
            arrangeBigFieldsCheckBox.Location = new Point(230, 1179);
            arrangeBigFieldsCheckBox.Size = new System.Drawing.Size(173, 29);
            logo.Location = new Point(380, 5);
            logo.Size = new System.Drawing.Size(130, 130);
            wrongValueLabel.Font = new Font("Tahoma", 10F);
            wrongValueLabel.Location = new Point(102, 227);
            wrongValueLabel.Size = new System.Drawing.Size(138, 17);
            alreadyExistsLabel.Font = new Font("Tahoma", 10F);
            alreadyExistsLabel.Location = new Point(93, 227);
            alreadyExistsLabel.Size = new System.Drawing.Size(161, 17);
            formTitle.Font = new Font("Tahoma", 20F);
            formTitle.Location = new Point(457, 1);
            formTitle.Size = new System.Drawing.Size(461, 33);
            maximizeBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            maximizeBtn.Location = new Point(51, 7);
            maximizeBtn.Size = new System.Drawing.Size(30, 24);
            studentSearchPanel.Location = new Point(12, 37);
            studentSearchPanel.Size = new System.Drawing.Size(926, 198);
            stdBirthDateToSearch.Font = new Font("Segoe UI", 9F);
            stdBirthDateToSearch.Location = new Point(10, 147);
            stdBirthDateToSearch.Size = new System.Drawing.Size(120, 36);
            stdBirthDateSearch.Font = new Font("Segoe UI", 9F);
            stdBirthDateSearch.Location = new Point(186, 147);
            stdBirthDateSearch.Size = new System.Drawing.Size(120, 36);
            stdBirthDateToCheckBox.Font = new Font("Tahoma", 12F);
            stdBirthDateToCheckBox.Location = new Point(127, 149);
            stdBirthDateToCheckBox.Size = new System.Drawing.Size(55, 32);
            stdBirthDateFromCheckBox.Font = new Font("Tahoma", 12F);
            stdBirthDateFromCheckBox.Location = new Point(302, 149);
            stdBirthDateFromCheckBox.Size = new System.Drawing.Size(50, 32);
            stdOfficeCheckBox.Font = new Font("Tahoma", 12F);
            stdOfficeCheckBox.Location = new Point(375, 11);
            stdOfficeCheckBox.Size = new System.Drawing.Size(91, 32);
            stdOfficeSearch.Font = new Font("Segoe UI", 10F);
            stdOfficeSearch.Location = new Point(23, 9);
            stdOfficeSearch.Size = new System.Drawing.Size(257, 36);
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
            stdNationalSearch.Location = new Point(521, 54);
            stdNationalSearch.Size = new System.Drawing.Size(257, 32);
            stdNameSearch.Font = new Font("Segoe UI", 12F);
            stdNameSearch.Location = new Point(23, 54);
            stdNameSearch.Size = new System.Drawing.Size(257, 32);
            searchPanelTitle.Font = new Font("Tahoma", 18F);
            searchPanelTitle.Location = new Point(788, 6);
            searchPanelTitle.Size = new System.Drawing.Size(133, 29);
            stdEmailCheckBox.Font = new Font("Tahoma", 12F);
            stdEmailCheckBox.Location = new Point(381, 99);
            stdEmailCheckBox.Size = new System.Drawing.Size(85, 32);
            stdPhoneCheckBox.Font = new Font("Tahoma", 12F);
            stdPhoneCheckBox.Location = new Point(785, 99);
            stdPhoneCheckBox.Size = new System.Drawing.Size(124, 32);
            stdNationalCheckBox.Font = new Font("Tahoma", 12F);
            stdNationalCheckBox.Location = new Point(783, 54);
            stdNationalCheckBox.Size = new System.Drawing.Size(126, 32);
            stdNameCheckBox.Font = new Font("Tahoma", 12F);
            stdNameCheckBox.Location = new Point(381, 54);
            stdNameCheckBox.Size = new System.Drawing.Size(85, 32);
            stdBirthDateCheckBox.Font = new Font("Tahoma", 12F);
            stdBirthDateCheckBox.Location = new Point(349, 149);
            stdBirthDateCheckBox.Size = new System.Drawing.Size(117, 32);
            cameraCheckBox.Font = new Font("Tahoma", 12F);
            cameraCheckBox.Location = new Point(668, 23);
            cameraCheckBox.Size = new System.Drawing.Size(83, 32);
            openCompsCheckBox.Font = new Font("Tahoma", 12F);
            openCompsCheckBox.Location = new Point(544, 23);
            openCompsCheckBox.Size = new System.Drawing.Size(104, 32);
            studentsListPanel.Location = new Point(12, 238);
            studentsListPanel.Size = new System.Drawing.Size(926, 403);
            openAddStudentBtn.Font = new Font("Segoe UI", 16F);
            openAddStudentBtn.Location = new Point(751, 5);
            openAddStudentBtn.Size = new System.Drawing.Size(165, 39);
            footerPanel.Location = new Point(12, 644);
            footerPanel.Size = new System.Drawing.Size(926, 49);
            colorBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            colorBtn.Location = new Point(265, 5);
            colorBtn.Size = new System.Drawing.Size(89, 39);
            checkUpdateBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            checkUpdateBtn.Location = new Point(180, 5);
            checkUpdateBtn.Size = new System.Drawing.Size(79, 39);
            officeHelperBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            officeHelperBtn.Location = new Point(449, 5);
            officeHelperBtn.Size = new System.Drawing.Size(145, 39);
            rankCalcBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            rankCalcBtn.Location = new Point(600, 5);
            rankCalcBtn.Size = new System.Drawing.Size(145, 39);
            releasesLatestBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            releasesLatestBtn.Location = new Point(97, 5);
            releasesLatestBtn.Size = new System.Drawing.Size(79, 39);
            extractExcelBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            extractExcelBtn.Location = new Point(6, 5);
            extractExcelBtn.Size = new System.Drawing.Size(79, 39);
            officeTextBox.Font = new Font("Segoe UI", 12F);
            officeTextBox.Location = new Point(91, 7);
            officeTextBox.Size = new System.Drawing.Size(351, 36);
            officeEnterBtn.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            officeEnterBtn.Location = new Point(6, 5);
            officeEnterBtn.Size = new System.Drawing.Size(79, 39);
            versionLabel.Font = new Font("Tahoma", 12F);
            versionLabel.Location = new Point(360, 7);
            versionLabel.Size = new System.Drawing.Size(80, 37);
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
            compDateTo.Location = new Point(290, 83);
            compDateTo.Size = new System.Drawing.Size(131, 36);
            compDateFrom.Font = new Font("Segoe UI", 9F);
            compDateFrom.Location = new Point(470, 83);
            compDateFrom.Size = new System.Drawing.Size(131, 36);
            levelCompCount.Font = new Font("Tahoma", 10F);
            levelCompCount.Location = new Point(424, 118);
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
            dateToLabel.Location = new Point(427, 90);
            dateToLabel.Size = new System.Drawing.Size(34, 19);
            dateFromLabel.Font = new Font("Tahoma", 12F);
            dateFromLabel.Location = new Point(607, 90);
            dateFromLabel.Size = new System.Drawing.Size(29, 19);
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
            wrongThingLabel.Location = new Point(360, 64);
            wrongThingLabel.Size = new System.Drawing.Size(166, 17);
            wrongThing2Label.Font = new Font("Tahoma", 10F);
            wrongThing2Label.Location = new Point(360, 64);
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

using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private void Form1_Load(object sender, EventArgs e)
        {
            AddTitleInStudentsListPanel(0);

            Timer timer = new Timer { Interval = 10 };
            timer.Tick += (s, e1) =>
            {
                timer.Stop();

                if (studentsListPanel.Visible)
                    studentsListPanel.Invalidate();

                if (studentDataPanel.Visible)
                    studentDataPanel.Invalidate();

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
            containerPanel.MouseDown += meh;
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
                Offices = new string[] { Application.ProductName };
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

            stdBirthDateLabel.Click += (s, e1) => stdBirthDate.Focus();
            guardianBirthLabel.Click += (s, e1) => guardianBirth.Focus();
            stdJoiningDateLabel.Click += (s, e1) => stdJoiningDate.Focus();
            stdFirstConclusionCheckBox.Click += (s, e1) => { if (stdFirstConclusionCheckBox.Checked) stdFirstConclusion.Focus(); };

            compLevel.Items.AddRange(Ranks.RanksText);
            compLevel.Items[0] = "الكل";
            compLevel.SelectedIndex = 1;
        }

        private bool zxing, aForge;

        public string[] Offices;
        private void GetOffice()
        {
            Offices = DatabaseHelper.GetOffices();
            if (Offices is null)
            {
                MessageBox.Show("خطأ، سيتم إغلاق البرنامج", "خطأ !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            AfterGetOffice(DatabaseHelper.CurrentOffice);
        }

        private void AfterGetOffice(int ofc)
        {
            formTitle.Text = Offices[ofc];

            Offices[0] = "اختر من القائمة";
            stdOffice.Items.Clear();
            stdOffice.Items.AddRange(Offices);
            stdOffice.SelectedIndex = ofc;

            stdOfficeSearch.Items.Clear();
            stdOfficeSearch.Items.AddRange(Offices);
            stdOfficeSearch.SelectedIndex = ofc;

            officeComboBox.Items.Clear();
            officeComboBox.Items.AddRange(Offices);
            officeComboBox.SelectedIndex = ofc;

            officeRank.Items.Clear();
            officeRank.Items.AddRange(Offices);
            officeRank.SelectedIndex = ofc;

            bool equalZero = ofc == 0;
            stdOffice.Enabled = equalZero;
            stdOfficeCheckBox.Checked = !equalZero;
            stdOfficeCheckBox.Enabled = equalZero;
            stdOfficeSearch.Enabled = equalZero;
            officeRank.Enabled = equalZero;

            StudentGradeRow.ReadOnly = !equalZero;
            StudentRankRow.ReadOnly = !equalZero;

            if (!equalZero)
            {
                dataRecorderCheckBox.Checked = true;
                disableAtAll.Visible = false;

                if (ranksCalculatorPanel.Visible)
                    CloseBtn2_Click(null, null);

                if (officeEnterBtn.Visible)
                    HideOfficeTools();
            }
            dataRecorderCheckBox.Enabled = equalZero;
        }

        private void FormImage_DoubleClick(object sender, EventArgs e)
        {
            if (officeComboBox.Visible)
            {
                if (stdOffice.Visible) return;
                DatabaseHelper.UpdateMetadataOffice(officeComboBox.SelectedIndex);
                GetOffice();
                officeComboBox.Visible = false;
                formTitle.Visible = true;
                DatabaseHelper.RemoveAllRecords();
                return;
            }

            if (DatabaseHelper.CurrentOffice != 0)
                MessageBox.Show("لا يمكن للنسخ الفرعية استعمال هذه الخاصية");

            if (DatabaseHelper.CurrentOffice != 0 && (!File.Exists("password.log") || !Secrets.ComputeSubCopyPassword(File.ReadAllText("password.log"))))
                return;

            if (MessageBox.Show("هل انت متأكد أنك تريد تحويل هذه النسخة إلى نسخة فرعية ؟\nلن تستطيع تحويلها الى نسخة رئيسية مجددا\nوسيتم حذف ملفات تسجيل حركة البيانات", "؟!?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            formTitle.Visible = false;
            officeComboBox.Visible = true;
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
                compLevel.ItemHeight = height;
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
            if (DatabaseHelper.CurrentOffice != 0)
            {
                MessageBox.Show("لا يمكن للنسخ الفرعية استعمال هذه الخاصية");
                return;
            }
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

        private void SearchBtn_SizeChanged(object sender, EventArgs e)
            => searchBtn.ImageSize = new Size(searchBtn.Height - 4, searchBtn.Height - 4);
        

        private void AddStudentRowsInSearchPanel(StudentSearchRowData[] students)
        {
            AddTitleInStudentsListPanel(students.Length);

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
                BirthDate = stdBirthDateLabel.Text + stdBirthDate.Value.ToStandardString(),
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

                StudentTeacher = stdTeacherLabel.Text + stdTeacher.Text,
                StudentGroup = stdGroupLabel.Text + stdGroup.Text,

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
                JoiningDate = stdJoiningDate.Value.ToStandardString(),
                FirstConclusionDate = (stdFirstConclusionCheckBox.Checked ? stdFirstConclusion.Value : stdBirthDate.Value.AddYears(-20)).ToStandardString(),
                StudentTeacher = stdTeacher.Text,
                StudentGroup = stdGroup.Text,
                StudentMashaykh = stdMashaykh.Text,
                MemorizePlaces = stdMemoPlaces.Text,
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

            DateTime now = DateTime.Now;

            stdName.Text = "";
            stdNational.Text = "";
            stdBirthDate.Value = now.AddYears(-10);
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
            guardianBirth.Value = now.AddYears(-20);
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
            stdJoiningDate.Value = now;
            stdFirstConclusionCheckBox.Checked = false;
            stdFirstConclusion.Value = now;
            stdTeacher.Text = "";
            stdGroup.Text = "";
            stdMashaykh.Text = "";
            stdMemoPlaces.Text = "";
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
            stdJoiningDate.Value = stdData.JoiningDate.ToStandardDateTime();
            stdFirstConclusion.Value = stdData.FirstConclusionDate.ToStandardDateTime();
            stdFirstConclusionCheckBox.Checked = stdFirstConclusion.Value > stdBirthDate.Value;
            stdTeacher.Text = stdData.StudentTeacher;
            stdGroup.Text = stdData.StudentGroup;
            stdMashaykh.Text = stdData.StudentMashaykh;
            stdMemoPlaces.Text = stdData.MemorizePlaces;
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
                GetStudentNameFromFieldData(box.Text, (TargetField)box.Tag);
            else if (e.KeyCode == Keys.F3)
                SearchWithFieldData(box.Text, (TargetField)box.Tag);
        }

        private void StudentDataField_KeyUp(object sender, KeyEventArgs e)
        {
            Guna2TextBox box = (Guna2TextBox)sender;
            if (e.KeyCode == Keys.F1)
                FieldHelp(box, (TargetField)box.Tag);
            else if (e.KeyCode == Keys.F2 && DatabaseHelper.CurrentOffice == 0)
                GetStudentNameFromFieldData(box.Text, (TargetField)box.Tag);
            else if (e.KeyCode == Keys.F3)
                SearchWithFieldData(box.Text, (TargetField)box.Tag);
        }

        private void StudentDateField_KeyUp(object sender, KeyEventArgs e)
        {
            Guna2DateTimePicker picker = (Guna2DateTimePicker)sender;
            TargetField target = (TargetField)picker.Tag;

            if (e.KeyCode == Keys.F1)
                FieldHelp(picker, target);
            else if (e.KeyCode == Keys.F2 && DatabaseHelper.CurrentOffice == 0)
                GetStudentNameFromFieldData(picker.Value.ToStandardString(), target);
            else if (e.KeyCode == Keys.F3)
                SearchWithFieldData(picker.Value.ToStandardString(), target);
            else if (e.KeyCode == Keys.F7)
                DateCounting(target, false);
            else if (e.KeyCode == Keys.F8)
                DateCounting(target, true);
        }

        private void FieldHelp(Guna2TextBox textbox, TargetField target)
        {
            FieldData[] data = DatabaseHelper.FieldSearch(target);
            if (data is null) return;
            ListViewDialog lvd = new ListViewDialog(GetColumnTitle(target), data);
            if (lvd.ShowDialog() != DialogResult.OK || lvd.SelectedIndex == -1) return;

            textbox.Text = data[lvd.SelectedIndex].Text;
        }

        private void FieldHelp(Guna2DateTimePicker picker, TargetField target)
        {
            FieldData[] data = DatabaseHelper.FieldSearch(target);
            if (data is null) return;
            ListViewDialog lvd = new ListViewDialog(GetColumnTitle(target), data);
            if (lvd.ShowDialog() != DialogResult.OK || lvd.SelectedIndex == -1) return;

            picker.Value = data[lvd.SelectedIndex].Text.ToStandardDateTime();
        }

        private void GetStudentNameFromFieldData(string text, TargetField target)
        {
            FieldData[] data = DatabaseHelper.FieldSearch(target, text);
            if (data is null) return;
            ListViewDialog lvd = new ListViewDialog(GetColumnTitle(target) + ": " + text, data);
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

        private void SearchWithFieldData(string text, TargetField target)
        {
            StudentSearchRowData[] data = DatabaseHelper.SelectStudents(target, text, stdOfficeCheckBox.Checked ? stdOfficeSearch.SelectedIndex : 0);
            if (data is null || data.Length == 0) return;

            stdNationalCheckBox.Checked = false;
            stdNameCheckBox.Checked = false;
            stdPhoneCheckBox.Checked = false;
            stdEmailCheckBox.Checked = false;
            stdBirthDateCheckBox.Checked = false;
            if (stdOfficeSearch.SelectedIndex == 0) stdOfficeCheckBox.Checked = false;
            CancelBtn_Click(null, null);

            if (target == TargetField.StudentBirthDate)
                SetSelectedSearch(text.ToStandardDateTime(), 0);

            AddStudentRowsInSearchPanel(data);
        }

        private void DateCounting(TargetField target, bool perYear)
        {
            FieldData[] data = DatabaseHelper.DateFieldSearch(target, perYear);
            if (data is null) return;
            ListViewDialog lvd = new ListViewDialog(GetColumnTitle(target), data);
            if (lvd.ShowDialog() != DialogResult.OK || lvd.SelectedIndex == -1) return;

            StudentSearchRowData[] rowData = DatabaseHelper.SelectStudents(target, data[lvd.SelectedIndex].Text, stdOfficeCheckBox.Checked ? stdOfficeSearch.SelectedIndex : 0, true);
            if (rowData is null || rowData.Length == 0) return;

            stdNationalCheckBox.Checked = false;
            stdNameCheckBox.Checked = false;
            stdPhoneCheckBox.Checked = false;
            stdEmailCheckBox.Checked = false;
            stdBirthDateCheckBox.Checked = false;
            if (stdOfficeSearch.SelectedIndex == 0) stdOfficeCheckBox.Checked = false;
            CancelBtn_Click(null, null);

            if (target == TargetField.StudentBirthDate)
                SetSelectedSearch((data[lvd.SelectedIndex].Text + (perYear ? "-01-01" : "-01")).ToStandardDateTime(), perYear ? 2 : 1);
            
            AddStudentRowsInSearchPanel(rowData);
        }

        private void SetSelectedSearch(DateTime date, int state)
        {
            stdBirthDateCheckBox.Checked = true;
            if (state == 0)
            {
                stdBirthDateFromCheckBox.Checked = false;
                stdBirthDateToCheckBox.Checked = false;
                stdBirthDateSearch.Value = date;
            }
            else if (state == 1)
            {
                stdBirthDateFromCheckBox.Checked = true;
                stdBirthDateToCheckBox.Checked = true;
                stdBirthDateSearch.Value = new DateTime(date.Year, date.Month, 1);
                stdBirthDateToSearch.Value = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            }
            else if (state == 2)
            {
                stdBirthDateFromCheckBox.Checked = true;
                stdBirthDateToCheckBox.Checked = true;
                stdBirthDateSearch.Value = new DateTime(date.Year, 1, 1);
                stdBirthDateToSearch.Value = new DateTime(date.Year, 12, 31);
            }
        }

        private static string GetColumnTitle(TargetField target)
        {
            switch (target)
            {
                case TargetField.StudentName: return "اسم الطالب";
                case TargetField.StudentBirthDate: return "تاريخ الميلاد";
                case TargetField.StudentJob: return "وظيفة الطالب";
                case TargetField.FatherQualification: return "مؤهل الأب";
                case TargetField.MotherQualification: return "مؤهل الأم";
                case TargetField.FatherJob: return "وظيفة الأب";
                case TargetField.MotherJob: return "وظيفة الأم";
                case TargetField.GuardianName: return "ولي الأمر";
                case TargetField.GuardianLink: return "الصلة بالطالب";
                case TargetField.GuardianBirthDate: return "ميلاد ولي الأمر";
                case TargetField.Address: return "عنوان الطالب";
                case TargetField.School: return "المدرسة/الكلية";
                case TargetField.Class: return "الفصل الدراسي";
                case TargetField.MaritalStatus: return "الحالة الاجتماعية";
                case TargetField.MemoAmount: return "مقدار الحفظ";
                case TargetField.JoiningDate: return "تاريخ الانضمام للمكتب";
                case TargetField.FirstConclusionDate: return "الختمة الأولى";
                case TargetField.StudentTeacher: return "مدرس الطالب";
                case TargetField.StudentGroup: return "مجموعة الطالب";
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
            CompetitionRankData[] ranks = DatabaseHelper.SelectCompetitionRanks(compLevel.SelectedIndex, compDateFrom.Value.ToStandardStringWithoutDay(), compDateTo.Value.ToStandardStringWithoutDay(), officeRank.SelectedIndex);
            if (ranks is null)
            {
                ErrorMessage();
                return;
            }

            bool isLevelZero = compLevel.SelectedIndex == 0;
            setRanksBtn.Enabled = officeRank.SelectedIndex == 0 && !isLevelZero;
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
            ranksListPanel.Controls.Add(new StudentRankRow(isLevelZero) { Location = new Point(20, 9) });

            StudentRankRow stdRow;
            for (int i = 0; i < ranks.Length; i++)
            {
                stdRow = new StudentRankRow(ranks[i], isLevelZero ? 0 : i + 1);
                stdRow.Location = new Point(20, (stdRow.Size.Height + 3) * (i + 1) + 9);
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

            float scr = 99999; int rnk = 0, rankShift = 0;
            StudentRankRow row;
            for (int i = 1; i < ranksListPanel.Controls.Count; i++)
            {
                row = (StudentRankRow)ranksListPanel.Controls[i];

                if (row.CompetitionRankData.Score < scr)
                {
                    rnk += rankShift + 1;
                    rankShift = 0;
                    scr = row.CompetitionRankData.Score;
                }
                else if (standardRankingCheckBox.Checked)
                    rankShift += 1;
                
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

            ExcelRowData[] rows = DatabaseHelper.SelectExcelRowData(0, 0, stdOfficeCheckBox.Checked ? stdOfficeSearch.SelectedIndex : 0);
            if (rows is null)
            {
                ErrorMessage();
                return;
            }

            ExcelHelper.ExtractExcel(rows, saveExcelFileDialog.FileName);
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

            if (Offices.Contains(officeTextBox.Text))
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
                containerPanel.FillColor = Color.FromArgb(192, 192, 255);
                containerPanel.FillColor2 = Color.FromArgb(128, 128, 255);
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
                    if (!Color01Execute()) ColorBtn_Click(null, null);
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

        private bool Color01Execute()
        {
            if (File.Exists("Color01.txt"))
            {
                string[] arr = string.Join("", File.ReadAllText("Color01.txt").Where(c => c == '|' || c == ',' || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))).Split('|');
                if (arr.Length == 7)
                {
                    SetColor(ParseColor(arr[0]), ParseColor(arr[1]));
                    ForeColor = ParseColor(arr[2]);
                    SetTextBoxColor(ParseColor(arr[3]));
                    SetTextBoxSearchColor(ParseColor(arr[4]));
                    SetCompoBoxColor(ParseColor(arr[5]));
                    SetRedTextColor(ParseColor(arr[6]));
                    return true;
                }
            }

            return false;
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
            closeBtn2.ForeColor = ForeColor;
        }

        private void SetColor(Color clr1, Color clr2)
        {
            containerPanel.FillColor = clr1;
            containerPanel.FillColor2 = clr2;
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

            if (System.Text.RegularExpressions.Regex.IsMatch(input, @"^[A-Fa-f0-9]{6}$"))
                return Color.FromArgb(
                    int.Parse(input.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(input.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(input.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)
                    );
            
            Color known = Color.FromName(input);
            if (known.IsKnownColor) return known;

            return Color.Black;
        }
        #endregion

    }
}

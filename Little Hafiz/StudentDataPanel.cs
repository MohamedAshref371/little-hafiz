using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public partial class Form1 : Form
    {
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
        }

        private void AddStudentBtn_Click(object sender, EventArgs e)
        {
            if (stdNational.Text.Length != 14 || wrongValueLabel.Visible)
            {
                MessageBox.Show("أدخل الرقم القومي الصحيح");
                return;
            }

            if (DatabaseHelper.AddStudent(GetStudentData()) != -1)
                studentDataPanel.Visible = false;
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
    }
}

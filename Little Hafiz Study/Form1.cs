using DocumentFormat.OpenXml.Office2010.Excel;
using Guna.UI2.WinForms;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //studentDataForm.HorizontalScroll.Maximum = 0;
            //studentDataForm.AutoScroll = false;
            //studentDataForm.VerticalScroll.Visible = false;
            //studentDataForm.AutoScroll = true;
        }

        private void CloseBtn_Click(object sender, EventArgs e)
            => Close();
        
        private void MinimizeBtn_Click(object sender, EventArgs e)
            => WindowState = FormWindowState.Minimized;
        
        #region StudentDataForm
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

        private void StudentDataForm_Scroll(object sender, ScrollEventArgs e)
            => studentDataForm.Invalidate();

        private void StdBrothers_ValueChanged(object sender, EventArgs e)
            => stdArrangement.Maximum = stdBrothers.Value + 1;

        private void StdImageSelectorBtn_Click(object sender, EventArgs e)
        {
            if (selectImageDialog.ShowDialog() == DialogResult.OK)
                stdImagePath.Text = selectImageDialog.FileName;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {

        }

        private void AddStudentBtn_Click(object sender, EventArgs e)
        {

        }
        #endregion

    }
}

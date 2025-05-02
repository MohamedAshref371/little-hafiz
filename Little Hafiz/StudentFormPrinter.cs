using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public class StudentFormPrinter
    {
        private readonly Image backgroundImage;
        private readonly StudentFormData data;
        private readonly PrintDocument printDocument;

        public StudentFormPrinter(StudentFormData data)
        {
            this.data = data;
            backgroundImage = new Bitmap(2480, 3508);

            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        public void ShowPreview()
        {
            PrintPreviewDialog previewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
                WindowState = FormWindowState.Maximized
            };
            previewDialog.ShowDialog();
        }

        public void Print()
        {
            printDocument.Print();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            float scaleX = (float)e.PageBounds.Width / backgroundImage.Width;
            float scaleY = (float)e.PageBounds.Height / backgroundImage.Height;

            g.DrawImage(backgroundImage, new Rectangle(0, 0, e.PageBounds.Width, e.PageBounds.Height));

            Font font = new Font("Arial", 11, FontStyle.Bold);
            Brush brush = Brushes.Black;

            g.DrawString(data.FullName, font, brush, scaleX * 580, scaleY * 70);
            g.DrawString(data.NationalNumber, font, brush, scaleX * 130, scaleY * 145);

        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace Little_Hafiz
{
    public class StudentFormPrinter
    {
        private readonly StudentFormData data;
        private readonly PrintDocument printDocument;
        private readonly PrintPreviewDialog previewDialog;

        public StudentFormPrinter(StudentFormData data)
        {
            this.data = data;
            printDocument = new PrintDocument();

            PaperSize a4Size = new PaperSize("A4", 827, 1169) { RawKind = (int)PaperKind.A4 };
            printDocument.DefaultPageSettings.PaperSize = a4Size;
            printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            previewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
                WindowState = FormWindowState.Maximized
            };

            paragraphs = new string[] { data.StudentMashaykh, data.MemorizePlaces, data.Certificates, data.Ijazah, data.Courses, data.Skills, data.Hobbies, data.StdComps, data.Notes };
        }

        public void ShowPreview()
        {
            printDocument.PrintPage += MultiPage_PrintPage;
            previewDialog.ShowDialog();
            printDocument.PrintPage -= MultiPage_PrintPage;
        }

        private void MultiPage_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            if (currentParagraphIndex == 0)
            {
                SetSmallFields(g);
                PrintBigFields(paragraphs, e, 670);
            }
            else
                PrintBigFields(paragraphs, e, 30);
        }

        private void SetSmallFields(Graphics g)
        {
            Brush brush = Brushes.Black;
            Font font = new Font("Arial", 16, FontStyle.Regular);
            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            g.DrawString(System.DateTime.Now.ToString(), new Font("Arial", 12), brush, new RectangleF(0, 1140, 200, 30), format);
            g.DrawString(data.PaperTitle, new Font("Arial", 20, FontStyle.Bold), brush, new RectangleF(250, 30, 300, 30), format);

            Rectangle imgRect = new Rectangle(10, 10, 140, 180);
            int radius = 10;

            using (GraphicsPath path = GetRoundedRectPath(imgRect, radius))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Region oldClip = g.Clip;
                g.SetClip(path);
                g.DrawImage(data.StudentImage, imgRect);
                g.Clip = oldClip;

                using (Pen pen = new Pen(Color.Black, 1))
                    g.DrawPath(pen, path);
            }

            g.DrawString(data.FullName, font, brush, new RectangleF(300, 80, 500, 30), format);
            g.DrawString(data.NationalNumber, font, brush, new RectangleF(300, 120, 500, 30), format);
            g.DrawString(data.BirthDate, font, brush, new RectangleF(300, 160, 500, 30), format);

            g.DrawString(data.Job, font, brush, new RectangleF(400, 200, 400, 30), format);
            g.DrawString(data.MaritalStatus, font, brush, new RectangleF(10, 200, 380, 30), format);

            g.DrawString(data.FatherQualification, font, brush, new RectangleF(400, 250, 400, 30), format);
            g.DrawString(data.FatherJob, font, brush, new RectangleF(10, 250, 380, 30), format);
            g.DrawString(data.MotherQualification, font, brush, new RectangleF(400, 290, 400, 30), format);
            g.DrawString(data.MotherJob, font, brush, new RectangleF(10, 290, 380, 30), format);
            g.DrawString(data.FatherPhone, font, brush, new RectangleF(400, 330, 400, 30), format);
            g.DrawString(data.MotherPhone, font, brush, new RectangleF(10, 330, 380, 30), format);
            g.DrawString(data.GuardianName, font, brush, new RectangleF(400, 370, 400, 30), format);
            g.DrawString(data.GuardianLink, font, brush, new RectangleF(10, 370, 380, 30), format);

            g.DrawString(data.PhoneNumber, font, brush, new RectangleF(400, 420, 400, 30), format);
            g.DrawString(data.Address, font, brush, new RectangleF(10, 420, 380, 30), format);
            g.DrawString(data.Email, font, brush, new RectangleF(400, 460, 400, 30), format);
            g.DrawString(data.Facebook, font, brush, new RectangleF(10, 460, 380, 30), format);
            g.DrawString(data.School, font, brush, new RectangleF(400, 500, 400, 30), format);
            g.DrawString(data.Class, font, brush, new RectangleF(10, 500, 380, 30), format);

            g.DrawString(data.BrothersCount, font, brush, new RectangleF(400, 540, 400, 30), format);
            g.DrawString(data.ArrangementBetweenBrothers, font, brush, new RectangleF(10, 540, 380, 30), format);

            g.DrawString(data.OfficeName, font, brush, new RectangleF(400, 580, 400, 30), format);
            g.DrawString(data.MemorizationAmount, font, brush, new RectangleF(10, 580, 380, 30), format);
            g.DrawString(data.JoiningDate, font, brush, new RectangleF(400, 620, 400, 30), format);
            g.DrawString(data.FirstConclusionDate, font, brush, new RectangleF(10, 620, 380, 30), format);
        }

        readonly string[] paragraphs;
        int currentParagraphIndex = 0;
        private void PrintBigFields(string[] paragraphs, PrintPageEventArgs e, float y)
        {
            float margin = 10;
            Font font = new Font("Arial", 16);
            Brush brush = Brushes.Black;
            StringFormat format = new StringFormat { FormatFlags = StringFormatFlags.DirectionRightToLeft };
            string text;
            while (currentParagraphIndex < paragraphs.Length)
            {
                text = paragraphs[currentParagraphIndex];
                if (text.Trim().Last() == ':') { currentParagraphIndex++; continue; }
                SizeF size = e.Graphics.MeasureString(text, font, 790, format);

                if (y + size.Height > e.MarginBounds.Bottom - 30)
                {
                    e.HasMorePages = true;
                    return;
                }
                e.Graphics.DrawString(text, font, brush, new RectangleF(10, y, 790, size.Height), format);
                y += size.Height + margin;
                currentParagraphIndex++;
            }

            e.HasMorePages = false;
        }

        public GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}

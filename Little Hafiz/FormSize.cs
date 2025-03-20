using Little_Hafiz;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuranKareem
{
    class FormSize
    {
        private readonly double xDiv, yDiv;

        public FormSize(int oldSizeX, int oldSizeY, int newSizeX, int newSizeY)
        {
            xDiv = newSizeX / (double)oldSizeX;
            yDiv = newSizeY / (double)oldSizeY;
        }

        public void SetControl(Control control, bool setFont = true)
        {
            control.Location = new Point(Round(control.Location.X * xDiv), Round(control.Location.Y * yDiv));
            control.Size = new Size(Round(control.Size.Width * xDiv), Round(control.Size.Height * yDiv));
            if (setFont) control.Font = new Font(control.Font.FontFamily, Round(control.Font.Size * (xDiv < yDiv ? xDiv : yDiv)));
        }

        public void SetControls(Control.ControlCollection controls)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                SetControl(controls[i]);
#warning Part " || controls[i] is StudentSearchRow" Make the Class unreusable.
                if (controls[i] is Panel || controls[i] is StudentSearchRow)
                    SetControls(controls[i].Controls);
            }
        }

        public Point GetNewLocation(Point p) => new Point(Round(p.X * xDiv), Round(p.Y * yDiv));

        public Size GetNewSize(Size sz) => new Size(Round(sz.Width * xDiv), Round(sz.Height * yDiv));

        public int GetNewX(int x) => Round(x * xDiv);

        public int GetNewY(int y) => Round(y * yDiv);
        
        public static int Round(double num) => (int)Math.Round(num);
        
    }
}

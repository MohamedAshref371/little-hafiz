using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Little_Hafiz
{
    public class BarcodeScanner
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private bool isScanning = false;

        private readonly Guna2TextBox targetTextBox;
        private readonly Guna2NumericUpDown numUpDown;
        private readonly Form ownerForm;
        private readonly BarcodeReader reader;

        public BarcodeScanner(Form ownerForm, Guna2TextBox targetTextBox, Guna2NumericUpDown numUpDown)
        {
            this.ownerForm = ownerForm;
            this.targetTextBox = targetTextBox;
            this.numUpDown = numUpDown;
            reader = new BarcodeReader();
        }

        public bool Start()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0) return false;

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(Video_NewFrame);
            videoSource.Start();
            isScanning = true;
            return true;
        }

        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!isScanning) return;
            var result = reader.Decode(eventArgs.Frame);

            if (result != null)
            {
                isScanning = false;

                string nat, code = null;
                if (result.Text.Length >= 14)
                {
                    nat = result.Text.Substring(0, 14);
                    if (result.Text.Length > 14)
                        code = result.Text.Substring(14);
                }
                else return;

                ownerForm.Invoke(new Action(() =>
                {
                    targetTextBox.Text = nat;
                    if (code != null && int.TryParse(code, out int val)) numUpDown.Value = val;
                    targetTextBox.Focus();
                    SendKeys.SendWait("{ENTER}");
                }));

                Stop();
            }
        }

        public void Stop()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource = null;
            }
        }
    }
}
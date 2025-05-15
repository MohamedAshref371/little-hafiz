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
        private readonly Guna2CheckBox checkBox;
        private readonly Form ownerForm;
        private readonly BarcodeReader reader;

        public BarcodeScanner(Form ownerForm, Guna2TextBox targetTextBox, Guna2CheckBox checkBox, Guna2NumericUpDown numUpDown)
        {
            this.ownerForm = ownerForm;
            this.targetTextBox = targetTextBox;
            this.numUpDown = numUpDown;
            this.checkBox = checkBox;
            reader = new BarcodeReader();
        }

        public bool Start()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0) return false;

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.Start();
            return true;
        }

        public void Resume()
        {
            videoSource.NewFrame += Video_NewFrame;
            isScanning = true;
        }

        private void Video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!isScanning) return;
            var result = reader.Decode(eventArgs.Frame);
            
            if (result != null)
            {
                string nat, code = null;
                if (targetTextBox.Visible && result.Text.Length >= 14)
                {
                    nat = result.Text.Substring(0, 14);
                    if (checkBox.Checked && result.Text.Length > 14)
                        code = result.Text.Substring(14);
                }
                else return;
                isScanning = false;

                ownerForm.Invoke(new Action(() =>
                {
                    targetTextBox.Text = nat;
                    if (code != null && int.TryParse(code, out int val)) numUpDown.Value = val;
                    Program.Form.NationalEnter();
                }));
            }
        }

        public void Pause()
        {
            videoSource.NewFrame -= Video_NewFrame;
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
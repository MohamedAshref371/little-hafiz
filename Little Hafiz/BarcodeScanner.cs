using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Linq;

namespace Little_Hafiz
{
    public class BarcodeScanner
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private bool isScanning = false;

        private readonly BarcodeReader reader;

        public BarcodeScanner()
        {
            reader = new BarcodeReader();
        }

        public string[] Init()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            return videoDevices.Cast<FilterInfo>().Select(f => f.Name).ToArray();
        }

        public void Start(int i = 0)
        {
            videoSource = new VideoCaptureDevice(videoDevices[i].MonikerString);
            videoSource.Start();
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
                isScanning = false;
                Program.Form.AfterQRCodeScanner(result.Text);
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
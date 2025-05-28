using System;
using System.IO;
using System.Windows.Forms;

namespace Little_Hafiz
{
    internal static class Program
    {
        public static Form1 Form;
        public static bool Record = true;

        [STAThread]
        static void Main()
        {
            //AssemblyResolver.Initialize();

            if (!File.Exists("Guna.UI2.dll"))
            {
                MessageBox.Show("Guna.UI2.dll file is missing.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (sender, e) =>
                LogError(e.Exception);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                LogError(ex);
            };

            try
            {
                System.Threading.Mutex mutex = new System.Threading.Mutex(true, Path.GetFullPath("data").Replace(":", "").Replace("\\", ""), out bool createdNew);
                if (createdNew)
                {
                    Form = new Form1();
                    Application.Run(Form);
                    mutex.ReleaseMutex();
                }
                else MessageBox.Show("لقد فتحت البرنامج بالفعل");
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show("حدث خطأ غير متوقع، سيتم إغلاق البرنامج");
            }
        }

        public static void LogError(Exception ex, bool inTryCatch = false)
            => File.AppendAllText("Errors.txt", $"{DateTime.Now}{(inTryCatch ? "  -  Inside Custom Try-Catch Block" : "")}\n{ex.Message}\n{ex.StackTrace}\n------------------\n\n");
        
    }
}

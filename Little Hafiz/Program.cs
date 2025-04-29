using System;
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
            AssemblyResolver.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (sender, e) =>
                LogError(e.Exception.Message, e.Exception.StackTrace);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                LogError(ex.Message, ex.StackTrace);
            };

            try
            {
                System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName + Application.CompanyName, out bool createdNew);
                if (createdNew)
                {
                    Record = Properties.Settings.Default.RecordEnabled;
                    Form = new Form1();
                    Application.Run(Form);
                    mutex.ReleaseMutex();
                }
                else MessageBox.Show("هناك نسخة من البرنامج مفتوحة");
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex.StackTrace);
                MessageBox.Show("حدث خطأ غير متوقع، سيتم إغلاق البرنامج");
            }
        }

        public static void LogError(string msg, string stack, bool inTryCatch = false)
        {
            if (!System.IO.File.Exists("Errors.txt"))
                System.IO.File.WriteAllText("Errors.txt", "");

            System.IO.File.AppendAllText("Errors.txt", $"{DateTime.Now}{(inTryCatch ? "  -  Inside Custom Try-Catch Block" : "")}\n{msg}\n{stack}\n------------------\n\n");
        }
    }
}

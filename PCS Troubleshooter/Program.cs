using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PCS_Troubleshooter
{
    static class Program
    {
        public static string Version = "1.2";

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool wow64Process);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public extern static Int32 SetWindowTheme(IntPtr hWnd, String textSubAppName, String textSubIdList);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public static void SaveException(Exception ex)
        {
            string directory = Path.Combine(Application.StartupPath, "StackTraces");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string file = string.Format("{0}\\StackTraces\\stackTrace.{1}.log",
                Application.StartupPath,
                string.Format("{0:dd_MM_yyyy_HH_mm_ss}", DateTime.Now));

            File.WriteAllText(file, ex.ToString());
        }
    }
}
using System;
using System.Windows.Forms;

namespace MyPasswordManager // Ensure this matches your project name!
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // This line tells the computer to launch YOUR code
            Application.Run(new Form1());
        }
    }
}
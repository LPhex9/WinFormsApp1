using System;
using System.Windows.Forms;

namespace MyPasswordManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 1. Standard WinForms setup
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 2. Open the Login window first
            using (LoginForm login = new LoginForm())
            {
                // This line pauses the code here and shows the Login screen
                Application.Run(login);

                // 3. Once the Login window closes, check if they actually logged in
                if (login.IsAuthenticated)
                {
                    // 4. THIS IS THE FIX: We pass the password from the login form 
                    // directly into the Form1 constructor.
                    Application.Run(new Form1(login.MasterPassword));
                }
                else
                {
                    // If they just closed the login window without unlocking, 
                    // the app simply exits.
                    Application.Exit();
                }
            }
        }
    }
}
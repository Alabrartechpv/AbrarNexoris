using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show splash screen first
            using (FrmSplashScreen splash = new FrmSplashScreen())
            {
                splash.ShowDialog();
            }

            // Then show the login form
            Application.Run(new Login());
        }
    }
}

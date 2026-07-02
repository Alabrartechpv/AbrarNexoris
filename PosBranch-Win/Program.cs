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

            bool runLogin = true;
            if (Utilities.InitialSetupHelper.IsDatabaseEmpty())
            {
                using (Utilities.FrmInitialSetup setup = new Utilities.FrmInitialSetup())
                {
                    Application.Run(setup);
                    // If they closed/cancelled setup without seeding the database, do not run Login
                    if (Utilities.InitialSetupHelper.IsDatabaseEmpty())
                    {
                        runLogin = false;
                    }
                }
            }

            if (runLogin)
            {
                // Then show the login form
                Application.Run(new Login());
            }
        }
    }
}

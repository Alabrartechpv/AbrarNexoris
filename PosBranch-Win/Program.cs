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

            // Verify database connection before proceeding
            bool connectionOk = false;
            while (!connectionOk)
            {
                if (TryConnectDatabase(out string error))
                {
                    connectionOk = true;
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        $"Database connection could not be established:\n\n{error}\n\nDo you want to configure connection settings now?",
                        "Database Connection Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);

                    if (result == DialogResult.Yes)
                    {
                        using (Connection connForm = new Connection())
                        {
                            if (connForm.ShowDialog() != DialogResult.OK)
                            {
                                // User cancelled database settings, exit
                                return;
                            }
                        }
                    }
                    else
                    {
                        // User chose not to configure connection and exit
                        return;
                    }
                }
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

        private static bool TryConnectDatabase(out string errorMessage)
        {
            errorMessage = null;
            Repository.BaseRepostitory repo = null;
            try
            {
                repo = new Repository.BaseRepostitory();
                if (repo.DataConnection == null)
                {
                    errorMessage = "SQL Connection string is missing or invalid in configuration file C:\\Connection\\Config.txt.";
                    return false;
                }
                repo.DataConnection.Open();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (repo != null)
                {
                    repo.Dispose();
                }
            }
        }
    }
}

using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Repository
{
    public class BaseRepostitory : IDisposable
    {
        public IDbConnection DataConnection;
        bool disposed = false;
        DataBase db = new DataBase();
        public BaseRepostitory()
        //test sibi ---
        //hgjhgjhgjhhj
        //change by Shaji
        //change by Ashlydd
        {
            // DataBase.Status = "Online";
            DataBase.Status = "Local";
            if (DataBase.Status == "Local")
            {
                string txtpath = @"C:\Connection\Config.txt";
                try
                {
                    if (File.Exists(txtpath))
                    {
                        using (StreamReader sr = new StreamReader(txtpath))
                        {
                            string ss = sr.ReadLine();
                            if (string.IsNullOrWhiteSpace(ss))
                                throw new Exception("Config file is empty");

                            string[] txtsplit = ss.Split(';');
                            if (txtsplit.Length < 4)
                                throw new Exception("Config file format is invalid. Expected: Server;Database;UserID;Password");

                            string server = txtsplit[0].Trim();
                            string database = txtsplit[1].Trim();
                            string userid = txtsplit[2].Trim(); // user id
                            string password = txtsplit[3].Trim(); // password

                            // Build proper SQL connection string
                            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                            builder.DataSource = server;
                            builder.InitialCatalog = database;
                            builder.UserID = userid;
                            builder.Password = password;
                            builder.TrustServerCertificate = true;

                            DataConnection = new SqlConnection(builder.ConnectionString);
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException($"Database configuration file not found at: {txtpath}. Please create the config file with format: Server;Database;UserID;Password");
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {
                string Local = "Data Source = 192.168.1.232\\SQLEXPRESS; Initial Catalog = RambaiTest; User ID = sa; Password =Abrar@123;";
                DataConnection = new SqlConnection(Local);
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (DataConnection != null)
                    {
                        if (DataConnection.State == ConnectionState.Open)
                        {
                            DataConnection.Close();
                        }
                        DataConnection.Dispose();
                        DataConnection = null;
                    }
                }
                disposed = true;
            }
        }

        ~BaseRepostitory()
        {
            Dispose(false);
        }
    }
}

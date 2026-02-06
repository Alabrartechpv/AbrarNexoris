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
                            string[] txtsplit = ss.Split(';');
                            string server = txtsplit[0].ToString();
                            string DataBase = txtsplit[1].ToString();
                            string userid = txtsplit[2].ToString(); // user id
                            string password = txtsplit[3].ToString(); // password
                                                                      // string Security = txtsplit[3].ToString();
                            string Local = server + ';' + DataBase + ';' + userid + ';' + password + ';';//+ Security

                            DataConnection = new SqlConnection(Local);
                        }
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

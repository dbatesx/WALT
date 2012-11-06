using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Data.Common;

namespace WALT.DAL
{
    public class Database
    {
        static string _dbName = ConfigurationManager.AppSettings["DBName"];
        static string _dbServer = ConfigurationManager.AppSettings["DBServer"];
        static string _dbPool = ConfigurationManager.AppSettings["DBPool"];
        static int _connections = 0;

        SqlConnection _conn;
        DataClasses1DataContext _context;
        DbTransaction _transaction;
        int _transCount = 0;
        string _connstr;

        /// <summary>
        /// 
        /// </summary>
        public Database()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetNumConnections()
        {
            return _connections;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Connect()
        {
            try
            {
                /* 
                 * Read in the database patches from the embedded resource files
                 * and apply if they have not been entered in the patch table.
                 * If the database does not exist yet, then create it.
                 * 
                 */

                if (_dbServer != null && _dbServer.Length == 0)
                {
                    ConnectFile();
                }
                else
                {
                    ConnectServer();
                }

                lock (this)
                {
                    _connections++;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ConnectFile()
        {
            /*
             * If the recreate flag is set, then delete the old files, so the database
             * is recreated from scratch automatically.
             * 
             */

            string dbpath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filename = dbpath + "\\" + _dbName + "_log.ldf";

            filename = dbpath + "\\" + _dbName + ".mdf";

            if (!System.IO.File.Exists(filename))
            {
                _conn = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=tempdb;Integrated Security=True;User Instance=True");
                _conn.Open();
                var cmd = _conn.CreateCommand();
                cmd.CommandText = "create database " + _dbName + " on primary (name=" + _dbName + ",filename='" + filename + "')";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "exec sp_detach_db '" + _dbName + "', 'true'";
                cmd.ExecuteNonQuery();
                _conn.Close();
            }

            _connstr =
                "Data Source=.\\SQLEXPRESS;AttachDbFilename=\"" +
                filename +
                "\";Integrated Security=True;User Instance=True;";

            _conn = new SqlConnection(_connstr);

            _conn.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        void ConnectServer()
        {
            _connstr = "Server=" + _dbServer + ";Database=" + _dbName + ";Trusted_Connection=true;";

            if (_dbPool == "0")
            {
                _connstr += "Pooling=false;";
            }
            else
            {
                _connstr += "Min Pool Size=0;Max Pool Size=" + _dbPool;
            }

            _conn = new SqlConnection(_connstr);

            _conn.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetConnection()
        {
            return _conn;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Upgrade()
        {
            try
            {
                var cmd = _conn.CreateCommand();
                cmd.CommandText = "create table patches (" +
                    "id bigint primary key identity," +
                    "name varchar(32) not null," +
                    "installed datetime not null default getdate());";
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Table must already exist
            }

            /*
             * Get the patches that are stored as embedded resources.
             */

            Assembly a = Assembly.GetExecutingAssembly();
            List<string> names = new List<string>(a.GetManifestResourceNames());
            names.Sort();

            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Contains("WALT.DAL.sql"))
                {
                    try
                    {
                        var cmd = _conn.CreateCommand();
                        cmd.CommandText = "select * from patches where name = @name";
                        cmd.Parameters.Add(new SqlParameter("@name", names[i]));
                        SqlDataReader dr = cmd.ExecuteReader();
                        bool rows = dr.HasRows;
                        dr.Close();

                        if (rows == false)
                        {
                            SqlTransaction t = _conn.BeginTransaction();

                            StreamReader sr = new StreamReader(
                                a.GetManifestResourceStream(names[i]));

                            string s;
                            string sql = "";
                            int line = 1;

                            while ((s = sr.ReadLine()) != null)
                            {
                                if ((s.Length > 0) && (s.StartsWith("--") == false))
                                {
                                    sql += s;

                                    if (sql.Contains(";"))
                                    {
                                        Debug.WriteLine(names[i] + ":" + line + ":" + sql);
                                        cmd = _conn.CreateCommand();
                                        cmd.CommandText = sql;
                                        cmd.Transaction = t;
                                        cmd.ExecuteNonQuery();
                                        sql = "";
                                    }
                                }

                                line++;
                            }

                            sr.Close();

                            cmd = _conn.CreateCommand();
                            cmd.CommandText = "insert into patches (name) values ('" +
                                names[i] + "')";
                            cmd.Transaction = t;
                            cmd.ExecuteNonQuery();

                            t.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }

            /* 
             * Make sure the actions table is up to date with the
             * DTO enumeration
             */

            foreach (string s in Enum.GetNames(typeof(DTO.Action)))
            {
                var query = from item in _context.actions
                            where item.title == s
                            select item;

                if (query.Count() == 0)
                {
                    action act = new action();
                    act.title = s;
                    _context.actions.InsertOnSubmit(act);
                    _context.SubmitChanges();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataClasses1DataContext GetContext()
        {
            return _context;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            _conn.Close();

            lock (this)
            {
                _connections--;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SubmitChanges()
        {
            try
            {
                _context.SubmitChanges();
            }
            catch
            {
                CancelTransaction();
                _context.Dispose();
                _context = new DataClasses1DataContext(_conn);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void BeginTransaction()
        {
            if (_transCount == 0)
            {
                _transaction = _conn.BeginTransaction();
                _context.Transaction = _transaction;
            }

            _transCount++;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CommitTransaction()
        {
            if (_transCount == 1)
            {
                _transaction.Commit();
                _transaction.Dispose();
            }

            if (_transCount > 0)
            {
                _transCount--;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelTransaction()
        {
            if (_transCount > 0)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transCount = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Refresh()
        {
            if (_conn == null || 
                _conn.State == System.Data.ConnectionState.Broken ||
                _conn.State == System.Data.ConnectionState.Closed)
            {
                Connect();
            }

            if (_context != null)
            {
                _context.Dispose();
            }

            _context = new DataClasses1DataContext(_conn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Done()
        {
            bool result = false;

            if (_dbPool != "0")
            {
                Close();
                result = true;
            }

            return result;
        }
    }
}

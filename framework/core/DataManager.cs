using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Xml.Serialization;

namespace Medtrix.Trace
{
    public class Logger
    {
        private static string LogFile = String.Empty;
        static Logger()
        {
            LogFile = System.Web.HttpContext.Current.Server.MapPath("..") + "/service/log.txt";
        }

        public static void Log(string msg)
        {
            try
            {
                System.IO.File.AppendAllText(LogFile, DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.fff") + " : " + msg + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
    }
}


namespace Medtrix.DataAccessControl
{
    public enum ExecutionStatus
    {
        ExecuteSuccess = 0,
        ExecuteFailed = 1
    }

    public class DataCommand
    {
        String _storedProcedure = String.Empty;
        List<SqlParameter> _params = new List<SqlParameter>();
        public DataCommand(String storedProcedure)
        {
            _storedProcedure = storedProcedure;
        }

        public void Add(String paramName, Object value, ParameterDirection dir = ParameterDirection.Input)
        {
            _params.Add(DataAccessManager.CreateParam(paramName, value, dir));
        }

        public bool ExecuteNonQuery()
        {
            return DataAccessManager.GetInstance().ExecuteNonQuery(_storedProcedure, _params.ToArray());
        }

        public object Execute(bool returnDataSet = false)
        {
            DataSet ds = DataAccessManager.GetInstance().Execute(_storedProcedure, _params.ToArray());

            if (returnDataSet)
                return ds;

            List<List<Dictionary<string, object>>> tables = new List<List<Dictionary<string, object>>>();
            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

            if (ds.Tables.Count == 1)
            {
                table = DataAccessManager.DataTableToJSON(ds.Tables[0]);
                if (ds.Tables[0].Rows.Count == 1)
                    return table[0];
                if (ds.Tables[0].Rows.Count == 0)
                    return null;
                return table;
            }
            else
            {

                foreach (DataTable dt in ds.Tables)
                    tables.Add(DataAccessManager.DataTableToJSON(dt));
            }
            return tables;
        }

        public object ExecuteWithResult(String paramName)
        {
            DataAccessManager.GetInstance().Execute(_storedProcedure, _params.ToArray());

            SqlParameter parameter = _params.FirstOrDefault(param => param.ParameterName == paramName);
            switch (parameter.SqlDbType)
            {
                case SqlDbType.BigInt:
                case SqlDbType.Int:
                case SqlDbType.TinyInt:
                case SqlDbType.SmallInt:
                    return Convert.ToUInt64(parameter.Value);
            }
            return parameter.SqlValue;
        }
    }
    public class DataAccessManager
    {
        private String _ConnectionString = String.Empty;
        private static DataAccessManager _dataConn = null;
        private static object _lock = new object();

        static DataAccessManager()
        {
            GetInstance(System.Configuration.ConfigurationManager.AppSettings["AppDB"]);
        }
        private DataAccessManager(String ConnectionString)
        {
            _ConnectionString = ConnectionString;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
            }
        }

        public static DataAccessManager GetInstance(String ConnectionString = "")
        {
            lock (_lock)
            {
                if (_dataConn == null)
                {
                    if (String.IsNullOrEmpty(ConnectionString))
                        throw new ArgumentNullException("Connection string is missing...");

                    _dataConn = new DataAccessManager(ConnectionString);
                }
            }
            return _dataConn;
        }

        public static List<Dictionary<string, object>> DataTableToJSON(DataTable dt)
        {
            if (dt == null)
                return null;

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName.ToLower(), dr[col]);
                }
                rows.Add(row);
            }
            return rows;
        }

        public static SqlParameter CreateParam(String Name, Object value, ParameterDirection pd)
        {
            SqlParameter param = new SqlParameter(Name, value);
            param.Direction = pd;
            if (value is UInt64)
                param.SqlDbType = SqlDbType.BigInt;
            if (value is String && pd == ParameterDirection.Output)
                param.Size = -1;
            return param;
        }

        public DataSet Execute(String StoreProcedure, SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_ConnectionString))
                {
                    con.Open();
                    SqlCommand command = new SqlCommand(StoreProcedure, con);
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter param in parameters)
                        command.Parameters.Add(param);

                    SqlDataAdapter sda = new SqlDataAdapter(command);
                    DataSet retSet = new DataSet();
                    sda.Fill(retSet);

                    return retSet;
                }
            }
            catch (Exception e)
            {
                Medtrix.Trace.Logger.Log(e.Message);
                return null;
            }
        }

        public bool ExecuteNonQuery(String StoreProcedure, SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_ConnectionString))
                {
                    con.Open();
                    SqlCommand command = new SqlCommand(StoreProcedure, con);
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter param in parameters)
                        command.Parameters.Add(param);

                    command.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception e)
            {
                Medtrix.Trace.Logger.Log(e.Message);
                return false;
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Apzon.Commons;
using Sap.Data.Hana;

namespace SAP.QuickCopyUDF
{
    public class DatabaseHanaClient
    {
        public string ConnectionString { get; set; }

        public HanaConnection DbConnection { get; set; }

        public HanaTransaction DbTransaction { get; set; }

        public DatabaseHanaClient(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            ConnectionString = string.Format("{0}; Pooling=false;Max Pool Size=50;Min Pool Size=5", connectionString);
            DbTransaction = null;
        }

        public DatabaseHanaClient(HanaConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            DbConnection = connection;
            DbTransaction = null;
        }

        public virtual string InsertDataTableIntoSql(DataTable dt)
        {
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                // Copy the DataTable to SQL Server Table using SqlBulkCopy
                using (var sqlBulkCopy = new SqlBulkCopy(ConnectionString))
                {
                    sqlBulkCopy.DestinationTableName = @"""" + dt.TableName + @"""";
                    foreach (var column in dt.Columns)
                        sqlBulkCopy.ColumnMappings.Add(column.ToString(), column.ToString());

                    sqlBulkCopy.WriteToServer(dt);
                    dt.Rows.Clear();
                    return "";
                }
            }
        }

        #region Các hàm thực thi câu lệnh sap hana

        public int ExecuteNonQuery(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var lst = parameters == null ? null : new HanaParameter[parameters.Length];
            var declarePara = ParseParamater(lst, parameters, ref query);
            if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                    BEGIN 
                                    {0}
                                        end;", query, declarePara) : query;
                    command.CommandType = commandType;
                    command.Transaction = DbTransaction;
                    command.TryAddParameters(lst);
                    return command.ExecuteNonQuery();
                }
            }
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                var command = conn.CreateCommand();
                command.CommandTimeout = 60000;
                command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                    BEGIN 
                                        {0}
                                    end;", query, declarePara) : query;
                command.CommandType = commandType;
                command.TryAddParameters(lst);
                var result = command.ExecuteNonQuery();
                //HanaConnection.ClearPool(conn);
                return result;
            }
        }

        public int ExecuteNonQueryNoParam(string query, CommandType commandType = CommandType.Text)
        {
            if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO BEGIN 
                                                    {0}
                                            end;", query) : query;
                    command.Transaction = DbTransaction;
                    command.CommandType = commandType;
                    return command.ExecuteNonQuery();
                }
            }
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                var command = conn.CreateCommand();
                command.CommandTimeout = 60000;
                command.CommandText = commandType == CommandType.Text ? string.Format(@"DO BEGIN 
                                                                            {0}
                                                    end;", query) : query;
                command.CommandType = commandType;
                var result = command.ExecuteNonQuery();
                //HanaConnection.ClearPool(conn);
                return result;
            }
        }

        public object ExecuteScalar(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var lst = parameters == null ? null : new HanaParameter[parameters.Length];
            var declarePara = ParseParamater(lst, parameters, ref query);
            if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                        BEGIN 
                                                            {0}
                                                         end;", query, declarePara) : query;
                    command.Transaction = DbTransaction;
                    command.CommandType = commandType;
                    command.TryAddParameters(lst);
                    return command.ExecuteScalar();
                }
            }
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                var command = conn.CreateCommand();
                command.CommandTimeout = 60000;
                command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                BEGIN 
                                                                    {0}
                                                end;", query, declarePara) : query;
                command.CommandType = commandType;
                command.TryAddParameters(lst);
                var result = command.ExecuteScalar();
                //HanaConnection.ClearPool(conn);
                return result;
            }
        }

        public DataSet ExecuteData(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var lst = parameters == null ? null : new HanaParameter[parameters.Length];
            var declarePara = ParseParamater(lst, parameters, ref query);
            var dtSet = new DataSet();
            if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                                    BEGIN 
                                                                                {0}
                                                                        end;", query, declarePara) : query;
                    command.Transaction = DbTransaction;
                    command.CommandType = commandType;
                    command.TryAddParameters(lst);
                    var adapter = new HanaDataAdapter(command);
                    adapter.Fill(dtSet);
                    command.Parameters.Clear();
                    return dtSet;
                }
            }
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                                        BEGIN 
                                                                                {0}
                                                                        end", query, declarePara) : query;
                    command.CommandType = commandType;
                    command.TryAddParameters(lst);
                    var adapter = new HanaDataAdapter(command);
                    adapter.Fill(dtSet);
                    command.Parameters.Clear();
                }
            }
            return dtSet;
        }

        public DataTable ExecuteDataTable(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            try
            {
                var lst = parameters == null ? null : new HanaParameter[parameters.Length];
                var declarePara = ParseParamater(lst, parameters, ref query);
                var dtSet = new DataTable();
                if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
                {
                    using (var command = DbConnection.CreateCommand())
                    {
                        command.CommandTimeout = 60000;
                        command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                                        BEGIN 
                                                                                {0}
                                                                           end;", query, declarePara) : query;
                        command.CommandType = commandType;
                        command.Transaction = DbTransaction;
                        command.TryAddParameters(lst);
                        var adapter = new HanaDataAdapter(command);
                        adapter.Fill(dtSet);
                        command.Parameters.Clear();
                        return dtSet;
                    }
                }
                using (var conn = new HanaConnection(ConnectionString))
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandTimeout = 60000;
                        command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                                        BEGIN 
                                                                                    {0}
                                                                        end;", query, declarePara) : query;
                        command.CommandType = commandType;
                        command.TryAddParameters(lst);
                        var adapter = new HanaDataAdapter(command);
                        adapter.Fill(dtSet);
                        command.Parameters.Clear();
                        conn.Dispose();
                    }
                }
                return dtSet;
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), query, ex.Message);
                if (null != ex.InnerException)
                    Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), query, Function.ToString(ex.InnerException.Message));
                throw (ex);
            }
        }

        public DataTable ExecuteDataTable(string query, string structTable, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var lst = parameters == null ? null : new HanaParameter[parameters.Length];
            var declarePara = ParseParamater(lst, parameters, ref query);
            var dtSet = new DataTable();
            if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                                            BEGIN 
                                                                                        {0}
                                                                            end;", query, declarePara) : query;
                    command.Transaction = DbTransaction;
                    command.CommandType = commandType;
                    command.TryAddParameters(lst);
                    var adapter = new HanaDataAdapter(command);
                    adapter.Fill(dtSet);
                    command.Parameters.Clear();
                    if (!string.IsNullOrEmpty(structTable))
                    {
                        SetDefaultValueToTable(structTable, dtSet);
                    }
                    return dtSet;
                }
            }
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO {1}
                                                                    BEGIN 
                                                                                    {0}
                                                                        end;", query, declarePara) : query;
                    command.CommandType = commandType;
                    command.TryAddParameters(lst);
                    var adapter = new HanaDataAdapter(command);
                    adapter.Fill(dtSet);
                    if (!string.IsNullOrEmpty(structTable))
                    {
                        SetDefaultValueToTable(structTable, dtSet);
                    }
                    command.Parameters.Clear();
                }
            }
            return dtSet;
        }

        public int GetMaxId(string table, string idColumn)
        {
            string query = string.Format(@"SELECT TOP 1 ""{0}"" FROM ""{1}"" ORDER BY ""{0}"" DESC;", idColumn, table);
            //string query = string.Format(@"SELECT MAX(""{0}"") FROM ""{1}"";", idColumn, table);
            CommandType commandType = CommandType.Text;
            if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = commandType == CommandType.Text ? string.Format(@"DO BEGIN 
                                                    {0}
                                            end;", query) : query;
                    command.Transaction = DbTransaction;
                    command.CommandType = commandType;
                    var result = command.ExecuteScalar();
                    //Logging.Write(Logging.WATCH, "ExecuteNonQuery", Function.ToString(result));
                    return Function.ParseInt(result);
                }
            }
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                var command = conn.CreateCommand();
                command.CommandTimeout = 60000;
                command.CommandText = commandType == CommandType.Text ? string.Format(@"DO BEGIN 
                                                                            {0}
                                                    end;", query) : query;
                command.CommandType = commandType;
                var result = command.ExecuteScalar();
                //Logging.Write(Logging.WATCH, "result", Function.ToString(result));
                return Function.ParseInt(result);
            }
        }

        public int ExecuteNonQueryAnonymousBlock(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var lst = parameters == null ? null : new HanaParameter[parameters.Length];
            var declarePara = ParseParamater(lst, parameters, ref query);
            if (DbConnection != null && !string.IsNullOrEmpty(DbConnection.ConnectionString) && DbConnection.State != ConnectionState.Closed)
            {
                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = query;
                    command.CommandType = commandType;
                    command.Transaction = DbTransaction;
                    command.TryAddParameters(lst);
                    return command.ExecuteNonQuery();
                }
            }
            using (var conn = new HanaConnection(ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                var command = conn.CreateCommand();
                command.CommandTimeout = 60000;
                command.CommandText = query;
                command.CommandType = commandType;
                command.TryAddParameters(lst);
                var result = command.ExecuteNonQuery();
                //HanaConnection.ClearPool(conn);
                return result;
            }
        }
        #endregion

        #region Hana bulkcopy

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tblName"></param>
        public void CreateColumnTable(DataTable dt, string tblName)
        {
            var sql = "";
            var cols = new List<string>();
            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var col = dt.Columns[i];
                var maxValueColumn = dt.AsEnumerable().Max(t => t[col] == null ? "" : t[col].ToString());
                var item = string.Format(@"""{0}"" {1} {2}", col.ColumnName, GetSqlType(col, maxValueColumn == null ? 50 : maxValueColumn.Length),
                    col.AllowDBNull ? "NULL" : "");
                cols.Add(item);
            }
            sql += string.Format(@" CREATE COLUMN TABLE ""{0}""({1});", tblName, string.Join(",", cols.ToArray()));
            if (DbConnection != null)
            {
                if (string.IsNullOrEmpty(DbConnection.ConnectionString))
                {
                    DbConnection.ConnectionString = ConnectionString;
                }
                if (DbConnection.State == ConnectionState.Closed)
                {
                    DbConnection.Open();
                }
            }
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tblName"></param>
        public void AddColumnToTable(DataColumn col, string tblName)
        {
            var sql = "";
            var cols = new List<string>();
            var item = string.Format(@"""{0}"" {1} {2}", col.ColumnName, GetSqlType(col, 250),
                col.AllowDBNull ? "NULL" : "");
            cols.Add(item);

            sql += string.Format(@" ALTER TABLE ""{0}"" ADD({1});", tblName, string.Join(",", cols.ToArray()));
            if (DbConnection != null)
            {
                if (string.IsNullOrEmpty(DbConnection.ConnectionString))
                {
                    DbConnection.ConnectionString = ConnectionString;
                }
                if (DbConnection.State == ConnectionState.Closed)
                {
                    DbConnection.Open();
                }
            }
            ExecuteNonQueryAnonymousBlock(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tblName"></param>
        public void CreateTempTable(DataTable dt, string tblName)
        {
            var sql = "";
            var cols = new List<string>();
            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var col = dt.Columns[i];
                var maxValueColumn = dt.AsEnumerable().Max(t => t[col] == null ? "" : t[col].ToString());
                var item = string.Format(@"""{0}"" {1} {2}", col.ColumnName, GetSqlType(col, maxValueColumn == null ? 50 : maxValueColumn.Length),
                    col.AllowDBNull ? "NULL" : "");
                cols.Add(item);
            }
            sql += string.Format(@" CREATE local temporary TABLE ""{0}""({1});", tblName, string.Join(",", cols.ToArray()));
            if (DbConnection != null)
            {
                if (string.IsNullOrEmpty(DbConnection.ConnectionString))
                {
                    DbConnection.ConnectionString = ConnectionString;
                }
                if (DbConnection.State == ConnectionState.Closed)
                {
                    DbConnection.Open();
                }
            }
            ExecuteNonQuery(sql);
        }

        private string GetSqlType(DataColumn col, int columnLength)
        {
            var hanatype = GetDBType(col.DataType);
            if (col.DataType == typeof(string))
            {
                return columnLength < 5001 ? hanatype + "(5000)" : "nclob";
            }
            else if (col.DataType == typeof(char))
            {
                return "CHAR(1)";
            }

            if (hanatype == HanaDbType.Decimal)
            {
                return hanatype + " (19,6)";
            }
            return hanatype.ToString();
        }

        private HanaDbType GetDBType(Type theType)
        {
            var pr = new HanaParameter();
            var tc = TypeDescriptor.GetConverter(pr.DbType);
            if (tc.CanConvertFrom(theType))
            {
                var convertFrom = tc.ConvertFrom(theType.Name);
                if (convertFrom != null) pr.DbType = (DbType)convertFrom;
            }
            else
            {
                try
                {
                    var convertFrom = tc.ConvertFrom(theType.Name);
                    if (convertFrom != null)
                        pr.DbType = (DbType)convertFrom;
                }
                catch (Exception ex)
                {
                    if (theType.Name.Equals("TimeSpan"))
                    {
                        pr.DbType = DbType.Time;
                    }
                    //Do Nothing; will return NVarChar as default
                }
            }
            return pr.HanaDbType;
        }

        public string BulkCopy(DataTable dt, string tblName)
        {
            try
            {
                if (DbConnection == null)
                {
                    DbConnection = new HanaConnection(ConnectionString);
                }
                if (DbConnection != null)
                {
                    if (string.IsNullOrEmpty(DbConnection.ConnectionString))
                    {
                        DbConnection.ConnectionString = ConnectionString;
                    }
                    if (DbConnection.State == ConnectionState.Closed)
                    {
                        DbConnection.Open();
                    }
                    DbTransaction = DbConnection.BeginTransaction();
                    using (HanaBulkCopy bulkCopy = new HanaBulkCopy(DbConnection, HanaBulkCopyOptions.Default, DbTransaction))
                    {
                        // The table I'm loading the data to
                        bulkCopy.DestinationTableName = tblName;
                        // How many records to send to the database in one go (all of them)
                        bulkCopy.BatchSize = 1000;
                        bulkCopy.BulkCopyTimeout = 60000;
                        // Load the data to the database
                        bulkCopy.WriteToServer(dt);
                        // Close up          
                        bulkCopy.Close();
                    }
                    DbTransaction.Commit();
                    return "";
                }
                using (HanaBulkCopy bulkCopy = new HanaBulkCopy(ConnectionString))
                {
                    // The table I'm loading the data to
                    bulkCopy.DestinationTableName = tblName;
                    // How many records to send to the database in one go (all of them)
                    bulkCopy.BatchSize = 1000;
                    bulkCopy.BulkCopyTimeout = 60000;
                    // Load the data to the database
                    bulkCopy.WriteToServer(dt);
                    // Close up          
                    bulkCopy.Close();
                }
                return "";
            }
            catch (Exception ex)
            {
                DbTransaction.Rollback();
                return ex.Message;
            }
        }

        public void BulkCopy(DataSet ds)
        {
            string tableName = "";
            try
            {
                if (DbConnection == null)
                {
                    DbConnection = new HanaConnection(ConnectionString);
                }
                if (DbConnection != null)
                {
                    if (string.IsNullOrEmpty(DbConnection.ConnectionString))
                    {
                        DbConnection.ConnectionString = ConnectionString;
                    }
                    if (DbConnection.State == ConnectionState.Closed)
                    {
                        DbConnection.Open();
                    }
                    using (DbTransaction = DbConnection.BeginTransaction())
                    {
                        using (HanaBulkCopy bulkCopy = new HanaBulkCopy(DbConnection, HanaBulkCopyOptions.Default, DbTransaction))
                        {
                            foreach (DataTable dt in ds.Tables)
                            {

                                tableName = dt.TableName;
                                // The table I'm loading the data to
                                bulkCopy.DestinationTableName = tableName;
                                // How many records to send to the database in one go (all of them)
                                bulkCopy.BatchSize = 1000;
                                bulkCopy.BulkCopyTimeout = 60000;
                                // Load the data to the database
                                bulkCopy.WriteToServer(dt);
                            }
                            // Close up          
                            bulkCopy.Close();
                        }
                        DbTransaction.Commit();
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message, tableName);
                throw ex;
            }
        }
        #endregion

        /// <summary>
        /// set default value for table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="dt"></param>
        public virtual void SetDefaultValueToTable(string table, DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0)
            {
                return;
            }
            const string sql = @"SELECT ""COLUMN_NAME"",DEFAULT_VALUE COLUMN_DEFAULT,""IS_NULLABLE"",DATA_TYPE_NAME DATA_TYPE FROM TABLE_COLUMNS WHERE SCHEMA_NAME = CURRENT_SCHEMA and TABLE_NAME= @tblName and (IS_NULLABLE = 'NO' or DEFAULT_VALUE is not null);";
            var tabelStruc = ExecuteDataTable(sql, CommandType.Text, new SqlParameter("@tblName", table));
            if (tabelStruc != null && tabelStruc.Rows.Count > 0)
            {
                for (var i = 0; i < tabelStruc.Rows.Count; i++)
                {
                    var sqltype = tabelStruc.Rows[i]["DATA_TYPE"].To_String().ToLower();
                    var colName = tabelStruc.Rows[i]["COLUMN_NAME"].To_String();
                    var dflValue = tabelStruc.Rows[i]["COLUMN_DEFAULT"].To_String();
                    var allownull = tabelStruc.Rows[i]["IS_NULLABLE"].To_String().Equals("YES");
                    if (!allownull || !string.IsNullOrEmpty(dflValue))
                    {
                        for (var k = 0; k < dt.Columns.Count; k++)
                        {
                            if (dt.Columns[k].ColumnName == colName)
                            {
                                var info = CultureInfo.InvariantCulture;
                                switch (sqltype)
                                {
                                    case "integer":
                                    case "bigint":
                                    case "smallint":
                                        dt.Columns[k].DefaultValue = dflValue.Replace("(", "").Replace(")", "").ParseInt();

                                        break;
                                    case "decimal":
                                        dt.Columns[k].DefaultValue = dflValue.Replace("(", "").Replace(")", "").ParseDecimal();
                                        break;

                                    case "datetime":
                                    case "date":
                                        if (!string.IsNullOrEmpty(dflValue) || !allownull)
                                        {
                                            var dfl = dflValue.ParseDateTime();
                                            dt.Columns[k].DefaultValue = dfl ?? new DateTime(1900, 1, 1);
                                        }
                                        break;
                                    case "numeric":
                                    case "float":
                                        dt.Columns[k].DefaultValue = dflValue.Replace("(", "").Replace(")", "").ParseDouble(info.NumberFormat);
                                        break;
                                    case "bit":
                                        dt.Columns[k].DefaultValue = dflValue.ParseBool();
                                        break;
                                    default:
                                        dt.Columns[k].DefaultValue = dflValue.Replace("''", "'").To_String();
                                        break;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public virtual DataTable GetTableStruct(string tblName)
        {
            const string sql =
                    @"SELECT ""COLUMN_NAME"",DEFAULT_VALUE COLUMN_DEFAULT,""IS_NULLABLE"",DATA_TYPE_NAME DATA_TYPE FROM TABLE_COLUMNS WHERE SCHEMA_NAME = CURRENT_SCHEMA and TABLE_NAME= @tblName order by position;";
            var dt = ExecuteDataTable(sql, CommandType.Text, new SqlParameter("@tblName", tblName));
            var ret = new DataTable { TableName = tblName };
            if (dt != null && dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    Type type;
                    var sqltype = dt.Rows[i]["DATA_TYPE"].To_String().ToLower();
                    var colName = dt.Rows[i]["COLUMN_NAME"].To_String();
                    var dtColumn = new DataColumn { ColumnName = colName };
                    var dflValue = "";
                    var dfltLen = dt.Rows[i]["COLUMN_DEFAULT"].To_String().Length;
                    if (dfltLen >= 4)
                    {
                        dflValue = dt.Rows[i]["COLUMN_DEFAULT"].To_String().Substring(2, dfltLen - 4);
                    }
                    var allownull = dt.Rows[i]["IS_NULLABLE"].To_String().Equals("YES");

                    var info = CultureInfo.InvariantCulture;
                    switch (sqltype)
                    {
                        case "int":
                        case "integer":
                        case "bigint":
                        case "smallint":
                        case "tinyint":
                            type = typeof(int);
                            if (!string.IsNullOrEmpty(dflValue) || !allownull)
                            {
                                dtColumn.DefaultValue = dflValue.Replace("(", "").Replace(")", "").ParseInt();
                            }

                            break;
                        case "decimal":
                        case "smalldecimal":
                            type = typeof(decimal);
                            if (!string.IsNullOrEmpty(dflValue) || !allownull)
                            {
                                dtColumn.DefaultValue = dflValue.Replace("(", "").Replace(")", "").ParseDecimal();
                            }
                            break;

                        case "datetime":
                        case "date":
                        case "timestamp":
                            type = typeof(DateTime);
                            if (!string.IsNullOrEmpty(dflValue) || !allownull)
                            {
                                var dfl = dflValue.ParseDateTime();
                                dtColumn.DefaultValue = dfl ?? new DateTime(1900, 1, 1);
                            }
                            break;
                        case "numeric":
                        case "float":
                        case "real":
                            type = typeof(double);
                            if (!string.IsNullOrEmpty(dflValue) || !allownull)
                            {
                                dtColumn.DefaultValue = dflValue.Replace("(", "").Replace(")", "").ParseDouble(info.NumberFormat);
                            }
                            break;
                        case "bit":
                            type = typeof(bool);
                            if (!string.IsNullOrEmpty(dflValue) || !allownull)
                            {
                                dtColumn.DefaultValue = dflValue.ParseBool();
                            }
                            break;
                        default:
                            type = typeof(string);
                            if (!string.IsNullOrEmpty(dflValue) || !allownull)
                            {
                                dtColumn.DefaultValue = dflValue.Replace("''", "'").To_String();
                            }
                            break;
                    }

                    dtColumn.DataType = type;
                    ret.Columns.Add(dtColumn);
                }
            }
            if (ret.Columns.Count > 0)
            {
                ret.Rows.Add(ret.NewRow());
                return ret;
            }
            return new DataTable();
        }

        public virtual DataTable GetTableStructure(string tblName)
        {
            const string sql = @"SELECT TOP 0 * FROM @tblName;";
            return ExecuteDataTable(sql, CommandType.Text, new SqlParameter("@tblName", tblName));
        }

        public DbConnection OpenConnection()
        {
            DbConnection = DbConnection ?? new HanaConnection(ConnectionString);
            if (DbConnection.State == ConnectionState.Closed)
            {
                DbConnection.Open();
            }
            return DbConnection;
        }

        public DbTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            DbTransaction = DbConnection.BeginTransaction(level);
            return DbTransaction;
        }

        private string ParseParamater(HanaParameter[] arrHanaPara, SqlParameter[] arrSqlPara, ref string hanaQuery)
        {
            if (arrSqlPara == null || arrSqlPara.Length == 0)
            {
                return "";
            }
            var additionPara = "(";
            for (var i = 0; i < arrSqlPara.Length; i++)
            {
                hanaQuery = hanaQuery.Replace(arrSqlPara[i].ParameterName, "$" + arrSqlPara[i].ParameterName.Substring(1));
                arrHanaPara[i] = new HanaParameter(arrSqlPara[i].ParameterName.Substring(1), arrSqlPara[i].Value);
                var type = arrHanaPara[i].HanaDbType.ToString();
                switch (type.ToUpper())
                {
                    case "NVARCHAR":
                        type = type + (arrHanaPara[i].Value.To_String().Length < 4000 ? "(4000)" : (string.Format("({0})", arrHanaPara[i].Value.To_String().Length + 10)));
                        break;
                    case "DECIMAL":
                        type = type + "(19,6)";
                        break;
                }
                additionPara += string.Format("in {0} {1} => ?,", arrHanaPara[i].ParameterName, type);
            }
            return string.Format("{0})", additionPara.Substring(0, additionPara.Length - 1));
        }

    }
}

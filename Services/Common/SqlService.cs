using AppSettings;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;

namespace CommonServices
{
    public class SqlService
    {
        public static string GetMasterDatabaseConnectionStirng()
        {
            return AppSetting.MySQLConnectionString;
        }

        public static string GetTenantDatabaseConnectionStirng(string Database_Server, string Database_Name)
        {
            StringBuilder sb = new StringBuilder();

            if (AppSetting.TenantDatabaseServerMaster.Rows.Count > 0)
            {
                foreach (DataRow dataRow in AppSetting.TenantDatabaseServerMaster.Rows)
                {
                    if (dataRow["Database_Server"].ToString() == Database_Server)
                    {
                        if (!string.IsNullOrWhiteSpace(dataRow["Server"].ToString()))
                        {
                            sb.Append("Server=" + dataRow["Server"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Port"].ToString()))
                        {
                            sb.Append("Port=" + dataRow["Port"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["UserID"].ToString()))
                        {
                            sb.Append("UserID=" + dataRow["UserID"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Password"].ToString()))
                        {
                            sb.Append("Password=" + dataRow["Password"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(Database_Name))
                        {
                            sb.Append("Database=" + Database_Name + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["ConnectionProtocol"].ToString()))
                        {
                            sb.Append("ConnectionProtocol=" + dataRow["ConnectionProtocol"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["SslMode"].ToString()))
                        {
                            sb.Append("SslMode=" + dataRow["SslMode"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Pooling"].ToString()))
                        {
                            sb.Append("Pooling=" + dataRow["Pooling"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["ConnectionLifeTime"].ToString()))
                        {
                            sb.Append("ConnectionLifeTime=" + dataRow["ConnectionLifeTime"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Others"].ToString()))
                        {
                            sb.Append(dataRow["Others"].ToString());
                        }

                        break;
                    }
                }

                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        public static string GetTenantDatabaseConnectionStirng(HttpContext httpContext)
        {
            StringBuilder sb = new StringBuilder();

            var currentUser = httpContext.User;
            string Database_Server = "";
            string Database_Name = "";

            if (currentUser.HasClaim(c => c.Type == "SID"))
            {
                Database_Server = currentUser.Claims.FirstOrDefault(c => c.Type == "SID").Value.ToString();
                Database_Server = CipherService.Decrypt(Database_Server);
            }
            if (currentUser.HasClaim(c => c.Type == "TID"))
            {
                Database_Name = currentUser.Claims.FirstOrDefault(c => c.Type == "TID").Value.ToString();
                Database_Name = CipherService.Decrypt(Database_Name);
            }

            if (AppSetting.TenantDatabaseServerMaster.Rows.Count > 0)
            {
                foreach (DataRow dataRow in AppSetting.TenantDatabaseServerMaster.Rows)
                {
                    if (dataRow["Database_Server"].ToString() == Database_Server)
                    {
                        if (!string.IsNullOrWhiteSpace(dataRow["Server"].ToString()))
                        {
                            sb.Append("Server=" + dataRow["Server"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Port"].ToString()))
                        {
                            sb.Append("Port=" + dataRow["Port"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["UserID"].ToString()))
                        {
                            sb.Append("UserID=" + dataRow["UserID"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Password"].ToString()))
                        {
                            sb.Append("Password=" + dataRow["Password"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(Database_Name))
                        {
                            sb.Append("Database=" + Database_Name + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["ConnectionProtocol"].ToString()))
                        {
                            sb.Append("ConnectionProtocol=" + dataRow["ConnectionProtocol"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["SslMode"].ToString()))
                        {
                            sb.Append("SslMode=" + dataRow["SslMode"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Pooling"].ToString()))
                        {
                            sb.Append("Pooling=" + dataRow["Pooling"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["ConnectionLifeTime"].ToString()))
                        {
                            sb.Append("ConnectionLifeTime=" + dataRow["ConnectionLifeTime"].ToString() + ";");
                        }

                        if (!string.IsNullOrWhiteSpace(dataRow["Others"].ToString()))
                        {
                            sb.Append(dataRow["Others"].ToString());
                        }

                        break;
                    }
                }

                return sb.ToString();
            }
            else
            {
                return null;
            }
        }


        #region MySQL Server Methods ------------------------------------------------------------------------------

        /// <summary>
        /// Static Method : Executes SQL query and returns DataSet
        /// - If error, MessageBox will open.
        /// <para>Parameter string : SQL Query</para>
        /// <return>Return DataSet : Executed Query Output</return>
        /// </summary>
        public static DataSet MySQLExecuteReaderDataSet(string sqlQuery)
        {
            DataSet ret_ds = new DataSet();

            MySqlConnection con = new MySqlConnection(AppSetting.MySQLConnectionString);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(ret_ds);
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return ret_ds;
        }


        /// <summary>
        /// Static Method : Executes SQL query and returns DataTable
        /// - If error, MessageBox will open.
        /// <para>Parameter string : SQL Query</para>
        /// <return>Return DataTable : Executed Query Output</return>
        /// </summary>
        public static DataTable MySQLExecuteReaderDataTable(string sqlQuery)
        {
            DataTable ret_dt = new DataTable();

            MySqlConnection con = new MySqlConnection(SQLService.connectionString); ;
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(ret_dt);
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return ret_dt;
        }


        /// <summary>
        /// Static Method : Executes SQL query and returns string
        /// - If error, MessageBox will open.
        /// <para>Parameter string : SQL Query</para>
        /// <return>Return string : Executed Query Output (first row - first column value)</return>
        /// </summary>
        public static string MySQLExecuteReaderDataString(string sqlQuery)
        {
            string ret_string = string.Empty;// Logically the same as ""
            DataTable dt = new DataTable();

            MySqlConnection con = new MySqlConnection(AppSetting.MySQLConnectionString);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    ret_string = dt.Rows[0][0].ToString();
                }
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            if (string.IsNullOrWhiteSpace(ret_string))
            {
                ret_string = string.Empty;// Logically the same as ""
            }

            return ret_string;
        }


        // Its for command objects (DataSet). It returns the value given by database through select statement.
        public static DataSet MySQLExecuteReaderDataSet(string conStr, string sqlQuery)
        {
            DataSet ret_ds = new DataSet();

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);

            try
            {
                cmd.Connection.Open();

                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(ret_ds);

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return ret_ds;
        }
        public static DataSet MySQLExecuteReaderDataSetSafe(string conStr, string sqlQuery, out string errorMessage)
        {
            DataSet ret_ds = new DataSet();
            errorMessage = null;

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            MySqlTransaction trans = null;

            try
            {
                cmd.Connection.Open();

                // Start a local transaction
                trans = con.BeginTransaction();

                // Assign transaction object for a pending local transaction
                cmd.Transaction = trans;

                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(ret_ds);

                trans.Commit();

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                trans?.Rollback();

                errorMessage = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return ret_ds;
        }


        // Its for command objects (DataTable). It returns the value given by database through select statement.
        public static DataTable MySQLExecuteReaderDataTable(string conStr, string sqlQuery)
        {
            DataTable ret_dt = new DataTable();

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);

            try
            {
                cmd.Connection.Open();

                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(ret_dt);

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return ret_dt;
        }
        public static DataTable MySQLExecuteReaderDataTableSafe(string conStr, string sqlQuery, out string errorMessage)
        {
            DataTable ret_dt = new DataTable();
            errorMessage = null;

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            MySqlTransaction trans = null;

            try
            {
                cmd.Connection.Open();

                // Start a local transaction
                trans = con.BeginTransaction();

                // Assign transaction object for a pending local transaction
                cmd.Transaction = trans;

                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(ret_dt);

                trans.Commit();

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                trans?.Rollback();

                errorMessage = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return ret_dt;
        }


        // It returns only one value (string). That value will the first column first row value.
        public static string MySQLExecuteReaderDataString(string conStr, string sqlQuery)
        {
            string ret_string = string.Empty;// Logically the same as ""
            DataTable dt = new DataTable();

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    ret_string = dt.Rows[0][0].ToString();
                }
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            if (string.IsNullOrWhiteSpace(ret_string))
            {
                ret_string = string.Empty;// Logically the same as ""
            }

            return ret_string;
        }
        public static string MySQLExecuteReaderDataStringSafe(string conStr, string sqlQuery, out string errorMessage)
        {
            string ret_string = string.Empty;// Logically the same as ""
            DataTable dt = new DataTable();
            errorMessage = null;

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            MySqlTransaction trans = null;

            try
            {
                cmd.Connection.Open();

                // Start a local transaction
                trans = con.BeginTransaction();

                // Assign transaction object for a pending local transaction
                cmd.Transaction = trans;

                //cmd.ExecuteReader();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter(cmd);
                sqlDA.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    ret_string = dt.Rows[0][0].ToString();
                }

                trans.Commit();

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                trans?.Rollback();

                errorMessage = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            if (string.IsNullOrWhiteSpace(ret_string))
            {
                ret_string = string.Empty;// Logically the same as ""
            }

            return ret_string;
        }


        // It will not return any data. It is used with insert and update and delete. It returns only the number of rows affected.
        public static int MySQLExecuteNonQuery(string conStr, string sqlQuery)
        {
            int rowsAffectedCount = 0;

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();

                rowsAffectedCount = cmd.ExecuteNonQuery();

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                rowsAffectedCount = -1;
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return rowsAffectedCount;
        }
        public static int MySQLExecuteNonQuerySafe(string conStr, string sqlQuery, out string errorMessage)
        {
            int rowsAffectedCount = 0;
            errorMessage = null;

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            MySqlTransaction trans = null;

            try
            {
                cmd.Connection.Open();

                // Start a local transaction
                trans = con.BeginTransaction();

                // Assign transaction object for a pending local transaction
                cmd.Transaction = trans;

                rowsAffectedCount = cmd.ExecuteNonQuery();

                trans.Commit();

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                trans?.Rollback();

                rowsAffectedCount = -1;
                errorMessage = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return rowsAffectedCount;
        }


        // It returns only one value (object). That value will the first column first row value.
        public static object MySQLExecuteScalar(string conStr, string sqlQuery)
        {
            object firstColumnRowValue = new object();

            MySqlConnection con = new MySqlConnection(conStr);
            MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();

                firstColumnRowValue = cmd.ExecuteScalar();

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MySQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }

            return firstColumnRowValue;
        }


        #endregion MySQL Server Methods ------------------------------------------------------------------------------



        #region MySQL Server Connection Status Methods ------------------------------------------------------------------------------

        /// <summary>
        /// Static Method : Opens MySQL Server Connection and returns Server Version and Status.
        /// - If error, MessageBox will open.
        /// <para>Parameter string : Connection String</para>
        /// <return>Return string : [ If success - Server Version : 1.0.0 - Status : Opened ] [ If fail - Failed : (error message) ]</return>
        /// </summary>
        public static string CheckMySQLConnection(string conStr)
        {
            string ret_string = string.Empty;// Logically the same as ""

            if (string.IsNullOrEmpty(conStr))
            {
                conStr = AppSetting.MySQLConnectionString;
            }

            MySqlConnection con = new MySqlConnection(conStr);
            try
            {
                con.Open();
                ret_string = "Server Version :" + con.ServerVersion.ToString() + " - Status :" + con.State.ToString();
                con.Close();
            }
            catch (Exception ex)
            {
                ret_string = "Failed : " + ex.Message.ToString();
            }
            finally
            {
                con.Close();
            }

            return ret_string;
        }


        /// <summary>
        /// Static Method : Opens MySQL Server Connection and returns true/false.
        /// - If error, MessageBox will open.
        /// <para>Parameter string : Connection String</para>
        /// <return>Return bool : true/false</return>
        /// </summary>
        public static bool CheckMySQLConnectionStatus(string conStr)
        {
            try
            {
                if (CheckMySQLConnection(conStr).Contains("Fail"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion MySQL Server Connection Status Methods ------------------------------------------------------------------------------


        #region MySQL Create Database -------------------
        public static JObject ScriptExecute(string ConStr, string SqlScript)
        {
            JObject jObject = new JObject();

            int ExecutedQueryCount = 0;
            string Status = "Fail";
            string Message = "";

            if (!string.IsNullOrWhiteSpace(ConStr) && !string.IsNullOrWhiteSpace(SqlScript))
            {
                MySqlConnection con = new MySqlConnection(ConStr);
                try
                {
                    con.Open();

                    MySqlScript script = new MySqlScript(con, SqlScript);

                    //MySqlScriptErrorEventHandler script_Error = null;
                    //script.Error += new MySqlScriptErrorEventHandler(script_Error);

                    //EventHandler script_ScriptCompleted = null;
                    //script.ScriptCompleted += new EventHandler(script_ScriptCompleted);

                    //MySqlStatementExecutedEventHandler script_StatementExecuted = null;
                    //script.StatementExecuted += new MySqlStatementExecutedEventHandler(script_StatementExecuted);

                    ExecutedQueryCount = script.Execute();
                    //Console.WriteLine("Executed " + ExecutedQueryCount + " statement(s).");
                    //Console.WriteLine("Delimiter: " + script.Delimiter);

                    Message = "Executed " + ExecutedQueryCount + " statement(s).";
                    Message += "Delimiter: " + script.Delimiter;

                    con.Close();
                    Status = "Success";
                }
                catch (Exception ex)
                {
                    Message = ex.ToString();
                    Status = "Fail";
                    //MessageBox.Show(ex.Message, "SQL Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    con.Close();
                }
            }

            jObject.Add("ExecutedQueryCount", ExecutedQueryCount);
            jObject.Add("Status", Status);
            jObject.Add("Message", Message);
            return jObject;
        }
        /*
        public void script_StatementExecuted(object sender, MySqlScriptEventArgs args)
        {
            //updateprogressbar();
            Console.WriteLine("script_StatementExecuted");
        }

        public void script_ScriptCompleted(object sender, EventArgs e)
        {
            /// EventArgs e will be EventArgs.Empty for this method
            Console.WriteLine("script_ScriptCompleted!");
        }

        public void script_Error(Object sender, MySqlScriptErrorEventArgs args)
        {
            Console.WriteLine("script_Error: " + args.Exception.ToString());
        }
        */
        #endregion MySQL Create Database ----------------


        #region SQL Parameter Construction Methods -------------------------------------------------------------------------
        public static string AddStringSQLParameter(string parameterValue, bool endWithComma = true, string defaultValue = null)
        {
            string returnValue = "";

            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                {
                    returnValue = "NULL";
                }
                else
                {
                    defaultValue = defaultValue.Replace("\\'", "'");
                    defaultValue = defaultValue.Replace("'", "\\'");

                    returnValue = "'" + defaultValue + "'";
                }
            }
            else
            {
                parameterValue = parameterValue.Replace("\\'", "'");
                parameterValue = parameterValue.Replace("'", "\\'");

                returnValue = "'" + parameterValue + "'";
            }

            if (endWithComma == true)
            {
                returnValue += ",";
            }

            return returnValue;
        }

        public static string AddBooleanSQLParameter(bool parameterValue, bool endWithComma = true)
        {
            string returnValue = "";

            if (parameterValue == true)
            {
                returnValue = "'1'";
            }
            else
            {
                returnValue = "'0'";
            }

            if (endWithComma == true)
            {
                returnValue += ",";
            }

            return returnValue;
        }

        public static string AddActiveSatusSQLParameter(bool parameterValue, bool endWithComma = true)
        {
            string returnValue = "";

            if (parameterValue == true)
            {
                returnValue = "'Active'";
            }
            else
            {
                returnValue = "'Inactive'";
            }

            if (endWithComma == true)
            {
                returnValue += ",";
            }

            return returnValue;
        }

        #endregion SQL Parameter Construction Methods ----------------------------------------------------------------------

    }
}

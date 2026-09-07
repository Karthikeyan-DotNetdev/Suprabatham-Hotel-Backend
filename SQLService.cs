    using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
//using System.Windows.Forms;

namespace CommonServices
{
    public class SQLService
    {

        
        public static string connectionString = "Data Source=103.91.186.153,34999;Database=ADMIN_SUPRABATHAM;User Id=saadmin;Password=sV6x9#rXrq4wG$ZHYL;Encrypt=True;TrustServerCertificate=True";
        //public static string connectionString = "Data Source=103.91.186.153,34999;Database=ADMIN_BITHOTEL;User Id=saadmin;Password=sV6x9#rXrq4wG$ZHYL;Encrypt=True;TrustServerCertificate=True";


        #region MSSQL Server Methods ------------------------------------------------------------------------------


        public static DataSet GetDataSet(string sqlQuery)
        {
            DataSet ret_ds = new DataSet();
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                //cmd.ExecuteNonQuery();
                SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
                sqlDA.Fill(ret_ds);
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MSSQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }
            return ret_ds;
        }
        public static DataTable GetDataTable(string sqlQuery, SqlParameter[] parameters = null)
        {
            DataTable ret_dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
            {
                try
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    con.Open();

                    using (SqlDataAdapter sqlDA = new SqlDataAdapter(cmd))
                    {
                        sqlDA.Fill(ret_dt);
                    }
                }
                catch (Exception ex)
                {
                    string Err = ex.Message;
                }
            }

            return ret_dt;
        }
        public static string GetResultAsString(string sqlQuery)
        {
            string ret_string = string.Empty;// Logically the same as ""
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                //cmd.ExecuteNonQuery();
                SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
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
                //MessageBox.Show(ex.Message, "MSSQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        public static string CheckServerConnection(string constr)
        {
            string ret_string = string.Empty;// Logically the same as ""
            SqlConnection con = new SqlConnection(constr);
            try
            {
                con.Open();
                ret_string = "Server Version:" + con.ServerVersion.ToString() + " - Status:" + con.State.ToString();
                con.Close();
            }
            catch (Exception ex)
            {
                ret_string = "Failed - " + ex.Message.ToString();
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MSSQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }
            return ret_string;
        }
        public static bool CheckServerConnectionStatus(string conStr)
        {
            try
            {
                if (CheckServerConnection(conStr).Contains("Fail"))
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
        public static int ExecuteNonQuery(string sqlQuery)
        {
            int ret_val = 0;
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sqlQuery, con);
            SqlTransaction transaction = null;
            try
            {
                cmd.Connection.Open();
                transaction = con.BeginTransaction();
                cmd.Transaction = transaction;
                ret_val = cmd.ExecuteNonQuery();
                cmd.Transaction.Commit();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                string Err = ex.Message;
                throw;
            }
            finally
            {
                con.Close();
            }
            return ret_val;
        }
        public static int ExecuteScalarQuery(string sqlQuery)
        {
            int ret_val = 0;
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                ret_val = (Int32)cmd.ExecuteScalar();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                string Err = ex.Message;
                //MessageBox.Show(ex.Message, "MSSQL Database Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }
            return ret_val;
        }

        public static int ExecuteScalarQueryCustom(string sqlQuery)
        {
            int ret_val = 0; // Default value if no records found
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, con))
                {
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar(); // Execute and get the result

                        // Check if the result is not null and is not DBNull
                        if (result != null && result != DBNull.Value)
                        {
                            // Convert to long first, then to int
                            ret_val = Convert.ToInt32(Convert.ToInt64(result));
                        }
                    }
                    catch (Exception ex)
                    {
                        string Err = ex.Message;
                        // Handle exception (log it, display a message, etc.)
                    }
                } // Command is disposed here
            } // Connection is disposed here
            return ret_val;
        }


        public static DataTable MsSQLExecuteReaderDataTable(string sqlQuery)
        {
            DataTable ret_dt = new DataTable();

            SqlConnection con = new SqlConnection(SQLService.connectionString); ;
            SqlCommand cmd = new SqlCommand(sqlQuery, con);
            try
            {
                cmd.Connection.Open();
                //cmd.ExecuteReader();
                SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
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

        public static DataSet MySQLExecuteReaderDataSetSafe(string sqlQuery, out string errorMessage)
        {
            DataSet ret_ds = new DataSet();
            errorMessage = null;

            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sqlQuery, con);
            SqlTransaction trans = null;

            try
            {
                cmd.Connection.Open();

                // Start a local transaction
                trans = con.BeginTransaction();

                // Assign transaction object for a pending local transaction
                cmd.Transaction = trans;

                //cmd.ExecuteReader();
                SqlDataAdapter sqlDA = new SqlDataAdapter(cmd);
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



        #endregion MSSQL Server Methods ------------------------------------------------------------------------------


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

            if (endWithComma)
            {
                returnValue += ",";
            }

            return returnValue;
        }
        public static string AddStringSQLParameter(string parameterName, string parameterValue, bool endWithComma = true, string defaultValue = null)
        {
            string returnValue = "";

            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                {
                    returnValue = " " + parameterName + " = " + "NULL";
                }
                else
                {
                    defaultValue = defaultValue.Replace("\\'", "'");
                    defaultValue = defaultValue.Replace("'", "\\'");

                    returnValue = " " + parameterName + " = " + "'" + defaultValue + "'";
                }
            }
            else
            {
                parameterValue = parameterValue.Replace("\\'", "'");
                parameterValue = parameterValue.Replace("'", "\\'");

                returnValue = " " + parameterName + " = " + "'" + parameterValue + "'";
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

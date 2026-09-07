using CommonServices;
using laptop_service.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;

namespace laptop_service.Services
{
    public class DatabaseService
    {
        public static bool GetDatabaseAvailability(string DatabaseServer, string DatabaseName)
        {
            bool retVal = false;

            string ConStr = SqlService.GetTenantDatabaseConnectionStirng(DatabaseServer, null);
            string SqlQuery = "SELECT COUNT(DISTINCT TABLE_SCHEMA) AS 'Count' FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" + DatabaseName + "';";
            SqlQuery = SqlService.MySQLExecuteReaderDataString(ConStr, SqlQuery);
            if (SqlQuery == "0")
            {
                retVal = false;
            }
            else
            {
                retVal = true;
            }

            return retVal;
        }

        public static string CreateDatabase(TenantMaster tenantMaster, string masterPassword)
        {
            string ret_val = "Fail;";

            string ConStr = SqlService.GetTenantDatabaseConnectionStirng(tenantMaster.Database_Server, null);

            try
            {
                string sql_script = "";
                string SqlQueryName = "";
                string SqlQueryCount = "0";
                StringBuilder stringBuilder = new StringBuilder();

                sql_script = FileService.TenantMasterScript();

                if (!string.IsNullOrWhiteSpace(sql_script))
                {
                    try
                    {
                        string firstline = sql_script.Split(new[] { '\r', '\n' }).FirstOrDefault();
                        sql_script = sql_script.Replace(firstline, "");
                        firstline = firstline.Replace("-- /", "");
                        var details = JObject.Parse(firstline);
                        SqlQueryName = details["SqlQueryName"]?.ToString();
                        SqlQueryCount = details["SqlQueryCount"]?.ToString();
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(sql_script))
                    {
                        stringBuilder.Append(sql_script);
                        stringBuilder.Replace("@Schema_Name@", tenantMaster.Database_Name);
                        stringBuilder.Replace("@Organization_Code@", tenantMaster.Organization_Code);
                        stringBuilder.Replace("@Organization_Name@", tenantMaster.Organization_Name);
                        stringBuilder.Replace("@Branch_Name@", "Main");
                        stringBuilder.Replace("@Employee_Name@", tenantMaster.Employee_Name);
                        stringBuilder.Replace("@Phone@", tenantMaster.Phone);
                        stringBuilder.Replace("@Email@", tenantMaster.Email);
                        stringBuilder.Replace("@City@", tenantMaster.City);
                        stringBuilder.Replace("@Country@", tenantMaster.Country);
                        stringBuilder.Replace("@Password@", masterPassword);

                        JObject jObject = new JObject();
                        jObject = SqlService.ScriptExecute(ConStr, stringBuilder.ToString());

                        string count = jObject["ExecutedQueryCount"]?.ToString();
                        string status = jObject["Status"]?.ToString();
                        string message = jObject["Message"]?.ToString();

                        if (status == "Success")
                        {
                            ret_val = "Success;";
                            ret_val += "SqlQuery Name : " + SqlQueryName + "   \r\n   ";
                            ret_val += "SqlQuery Count : " + SqlQueryCount + "   \r\n   ";
                            ret_val += "Execution Status : " + status + "   \r\n   ";
                            ret_val += "Execution message : " + message + "   \r\n   ";
                            ret_val += "Executed Query Count : " + count;
                        }
                        else
                        {
                            ret_val = "Fail;";
                            ret_val += "SqlQuery Name : " + SqlQueryName + "   \r\n   ";
                            ret_val += "SqlQuery Count : " + SqlQueryCount + "   \r\n   ";
                            ret_val += "Execution Status : " + status + "   \r\n   ";
                            ret_val += "Execution message : " + message + "   \r\n   ";
                            ret_val += "Executed Query Count : " + count;
                        }
                    }
                    else
                    {
                        ret_val = "Fail;Script File Empty.";
                    }
                }
                else
                {
                    ret_val = "Fail;Reading Script File Empty.";
                }
            }
            catch (Exception ex)
            {
                ret_val = "Fail;" + ex.ToString();
            }

            return ret_val;
        }

        public static string UpdateDatabase(DataRow dataRow, string sqlVersion)
        {
            string ret_val = "Fail;";

            string ConStr = SqlService.GetTenantDatabaseConnectionStirng(dataRow["Database_Server"].ToString(), dataRow["Database_Name"].ToString());

            try
            {
                string sql_script = "";
                string SqlQueryName = "";
                string SqlQueryCount = "0";
                StringBuilder stringBuilder = new StringBuilder();

                sql_script = FileService.TenantMasterUpdateScript(sqlVersion);

                if (!string.IsNullOrWhiteSpace(sql_script))
                {
                    try
                    {
                        string firstline = sql_script.Split(new[] { '\r', '\n' }).FirstOrDefault();
                        sql_script = sql_script.Replace(firstline, "");
                        firstline = firstline.Replace("-- /", "");
                        var details = JObject.Parse(firstline);
                        SqlQueryName = details["SqlQueryName"]?.ToString();
                        SqlQueryCount = details["SqlQueryCount"]?.ToString();
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(sql_script))
                    {
                        stringBuilder.Append(sql_script);
                        JObject jObject = new JObject();
                        jObject = SqlService.ScriptExecute(ConStr, stringBuilder.ToString());

                        string count = jObject["ExecutedQueryCount"]?.ToString();
                        string status = jObject["Status"]?.ToString();
                        string message = jObject["Message"]?.ToString();

                        if (status == "Success")
                        {
                            ret_val = "Success;";
                            ret_val += "SqlQuery Name : " + SqlQueryName + "   \r\n   ";
                            ret_val += "SqlQuery Count : " + SqlQueryCount + "   \r\n   ";
                            ret_val += "Execution Status : " + status + "   \r\n   ";
                            ret_val += "Execution message : " + message + "   \r\n   ";
                            ret_val += "Executed Query Count : " + count;

                            ConStr = SqlService.GetMasterDatabaseConnectionStirng();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("CALL `pr_insertupdate_Tenant_Master_Property_SQL`(");
                            sb.Append(SqlService.AddStringSQLParameter(dataRow["Organization_Code"].ToString(), true, null));
                            sb.Append(SqlService.AddStringSQLParameter(sqlVersion, false, null));
                            sb.Append(");");
                            SqlService.MySQLExecuteReaderDataTable(ConStr, sb.ToString());
                        }
                        else
                        {
                            ret_val = "Fail;";
                            ret_val += "SqlQuery Name : " + SqlQueryName + "   \r\n   ";
                            ret_val += "SqlQuery Count : " + SqlQueryCount + "   \r\n   ";
                            ret_val += "Execution Status : " + status + "   \r\n   ";
                            ret_val += "Execution message : " + message + "   \r\n   ";
                            ret_val += "Executed Query Count : " + count;

                            ConStr = SqlService.GetMasterDatabaseConnectionStirng();
                            StringBuilder sb = new StringBuilder();
                            sb.Append("CALL `pr_insert_Tenant_Master_SQL_Log`(");
                            sb.Append(SqlService.AddStringSQLParameter(dataRow["Organization_Code"].ToString(), true, null));
                            sb.Append(SqlService.AddStringSQLParameter(sqlVersion, true, null));
                            sb.Append(SqlService.AddStringSQLParameter(message, false, null));
                            sb.Append(");");
                            SqlService.MySQLExecuteReaderDataTable(ConStr, sb.ToString());
                        }
                    }
                    else
                    {
                        ret_val = "Fail;Script File Empty.";
                    }
                }
                else
                {
                    ret_val = "Fail;Reading Script File Empty.";
                }
            }
            catch (Exception ex)
            {
                ret_val = "Fail;" + ex.ToString();
            }

            return ret_val;
        }

    }
}

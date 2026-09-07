using AppSettings;
using laptop_service.Views;
using System.Data;
using System.Data.SqlClient;

namespace CommonServices
{
    public class GeneralService 
    {
        public static string ApiVersion()
        {
            return AppSetting.ApiVersion;
        }


        private readonly string _connectionString;

        public GeneralService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("connectionString");
        }


        public async Task updateuserActivity(M_USERTRACKING_TAB M_USERTRACKING_TAB)
        {
            M_USERTRACKING_TAB activity = new M_USERTRACKING_TAB();
            activity.DATE = DateTime.Now;
            activity.USER_CODE = M_USERTRACKING_TAB.USER_CODE;
            activity.USER_NAME = M_USERTRACKING_TAB.USER_NAME;
            activity.MODULE = M_USERTRACKING_TAB.MODULE;
            activity.USER_CODE = M_USERTRACKING_TAB.USER_CODE;

        }







        public async Task InsertUserTrackingAsync(M_USERTRACKING_TAB M_USERTRACKING_TAB)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmdu = new SqlCommand("sp_insertusertracking", connection))
                    {
                        cmdu.CommandType = CommandType.StoredProcedure;
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@date", SqlDbType = SqlDbType.DateTime, Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt") });
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@usercode", SqlDbType = SqlDbType.VarChar, Value = M_USERTRACKING_TAB.USER_CODE });
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@username", SqlDbType = SqlDbType.VarChar, Value = M_USERTRACKING_TAB.USER_NAME });
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@module", SqlDbType = SqlDbType.VarChar, Value = M_USERTRACKING_TAB.MODULE });
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@operation", SqlDbType = SqlDbType.VarChar, Value = M_USERTRACKING_TAB.OPERATION });
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@notes", SqlDbType = SqlDbType.VarChar, Value = M_USERTRACKING_TAB.NOTES });
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@branchcode", SqlDbType = SqlDbType.VarChar, Value = M_USERTRACKING_TAB.BRANCH_CODE });
                        cmdu.Parameters.Add(new SqlParameter { ParameterName = "@branchname", SqlDbType = SqlDbType.VarChar, Value = M_USERTRACKING_TAB.BRANCH_NAME });

                        await cmdu.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }




        /*
        public static JArray SQLVersionUpdate(string conStr, SqlVersion sqlVersion)
        {
            JArray jArray = new JArray();
            string sqlQuery = "";

            if (sqlVersion.Organization_Code != null && sqlVersion.Organization_Code.Length > 0)
            {
                foreach (string organizationCode in sqlVersion.Organization_Code)
                {
                    if (!string.IsNullOrWhiteSpace(organizationCode))
                    {
                        // Read the tenant master property details
                        sqlQuery = "CALL `pr_read_Tenant_Master_Property`('"+ organizationCode + "');";
                        DataTable dataTable = SqlService.MySQLExecuteReaderDataTable(conStr, sqlQuery);
                        if (dataTable.Rows.Count > 0)
                        {
                            int currentVersion = 0;
                            int newVersion = 0;
                            try
                            {
                                currentVersion = Convert.ToInt32(dataTable.Rows[0]["SQL_Version_No"].ToString());
                                newVersion = Convert.ToInt32(sqlVersion.Sql_Version);
                            }
                            catch { }

                            if (newVersion > currentVersion)
                            {
                                if (DatabaseService.GetDatabaseAvailability(dataTable.Rows[0]["Database_Server"].ToString(), dataTable.Rows[0]["Database_Name"].ToString()) == true)
                                {
                                    string ret = DatabaseService.UpdateDatabase(dataTable.Rows[0], sqlVersion.Sql_Version);
                                    if (ret.Contains("Fail"))
                                    {
                                        JObject jObject = new JObject();
                                        jObject.Add("Organization_Code", dataTable.Rows[0]["Organization_Code"].ToString());
                                        jObject.Add("Status", "Fail");
                                        jObject.Add("Result", ret);
                                        jArray.Add(jObject);
                                    }
                                    else
                                    {
                                        JObject jObject = new JObject();
                                        jObject.Add("Organization_Code", dataTable.Rows[0]["Organization_Code"].ToString());
                                        jObject.Add("Status", "Success");
                                        jObject.Add("Result", ret);
                                        jArray.Add(jObject);
                                    }
                                }
                            }
                        }
                        else
                        {
                            JObject jObject = new JObject();
                            jObject.Add("Organization_Code", organizationCode);
                            jObject.Add("Status", "Fail");
                            jObject.Add("Result", "Cannot read tenant master property details.");
                            jArray.Add(jObject);
                        }
                    }
                }
            }
            else
            {
                // Update For All the Accounts
                sqlQuery = "CALL `pr_get_Tenant_Master_Property`();";
                DataTable dataTable = SqlService.MySQLExecuteReaderDataTable(conStr, sqlQuery);
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    int currentVersion = 0;
                    int newVersion = 0;
                    try {
                        currentVersion = Convert.ToInt32(dataRow["SQL_Version_No"].ToString());
                        newVersion = Convert.ToInt32(sqlVersion.Sql_Version);
                    } catch { }
                    
                    if (newVersion > currentVersion)
                    {
                        if (DatabaseService.GetDatabaseAvailability(dataRow["Database_Server"].ToString(), dataRow["Database_Name"].ToString()) == true)
                        {
                            string ret = DatabaseService.UpdateDatabase(dataRow, sqlVersion.Sql_Version);
                            if (ret.Contains("Fail"))
                            {
                                JObject jObject = new JObject();
                                jObject.Add("Organization_Code", dataRow["Organization_Code"].ToString());
                                jObject.Add("Status", "Fail");
                                jObject.Add("Result", ret);
                                jArray.Add(jObject);
                            }
                            else
                            {
                                JObject jObject = new JObject();
                                jObject.Add("Organization_Code", dataRow["Organization_Code"].ToString());
                                jObject.Add("Status", "Success");
                                jObject.Add("Result", ret);
                                jArray.Add(jObject);
                            }
                        }
                    }
                }

            }

            return jArray;
        }
        */
    }
}
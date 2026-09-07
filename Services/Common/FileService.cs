using AppSettings;

namespace CommonServices
{
    public class FileService
    {
        public static string EmailTemplate(string templateName)
        {
            string ret_val = "";

            if (!string.IsNullOrWhiteSpace(templateName))
            {
                string filePath = "";

                try
                {
                    filePath = AppSetting.ServerwwwwrootPath;
                    filePath = Path.Combine(filePath, "email_template\\" + templateName);
                }
                catch { }

                try
                {
                    if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(@filePath))
                    {
                        ret_val = File.ReadAllText(@filePath);
                    }
                }
                catch { }
            }

            return ret_val;
        }

        public static string TenantMasterScript()
        {
            string ret_val = "";

            string filePath = "";

            try
            {
                filePath = AppSetting.ServerwwwwrootPath;
                filePath = Path.Combine(filePath, "tenant_sql_script\\botpos_tenant_db.sql");
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(@filePath))
                {
                    ret_val = File.ReadAllText(@filePath);
                }
            }
            catch { }

            return ret_val;
        }

        public static string TenantMasterUpdateScript(string sqlVersion)
        {
            string ret_val = "";

            string filePath = "";

            try
            {
                filePath = AppSetting.ServerwwwwrootPath;
                filePath = Path.Combine(filePath, "tenant_sql_script\\botpos_tenant_db_update_" + sqlVersion + ".sql");
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(@filePath))
                {
                    ret_val = File.ReadAllText(@filePath);
                }
            }
            catch { }

            return ret_val;
        }

        /*
        public static string UrbanPiperScript()
        {
            string ret_val = "";

            string filePath = "";

            try
            {
                filePath = AppSetting.ServerwwwwrootPath;
                filePath = Path.Combine(filePath, "urbanpiper_sql_script\\urbanpiper_script.sql");
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(@filePath))
                {
                    ret_val = File.ReadAllText(@filePath);
                }
            }
            catch { }

            return ret_val;
        }

        public static string GupshupScript()
        {
            string ret_val = "";

            string filePath = "";

            try
            {
                filePath = AppSetting.ServerwwwwrootPath;
                filePath = Path.Combine(filePath, "gupshup_sql_script\\gupshup_script.sql");
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(@filePath))
                {
                    ret_val = File.ReadAllText(@filePath);
                }
            }
            catch { }

            return ret_val;
        }

        public static string AllposOnlineOrderScript()
        {
            string ret_val = "";

            string filePath = "";

            try
            {
                filePath = AppSetting.ServerwwwwrootPath;
                filePath = Path.Combine(filePath, "online_order_sql_script\\online_order_script.sql");
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(@filePath))
                {
                    ret_val = File.ReadAllText(@filePath);
                }
            }
            catch { }

            return ret_val;
        }
        */
    }
}

using CommonModels;
using CommonServices;
using laptop_service.Models;
using System.Data;
using System.Text;

namespace laptop_service.Services
{
    public class TenantService
    {
        #region Tenant Database Connection Usage
        public static DataTable GetTenantDatabaseServerMaster(string ConStr)
        {
            string SqlQuery = "SELECT * FROM `Database_Server_Master`;";

            return SqlService.MySQLExecuteReaderDataTable(ConStr, SqlQuery);
        }
        public static DataTable GetTenantDetailsForAccountId(string Account_Id)
        {
            string ConStr = SqlService.GetMasterDatabaseConnectionStirng();
            string SqlQuery = "CALL `pr_get_Account_Organization_Code`(";
            SqlQuery += string.IsNullOrWhiteSpace(Account_Id) ? "NULL" : "'" + Account_Id + "'";
            SqlQuery += ");";

            DataTable dataTable = SqlService.MySQLExecuteReaderDataTable(ConStr, SqlQuery);
            return dataTable;
        }

        #endregion Tenant Database Connection Usage

        public static DataTable List(string conStr, string fromDate, string toDate)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT `Tenant_Master`.`Account_Id`, `Tenant_Master`.`Organization_Code`, `Tenant_Master`.`Organization_Name`,");
            sb.Append(" `Tenant_Master`.`Employee_Name`, `Tenant_Master`.`Phone`,");
            sb.Append(" `Tenant_Master`.`Email`, `Tenant_Master`.`City`, `Tenant_Master`.`Country`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Tenant_Master`.`Activated_Date`,'%d-%m-%Y'),' ', `Tenant_Master`.`Activated_Time`) AS 'Activated_On',");
            sb.Append(" DATE_FORMAT(`Tenant_Master`.`Subscription_Expiry`,'%d-%m-%Y') AS 'Subscription_Expiry',");
            sb.Append(" `Tenant_Master`.`Subscription_Plan`, `Tenant_Master`.`Is_Active`");
            sb.Append(" FROM `Tenant_Master`");
            sb.Append(" WHERE `Tenant_Master`.`Activated_Date` BETWEEN '" + fromDate + "' AND '" + toDate + "'");
            sb.Append(" ORDER BY `Tenant_Master`.`Tenant_Master_Id` DESC;");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }

        public static DataTable SearchList(string conStr, string searchData)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT `Tenant_Master`.`Account_Id`, `Tenant_Master`.`Organization_Code`, `Tenant_Master`.`Organization_Name`,");
            sb.Append(" `Tenant_Master`.`Employee_Name`, `Tenant_Master`.`Phone`,");
            sb.Append(" `Tenant_Master`.`Email`, `Tenant_Master`.`City`, `Tenant_Master`.`Country`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Tenant_Master`.`Activated_Date`,'%d-%m-%Y'),' ', `Tenant_Master`.`Activated_Time`) AS 'Activated_On',");
            sb.Append(" DATE_FORMAT(`Tenant_Master`.`Subscription_Expiry`,'%d-%m-%Y') AS 'Subscription_Expiry',");
            sb.Append(" `Tenant_Master`.`Subscription_Plan`, `Tenant_Master`.`Is_Active`");
            sb.Append(" FROM `Tenant_Master`");
            sb.Append(" WHERE (`Tenant_Master`.`Organization_Code` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Tenant_Master`.`Organization_Name` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Tenant_Master`.`Email` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Tenant_Master`.`City` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Tenant_Master`.`Country` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Tenant_Master`.`Subscription_Plan` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" ) ORDER BY `Tenant_Master`.`Tenant_Master_Id` DESC LIMIT 100;");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }

        public static DataTable Read(string conStr, string email)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT `Tenant_Master`.`Account_Id`, `Tenant_Master`.`Organization_Code`, `Tenant_Master`.`Sub_Domain_URL`,");
            sb.Append(" `Tenant_Master`.`Database_Server`, `Tenant_Master`.`Database_Name`, `Tenant_Master`.`Organization_Name`,");
            sb.Append(" `Tenant_Master`.`Employee_Name`, `Tenant_Master`.`Phone`, `Tenant_Master`.`Email`,");
            sb.Append(" `Tenant_Master`.`City`, `Tenant_Master`.`Country`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Tenant_Master`.`Activated_Date`,'%d-%m-%Y'), ' ', `Tenant_Master`.`Activated_Time`) AS 'Activated_On',");
            sb.Append(" `Tenant_Master`.`Subscription_Plan`, `Tenant_Master`.`Subscription_Notes`,");
            sb.Append(" DATE_FORMAT(`Tenant_Master`.`Subscription_Expiry`,'%d-%m-%Y') AS 'Subscription_Expiry', `Tenant_Master`.`Is_Active`,");
            sb.Append(" DATE_FORMAT(`Tenant_Master`.`Created_On`,'%d-%m-%Y %H:%i:%S') AS 'Created_On', DATE_FORMAT(`Tenant_Master`.`Updated_On`,'%d-%m-%Y %H:%i:%S') AS 'Updated_On'");
            sb.Append(" FROM `Tenant_Master`");
            sb.Append(" WHERE `Tenant_Master`.`Email` = '" + email + "';");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }


        public static SqlResponse UpdateMasterDetail(string conStr, Tenant tenant)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE `Tenant_Master` SET");
            sb.Append(" `Tenant_Master`.`Organization_Name` = '" + tenant.Organization_Name + "',");
            sb.Append(" `Tenant_Master`.`Employee_Name` = '" + tenant.Employee_Name + "',");
            sb.Append(" `Tenant_Master`.`Phone` = '" + tenant.Phone + "',");
            //sb.Append(" `Tenant_Master`.`Email` = '" + tenant.Email + "',");
            sb.Append(" `Tenant_Master`.`City` = '" + tenant.City + "',");
            sb.Append(" `Tenant_Master`.`Country` = '" + tenant.Country + "',");
            sb.Append(" `Tenant_Master`.`Updated_On`= CONVERT_TZ(NOW(), @@session.time_zone, '+00:00')");
            sb.Append(" WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "';");

            int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
            if (result >= 0)
            {
                return new SqlResponse { status = true, message = "Master Data Updated Successfully.", count = result };
            }
            else
            {
                return new SqlResponse { status = false, message = "Cannot Update Master Data." };
            }
        }

        public static SqlResponse UpdateAccountId(string conStr, Tenant tenant)
        {
            //string SQLQuery = "SELECT IF(1 XOR 0, 1, 0);";
            string SQLQuery = "SELECT IF((SELECT COUNT(`Tenant_Master`.`Tenant_Master_Id`) FROM `Tenant_Master` WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "' AND `Tenant_Master`.`Account_Id` = '" + tenant.Account_Id + "') XOR (SELECT COUNT(`Tenant_Master`.`Tenant_Master_Id`) FROM `Tenant_Master` WHERE `Tenant_Master`.`Account_Id` = '" + tenant.Account_Id + "'), 1, 0);";
            Int64 validationCount = 0;
            validationCount = (Int64)SqlService.MySQLExecuteScalar(conStr, SQLQuery);
            if (validationCount > 0)
            {
                return new SqlResponse { status = false, message = "Account Id Already Exists." };
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE `Tenant_Master` SET");
                sb.Append(" `Tenant_Master`.`Account_Id` = '" + tenant.Account_Id + "',");
                sb.Append(" `Tenant_Master`.`Updated_On`= CONVERT_TZ(NOW(), @@session.time_zone, '+00:00')");
                sb.Append(" WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "';");

                int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
                if (result >= 0)
                {
                    return new SqlResponse { status = true, message = "Account Id Updated Successfully.", count = result };
                }
                else
                {
                    return new SqlResponse { status = false, message = "Cannot Update Account Id." };
                }
            }
        }

        public static SqlResponse UpdateSubDomainURL(string conStr, Tenant tenant)
        {
            //string SQLQuery = "SELECT IF(1 XOR 0, 1, 0);";
            string SQLQuery = "SELECT IF((SELECT COUNT(`Tenant_Master`.`Tenant_Master_Id`) FROM `Tenant_Master` WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "' AND `Tenant_Master`.`Sub_Domain_URL` = '" + tenant.Sub_Domain_URL + "') XOR (SELECT COUNT(`Tenant_Master`.`Tenant_Master_Id`) FROM `Tenant_Master` WHERE `Tenant_Master`.`Sub_Domain_URL` = '" + tenant.Sub_Domain_URL + "'), 1, 0);";
            Int64 validationCount = 0;
            validationCount = (Int64)SqlService.MySQLExecuteScalar(conStr, SQLQuery);
            if (validationCount > 0)
            {
                return new SqlResponse { status = false, message = "Account Id Already Exists." };
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE `Tenant_Master` SET");
                sb.Append(" `Tenant_Master`.`Sub_Domain_URL` = '" + tenant.Sub_Domain_URL + "',");
                sb.Append(" `Tenant_Master`.`Updated_On`= CONVERT_TZ(NOW(), @@session.time_zone, '+00:00')");
                sb.Append(" WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "';");

                int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
                if (result >= 0)
                {
                    return new SqlResponse { status = true, message = "Sub Domain URL Updated Successfully.", count = result };
                }
                else
                {
                    return new SqlResponse { status = false, message = "Cannot Update Sub Domain URL." };
                }
            }
        }

        public static SqlResponse UpdateSubscriptionPlan(string conStr, Tenant tenant)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE `Tenant_Master` SET");
            sb.Append(" `Tenant_Master`.`Subscription_Plan` = '" + tenant.Subscription_Plan + "',");
            sb.Append(" `Tenant_Master`.`Updated_On`= CONVERT_TZ(NOW(), @@session.time_zone, '+00:00')");
            sb.Append(" WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "';");

            int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
            if (result >= 0)
            {
                return new SqlResponse { status = true, message = "Subscription Plan Updated Successfully.", count = result };
            }
            else
            {
                return new SqlResponse { status = false, message = "Cannot Update Subscription Plan." };
            }
        }

        public static SqlResponse UpdateSubscriptionExpiry(string conStr, Tenant tenant)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE `Tenant_Master` SET");
            sb.Append(" `Tenant_Master`.`Subscription_Expiry` = '" + tenant.Subscription_Expiry + "',");
            sb.Append(" `Tenant_Master`.`Updated_On`= CONVERT_TZ(NOW(), @@session.time_zone, '+00:00')");
            sb.Append(" WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "';");

            int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
            if (result >= 0)
            {
                return new SqlResponse { status = true, message = "Subscription Expiry Updated Successfully.", count = result };
            }
            else
            {
                return new SqlResponse { status = false, message = "Cannot Update Subscription Expiry." };
            }
        }

        public static SqlResponse UpdateSubscriptionNotes(string conStr, Tenant tenant)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE `Tenant_Master` SET");
            sb.Append(" `Tenant_Master`.`Subscription_Notes` = '" + tenant.Subscription_Notes + "',");
            sb.Append(" `Tenant_Master`.`Updated_On`= CONVERT_TZ(NOW(), @@session.time_zone, '+00:00')");
            sb.Append(" WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "';");

            int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
            if (result >= 0)
            {
                return new SqlResponse { status = true, message = "Subscription Notes Updated Successfully.", count = result };
            }
            else
            {
                return new SqlResponse { status = false, message = "Cannot Update Subscription Notes." };
            }
        }

        public static SqlResponse UpdateIsActive(string conStr, Tenant tenant)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE `Tenant_Master` SET");
            sb.Append(" `Tenant_Master`.`Is_Active` = '" + tenant.Is_Active + "',");
            sb.Append(" `Tenant_Master`.`Updated_On`= CONVERT_TZ(NOW(), @@session.time_zone, '+00:00')");
            sb.Append(" WHERE `Tenant_Master`.`Email` = '" + tenant.Email + "';");

            int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
            if (result >= 0)
            {
                return new SqlResponse { status = true, message = "Is Active Updated Successfully.", count = result };
            }
            else
            {
                return new SqlResponse { status = false, message = "Cannot Update Is Active." };
            }
        }
    }
}

using CommonModels;
using CommonServices;
using laptop_service.Models;
using System.Data;
using System.Text;

namespace laptop_service.Services
{
    public class RegistrationService
    {
        public static DataTable List(string conStr, string? status)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT `Registration_Master`.`Organization_Name`, `Registration_Master`.`Employee_Name`, `Registration_Master`.`Phone`,");
            sb.Append(" `Registration_Master`.`Email`, `Registration_Master`.`City`, `Registration_Master`.`Country`, `Registration_Master`.`Status`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Registered_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Registered_Time`) AS 'Registered_On',");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Approved_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Approved_Time`) AS 'Approved_On', `Registration_Master`.`Approved_By`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Activated_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Activated_Time`) AS 'Activated_On',");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Rejected_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Rejected_Time`) AS 'Rejected_On', `Registration_Master`.`Rejected_By`,");
            sb.Append(" `Registration_Master`.`Sale_Channel`, `Registration_Master`.`Sale_Person`, `Registration_Master`.`Remarks`,");
            sb.Append(" DATE_FORMAT(`Registration_Master`.`Created_On`,'%d-%m-%Y %H:%i:%S') AS 'Created_On', DATE_FORMAT(`Registration_Master`.`Updated_On`,'%d-%m-%Y %H:%i:%S') AS 'Updated_On'");
            sb.Append(" FROM `Registration_Master`");
            if (!string.IsNullOrWhiteSpace(status))
            {
                sb.Append(" WHERE `Registration_Master`.`Status` = '" + status + "'");
            }
            else
            {
                sb.Append(" WHERE `Registration_Master`.`Status` IN ('Registered','Approved','Rejected')");
            }
            sb.Append(" ORDER BY `Registration_Master`.`Registration_Master_Id` DESC LIMIT 100;");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }

        public static DataTable SearchList(string conStr, string searchData)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT `Registration_Master`.`Organization_Name`, `Registration_Master`.`Employee_Name`, `Registration_Master`.`Phone`,");
            sb.Append(" `Registration_Master`.`Email`, `Registration_Master`.`City`, `Registration_Master`.`Country`, `Registration_Master`.`Status`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Registered_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Registered_Time`) AS 'Registered_On',");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Approved_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Approved_Time`) AS 'Approved_On', `Registration_Master`.`Approved_By`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Activated_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Activated_Time`) AS 'Activated_On',");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Rejected_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Rejected_Time`) AS 'Rejected_On', `Registration_Master`.`Rejected_By`,");
            sb.Append(" `Registration_Master`.`Sale_Channel`, `Registration_Master`.`Sale_Person`, `Registration_Master`.`Remarks`,");
            sb.Append(" DATE_FORMAT(`Registration_Master`.`Created_On`,'%d-%m-%Y %H:%i:%S') AS 'Created_On', DATE_FORMAT(`Registration_Master`.`Updated_On`,'%d-%m-%Y %H:%i:%S') AS 'Updated_On'");
            sb.Append(" FROM `Registration_Master`");
            sb.Append(" WHERE (`Registration_Master`.`Organization_Name` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Registration_Master`.`Email` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Registration_Master`.`City` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" OR `Registration_Master`.`Country` LIKE CONCAT('%','" + searchData + "','%')");
            sb.Append(" ) AND `Registration_Master`.`Status` IN ('Registered','Approved','Rejected')");
            sb.Append(" ORDER BY `Registration_Master`.`Registration_Master_Id` DESC LIMIT 100;");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }

        public static DataTable Read(string conStr, string email)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT `Registration_Master`.`Organization_Name`, `Registration_Master`.`Employee_Name`, `Registration_Master`.`Phone`,");
            sb.Append(" `Registration_Master`.`Email`, `Registration_Master`.`City`, `Registration_Master`.`Country`, `Registration_Master`.`Status`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Registered_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Registered_Time`) AS 'Registered_On',");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Approved_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Approved_Time`) AS 'Approved_On', `Registration_Master`.`Approved_By`,");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Activated_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Activated_Time`) AS 'Activated_On',");
            sb.Append(" CONCAT(DATE_FORMAT(`Registration_Master`.`Rejected_Date`,'%d-%m-%Y'),' ', `Registration_Master`.`Rejected_Time`) AS 'Rejected_On', `Registration_Master`.`Rejected_By`,");
            sb.Append(" `Registration_Master`.`Sale_Channel`, `Registration_Master`.`Sale_Person`, `Registration_Master`.`Remarks`,");
            sb.Append(" DATE_FORMAT(`Registration_Master`.`Created_On`,'%d-%m-%Y %H:%i:%S') AS 'Created_On', DATE_FORMAT(`Registration_Master`.`Updated_On`,'%d-%m-%Y %H:%i:%S') AS 'Updated_On'");
            sb.Append(" FROM `Registration_Master`");
            sb.Append(" WHERE `Registration_Master`.`Email` = '" + email + "';");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }

        public static DataTable UpdateStatus(string conStr, string updatedBy, Registration registration)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CALL `pr_updateStatus_Registration_Master`(");
            sb.Append(SqlService.AddStringSQLParameter(registration.Email, true, null));
            sb.Append(SqlService.AddStringSQLParameter(registration.Status, true, null));
            sb.Append(SqlService.AddStringSQLParameter(updatedBy, false, null));
            sb.Append(");");

            return SqlService.MySQLExecuteReaderDataTable(conStr, sb.ToString());
        }

        public static SqlResponse UpdateRemarks(string conStr, Registration registration)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UPDATE `Registration_Master` SET");
            sb.Append(" `Registration_Master`.`Remarks` = '" + registration.Remarks + "'");
            sb.Append(" WHERE `Registration_Master`.`Email` = '" + registration.Email + "';");

            int result = SqlService.MySQLExecuteNonQuery(conStr, sb.ToString());
            if (result >= 0)
            {
                return new SqlResponse { status = true, message = "Remarks Updated Successfully.", count = result };
            }
            else
            {
                return new SqlResponse { status = false, message = "Cannot Update Remarks." };
            }
        }
    }
}

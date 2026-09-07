using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace laptop_service.Models
{
    public class Tenant
    {
        [StringLength(100), DisplayName("Account Id")]
        public string? Account_Id { get; set; }

        [StringLength(10), DisplayName("Organization Code")]
        public string? Organization_Code { get; set; }

        [StringLength(100), DisplayName("Sub Domain URL")]
        public string? Sub_Domain_URL { get; set; }

        [StringLength(30), DisplayName("Database Server")]
        public string? Database_Server { get; set; }

        [StringLength(30), DisplayName("Database Name")]
        public string? Database_Name { get; set; }

        [StringLength(100), DisplayName("Organization Name")]
        public string? Organization_Name { get; set; }

        [StringLength(100), DisplayName("Employee Name")]
        public string? Employee_Name { get; set; }

        [StringLength(20), DisplayName("Phone")]
        public string? Phone { get; set; }

        [StringLength(100), DisplayName("Email")]
        public string? Email { get; set; }

        [StringLength(100), DisplayName("City")]
        public string? City { get; set; }

        [StringLength(100), DisplayName("Country")]
        public string? Country { get; set; }

        [StringLength(50), DisplayName("Subscription Plan")]
        public string? Subscription_Plan { get; set; }

        [StringLength(10), DisplayName("Subscription Expiry")]
        public string? Subscription_Expiry { get; set; }

        [StringLength(1000), DisplayName("Subscription Notes")]
        public string? Subscription_Notes { get; set; }

        [StringLength(1), DisplayName("Is Active")]
        public string? Is_Active { get; set; }
    }

    public class TenantMaster
    {
        public string? Tenant_Master_Id { get; set; }
        public string? Account_Id { get; set; }
        public string? Organization_Code { get; set; }
        public string? Sub_Domain_URL { get; set; }
        public string? Database_Server { get; set; }
        public string? Database_Name { get; set; }
        public string? Organization_Name { get; set; }
        public string? Employee_Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Activated_Date { get; set; }
        public string? Activated_Time { get; set; }
        public string? Subscription_Plan { get; set; }
        public string? Subscription_Expiry { get; set; }
        public string? Subscription_Notes { get; set; }
        public string? Is_Active { get; set; }
        public string? Is_Deleted { get; set; }
        public string? Created_On { get; set; }
        public string? Updated_On { get; set; }
    }
    public class TenantMasterProperty
    {
        public string? Tenant_Master_Property_Id { get; set; }
        public string? Organization_Code { get; set; }
        public string? SQL_Version_No { get; set; }
        public string? SQL_Version_On { get; set; }
    }
    public class TenantMasterSQLLog
    {
        public string? Tenant_Master_SQL_Log_Id { get; set; }
        public string? Organization_Code { get; set; }
        public string? SQL_Version_No { get; set; }
        public string? SQL_Version_On { get; set; }
        public string? SQL_Error_Log { get; set; }
    }

    public class TenantDetail
    {
        public TenantDetail(DataTable dataTable)
        {
            if (dataTable.Rows.Count > 0)
            {
                try
                {
                    Tenant_Master_Id = dataTable.Rows[0]["Tenant_Master_Id"].ToString();
                }
                catch { }
                try
                {
                    Account_Id = dataTable.Rows[0]["Account_Id"].ToString();
                }
                catch { }
                try
                {
                    Organization_Code = dataTable.Rows[0]["Organization_Code"].ToString();
                }
                catch { }
                try
                {
                    Database_Server = dataTable.Rows[0]["Database_Server"].ToString();
                }
                catch { }
                try
                {
                    Database_Name = dataTable.Rows[0]["Database_Name"].ToString();
                }
                catch { }
                try
                {
                    Email = dataTable.Rows[0]["Email"].ToString();
                }
                catch { }
                try
                {
                    Sub_Domain_URL = dataTable.Rows[0]["Sub_Domain_URL"].ToString();
                }
                catch { }
                try
                {
                    Is_Active = dataTable.Rows[0]["Is_Active"].ToString();
                }
                catch { }
                try
                {
                    Created_On = dataTable.Rows[0]["Created_On"].ToString();
                }
                catch { }
                try
                {
                    Updated_On = dataTable.Rows[0]["Updated_On"].ToString();
                }
                catch { }
            }
        }
        public string? Tenant_Master_Id { get; set; }
        public string? Account_Id { get; set; }
        public string? Organization_Code { get; set; }
        public string? Database_Server { get; set; }
        public string? Database_Name { get; set; }
        public string? Email { get; set; }
        public string? Sub_Domain_URL { get; set; }
        public string? Is_Active { get; set; }
        public string? Created_On { get; set; }
        public string? Updated_On { get; set; }
    }

}

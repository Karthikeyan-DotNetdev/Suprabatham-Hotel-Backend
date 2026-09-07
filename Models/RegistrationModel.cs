using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace laptop_service.Models
{
    public class Registration
    {
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

        [StringLength(30), DisplayName("Status")]
        public string? Status { get; set; }

        [StringLength(100), DisplayName("Sale Channel")]
        public string? Sale_Channel { get; set; }

        [StringLength(100), DisplayName("Sale Person")]
        public string? Sale_Person { get; set; }

        [StringLength(1000), DisplayName("Remarks")]
        public string? Remarks { get; set; }
    }

    public class RegistrationMaster
    {
        public string? Registration_Master_Id { get; set; }
        public string? Organization_Name { get; set; }
        public string? Employee_Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Registered_Date { get; set; }
        public string? Registered_Time { get; set; }
        public string? Approved_Date { get; set; }
        public string? Approved_Time { get; set; }
        public string? Approved_By { get; set; }
        public string? Activated_Date { get; set; }
        public string? Activated_Time { get; set; }
        public string? Rejected_Date { get; set; }
        public string? Rejected_Time { get; set; }
        public string? Rejected_By { get; set; }
        public string? Status { get; set; }
        public string? Sale_Channel { get; set; }
        public string? Sale_Person { get; set; }
        public string? Remarks { get; set; }
        public string? Created_On { get; set; }
        public string? Updated_On { get; set; }
    }

}

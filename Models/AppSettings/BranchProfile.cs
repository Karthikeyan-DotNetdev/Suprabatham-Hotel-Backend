using System;

namespace laptop_service.Models.AppSettings
{
    public class BranchProfile
    {
        public string Branch_Code { get; set; } = string.Empty;
        public string Restaurant_Name { get; set; } = string.Empty;
        public string? Address_Line1 { get; set; }
        public string? Address_Line2 { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? GSTIN { get; set; }
        public string? FSSAI_Number { get; set; }
        public string? Logo_Url { get; set; }
        public string? Is_Active { get; set; } = "A";
        public string? Created_By { get; set; }
        public DateTime? Created_On { get; set; }
        public string? Updated_By { get; set; }
        public DateTime? Updated_On { get; set; }
    }
}

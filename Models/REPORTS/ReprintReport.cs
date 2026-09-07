using System;

namespace laptop_service.Models.REPORTS
{
    public class ReprintReport
    {
        public DateTime Log_Time { get; set; }
        public string User_Name { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string Action_Type { get; set; } = string.Empty;
        public string Action_Details { get; set; } = string.Empty;
    }
}

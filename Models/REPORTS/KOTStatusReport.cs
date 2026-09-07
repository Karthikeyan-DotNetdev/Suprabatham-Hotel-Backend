using System;

namespace laptop_service.Models.REPORTS
{
    public class KOTStatusReport
    {
        public int KOT_Id { get; set; }
        public string KOT_No { get; set; } = string.Empty;
        public DateTime KOT_Time { get; set; }
        public string Order_Type { get; set; } = string.Empty;
        public string Table_Name { get; set; } = string.Empty;
        public string Customer_Name { get; set; } = string.Empty;
        public string Order_Status { get; set; } = string.Empty;
        public string Created_By { get; set; } = string.Empty;
        public string? Bill_No { get; set; }
        public decimal? Bill_Amount { get; set; }
    }
}

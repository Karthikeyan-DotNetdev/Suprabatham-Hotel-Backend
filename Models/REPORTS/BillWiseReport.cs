using System;

namespace laptop_service.Models.REPORTS
{
    public class BillWiseReport
    {
        public string Bill_No { get; set; } = string.Empty;
        public string KOT_No { get; set; } = string.Empty;
        public DateTime Bill_Time { get; set; }
        public string Order_Type { get; set; } = string.Empty;
        public string Table_Name { get; set; } = string.Empty;
        public string Customer_Name { get; set; } = string.Empty;
        public decimal Sub_Total { get; set; }
        public decimal Tax_Amount { get; set; }
        public decimal Grand_Total { get; set; }
        public string? Payment_Mode { get; set; }
        public string Billed_By { get; set; } = string.Empty;
    }
}

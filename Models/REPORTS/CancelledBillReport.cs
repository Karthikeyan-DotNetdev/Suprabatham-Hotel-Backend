using System;

namespace laptop_service.Models.REPORTS
{
    public class CancelledBillReport
    {
        public string Bill_No { get; set; } = string.Empty;
        public string KOT_No { get; set; } = string.Empty;
        public DateTime Bill_Time { get; set; }
        public decimal Grand_Total { get; set; }
        public string Cancelled_By { get; set; } = string.Empty;
        public DateTime Cancelled_Time { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}

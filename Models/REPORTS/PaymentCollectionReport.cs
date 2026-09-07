using System;

namespace laptop_service.Models.REPORTS
{
    public class PaymentCollectionReport
    {
        public string Bill_No { get; set; } = string.Empty;
        public string KOT_No { get; set; } = string.Empty;
        public DateTime Bill_Time { get; set; }
        public decimal Paid_Amount { get; set; }
        public string Payment_Mode { get; set; } = string.Empty;
        public string Payment_Reference { get; set; } = string.Empty;
        public string Billed_By { get; set; } = string.Empty;
    }
}

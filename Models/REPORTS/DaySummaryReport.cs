namespace laptop_service.Models.REPORTS
{
    /// <summary>
    /// Maps to SP_R_DaySummaryReport output columns.
    /// Bill_Status in DB = 'BILLED' (set by SP_SAVE_BILL_FROM_KOT).
    /// Payment_Mode in DB is stored UPPER-CASED by SaveBill endpoint.
    /// </summary>
    public class DaySummaryReport
    {
        public decimal Total_Revenue   { get; set; }
        public int     Total_Bills     { get; set; }
        public decimal Total_GST       { get; set; }
        public decimal Net_Revenue     { get; set; }
        public decimal Cash_Amount     { get; set; }
        public decimal UPI_Amount      { get; set; }
        public decimal Split_Amount    { get; set; }
        public decimal Card_Amount     { get; set; }
        public decimal Other_Amount    { get; set; }
        public int     Cancelled_Count { get; set; }
    }
}

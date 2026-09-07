namespace laptop_service.Models.REPORTS
{
    public class PaymentModeBreakupReport
    {
        public string Payment_Mode { get; set; } = string.Empty;
        public int Total_Bills { get; set; }
        public decimal Total_Amount { get; set; }
        public decimal Percentage_Of_Total { get; set; }
    }
}

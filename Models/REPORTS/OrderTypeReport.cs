namespace laptop_service.Models.REPORTS
{
    public class OrderTypeReport
    {
        public string Order_Type { get; set; } = string.Empty;
        public int Total_Bills { get; set; }
        public decimal Total_Revenue { get; set; }
        public decimal Total_GST { get; set; }
        public decimal Avg_Bill_Value { get; set; }
    }
}

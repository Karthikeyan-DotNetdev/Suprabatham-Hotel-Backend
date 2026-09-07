namespace laptop_service.Models.REPORTS
{
    public class ItemWiseReport
    {
        public string Product_Code { get; set; } = string.Empty;
        public string Product_Name { get; set; } = string.Empty;
        public string Variant_Name { get; set; } = string.Empty;
        public decimal Total_Qty { get; set; }
        public decimal Avg_Rate { get; set; }
        public decimal Total_Amount { get; set; }
    }
}

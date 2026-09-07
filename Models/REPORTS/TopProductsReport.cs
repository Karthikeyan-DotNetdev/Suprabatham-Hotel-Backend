namespace laptop_service.Models.REPORTS
{
    public class TopProductsReport
    {
        public long Product_Rank { get; set; }
        public string Product_Code { get; set; } = string.Empty;
        public string Product_Name { get; set; } = string.Empty;
        public decimal Total_Qty { get; set; }
        public decimal Total_Revenue { get; set; }
    }
}

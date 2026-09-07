namespace laptop_service.Models.REPORTS
{
    public class CategoryWiseReport
    {
        public string Category_Code { get; set; } = string.Empty;
        public string Category_Name { get; set; } = string.Empty;
        public decimal Total_Qty { get; set; }
        public decimal Total_Amount { get; set; }
        public decimal Contribution_Percentage { get; set; }
    }
}

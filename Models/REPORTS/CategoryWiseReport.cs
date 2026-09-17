using System.Collections.Generic;

namespace laptop_service.Models.REPORTS
{
    public class CategoryWiseReport
    {
        public string Category_Code { get; set; } = string.Empty;
        public string Category_Name { get; set; } = string.Empty;
        public int Total_Qty { get; set; }
        public decimal Total_Amount { get; set; }
        public decimal Contribution_Percentage { get; set; }
        public List<CategoryItemDetail> Items { get; set; } = new List<CategoryItemDetail>();
    }

    public class CategoryItemDetail
    {
        public string Product_Code { get; set; } = string.Empty;
        public string Product_Name { get; set; } = string.Empty;
        public string Variant_Name { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Amount { get; set; }
        public decimal Avg_Rate { get; set; }
        public decimal Category_Share_Pct { get; set; }
    }
}

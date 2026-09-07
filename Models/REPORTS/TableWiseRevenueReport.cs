namespace laptop_service.Models.REPORTS
{
    public class TableWiseRevenueReport
    {
        public string Table_Code { get; set; } = string.Empty;
        public string Table_Name { get; set; } = string.Empty;
        public int Total_Bills { get; set; }
        public decimal Total_Revenue { get; set; }
        public decimal Avg_Spend_Per_Table { get; set; }
    }
}

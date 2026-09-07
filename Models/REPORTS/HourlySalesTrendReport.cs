namespace laptop_service.Models.REPORTS
{
    public class HourlySalesTrendReport
    {
        public int Hour_Number { get; set; }
        public string Hour_Text { get; set; } = string.Empty;
        public int Total_Bills { get; set; }
        public decimal Total_Revenue { get; set; }
        public decimal Average_Bill { get; set; }
    }
}

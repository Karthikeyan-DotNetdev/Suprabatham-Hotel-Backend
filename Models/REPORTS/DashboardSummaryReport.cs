namespace laptop_service.Models.REPORTS
{
    public class DashboardSummaryReport
    {
        public decimal Today_Sales      { get; set; }
        public int     Total_Bills      { get; set; }
        public int     Total_KOTs       { get; set; }
        public int     Occupied_Tables  { get; set; }
        public int     Total_Tables     { get; set; }
        public int     Total_Customers  { get; set; }
        public int     Pending_KOTs     { get; set; }
    }
}

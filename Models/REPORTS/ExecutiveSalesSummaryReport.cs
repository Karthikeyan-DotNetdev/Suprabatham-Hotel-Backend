using System;

namespace laptop_service.Models.REPORTS
{
    public class ExecutiveSalesSummaryReport
    {
        public DateTime Bill_Date { get; set; }
        public decimal Total_Revenue { get; set; }
        public int Total_Bills { get; set; }
        public decimal Gross_Sales { get; set; }
        public decimal Total_Discounts { get; set; }
        public decimal Total_Tax { get; set; }
        public decimal Average_Ticket_Value { get; set; }
        public decimal Discount_Ratio { get; set; }
        public decimal Cash_Amount { get; set; }
        public decimal UPI_Amount { get; set; }
        public decimal Card_Amount { get; set; }
        public string Split_Amount { get; set; }
        public decimal Other_Amount { get; set; }
        public int Cancelled_Bills_Count { get; set; }
        public decimal Cancelled_Bills_Amount { get; set; }
        public decimal Cancelled_Ratio { get; set; }
    }
}

using System;

namespace laptop_service.Models.REPORTS
{
    public class SalesSummaryReport
    {
        public DateTime Bill_Date { get; set; }
        public decimal Total_Revenue { get; set; }
        public int Total_Bills { get; set; }
        public decimal Sub_Total { get; set; }
        public decimal Tax_Amount { get; set; }
        public decimal Discount_Amount { get; set; }
        public decimal Cash_Amount { get; set; }
        public decimal UPI_Amount { get; set; }
        public string Split_Amount { get; set; }
        public decimal Card_Amount { get; set; }
        public decimal Other_Amount { get; set; }
        public decimal DineIn_Amount { get; set; }
        public decimal Takeaway_Amount { get; set; }
        public decimal Delivery_Amount { get; set; }
        public int DineIn_Count { get; set; }
        public int Takeaway_Count { get; set; }
        public int Delivery_Count { get; set; }
        public int Cancelled_Bills_Count { get; set; }
        public decimal Cancelled_Bills_Amount { get; set; }
    }
}

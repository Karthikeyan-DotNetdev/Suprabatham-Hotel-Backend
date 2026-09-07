using System;

namespace laptop_service.Models.REPORTS
{
    public class GSTReport
    {
        public string Bill_No { get; set; } = string.Empty;
        public string KOT_No { get; set; } = string.Empty;
        public DateTime Bill_Date { get; set; }
        public decimal Sub_Total { get; set; }
        public decimal Discount_Amount { get; set; }
        public decimal Taxable_Value { get; set; }
        public decimal CGST_Amount { get; set; }
        public decimal SGST_Amount { get; set; }
        public decimal Tax_Amount { get; set; }
        public decimal Grand_Total { get; set; }
    }
}

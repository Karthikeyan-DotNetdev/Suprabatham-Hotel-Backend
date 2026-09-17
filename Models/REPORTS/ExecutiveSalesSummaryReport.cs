using System;
using System.Collections.Generic;

namespace laptop_service.Models.REPORTS
{
    public class ExecutiveSalesSummaryResponse
    {
        public ExecutiveBillingSuccess Billing_Success { get; set; } = new();
        public ExecutiveBillingCancel Billing_Cancel { get; set; } = new();
        public List<ExecutiveOrderTypeSummary> Order_Types { get; set; } = new();
        public List<ExecutivePaymentModeSummary> Payment_Modes { get; set; } = new();
    }

    public class ExecutiveBillingSuccess
    {
        public int Order_Count { get; set; }
        public string Min_Invoice_No { get; set; } = "";
        public string Max_Invoice_No { get; set; } = "";
        public string Invoice_Nos_Range => string.IsNullOrEmpty(Min_Invoice_No) ? "-" : $"{Min_Invoice_No} - {Max_Invoice_No}";
        public decimal Sub_Total { get; set; }
        public decimal Discount { get; set; }
        public decimal Delivery_Charge { get; set; }
        public decimal Container_Charge { get; set; }
        public decimal Service_Charge { get; set; }
        public decimal Additional_Charge { get; set; }
        public decimal Round_Off { get; set; }
        public decimal Waived_Off { get; set; }
        public decimal Grand_Total { get; set; }
        public decimal Net_Sales { get; set; }
    }

    public class ExecutiveBillingCancel
    {
        public int Order_Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExecutiveOrderTypeSummary
    {
        public string Order_Type { get; set; } = "";
        public int Count { get; set; }
        public decimal Total_Amount { get; set; }
    }

    public class ExecutivePaymentModeSummary
    {
        public string Payment_Type { get; set; } = "";
        public int Count { get; set; }
        public decimal Total_Amount { get; set; }
    }
}

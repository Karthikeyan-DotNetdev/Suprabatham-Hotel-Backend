using System;
using System.Data;
using System.Collections.Generic;
using laptop_service.Models.REPORTS;
using CommonServices;

namespace laptop_service.Services.REPORTS
{
    public class PaymentReportsService
    {
        // ─────────────────────────────────────────────────────────────────────
        // 1. GET PAYMENT COLLECTION REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<PaymentCollectionReport> GetPaymentCollection(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_PaymentCollection 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<PaymentCollectionReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new PaymentCollectionReport
                    {
                        Bill_No           = row["Bill_No"]?.ToString() ?? string.Empty,
                        KOT_No            = row["KOT_No"]?.ToString() ?? string.Empty,
                        Bill_Time         = Convert.ToDateTime(row["Bill_Time"]),
                        Paid_Amount       = Convert.ToDecimal(row["Paid_Amount"]),
                        Payment_Mode      = row["Payment_Mode"]?.ToString() ?? string.Empty,
                        Payment_Reference = row["Payment_Reference"]?.ToString() ?? string.Empty,
                        Billed_By         = row["Billed_By"]?.ToString() ?? string.Empty
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. GET PAYMENT MODE BREAKUP REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<PaymentModeBreakupReport> GetPaymentModeBreakup(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_PaymentModeBreakup 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<PaymentModeBreakupReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new PaymentModeBreakupReport
                    {
                        Payment_Mode        = row["Payment_Mode"]?.ToString() ?? string.Empty,
                        Total_Bills         = Convert.ToInt32(row["Total_Bills"]),
                        Total_Amount        = Convert.ToDecimal(row["Total_Amount"]),
                        Percentage_Of_Total = Convert.ToDecimal(row["Percentage_Of_Total"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // SQL ESCAPE HELPER
        // ─────────────────────────────────────────────────────────────────────
        private static string EscapeSql(string? value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Replace("'", "''");
        }
    }
}

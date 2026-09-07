using System;
using System.Data;
using System.Collections.Generic;
using laptop_service.Models.REPORTS;
using CommonServices;

namespace laptop_service.Services.REPORTS
{
    public class FinanceReportsService
    {
        // ─────────────────────────────────────────────────────────────────────
        // 1. GET GST SALES REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<GSTReport> GetGSTReport(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_GSTReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<GSTReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new GSTReport
                    {
                        Bill_No         = row["Bill_No"]?.ToString() ?? string.Empty,
                        KOT_No          = row["KOT_No"]?.ToString() ?? string.Empty,
                        Bill_Date       = Convert.ToDateTime(row["Bill_Date"]),
                        Sub_Total       = Convert.ToDecimal(row["Sub_Total"]),
                        Discount_Amount = Convert.ToDecimal(row["Discount_Amount"]),
                        Taxable_Value   = Convert.ToDecimal(row["Taxable_Value"]),
                        CGST_Amount     = Convert.ToDecimal(row["CGST_Amount"]),
                        SGST_Amount     = Convert.ToDecimal(row["SGST_Amount"]),
                        Tax_Amount      = Convert.ToDecimal(row["Tax_Amount"]),
                        Grand_Total     = Convert.ToDecimal(row["Grand_Total"])
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

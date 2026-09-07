using System;
using System.Data;
using System.Collections.Generic;
using laptop_service.Models.REPORTS;
using CommonServices;

namespace laptop_service.Services.REPORTS
{
    public class ExceptionReportsService
    {
        // ─────────────────────────────────────────────────────────────────────
        // 1. GET CANCELLED BILLS REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<CancelledBillReport> GetCancelledBillReport(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_CancelledBillReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<CancelledBillReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new CancelledBillReport
                    {
                        Bill_No        = row["Bill_No"]?.ToString() ?? string.Empty,
                        KOT_No         = row["KOT_No"]?.ToString() ?? string.Empty,
                        Bill_Time      = row["Bill_Time"] != DBNull.Value ? Convert.ToDateTime(row["Bill_Time"]) : DateTime.MinValue,
                        Grand_Total    = row["Grand_Total"] != DBNull.Value ? Convert.ToDecimal(row["Grand_Total"]) : 0m,
                        Cancelled_By   = row["Cancelled_By"]?.ToString() ?? string.Empty,
                        Cancelled_Time = row["Cancelled_Time"] != DBNull.Value ? Convert.ToDateTime(row["Cancelled_Time"]) : DateTime.MinValue,
                        Remarks        = row["Remarks"]?.ToString() ?? string.Empty
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

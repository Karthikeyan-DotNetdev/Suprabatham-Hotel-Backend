using System;
using System.Data;
using laptop_service.Models.REPORTS;
using CommonServices;

namespace laptop_service.Services.REPORTS
{
    public class DashboardService
    {
        public DashboardSummaryReport GetDashboardSummary(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_DashboardSummary 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var report = new DashboardSummaryReport();

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                report.Today_Sales     = Convert.ToDecimal(row["Today_Sales"]);
                report.Total_Bills     = Convert.ToInt32(row["Total_Bills"]);
                report.Total_KOTs      = Convert.ToInt32(row["Total_KOTs"]);
                report.Occupied_Tables = Convert.ToInt32(row["Occupied_Tables"]);
                report.Total_Tables    = Convert.ToInt32(row["Total_Tables"]);
                report.Total_Customers = Convert.ToInt32(row["Total_Customers"]);
                report.Pending_KOTs    = Convert.ToInt32(row["Pending_KOTs"]);
            }

            return report;
        }

        private static string EscapeSql(string? value)
        {
            return (value ?? string.Empty).Trim().Replace("'", "''");
        }
    }
}

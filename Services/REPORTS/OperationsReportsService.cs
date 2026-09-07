using System;
using System.Data;
using System.Collections.Generic;
using laptop_service.Models.REPORTS;
using CommonServices;

namespace laptop_service.Services.REPORTS
{
    public class OperationsReportsService
    {
        // ─────────────────────────────────────────────────────────────────────
        // 1. GET PENDING KOTs
        // ─────────────────────────────────────────────────────────────────────
        public List<KOTPendingReport> GetKOTPending(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_KOTPending 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<KOTPendingReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new KOTPendingReport
                    {
                        KOT_Id        = Convert.ToInt32(row["KOT_Id"]),
                        KOT_No        = row["KOT_No"]?.ToString() ?? string.Empty,
                        KOT_Time      = Convert.ToDateTime(row["KOT_Time"]),
                        Order_Type    = row["Order_Type"]?.ToString() ?? string.Empty,
                        Table_Name    = row["Table_Name"]?.ToString() ?? string.Empty,
                        Customer_Name = row["Customer_Name"]?.ToString() ?? string.Empty,
                        Guest_Count   = Convert.ToInt32(row["Guest_Count"]),
                        Order_Status  = row["Order_Status"]?.ToString() ?? string.Empty,
                        Created_By    = row["Created_By"]?.ToString() ?? string.Empty,
                        Total_Items   = Convert.ToInt32(row["Total_Items"]),
                        Total_Qty     = Convert.ToDecimal(row["Total_Qty"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. GET KOT STATUS AUDIT LOG
        // ─────────────────────────────────────────────────────────────────────
        public List<KOTStatusReport> GetKOTStatus(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_KOTStatus 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<KOTStatusReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new KOTStatusReport
                    {
                        KOT_Id        = Convert.ToInt32(row["KOT_Id"]),
                        KOT_No        = row["KOT_No"]?.ToString() ?? string.Empty,
                        KOT_Time      = Convert.ToDateTime(row["KOT_Time"]),
                        Order_Type    = row["Order_Type"]?.ToString() ?? string.Empty,
                        Table_Name    = row["Table_Name"]?.ToString() ?? string.Empty,
                        Customer_Name = row["Customer_Name"]?.ToString() ?? string.Empty,
                        Order_Status  = row["Order_Status"]?.ToString() ?? string.Empty,
                        Created_By    = row["Created_By"]?.ToString() ?? string.Empty,
                        Bill_No       = row["Bill_No"]?.ToString(),
                        Bill_Amount   = row["Bill_Amount"] != DBNull.Value ? Convert.ToDecimal(row["Bill_Amount"]) : (decimal?)null
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. GET HOURLY SALES TREND
        // ─────────────────────────────────────────────────────────────────────
        public List<HourlySalesTrendReport> GetHourlySalesTrend(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_HourlySalesTrend 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<HourlySalesTrendReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new HourlySalesTrendReport
                    {
                        Hour_Number   = Convert.ToInt32(row["Hour_Number"]),
                        Hour_Text     = row["Hour_Text"]?.ToString() ?? string.Empty,
                        Total_Bills   = Convert.ToInt32(row["Total_Bills"]),
                        Total_Revenue = Convert.ToDecimal(row["Total_Revenue"]),
                        Average_Bill  = Convert.ToDecimal(row["Average_Bill"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. GET TABLE-WISE REVENUE
        // ─────────────────────────────────────────────────────────────────────
        public List<TableWiseRevenueReport> GetTableWiseRevenue(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_TableWiseRevenue 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<TableWiseRevenueReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new TableWiseRevenueReport
                    {
                        Table_Code           = row["Table_Code"]?.ToString() ?? string.Empty,
                        Table_Name           = row["Table_Name"]?.ToString() ?? string.Empty,
                        Total_Bills          = Convert.ToInt32(row["Total_Bills"]),
                        Total_Revenue        = Convert.ToDecimal(row["Total_Revenue"]),
                        Avg_Spend_Per_Table  = Convert.ToDecimal(row["Avg_Spend_Per_Table"])
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

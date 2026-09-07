using System;
using System.Data;
using System.Collections.Generic;
using laptop_service.Models.REPORTS;
using CommonServices;

namespace laptop_service.Services.REPORTS
{
    public class SalesSummaryService
    {
        // ─────────────────────────────────────────────────────────────────────
        // 1. GET DAY SUMMARY REPORT
        // ─────────────────────────────────────────────────────────────────────
        public DaySummaryReport GetDaySummary(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_DaySummaryReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);

            var report = new DaySummaryReport();

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                report.Total_Revenue   = Convert.ToDecimal(row["Total_Revenue"]);
                report.Total_Bills     = Convert.ToInt32(row["Total_Bills"]);
                report.Total_GST       = Convert.ToDecimal(row["Total_GST"]);
                report.Net_Revenue     = Convert.ToDecimal(row["Net_Revenue"]);
                report.Cash_Amount     = Convert.ToDecimal(row["Cash_Amount"]);
                report.UPI_Amount      = Convert.ToDecimal(row["UPI_Amount"]);
                report.Split_Amount    = Convert.ToDecimal(row["Split_Amount"]);
                report.Card_Amount     = Convert.ToDecimal(row["Card_Amount"]);
                report.Other_Amount    = Convert.ToDecimal(row["Other_Amount"]);
                report.Cancelled_Count = Convert.ToInt32(row["Cancelled_Count"]);
            }

            return report;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. GET BILL-WISE REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<BillWiseReport> GetBillWiseReport(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_BillWiseReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);

            var list = new List<BillWiseReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new BillWiseReport
                    {
                        Bill_No       = row["Bill_No"]?.ToString() ?? string.Empty,
                        KOT_No        = row["KOT_No"]?.ToString() ?? string.Empty,
                        Bill_Time     = Convert.ToDateTime(row["Bill_Time"]),
                        Order_Type    = row["Order_Type"]?.ToString() ?? string.Empty,
                        Table_Name    = row["Table_Name"]?.ToString() ?? string.Empty,
                        Customer_Name = row["Customer_Name"]?.ToString() ?? string.Empty,
                        Sub_Total     = Convert.ToDecimal(row["Sub_Total"]),
                        Tax_Amount    = Convert.ToDecimal(row["Tax_Amount"]),
                        Grand_Total   = Convert.ToDecimal(row["Grand_Total"]),
                        Payment_Mode  = row["Payment_Mode"]?.ToString(),
                        Billed_By     = row["Billed_By"]?.ToString() ?? string.Empty
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. GET ITEM-WISE REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<ItemWiseReport> GetItemWiseReport(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_ItemWiseReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);

            var list = new List<ItemWiseReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ItemWiseReport
                    {
                        Product_Code = row["Product_Code"]?.ToString() ?? string.Empty,
                        Product_Name = row["Product_Name"]?.ToString() ?? string.Empty,
                        Variant_Name = row["Variant_Name"]?.ToString() ?? string.Empty,
                        Total_Qty    = Convert.ToDecimal(row["Total_Qty"]),
                        Avg_Rate     = Convert.ToDecimal(row["Avg_Rate"]),
                        Total_Amount = Convert.ToDecimal(row["Total_Amount"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. GET CATEGORY-WISE REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<CategoryWiseReport> GetCategoryWiseReport(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_CategoryWiseReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);

            var list = new List<CategoryWiseReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new CategoryWiseReport
                    {
                        Category_Code           = row["Category_Code"]?.ToString() ?? string.Empty,
                        Category_Name           = row["Category_Name"]?.ToString() ?? string.Empty,
                        Total_Qty               = Convert.ToDecimal(row["Total_Qty"]),
                        Total_Amount            = Convert.ToDecimal(row["Total_Amount"]),
                        Contribution_Percentage = Convert.ToDecimal(row["Contribution_Percentage"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. GET ORDER TYPE REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<OrderTypeReport> GetOrderTypeReport(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_OrderTypeReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);

            var list = new List<OrderTypeReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new OrderTypeReport
                    {
                        Order_Type      = row["Order_Type"]?.ToString() ?? string.Empty,
                        Total_Bills     = Convert.ToInt32(row["Total_Bills"]),
                        Total_Revenue   = Convert.ToDecimal(row["Total_Revenue"]),
                        Total_GST       = Convert.ToDecimal(row["Total_GST"]),
                        Avg_Bill_Value  = Convert.ToDecimal(row["Avg_Bill_Value"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. GET TOP PRODUCTS REPORT
        // ─────────────────────────────────────────────────────────────────────
        public List<TopProductsReport> GetTopProductsReport(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_TopProductsReport 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);

            var list = new List<TopProductsReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new TopProductsReport
                    {
                        Product_Rank  = Convert.ToInt64(row["Product_Rank"]),
                        Product_Code  = row["Product_Code"]?.ToString() ?? string.Empty,
                        Product_Name  = row["Product_Name"]?.ToString() ?? string.Empty,
                        Total_Qty     = Convert.ToDecimal(row["Total_Qty"]),
                        Total_Revenue = Convert.ToDecimal(row["Total_Revenue"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 7. GET SALES SUMMARY REPORT (List / Date-Wise)
        // ─────────────────────────────────────────────────────────────────────
        public List<SalesSummaryReport> GetSalesSummary(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_SalesSummary 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<SalesSummaryReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new SalesSummaryReport
                    {
                        Bill_Date = row.Table.Columns.Contains("Bill_Date") && row["Bill_Date"] != DBNull.Value
                                    ? Convert.ToDateTime(row["Bill_Date"]) : DateTime.MinValue,
                        Total_Revenue = Convert.ToDecimal(row["Total_Revenue"]),
                        Total_Bills = Convert.ToInt32(row["Total_Bills"]),
                        Sub_Total = Convert.ToDecimal(row["Sub_Total"]),
                        Tax_Amount = Convert.ToDecimal(row["Tax_Amount"]),
                        Discount_Amount = Convert.ToDecimal(row["Discount_Amount"]),
                        Cash_Amount = Convert.ToDecimal(row["Cash_Amount"]),
                        UPI_Amount = Convert.ToDecimal(row["UPI_Amount"]),
                        Split_Amount = row["Split_Amount"]?.ToString() ?? "-",
                        Card_Amount = Convert.ToDecimal(row["Card_Amount"]),
                        Other_Amount = Convert.ToDecimal(row["Other_Amount"]),
                        DineIn_Amount = Convert.ToDecimal(row["DineIn_Amount"]),
                        Takeaway_Amount = Convert.ToDecimal(row["Takeaway_Amount"]),
                        Delivery_Amount = Convert.ToDecimal(row["Delivery_Amount"]),
                        DineIn_Count = Convert.ToInt32(row["DineIn_Count"]),
                        Takeaway_Count = Convert.ToInt32(row["Takeaway_Count"]),
                        Delivery_Count = Convert.ToInt32(row["Delivery_Count"]),
                        Cancelled_Bills_Count = Convert.ToInt32(row["Cancelled_Bills_Count"]),
                        Cancelled_Bills_Amount = Convert.ToDecimal(row["Cancelled_Bills_Amount"])
                    });
                }
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 8. GET EXECUTIVE SALES SUMMARY REPORT (List / Date-Wise)
        // ─────────────────────────────────────────────────────────────────────
        public List<ExecutiveSalesSummaryReport> GetExecutiveSalesSummary(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC SP_R_ExecutiveSalesSummary 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataTable dt = SQLService.GetDataTable(query);
            var list = new List<ExecutiveSalesSummaryReport>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ExecutiveSalesSummaryReport
                    {
                        Bill_Date = row.Table.Columns.Contains("Bill_Date") && row["Bill_Date"] != DBNull.Value
                                    ? Convert.ToDateTime(row["Bill_Date"]) : DateTime.MinValue,
                        Total_Revenue = Convert.ToDecimal(row["Total_Revenue"]),
                        Total_Bills = Convert.ToInt32(row["Total_Bills"]),
                        Gross_Sales = Convert.ToDecimal(row["Gross_Sales"]),
                        Total_Discounts = Convert.ToDecimal(row["Total_Discounts"]),
                        Total_Tax = Convert.ToDecimal(row["Total_Tax"]),
                        Average_Ticket_Value = Convert.ToDecimal(row["Average_Ticket_Value"]),
                        Discount_Ratio = Convert.ToDecimal(row["Discount_Ratio"]),
                        Cash_Amount = Convert.ToDecimal(row["Cash_Amount"]),
                        UPI_Amount = Convert.ToDecimal(row["UPI_Amount"]),
                        Card_Amount = Convert.ToDecimal(row["Card_Amount"]),
                        Split_Amount = row["Split_Amount"]?.ToString() ?? "-",
                        Other_Amount = Convert.ToDecimal(row["Other_Amount"]),
                        Cancelled_Bills_Count = Convert.ToInt32(row["Cancelled_Bills_Count"]),
                        Cancelled_Bills_Amount = Convert.ToDecimal(row["Cancelled_Bills_Amount"]),
                        Cancelled_Ratio = Convert.ToDecimal(row["Cancelled_Ratio"])
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

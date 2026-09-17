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
            var list = new List<CategoryWiseReport>();
            string query = $"EXEC SP_R_CategoryWiseReport @BranchCode='{EscapeSql(branchCode)}', @FromDate='{EscapeSql(fromDate)}', @ToDate='{EscapeSql(toDate)}'";
            DataTable dt = SQLService.GetDataTable(query);

            if (dt != null && dt.Rows.Count > 0)
            {
                decimal grandTotal = 0;
                if (dt.Columns.Contains("Grand_Total_Revenue") && dt.Rows[0]["Grand_Total_Revenue"] != DBNull.Value)
                {
                    grandTotal = Convert.ToDecimal(dt.Rows[0]["Grand_Total_Revenue"]);
                }

                // Group rows by Category
                var groupedByCategory = dt.AsEnumerable().GroupBy(r => new
                {
                    Category_Code = r["Category_Code"]?.ToString() ?? "",
                    Category_Name = r["Category_Name"]?.ToString() ?? "Uncategorized"
                });

                foreach (var group in groupedByCategory)
                {
                    var catReport = new CategoryWiseReport
                    {
                        Category_Code = group.Key.Category_Code,
                        Category_Name = group.Key.Category_Name,
                        Total_Qty = group.Sum(r => Convert.ToInt32(r["Total_Qty"] == DBNull.Value ? 0 : r["Total_Qty"])),
                        Total_Amount = group.Sum(r => Convert.ToDecimal(r["Total_Amount"] == DBNull.Value ? 0 : r["Total_Amount"])),
                        Items = new List<CategoryItemDetail>()
                    };

                    catReport.Contribution_Percentage = grandTotal > 0
                        ? Math.Round((catReport.Total_Amount / grandTotal) * 100, 2)
                        : 0;

                    foreach (var r in group)
                    {
                        int itemQty = Convert.ToInt32(r["Total_Qty"] == DBNull.Value ? 0 : r["Total_Qty"]);
                        decimal itemAmount = Convert.ToDecimal(r["Total_Amount"] == DBNull.Value ? 0 : r["Total_Amount"]);

                        catReport.Items.Add(new CategoryItemDetail
                        {
                            Product_Code = r["Product_Code"]?.ToString() ?? "",
                            Product_Name = r["Product_Name"]?.ToString() ?? "",
                            Variant_Name = r["Variant_Name"]?.ToString() ?? "",
                            Qty = itemQty,
                            Amount = itemAmount,
                            Avg_Rate = itemQty > 0 ? Math.Round(itemAmount / itemQty, 2) : 0,
                            Category_Share_Pct = catReport.Total_Amount > 0
                                ? Math.Round((itemAmount / catReport.Total_Amount) * 100, 2)
                                : 0
                        });
                    }

                    list.Add(catReport);
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
        // 8. GET EXECUTIVE SALES SUMMARY REPORT (PetPooja Structured Heading-Wise)
        // ─────────────────────────────────────────────────────────────────────
        public ExecutiveSalesSummaryResponse GetExecutiveSalesSummary(string branchCode, string fromDate, string toDate)
        {
            string query = $@"
                EXEC dbo.SP_R_ExecutiveSalesSummary 
                @BranchCode = '{EscapeSql(branchCode)}', 
                @FromDate   = '{EscapeSql(fromDate)}', 
                @ToDate     = '{EscapeSql(toDate)}'";

            DataSet ds = SQLService.GetDataSet(query);
            var response = new ExecutiveSalesSummaryResponse();

            if (ds != null && ds.Tables.Count > 0)
            {
                // 1. Billing Success
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow r = ds.Tables[0].Rows[0];
                    response.Billing_Success = new ExecutiveBillingSuccess
                    {
                        Order_Count = r["Order_Count"] != DBNull.Value ? Convert.ToInt32(r["Order_Count"]) : 0,
                        Min_Invoice_No = r["Min_Invoice_No"]?.ToString() ?? "",
                        Max_Invoice_No = r["Max_Invoice_No"]?.ToString() ?? "",
                        Sub_Total = r["Sub_Total"] != DBNull.Value ? Convert.ToDecimal(r["Sub_Total"]) : 0,
                        Discount = r["Discount"] != DBNull.Value ? Convert.ToDecimal(r["Discount"]) : 0,
                        Delivery_Charge = r["Delivery_Charge"] != DBNull.Value ? Convert.ToDecimal(r["Delivery_Charge"]) : 0,
                        Container_Charge = r["Container_Charge"] != DBNull.Value ? Convert.ToDecimal(r["Container_Charge"]) : 0,
                        Service_Charge = r["Service_Charge"] != DBNull.Value ? Convert.ToDecimal(r["Service_Charge"]) : 0,
                        Additional_Charge = r["Additional_Charge"] != DBNull.Value ? Convert.ToDecimal(r["Additional_Charge"]) : 0,
                        Round_Off = r["Round_Off"] != DBNull.Value ? Convert.ToDecimal(r["Round_Off"]) : 0,
                        Waived_Off = r["Waived_Off"] != DBNull.Value ? Convert.ToDecimal(r["Waived_Off"]) : 0,
                        Grand_Total = r["Grand_Total"] != DBNull.Value ? Convert.ToDecimal(r["Grand_Total"]) : 0,
                        Net_Sales = r["Net_Sales"] != DBNull.Value ? Convert.ToDecimal(r["Net_Sales"]) : 0
                    };
                }

                // 2. Billing Cancel
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow r = ds.Tables[1].Rows[0];
                    response.Billing_Cancel = new ExecutiveBillingCancel
                    {
                        Order_Count = r["Cancel_Order_Count"] != DBNull.Value ? Convert.ToInt32(r["Cancel_Order_Count"]) : 0,
                        Amount = r["Cancel_Amount"] != DBNull.Value ? Convert.ToDecimal(r["Cancel_Amount"]) : 0
                    };
                }

                // 3. Order Types Breakdown
                if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                {
                    foreach (DataRow r in ds.Tables[2].Rows)
                    {
                        response.Order_Types.Add(new ExecutiveOrderTypeSummary
                        {
                            Order_Type = r["Order_Type"]?.ToString() ?? "-",
                            Count = r["Order_Count"] != DBNull.Value ? Convert.ToInt32(r["Order_Count"]) : 0,
                            Total_Amount = r["Total_Amount"] != DBNull.Value ? Convert.ToDecimal(r["Total_Amount"]) : 0
                        });
                    }
                }

                // 4. Payment Modes Breakdown
                if (ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
                {
                    foreach (DataRow r in ds.Tables[3].Rows)
                    {
                        response.Payment_Modes.Add(new ExecutivePaymentModeSummary
                        {
                            Payment_Type = r["Payment_Type"]?.ToString() ?? "-",
                            Count = r["Order_Count"] != DBNull.Value ? Convert.ToInt32(r["Order_Count"]) : 0,
                            Total_Amount = r["Total_Amount"] != DBNull.Value ? Convert.ToDecimal(r["Total_Amount"]) : 0
                        });
                    }
                }
            }

            return response;
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

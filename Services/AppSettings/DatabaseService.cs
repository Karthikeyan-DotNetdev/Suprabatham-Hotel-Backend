using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using laptop_service.Models.AppSettings;

namespace laptop_service.Services.AppSettings
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Smart_POS") 
                                ?? configuration.GetConnectionString("MySQL") 
                                ?? "";
        }

        public async Task<ReceiptSetting?> GetReceiptSettingByTillAsync(string branchCode, string tillCode)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetReceiptSetting", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchCode", branchCode);
            cmd.Parameters.AddWithValue("@TillCode", tillCode);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ReceiptSetting
                {
                    Branch_Code = reader.GetString(reader.GetOrdinal("Branch_Code")),
                    Till_Code = reader.GetString(reader.GetOrdinal("Till_Code")),
                    Logo_Image = reader.IsDBNull(reader.GetOrdinal("Logo_Image")) ? null : reader.GetString(reader.GetOrdinal("Logo_Image")),
                    Logo_Align = reader.IsDBNull(reader.GetOrdinal("Logo_Align")) ? "Center" : reader.GetString(reader.GetOrdinal("Logo_Align")),
                    Print_Language = reader.GetString(reader.GetOrdinal("Print_Language")),
                    Print_Header_Text = reader.IsDBNull(reader.GetOrdinal("Print_Header_Text")) ? null : reader.GetString(reader.GetOrdinal("Print_Header_Text")),
                    Print_Footer_Text = reader.IsDBNull(reader.GetOrdinal("Print_Footer_Text")) ? null : reader.GetString(reader.GetOrdinal("Print_Footer_Text")),
                    Print_Logo = reader.GetString(reader.GetOrdinal("Print_Logo")),
                    Print_Company_Name = reader.GetString(reader.GetOrdinal("Print_Company_Name")),
                    Print_Address_Detail = reader.GetString(reader.GetOrdinal("Print_Address_Detail")),
                    Print_Address_Line1 = reader.GetString(reader.GetOrdinal("Print_Address_Line1")),
                    Print_Address_Line2 = reader.GetString(reader.GetOrdinal("Print_Address_Line2")),
                    Print_City = reader.GetString(reader.GetOrdinal("Print_City")),
                    Print_Tax_Detail = reader.GetString(reader.GetOrdinal("Print_Tax_Detail")),
                    Print_Contact_Detail = reader.GetString(reader.GetOrdinal("Print_Contact_Detail")),
                    Print_Phone = reader.GetString(reader.GetOrdinal("Print_Phone")),
                    Print_Email = reader.GetString(reader.GetOrdinal("Print_Email")),
                    Print_Customer_Detail = reader.GetString(reader.GetOrdinal("Print_Customer_Detail")),
                    Print_Sale_Order_Type = reader.GetString(reader.GetOrdinal("Print_Sale_Order_Type")),
                    Print_DateWithTime = reader.GetString(reader.GetOrdinal("Print_DateWithTime")),
                    Print_Till_Detail = reader.GetString(reader.GetOrdinal("Print_Till_Detail")),
                    Print_Table_Detail = reader.GetString(reader.GetOrdinal("Print_Table_Detail")),
                    Print_KOT_Number = reader.GetString(reader.GetOrdinal("Print_KOT_Number")),
                    Print_Captain_Detail = reader.GetString(reader.GetOrdinal("Print_Captain_Detail")),
                    Print_Cashier_Detail = reader.GetString(reader.GetOrdinal("Print_Cashier_Detail")),
                    Print_Short_BillNo = reader.GetString(reader.GetOrdinal("Print_Short_BillNo")),
                    Print_Total_ItemsQty = reader.GetString(reader.GetOrdinal("Print_Total_ItemsQty")),
                    Print_Addon_Detail = reader.GetString(reader.GetOrdinal("Print_Addon_Detail")),
                    Print_FSS_Detail = reader.GetString(reader.GetOrdinal("Print_FSS_Detail")),
                    Print_Wide_Product_Name = reader.GetString(reader.GetOrdinal("Print_Wide_Product_Name")),
                    Print_Product_Name_Wrapping = reader.GetString(reader.GetOrdinal("Print_Product_Name_Wrapping")),
                    Print_Product_Code = reader.GetString(reader.GetOrdinal("Print_Product_Code")),
                    Print_SKU_Code = reader.GetString(reader.GetOrdinal("Print_SKU_Code")),
                    Print_HSN_SAC_Code = reader.GetString(reader.GetOrdinal("Print_HSN_SAC_Code")),
                    Print_Tax_Column = reader.GetString(reader.GetOrdinal("Print_Tax_Column")),
                    Print_Line_Item_Discount = reader.GetString(reader.GetOrdinal("Print_Line_Item_Discount")),
                    Print_Total_Savings = reader.GetString(reader.GetOrdinal("Print_Total_Savings")),
                    Print_Tax_Summary = reader.GetString(reader.GetOrdinal("Print_Tax_Summary")),
                    Print_Payment_Summary = reader.GetString(reader.GetOrdinal("Print_Payment_Summary")),
                    Print_Terms_Conditions = reader.GetString(reader.GetOrdinal("Print_Terms_Conditions")),
                    Print_Customer_Outstanding = reader.GetString(reader.GetOrdinal("Print_Customer_Outstanding")),
                    Print_Customer_Loyalty = reader.GetString(reader.GetOrdinal("Print_Customer_Loyalty")),
                    Print_Bar_Code = reader.GetString(reader.GetOrdinal("Print_Bar_Code")),
                    Print_OnTime = reader.GetString(reader.GetOrdinal("Print_OnTime")),
                    Print_Token_Number = reader.GetString(reader.GetOrdinal("Print_Token_Number"))
                };
            }
            return null;
        }

        public async Task SaveReceiptSettingAsync(ReceiptSetting s, string updatedBy)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_SaveReceiptSetting", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Branch_Code", s.Branch_Code);
            cmd.Parameters.AddWithValue("@Till_Code", s.Till_Code);
            cmd.Parameters.AddWithValue("@Logo_Image", (object?)s.Logo_Image ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Logo_Align", s.Logo_Align ?? "Center");
            cmd.Parameters.AddWithValue("@Print_Language", s.Print_Language);
            cmd.Parameters.AddWithValue("@Print_Header_Text", (object?)s.Print_Header_Text ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Print_Footer_Text", (object?)s.Print_Footer_Text ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Print_Logo", s.Print_Logo);
            cmd.Parameters.AddWithValue("@Print_Company_Name", s.Print_Company_Name);
            cmd.Parameters.AddWithValue("@Print_Address_Detail", s.Print_Address_Detail);
            cmd.Parameters.AddWithValue("@Print_Address_Line1", s.Print_Address_Line1);
            cmd.Parameters.AddWithValue("@Print_Address_Line2", s.Print_Address_Line2);
            cmd.Parameters.AddWithValue("@Print_City", s.Print_City);
            cmd.Parameters.AddWithValue("@Print_Tax_Detail", s.Print_Tax_Detail);
            cmd.Parameters.AddWithValue("@Print_Contact_Detail", s.Print_Contact_Detail);
            cmd.Parameters.AddWithValue("@Print_Phone", s.Print_Phone);
            cmd.Parameters.AddWithValue("@Print_Email", s.Print_Email);
            cmd.Parameters.AddWithValue("@Print_Customer_Detail", s.Print_Customer_Detail);
            cmd.Parameters.AddWithValue("@Print_Sale_Order_Type", s.Print_Sale_Order_Type);
            cmd.Parameters.AddWithValue("@Print_DateWithTime", s.Print_DateWithTime);
            cmd.Parameters.AddWithValue("@Print_Till_Detail", s.Print_Till_Detail);
            cmd.Parameters.AddWithValue("@Print_Table_Detail", s.Print_Table_Detail);
            cmd.Parameters.AddWithValue("@Print_KOT_Number", s.Print_KOT_Number);
            cmd.Parameters.AddWithValue("@Print_Captain_Detail", s.Print_Captain_Detail);
            cmd.Parameters.AddWithValue("@Print_Cashier_Detail", s.Print_Cashier_Detail);
            cmd.Parameters.AddWithValue("@Print_Short_BillNo", s.Print_Short_BillNo);
            cmd.Parameters.AddWithValue("@Print_Total_ItemsQty", s.Print_Total_ItemsQty);
            cmd.Parameters.AddWithValue("@Print_Addon_Detail", s.Print_Addon_Detail);
            cmd.Parameters.AddWithValue("@Print_FSS_Detail", s.Print_FSS_Detail);
            cmd.Parameters.AddWithValue("@Print_Wide_Product_Name", s.Print_Wide_Product_Name);
            cmd.Parameters.AddWithValue("@Print_Product_Name_Wrapping", s.Print_Product_Name_Wrapping);
            cmd.Parameters.AddWithValue("@Print_Product_Code", s.Print_Product_Code);
            cmd.Parameters.AddWithValue("@Print_SKU_Code", s.Print_SKU_Code);
            cmd.Parameters.AddWithValue("@Print_HSN_SAC_Code", s.Print_HSN_SAC_Code);
            cmd.Parameters.AddWithValue("@Print_Tax_Column", s.Print_Tax_Column);
            cmd.Parameters.AddWithValue("@Print_Line_Item_Discount", s.Print_Line_Item_Discount);
            cmd.Parameters.AddWithValue("@Print_Total_Savings", s.Print_Total_Savings);
            cmd.Parameters.AddWithValue("@Print_Tax_Summary", s.Print_Tax_Summary);
            cmd.Parameters.AddWithValue("@Print_Payment_Summary", s.Print_Payment_Summary);
            cmd.Parameters.AddWithValue("@Print_Terms_Conditions", s.Print_Terms_Conditions);
            cmd.Parameters.AddWithValue("@Print_Customer_Outstanding", s.Print_Customer_Outstanding);
            cmd.Parameters.AddWithValue("@Print_Customer_Loyalty", s.Print_Customer_Loyalty);
            cmd.Parameters.AddWithValue("@Print_Bar_Code", s.Print_Bar_Code);
            cmd.Parameters.AddWithValue("@Print_OnTime", s.Print_OnTime);
            cmd.Parameters.AddWithValue("@Print_Token_Number", s.Print_Token_Number);
            cmd.Parameters.AddWithValue("@Updated_By", updatedBy);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

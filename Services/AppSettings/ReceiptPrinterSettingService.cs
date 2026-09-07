using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using laptop_service.Models.AppSettings;

namespace laptop_service.Services.AppSettings
{
    public class ReceiptPrinterSettingService
    {
        private readonly string _connectionString;

        public ReceiptPrinterSettingService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Smart_POS") 
                                ?? configuration.GetConnectionString("MySQL") 
                                ?? "";
        }

        public async Task<ReceiptPrinterSetting?> GetReceiptPrinterSettingByBranchAndTillAsync(string branchCode, string tillCode)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetReceiptPrinterSetting", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchCode", branchCode);
            cmd.Parameters.AddWithValue("@TillCode", tillCode);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ReceiptPrinterSetting
                {
                    Branch_Code = reader.GetString(reader.GetOrdinal("Branch_Code")),
                    Till_Code = reader.GetString(reader.GetOrdinal("Till_Code")),
                    Printer_Width = reader.GetString(reader.GetOrdinal("Printer_Width")),
                    Printer_Interface = reader.IsDBNull(reader.GetOrdinal("Printer_Interface")) ? null : reader.GetString(reader.GetOrdinal("Printer_Interface")),
                    Interface_Detail = reader.IsDBNull(reader.GetOrdinal("Interface_Detail")) ? null : reader.GetString(reader.GetOrdinal("Interface_Detail")),
                    Start_Feed_Length = reader.GetString(reader.GetOrdinal("Start_Feed_Length")),
                    End_Feed_Length = reader.GetString(reader.GetOrdinal("End_Feed_Length")),
                    Start_Command = reader.GetString(reader.GetOrdinal("Start_Command")),
                    End_Command = reader.GetString(reader.GetOrdinal("End_Command")),
                    Print_As_Image = reader.GetString(reader.GetOrdinal("Print_As_Image")),
                    Image_Print_Command = reader.GetString(reader.GetOrdinal("Image_Print_Command")),
                    Cash_Drawer_Command = reader.GetString(reader.GetOrdinal("Cash_Drawer_Command")),
                    Test_Print_Text = reader.IsDBNull(reader.GetOrdinal("Test_Print_Text")) ? string.Empty : reader.GetString(reader.GetOrdinal("Test_Print_Text")),
                    No_Of_Print = reader.IsDBNull(reader.GetOrdinal("No_Of_Print")) ? 1 : reader.GetInt32(reader.GetOrdinal("No_Of_Print")),
                    Updated_By = reader.IsDBNull(reader.GetOrdinal("Updated_By")) ? null : reader.GetString(reader.GetOrdinal("Updated_By"))
                };
            }
            return null;
        }

        public async Task SaveReceiptPrinterSettingAsync(ReceiptPrinterSetting s, string updatedBy)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_SaveReceiptPrinterSetting", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@BranchCode", s.Branch_Code);
            cmd.Parameters.AddWithValue("@TillCode", s.Till_Code);
            cmd.Parameters.AddWithValue("@PrinterWidth", s.Printer_Width);
            cmd.Parameters.AddWithValue("@PrinterInterface", (object?)s.Printer_Interface ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@InterfaceDetail", (object?)s.Interface_Detail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartFeedLength", s.Start_Feed_Length);
            cmd.Parameters.AddWithValue("@EndFeedLength", s.End_Feed_Length);
            cmd.Parameters.AddWithValue("@StartCommand", s.Start_Command);
            cmd.Parameters.AddWithValue("@EndCommand", s.End_Command);
            cmd.Parameters.AddWithValue("@PrintAsImage", s.Print_As_Image);
            cmd.Parameters.AddWithValue("@ImagePrintCommand", s.Image_Print_Command);
            cmd.Parameters.AddWithValue("@CashDrawerCommand", s.Cash_Drawer_Command);
            cmd.Parameters.AddWithValue("@TestPrintText", (object?)s.Test_Print_Text ?? string.Empty);
            cmd.Parameters.AddWithValue("@NoOfPrint", s.No_Of_Print);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

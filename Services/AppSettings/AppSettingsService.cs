using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using laptop_service.Models.AppSettings;
using System.Data;

namespace laptop_service.Services.AppSettings
{
    /// <summary>
    /// Handles all DB operations for the M_Branch_Settings table.
    /// Covers categories: BILLING, ORDER, OPERATIONS, KITCHEN, PAYMENT, DISPLAY, SECURITY, PROFILE, INTEGRATION, REPORTS, NOTIFICATION
    ///
    /// Frontend always sends all values as strings (String(val) conversion).
    /// So this service uses Dictionary<string, string?> safely.
    /// </summary>
    public class AppSettingsService
    {
        private readonly string _connectionString;

        public AppSettingsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Smart_POS") 
                                ?? configuration.GetConnectionString("MySQL") 
                                ?? "";
        }

        // GET: Fetch all settings for a branch + category as a flat dict
        // Calls: sp_GetBranchSettings(@BranchCode, @Category)
       
        public async Task<Dictionary<string, string?>> GetSettingsAsync(string branchCode, string category)
        {
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetBranchSettings", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@BranchCode", branchCode);
            cmd.Parameters.AddWithValue("@Category",   category);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var key   = reader.GetString(reader.GetOrdinal("Setting_Key"));
                var value = reader.IsDBNull(reader.GetOrdinal("Setting_Value"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Setting_Value"));

                result[key] = value;
            }

            return result;
        }

      
        // SAVE: Upsert all key-values for a branch + category
        // Frontend sends all values pre-converted to string via String(val)
        
        public async Task SaveSettingsAsync(
            string branchCode,
            string category,
            Dictionary<string, string?> settings,
            string updatedBy = "Admin")
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            foreach (var kvp in settings)
            {
                using var cmd = new SqlCommand("sp_SaveBranchSetting", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@BranchCode",   branchCode);
                cmd.Parameters.AddWithValue("@Category",     category);
                cmd.Parameters.AddWithValue("@SettingKey",   kvp.Key);
                cmd.Parameters.AddWithValue("@SettingValue", (object?)kvp.Value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataType",     InferDataType(kvp.Value));
                cmd.Parameters.AddWithValue("@UpdatedBy",    updatedBy);

                await cmd.ExecuteNonQueryAsync();
            }
        }


        // HELPER: Infer Data_Type from string value for audit purposes

        private static string InferDataType(string? value)
        {
            if (value is null) return "STRING";
            if (value == "true" || value == "false") return "BOOLEAN";
            if (decimal.TryParse(value, out _))      return "NUMBER";
            if (value.TrimStart().StartsWith("{") || value.TrimStart().StartsWith("[")) return "JSON";
            return "STRING";
        }
    }
}

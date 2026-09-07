using System;

namespace laptop_service.Models.AppSettings
{
    /// <summary>
    /// Maps a single row from M_Branch_Settings table.
    /// Used internally by AppSettingsService for DB read/write.
    /// </summary>
    public class AppSettings
    {
        public string Branch_Code { get; set; } = string.Empty;
        public string Setting_Category { get; set; } = string.Empty;
        public string Setting_Key { get; set; } = string.Empty;
        public string? Setting_Value { get; set; }
        public string Data_Type { get; set; } = "STRING"; // STRING | NUMBER | BOOLEAN | JSON
        public string Is_Active { get; set; } = "A";
        public string? Created_By { get; set; }
        public DateTime? Created_On { get; set; }
        public string? Updated_By { get; set; }
        public DateTime? Updated_On { get; set; }
    }
}

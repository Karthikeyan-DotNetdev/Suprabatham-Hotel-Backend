namespace laptop_service.Models.AppSettings
{
    public class KOTPrinterSetting
    {
        public string Branch_Code { get; set; } = string.Empty;
        public string Kitchen_Code { get; set; } = string.Empty;
        public string Kitchen_Name { get; set; } = string.Empty;
        public string Printer_Width { get; set; } = "576";
        public string? Printer_Interface { get; set; } = "Ethernet";
        public string? Interface_Detail { get; set; }
        public string Start_Feed_Length { get; set; } = "0";
        public string End_Feed_Length { get; set; } = "0";
        public string Start_Command { get; set; } = "None";
        public string End_Command { get; set; } = "None";
        public string Print_As_Image { get; set; } = "1";
        public string Image_Print_Command { get; set; } = "None";
        public string Cash_Drawer_Command { get; set; } = "None";
        public string? Test_Print_Text { get; set; } = string.Empty;
        public int No_Of_Print { get; set; } = 1;
        public string? Created_On { get; set; }
        public string? Updated_On { get; set; }
        public string? Updated_By { get; set; }
    }
}

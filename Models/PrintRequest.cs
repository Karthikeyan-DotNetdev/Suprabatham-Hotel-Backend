namespace laptop_service.Models
{
    public class PrintRequest
    {
        public int? No_Of_Print { get; set; }
        public string? Data_To_Print { get; set; }
        public string? Branch_Code { get; set; }
        public string? Till_Code { get; set; }
        public string? Printer_Width { get; set; }
        public string? Printer_Interface { get; set; }
        public string? Interface_Detail { get; set; }
        public object? Start_Feed_Length { get; set; }
        public object? End_Feed_Length { get; set; }
        public string? Start_Command { get; set; }
        public string? End_Command { get; set; }
        public object? Print_As_Image { get; set; }
        public string? Image_Print_Command { get; set; } = string.Empty;
        public string? Cash_Drawer_Command { get; set; } = string.Empty;
        public string? Test_Print_Text { get; set; } = string.Empty;
        public string? Local_App_Url { get; set; } = string.Empty;
    }
}

namespace laptop_service.Models.MASTERS
{
    public class PRINTER_MASTER
    {
        public string? Printer_Code { get; set; }
        public string? Printer_Name { get; set; }
        public string? Printer_Interface { get; set; } // 'Ethernet', 'Printer Driver', 'Bluetooth'
        public string? IP_Address { get; set; }
        public int? Port_Number { get; set; }
        public string? Bluetooth_Name { get; set; }
        public string? Bluetooth_Address { get; set; }
        public string? Paper_Size { get; set; } // '80MM', '58MM'
        public string? Is_Active { get; set; } // 'A' (Active), 'D' (Deactive)
        public string? Created_By { get; set; }
        public string? Created_On { get; set; }
        public string? Updated_By { get; set; }
        public string? Updated_On { get; set; }
    }
}

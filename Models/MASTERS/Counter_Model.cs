namespace laptop_service.Models.MASTERS
{
    public class COUNTER_MASTER
    {
        public long Counter_Id { get; set; }

        public string? Counter_Code { get; set; }

        public string? Counter_Name { get; set; }

        public string? Branch_Code { get; set; }

        public string? Counter_Type { get; set; }

        public string? Location_Name { get; set; }

        public string? Printer_Name { get; set; }

        public string? Printer_IP { get; set; }

        public string? Device_Name { get; set; }

        public string? Device_Id { get; set; }

        public string? Is_Default { get; set; } = "N";

        public string? Is_Active { get; set; } = "A";

        public string? Remarks { get; set; }

        public string? Created_By { get; set; }

        public string? Created_On { get; set; }

        public string? Updated_By { get; set; }

        public string? Updated_On { get; set; }
    }
}
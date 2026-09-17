namespace laptop_service.Models.MASTERS
{
    public class BarcodeUpdateItem
    {
        public string Product_Code { get; set; } = string.Empty;
        public string? Short_Name { get; set; }
        public string? Barcode { get; set; }
    }

    public class BulkBarcodeUpdateRequest
    {
        public List<BarcodeUpdateItem> Items { get; set; } = new List<BarcodeUpdateItem>();
        public string UpdatedBy { get; set; } = "admin";
    }
}

namespace laptop_service.Models.MASTERS
{
    public class RateUpdateItem
    {
        public string? Branch_Code { get; set; }
        public string? Product_Code { get; set; }
        public string? Area_Type { get; set; } = "ALL";
        public decimal Selling_Price { get; set; }
    }

    public class BulkRateUpdateRequest
    {
        public List<RateUpdateItem>? Rates { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class ResetRateRequest
    {
        public string? Branch_Code { get; set; }
        public List<string>? Product_Codes { get; set; }
        public string? Area_Type { get; set; } = "ALL_AREAS";
        public string? Updated_By { get; set; }
    }
}

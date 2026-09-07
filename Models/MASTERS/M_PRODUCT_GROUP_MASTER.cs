namespace laptop_service.Models.MASTERS
{
    public class M_PRODUCT_GROUP_MASTER
    {
        public string? Group_Code { get; set; }
        public string? Group_Name { get; set; }
        public string? Category_Code { get; set; }
        public string? Category_Name { get; set; }
        public int? Display_Order { get; set; } = 0;
        public string? Is_Active { get; set; } = "A";
        public string? Created_By { get; set; }
        public string? Created_On { get; set; }
        public string? Updated_By { get; set; }
        public string? Updated_On { get; set; }
    }
}

namespace laptop_service.Models.MASTERS
{
    public class M_PRODUCT_MASTER
    {
        public int? Product_Id { get; set; }

        public string? Product_Code { get; set; }

        public string? Product_Name { get; set; }

        public string? Short_Name { get; set; }

        public string? Product_Description { get; set; }

        public string? Barcode { get; set; }

        public string? Category_Code { get; set; }

        public string? UOM_Code { get; set; }

        public string? HSN_Code { get; set; }

        public string? GST_Code { get; set; }

        public string? Variant_Code { get; set; }

        public string? Kitchen_Code { get; set; }

        public decimal? MRP_Price { get; set; }

        public decimal? Cost_Price { get; set; }

        public decimal? Selling_Price { get; set; }

        public decimal? Takeaway_Price { get; set; }

        public decimal? Zomato_Price { get; set; }

        public decimal? Swiggy_Price { get; set; }

        public string? Is_Taxable { get; set; }

        public string? Tax_Inclusive { get; set; }

        public string? Is_Stock_Item { get; set; }

        public string? Is_Available { get; set; }

        public int? Display_Order { get; set; }

        public string? Is_Active { get; set; }

        public string? Is_Favorite { get; set; }

        public string? Created_By { get; set; }

        public string? Created_On { get; set; }

        public string? Updated_By { get; set; }

        public string? Updated_On { get; set; }
    }
}
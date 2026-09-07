namespace laptop_service.Models.AppSettings
{
    public class ReceiptSetting
    {
        public string Branch_Code { get; set; } = string.Empty;
        public string Till_Code { get; set; } = string.Empty;
        public string? Logo_Image { get; set; }
        public string Logo_Align { get; set; } = "Center";
        public string Print_Language { get; set; } = "English";
        public string? Print_Header_Text { get; set; }
        public string? Print_Footer_Text { get; set; }
        
        public string Print_Logo { get; set; } = "1";
        public string Print_Company_Name { get; set; } = "1";
        public string Print_Address_Detail { get; set; } = "0";
        public string Print_Address_Line1 { get; set; } = "1";
        public string Print_Address_Line2 { get; set; } = "1";
        public string Print_City { get; set; } = "1";
        public string Print_Tax_Detail { get; set; } = "1";
        public string Print_Contact_Detail { get; set; } = "1";
        public string Print_Phone { get; set; } = "1";
        public string Print_Email { get; set; } = "1";
        public string Print_Customer_Detail { get; set; } = "1";
        public string Print_Sale_Order_Type { get; set; } = "1";
        public string Print_DateWithTime { get; set; } = "1";
        public string Print_Till_Detail { get; set; } = "1";
        public string Print_Table_Detail { get; set; } = "0";
        public string Print_KOT_Number { get; set; } = "0";
        public string Print_Captain_Detail { get; set; } = "0";
        public string Print_Cashier_Detail { get; set; } = "1";
        public string Print_Short_BillNo { get; set; } = "0";
        public string Print_Total_ItemsQty { get; set; } = "1";
        public string Print_Addon_Detail { get; set; } = "0";
        public string Print_FSS_Detail { get; set; } = "0";
        public string Print_Wide_Product_Name { get; set; } = "0";
        public string Print_Product_Name_Wrapping { get; set; } = "1";
        public string Print_Product_Code { get; set; } = "0";
        public string Print_SKU_Code { get; set; } = "0";
        public string Print_HSN_SAC_Code { get; set; } = "0";
        public string Print_Tax_Column { get; set; } = "1";
        public string Print_Line_Item_Discount { get; set; } = "0";
        public string Print_Total_Savings { get; set; } = "0";
        public string Print_Tax_Summary { get; set; } = "1";
        public string Print_Payment_Summary { get; set; } = "1";
        public string Print_Terms_Conditions { get; set; } = "0";
        public string Print_Customer_Outstanding { get; set; } = "0";
        public string Print_Customer_Loyalty { get; set; } = "0";
        public string Print_Bar_Code { get; set; } = "0";
        public string Print_OnTime { get; set; } = "0";
        public string Print_Token_Number { get; set; } = "0";
    }
}

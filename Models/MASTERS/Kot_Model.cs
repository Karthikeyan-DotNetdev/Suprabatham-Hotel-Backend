using System.ComponentModel.DataAnnotations;

namespace laptop_service.Models.MASTERS
{
    public class KOT_SAVE_REQUEST
    {
        public long KOT_Id { get; set; }

        public string? KOT_No { get; set; }

        public string? Branch_Code { get; set; }

        public string? Table_Code { get; set; }

        public int Guest_Count { get; set; } = 1;

        public string? Customer_Code { get; set; }

        public string? Customer_Name { get; set; }

        public string? Mobile_No { get; set; }

        public string? Order_Type { get; set; } = "DINE_IN";

        public string? Remarks { get; set; }

        public string? Direct_Bill { get; set; } = "N";

        public string? Created_By { get; set; }

        public List<KOT_DETAIL_REQUEST> Items { get; set; } = new();
    }

    public class KOT_DETAIL_REQUEST
    {
        public string? Product_Code { get; set; }

        public string? Product_Name { get; set; }

        public string? Category_Code { get; set; }

        public string? Variant_Code { get; set; }

        public string? Variant_Name { get; set; }

        public string? Kitchen_Code { get; set; }

        public string? Cooking_Code { get; set; }

        public string? Cooking_Notes { get; set; }

        public decimal Qty { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }
    }

    public class KOT_REORDER_REQUEST
    {
        public long KOT_Id { get; set; }

        public string? KOT_No { get; set; }

        public string? Updated_By { get; set; }

        public List<KOT_DETAIL_REQUEST> Items { get; set; } = new();
    }

    public class KOT_STATUS_REQUEST
    {
        public long KOT_Id { get; set; }

        public string? KOT_No { get; set; }

        public string? Updated_By { get; set; }
    }

    public class KOT_ITEM_STATUS_REQUEST
    {
        public long KOT_Detail_Id { get; set; }

        public string? Item_Status { get; set; }

        public string? Updated_By { get; set; }
    }
    public class BILL_SAVE_REQUEST
    {
        public long KOT_Id { get; set; }
        public string? Order_Type { get; set; } = "DINE_IN"; // "DINE_IN" | "TAKE_AWAY" | "DIRECT_BILL" | "DELIVERY"
        public string? Branch_Code { get; set; }
        public string? Customer_Code { get; set; }
        public string? Customer_Name { get; set; }
        public string? Mobile_No { get; set; }
        public decimal Sub_Total { get; set; }
        public decimal Discount_Percentage { get; set; }
        public decimal Discount_Amount { get; set; }
        public decimal Tax_Amount { get; set; }
        public decimal Round_Off { get; set; }
        public decimal Net_Amount { get; set; }
        public decimal Paid_Amount { get; set; }
        public string? Payment_Mode { get; set; }
        public string? Payment_Reference { get; set; }
        public string? Remarks { get; set; }
        public string Created_By { get; set; } = string.Empty;
        // Cart items for Direct Bill / Takeaway
        public List<BILL_ITEM_REQUEST>? Items { get; set; }
    }
    public class BILL_ITEM_REQUEST
    {
        public string Product_Code { get; set; } = string.Empty;
        public string Product_Name { get; set; } = string.Empty;
        public string? Category_Code { get; set; }
        public string? Variant_Code { get; set; }
        public string? Variant_Name { get; set; }
        public string? Kitchen_Code { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }

}
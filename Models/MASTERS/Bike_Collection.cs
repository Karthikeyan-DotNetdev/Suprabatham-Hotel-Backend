
namespace laptop_service.Models.MASTERS
{
    public class M_BIKE_COLLECTION_SETTLEMENT
    {
        public int Shift_Id { get; set; }
        public string Shift_Name { get; set; }

        public DateTime InDate { get; set; }
        public DateTime? OutDate { get; set; }

        public decimal Actual_Cash_Bill { get; set; }
        public decimal Actual_Gpay_Bill { get; set; }
        public decimal Actual_Credit_Bill { get; set; }

        public decimal Cash_Short_Fall { get; set; }
        public decimal Gpay_Short_Fall { get; set; }

        public decimal Actual_Gpay_Report_In_App { get; set; }

        public decimal Final_Cash { get; set; }
        public decimal Final_Gpay { get; set; }

        public bool Is_Active { get; set; }

        public List<M_BIKE_DENOMINATION>? Denominations { get; set; }
    }
    public class M_BIKE_DENOMINATION
    {
        public decimal Rate { get; set; }
        public decimal Count { get; set; }
        public decimal Total { get; set; }
    }
}
namespace ERP_API.Moduls
{
    public class PurchaseorderReadOnly
    {
        public int POId { get; set; }

        public int? VendId { get; set; }

        public string? Pono { get; set; }

        public DateTime? Podate { get; set; }


        public List<PurchaseorderSubReadOnly> PurchaseorderSubs { get; set; }
    }
}

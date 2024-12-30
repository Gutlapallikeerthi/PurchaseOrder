using System;
using System.Collections.Generic;

namespace ERP_API.Data;

public partial class Purchaseorder
{
    public int POId { get; set; }

    public int? VendId { get; set; }

    public string? Pono { get; set; }

    public DateTime? Podate { get; set; }
    public ICollection<PurchaseorderSub> PurchaseorderSubs { get; set; } = new List<PurchaseorderSub>();
}

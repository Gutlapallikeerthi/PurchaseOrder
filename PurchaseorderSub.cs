using System;
using System.Collections.Generic;

namespace ERP_API.Data;

public partial class PurchaseorderSub
{
    public int? POSubId { get; set; }
    public int? POId { get; set; }

    public int? SlNo { get; set; }

    public int? ItemId { get; set; }
    public float? Qty { get; set; }

   
    public Purchaseorder Purchaseorder { get; set; }
}

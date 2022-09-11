using System;
using System.Collections.Generic;

namespace eStoreApi.Model
{
    public partial class TblPurchasehistory
    {
        public string PurchaseId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? PromoCodes { get; set; }
        public byte[]? Qrimage { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? EvoucherId { get; set; }
        public bool? IsUsed { get; set; }
        public double PromoAmount { get; set; }
    }
}

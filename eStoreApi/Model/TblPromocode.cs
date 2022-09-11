using System;
using System.Collections.Generic;

namespace eStoreApi.Model
{
    public partial class TblPromocode
    {
        public string Id { get; set; } = null!;
        public byte[]? Qr { get; set; }
        public string? PromoCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? Active { get; set; }
    }
}

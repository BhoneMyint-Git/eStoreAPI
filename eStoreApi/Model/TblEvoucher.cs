using System;
using System.Collections.Generic;

namespace eStoreApi.Model
{
    public partial class TblEvoucher
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public byte[]? Qrimage { get; set; }
        public double Amount { get; set; }
        public double? PaymentDiscount { get; set; }
        public bool IsGift { get; set; }
        public string Phone { get; set; } = null!;
        public bool Active { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? MidifiedDate { get; set; }
        public string? PaymentType { get; set; }
        public string? VoucherCodes { get; set; }
        public bool? IsUsed { get; set; }
        public string? Title { get; set; }
        public double Price { get; set; }
    }
}

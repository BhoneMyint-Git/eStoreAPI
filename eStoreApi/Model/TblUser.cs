using System;
using System.Collections.Generic;

namespace eStoreApi.Model
{
    public partial class TblUser
    {
        public string UserId { get; set; } = null!;
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public int? MaximumVoucher { get; set; }
        public int? MaximumGift { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? MidifiedDate { get; set; }
    }
}

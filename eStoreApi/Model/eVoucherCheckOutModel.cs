using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace eStoreApi.Model
{
    public class eVoucherCheckOutModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public bool IsGift { get; set; } = false;
        public string PromoCode { get; set; } = "";
        [Required]
        public string CreditCard { get; set; }
        
    }
}

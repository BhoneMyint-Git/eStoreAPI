using Newtonsoft.Json;

namespace eStoreApi.Model
{
    public class eVoucherGetModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}

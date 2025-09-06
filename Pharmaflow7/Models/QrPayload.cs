using System.Text.Json.Serialization;

namespace Pharmaflow7.Helpers
{
    public class QrPayload
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ProductionDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class QrVerifyRequest
    {

        [JsonPropertyName("qrData")]
        public string QrData { get; set; } = string.Empty;
        public string RawData { get; set; }
    }
}

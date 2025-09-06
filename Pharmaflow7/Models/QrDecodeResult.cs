using Pharmaflow7.Helpers;

namespace Pharmaflow7.Models
{
    public class QrDecodeResult
    {
        public QrPayload Payload { get; set; }
        public bool IsValid { get; set; }
    }
}

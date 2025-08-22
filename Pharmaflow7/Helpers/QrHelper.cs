using System.Security.Cryptography;
using System.Text;

namespace Pharmaflow7.Helpers
{
    public static class QrHelper
    {
        private static string secretKey = "PharmaFlowSecretKey";

        public static string GenerateSignature(string rawData)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(hash);
        }

        public static string GenerateQrData(QrPayload product)
        {
            var rawData = $"{product.Id}|{product.Name}|{product.ProductionDate}|{product.ExpirationDate}|{product.Description}";
            product.Signature = GenerateSignature(rawData);
            return $"{rawData}|{product.Signature}"; // pipe-delimited
        }
    }
}

using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Drawing;
using System;
using System.IO;
 
using ZXing;
using ZXing.Windows.Compatibility;

namespace Pharmaflow7.Helpers
{
    public static class QrHelper
    {
        private static readonly string SecretKey = "PharmaFlowSecretKey123!"; 

        public static string GenerateQrData(QrPayload payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            payload.Signature = GenerateHash(json);
            return JsonConvert.SerializeObject(payload);
        }

        public static bool VerifyQrData(string qrData)
        {
            var payload = JsonConvert.DeserializeObject<QrPayload>(qrData);
            if (payload == null) return false;

            string signature = payload.Signature;
            payload.Signature = string.Empty;
            string expectedHash = GenerateHash(JsonConvert.SerializeObject(payload));

            return expectedHash == signature;
        }

        private static string GenerateHash(string input)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static string ReadQrFromImage(byte[] imageBytes)
        {
            using var ms = new MemoryStream(imageBytes);
            using var bitmap = new Bitmap(ms);
            var reader = new BarcodeReader();
            var result = reader.Decode(bitmap);
            return result?.Text;
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Pharmaflow7.Helpers;

namespace Pharmaflow7.Controllers
{
    [Route("api/qr")]
    [ApiController]
    public class QrController : ControllerBase
    {
        [HttpPost("verify")]
        public IActionResult Verify([FromBody] QrVerifyRequest request)
        {
            if (request?.RawData == null)
                return BadRequest(new { success = false, message = "Missing QR data" });

            try
            {
                var parts = request.RawData.Split('|');
                if (parts.Length < 6)
                    return BadRequest(new { success = false, message = "Invalid QR format" });

                var payload = new QrPayload
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1],
                    ProductionDate = DateTime.Parse(parts[2]),
                    ExpirationDate = DateTime.Parse(parts[3]),
                    Description = parts[4],
                    Signature = parts[5]
                };

                var rawData = $"{payload.Id}|{payload.Name}|{payload.ProductionDate}|{payload.ExpirationDate}|{payload.Description}";
                bool isValid = payload.Signature == QrHelper.GenerateSignature(rawData);

                return Ok(new { success = true, isValid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "QR parse error", error = ex.Message });
            }
        }
    }
}

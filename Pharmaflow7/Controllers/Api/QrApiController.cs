using Microsoft.AspNetCore.Mvc;
using Pharmaflow7.Helpers;

namespace Pharmaflow7.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrApiController : ControllerBase
    {
        [HttpPost("verify")]
        public IActionResult Verify([FromBody] QrVerifyRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.QrData))
            {
                return BadRequest(new { success = false, message = "QR data is required" });
            }

            bool isValid = QrHelper.VerifyQrData(request.QrData);

            return Ok(new { success = isValid, message = isValid ? "Product is genuine" : "Product is fake" });
        }
    }

    public class QrVerifyRequest
    {
        public string QrData { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pharmaflow7.Helpers;
public class VerifyController : Controller
{
    // GET /Verify/Scan
    [HttpGet]
    public IActionResult Scan()
    {
        return View(); // ترجع Scan.cshtml
    }

    [HttpPost]
    public async Task<IActionResult> ScanFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded" });

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // استخدم مكتبة jsQR أو ZXing.NET على الـ server لتحليل الصورة
            var qrData = QrHelper.ReadQrFromImage(bytes); // حتكتب الدالة دي

            if (string.IsNullOrEmpty(qrData))
                return Json(new { success = false, message = "QR not found" });

            var payload = QrHelper.VerifyQrData(qrData);
            bool isGenuine = payload != null;

            return Json(new { success = isGenuine });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error processing file: " + ex.Message });
        }
    }

}


using Microsoft.AspNetCore.Mvc;

namespace Pharmaflow7.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("~/Views/Shared/NotFound.cshtml");
                default:
                    return View("~/Views/Shared/Error.cshtml");
            }
        }
    }
}
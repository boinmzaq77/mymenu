using Microsoft.AspNetCore.Mvc;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class ReceiptController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

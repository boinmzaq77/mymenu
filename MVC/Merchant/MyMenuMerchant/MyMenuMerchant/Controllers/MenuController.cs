using Microsoft.AspNetCore.Mvc;

namespace MyMenuMerchant.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

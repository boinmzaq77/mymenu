using Microsoft.AspNetCore.Mvc;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

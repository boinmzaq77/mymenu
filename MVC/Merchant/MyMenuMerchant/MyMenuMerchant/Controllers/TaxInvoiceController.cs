using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class TaxInvoiceController : Controller
    {
        // GET: TaxInvoiceController
        public ActionResult Index()
        {
            return View();
        }

        // GET: TaxInvoiceController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TaxInvoiceController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TaxInvoiceController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TaxInvoiceController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TaxInvoiceController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TaxInvoiceController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TaxInvoiceController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

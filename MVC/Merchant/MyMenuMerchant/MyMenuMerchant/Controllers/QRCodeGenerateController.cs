using Microsoft.AspNetCore.Mvc;
using MyMenu.JAM;
using System.Globalization;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class QRCodeGenerateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public List<QRCodeindex> GetQRCodeData() 
        {
            List<QRCodeindex> lstqrcode = new List<QRCodeindex>();
            try
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                var timezoneslocal = TimeZoneInfo.Local;
                QRCodeindex qR ;
                for (int i = 0; i < 20; i++)
                {
                    qR = new QRCodeindex()
                    {
                        Date = DateTime.Today.ToString("dd/MM/yyyy", new CultureInfo("en-US")),  
                        Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddMinutes(13), timezoneslocal).ToString("HH:mm tt", new CultureInfo("en-US")),
                        TypeOrder = (i % 3 == 0 ? "ทานที่ร้าน" : "สั่งกลับบ้าน"),
                        FoodTable = (i % 3 == 0 ? "A"+ i : "-"),
                        QRCodeImage = ""
                    };
                    lstqrcode.Add(qR);
                }

                return lstqrcode;
            }
            catch (Exception ex)
            {
                return lstqrcode;
            }
        }

        public IActionResult QRCodeGenerateAdd()
        {
            return View();
        }

    }
}

using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyMenuMerchant.Models;
using MyMenuMerchant.Utills;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MyMenuMerchant.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        private readonly string role;

        public HomeController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AllNotification()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<string> GetNoti()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    string url = MicroServiceName.MyMenuAPI + "Notification/Notification?BranchID=" + branchID ;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    Client.SetBearerToken(Jwt);
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<string> GetNotiLoop(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                date = "0";
            }
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    

                    string url = MicroServiceName.MyMenuAPI + "Notification/Notification?BranchID=" + branchID + "&ticksNumber=" + date;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    Client.SetBearerToken(Jwt);
                    var result = await Client.GetAsync(url);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(await result.Content.ReadAsStringAsync());
                        }
                    }

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return json_Catagory;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
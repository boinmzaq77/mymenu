using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using MyMenu.ORM.Period;
using MyMenuMerchant.Models;
using MyMenuMerchant.Utills;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Image = System.Drawing.Image;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class ReceiptHistoryController : Controller
    {
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        private readonly string role;

        public ReceiptHistoryController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }
        // GET: ReceiptHistoryController

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TAXInvoiceHistory()
        {
            return View();
        }

        public async Task<ActionResult> ReceiptInformation(string tranHistoryModel)
        {
            try
            {
                ViewBag.role = role;
                if (string.IsNullOrEmpty(tranHistoryModel))
                {
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "ReceiptHistory", action = "Index" }));
                }

                TranHistoryModel tranHistoryModels = new TranHistoryModel();
                tranHistoryModels = JsonConvert.DeserializeObject<TranHistoryModel>(tranHistoryModel);
                var tran = await GetHistoryDetail(tranHistoryModels);
                return View(tran);
            }
            catch (Exception)
            {
                TranModel tran = new TranModel();
                return View(tran);
            }
        }
        public async Task<ActionResult> GenerateTaxInvoices(string tranDetailModel)
        {
            try
            {
                ViewBag.role = role;
                if (string.IsNullOrEmpty(tranDetailModel))
                {
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "ReceiptHistory", action = "Index" }));
                }
               

                TranDetailModel tranDetail = new TranDetailModel();
                tranDetail = JsonConvert.DeserializeObject<TranDetailModel>(tranDetailModel);
                return View(tranDetail);
            }
            catch (Exception)
            {
                TranDetailModel tran = new TranDetailModel();
                return View(tran);
            }
        }
        public async Task<ActionResult> Receipts(string tranDetailModel, string page)
        {
            try
            {
                TranDetailModel tranDetail;
                ViewBag.role = role;
                ViewBag.page = page;
                if (string.IsNullOrEmpty(tranDetailModel))
                {
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "ReceiptHistory", action = "Index" }));
                }

                if (page == "GenerateTaxInvoice")
                {
                    tranDetail = new TranDetailModel();
                    tranDetail = JsonConvert.DeserializeObject<TranDetailModel>(tranDetailModel);
                }
                else
                {
                    TranHistoryModel tranHistoryModels = new TranHistoryModel();
                    tranHistoryModels = JsonConvert.DeserializeObject<TranHistoryModel>(tranDetailModel);
                    tranDetail = await GetHistoryDetail(tranHistoryModels);
                }
                return View(tranDetail);
            }
            catch (Exception)
            {
                return View();
            }
        }

        public async Task<string> GetHistory(string offset)
        {
            try
            {
                if (string.IsNullOrEmpty(offset))
                {
                    offset = "0";
                }
                var branchID = _cookieService.GetBranch();
                char TranType = 'B';
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Tran/HistoryTran?BranchID="+ branchID + "&offset=" + offset + "&TranType=" + TranType;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
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

        public async Task<string> GetTAXInvoiceHistory(string offset)
        {
            try
            {
                if (string.IsNullOrEmpty(offset))
                {
                    offset = "0";
                }
                var branchID = _cookieService.GetBranch();
                char TranType = 'T';
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Tran/HistoryTran?BranchID=" + branchID + "&offset=" + offset + "&TranType=" + TranType;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
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

        public async Task<TranDetailModel> GetHistoryDetail(TranHistoryModel tranHistoryModel)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Tran/TranDetailModel?BranchID=" + tranHistoryModel.BranchID + "&TranNo=" + tranHistoryModel.TranNo;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
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
                            return JsonConvert.DeserializeObject<TranDetailModel>(json_Catagory);
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

        [HttpPost]
        public async Task<ActionResult> CreateTAXInvoice(string trandetailmodel)
        {
            try
            {
                var tranModel = JsonConvert.DeserializeObject<TranDetailModel>(trandetailmodel);

                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Tran/TAXInvoice";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var json = JsonConvert.SerializeObject(tranModel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            return Conflict("ร้านค้ากรอกข้อมูล TAX ไม่ครบ");
                        }
                        else
                        {
                            return Conflict(content);
                        }
                    }
                    else
                    {
                        return Ok(content);
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> PutPrintCounter(string BranchID, string Tranno)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Tran/PrintCounter?BranchID=" + BranchID + "&Tranno=" + Tranno;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var json = JsonConvert.SerializeObject("", settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PutAsync(url, httpContent);
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
                            return Ok(json_Catagory);
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public static string ConvertDatetimeLocal(DateTime d)
        {
            try
            {
                Console.WriteLine(d.ToString());
                var timezoneslocal = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                string datetime = TimeZoneInfo.ConvertTimeFromUtc(d, timezoneslocal).ToString("dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"));
                return datetime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }

        public static string ConverttimeLocal(DateTime d)
        {
            try
            {
                Console.WriteLine(d.ToString());
                var timezoneslocal = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                string datetime = TimeZoneInfo.ConvertTimeFromUtc(d, timezoneslocal).ToString("HH:mm:ss", new CultureInfo("en-US"));
                return datetime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }
        public static string Getonlydate(string d)
        {
            try
            {
                if (!(d.Contains(" "))) return "";
                var arr = d.Split(" ");
                return arr[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }
        public static string Getonlytime(string d)
        {
            try
            {
                if (!(d.Contains(" "))) return "";
                var arr = d.Split(" ");
                return arr[1];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }
        public static string ConvertDatetimeLocalReceiptinformatuion(DateTime d)
        {
            try
            {
                var year = "";
                var month = "";
                var day = "";
                var hour = "";
                var minute = "" ;
                var second = "";

                var timezoneslocal = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                d = TimeZoneInfo.ConvertTimeFromUtc(d, timezoneslocal);

                if (d.Day.ToString().Length < 2)
                {
                    day = "0" + d.Day.ToString();
                }
                else
                {
                    day =  d.Day.ToString();
                }

                if (d.Month.ToString().Length < 2)
                {
                    month = "0" + d.Month.ToString();
                }
                else
                {
                    month = d.Month.ToString();
                }

                if (d.Year.ToString().Length < 2)
                {
                    year = '0' + d.Year.ToString();
                }
                else
                {
                    year = d.Year.ToString();
                }  

                if (d.Hour.ToString().Length < 2)
                {
                    hour = '0' + d.Hour.ToString();
                }
                else
                {
                    hour = d.Hour.ToString();
                }

                if (d.Minute.ToString().Length < 2)
                {
                    minute = '0' + d.Minute.ToString();
                }
                else
                {
                    minute = d.Minute.ToString();
                }

                if (d.Second.ToString().Length < 2)
                {
                    second = '0' + d.Second.ToString();
                }
                else
                {
                    second = d.Second.ToString();
                }

                var formatdate = day + '/' + month + '/' + year + ' ' + hour + ':' + minute + ':' + second;
                return formatdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }

    }
}

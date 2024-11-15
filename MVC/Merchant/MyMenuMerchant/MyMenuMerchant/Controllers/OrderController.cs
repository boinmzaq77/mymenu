using IdentityModel.Client;
using LinqToDB.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using MyMenuMerchant.Models;
using MyMenuMerchant.Utills;
using Newtonsoft.Json;
using NuGet.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MyMenuMerchant.Controllers
{
    public class OrderController : Controller
    {
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        private readonly string role;

        public OrderController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }
        public async Task<IActionResult> Index()
        {
            var detail = await GetOrderMain();
            return View(detail);
        }
        public async Task<IActionResult> OrderDetail(string OpenOrdersID, string OrdersID)
        {
            var detail = await GetOrderdetail(OpenOrdersID, OrdersID);
            ViewBag.role = role;
            return View(detail);
        }
        public async Task<IActionResult> AllOrders()
        {
            ViewBag.role = role;
            return View();
        }
        public async Task<IActionResult> OrderRegister()
        {
            ViewBag.role = role;
            var data = await GetDataPrints();
            var modelprint = JsonConvert.DeserializeObject<List<PrinterModel>>(data);
            return View(modelprint);
        }
        public async Task<string> GetDataPrints()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Printer" + "?BranchID=" + branchID;
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
        public async Task Print()
        {
            List<string> test = new List<string>() { "192.168.200.32" };
            foreach (var testItem in test)
            {
                SettingPrinter settingPrinter = new SettingPrinter() { IPADDRESS = testItem, PORTNUMBER = "9100" };
                PrintManager printManager = new PrintManager(settingPrinter);
                await printManager.Open();
                byte[] bytes = Encoding.ASCII.GetBytes("123456789");
                //var arr = await OrderKitchen(1, new OrderWithPrintModel());
                //foreach (var item in arr) 
                //{
                //    await printManager.Write(Encoding.ASCII.GetBytes(item), 1000);
                //}
                byte[] bytes2 = Encoding.ASCII.GetBytes("\n\n");
                await printManager.Write(bytes2, 1000);
                await printManager.Close();
            }
            return;
        }


        //public async Task<List<string>> OrderKitchen(int size, OrderWithPrintModel orderDetailWithPrints)
        public async Task<List<string>> OrderKitchen(int size, List<OrderWithPrintModel> orderDetailWithPrints , PrinterModel printer)
        {
           
            List<string> data = new List<string>();

            foreach (var orderKitchen in orderDetailWithPrints)
            {
               
                PrintManager PM = new PrintManager(size);
                int[] itemLength = { 28, 40 };
                if (orderKitchen.OpenOrdersType == 'T')
                {
                    data.Add(orderKitchen.CustomerName + orderKitchen.CustomerMobile);
                }
                else
                {
                    data.Add(orderKitchen.FoodTableName);
                }
                data.Add(PM.ReplaceSpacebar2(PrintManager.PrintDateTime(orderKitchen.DateModified), orderKitchen.OpenOrdersType == 'D' ? "ทานที่ร้าน" : " สั่งกลับบ้าน"));
                data.Add(PM.ReplaceSpacebar2("Order No. " + orderKitchen.OrdersID, printer.PrinterName));
                data.Add("-------------------------------------------".Substring(0, itemLength[size - 1]));
                foreach (var orderDetail in orderKitchen.OrderDetailModel)
                {
                    string res = "";
                    string res2 = "";
                    string res3 = "";
                    string space = "";
                    var count = 4 - (int)orderDetail.Quantity.ToString("###0").Length;
                    var itemNames = PrintManager.SplitItemName(itemLength[size - 1], orderDetail.ItemName);
                    switch (count)
                    {
                        case 3:
                            res = orderDetail.Quantity.ToString("###0") + "   " + itemNames[0];
                            if (!string.IsNullOrEmpty(itemNames[1]))
                            {
                                res2 = "   " + itemNames[1];
                            }
                            if (!string.IsNullOrEmpty(itemNames[2]))
                            {
                                res3 = "   " + itemNames[2];
                            }
                            space = "   ";
                            break;
                        case 2:
                            res = orderDetail.Quantity.ToString("###0") + "  " + itemNames[0];
                            if (!string.IsNullOrEmpty(itemNames[1]))
                            {
                                res2 = "  " + itemNames[1];
                            }
                            if (!string.IsNullOrEmpty(itemNames[2]))
                            {
                                res3 = "  " + itemNames[2];
                            }
                            space = "  ";
                            break;
                        case 1:
                            res = orderDetail.Quantity.ToString("###0") + " " + itemNames[0];
                            if (!string.IsNullOrEmpty(itemNames[1]))
                            {
                                res2 = " " + itemNames[1];
                            }
                            if (!string.IsNullOrEmpty(itemNames[2]))
                            {
                                res3 = " " + itemNames[2];
                            }
                            space = " ";
                            break;
                    }
                    if (res.Length > itemLength[size - 1])
                    {
                        res = res.Substring(0, itemLength[size - 1]);
                    }
                    data.Add(res);
                    if (!string.IsNullOrEmpty(res2))
                    {
                        data.Add("  " + res2);
                    }
                    if (!string.IsNullOrEmpty(res3))
                    {
                        data.Add("  " + res3);
                    }

                    foreach (var optionExtra in orderDetail.OptionExtra.OrderByDescending(x=> x.Type))
                    {
                        if (optionExtra.Type == 'O')
                        {
                            data.Add("  " + space + optionExtra.OptionExtraDetailName);

                        }
                        else
                        {
                            data.Add("  " + space + "+" + optionExtra.OptionExtraDetailName);
                        }
                    }

                    if (!string.IsNullOrEmpty(orderDetail.Comments))
                    {
                        data.Add("  " + space + orderDetail.Comments);
                    }


                }
                data.Add("-------------------------------------------".Substring(0, itemLength[size - 1]));

                data.Add("จำนวนรายการทั้งหมด :" + orderKitchen.OrderDetailModel.Sum(x => x.Quantity));
            }

            return data;

        }
        public async Task<string> GetOrder()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    
                    string url = MicroServiceName.MyMenuAPI + "Order?BranchID=" + branchID;//+ "&dateTime=" + "2023-09-21T06:41:58.633Z";//DateTime.UtcNow.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
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
        public async Task<string> GetOrderLoop(string date)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    DateTime dateTime = DateTime.Parse(date);
                    dateTime.AddSeconds(1);

                    IFormatProvider culture = new CultureInfo("en-US", true);
                    string url = MicroServiceName.MyMenuAPI + "Order?BranchID=" + branchID + "&dateTime=" + dateTime.ToString("yyyyMMddHHmmss", culture);//
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
        public async Task<string> GetOrderPrinter()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Order/OrderDetails?BranchID=" + branchID;//+ "&dateTime=" + "2023-09-21T06:41:58.633Z";//DateTime.UtcNow.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
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
        public async Task<OrderModel> GetOrderdetail(string OpenOrdersID, string OrdersID)
        {
            OrderModel orderModel;
            try
            {
                //var branchid = "1";
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var branchID = _cookieService.GetBranch();
                    var url = MicroServiceName.MyMenuAPI + "Order/OrderDetail?BranchID=" + branchID + "&OpenOrdersID=" + OpenOrdersID + "&OrdersID=" + OrdersID;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        orderModel = new OrderModel();
                        orderModel = JsonConvert.DeserializeObject<OrderModel>(contents);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(contents))
                        {
                            throw new WebException(contents);
                        }
                        else
                        {
                            string message = ((HttpStatusCode)res.StatusCode).ToString();
                            throw new WebException(message);
                        }
                    }
                }
                return orderModel;
            }
            catch (Exception)
            {
                return new OrderModel();
            }
        }
        public async Task<ManageOrderModel> GetOrderMain()
        {
            ManageOrderModel orderModel;
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    
                    var url = MicroServiceName.MyMenuAPI + "Order/ManageOrder?BranchID="+ branchID;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        orderModel = new ManageOrderModel();
                        orderModel = JsonConvert.DeserializeObject<ManageOrderModel>(contents);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(contents))
                        {
                            throw new WebException(contents);
                        }
                        else
                        {
                            string message = ((HttpStatusCode)res.StatusCode).ToString();
                            throw new WebException(message);
                        }
                    }
                }
                return orderModel;
            }
            catch (Exception)
            {
                return new ManageOrderModel();
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditStatusOrderdetail(OrderDetail orderDetail)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                var branchID = _cookieService.GetBranch();
                orderDetail.BranchID = branchID;
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(orderDetail, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string url = MicroServiceName.MyMenuAPI + "order/UpdateOrderDetail";
                    var result = await Client.PutAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditStatusOrder(Order order)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                order.BranchID = branchID;
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt); 
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(order, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string url = MicroServiceName.MyMenuAPI + "order/UpdateOrder";
                    var result = await Client.PutAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditOrderdetailquantity(OrderDetail orderdetail)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(orderdetail, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string url = MicroServiceName.MyMenuAPI + "Order/UpdateQuantity?BranchID="+ orderdetail.BranchID + "&OpenOrdersID="+ orderdetail.OpenOrdersID+ "&OrdersID="+ orderdetail.OrdersID+ "&OrderDetailNo="+orderdetail.OrderDetailNo+ "&Quantity="+orderdetail.Quantity;
                    var result = await Client.PutAsync(url, null);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok();
                    }
                }
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
    }
}

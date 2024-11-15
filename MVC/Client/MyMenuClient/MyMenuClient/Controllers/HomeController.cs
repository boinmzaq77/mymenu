using AutoMapper;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using MyMenuClient.Models;
using MyMenuClient.Utills;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using static LinqToDB.SqlQuery.SqlPredicate;
using static MyMenuClient.Models.SubListAddtoCartModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyMenuClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceHostName MicroServiceName;
        public string MyMenuAPI = "";
        public string Client = "/Client";

        public HomeController(ILogger<HomeController> logger, ICookieService cookieService, IOptions<MicroServiceHostName> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            MyMenuAPI = microservicename.Value.MyMenuAPI;
        }
        
        public async Task<IActionResult> Store(int merchantid, string branchid, string generatecode,char OpenOrdersType)
        {
            try
            {
                //เช็ค generatecode ว่าเป็นชุดเดิมไหม เพื่อเช็คว่าปิดโต๊ะไปหรือยัง
                var generatecodeCurrent = _cookieService.GetCookieValue("generatecode");
                if (generatecodeCurrent != null)
                {
                    _cookieService.SetCookie("generatecodeTemp", generatecodeCurrent, null);
                }

                var Store = await CheckTableStoreAsync(merchantid, branchid , generatecode);
                if (Store)
                {
                    var Jwt_CC = BlueidConnect.Get_MyMenuCC();
                    var SecretCode = await GetBotSecretAsync(merchantid, Jwt_CC);
                    if (SecretCode != null)
                    {
                        var Jwt = BlueidConnect.Get_MyMenuClient(merchantid.ToString(), SecretCode);
                        _cookieService.SetToken(Jwt.access_token, Jwt.refresh_token, Jwt.expires_in);
                        _cookieService.SetClient(merchantid.ToString(), branchid.ToString(), generatecode, null);
                        if (OpenOrdersType == 'T')
                        {
                            return RedirectToAction("OrderHome", new RouteValueDictionary(new { controller = "Home", action = "OrderHome" }));
                        }
                        return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Home", action = "Index" }));
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                //return Conflict(ex.Message);
                return RedirectToAction("ConnectionFailed", new RouteValueDictionary(new { controller = "Home", action = "ConnectionFailed" }));
            }
        }

        public async Task<bool> CheckTableStoreAsync(int merchantid, string branchid,string generatecode)
        {
            try
            {
                if (string.IsNullOrEmpty(generatecode) || merchantid == 0 || string.IsNullOrEmpty(branchid) )
                {
                    return false;
                }
                using (var httpClient = new HttpClient())
                {
                    var url = MicroServiceName.MyMenuAPI + Client + "/Verify?MerchantID={0}&branchid={1}";
                    httpClient.DefaultRequestHeaders.Add("GenerateCode", generatecode);
                    var res = await httpClient.GetAsync(string.Format(url, merchantid, branchid));
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(contents))
                        {
                            throw new Exception(contents);
                        }
                        else
                        {
                            string message = ((HttpStatusCode)res.StatusCode).ToString();
                            throw new Exception(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetBotSecretAsync(int MerchantID, string jwt)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(jwt);
                    var url = MicroServiceName.SeAuth2APIHost + "/BotAccount/Secret?merchantid={0}&botrole={1}";
                    var res = await httpClient.GetAsync(string.Format(url, MerchantID, "MyMenuClient"));
                    if (res.IsSuccessStatusCode)
                    {
                        var contents = await res.Content.ReadAsStringAsync();
                        return contents;
                    }
                    else
                    {
                        var contents = await res.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(contents))
                        {
                            throw new Exception(contents);
                        }
                        else
                        {
                            string message = ((HttpStatusCode)res.StatusCode).ToString();
                            throw new Exception(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [JWT]
        public async Task<IActionResult> Index(int focuscategoryID, int focussubcategoryID)
        {
            List<ClinetOrderModel> listCartOrderModels = new List<ClinetOrderModel>();
            HomeIndexModel homeIndex = new HomeIndexModel();
            List<MenuCategoryModel> lstmenuCategory = new List<MenuCategoryModel>();
            MenuCategoryModel menuCategory;
            string MerchantName = string.Empty;
            int FoodTableNo = 0;
            int MerchantID = 0;
            string BranchID = string.Empty;
            string FoodTableName = string.Empty;
            string CustomerName = string.Empty;
            string Customerphone = string.Empty;

            ClientModel getClientModel = new ClientModel();
            try
            {
                _cookieService.SetLanguage("Th");
                var data = _cookieService.GetClientAndCustomer();
                getClientModel = await GetHomeIndexDetail(data.branchid);

                if (getClientModel.Merchant != null)
                {
                    homeIndex.MerchantName = getClientModel.Merchant.Name;
                    homeIndex.MerchantSlogans = getClientModel.Merchant.Slogans;
                    homeIndex.MerchantLogo = getClientModel.Merchant.LogoPath;
                    homeIndex.MerchantBackgroundPath = getClientModel.Merchant.BackgroundPath;
                }

                homeIndex.FoodTableNo = getClientModel.FoodTableNo;
                homeIndex.FoodTableName = getClientModel.FoodTableName;
                homeIndex.OpenOrdersID = getClientModel.OpenOrdersID;
                homeIndex.OpeningHours = getClientModel?.branches?.OpeningHours;
                homeIndex.MerchantAddress = getClientModel?.branches?.Address1 + getClientModel?.branches?.Address2;
                homeIndex.CallStaff = getClientModel?.branches.CallStaff;

                foreach (var item in getClientModel.lstmenuCategory)
                {
                    menuCategory = new MenuCategoryModel()
                    {
                        CategoryID = item.CategoryID,
                        CategoryName = item.CategoryName
                    };
                    lstmenuCategory.Add(menuCategory);
                }
                homeIndex.lstmenuCategory.AddRange(lstmenuCategory);

                if (homeIndex.OpenOrdersID == null)
                {
                    homeIndex.OpenOrdersID = string.Empty;
                }
                _cookieService.SetMerchantandFoodTable(homeIndex.FoodTableName, homeIndex.MerchantName, homeIndex.OpenOrdersID, null);

                MerchantName = homeIndex.MerchantName;
                FoodTableName = homeIndex.FoodTableName;

                ViewBag.MerchantName = MerchantName;
                ViewBag.FoodTableName = FoodTableName;
                ViewBag.FocusCategoryId = focuscategoryID;
                ViewBag.FocusSubCategoryID = focussubcategoryID;


                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                CustomerName = clientAndCustomer.customername;
                Customerphone = clientAndCustomer.phonenumber;

                ViewBag.CustomerName = CustomerName;
                ViewBag.Customerphone = Customerphone;

                return View(homeIndex);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "Missing generateCode" || ex.Message.ToString() == "Missing merchantid" || ex.Message.ToString() == "invalid generateCode")
                {
                    return RedirectToAction("ConnectionFailed", new RouteValueDictionary(new { controller = "Home", action = "ConnectionFailed" }));
                }
                else
                {
                    return View(homeIndex);
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> SubList(int itemID, int focuscategoryID, int focussubcategoryID)
        {
            GetItemInfoModel itemInfo = new GetItemInfoModel();
            try
            {
                string MerchantName = string.Empty;
                string FoodTableName = string.Empty;
                string BranchID = string.Empty;
                string CustomerName = string.Empty;
                string Customerphone = string.Empty;

                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                MerchantName = clientAndCustomer.merchantname;
                FoodTableName = clientAndCustomer.foodtablename;
                BranchID = clientAndCustomer.branchid;
                CustomerName = clientAndCustomer.customername;
                Customerphone = clientAndCustomer.phonenumber;

                ViewBag.MerchantName = MerchantName;
                ViewBag.FoodTableName = FoodTableName;
                ViewBag.FocusCategoryId = focuscategoryID;
                ViewBag.FocusSubCategoryId = focussubcategoryID;

                ViewBag.CustomerName = CustomerName;
                ViewBag.Customerphone = Customerphone;

                itemInfo = await GetItemInfo(itemID, BranchID);
                return View(itemInfo);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "Missing generateCode" || ex.Message.ToString() == "Missing merchantid" || ex.Message.ToString() == "invalid generateCode")
                {
                    return RedirectToAction("ConnectionFailed", new RouteValueDictionary(new { controller = "Home", action = "ConnectionFailed" }));
                }
                else
                {
                    return View(itemInfo);
                }
            }
        }
        
        public async Task<IActionResult> SubListEdit(int itemID, string jsonorderDetail, int indexrow)
        {
            GetItemInfoModel itemInfo = new GetItemInfoModel();
            SubListAddtoCartModel itemChoose = new SubListAddtoCartModel();
            EditItem editItem = new EditItem();
            try
            {
                if (itemID == 0)
                {
                    itemID = TempData["itemID"] as int? ?? 0;
                    jsonorderDetail = TempData["jsonorderDetail"] as string;
                    indexrow = TempData["indexrow"] as int? ?? 0;

                    TempData["itemID"] = itemID;
                    TempData["jsonorderDetail"] = jsonorderDetail;
                    TempData["indexrow"] = indexrow;
                }
                else
                {
                    TempData["itemID"] = itemID;
                    TempData["jsonorderDetail"] = jsonorderDetail;
                    TempData["indexrow"] = indexrow;
                }

                string MerchantName = string.Empty;
                string FoodTableName = string.Empty;
                string BranchID = string.Empty;
                string CustomerName = string.Empty;
                string Customerphone = string.Empty;

                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                MerchantName = clientAndCustomer.merchantname;
                FoodTableName = clientAndCustomer.foodtablename;
                BranchID = clientAndCustomer.branchid;
                CustomerName = clientAndCustomer.customername;
                Customerphone = clientAndCustomer.phonenumber;

                ViewBag.MerchantName = MerchantName;
                ViewBag.FoodTableName = FoodTableName;
                ViewBag.CustomerName = CustomerName;
                ViewBag.Customerphone = Customerphone;

                itemChoose = JsonConvert.DeserializeObject<SubListAddtoCartModel>(jsonorderDetail);
                itemInfo = await GetItemInfo(itemID, BranchID);

                editItem.item = itemInfo;
                editItem.chooseitem = itemChoose;
                editItem.indexrow = indexrow;

                return View(editItem);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "Missing generateCode" || ex.Message.ToString() == "Missing merchantid" || ex.Message.ToString() == "invalid generateCode")
                {
                    return RedirectToAction("ConnectionFailed", new RouteValueDictionary(new { controller = "Home", action = "ConnectionFailed" }));
                }
                else
                {
                    return View(editItem);
                }
            }
        }

        public async Task<IActionResult> Basket(string json)
        {
            List<ClinetOrderModel> currentorder = new List<ClinetOrderModel>();
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    json = TempData["json"] as string;
                    TempData["json"] = json;
                }
                else
                {
                    TempData["json"] = json;
                }

                currentorder = CreateOrder(json);

                string CustomerName = string.Empty;
                string PhoneNumber = string.Empty;

                var clientAndCustomer = _cookieService.GetClientAndCustomer();

                ViewBag.CustomerName = clientAndCustomer.customername;
                ViewBag.Customerphone = clientAndCustomer.phonenumber;
                ViewBag.FoodTableName = clientAndCustomer.foodtablename;

                return View(currentorder);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "Missing generateCode" || ex.Message.ToString() == "Missing merchantid" || ex.Message.ToString() == "invalid generateCode")
                {
                    return RedirectToAction("ConnectionFailed", new RouteValueDictionary(new { controller = "Home", action = "ConnectionFailed" }));
                }
                else
                {
                    return View(currentorder);
                }
            }
        }

        public IActionResult OrderCartConfirmation()
        {
            try
            {
                string MerchantName = string.Empty;
                string FoodTableName = string.Empty;
                string CustomerName = string.Empty;
                string Customerphone = string.Empty;

                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                MerchantName = clientAndCustomer.merchantname;
                FoodTableName = clientAndCustomer.foodtablename;
                CustomerName = clientAndCustomer.customername;
                Customerphone = clientAndCustomer.phonenumber;

                ViewBag.MerchantName = MerchantName;
                ViewBag.FoodTableName = FoodTableName;
                ViewBag.CustomerName = CustomerName;
                ViewBag.Customerphone = Customerphone;
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        public async Task<IActionResult> RecordOrder(string openPage) //openPage normal = n , changepage = c
        {
            OpenOrderWithDetailModel openOrderWithDetail = new OpenOrderWithDetailModel();
            string CustomerMobile = string.Empty;
            string generateCode = string.Empty;
            string FoodTableName = string.Empty;
            string CustomerName = string.Empty;

            if (!string.IsNullOrEmpty(openPage) || openPage == "c")
            {
                ViewBag.OpenPage = "C";
            }

            try
            {
                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                FoodTableName = clientAndCustomer.foodtablename;
                generateCode = clientAndCustomer.generatecode;
                CustomerMobile = clientAndCustomer.phonenumber;
                CustomerName = clientAndCustomer.customername;

                ViewBag.FoodTableName = FoodTableName;
                ViewBag.CustomerName = CustomerName;
                ViewBag.Customerphone = CustomerMobile;

                openOrderWithDetail = await GetHistoryOrder(generateCode, CustomerMobile);

                return View(openOrderWithDetail);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "QrCodeGenerate is null" || ex.Message.ToString() == "FoodTableNO in null" || ex.Message.ToString() == "CustomerMobile in null" || ex.Message.ToString() == "OpenOrders Not Found" || string.IsNullOrEmpty(ex.Message))
                {
                    return View(openOrderWithDetail);
                }
                else if (ex.Message.ToString() == "Missing generateCode" || ex.Message.ToString() == "Missing merchantid" || ex.Message.ToString() == "invalid generateCode")
                {
                    return RedirectToAction("ConnectionFailed", new RouteValueDictionary(new { controller = "Home", action = "ConnectionFailed" }));
                }
                else 
                {
                    return View(openOrderWithDetail);
                }
            }
        }

        public async Task<IActionResult> ListOrder(string OpenOrdersID, int OrdersID)
        {
            int MerchantID = 0;
            string BranchID = string.Empty;
            List<OrderDetailModel> orderDetailModel = new List<OrderDetailModel>();
            try
            {
                if (OrdersID == 0)
                {
                    OpenOrdersID = TempData["OpenOrdersID"] as string;
                    OrdersID = TempData["OrdersID"] as int? ?? 0;

                    TempData["OpenOrdersID"] = OpenOrdersID;
                    TempData["OrdersID"] = OrdersID;
                }
                else
                {
                    TempData["OpenOrdersID"] = OpenOrdersID;
                    TempData["OrdersID"] = OrdersID;
                }

                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                MerchantID = Convert.ToInt32(clientAndCustomer.merchantid);
                BranchID = clientAndCustomer.branchid;

                orderDetailModel = await GetHistoryOrderDetail(BranchID, OpenOrdersID, OrdersID);

                return View(orderDetailModel);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString() == "Token is Null" || string.IsNullOrEmpty(ex.Message))
                {
                    return View(orderDetailModel);
                }
                else if (ex.Message.ToString() == "Missing generateCode" || ex.Message.ToString() == "Missing merchantid" || ex.Message.ToString() == "invalid generateCode")
                {
                    return RedirectToAction("ConnectionFailed", new RouteValueDictionary(new { controller = "Home", action = "ConnectionFailed" }));
                }
                else 
                {
                    return View(orderDetailModel);
                }
            }
        }

        public IActionResult Order()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        public async Task<IActionResult> OrderHome()
        {
            HomeIndexModel homeIndex = new HomeIndexModel();
            try
            {
                var data =  _cookieService.GetClientAndCustomer();
                var getClientModel = await GetHomeIndexDetail(data.branchid);

                if (getClientModel.Merchant != null)
                {
                    homeIndex.MerchantName = getClientModel.Merchant.Name;
                    homeIndex.MerchantLogo = getClientModel.Merchant.LogoPath;
                }

                return View(homeIndex);
            }
            catch (Exception)
            {
                return View(homeIndex);
            }
        }

        public IActionResult SaveCustomer(string CustomerName, string PhoneNumber)
        {
            try
            {
                string _CustomerName = string.Empty;
                string _PhoneNumber = string.Empty;

                _CustomerName = CustomerName;
                _PhoneNumber = PhoneNumber.Replace("-", "");

                _cookieService.SetCustomer(_CustomerName, _PhoneNumber,null);

                return  RedirectToAction("Index","Home");
            }
            catch (Exception)
            {
                return RedirectToAction("SaveCustomer", "Home");
            }
        }

        public IActionResult ConnectionFailed()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<List<ClientCategoryModel>> SubCategoryItem(string value)
        {
            List<ClientCategoryModel> categoryModels = new List<ClientCategoryModel>();
            ClientCategoryModel clientSubCategoryandItemModel;
            ClientSubCategoryModel clientSubCategorys = new ClientSubCategoryModel();
            List<ClientItemsModel> lstitem = new List<ClientItemsModel>();
            try
            {
                var data = _cookieService.GetClientAndCustomer();
                categoryModels = await GetSubCategoryandItemDetail(data.branchid, Convert.ToInt32(value));

                foreach (var categoryModel in categoryModels)
                {
                    clientSubCategorys = new ClientSubCategoryModel()
                    {
                        CategoryID = categoryModel.CategoryID,
                        SubCategoryID = 0,
                        SubCategoryName = "ทั้งหมด",
                    };
                    categoryModel.subCategories.Insert(0, clientSubCategorys);
                }

                return categoryModels;
            }
            catch (Exception ex)
            {
                return categoryModels;
            }
        }

        public List<ClinetOrderModel> CreateOrder(string value)
        {
            List<ClinetOrderModel> listCartOrderModels = new List<ClinetOrderModel>();
            ClinetOrderModel order = new ClinetOrderModel();
            List<ClientOrderDetailModel> lstOrderDetailModel = new List<ClientOrderDetailModel>();
            SubListAddtoCartModel.ItemOption itemTopping = new SubListAddtoCartModel.ItemOption();
            try
            {
                string json = value;
                List<SubListAddtoCartModel> listsubitem = JsonConvert.DeserializeObject<List<SubListAddtoCartModel>>(json);

                int MerchantID = 0;
                string BranchID = string.Empty;
                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                MerchantID = Convert.ToInt32(clientAndCustomer.merchantid);
                BranchID = clientAndCustomer.branchid;


                foreach (var item in listsubitem)
                {
                    //เตรียมข้อมูลส่งไป API Order
                    List<OrderDetailOptionExtra> lstOptionExtra = new List<OrderDetailOptionExtra>();
                    if (item.itemOption.Count > 0)
                    {
                        foreach (var topping in item.itemOption)
                        {
                            OrderDetailOptionExtra orderDetailOptionExtra = new OrderDetailOptionExtra()
                            {
                                MerchantID = MerchantID,
                                BranchID = BranchID,
                                OpenOrdersID = null, //รับค่าจากการเปิดโต๊ะ ครั้งแรก null
                                OrdersID = 0, //ทาง api update OrdersID
                                OrderDetailNo = 0, //index ที่ row ของ lstorder  //ทาง api update OrderDetailNo
                                OrderDetailOptionExtraNo = 0, //ทาง api update OrderDetailNo
                                Type = 'O', //'O' Option, 'E' Extra: type O OptionID, OptionDetailNO, type E ExtraID, ExtraDetailNO
                                OptionID = topping.OptionID,
                                OptionDetailNO = topping.OptionDetailNO,
                                ExtraID = null,
                                ExtraDetailNO = null,
                                Price = topping.Price,    //sum ราคาของ Option , Extra
                                OptionExtraDetailName = topping.OptionDetailName,
                            };
                            lstOptionExtra.Add(orderDetailOptionExtra);
                        }
                    }

                    if (item.itemExtras.Count > 0)
                    {
                        foreach (var extra in item.itemExtras)
                        {
                            OrderDetailOptionExtra orderDetailOptionExtra = new OrderDetailOptionExtra()
                            {
                                MerchantID = MerchantID,
                                BranchID = BranchID,
                                OpenOrdersID = null, //รับค่าจากการเปิดโต๊ะ ครั้งแรก null
                                OrdersID = 0, //ทาง api update OrdersID
                                OrderDetailNo = 0, //ทาง api update OrderDetailNo
                                OrderDetailOptionExtraNo = 0, //ทาง api update OrderDetailNo
                                Type = 'E', //'O' Option, 'E' Extra: type O OptionID, OptionDetailNO, type E ExtraID, ExtraDetailNO
                                OptionID = null,
                                OptionDetailNO = null,
                                ExtraID = extra.ExtraID,
                                ExtraDetailNO = extra.ExtraDetailNO,
                                Price = extra.Price,    //sum ราคาของ Option , Extra
                                OptionExtraDetailName = extra.ExtraDetailName,
                            };
                            lstOptionExtra.Add(orderDetailOptionExtra);
                        }
                    }

                    var _SumOptionExtraPrice = lstOptionExtra.Sum(x => x.Price);
                    var _Amount = (item.SellingPrice + _SumOptionExtraPrice) * item.ItemQuantity;

                    ClientOrderDetailModel orderDetail = new ClientOrderDetailModel()
                    {
                        MerchantID = MerchantID,
                        BranchID = BranchID,
                        OrdersID = 0, //ทาง api update OrdersID
                        OpenOrdersID = null,//รับค่าจากการเปิดโต๊ะ ครั้งแรก null
                        OrderDetailNo = 0, //ทาง api update OrderDetailNo
                        ItemID = item.ItemID, //ลูกค้าเลือกสินค้ามา
                        Status = 'W', // Waiting Served รอเสิร์ฟ (Default) : W
                        Price = item.SellingPrice, //ลูกค้าเลือกสินค้ามา ราคาที่ลดแล้ว
                        NormalPrice = item.Price, //ราคาสินค้า ราคาปกติ แสดงราคาที่ตั้งไว้
                        Quantity = item.ItemQuantity, //ลูกค้าเลือกสินค้ามา
                        SumOptionExtraPrice = _SumOptionExtraPrice, //ยอดรวมของ OrderDetailOptionExtra
                        Amount = _Amount, //(Price + SumOptionExtraPrice )* Quantity
                        OptionExtra = lstOptionExtra,
                        Comments = item.Comment,
                        itemName = item.ItemName,
                    };
                    lstOrderDetailModel = new List<ClientOrderDetailModel>();
                    lstOrderDetailModel.Add(orderDetail);

                    //save to order
                    order = new ClinetOrderModel
                    {
                        BranchID = BranchID,
                        DateModified = DateTime.UtcNow,
                        MerchantID = MerchantID,
                        OrdersID = 0, //ทาง api update OrdersID
                        OrderDetailModel = lstOrderDetailModel, //list ของ item ที่เลือก
                        OpenOrdersID = null,//รับค่าจากการเปิดโต๊ะ ครั้งแรก null
                        Status = 'W', //W : Waiting Served รอเสิร์ฟ (Default)
                        Total = lstOrderDetailModel.Sum(x => x.Amount), //sum ไปก่อนน                    
                    };
                    listCartOrderModels.Add(order);

                }
                return listCartOrderModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return listCartOrderModels;
            }
        }

        public static string ConvertToCurrency(decimal txt)
        {
            decimal decimalValue = txt;
            decimalValue = Math.Round(decimalValue, 2, MidpointRounding.AwayFromZero);
            string ctxt = decimalValue.ToString("#,####.#0");

            if (ctxt == ".00")
            {
                ctxt = "0.00";
            }

            if (decimalValue > 0)
            {
                return "+" + ctxt;
            }
            else
            {
                return ctxt;
            }
        }

        public static string ConvertToCurrencyPrice(decimal txt)
        {
            decimal decimalValue = txt;
            decimalValue = Math.Round(decimalValue, 2, MidpointRounding.AwayFromZero);
            string ctxt = decimalValue.ToString("#,####.#0");

            if (ctxt == ".00")
            {
                ctxt = "0.00";
            }
            return ctxt;
        }

        public static int ConvertToInt(decimal txt)
        {
            int Value = Convert.ToInt32(txt);
            return Value;
        }

        public static string ConvertDatetimeLocal(DateTime d)
        {
            try
            {
                var timezoneslocal = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                string datetime = TimeZoneInfo.ConvertTimeFromUtc(d, timezoneslocal).ToString("dd/MM/yyyy HH:mm", new CultureInfo("en-US"));
                return datetime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }

        public static string PhoneNumberFormat(string phonenumber)
        {
            try
            {
                string Phone = phonenumber;
                phonenumber = phonenumber.Insert(6, "-").Insert(3, "-");
                return phonenumber;
            }
            catch (Exception)
            {
                return phonenumber;
            }
        }


        [HttpPost]
        public List<SubListAddtoCartModel> CheckSameOrderSelect(List<SubListAddtoCartModel> lstorder, SubListAddtoCartModel chooseorder )
        {
            try
            {
                var findlist = lstorder.FindAll(x => x.ItemID == chooseorder.ItemID);
                if (findlist.Count == 0)
                {
                    lstorder.Add(chooseorder);
                    return lstorder;
                }
                else
                {
                    int indexsame = -1;
                    bool newitem = false; //newitem = true 
                    int i = -1;
                    foreach (var itemfind in findlist)
                    {
                        i++;

                        if (itemfind.itemOption.Count == chooseorder.itemOption.Count)
                        {
                            var lsttemp = itemfind.itemOption.Where(x => !chooseorder.itemOption.Any(y => y.OptionID == x.OptionID && y.OptionDetailNO == x.OptionDetailNO)).ToList();
                            if (lsttemp.Count > 0)
                            {
                                newitem = true;
                                continue;
                            }
                        }
                        else
                        {
                            newitem = true;
                            continue;
                        }

                        if (itemfind.itemExtras.Count == chooseorder.itemExtras.Count)
                        {
                            var lsttemp = itemfind.itemExtras.Where(x => !chooseorder.itemExtras.Any(y => y.ExtraID == x.ExtraID && y.ExtraDetailNO == x.ExtraDetailNO)).ToList();
                            if (lsttemp.Count > 0)
                            {
                                newitem = true;
                                continue;
                            }
                        }
                        else
                        {
                            newitem = true;
                            continue;
                        }

                        if (itemfind.Comment != chooseorder.Comment)
                        {
                            newitem = true;
                            continue;
                        }
                        newitem = false;
                        indexsame = i;
                        break;
                    }
                    if (newitem)
                    {
                        lstorder.Add(chooseorder);
                        return lstorder;
                    }

                    findlist[indexsame].ItemQuantity += chooseorder.ItemQuantity;
                    return lstorder;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return lstorder;
            }
        }

        ///// API /////
        public async Task<ClientModel> GetHomeIndexDetail(string BranchID)
        {
            ClientModel clientModel;
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    var httpClient = SetHttpClientConnectMyMenuAPIClient(_httpClient);
                    var url = MicroServiceName.MyMenuAPI + Client + "?BranchID=" + BranchID;
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        clientModel = new ClientModel();
                        clientModel = JsonConvert.DeserializeObject<ClientModel>(contents);
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
                return clientModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private HttpClient SetHttpClientConnectMyMenuAPIClient(HttpClient? httpClient)
        {
            try
            {
                var jwt = _cookieService.GetToken();
                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                httpClient.SetBearerToken(jwt);
                httpClient.DefaultRequestHeaders.Add("GenerateCode", clientAndCustomer.generatecode);
                httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                return httpClient;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ClientCategoryModel>> GetSubCategoryandItemDetail(string BranchID, int CategoryID)
        {
            List<ClientCategoryModel> clientSubCategoryandItems;
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    var httpClient = SetHttpClientConnectMyMenuAPIClient(_httpClient);
                    var url = MicroServiceName.MyMenuAPI + Client + "/ListSubandItem" + "?BranchID=" + BranchID + "&CategoryID=" + CategoryID;
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        clientSubCategoryandItems = new List<ClientCategoryModel>();
                        clientSubCategoryandItems = JsonConvert.DeserializeObject<List<ClientCategoryModel>>(contents);
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
                return clientSubCategoryandItems;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<GetItemInfoModel> GetItemInfo(int itemID, string? BranchID)
        {
            GetItemInfoModel itemInfoModel;
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    var httpClient = SetHttpClientConnectMyMenuAPIClient(_httpClient);
                    var url = MicroServiceName.MyMenuAPI + Client + "/ItemInfo?itemID= " + itemID + "&BranchID=" + BranchID;
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        itemInfoModel = new GetItemInfoModel();
                        itemInfoModel = JsonConvert.DeserializeObject<GetItemInfoModel>(contents);
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
                return itemInfoModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OpenOrderWithDetailModel> GetHistoryOrder(string generateCode, string CustomerMobile)
        {
            OpenOrderWithDetailModel openOrderWithDetails;
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    var httpClient = SetHttpClientConnectMyMenuAPIClient(_httpClient);
                    var url = MicroServiceName.MyMenuAPI + Client + "/GetHistoryOrder?generateCode=" + generateCode + "&CustomerMobile=" + CustomerMobile;
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        openOrderWithDetails = new OpenOrderWithDetailModel();
                        openOrderWithDetails = JsonConvert.DeserializeObject<OpenOrderWithDetailModel>(contents);
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
                return openOrderWithDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<OrderDetailModel>> GetHistoryOrderDetail(string BranchID, string OpenOrdersID, int OrdersID)
        {
            List<OrderDetailModel> orderDetail;
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    var httpClient = SetHttpClientConnectMyMenuAPIClient(_httpClient);
                    var url = MicroServiceName.MyMenuAPI + Client + "/GetHistoryOrderDetail?BranchID=" + BranchID + "&OpenOrdersID=" + OpenOrdersID + "&OrdersID=" + OrdersID;
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        orderDetail = new List<OrderDetailModel>();
                        orderDetail = JsonConvert.DeserializeObject<List<OrderDetailModel>>(contents);
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
                return orderDetail;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostOrder(string value)
        {
            try
            {
                string CustomerName = string.Empty;
                string PhoneNumber = string.Empty;
                string generateCode = string.Empty;
                string OpenOrdersID = string.Empty;

                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                CustomerName = clientAndCustomer.customername;
                PhoneNumber = clientAndCustomer.phonenumber;
                generateCode = clientAndCustomer.generatecode;
                OpenOrdersID = clientAndCustomer.openordersid;

                List<ClinetOrderModel> lstorder = new List<ClinetOrderModel>();
                lstorder = CreateOrder(value);

                List<ClientOrderDetailModel> lstorderdetail = new List<ClientOrderDetailModel>();
                foreach (var clinetOrderModel in lstorder)
                {
                    lstorderdetail.AddRange(clinetOrderModel.OrderDetailModel);
                }

                //mapping Order to OrderNew
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<ClientOrderDetailModel, OrderDetailModel>();
                });

                var Imapper = config.CreateMapper();
                var lstOrdersDetail = Imapper.Map<List<ClientOrderDetailModel>, List<OrderDetailModel>>(lstorderdetail);


                OrderModel orderModel = new OrderModel()
                {
                    BranchID = lstorder.FirstOrDefault().BranchID,
                    DateModified = lstorder.FirstOrDefault().DateModified,
                    MerchantID = lstorder.FirstOrDefault().MerchantID,
                    OpenOrdersID = (OpenOrdersID == string.Empty ? null: OpenOrdersID),
                    OrderDetailModel = lstOrdersDetail,
                    OrdersID = 0,
                    Status = lstorder.FirstOrDefault().Status,
                    Total = lstorder.Sum(x => x.Total),
                    CustomerMobile = PhoneNumber,
                    CustomerName = CustomerName,
                    FoodTableNO = null,
                };

                using (var _httpClient = new HttpClient())
                {
                    var httpClient = SetHttpClientConnectMyMenuAPIClient(_httpClient);
                    var url = MicroServiceName.MyMenuAPI + Client + "/Order";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(orderModel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await httpClient.PostAsync(url, httpContent);
                    var contents = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(contents);
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
        public async Task<ActionResult> CallStaff(string? Comments, char NotificationType)
        {
            try
            {
                var clientAndCustomer = _cookieService.GetClientAndCustomer();
                var CUSTOMERMOBILE = clientAndCustomer.phonenumber;

                using (var _httpClient = new HttpClient())
                {
                    var httpClient = SetHttpClientConnectMyMenuAPIClient(_httpClient);
                    var url = MicroServiceName.MyMenuAPI + Client + "/CallStaff?Comments=" + Comments + "&NotificationType=" + NotificationType + "&CUSTOMERMOBILE=" + CUSTOMERMOBILE; 
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,
                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var json = JsonConvert.SerializeObject("", settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await httpClient.PostAsync(url, httpContent);
                    var contents = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(contents);
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

    }
}
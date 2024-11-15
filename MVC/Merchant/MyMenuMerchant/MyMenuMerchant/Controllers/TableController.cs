using AutoMapper;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using MyMenu.ORM.Period;
using MyMenuClient.Models;
using MyMenuMerchant.Models;
using MyMenuMerchant.Utills;
using Newtonsoft.Json;
using NuGet.Configuration;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class TableController : Controller
    {
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        private readonly string role;

        public TableController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }
        public static string ConvertToCurrency(decimal txt)
        {
            decimal decimalValue = txt;
            decimalValue = Math.Round(decimalValue, 2, MidpointRounding.AwayFromZero);
            var x = decimalValue.ToString("#,###0.#0");
            return x;
        }
        // GET: TableController
        public async Task<IActionResult> Index(bool error, string UserMessage)
        {
            if (!string.IsNullOrEmpty(UserMessage))
            {
                TempData["UserMessage"] = UserMessage ;

            }
            var GetMerchant = await GetTable();
            ViewBag.role = role;
            
            return View(GetMerchant);
        }
        public async Task<IActionResult> Table(string data)
        {
            try
            {
                var table = JsonConvert.DeserializeObject<TableWithOrderModel>(data);
                return View(table);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public async Task<IActionResult> ModalMenu(string id)
        {
            var cate = await GetMenuCategory();
            return View(cate);
        }

        public async Task<IActionResult> ModalDetailsMenu(string id)
        {
            var iteminfo = await GetItemInfo(id);
            return View(iteminfo);
        }
        public async Task<IActionResult> ModalCart(List<SubListAddtoCartModel> lstorder)
        {
            
            return View(lstorder);
        }
        public async Task<IActionResult> TableDetails(string id)
        {
            var iteminfo = await GetTableDetail(id);
            ViewBag.role = role;
            if (iteminfo.OpenOrdersID == null)
            {
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Table", action = "Index", error = true, UserMessage = "คำสั่งซื้อนี้สำเร็จแล้ว" }));
            }
            else if(iteminfo.OrderModels.Count == 0) 
            {
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Table", action = "Index", error = true, UserMessage = iteminfo.FoodTableName+" เรียกพนักงาน" }));
            }
            else
            {
                return View(iteminfo);
            }
        }
        [HttpPost]
        public async Task<ActionResult> PaymentProcess(string transtring)
        {
            var tran = JsonConvert.DeserializeObject<TranModel>(transtring);
            var branchConfigs= await GetListMyQRCode();
            var qr = branchConfigs.Where(x => x.CfgKey == "QRPATH" && !string.IsNullOrEmpty(x.CfgString)).FirstOrDefault();
            ViewBag.qrcode = qr;
            try
            {
                return View(tran);
            }
            catch (Exception)
            {
                return View(tran);
            }
        }

        public async Task<TranModel> GetHistoryDetail(TranHistoryModel tranHistoryModel)
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
                        return JsonConvert.DeserializeObject<TranModel>(json_Catagory);
                    }
                }
            }
            return null;
        }

        [HttpPost]
        public async Task<ActionResult>  PostTran(string value)
        {
            try
            {
                var TranModel = JsonConvert.DeserializeObject<TranModel>(value);
                
                //string branchid = "1";
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Tran";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };

                    var json = JsonConvert.SerializeObject(TranModel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await httpClient.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        string json_Catagory = await result.Content.ReadAsStringAsync();
                        if (json_Catagory != null)
                        {
                            return Conflict(json_Catagory);
                        }
                        return Conflict(json_Catagory);
                    }
                    else
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string json_Catagory = await result.Content.ReadAsStringAsync();
                            if (json_Catagory != null)
                            {
                                return Ok(json_Catagory);
                            }
                        }
                    }
                    return Ok();
                }
                
            }
            catch (Exception ex)
            {
                return Conflict(ex);
            }
        }

        public ActionResult TableQRCode(string json)
        {
            var qrcode  = JsonConvert.DeserializeObject<QrCodeGenerateModel>(json);
            System.Uri uri = new System.Uri(qrcode.GenerateCode);
            qrcode.GenerateCode = uri.ToString();
            return View(qrcode);
        }
        public async Task<List<TableWithOrderModel>> GetTable()
        {
            List<TableWithOrderModel> tableWithOrderModels;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var branchID = _cookieService.GetBranch();
                    var url = MicroServiceName.MyMenuAPI + "FoodTable/TableWithOrder?BranchID=" + branchID;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        tableWithOrderModels = new List<TableWithOrderModel>();
                        tableWithOrderModels = JsonConvert.DeserializeObject<List<TableWithOrderModel>>(contents);
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
                return tableWithOrderModels;
            }
            catch (Exception)
            {
                return new List<TableWithOrderModel>();
            }
        }
        public async Task<List<TableWithOrderModel>> GetTableloop(string date)
        {
            List<TableWithOrderModel> tableWithOrderModels;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    DateTime dateTime = DateTime.Parse(date);
                    dateTime = dateTime.AddSeconds(1);
                    IFormatProvider culture = new CultureInfo("en-US", true);
                    httpClient.SetBearerToken(Jwt);
                    var branchID = _cookieService.GetBranch();
                    var url = MicroServiceName.MyMenuAPI + "FoodTable/TableWithOrderLast?BranchID=" + branchID + "&dateTime=" + dateTime.ToString("yyyyMMddHHmmss", culture);
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        tableWithOrderModels = new List<TableWithOrderModel>();
                        tableWithOrderModels = JsonConvert.DeserializeObject<List<TableWithOrderModel>>(contents);
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
                return tableWithOrderModels;
            }
            catch (Exception)
            {
                return new List<TableWithOrderModel>();
            }
        }
        public async Task<OpenOrderWithDetailModel> GetTableDetail(string OpenOrdersID)
        {
            OpenOrderWithDetailModel openOrder;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var branchID = _cookieService.GetBranch();
                    var url = MicroServiceName.MyMenuAPI + "Openorder/OpenOrderDetail?BranchID=" + branchID + "&OpenOrdersID="+ OpenOrdersID;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        openOrder = new OpenOrderWithDetailModel();
                        openOrder = JsonConvert.DeserializeObject<OpenOrderWithDetailModel>(contents);
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
                return openOrder;
            }
            catch (Exception)
            {
                return new OpenOrderWithDetailModel();
            }
        }

        [HttpPost]
        public List<SubListAddtoCartModel> CheckSameOrderSelect(List<SubListAddtoCartModel> lstorder, SubListAddtoCartModel chooseorder)
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
        [HttpPost]
        public async Task<ActionResult> Opentable(string tableno)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "FoodTable/OpenTable";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    OpenOrder openOrder = new OpenOrder() 
                    {
                        FoodTableNO = Convert.ToInt32(tableno), 
                        BranchID = branchID,
                        OpenOrdersType = 'D'

                    };
                    var json = JsonConvert.SerializeObject(openOrder, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await httpClient.PutAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string json_Catagory = await result.Content.ReadAsStringAsync();
                            if (json_Catagory != null)
                            {
                                return Ok(json_Catagory);
                            }
                        }
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Closetable(string OpenOrdersID)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "FoodTable/CloseTable?BranchID="+ branchID + "&OpenOrdersID="+ OpenOrdersID;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    
                    var result = await httpClient.PutAsync(url, null);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return Ok();
                        }
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CaltranBefore(string id)
        {
            try
            {
                OpenOrderWithDetailModel open = JsonConvert.DeserializeObject<OpenOrderWithDetailModel>(id);
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Tran/CreateTran";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    
                    var json = JsonConvert.SerializeObject(open, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await httpClient.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string json_Catagory = await result.Content.ReadAsStringAsync();
                            if (json_Catagory != null)
                            {
                                return Ok(json_Catagory);
                            }
                        }
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> PostOrder(string value , string id)
        {
            try
            {
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
                    OpenOrdersID = id,
                    OrderDetailModel = lstOrdersDetail,
                    OrdersID = 0,
                    Status = lstorder.FirstOrDefault().Status,
                    Total = lstorder.Sum(x => x.Total),
                };

                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Order";
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(orderModel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await httpClient.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        var iteminfo = await GetTableDetail(id);
                        var json2 = JsonConvert.SerializeObject(iteminfo);
                        return Ok(json2);
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
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
                var branchID = _cookieService.GetBranch();
                int MerchantID = 0;
                string BranchID = string.Empty;
                MerchantID = Convert.ToInt32(_cookieService.GetCookieValue("MerchantID"));
                BranchID = branchID;
                //BranchID = _cookieService.GetCookieValue("BranchID");


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
                        Price = item.SellingPrice, //ลูกค้าเลือกสินค้ามา
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
        public async Task<ItemWithDetail> GetItemInfo(string itemid)
        {
            ItemWithDetail itemWithDetails;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var branchID = _cookieService.GetBranch();
                    var url = MicroServiceName.MyMenuAPI + "Item/GetItemWiteDetial?BranchID=" + branchID + "&ItemID="+ itemid;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        itemWithDetails = new ItemWithDetail();
                        itemWithDetails = JsonConvert.DeserializeObject<ItemWithDetail>(contents);
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
                return itemWithDetails;
            }
            catch (Exception)
            {
                return new ItemWithDetail();
            }
        }

        public async Task<List<Category>> GetMenuCategory()
        {
            List<Category> listcatagory;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Category";
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        listcatagory = new List<Category>();
                        listcatagory = JsonConvert.DeserializeObject<List<Category>>(contents);
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
                return listcatagory;
            }
            catch (Exception)
            {
                return new List<Category>();
            }
        }
        public async Task<MerchantItemsModel> GetItemMenu(string categoryid)
        {
            MerchantItemsModel listitemmodel;
            try
            {
                var branchID = _cookieService.GetBranch();

                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Item/GetMerchantItemsModel?BranchID="+ branchID + "&CategoryID="+ categoryid;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        listitemmodel = new MerchantItemsModel();
                        listitemmodel = JsonConvert.DeserializeObject<MerchantItemsModel>(contents);
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
                return listitemmodel;
            }
            catch (Exception)
            {
                return new MerchantItemsModel();
            }
        }

        public static string ConvertDatetimeLocal(DateTime d)
        {
            try
            {
                //var timezoneslocal = TimeZoneInfo.Local;
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

        public static string ConverttimeLocalcheckNull(DateTime? d)
        {
            try
            {
                if (d.HasValue)
                {
                    return d.Value.TimeOfDay.ToString();
                }
                return string.Empty;
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
                var timezoneslocal = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                string datetime = TimeZoneInfo.ConvertTimeFromUtc(d, timezoneslocal).ToString("HH:mm:ss", new CultureInfo("th-TH"));
                return datetime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }

        public static string ConvertDateLocal(DateTime d)
        {
            try
            {
                //var timezoneslocal = TimeZoneInfo.Local;
                var timezoneslocal = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                string datetime = TimeZoneInfo.ConvertTimeFromUtc(d, timezoneslocal).ToString("dd/MM/yyyy", new CultureInfo("th-TH"));
                return datetime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
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

        //BranchConfig
        public async Task<List<BranchConfig>> GetListMyQRCode()
        {
            List<BranchConfig> lstbranchconfig;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var branchid = _cookieService.GetBranch();
                    var url = MicroServiceName.MyMenuAPI + "BranchConfig" + "?BranchID=" + branchid;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        lstbranchconfig = new List<BranchConfig>();
                        lstbranchconfig = JsonConvert.DeserializeObject<List<BranchConfig>>(contents);
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
                return lstbranchconfig;
            }
            catch (Exception)
            {
                return new List<BranchConfig>();
            }
        }

        //CommentOpenOrder edit
        public async Task<ActionResult> EditComment(OpenOrderWithDetailModel openOrder)
        {
            try
            {
                string BranchID = _cookieService.GetBranch();
                openOrder.BranchID = BranchID;
                openOrder.Discount = "";
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "OpenOrder/CommentOpenOrder";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(openOrder, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PutAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(content);
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

        public async Task<ActionResult> MoveTable(string BranchID, string OpenOrdersID, int FoodTableNO)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "FoodTable/MoveTable?BranchID=" + branchID + "&OpenOrdersID=" + OpenOrdersID + "&FoodTableNO=" + FoodTableNO;
                    var result = await httpClient.PutAsync(url, null);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(content);
                    }
                    else
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return Ok(content);
                        }
                    }
                    return Ok(content);
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

    }
}

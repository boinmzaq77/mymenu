using IdentityModel.Client;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Options;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using MyMenuMerchant.Utills;
using Newtonsoft.Json;
using NuGet.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class BranchManagementController : Controller
    {
        public string Merchant = "Merchant";
        public string FoodTable = "FoodTable";
        public string Branchdetail = "Branch/GetBranchDetail";

        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        private readonly string role;

        public BranchManagementController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }

        // GET: Main Index
        public async Task<IActionResult> Index()
        {
            var branchID =  _cookieService.GetBranch();
            var GetBranch = await GetBranchManage(branchID);
            return View(GetBranch);
        }

        // GET: BranchEdit
        public async Task<IActionResult> BranchEdit()
        {
            var branchID = _cookieService.GetBranch();
            var GetBranch = await GetBranchDetail(branchID);
            return View(GetBranch);

        }

        // GET: TableManagement
        public ActionResult TableManagement()
        {
            ViewBag.role = role;
            return View();
        }

        // GET: MyQR
        public async Task<ActionResult> MyQR()
        {
            try
            {
                ViewBag.role = role;
                var GetlstBranchconfig = await GetListMyQRCode();
                return View(GetlstBranchconfig);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: TableManagementAdd
        public ActionResult TableManagementAdd()
        {
            return View();
        }

        // GET: QRCodeSetup
        public ActionResult QRCodeSetup()
        {
            ViewBag.role = role;
            return View();
        }

        // GET: QRCodeSetupDetails
        public async Task<IActionResult> QRCodeSetupDetails(string text)
        {
            var x = text; 
            QrCodeGenerate qrCodeGenerate = JsonConvert.DeserializeObject<QrCodeGenerate>(text);
            var getqrcode = await GetQrcodeDetail(qrCodeGenerate.QrCodeGenerateNO.ToString());

            //เพิ่ม 
            System.Uri uri = new System.Uri(getqrcode.GenerateCode);
            getqrcode.GenerateCode = uri.ToString();
            return View(getqrcode);
        }

        // GET: MenuBranch
        public ActionResult MenuBranch()
        {
            ViewBag.role = role;
            return View();
        }

        // GET: PaymentReceipt
        public async Task<ActionResult> PaymentReceipt()
        {
            try
            {
                ViewBag.role = role;
                var branchID = _cookieService.GetBranch();
                var GetBranch = await GetBranchDetail(branchID);
                return View(GetBranch);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: PaymentReceiptEdit
        public async Task<ActionResult> PaymentReceiptEdit()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var GetBranch = await GetBranchDetail(branchID);
                return View(GetBranch);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: POSMachice
        public ActionResult POSMachice()
        {
            ViewBag.role = role;
            return View();
        }

        // GET: BranchSetup
        public async Task<ActionResult> BranchSetup()
        {
            ViewBag.role = role;
            var branchID = _cookieService.GetBranch();
            var GetBranch = await GetBranchDetail(branchID);
            return View(GetBranch);
        }
        // GET: Printers
        public ActionResult Printers()
        {
            ViewBag.role = role;
            return View();
        }

        public async Task<ActionResult> ServiceCharge()
        {
            ViewBag.role = role;
            var branchID = _cookieService.GetBranch();
            var GetBranch = await GetBranchDetail(branchID);
            return View(GetBranch);
        }

        public async Task<ActionResult> CashDrawer()
        {
            ViewBag.role = role;
            var branchID = _cookieService.GetBranch();
            var GetBranch = await GetBranchManage(branchID);
            return View(GetBranch);
        }

        public async Task<ActionResult> AutoServe()
        {
            ViewBag.role = role;
            var branchID = _cookieService.GetBranch();
            var GetBranch = await GetBranchManage(branchID);
            return View(GetBranch);
        }

        public class DataPrintPoint
        {
            public int printID { get; set; }
            public string printName { get; set; }
            public string printType { get; set; }
            public string PrintCut { get; set; }
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
                            var printerModels = JsonConvert.DeserializeObject<List<PrinterModel>>(json_Catagory);
                            List<PrinterModel> printerModelsnew = new List<PrinterModel>();
                            var main = printerModels.Where(x => x.PrinterMain == 1).FirstOrDefault();
                            if (main != null)
                            {
                                printerModelsnew.Add(main);
                            }
                            printerModelsnew.AddRange(printerModels.Where(x => x.PrinterMain == 0).ToList());
                            return JsonConvert.SerializeObject(printerModelsnew);
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
        public async Task<string> GetPrintsPointDetail()
        {
            PrinterModel ret = new PrinterModel()
            {
                PrinterNO = 1,
                PrinterName = "cashier",
                PrinterType = 'D',
                PrinterMain = 0,
                PaperSize = "80",
                AutoPrintCheckBill = 0,
                AutoSendKitchen = 0,
                CustingPaperMode = 'O',
                Categories = new List<Category>()
                {
                    new Category() {  CategoryID = 1 ,CategoryName = "กาแฟ" },
                    new Category() {  CategoryID = 2 ,CategoryName = "ขนม" },
                    new Category() {  CategoryID = 3 ,CategoryName = "อาหารทานเล่น" },
                    new Category() {  CategoryID = 4 ,CategoryName = "อาหารจานเดียว" },
                    new Category() {  CategoryID = 5 ,CategoryName = "เครื่องดื่มโซดา" },
                    new Category() {  CategoryID = 6 ,CategoryName = "เครื่องดื่มชา" }
                },

                IPAdress = "192.168.200.182", // เก็บที่เครื่อง 
                Port = "80",// เก็บที่เครื่อง 
            };

            return JsonConvert.SerializeObject(ret); ;
        }


        // GET: PrintersAdd
        public async Task<ActionResult> PrintersAdd()
        {
            var cat = await GetMenuCategory();
            var category = JsonConvert.DeserializeObject<List<CategoryModel>>(cat);
            return View(category);
        }
        // GET: PrintersEdit
        public async Task<ActionResult> PrintersEdit(string print)
        {
            ViewBag.role = role;
            var printerModels = JsonConvert.DeserializeObject<PrinterModel>(print);
            var cat = await GetMenuCategory();
            var category = JsonConvert.DeserializeObject<List<CategoryModel>>(cat);
            PrinterEditModel printerEditModel = new PrinterEditModel()
            {
                categorys = category,
                printerModel = printerModels
            };
            return View(printerEditModel);
        }

        public async Task<string> GetDatableTable()
        {
            var Gettable = await GetTable();
            return JsonConvert.SerializeObject(Gettable); ;
        }
        public class PrinterEditModel
        {
            public PrinterModel printerModel { get; set; } // integer
            public List<CategoryModel> categorys { get; set; } // integer
        }
        public async Task<string> GetItemOnBranch()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    string url = MicroServiceName.MyMenuAPI + "ItemOnBranch?BranchID=" + branchID + "&offset = 0"
                        ;
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

        [HttpPost]
        public async Task<string> GetMenubranchItemoffset(string offset)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "ItemOnBranch?BranchID=" + branchID + "&offset=" + offset;
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
        public async Task<string> GetMenuCategory()
        {
            try
            {

                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Category";
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

        public async Task<string> GetQrcode()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    string url = MicroServiceName.MyMenuAPI + "QrCodeGenerates?BranchID=" + branchID;

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
        public async Task<BranchManageModel> GetBranchManage(string id)
        {
            BranchManageModel branch;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Branch/BrachManage" + "?BranchID=" + id;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        branch = new BranchManageModel();
                        branch = JsonConvert.DeserializeObject<BranchManageModel>(contents);
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
                return branch;
            }
            catch (Exception)
            {
                return new BranchManageModel();
            }
        }

        public async Task<Branch> GetBranchDetail(string id)
        {
            Branch branch;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + Branchdetail + "?BranchID=" + id;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        branch = new Branch();
                        branch = JsonConvert.DeserializeObject<Branch>(contents);
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
                return branch;
            }
            catch (Exception)
            {
                return new Branch();
            }
        }
        public async Task<QrCodeGenerateModel> GetQrcodeDetail(string qrcodegenerateno)
        {
            QrCodeGenerateModel qrCodeGenerateModel;
            var branchID = _cookieService.GetBranch();
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "QrCodeGenerates/GetQRCodeDetail?BranchID=" + branchID + "&QrCodeGenerateNO=" + qrcodegenerateno;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        qrCodeGenerateModel = new QrCodeGenerateModel();
                        qrCodeGenerateModel = JsonConvert.DeserializeObject<QrCodeGenerateModel>(contents);
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
                return qrCodeGenerateModel;
            }
            catch (Exception)
            {
                return new QrCodeGenerateModel();
            }
        }
        //public async Task<Branch> GetBranchSetting(string id)
        //{
        //    Branch branch;
        //    try
        //    {
        //        using (var httpClient = new HttpClient())
        //        {
        //            var url = MicroServiceName.MyMenuAPI  + "Branch/GetBranchSetting" + "?BranchID=" + id;
        //            httpClient.Timeout = TimeSpan.FromMinutes(0.5);
        //            var res = await httpClient.GetAsync(url);
        //            var contents = await res.Content.ReadAsStringAsync();
        //            if (res.IsSuccessStatusCode)
        //            {
        //                branch = new Branch();
        //                branch = JsonConvert.DeserializeObject<Branch>(contents);
        //            }
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(contents))
        //                {
        //                    throw new WebException(contents);
        //                }
        //                else
        //                {
        //                    string message = ((HttpStatusCode)res.StatusCode).ToString();
        //                    throw new WebException(message);
        //                }
        //            }
        //        }
        //        return branch;
        //    }
        //    catch (Exception)
        //    {
        //        return new Branch();
        //    }
        //}
        public async Task<List<FoodTableModel>> GetTable()
        {
            List<FoodTableModel> table;
            var branchID = _cookieService.GetBranch();
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + FoodTable + "?BranchID=" + branchID;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        table = new List<FoodTableModel>();
                        table = JsonConvert.DeserializeObject<List<FoodTableModel>>(contents);
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
                return table;
            }
            catch (Exception)
            {
                return new List<FoodTableModel>();
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditTable(FoodTableModel foodTableModel)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                foodTableModel.BranchID = branchID;
                List<FoodTableModel> table = new List<FoodTableModel>();
                table.Add(foodTableModel);
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + FoodTable + "/UpdateTableName?BranchID=" + foodTableModel.BranchID + "&FoodTableNO=" + foodTableModel.FoodTableNO + "&FoodTableName=" + foodTableModel.FoodTableName;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(table, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
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
        public async Task<ActionResult> AddPrinter(PrinterModel printerModel)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                printerModel.BranchID = branchID;
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Printer";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(printerModel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok(await result.Content.ReadAsStringAsync());
                    }
                }


            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditPrinter(PrinterModel printerModel)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                printerModel.BranchID = branchID;
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Printer";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include
                    };
                    var json = JsonConvert.SerializeObject(printerModel, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PutAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(await result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok(await result.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddTable(FoodTableModel foodTableModel)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                foodTableModel.BranchID = branchID;
                List<FoodTableModel> table = new List<FoodTableModel>();
                table.Add(foodTableModel);
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + FoodTable + "/PostFoodTable?BranchID=" + foodTableModel.BranchID + "&FoodTableName=" + foodTableModel.FoodTableName;
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(table, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, null);
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
        public async Task<ActionResult> DeleteTable(string id)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var branchID = _cookieService.GetBranch();
                    string url = MicroServiceName.MyMenuAPI + FoodTable + "/DeleteFoodTable?BranchID=" + branchID + "&FoodTableNO=" + id;

                    var result = await Client.DeleteAsync(url);

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
        public async Task<ActionResult> DeletePrinter(string id)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var branchID = _cookieService.GetBranch();
                    string url = MicroServiceName.MyMenuAPI + "Printer?BranchID=" + branchID + "&PrinterNO=" + id;

                    var result = await Client.DeleteAsync(url);

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
        public async Task<ActionResult> ChangePrice(string ItemID, string Price, string SellingPrice)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "ItemOnBranch/UpdatePriceItem?ItemID=" + ItemID + "&BranchID=" + branchID + "&Price=" + Price + "&SellingPrice=" + SellingPrice;
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
        [HttpPost]
        public async Task<ActionResult> EditStatusItemOnBranch(string branchid, string status, string itemid)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "ItemOnBranch/UpdateStatus?ItemID=" + itemid + "&BranchID=" + branchID + "&Status=" + status;
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

        [HttpPost]
        public async Task<ActionResult> EditBranchSetting(char orderoptiontype,
            string callstaff,
            char qrcodegeneratetype)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var GetBranch = await GetBranchDetail(branchID);
                GetBranch.OrderOptionType = orderoptiontype;
                GetBranch.CallStaff = int.Parse(callstaff);
                GetBranch.QrCodeGenerateType = qrcodegeneratetype;
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var json = JsonConvert.SerializeObject(GetBranch);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string url = MicroServiceName.MyMenuAPI + "Branch/UpdateBranchSetting";
                    var result = await Client.PutAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(content);
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
        public async Task<ActionResult> EditServiceCharge(char servicechargetype,
            string servicechargerate)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var GetBranch = await GetBranchDetail(branchID);
                GetBranch.ServicechargeType = servicechargetype;
                GetBranch.ServicechargeRate = servicechargerate;
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var json = JsonConvert.SerializeObject(GetBranch);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string url = MicroServiceName.MyMenuAPI + "Branch/UpdateBranchSetting";
                    var result = await Client.PutAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(content);
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
        public async Task<ActionResult> EditCashDrawer(int cashdrawer)
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                var GetBranch = await GetBranchManage(branchID);
                GetBranch.Cashdrawer = cashdrawer;
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    var json = JsonConvert.SerializeObject(GetBranch);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string url = MicroServiceName.MyMenuAPI + "Branch/UpdateBranchSetting";
                    var result = await Client.PutAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(content);
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
        public async Task<ActionResult> ResetQRCode(string BranchID, int QrCodeGenerateNO)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "QrCodeGenerates/ResetQRCode?BranchID=" + BranchID + "&QrCodeGenerateNO=" + QrCodeGenerateNO;
                    var result = await Client.PutAsync(url, null);
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

        [HttpGet]
        public async Task<string> QRCodeDetails(string text)
        {
            var x = text;
            QrCodeGenerate qrCodeGenerate = JsonConvert.DeserializeObject<QrCodeGenerate>(text);
            var getqrcode = await GetQrcodeDetail(qrCodeGenerate.QrCodeGenerateNO.ToString());

            //เพิ่ม 
            System.Uri uri = new System.Uri(getqrcode.GenerateCode);
            getqrcode.GenerateCode = uri.ToString();
            return getqrcode.GenerateCode;
        }

        [HttpGet]
        public async Task<QrCodeGenerateModel> QrCodeGenerateModel(string text)
        {
            var x = text;
            QrCodeGenerate qrCodeGenerate = JsonConvert.DeserializeObject<QrCodeGenerate>(text);
            var getqrcode = await GetQrcodeDetail(qrCodeGenerate.QrCodeGenerateNO.ToString());

            //เพิ่ม 
            System.Uri uri = new System.Uri(getqrcode.GenerateCode);
            getqrcode.GenerateCode = uri.ToString();
            return getqrcode;
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

        [HttpPut]
        public async Task<ActionResult> UpdateHeaderFooterReceipt(string ReceiptName,string ReceiptTextFooter)
        {
            try
            {
                string BranchID = _cookieService.GetBranch();
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Branch/UpdateBranchReceipt?BranchID=" + BranchID + "&ReceiptName=" + ReceiptName + "&ReceiptTextFooter=" + ReceiptTextFooter;
                    var result = await Client.PutAsync(url, null);
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

        [HttpPut]
        public async Task<ActionResult> UpdateQRpath([FromForm] JsonOfBranch json)
        {
            try
            {
                var value = JsonConvert.DeserializeObject<List<BranchConfig>>(json.value);
                foreach (var item in value)
                {
                    var mercahntid = _cookieService.GetMerchantid();
                    item.MerchantID = int.Parse(mercahntid);
                    item.BranchID = _cookieService.GetBranch();
                }
                var formDataContent = new MultipartFormDataContent();

                if (json.FileQRPATH != null)
                {
                    var fileStreamContent = new StreamContent(json.FileQRPATH.OpenReadStream());
                    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FileQRPATH", FileName = json.FileQRPATH.FileName };
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    formDataContent.Add(fileStreamContent);
                }
                formDataContent.Add(new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"), "value");

                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "BranchConfig";
                    var result = await Client.PutAsync(url, formDataContent);
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
                        return Ok();
                    }
                    else
                    {
                        throw new Exception(await result.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

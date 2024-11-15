
using Elfie.Serialization;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using MyMenuMerchant.Models;
using MyMenuMerchant.Utills;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using static IdentityModel.OidcConstants;
using static System.Net.WebRequestMethods;

namespace MyMenuMerchant.Controllers
{

    public class LoginController : Controller
    {
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        public LoginController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
        }

        public async Task<IActionResult> Index(bool error, string UserMessage)
        {
            try
            {

                //_cookieService.SetLanguage("Th");
                if (!string.IsNullOrEmpty(UserMessage))
                {
                    if (error)
                    {
                        TempData["UserMessage"] = new MessageViewModel() { CssClassName = "alert-danger  alert-dismissible", Title = "Error : ", Message = UserMessage };
                    }
                    else
                    {
                        TempData["UserMessage"] = new MessageViewModel() { CssClassName = "alert-success", Title = "Success : ", Message = UserMessage };
                    }
                }
                if (string.IsNullOrEmpty(_cookieService.GetLanguage())){
                    _cookieService.SetLanguage("Th");
                }
                

                var Jwt = _cookieService.GetToken();
                if (Jwt != null)
                {
                    TempData["UserMessage"] = null;
                    return RedirectToAction("LoginBranchs", new RouteValueDictionary(new { controller = "Login", action = "LoginBranchs" }));
                }
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        public IActionResult ChangeLanguage(string LanguageID)
        {
            try
            {
                _cookieService.SetLanguage(LanguageID);
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", new RouteValueDictionary(new { controller = "Login", action = "Index" }));
            }
        }
        public IActionResult ChangeLanguageMain(string LanguageID)
        {
            try
            {
                _cookieService.SetLanguage(LanguageID);
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index" }));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", new RouteValueDictionary(new { controller = "Login", action = "Index" }));
            }
        }

        public async Task<IActionResult> OTP(LoginModel value)
        {
            try
            {
                if (!string.IsNullOrEmpty(value.MoblieNumber))
                {
                    string RefOTP = await Initial(value);
                    value.RefOTP = RefOTP;
                    return View(value);
                }
                else
                {
                    if (string.IsNullOrEmpty(value.MerchantID) || string.IsNullOrEmpty(value.Username) || string.IsNullOrEmpty(value.Password))
                    {
                        return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index" }));
                    }
                    var jwt = BlueidConnect.Get_MyMenuApp(value.MerchantID, value.Username, value.Password);
                    var SetToken = _cookieService.SetToken(jwt.access_token, "password", jwt.refresh_token, jwt.expires_in);
                    if (!SetToken)
                    {
                        return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = true, UserMessage = "Set Cookies Error" }));
                    }
                    return RedirectToAction("LoginBranchs", new RouteValueDictionary(new { controller = "Login", action = "LoginBranchs" }));
                }
                return View(value);
            }
            catch (Exception ex)
            {
                if (ex.Message == "invalid_grant: invalid_username_or_password" || ex.Message == "UserPassIncorrect")
                {
                    //ไม่สามารถเข้าสู่ระบบได้ กรุณากรอกข้อมูลให้ถูกต้อง
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = true, UserMessage = "ไม่สามารถเข้าสู่ระบบได้ กรุณากรอกข้อมูลให้ถูกต้อง" }));
                }
                else if (ex.Message == "invalid_grant: User Can't Access CloudProduct" || ex.Message == "UserCanNotAccessCloudProduct")
                {
                    //ไม่สามารถเข้าสู่ระบบได้ เนื่องจากไม่มีสิทธิ์ในการใช้งาน
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = true, UserMessage = "ไม่สามารถเข้าสู่ระบบได้ เนื่องจากไม่มีสิทธิ์ในการใช้งาน" }));
                }
                else if (ex.Message == "CloudProductExpired" || ex.Message == "invalid_grant: Cloud Product Expired")
                {
                    //แพ็กเกจของคุณหมดอายุ
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = true, UserMessage = "แพ็กเกจของคุณหมดอายุ"}));
                }
                else
                {
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = true, UserMessage = ex.Message }));
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> VerifyOTP(LoginModel value)
        {
            try
            {
                var Jwt = BlueidConnect.Get_MyMenuCC();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);

                    string url = string.Empty;
                    if (value.Type == "R")
                    {
                        url = MicroServiceName.SeAuth2APIHost + "/OTP/Create";
                    }
                    else
                    {
                        url = MicroServiceName.SeAuth2APIHost + "/OTP/Login";
                    }
                    var json = JsonConvert.SerializeObject(new
                    {
                        OwnerID = value.OwnerID,
                        OTP = value.OTP,
                        RefOTP = value.RefOTP,
                        CloudProductID = 10
                    });

                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var result = await Client.PostAsync(url, httpContent);

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
                        var JWT_MyMenuDC = BlueidConnect.Get_MyMenuDC(value.OwnerID, value.OTP);
                        var SetToken = _cookieService.SetToken(JWT_MyMenuDC.access_token, "code", JWT_MyMenuDC.refresh_token, JWT_MyMenuDC.expires_in);
                        if (!SetToken)
                        {
                            return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = true, UserMessage = "Set Cookies Error" }));
                        }
                        //var OwnerAccount = await result.Content.ReadAsStringAsync();
                        //return  RedirectToAction("LoginBranchs", new RouteValueDictionary(new { controller = "Login", action = "LoginBranchs" }));

                        _cookieService.SetOwnerID(value.OwnerID);
                        return Ok();
                    }
                }

                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index" }));

            }
            catch (Exception ex)
            {
                var Message = "";
                //return RedirectToAction("OTP", new RouteValueDictionary(new { controller = "Login", action = "OTP", error = true, UserMessage = ex.Message}));
                if (ex.Message == "Password is incorrect, please try again.")
                {
                    Message = "รหัส OTP ไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง";
                }
                else if (ex.Message == "invalid_grant: invalid_username_or_password" || ex.Message == "UserPassIncorrect")
                {
                    //ไม่สามารถเข้าสู่ระบบได้ กรุณากรอกข้อมูลให้ถูกต้อง
                    Message = "ไม่สามารถเข้าสู่ระบบได้ กรุณากรอกข้อมูลให้ถูกต้อง";
                }
                else if (ex.Message == "invalid_grant: User Can't Access CloudProduct" || ex.Message == "UserCanNotAccessCloudProduct")
                {
                    //ไม่สามารถเข้าสู่ระบบได้ เนื่องจากไม่มีสิทธิ์ในการใช้งาน
                    Message = "ไม่สามารถเข้าสู่ระบบได้ เนื่องจากไม่มีสิทธิ์ในการใช้งาน";
                }
                else if (ex.Message == "CloudProductExpired" || ex.Message == "invalid_grant: Cloud Product Expired")
                {
                    //แพ็กเกจของคุณหมดอายุ
                    Message = "แพ็กเกจของคุณหมดอายุ";
                }
                else
                {
                    Message = ex.Message;
                }
                return Conflict(Message);
            }
        }

        [JWT]
        public async Task<IActionResult> LoginBranchs()
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                if (Jwt == null)
                {
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = false, UserMessage = "Session Expired" }));
                }

                var merchantid = BlueidConnect.getJWTTokenClaim(Jwt, "merchantid");
                if (!string.IsNullOrEmpty(merchantid))
                {
                    _cookieService.SetMerchantid(merchantid);
                }
                else
                {
                    return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = false, UserMessage = "Merchantid is missing" }));
                }

                //เก็บ path รูปไว้ก่อน
                var GetMerchant = await GetMenageMechant();
                if (GetMerchant.Merchant == null)
                {
                    //Create Merhcant
                    return RedirectToAction("Register", new RouteValueDictionary(new { controller = "Login", action = "Register" }));
                }

                if (GetMerchant.Merchant.FPrivacyPolicy == 'N' && GetMerchant.Merchant.FTermConditions == 'N')
                {
                    return RedirectToAction("UpdateDetailsPrivacyPolicy", new RouteValueDictionary(new { controller = "Login", action = "UpdateDetailsPrivacyPolicy" }));
                }
                _cookieService.SetLogoMerchant(GetMerchant?.Merchant?.LogoPath);

                //CloudProductLicences
                var ownerId = _cookieService.GetOwnerId();
                var Cloudproduct = await GetMymenuInfo(ownerId);
                if (!string.IsNullOrEmpty(Cloudproduct))
                {
                    TempData["Cloudproduct"] = Cloudproduct;
                }

                //แยกเคสตามการเข้าใช้งาน
                var role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
                if (role.ToLower() == "owner")
                {
                    var Branch = await GetAllBranch();
                    if (Branch.Count() == 1)
                    {
                        _cookieService.SetBranch(Branch.FirstOrDefault().BranchID);
                        return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));
                    }
                    return View(Branch);
                }
                else
                {
                    var Branch = await GetAllBranch();

                    List<Branch> lstbranch = new List<Branch>();
                    var username = BlueidConnect.getJWTTokenClaim(Jwt, "username");
                    var BranchPolicy = await GetBranchPolicy(username);

                    foreach (var branchPolicy in BranchPolicy)
                    {
                        Branch branch = new Branch()
                        {
                            BranchID = branchPolicy.BranchID,
                            BranchName = Branch.Where(x => x.BranchID == branchPolicy.BranchID).Select(x => x.BranchName).FirstOrDefault(),
                        };
                        lstbranch.Add(branch);
                    }

                    if (lstbranch.Count() == 1)
                    {
                        _cookieService.SetBranch(lstbranch.FirstOrDefault().BranchID);
                        return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));
                    }
                    return View(lstbranch);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index", error = true, UserMessage = ex.Message }));
            }
        }

        public IActionResult SelectBranch(string Branchid)
        {
            try
            {
                _cookieService.SetBranch(Branchid);
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));
            }
            catch (Exception ex)
            {
                return RedirectToAction("LoginBranchs", new RouteValueDictionary(new { controller = "Login", action = "LoginBranchs", error = true, UserMessage = ex.Message }));
            }
        }

        public async Task<List<BranchPolicy>> GetBranchPolicy(string username)
        {
            List<BranchPolicy> lstbranch;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "BranchPolicy" + "?getuserName=" + username;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        lstbranch = new List<BranchPolicy>();
                        lstbranch = JsonConvert.DeserializeObject<List<BranchPolicy>>(contents);
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
                return lstbranch;
            }
            catch (Exception)
            {
                return new List<BranchPolicy>();
            }
        }

        public async Task<List<Branch>> GetAllBranch()
        {
            List<Branch> lstbranch;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Branch";
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        lstbranch = new List<Branch>();
                        lstbranch = JsonConvert.DeserializeObject<List<Branch>>(contents);
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
                return lstbranch;
            }
            catch (Exception)
            {
                return new List<Branch>();
            }
        }

        public IActionResult Logout()
        {
            _cookieService.DeleteToken();
            return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Login", action = "Index" }));
        }

        public async Task<MerchantMenageModel> GetMenageMechant()
        {
            MerchantMenageModel merchant;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Merchant/MenageMechant";
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        merchant = new MerchantMenageModel();
                        merchant = JsonConvert.DeserializeObject<MerchantMenageModel>(contents);
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
                return merchant;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> DetailsPrivacyPolicy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        public async Task<IActionResult> Register()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        public async Task<ResultAPI> CheckSingUpOrLogin(string MoblieNumber, int CloudProductID)
        {
            try
            {
                var OwnerID = MoblieNumber.Replace("-", "");
                var jwt = BlueidConnect.Get_MyMenuCC();
                using (var httpClient = new HttpClient())
                {
                    var url = MicroServiceName.SeAuth2APIHost + "/OTP/CheckSingUpOrLoginApp?OwnerID=" + OwnerID + "&CloudProductID=" + CloudProductID;
                    httpClient.SetBearerToken(jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            return new ResultAPI(true, "NoContent");
                        }
                        return new ResultAPI(true, contents);
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
            }
            catch (Exception ex)
            {
                return new ResultAPI(false, ex.Message);
            }
        }

        public async Task<string> Initial(LoginModel value)
        {
            try
            {
                string url = string.Empty;
                value.OwnerID = value.MoblieNumber.Replace("-", "");
                var jwt = BlueidConnect.Get_MyMenuCC();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(jwt);
                    if (value.Type == "R")
                    {
                        url = MicroServiceName.SeAuth2APIHost + "/OTP/InitialCreate";
                    }
                    else
                    {
                        url = MicroServiceName.SeAuth2APIHost + "/OTP/InitialLogin";
                    }

                    var json = JsonConvert.SerializeObject(new
                    {
                        OwnerID = value.OwnerID,
                        CloudProductID = 10,
                        UDID = value.OwnerID,
                    });

                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var result = await Client.PostAsync(url, httpContent);

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

                    string RefOTP = string.Empty;
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        RefOTP = await result.Content.ReadAsStringAsync();
                    }
                    return RefOTP;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResultAPI> PostSeauthMerchant(MerchantLicenceModel value)
        {
            try
            {
                var json = JsonConvert.SerializeObject(value);
                var jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(jwt);
                    string url = MicroServiceName.SeAuth2APIHost + "/MerchantLicences";
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(content);
                        }
                    }
                    return new ResultAPI(true, content);
                }
            }
            catch (Exception ex)
            {
                ResultAPI resultAPI = new ResultAPI(false, ex.Message);
                return resultAPI;
            }
        }

        public async Task<string> GetMymenuInfo(string Ownerid)
        {
            string cloudProductLicence;
            try
            {
                string ProductName = "Mymenu";
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.SeAuth2APIHost + "/MerchantLicences/info" + "?Ownerid=" + Ownerid + "&ProductName=" + ProductName;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
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
                    else
                    {
                        cloudProductLicence = contents;
                    }
                }
                return cloudProductLicence;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResultAPI> PostPromotion(MerchantLicenceModel value)
        {
            try
            {
                MerchantLicenceModel merchantLicence;
                var json = JsonConvert.SerializeObject(value);
                var jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(jwt);
                    string url = MicroServiceName.SeAuth2APIHost + "/MerchantLicences/Promotion";
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    var content = await result.Content.ReadAsStringAsync();
                    if (!result.IsSuccessStatusCode)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized || result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception("ไม่มีสิทธิ์เข้าใช้งาน");
                        }
                        else
                        {
                            throw new Exception(content);
                        }
                    }
                    else
                    {
                        merchantLicence = new MerchantLicenceModel();
                        merchantLicence = JsonConvert.DeserializeObject<MerchantLicenceModel>(content);

                    }
                    return new ResultAPI(true, ConvertDateLocal(merchantLicence.ExpiryDate));
                }
            }
            catch (Exception ex)
            {
                return new ResultAPI(false, ex.Message);
            }
        }

        public static string ConvertDateLocal(DateTime d)
        {
            try
            {
                var timezoneslocal = TimeZoneInfo.Local;
                string datetime = TimeZoneInfo.ConvertTimeFromUtc(d, timezoneslocal).ToString("dd/MM/yyyy", new CultureInfo("en-US"));
                return datetime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return d.ToString();
            }
        }

        public async Task<IActionResult> UpdateDetailsPrivacyPolicy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateMerchantTermPolilcy(Merchant merchant)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Merchant/MerchantTermPolilcy";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(merchant, settings);
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

    }
}

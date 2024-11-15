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
using System.Globalization;
using System.Net;
using System.Text;

namespace MyMenuMerchant.Controllers
{
    [JWT]
    public class MerchantController : Controller
    {
        public string Merchant = "Merchant";
        public string Branch = "Branch";
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        private readonly string role;

        public MerchantController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }
        //view
        public async Task<IActionResult> Index()
        {
            MerchantMenageModel GetMerchant = new MerchantMenageModel();
            try
            {
                GetMerchant = await GetMenageMechant();

                //check image logo ว่าเป็นรูปเดิมหรือไม่
                var logo = _cookieService.GetLogoMerchant();
                if (GetMerchant?.Merchant?.LogoPath != logo)
                {
                    _cookieService.SetLogoMerchant(GetMerchant?.Merchant?.LogoPath);
                }

                return View(GetMerchant);
            }
            catch (Exception)
            {
                return View(GetMerchant);
            }
        }

        public IActionResult MarketInformation()
        {
            return View();
        }
        public async Task<IActionResult>  Branchs()
        {
            ViewBag.role = role;
            var Jwt = _cookieService.GetToken();
            var username = BlueidConnect.getJWTTokenClaim(Jwt, "username");
            var lstbranchpolicy = await GetBranchPolicy(username);
            ViewBag.lstbranchpolicy = lstbranchpolicy;
            return View();
        }
        public IActionResult BranchsInfo()
        {
            return View();
        }
        public IActionResult BranchsAdd()
        {
            return View();
        }
        public IActionResult BranchsEdit(string branch)
        {
            ViewBag.role = role;
            if (string.IsNullOrEmpty(branch))
            {
                return View();
            }
            else
            {
                Branch model = JsonConvert.DeserializeObject<Branch>(branch);
                return View(model);
            }
        }

        public async Task<IActionResult> Personnel()
        {
            ViewBag.role = role;
            return View();
        }

        public async Task<IActionResult> PersonnelEdit(string userName)
        {
            ViewBag.role = role;
            UserAccountResult userAccountResult = new UserAccountResult();
            var lstbranch = await GetListDatableBranch();
            ViewBag.listbranch = lstbranch;
            try
            {
                userAccountResult = await GetDataUserAccount(userName);
                return View(userAccountResult);
            }
            catch (Exception)
            {
                return View(userAccountResult);
            }
        }

        public IActionResult PersonnelAdd()
        {
            return View();
        }

        public async Task<IActionResult> PersonnelNewAdd(string userName)
        {
            useraccount result = new useraccount();
            var lstbranch = await GetListDatableBranch();
            ViewBag.listbranch = lstbranch;
            try
            {
                result = await CheckUserAccount(userName);
                result.UserName = userName;
                return View(result);
            }
            catch (Exception)
            {
                result.UserName = userName;
                var merchantId = _cookieService.GetMerchantid();
                result.MerchantID = Convert.ToInt32(merchantId);
                return View(result);
            }
        }

        public async Task<IActionResult> PersonnelOldAdd(string userName)
        {
            useraccount result = new useraccount();
            var lstbranch = await GetListDatableBranch();
            ViewBag.listbranch = lstbranch;
            try
            {
                result = await CheckUserAccount(userName);
                return View(result);
            }
            catch (Exception)
            {
                return View(result);
            }
        }

        public IActionResult Customers()
        {
            return View();
        }

        public IActionResult CustomersAdd()
        {
            return View();
        }

        public IActionResult CustomersEdit()
        {
            return View();
        }

        public async Task<IActionResult> EditProfile()
        {
            ViewBag.role = role;
            var GetMerchant = await GetMerchantDetail();
            return View(GetMerchant);
        }

        public IActionResult SetDiscounts()
        {
            ViewBag.role = role;
            return View();
        }
        public IActionResult DiscountAdd()
        {
            return View();
        }
        public IActionResult DiscountEdit(string discount)
        {
            DiscountTemplate discountTemplate = JsonConvert.DeserializeObject<DiscountTemplate>(discount);
            return View(discountTemplate);
        }

        public async Task<IActionResult> SetVat()
        {
            ViewBag.role = role;
            var GetMerchant = await GetMerchantDetail();
            return View(GetMerchant);
        }

        //api
        public async Task<string> GetDatableBranch()
        {
            var GetBranch = await GetBranchDetail();
            return JsonConvert.SerializeObject(GetBranch);
        }

        public async Task<List<Branch>> GetListDatableBranch()
        {
            var GetBranch = await GetBranchDetail();
            return GetBranch;
        }

        public async Task<string> GetDataDiscoount()
        {
            List<DiscountTemplate> discountTemplates = new List<DiscountTemplate>();
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "DiscountTemplate/Discount";
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    var result = await httpClient.GetAsync(url);
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
                        string json = await result.Content.ReadAsStringAsync();
                        if (json != null)
                        {
                            return json;
                        }
                    }
                    return null;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddDiscount(DiscountTemplate discountTemplate, string type)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                if (type == "P")
                {
                    discountTemplate.FmlDiscount += "%";
                }
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "DiscountTemplate";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(discountTemplate, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok(result.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditDiscount(DiscountTemplate discountTemplate, string type)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                if (type == "P")
                {
                    discountTemplate.FmlDiscount += "%";
                }
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "DiscountTemplate";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(discountTemplate, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PutAsync(url, httpContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        return Conflict(result.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        return Ok(result.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public async Task<ActionResult> DeleteDiscount(string id)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "DiscountTemplate" + "?discountTemplateID=" + id;

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

        public async Task<Merchant> GetMerchantDetail()
        {
            Merchant merchant;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + Merchant;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        merchant = new Merchant();
                        merchant = JsonConvert.DeserializeObject<Merchant>(contents);
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

        public async Task<List<Branch>> GetBranchDetail()
        {
            List<Branch> lstbranch;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + Branch;
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditMerchant([FromForm] JsonOfMerchants itemInfoModel)
        {
            try
            {
                var value = JsonConvert.DeserializeObject<Merchant>(itemInfoModel.value);
                var formDataContent = new MultipartFormDataContent();
                #region Current Code
                //if (itemInfoModel.FilePicture != null)
                //{
                //    var picresize = MyMenuMerchant.Utills.ManageImage.Resize(itemInfoModel.FilePicture, new System.Drawing.Size(1024,300));
                //    StreamContent fileStreamContent = new StreamContent(picresize);
                //    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FilePicture", FileName = itemInfoModel.FilePicture.FileName };
                //    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                //    formDataContent.Add(fileStreamContent);
                //}
                //if (itemInfoModel.FileLogo != null)
                //{
                //    var logoresize = MyMenuMerchant.Utills.ManageImage.Resize(itemInfoModel.FileLogo, new System.Drawing.Size(150,150));
                //    StreamContent fileStreamContent = new StreamContent(logoresize);
                //    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FileLogo", FileName = itemInfoModel.FileLogo.FileName };
                //    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                //    formDataContent.Add(fileStreamContent);
                //} 
                #endregion


                #region old Code
                if (itemInfoModel.FilePicture != null)
                {
                    //var picresize = MyMenuMerchant.Utills.ManageImage.Resize(itemInfoModel.FilePicture, new System.Drawing.Size(1024, 300));
                    //var fileStreamContent = picresize;
                    var fileStreamContent = new StreamContent(itemInfoModel.FilePicture.OpenReadStream());
                    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FilePicture", FileName = itemInfoModel.FilePicture.FileName };
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    formDataContent.Add(fileStreamContent);
                }
                if (itemInfoModel.FileLogo != null)
                {
                    //var logoresize = MyMenuMerchant.Utills.ManageImage.Resize(itemInfoModel.FileLogo, new System.Drawing.Size(150, 150));
                    //var fileStreamContent = logoresize;
                    var fileStreamContent = new StreamContent(itemInfoModel.FileLogo.OpenReadStream());
                    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "FileLogo", FileName = itemInfoModel.FileLogo.FileName };
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    formDataContent.Add(fileStreamContent);
                } 
                #endregion
                formDataContent.Add(new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json"), "value");

                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    //Client.SetBearerToken(Jwt);
                    //Client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationID);
                    string url = MicroServiceName.MyMenuAPI + "Merchant";

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

        [HttpPost]
        public async Task<ActionResult> AddBranch(Branch branch)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "branch";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(branch, settings);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = await Client.PostAsync(url, httpContent);
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
        public async Task<ActionResult> EditBranch(Branch branch)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "branch/UpdateBranchDetail";
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Include,

                        DefaultValueHandling = DefaultValueHandling.Include

                    };
                    var json = JsonConvert.SerializeObject(branch, settings);
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
        public async Task<ActionResult> EditStatusBranch(string branchid, string status)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "branch/UpdateBranchStatus?BranchID=" + branchid + "&Status=" + status;
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
        public async Task<ActionResult> DeleteBranch(string branchid)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);

                    string url = MicroServiceName.MyMenuAPI + "branch?BranchID=" + branchid;

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
        public async Task<ActionResult> EditTax(string TaxType, string TaxRate)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var Client = new HttpClient())
                {
                    Client.SetBearerToken(Jwt);
                    string url = MicroServiceName.MyMenuAPI + "Merchant/SetVat?TaxType=" + TaxType + "&TaxRate=" + TaxRate;
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

        //MymenuAPI UserAccount

        public async Task<ListUserAccount> GetListUserAccount()
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo";
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
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
                        ListUserAccount listUserAccount = new ListUserAccount();
                        listUserAccount = JsonConvert.DeserializeObject<ListUserAccount>(contents);
                        return listUserAccount;
                    }
                }
            }
            catch (Exception ex) 
            { 
                throw ex; 
            }
        }

        public async Task<useraccount> CheckUserAccount(string username)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo/CheckUserAccount" + "?username=" + username;
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
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

                        useraccount userAccountInfo = new useraccount();
                        userAccountInfo = JsonConvert.DeserializeObject<useraccount>(contents);
                        return userAccountInfo;
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        public async Task<UserAccountResult> GetDataUserAccount(string username)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo/GetDataUserAccount" + "?username=" + username;
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
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
                        UserAccountResult userAccountResult = new UserAccountResult();
                        userAccountResult = JsonConvert.DeserializeObject<UserAccountResult>(contents);
                        return userAccountResult;
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }

        public async Task<ActionResult> PostUserAccount(UserAccountResult value)
        {
            try
            {
                string json = JsonConvert.SerializeObject(value);
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo";
                    var res = await httpClient.PostAsync(url, httpContent);
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
                    return Ok(contents);
                }
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public async Task<ActionResult> PostUserAccountwithSeauth(UserAccountResult value)
        {
            try
            {
                string json = JsonConvert.SerializeObject(value);
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo/PostSeauth";
                    var res = await httpClient.PostAsync(url, httpContent);
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
                    return Ok(contents);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "UserIsMax(จำนวนบัญชีผู้ใช้เต็มแล้ว)")
                {
                    return Conflict("จำนวนบัญชีผู้ใช้เต็มแล้ว");
                }
                else
                {
                    return Conflict(ex.Message);
                }
            }
        }

        public async Task<ActionResult> PutDataUserAccount(UserAccountResult value)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(value);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo/";
                    var res = await httpClient.PutAsync(url, httpContent);
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
                    return Ok(contents);
                }
            }
            catch (WebException ex)
            {
                if (ex.Message.ToLower() == "forbidden")
                {
                    return Conflict("ไม่มีสิทธิ์ในการแก้ไขข้อมูล");
                }
                else
                {
                    return Conflict(ex.Message);
                }
            }
        }

        public async Task<ActionResult> PutChangePassword(ChangePassword value)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(value);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo/ChangePassword";
                    var res = await httpClient.PutAsync(url, httpContent);
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
                    return Ok(contents);
                }
            }
            catch (WebException ex)
            {
                if (ex.Message.ToLower() == "forbidden")
                {
                    return Conflict("ไม่มีสิทธิ์ในการแก้ไขข้อมูล");
                }
                else
                {
                    return Conflict(ex.Message);
                }
            }
        }

        public async Task<ActionResult> DeleteUserAccount(string username)
        {
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    var url = MicroServiceName.MyMenuAPI + "UserAccountInfo" + "?username=" + username;
                    httpClient.SetBearerToken(Jwt);
                    httpClient.Timeout = TimeSpan.FromMinutes(1);
                    var res = await httpClient.DeleteAsync(url);
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
                    return Ok();
                }
            }
            catch (WebException ex)
            {
                if (ex.Message.ToLower() == "forbidden")
                {
                    return Conflict("ไม่มีสิทธิ์ในการแก้ไขข้อมูล");
                }
                else
                {
                    return Conflict(ex.Message);
                }
            }
        }

        public async Task<string> GetDatableEmployee()
        {
            List<DataTableEmployee> ret = new List<DataTableEmployee>();
            try
            {
                ListUserAccount listUserAccount = new ListUserAccount();
                listUserAccount = await GetListUserAccount();

                foreach (var item in listUserAccount.userAccountInfos)
                {
                    DataTableEmployee employee = new DataTableEmployee();
                    employee.userName = item.UserName;
                    employee.nameEmployee = item.FullName;
                    employee.role = item.MainRole;
                    employee.totalBranch = item.Totalbranch;
                    employee.branchName = item.BranchPolicyName;
                    employee.comment = item.Comments;
                    ret.Add(employee);
                }
                return JsonConvert.SerializeObject(ret);
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(ret);
            }
        }

        public async Task<ActionResult> PostCheckUserName(string userName)
        {
            try
            {
                useraccount result = await CheckUserAccount(userName);
                return Ok();
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        public async Task<IActionResult> DetailsPrivacyPolicy()
        {
            try
            {
                ViewBag.role = role;
                return View();
            }
            catch (Exception ex)
            {
                return View(ex);
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
    }
}

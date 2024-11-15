using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Options;
using MyMenu.JAM;
using MyMenu.ORM.Master;
using MyMenuMerchant.Utills;
using Newtonsoft.Json;
using System.Net;

namespace MyMenuMerchant.Controllers
{
    //[JWT]
    //[Branch]
    [JWT]
    [Branch]
    public class DashboardController : Controller
    {
        private readonly ILogger<TableController> _logger;
        private readonly ICookieService _cookieService;
        private readonly MicroServiceNameModel MicroServiceName;
        public string Branchdetail = "Branch/GetBranchDetail";
        private readonly string role;

        public DashboardController(ILogger<TableController> logger, ICookieService cookieService, IOptions<MicroServiceNameModel> microservicename)
        {
            _logger = logger;
            _cookieService = cookieService;
            MicroServiceName = microservicename.Value;
            var Jwt = _cookieService.GetToken();
            role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                Branch branch = await GetBranchDetail(branchID);
                ViewBag.BranchID = branchID;
                ViewBag.BranchName = branch.BranchName;
                var jwt = _cookieService.GetToken();
                var getDashboardInfo = await GetDashBoard(branchID);
                var getDashboardInfomonth = await GetDashBoardM(branchID);
                getDashboardInfo.SummaryMonth = getDashboardInfomonth;
                return View(getDashboardInfo);
            }
			catch (Exception)
			{
                return View();
            }
        }

        public async Task<List<Branch>> GetBranch()
        {
            try
            {
                var branchID = _cookieService.GetBranch();
                //แยกเคสตามการเข้าใช้งาน
                var Jwt = _cookieService.GetToken();
                var role = BlueidConnect.getJWTTokenClaim(Jwt, "role");
                if (role.ToLower() == "owner")
                {
                    List<Branch> lstbranch = new List<Branch>();
                    lstbranch = await GetListBranch();
                    return lstbranch;
                }
                else
                {
                    var Branch = await GetListBranch();
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
                    return lstbranch;

                }
            }
            catch (Exception ex)
            {
                return new List<Branch>();
            }
        }

        public IActionResult ChangeBranch(string branchID)
        {
            try
            {
                _cookieService.SetBranch(branchID);
                return RedirectToAction("Index", new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", new RouteValueDictionary(new { controller = "Login", action = "Index"}));
            }
        }

        public async Task<DashboardInfoall> GetDashBoard(string id)
        {
            DashboardInfoall dashboardInfo;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Dashboard/DashboardInfo" + "?BranchID=" + id;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        dashboardInfo = new DashboardInfoall();
                        dashboardInfo = JsonConvert.DeserializeObject<DashboardInfoall>(contents);
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
                return dashboardInfo;
            }
            catch (Exception)
            {
                return new DashboardInfoall();
            }
        }
        public async Task<DashboardInfo> GetDashBoardW(string id)
        {
            DashboardInfo dashboardInfo;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Dashboard/DashboardInfoWeek" + "?BranchID=" + id;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        dashboardInfo = new DashboardInfo();
                        dashboardInfo = JsonConvert.DeserializeObject<DashboardInfo>(contents);
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
                return dashboardInfo;
            }
            catch (Exception)
            {
                return new DashboardInfo();
            }
        }
        public async Task<List<SumaryDaily>> GetDashBoardM(string id)
        {
            List<SumaryDaily> dashboardInfo;
            try
            {
                var Jwt = _cookieService.GetToken();
                using (var httpClient = new HttpClient())
                {
                    httpClient.SetBearerToken(Jwt);
                    var url = MicroServiceName.MyMenuAPI + "Dashboard/DashboardInfoMonth" + "?BranchID=" + id;
                    httpClient.Timeout = TimeSpan.FromMinutes(0.5);
                    var res = await httpClient.GetAsync(url);
                    var contents = await res.Content.ReadAsStringAsync();
                    if (res.IsSuccessStatusCode)
                    {
                        dashboardInfo = new List<SumaryDaily>();
                        dashboardInfo = JsonConvert.DeserializeObject<List<SumaryDaily>>(contents);
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
                return dashboardInfo;
            }
            catch (Exception)
            {
                return new List<SumaryDaily>();
            }
        }

        public async Task<List<Branch>> GetListBranch()
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

using Microsoft.AspNetCore.Authentication.Cookies;

namespace MyMenuClient.Utills
{
    public interface ICookieService
    {
        string? GetToken();
        bool SetToken(string access_token, string refresh_token, DateTime expires_in);
        string GetCookieValue(string key);
        void SetCookie(string key, string value, DateTime? expires_in);
        bool SetCustomer(string customername, string tel, DateTime? expires_in);
        bool SetClient(string merchantid, string branchid, string generatecode, DateTime? expires_in);

        bool SetMerchantandFoodTable(string foodtablename, string merchantname, string OpenOrdersID, DateTime? expires_in);

        string? GetLanguage();
        void SetLanguage(string language);

        (string? merchantid, string? branchid, string? generatecode, string? customername, string? phonenumber, string? merchantname, string? foodtablename, string? openordersid) GetClientAndCustomer();
         bool DeleteAll();
    }

    public class Cookies : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Cookies(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetToken()
        {
            try
            {
                var access_token = _httpContextAccessor.HttpContext.Request.Cookies["access_token"];
                if (string.IsNullOrEmpty(access_token))
                {
                    var refresh_token = _httpContextAccessor.HttpContext.Request.Cookies["refresh_token"];
                    if (string.IsNullOrEmpty(refresh_token))
                    {
                        return null;
                    }
                    else
                    {
                        var jwt = BlueidConnect.Refresh_MyMenuClient(refresh_token);
                        _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", jwt.access_token, new CookieOptions() { Expires = jwt.expires_in });
                        _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", jwt.refresh_token);
                        return jwt.access_token;
                    }
                }
                else
                {
                    return access_token;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SetToken(string access_token,string refresh_token, DateTime expires_in)
        {
            try
            {
                if (access_token == null || refresh_token == null || expires_in == DateTime.MinValue)
                {
                    return false;
                }

                _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", access_token, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", refresh_token);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SetClient(string merchantid, string branchid, string generatecode, DateTime? expires_in)
        {
            try
            {
                if (merchantid == null || branchid == null || generatecode == null )
                {
                    return false;
                }

                _httpContextAccessor.HttpContext.Response.Cookies.Append("merchantid", merchantid, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("branchid", branchid, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("generatecode", generatecode, new CookieOptions() { Expires = expires_in });
               


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SetMerchantandFoodTable(string foodtablename, string merchantname, string openordersid, DateTime? expires_in)
        {
            try
            {
                if (foodtablename == null || merchantname == null )
                {
                    return false;
                }

                _httpContextAccessor.HttpContext.Response.Cookies.Append("foodtablename", foodtablename, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("merchantname", merchantname, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("openordersid", openordersid, new CookieOptions() { Expires = expires_in });

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SetCustomer(string customername, string phonenumber, DateTime? expires_in)
        {
            try
            {
                if (customername == null || phonenumber == null)
                {
                    return false;
                }

                _httpContextAccessor.HttpContext.Response.Cookies.Append("customername", customername, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("phonenumber", phonenumber, new CookieOptions() { Expires = expires_in });

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetLanguage(string language)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("MyMenuClientLanguage", language);
        }

        public (string? merchantid, string? branchid, string? generatecode, string? customername, string? phonenumber,string? merchantname, string? foodtablename,string? openordersid) GetClientAndCustomer()
        {
            var merchantid = _httpContextAccessor.HttpContext.Request.Cookies["merchantid"];
            var branchid = _httpContextAccessor.HttpContext.Request.Cookies["branchid"];
            var generatecode = _httpContextAccessor.HttpContext.Request.Cookies["generatecode"];
            var customername = _httpContextAccessor.HttpContext.Request.Cookies["customername"];
            var phonenumber = _httpContextAccessor.HttpContext.Request.Cookies["phonenumber"];
            var merchantname = _httpContextAccessor.HttpContext.Request.Cookies["merchantname"];
            var foodtablename = _httpContextAccessor.HttpContext.Request.Cookies["foodtablename"];
            var openordersid = _httpContextAccessor.HttpContext.Request.Cookies["openordersid"];

            return (merchantid, branchid, generatecode, customername, phonenumber,merchantname,foodtablename, openordersid);
        }
       

        public bool DeleteAll()
        {
            try
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("merchantid");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("branchid");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("generatecode");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("customername");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("phonenumber");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("access_token");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("refresh_token");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("merchantname");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("foodtablename"); 
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("openordersid");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("MyMenuClientLanguage");
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
        public string? GetCookieValue(string key)
        {
            return _httpContextAccessor.HttpContext.Request.Cookies[key];
        }

        public void SetCookie(string key, string value, DateTime? expires_in)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, new CookieOptions { Expires = expires_in });
        }

        public string? GetLanguage()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies["MyMenuClientLanguage"];
        }
    }
}

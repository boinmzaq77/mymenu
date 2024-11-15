using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MyMenu.ORM.Master;

namespace MyMenuMerchant.Utills
{

    public interface ICookieService
    {
        string GetCookieValue(string key);
        void SetCookie(string key, string value, DateTime? expires_in);
        string? GetToken();
        bool SetToken(string access_token, string type_token, string refresh_token, DateTime expires_in);
        string? GetBranch();
        void SetBranch(string branch);
        bool DeleteToken();
        string? GetMerchantid();
        void SetMerchantid(string merchantid);

        string? GetLogoMerchant();
        bool SetLogoMerchant(string logoMerchant);

        string? GetOwnerId();
        void SetOwnerID(string OwnerId);

        string? GetLanguage();
        void SetLanguage(string language);
    }

    public class Cookies : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Cookies(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool SetToken(string access_token, string type_token, string refresh_token, DateTime expires_in)
        {
            try
            {
                if (access_token == null || type_token == null || refresh_token == null || expires_in == DateTime.MinValue)
                {
                    return false;
                }
                _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", access_token, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", access_token, new CookieOptions() { Expires = expires_in });
                _httpContextAccessor.HttpContext.Response.Cookies.Append("type_token", type_token);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", refresh_token);
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

        public string? GetToken()
        {
            try
            {
                var access_token = _httpContextAccessor.HttpContext.Request.Cookies["access_token"];
                var type_token = _httpContextAccessor.HttpContext.Request.Cookies["type_token"];
                if (string.IsNullOrEmpty(access_token))
                {
                    var refresh_token = _httpContextAccessor.HttpContext.Request.Cookies["refresh_token"];
                    if (string.IsNullOrEmpty(refresh_token))
                    {
                        return null;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(type_token))
                        {
                            return null;
                        }
                        else if (type_token == "code")
                        {
                            var jwt = BlueidConnect.Refresh_MyMenuDC(refresh_token);
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", jwt.access_token, new CookieOptions() { Expires = jwt.expires_in });
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("type_token", "code");
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", jwt.refresh_token);
                            return jwt.access_token;

                        }
                        else if (type_token == "password")
                        {
                            var jwt = BlueidConnect.Refresh_MyMenuApp(refresh_token);
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", jwt.access_token, new CookieOptions() { Expires = jwt.expires_in });
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("type_token", "password");
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", jwt.refresh_token);
                            return jwt.access_token;
                        }
                        else
                        {
                            return null;
                        }
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

        public string? GetBranch()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies["branch"];
        }

        public string? GetMerchantid()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies["merchantid"];
        }

        public string? GetLogoMerchant()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies["logopath"];
        }

        public string? GetOwnerId()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies["ownerid"];
        }

        public string? GetLanguage()
        {
            return _httpContextAccessor.HttpContext.Request.Cookies["MyMenuLanguage"];
        }

        public void SetMerchantid(string merchantid)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("merchantid", merchantid);
        }

        public void SetBranch(string branch)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("branch", branch);
        }

        public bool SetLogoMerchant(string PathLogo)
        {
            if (string.IsNullOrEmpty(PathLogo))
            {
                return false;
            }

            _httpContextAccessor.HttpContext.Response.Cookies.Append("logopath", PathLogo);

            return true;
        }

        public void SetOwnerID(string OwnerId)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("ownerid", OwnerId);
        }

        public void SetLanguage(string language)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("MyMenuLanguage", language);
        }

        public bool DeleteToken()
        {
            try
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("access_token");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("type_token");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("refresh_token");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("branch");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("merchantid");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("logopath");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("username");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("ownerid");
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("MyMenuLanguage");

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

using Microsoft.VisualBasic;
using MyMenu.JAM;
using MyMenuMerchant.Models;
using Newtonsoft.Json;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

namespace MyMenuMerchant.Utills
{
    public static class BlueidConnect
    {
        public static string SeAuth2AuthorityHost;
        static DateTime expirydate;
        static string access_token = null;
        static string expires_in = null;
        //static string token_type = null;
        //static string refresh_token = null;

        public static string Get_MyMenuCC()
        {
            try
            {
                if (expirydate <= DateTime.UtcNow)
                {
                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) =>
                    {
                        return true;
                    };

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(SeAuth2AuthorityHost + "/connect/token") as HttpWebRequest;

                    string postData;
                    var client_credentials = "client_credentials";
                    var client_secret = "SQMmfNO5yd5nLVLmx8cHMV8cD4a25QfKHxfHZMj7AzOZCWd63qbVY6UGmEObclEOXpsCnDR4B7lg7CR9YpQUmA";
                    var client_id = "MyMenuCC";
                    var scope = "seauth2.accesswithotp";

                    postData = "grant_type=" + client_credentials;
                    postData += "&client_secret=" + client_secret;
                    postData += "&client_id=" + client_id;
                    postData += "&scope=" + scope;
                    var data = Encoding.ASCII.GetBytes(postData);

                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                    httpWebRequest.ContentLength = data.Length;

                    using (var stream = httpWebRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = (HttpWebResponse)httpWebRequest.GetResponse();

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var res = JsonConvert.DeserializeObject<Dictionary<dynamic, string>>(responseString);


                    var get_access_token = res.TryGetValue("access_token", out access_token);
                    res.TryGetValue("expires_in", out expires_in);
                    //res.TryGetValue("token_type", out token_type);
                    //res.TryGetValue("refresh_token", out refresh_token);

                    if (!get_access_token)
                    {
                        throw new Exception("Get access_token is failed");
                    }

                    expirydate = DateTime.UtcNow.AddSeconds(Convert.ToInt64(expires_in)).AddMinutes(-15);

                    return access_token;
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

        public static (string? access_token, DateTime expires_in, string? refresh_token) Get_MyMenuDC(string ownerid, string delegatecode)
        {
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                 (sender, cert, chain, sslPolicyErrors) =>
                 {
                     return true;
                 };

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(SeAuth2AuthorityHost + "/connect/token") as HttpWebRequest;

                string postData;
                var client_credentials = "delegatecode";
                var client_secret = "LlBDxJ8V6Ib8lmxrUZzbd3R386iJTtpEN6dzX37YjjdkPbj5yIjwHFdTOmdxHVasxd94dvxEdezoiDNmjDpuPg";
                var client_id = "MyMenuDC";
                var scope = "offline_access seauth2.mymerchant mymenu.merchant";

                postData = "grant_type=" + client_credentials;
                postData += "&client_secret=" + client_secret;
                postData += "&client_id=" + client_id;
                postData += "&scope=" + scope;
                postData += "&ownerid=" + ownerid;
                postData += "&delegatecode=" + delegatecode;
                var data = Encoding.ASCII.GetBytes(postData);

                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.ContentLength = data.Length;

                using (var stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var res = JsonConvert.DeserializeObject<Dictionary<dynamic, string>>(responseString);

                string access_token_jwt_2 = null;
                string expires_in_jwt_2 = null;
                string refresh_token_jwt_2 = null;
                var get_access_token = res.TryGetValue("access_token", out access_token_jwt_2);
                res.TryGetValue("expires_in", out expires_in_jwt_2);
                //res.TryGetValue("token_type", out token_type);
                res.TryGetValue("refresh_token", out refresh_token_jwt_2);

                if (!get_access_token)
                {
                    throw new Exception("Get access_token is failed");
                }

                var expirydate = DateTime.UtcNow.AddSeconds(Convert.ToInt64(expires_in)).AddMinutes(-15);

                return (access_token_jwt_2, expirydate, refresh_token_jwt_2);

            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    if (errorResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        using (Stream errorStream = errorResponse.GetResponseStream())
                        using (StreamReader reader = new StreamReader(errorStream))
                        {
                            string errorContent = reader.ReadToEnd();
                            var error = JsonConvert.DeserializeObject<ErrorgetTokenModel>(errorContent);
                            string ShowError = ErrorGetToken.CheckErrorGetToken(error.error_description);
                            throw new Exception(ShowError);
                        }
                    }
                    throw ex;
                }
                else
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static (string? access_token, DateTime expires_in, string? refresh_token) Refresh_MyMenuDC(string refresh_token)
        {
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                 (sender, cert, chain, sslPolicyErrors) =>
                 {
                     return true;
                 };

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(SeAuth2AuthorityHost + "/connect/token") as HttpWebRequest;

                string postData;
                var client_credentials = "refresh_token";
                var client_secret = "LlBDxJ8V6Ib8lmxrUZzbd3R386iJTtpEN6dzX37YjjdkPbj5yIjwHFdTOmdxHVasxd94dvxEdezoiDNmjDpuPg";
                var client_id = "MyMenuDC";
                var scope = "offline_access seauth2.mymerchant mymenu.merchant";

                postData = "grant_type=" + client_credentials;
                postData += "&client_secret=" + client_secret;
                postData += "&client_id=" + client_id;
                postData += "&refresh_token=" + refresh_token;

                var data = Encoding.ASCII.GetBytes(postData);

                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.ContentLength = data.Length;

                using (var stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var res = JsonConvert.DeserializeObject<Dictionary<dynamic, string>>(responseString);

                string access_token_jwt_2 = null;
                string expires_in_jwt_2 = null;
                string refresh_token_jwt_2 = null;
                var get_access_token = res.TryGetValue("access_token", out access_token_jwt_2);
                res.TryGetValue("expires_in", out expires_in_jwt_2);
                //res.TryGetValue("token_type", out token_type);
                res.TryGetValue("refresh_token", out refresh_token_jwt_2);

                if (!get_access_token)
                {
                    throw new Exception("Get access_token is failed");
                }

                var expirydate = DateTime.UtcNow.AddSeconds(Convert.ToInt64(expires_in_jwt_2)).AddMinutes(-15);

                return (access_token_jwt_2, expirydate, refresh_token_jwt_2);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static (string? access_token, DateTime expires_in, string? refresh_token) Get_MyMenuApp(string merchantid, string username , string password)
        {
            

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                 (sender, cert, chain, sslPolicyErrors) =>
                 {
                     return true;
                 };

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(SeAuth2AuthorityHost + "/connect/token") as HttpWebRequest;
                string postData;
                var client_credentials = "password";
                var client_secret = "bccIbmc1WIwbjtzNoKjHUh15Aldmm7Yp3Al2dtMJIhfgE6qgHC1mUnikBgwpWNkhkHi6jmUWxWdhzC2g1uIEYg";
                var client_id = "MyMenuApp";
                var scope = "offline_access seauth2.mymerchant mymenu.merchant";

                postData = "grant_type=" + client_credentials;
                postData += "&client_secret=" + client_secret;
                postData += "&client_id=" + client_id;
                postData += "&scope=" + scope;
                postData += "&merchantid=" + merchantid;
                postData += "&username=" + username;
                postData += "&password=" + password;
                var data = Encoding.ASCII.GetBytes(postData);

                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.ContentLength = data.Length;

                using (var stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var res = JsonConvert.DeserializeObject<Dictionary<dynamic, string>>(responseString);

                string access_token_jwt_2 = null;
                string expires_in_jwt_2 = null;
                string refresh_token_jwt_2 = null;
                var get_access_token = res.TryGetValue("access_token", out access_token_jwt_2);
                res.TryGetValue("expires_in", out expires_in_jwt_2);
                //res.TryGetValue("token_type", out token_type);
                res.TryGetValue("refresh_token", out refresh_token_jwt_2);

                if (!get_access_token)
                {
                    throw new Exception("Get access_token is failed");
                }

                var expirydate = DateTime.UtcNow.AddSeconds(Convert.ToInt64(expires_in_jwt_2)).AddMinutes(-15);

                return (access_token_jwt_2, expirydate, refresh_token_jwt_2);

            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    if (errorResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        using (Stream errorStream = errorResponse.GetResponseStream())
                        using (StreamReader reader = new StreamReader(errorStream))
                        {
                            string errorContent = reader.ReadToEnd();
                            var error = JsonConvert.DeserializeObject<ErrorgetTokenModel>(errorContent);
                            string ShowError = ErrorGetToken.CheckErrorGetToken(error.error_description);
                            throw new Exception(ShowError);
                        }
                    }
                    throw ex;
                }
                else
                {
                    throw ex;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static (string? access_token, DateTime expires_in, string? refresh_token) Refresh_MyMenuApp(string refresh_token)
        {
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                 (sender, cert, chain, sslPolicyErrors) =>
                 {
                     return true;
                 };

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(SeAuth2AuthorityHost + "/connect/token") as HttpWebRequest;

                string postData;
                var client_credentials = "refresh_token";
                var client_secret = "bccIbmc1WIwbjtzNoKjHUh15Aldmm7Yp3Al2dtMJIhfgE6qgHC1mUnikBgwpWNkhkHi6jmUWxWdhzC2g1uIEYg";
                var client_id = "MyMenuApp";
                var scope = "offline_access seauth2.mymerchant mymenu.merchant";

                postData = "grant_type=" + client_credentials;
                postData += "&client_secret=" + client_secret;
                postData += "&client_id=" + client_id;
                postData += "&refresh_token=" + refresh_token;
             
                var data = Encoding.ASCII.GetBytes(postData);

                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.ContentLength = data.Length;

                using (var stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)httpWebRequest.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var res = JsonConvert.DeserializeObject<Dictionary<dynamic, string>>(responseString);

                string access_token_jwt_2 = null;
                string expires_in_jwt_2 = null;
                string refresh_token_jwt_2 = null;
                var get_access_token = res.TryGetValue("access_token", out access_token_jwt_2);
                res.TryGetValue("expires_in", out expires_in_jwt_2);
                //res.TryGetValue("token_type", out token_type);
                res.TryGetValue("refresh_token", out refresh_token_jwt_2);

                if (!get_access_token)
                {
                    throw new Exception("Get access_token is failed");
                }

                var expirydate = DateTime.UtcNow.AddSeconds(Convert.ToInt64(expires_in_jwt_2)).AddMinutes(-15);

                return (access_token_jwt_2, expirydate, refresh_token_jwt_2);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string getJWTTokenClaim(string Jwt, string claimName)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(Jwt);
                string claimValue = token.Claims.OrderByDescending(x=>x.Value).FirstOrDefault(claim => claim.Type == claimName)?.Value;
                return claimValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}

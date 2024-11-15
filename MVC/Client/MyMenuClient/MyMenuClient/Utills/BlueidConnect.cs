using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MyMenuClient.Utills
{
    public static class BlueidConnect
    {
        public static string SeAuth2AuthorityHost;
        static DateTime expirydate;
        static string access_token = null;

        public static string Get_MyMenuCC()
        {
            try
            {
                if (expirydate <= DateTime.UtcNow)
                {
                    string expires_in = null;

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

        public static (string? access_token, DateTime expires_in, string? refresh_token) Get_MyMenuClient(string merchantid, string botsecret)
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
                var client_credentials = "botsecretcode";
                var client_secret = "tmrMyjaUe9jNdRvA4wFME0dJUKBY9qtXxO8hzMoS0Pb11xnbeDgHIL5pAC47oGDqdwYSDIyfBdKPMzkrLAkJiw";
                var client_id = "MyMenuClient";
                var scope = "offline_access mymenu.client";
                var botroleid = "MyMenuClient";

                postData = "grant_type=" + client_credentials;
                postData += "&client_secret=" + client_secret;
                postData += "&client_id=" + client_id;
                postData += "&scope=" + scope;
                postData += "&merchantid=" + merchantid;
                postData += "&botroleid=" + botroleid;
                postData += "&botsecret=" + botsecret;
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

        public static (string? access_token, DateTime expires_in, string? refresh_token) Refresh_MyMenuClient(string refresh_token)
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
                var client_secret = "tmrMyjaUe9jNdRvA4wFME0dJUKBY9qtXxO8hzMoS0Pb11xnbeDgHIL5pAC47oGDqdwYSDIyfBdKPMzkrLAkJiw";
                var client_id = "MyMenuClient";

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



    }
}

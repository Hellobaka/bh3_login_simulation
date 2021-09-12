using CustomGacha.SDK.Tool.Http;
using System;
using System.Collections.Generic;

namespace BH3_QRCodeScanner
{
    public static class Helper
    {
        public static Dictionary<string, RoleData> Success_Role = new Dictionary<string, RoleData>();
        public static HttpWebClient GetCommonHttp(bool islogin = true)
        {
            HttpWebClient http = new HttpWebClient();
            http.Encoding = System.Text.Encoding.UTF8;
            if(islogin)
            {
                http.Headers["User-Agent"] = "Mozilla/5.0 BSGameSDK";
                http.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                http.Headers["Host"] = "line1-sdk-center-login-sh.biligame.net";
            }
            else
            {
                http.Headers["accept"] = "*/*";
                http.KeepAlive = true;
            }
            return http;
        }
        public static long TimeStamp => (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        public static long TimeStampMs => (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        public static Dictionary<string, string> GetURLQuery(string url)
        {
            var t = url.Substring(url.IndexOf('?')+1).Split('&');
            Dictionary<string, string> query = new Dictionary<string, string>();
            foreach(var item in t)
            {
                var c = item.Split('=');
                query.Add(c[0], c[1]);
            }
            return query;
        }
    }
}

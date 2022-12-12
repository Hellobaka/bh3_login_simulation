using System;
using System.Collections.Generic;
using System.Net;

namespace BH3_QRCodeScanner
{
    public static class Helper
    {
        public static string BH3Ver { get; set; } = "";
        public static Dictionary<string, RoleData> Success_Role { get; set; } = new Dictionary<string, RoleData>();
        public static WebClient GetCommonHttp(bool islogin = true)
        {
            WebClient http = new WebClient();
            http.Encoding = System.Text.Encoding.UTF8;
            if (islogin)
            {
                http.Headers["User-Agent"] = "Mozilla/5.0 BSGameSDK";
                http.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                http.Headers["Host"] = "line1-sdk-center-login-sh.biligame.net";
            }
            else
            {
                http.Headers["accept"] = "*/*";
            }
            return http;
        }
        public static long TimeStamp => (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        public static long TimeStampMs => (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        public static Dictionary<string, string> GetURLQuery(string url)
        {
            var t = url.Substring(url.IndexOf('?') + 1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> query = new Dictionary<string, string>();
            foreach (var item in t)
            {
                var c = item.Split('=');
                query.Add(c[0], c[1]);
            }
            return query;
        }

        public static Action<string, string> LogMethod { get; set; } = (a, b) =>
        {
            Console.WriteLine($"{a}: {b}");
        };

        public static void Log(string source, string content)
        {
            LogMethod.Invoke(source, content);
        }
    }
}

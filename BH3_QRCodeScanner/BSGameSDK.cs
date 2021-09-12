using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH3_QRCodeScanner
{
    //https://github.com/cc004/pcrjjc2/blob/main/bsgamesdk.py
    public class BSGameSDK
    {
        string Account { get; set; }
        string Password { get; set; }

        const string bililogin = "https://line1-sdk-center-login-sh.biligame.net/";
        object Captcha_Lock = new object();
        public BSGameSDK(string account, string password)
        {
            Account = account;
            Password = password;
        }
        public JObject Login()
        {
            JObject data = Login_NoCaptcha();
            if(data.ContainsKey("access_key") == false)
            {
                Console.WriteLine("Need Captcha");
                var cap = Captcha();
                string captchaR = DoCaptcha(cap["gt"].ToString(), cap["challenge"].ToString(), cap["gt_user_id"].ToString());
                data = Login_WithCaptcha(cap["challenge"].ToString(), cap["gt_user_id"].ToString(), captchaR);
            }
            return data;
        }
        public string DoCaptcha(string gt, string challenge, string userid)
        {
            lock (Captcha_Lock)
            {
                Console.WriteLine("Paste the validate=xxxx");
                string url = $"https://help.tencentbot.top/geetest/?captcha_type=1&challenge={challenge}&gt={gt}&userid={userid}&gs=1";
                Process.Start(url);
                return Console.ReadLine();
            }
        }
        public JObject Login_WithCaptcha(string challenge, string userid, string captchaResult)
        {
            JObject data = JObject.Parse(File.ReadAllText("modolrsa_B.json"));

            string query = Encrypt.Login_SetSign(data);
            var http = Helper.GetCommonHttp();
            string t = http.UploadString(bililogin + "api/client/rsa", query);
            http.Dispose();
            var rsa = JObject.Parse(t);
            data = JObject.Parse(File.ReadAllText("modollogin_B.json"));

            string public_key = rsa.ContainsKey("rsa_key") ? rsa["rsa_key"].ToString() : "";
            string hash = rsa.ContainsKey("hash") ? rsa["hash"].ToString() : "";
            data["gt_user_id"] = userid;
            data["validate"] = "";
            data["challenge"] = challenge;
            data["user_id"] = Account;
            data["validate"] = captchaResult;
            data["seccode"] = captchaResult + "|jordan";
            data["pwd"] = Encrypt.RSAEncrypt(hash + Password, public_key);
            query = Encrypt.Login_SetSign(data);
            http = Helper.GetCommonHttp();
            return JObject.Parse(http.UploadString(bililogin + "api/client/login", query));
        }
        public JObject Captcha()
        {
            JObject data = JObject.Parse(File.ReadAllText("modolcaptcha.json"));
            string query = Encrypt.Login_SetSign(data);
            using (var http = Helper.GetCommonHttp())
                return JObject.Parse(http.UploadString(bililogin + "api/client/start_captcha", query));
        }
        public JObject Login_NoCaptcha()
        {
            JObject data = JObject.Parse(File.ReadAllText("modolrsa_B.json"));

            string query = Encrypt.Login_SetSign(data);
            string t = "";
            using (var http = Helper.GetCommonHttp())
                t = http.UploadString(bililogin + "api/client/rsa", query);
            var rsa = JObject.Parse(t);
            if (rsa["code"].ToString() != "0")
            {
                throw new Exception($"Login_NoCaptcha Error ,msg = {rsa["message"]}");
            }
            data = JObject.Parse(File.ReadAllText("modollogin_B.json"));

            string public_key = rsa.ContainsKey("rsa_key") ? rsa["rsa_key"].ToString() : "";
            string hash = rsa.ContainsKey("hash") ? rsa["hash"].ToString() : "";
            data["gt_user_id"] = "";
            data["validate"] = "";
            data["challenge"] = "";
            data["user_id"] = Account;
            data["pwd"] = Encrypt.RSAEncrypt(hash + Password, public_key);
            query = Encrypt.Login_SetSign(data);
            using (var http = Helper.GetCommonHttp())
                return JObject.Parse(http.UploadString(bililogin + "api/client/login", query));
        }
    }
}

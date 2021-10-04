using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace BH3_QRCodeScanner
{
    //https://github.com/cc004/pcrjjc2/blob/main/bsgamesdk.py
    /// <summary>
    /// Bilibili游戏登录SDK
    /// </summary>
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
        /// <summary>
        /// 登录Bilibili账号
        /// </summary>
        /// <returns>结果Json</returns>
        public JObject Login()
        {
            JObject data = Login_NoCaptcha();//尝试无验证码登录
            if (data["code"].ToString() == "500002")
            {
                return new JObject 
                {
                    {"code", "500002" },
                    {"msg", "账号或密码错误" }
                };
            }
            else if (data.ContainsKey("access_key") is false)
            {
                return new JObject
                {
                    {"code", data["code"] },
                    {"msg", data["msg"] }
                };
            }
            else
            {
                return data;
            }
        }
        [Obsolete("此函数只在测试时使用, 用于在控制台进行二维码验证")]
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
        /// <summary>
        /// 进行带验证码的登录
        /// </summary>
        public JObject Login_WithCaptcha(string challenge, string userid, string captchaResult)
        {
            JObject data = JObject.Parse(Resource_Json.modolrsa_B);

            string query = Encrypt.Login_SetSign(data);
            var http = Helper.GetCommonHttp();
            string t = http.UploadString(bililogin + "api/client/rsa", query);
            http.Dispose();
            var rsa = JObject.Parse(t);
            data = JObject.Parse(Resource_Json.modollogin_B);

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
        /// <summary>
        /// 获取验证码信息
        /// </summary>
        public JObject Captcha()
        {
            JObject data = JObject.Parse(Resource_Json.modolcaptcha_B);
            string query = Encrypt.Login_SetSign(data);
            using (var http = Helper.GetCommonHttp())
                return JObject.Parse(http.UploadString(bililogin + "api/client/start_captcha", query));
        }
        /// <summary>
        /// 尝试无验证码登录
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public JObject Login_NoCaptcha()
        {
            JObject data = JObject.Parse(Resource_Json.modolrsa_B);

            string query = Encrypt.Login_SetSign(data);
            string t = "";
            using (var http = Helper.GetCommonHttp())
                t = http.UploadString(bililogin + "api/client/rsa", query);
            var rsa = JObject.Parse(t);
            if (rsa["code"].ToString() != "0")
            {
                throw new Exception($"Login_NoCaptcha Error ,msg = {rsa["message"]}");
            }
            data = JObject.Parse(Resource_Json.modollogin_B);

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

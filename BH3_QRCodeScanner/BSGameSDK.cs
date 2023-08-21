using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading;

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
            else if (data["code"].ToString() == "200000")
            {
                return Login_WithAutoCaptcha();
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

        public JObject Login_WithAutoCaptcha()
        {
            LogHelper.Info("自动验证码流程", "开始");
            var captcha = Captcha();
            if (captcha == null || captcha.ContainsKey("gt") is false)
            {
                LogHelper.Info("自动验证码流程", "拉取验证码失败");
                return null;
            }
            if (!HandleCapture(captcha["challenge"].ToString(), captcha["gt"].ToString(), captcha["gt_user_id"].ToString(), out string challenage, out string gt, out string validate, out string err))
            {
                LogHelper.Info("自动验证码流程", $"自动验证码失败: {err}");
                return new JObject
                {
                    {"code", "200000" },
                    {"msg", err }
                };
            }
            LogHelper.Info("自动验证码流程", $"自动验证码成功，开始登录...");
            JObject data = JObject.Parse(Resource_Json.modolrsa_B);

            string query = Encrypt.Login_SetSign(data);
            var http = Helper.GetCommonHttp();
            string t = http.UploadString(bililogin + "api/client/rsa", query);
            http.Dispose();
            var rsa = JObject.Parse(t);
            data = JObject.Parse(Resource_Json.modollogin_B);

            string public_key = rsa.ContainsKey("rsa_key") ? rsa["rsa_key"].ToString() : "";
            string hash = rsa.ContainsKey("hash") ? rsa["hash"].ToString() : "";
            data["gt_user_id"] = gt;
            data["validate"] = "";
            data["challenge"] = challenage;
            data["user_id"] = Account;
            data["validate"] = validate;
            data["seccode"] = validate + "|jordan";
            data["pwd"] = Encrypt.RSAEncrypt(hash + Password, public_key);
            query = Encrypt.Login_SetSign(data);
            http = Helper.GetCommonHttp(true);
            return JObject.Parse(http.UploadString(bililogin + "api/client/login", query));
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
            data["access_key"] = "";
            data["gt_user_id"] = "";
            data["uid"] = "";
            data["validate"] = "";
            data["challenge"] = "";
            data["user_id"] = Account;
            data["pwd"] = Encrypt.RSAEncrypt(hash + Password, public_key);
            query = Encrypt.Login_SetSign(data);
            using (var http = Helper.GetCommonHttp())
                return JObject.Parse(http.UploadString(bililogin + "api/client/login", query));
        }

        public bool HandleCapture(string challenge, string gt, string uid,
           out string challenge_Captcha, out string gt_Captcha, out string validate, out string msg)
        {
            validate = "";
            msg = "";
            challenge_Captcha = "";
            gt_Captcha = "";

            string queueAPI = $"https://pcrd.tencentbot.top/geetest_renew?captcha_type=1&challenge={challenge}&gt={gt}&userid={uid}&gs=1";
            using (var http = Helper.GetCommonHttp(false))
            {
                // http.Headers.Add("User-Agent", "pcrjjc2/1.0.0");// sorry
                string uuid = "";

                var queue = JObject.Parse(http.DownloadString(queueAPI));
                if (!queue.ContainsKey("uuid"))
                {
                    LogHelper.Info("自动验证码流程", "排队失败，UUID为空");
                    msg = "UUID 为空";
                    return false;
                }
                LogHelper.Info("自动验证码流程", $"排队成功");
                uuid = queue["uuid"].ToString();
                string resultAPI = $"https://pcrd.tencentbot.top/check/{uuid}";
                for (int i = 0; i < 5; i++)
                {
                    var result = JObject.Parse(http.DownloadString(resultAPI));
                    if (result.ContainsKey("queue_num"))
                    {
                        int queueNum = result["queue_num"].ToObject<int>();
                        LogHelper.Info("自动验证码流程", $"Queue_Num={queueNum}");
                        if (queueNum >= 1)
                        {
                            Thread.Sleep(3000);
                            continue;
                        }
                    }
                    else
                    {
                        if(!result.ContainsKey("info"))
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        try
                        {
                            if (result["info"] is JProperty)
                            {
                                msg = result["info"].ToString();
                                if (msg == "in running")
                                {
                                    Thread.Sleep(6000);
                                    continue;
                                }
                                return false;
                            }
                            var info = result["info"].ToObject<JObject>();
                            if (info.ContainsKey("validate"))
                            {
                                validate = info["validate"].ToString();
                                challenge_Captcha = info["challenge"].ToString();
                                gt_Captcha = info["gt_user_id"].ToString();
                                return true;
                            }
                            else
                            {
                                msg = "过验证码失败";
                                return false;
                            }
                        }
                        catch
                        {
                            LogHelper.Info("自动验证码流程", $"result={result}");
                            Thread.Sleep(3000);
                        }                        
                    }
                }
                msg = "过验证码失败";
                return false;
            }
        }
    }
}

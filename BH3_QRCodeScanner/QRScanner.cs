using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using ZXing;

namespace BH3_QRCodeScanner
{
    /// <summary>
    /// 描述扫码登录的类
    /// </summary>
    public class QRScanner
    {
        const string Channel_id = "14";
        const string Device_ID = "2e7793608ac77ee7";
        string UID { get; set; }
        string Access_Key { get; set; }
        string Ticket { get; set; }
        string App_id { get; set; }
        string Biz_key { get; set; }

        public QRScanner(string uID, string access_Key)
        {
            UID = uID;
            Access_Key = access_Key;
        }
        /// <summary>
        /// 渠道登录, 获取崩坏三账号信息
        /// </summary>
        public void Bili_Login()
        {
            JObject c = AccountVerify(Channel_id);
            if (c == null)
            {
                Helper.Log("账号验证", "AccountVerify return null");
            }
            else if (c["retcode"].ToString() != "0")
            {
                Helper.Log("账号验证", $"AccountVerify return invalid, msg: {c["message"]}");
            }
            else
            {
                Helper.Log("账号验证", "账号验证成功");
                string combo_id = c["data"]["combo_id"].ToString();
                string open_id = c["data"]["open_id"].ToString();
                string combo_token = c["data"]["combo_token"].ToString();
                if (Helper.Success_Role.ContainsKey(UID))
                    Helper.Success_Role[UID] = new RoleData(open_id, "", combo_id, combo_token, Channel_id, "2", "bilibili", 0);
                else
                    Helper.Success_Role.Add(UID, new RoleData(open_id, "", combo_id, combo_token, Channel_id, "2", "bilibili", 0));
            }
        }
        /// <summary>
        /// 使用bilibili登录后获取到的Access_Key进行崩坏三登录
        /// </summary>
        public JObject AccountVerify(string channel_id)
        {
            string json_data = new JObject
            {
                {"uid", Convert.ToInt32(UID) },
                {"access_key", Access_Key },
            }.ToString(Formatting.None);
            JObject json = new JObject
            {
                {"app_id", "1" },
                {"channel_id", channel_id },
                {"data", json_data },
                {"device", Device_ID },
            };
            json.Add("sign", Encrypt.Scan_bh3sign(json));
            using (var http = Helper.GetCommonHttp(false))
            {
                return JObject.Parse(http.UploadString("https://api-sdk.mihoyo.com/bh3_cn/combo/granter/login/v2/login", json.ToString(Formatting.None)));
            }
        }
        /// <summary>
        /// 获取二维码内容
        /// </summary>
        /// <param name="path">图片路径</param>
        public static string ScanQRCode(string path) => ScanQRCode((Bitmap)Image.FromFile(path));
        /// <summary>
        /// 获取二维码内容
        /// </summary>
        /// <param name="qr">图片对象</param>
        public static string ScanQRCode(Bitmap qr)
        {
            ZXing.Windows.Compatibility.BarcodeReader reader = new ZXing.Windows.Compatibility.BarcodeReader();
            var result = reader.Decode(qr);
            return result != null ? result.Text : string.Empty;
        }
        /// <summary>
        /// 拆解二维码登录网址参数列表
        /// </summary>
        /// <param name="url">登录网址</param>
        public bool ParseURL(string url)
        {
            var query = Helper.GetURLQuery(url);
            try
            {
                Ticket = query["ticket"];
                App_id = query["app_id"];
                Biz_key = query["biz_key"];
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
        /// <summary>
        /// 参数完整后, 进行最终二维码登录
        /// </summary>
        /// <returns></returns>
        public JObject DoQRLogin()
        {
            if (string.IsNullOrWhiteSpace(Ticket) || string.IsNullOrWhiteSpace(App_id) || string.IsNullOrWhiteSpace(Biz_key))
                return null;
            JObject data_Base = new JObject
            {
                {"app_id" ,App_id},
                {"device" , Device_ID},
                {"ticket" ,Ticket},
                {"ts" , Helper.TimeStampMs},
            };
            JObject data = new JObject();
            foreach (var item in data_Base)
                data.Add(item.Key, item.Value);
            data.Add("sign", Encrypt.Scan_bh3sign(data));
            using(var http = Helper.GetCommonHttp(false))
            {
                string url = $"https://api-sdk.mihoyo.com/{Biz_key}/combo/panda/qrcode/scan";
                data = JObject.Parse(http.UploadString(url, data.ToString()));
            }
            if(data["retcode"].ToString() != "0")
            {
                Helper.Log("扫码登录", $"扫码有误，msg = {(data.ContainsKey("message")? data["message"]: "已过期")}");
                return data;
            }
            else
            {
                data = GenRequest(data_Base);
                using(var http = Helper.GetCommonHttp(false))
                {
                    string url = $"https://api-sdk.mihoyo.com/{Biz_key}/combo/panda/qrcode/confirm";
                    data = JObject.Parse(http.UploadString(url, data.ToString(Formatting.None)));
                    if (data["retcode"].ToString() != "0")
                        Helper.Log("扫码登录", $"扫码错误，返回值不为0, msg = {(data.ContainsKey("message") ? data["message"] : "已过期")}");
                    else
                        http.UploadString("https://service-beurmroh-1256541670.sh.apigw.tencentcs.com/succeed", "");
                    return data;
                }
            }
        }
        /// <summary>
        /// 最终上报时的请求体, 昵称可自定义显示
        /// </summary>
        private JObject GenRequest(JObject qr_check)
        {
            RoleData role = Helper.Success_Role[UID];
            JObject raw_json = new JObject() 
            {
                {"heartbeat", false },
                {"open_id", role.open_id },
                {"device_id", Device_ID },
                {"app_id", App_id },
                {"channel_id", role.channel_id },
                {"combo_token", role.combo_token },
                {"asterisk_name", "@水银之翼Bot" },
                {"combo_id", role.combo_id },
                {"account_type", role.account_type },
            };
            if(!string.IsNullOrWhiteSpace(role.open_id))
            {
                raw_json.Add("open_token", role.open_token);
                raw_json.Add("guest", false);
            }
            JObject data_json = new JObject()
            {
                {"accountType", role.accountType },
                {"accountID", role.open_id },
                {"accountToken", role.combo_token },
                {"dispatch", role.oaserver },
            };
            JObject ext_json = new JObject { {"data", data_json } };
            JObject payload_json = new JObject
            {
                {"raw", raw_json.ToString(Formatting.None) },
                {"proto", "Combo" },
                {"ext", ext_json.ToString(Formatting.None) }
            };
            JObject confirm_json = new JObject
            {
                {"device", Device_ID },
                {"app_id", App_id },
                {"ts", Helper.TimeStampMs },
                {"ticket", Ticket },
                {"payload", payload_json },
            };
            qr_check.Add("payload", payload_json);
            confirm_json.Add("sign", Encrypt.Scan_bh3sign(qr_check));
            return confirm_json;
        }
    }
}

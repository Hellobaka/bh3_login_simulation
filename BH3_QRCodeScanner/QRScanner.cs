using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace BH3_QRCodeScanner
{
    public class QRScanner
    {
        const string Channel_id = "14";
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
        public void Bili_Login()
        {
            JObject c = AccountVerify(Channel_id);
            if (c == null)
            {
                throw new WebException("AccountVerify return null");
            }
            else if (c["retcode"].ToString() != "0")
            {
                throw new WebException($"AccountVerify return invalid, msg: {c["message"]}");
            }
            else
            {
                Console.WriteLine($"账号验证成功");
                string combo_id = c["data"]["combo_id"].ToString();
                string open_id = c["data"]["open_id"].ToString();
                string combo_token = c["data"]["combo_token"].ToString();
                if (Helper.Success_Role.ContainsKey(UID))
                    Helper.Success_Role[UID] = new RoleData(open_id, "", combo_id, combo_token, Channel_id, "2", "bilibili", 0);
                else
                    Helper.Success_Role.Add(UID, new RoleData(open_id, "", combo_id, combo_token, Channel_id, "2", "bilibili", 0));
            }
        }
        public JObject AccountVerify(string channel_id)
        {
            string json_data = new JObject
            {
                {"uid", UID },
                {"access_key", Access_Key },
            }.ToString();
            JObject json = new JObject
            {
                {"device", "" },
                {"app_id", "1" },
                {"channel_id", channel_id },
                {"data", json_data },
            };
            json.Add("sign", Encrypt.Scan_bh3sign(json));
            using (var http = Helper.GetCommonHttp())
            {
                return JObject.Parse(http.UploadString("https://api-sdk.mihoyo.com/bh3_cn/combo/granter/login/v2/login", json.ToString()));
            }
        }
        public static string ScanQRCode(string path) => ScanQRCode((Bitmap)Image.FromFile(path));
        public static string ScanQRCode(Bitmap qr)
        {
            IBarcodeReader reader = new BarcodeReader();
            var result = reader.Decode(qr);
            return result != null ? result.Text : string.Empty;
        }
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
                throw new KeyNotFoundException("请扫描正确的二维码");
            }
        }
        public JObject DoQRLogin()
        {
            if (string.IsNullOrWhiteSpace(Ticket) || string.IsNullOrWhiteSpace(App_id) || string.IsNullOrWhiteSpace(Biz_key))
                return null;
            JObject data_Base = new JObject
            {
                {"app_id" ,App_id},
                {"device" ,""},
                {"ticket" ,Ticket},
                {"ts" , Helper.TimeStampMs},
            };
            JObject data = new JObject();
            foreach (var item in data_Base)
                data.Add(item.Key, item.Value);
            data.Add("sign", Encrypt.Scan_bh3sign(data));
            using(var http = Helper.GetCommonHttp())
            {
                string url = $"https://api-sdk.mihoyo.com/{Biz_key}/combo/panda/qrcode/scan";
                data = JObject.Parse(http.UploadString(url, data.ToString()));
            }
            if(data["retcode"].ToString() != "0")
            {
                Console.WriteLine($"扫码有误，msg = {(data.ContainsKey("message")? data["message"]: "已过期")}");
                return data;
            }
            else
            {
                data = GenRequest(data_Base);
                using(var http = Helper.GetCommonHttp())
                {
                    string url = $"https://api-sdk.mihoyo.com/{Biz_key}/combo/panda/qrcode/confirm";
                    data = JObject.Parse(http.UploadString(url, data.ToString()));
                    if (data["retcode"].ToString() != "0")
                    {
                        Console.WriteLine("扫码错误");
                        throw new Exception($"扫码错误，返回值不为0, msg = {(data.ContainsKey("message") ? data["message"] : "已过期")}");
                    }
                    else
                    {
                        http.UploadString("https://service-beurmroh-1256541670.sh.apigw.tencentcs.com/succeed", "");
                        return data;
                    }
                }
            }
        }

        private JObject GenRequest(JObject qr_check)
        {
            RoleData role = Helper.Success_Role[UID];
            JObject raw_json = new JObject() 
            {
                {"heartbeat", false },
                {"open_id", role.open_id },
                {"device_id", "" },
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
            JObject dispatch_json = new JObject()
            {
                {"account_url",  role.oaserver["account_url"].ToString()},
                {"account_url_backup",  role.oaserver["account_url_backup"].ToString()},
                {"asset_boundle_url",  role.oaserver["asset_boundle_url"].ToString()},
                {"ex_resource_url",  role.oaserver["ex_resource_url"].ToString()},
                {"ext",  role.oaserver["ext"]},
                {"gameserver",  role.oaserver["gameserver"]},
                {"gateway",  role.oaserver["gateway"]},
                {"oaserver_url",  role.oaserver["oaserver_url"].ToString()},
                {"region_name",  role.oaserver["region_name"].ToString()},
                {"retcode",  "0"},
                {"is_data_ready",  true},
                {"server_ext",  role.oaserver["server_ext"]},
            };
            JObject data_json = new JObject()
            {
                {"accountType", role.accountType },
                {"accountID", role.open_id },
                {"accountToken", role.combo_token },
                {"dispatch", dispatch_json },
            };
            JObject ext_json = new JObject { {"data", data_json } };
            JObject payload_json = new JObject
            {
                {"raw", raw_json },
                {"proto", "Combo" },
                {"ext", ext_json }
            };
            JObject confirm_json = new JObject
            {
                {"device", "" },
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

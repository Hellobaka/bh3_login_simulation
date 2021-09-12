using CustomGacha.SDK.Tool.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BH3_QRCodeScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            //string query = "?operators=1&merchant_id=590&isRoot=0&domain_switch_count=0&sdk_type=1&sdk_log_type=1&timestamp=1631325134181&support_abis=x86%2Carmeabi-v7a%2Carmeabi&access_key=&sdk_ver=3.4.2&oaid=&dp=1440*810&original_domain=&imei=330000000142738&version=1&udid=KREhESMUI0F4T3hPM08zSTEAZ1NhAnIYfA%3D%3D&apk_sign=d1b01b32b10526be2659108204a751d8&platform_type=3&old_buvid=XZ4596E46B8FB6B2152AD5BE95099CF082204&android_id=2e7793608ac77ee7&sign=4ff92b7be94ca510337d47e1c6079f46&fingerprint=&mac=08%3A00%3A27%3AB0%3A96%3A73&server_id=378&domain=line1-sdk-center-login-sh.biligame.net&app_id=180&version_code=19&net=4&pf_ver=6.0.1&cur_buvid=XZ4596E46B8FB6B2152AD5BE95099CF082204&c=1&brand=Xiaomi&client_timestamp=1631325134402&channel_id=1&uid=&game_id=180&ver=1.4.2-dev&model=MI+NOTE+3";
            //var c = Helper.GetURLQuery(HttpTool.UrlDecode(query));
            //JObject json = new JObject { };
            //foreach (var item in c)
            //{
            //    json.Add(item.Key, item.Value);
            //}
            //Console.WriteLine(json.ToString(Newtonsoft.Json.Formatting.None));
            //string c = "{\"app_id\":\"1\",\"channel_id\":\"14\",\"data\":\"{\\\"uid\\\":87115779,\\\"access_key\\\":\\\"26a1cfe42da2f527fdb1fd2b0daf81f4_sh\\\"}\",\"device\":\"2e7793608ac77ee7\"}";
            //Console.WriteLine(Encrypt.Scan_bh3sign(JObject.Parse(c)));
            BSGameSDK sdk = new BSGameSDK(account, password);
            var r = sdk.Login();
            Console.WriteLine($"blbl账号登录成功，结果: {r}");
            QRScanner scanner = new QRScanner(r["uid"].ToString(), r["access_key"].ToString());
            scanner.Bili_Login();
            Console.WriteLine("请输入二维码地址");
            string path = Console.ReadLine();
            string url = QRScanner.ScanQRCode(path);
            Console.WriteLine($"扫码成功，url={url}");
            scanner.ParseURL(url);
            scanner.DoQRLogin();
            Console.WriteLine("登录成功");
            Console.ReadLine();
        }        
    }
}

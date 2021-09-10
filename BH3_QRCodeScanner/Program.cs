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
        static string account = "2185367837@qq.com";
        static string password = "asq56747277";


        static void Main(string[] args)
        {
            BSGameSDK sdk = new BSGameSDK(account, password);
            var r = sdk.Login();
            Console.WriteLine($"blbl账号登录成功，结果: {r}");
            QRScanner scanner = new QRScanner(r["uid"].ToString(), r["access_key"].ToString());
            scanner.Bili_Login();
            Console.WriteLine("请输入二维码地址");
            string path = Console.ReadLine();
            string url = QRScanner.ScanQRCode(path);
            Console.WriteLine($"扫码成功，url={url}");
            scanner.DoQRLogin();
            Console.ReadLine();
        }        
    }
}

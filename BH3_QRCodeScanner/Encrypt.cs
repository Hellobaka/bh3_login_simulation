using CustomGacha.SDK.Tool.Http;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BH3_QRCodeScanner
{
    public static class Encrypt
    {
        //const string SALT = "fe8aac4e02f845b8ad67c427d48bfaf1";
        const string SALT = "dbf8f1b4496f430b8a3c0f436a35b931";
        public const string BH_PUBLIC_KEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDDvekdPMHN3AYhm/vktJT+YJr7cI5DcsNKqdsx5DZX0gDuWFuIjzdwButrIYPNmRJ1G8ybDIF7oDW2eEpm5sMbL9zs\n9ExXCdvqrn51qELbqj0XxtMTIpaCHFSI50PfPpTFV9Xt/hmyVwokoOXFlAEgCn+Q\nCgGs52bFoYMtyi+xEQIDAQAB\n";
        public const string BH_APP_KEY = "0ebc517adb1b62c6b408df153331f9aa";
        public static string Login_SetSign(JObject data)
        {
            data["timestamp"] = Helper.TimeStamp.ToString();
            data["client_timestamp"] = Helper.TimeStamp.ToString();

            string query = "";
            List<KeyValuePair<string, JToken>> ordered = new List<KeyValuePair<string, JToken>>();
            foreach (var item in data)
            {
                ordered.Add(item);
                if (item.Key == "pwd")
                {
                    query += $"{item.Key}={HttpTool.UrlEncode(item.Value.ToString())}&";
                    // continue;
                }
                query += $"{item.Key}={item.Value}&";
            }
            string sign = "";
            ordered = ordered.OrderBy(x => x.Key).ToList();
            ordered.ForEach(x => sign += $"{x.Value}");
            sign += SALT;

            sign = MD5Encrypt(sign).ToLower();
            query += "sign=" + sign;
            return query;
        }

        public static string Scan_bh3sign(JObject data)
        {
            List<KeyValuePair<string, JToken>> ordered = new List<KeyValuePair<string, JToken>>();
            foreach (var item in data)
                ordered.Add(item);
            ordered = ordered.OrderBy(x => x.Key).ToList();
            string sign = "";
            ordered.ForEach(x => sign += $"{x.Key}={x.Value}&");
            return HmacSHA256(sign.Substring(0, sign.Length - 1), BH_APP_KEY);
        }
        public static string MD5Encrypt(string baseStr)
        {
            MD5 md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(baseStr));
            return BitConverter.ToString(hash).Replace("-", "");
        }
        public static string RSAEncrypt(string plainString, string publicKey)
        {
            return Convert.ToBase64String(RSAEncrypt(Encoding.UTF8.GetBytes(plainString), publicKey)).Replace("-", "");
        }
        public static byte[] RSAEncrypt(byte[] plainBytes, string publicKey)
        {
            var encryptEngine = new Pkcs1Encoding(new RsaEngine()); // new Pkcs1Encoding (new RsaEngine());
            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();
                encryptEngine.Init(true, keyParameter);
            }
            var encrypted = encryptEngine.ProcessBlock(plainBytes, 0, plainBytes.Length);
            return encrypted;
        }
        public static string HmacSHA256(string secret, string signKey)
        {
            string signRet = string.Empty;
            using (HMACSHA256 mac = new HMACSHA256(Encoding.UTF8.GetBytes(signKey)))
            {
                byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(secret));
                signRet = BitConverter.ToString(hash).Replace("-", "");
            }
            return signRet.ToLower();
        }
    }
}

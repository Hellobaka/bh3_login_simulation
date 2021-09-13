using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BH3Scanner.PublicInfos
{
    public static class AccountSave
    {
        public static Dictionary<long, JObject> Accounts { get; set; } = new Dictionary<long, JObject>();
        public static void AddAccount(long qq, string account, string password)
        {
            if (Accounts.ContainsKey(qq))
            {
                if (Accounts[qq].ContainsKey(account))
                {
                    Accounts[qq][account] = password;
                }
                else
                {
                    Accounts[qq].Add(account, password);
                }
            }
            else
            {
                Accounts.Add(qq, new JObject { { account, password } });
            }
            MainSave.CQLog.Info("账号添加成功", $"account = {account}");
            AccountSecurity.SaveData(Accounts);
        }
        public static void LoadAccount()
        {
            JObject t = AccountSecurity.LoadData();
            if (t == null)
                t = new JObject();
            int count = 0;
            foreach (var item in t)
            {
                Accounts.Add(Convert.ToInt64(item.Key), item.Value as JObject);
                count += (item.Value as JObject).Count;
            }
            MainSave.CQLog.Info("账号读取成功", $"共读取了 {count} 个账号");
        }
        public static void RemoveAccount(long qq, string account)
        {
            if (Accounts.ContainsKey(qq))
            {
                if (Accounts[qq].ContainsKey(account))
                {
                    Accounts[qq].Remove(account);
                    MainSave.CQLog.Info("账号移除成功", $"account = {account}");
                }
                else
                    MainSave.CQLog.Info("账号不存在", $"QQ={qq}, account = {account}");
            }
            else
            {
                MainSave.CQLog.Info("账号不存在", $"QQ={qq}");
            }
            AccountSecurity.SaveData(Accounts);
        }
    }
    public static class AccountSecurity
    {
        const string XOR_KEY = "BOT";
        public static void SaveData(Dictionary<long, JObject> accounts)
        {
            JObject t = new JObject();
            foreach (var item in accounts)
                t.Add(item.Key.ToString(), item.Value);
            SaveData(t);
        }
        public static void SaveData(JObject account)
        {
            string baseStr = account.ToString(Formatting.None);
            File.WriteAllBytes(Path.Combine(MainSave.AppDirectory, "verify.bin"), XorEncrypt(baseStr, XOR_KEY));
        }
        public static JObject LoadData()
        {
            string filepath = Path.Combine(MainSave.AppDirectory, "verify.bin");
            if (!File.Exists(filepath))
                return null;
            byte[] c = File.ReadAllBytes(filepath);
            return JObject.Parse(Encoding.UTF8.GetString(XorEncrypt(c, XOR_KEY)));
        }
        private static byte[] XorEncrypt(byte[] plainText, string key)
        {
            return XorEncrypt(plainText, Encoding.UTF8.GetBytes(key));
        }
        private static byte[] XorEncrypt(string plainText, string key)
        {
            return XorEncrypt(Encoding.UTF8.GetBytes(plainText), Encoding.UTF8.GetBytes(key));
        }
        private static byte[] XorEncrypt(byte[] plainBytes, byte[] key)
        {
            int index = 0;
            for (int i = 0; i < plainBytes.Length; i++)
            {
                plainBytes[i] = (byte)(plainBytes[i] ^ key[index]);
                index++;
                if (index == key.Length)
                    index = 0;
            }
            return plainBytes;
        }
    }
}

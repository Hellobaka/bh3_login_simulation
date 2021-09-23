using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.EventArgs;
using BH3Scanner.PublicInfos;
using BH3_QRCodeScanner;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.Model;
using System.IO;

namespace me.cqp.luohuaming.BH3Scanner.Code.OrderFunctions
{
    public class DoScan : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => OrderText.QRCodeScan;
        public int Protity { get; set; } = 10;
        public bool Judge(string destStr) => true;

        public FunctionResult Progress(CQGroupMessageEventArgs e)//群聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromGroup,
            };
            var r = ScanResult(e.FromGroup, e.FromQQ, e.Message.Text);
            if (string.IsNullOrWhiteSpace(r) is false)
                sendText.MsgToSend.Add(r);
            else
            {
                result.Result = false;
                result.SendFlag = false;
            }
            result.SendObject.Add(sendText);
            return result;
        }

        public FunctionResult Progress(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromQQ,
            };
            var r = ScanResult(-1, e.FromQQ, e.Message.Text);
            if (string.IsNullOrWhiteSpace(r) is false)
                sendText.MsgToSend.Add(r);
            else
            {
                result.Result = false;
                result.SendFlag = false;
            }
            result.SendObject.Add(sendText);
            return result;
        }
        class Scan
        {
            public string Account { get; set; }
            public string Password { get; set; }
            private BSGameSDK login;
            private QRScanner Scanner { get; set; }
            public Scan(string account, string password)
            {
                Account = account;
                Password = password;
                login = new BSGameSDK(account, password);
            }
            public bool Login()
            {
                var r = login.Login();
                if (r["code"].ToString() == "0")
                {
                    Scanner = new QRScanner(r["uid"].ToString(), r["access_key"].ToString());
                    Scanner.Bili_Login();
                    return true;
                }
                else
                {
                    MainSave.CQLog.Info("登录失败", $"code={r["code"]} msg={r["msg"]}");
                    return false;
                }
            }
            public bool QRCodeScan(string path)
            {
                string url = QRScanner.ScanQRCode(path);
                MainSave.CQLog.Info("二维码扫描", $"扫码成功，url={url}");
                if (Scanner.ParseURL(url))
                {
                    var b = Scanner.DoQRLogin();
                    if (b != null && b["retcode"].ToString() == "0")
                        return true;
                    MainSave.CQLog.Info("二维码扫描", $"登录失败，msg={b["message"]}");
                    return false;
                }
                else
                    return false;
            }
        }
        class User
        {
            public long GroupID;
            public long QQID;
        }
        static Dictionary<User, Scan> WaitLogin = new Dictionary<User, Scan>();
        public string ScanResult(long Group, long qq, string msg)
        {
            try
            {
                if (WaitLogin.Any(x => x.Key.GroupID == Group && x.Key.QQID == qq))
                {
                    User user = WaitLogin.First(x => x.Key.GroupID == Group && x.Key.QQID == qq).Key;
                    var c = CQCode.Parse(msg);
                    if (c.Any(x => x.IsImageCQCode))
                    {
                        var img = c.First(x => x.IsImageCQCode);
                        using (var http = new Tool.Http.HttpWebClient())
                        {
                            Directory.CreateDirectory(Path.Combine(MainSave.ImageDirectory, "tmp"));
                            string filename = Path.Combine(MainSave.ImageDirectory, "tmp", Guid.NewGuid().ToString() + ".jpg");
                            http.DownloadFile(CommonHelper.GetImageURL(img.ToString()), filename);
                            string retMsg = "";
                            if (WaitLogin[user].QRCodeScan(filename))
                            {
                                retMsg = "扫码完成";
                            }
                            else
                            {
                                retMsg = "扫码登录失败，请查看日志排查问题";
                            }
                            WaitLogin.Remove(user);
                            return retMsg;
                        }
                    }
                    else
                    {
                        WaitLogin.Remove(user);
                        return "无效图片回复，请重新输入登录指令；注：现已经登录了一次，短时间多次登录账号可能会出问题";
                    }
                }
                if (msg.StartsWith(GetOrderStr()) is false)
                    return string.Empty;
                KeyValuePair<string, string> account = new KeyValuePair<string, string>();
                if (AccountSave.Accounts.ContainsKey(qq))
                {
                    var accountList = AccountSave.Accounts[qq];
                    if (accountList.Count == 0)
                        return $"请先绑定账号，输入 {OrderText.SetAccount}";
                    else if (accountList.Count == 1)
                    {
                        foreach (var item in accountList)
                            account = new KeyValuePair<string, string>(item.Key, item.Value.ToString());
                    }
                    else
                    {
                        var b = msg.Trim().Split(' ');
                        if (b.Length > 1)
                        {
                            if (accountList.ContainsKey(b[1]))
                            {
                                account = new KeyValuePair<string, string>(b[1], accountList[b[1]].ToString());
                            }
                            else
                            {
                                int index = 1;
                                foreach (var item in accountList)
                                {
                                    if (index.ToString() == b[1])
                                    {
                                        account = new KeyValuePair<string, string>(item.Key, item.Value.ToString());
                                        break;
                                    }
                                    else
                                        index++;
                                }
                                if (account.Key == null)
                                {
                                    return $"未找到对应账号，请输入序号或是 已绑定的手机号或是邮箱";
                                }
                            }
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"你的QQ下绑定了多个账号，输入 {OrderText.QRCodeScan} [序号/绑定账号]");
                            int index = 1;
                            foreach (var item in accountList)
                            {
                                sb.AppendLine($"{index}. {CommonHelper.GetMask(item.Key)}");
                                index++;
                            }
                            return sb.ToString();
                        }
                    }
                }
                else
                {
                    return $"请先绑定账号，输入 {OrderText.SetAccount}";
                }
                var s = new Scan(account.Key, account.Value);
                MainSave.CQLog.Info("扫描二维码", $"QQ={qq} account={account.Key}, 账号获取成功，开始登录");
                if (s.Login())
                {
                    WaitLogin.Add(new User { QQID = qq, GroupID = Group }, s);
                    return $"账号登录成功，请放置二维码图片";
                }
                else
                {
                    return "登录失败，请查看日志排查问题";
                }
            }
            catch (Exception e)
            {

                return $"扫码登录失败，{e.Message}请稍等后重试，若依旧如此请联系作者获取帮助";
            }
        }
    }
}

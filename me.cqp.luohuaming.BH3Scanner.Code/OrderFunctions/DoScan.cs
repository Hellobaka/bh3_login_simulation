using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.EventArgs;
using BH3Scanner.PublicInfos;
using BH3_QRCodeScanner;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.Model;

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
        /// <summary>
        /// 记录用户信息以及进行二维码扫描的类
        /// </summary>
        class Scan
        {
            /// <summary>
            /// 用户账号
            /// </summary>
            public string Account { get; set; }
            /// <summary>
            /// 用户密码
            /// </summary>
            public string Password { get; set; }
            /// <summary>
            /// SDK实例
            /// </summary>
            private BSGameSDK login;
            /// <summary>
            /// 扫码器实例
            /// </summary>
            private QRScanner Scanner { get; set; }
            public Scan(string account, string password)
            {
                Account = account;
                Password = password;
                login = new BSGameSDK(account, password);
            }
            /// <summary>
            /// 进行登录流程
            /// </summary>
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
                    MainSave.CQLog.Info("登录失败", $"code={r["code"]} msg={(r.ContainsKey("msg") ? r["msg"] : r["message"])}");
                    return false;
                }
            }
            /// <summary>
            /// 进行二维码登录
            /// </summary>
            /// <param name="path">图片路径</param>
            public bool QRCodeScan(string path)
            {
                try
                {
                    string url = QRScanner.ScanQRCode(path);
                    if (string.IsNullOrEmpty(url))
                    {
                        MainSave.CQLog.Info("二维码扫描", $"扫码失败 URL为空");
                        return false;
                    }
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
                catch(Exception e)
                {
                    MainSave.CQLog.Info("二维码扫描", $"登录失败，msg={e.Message + e.StackTrace}");
                    return false;
                }
            }
        }
        /// <summary>
        /// 扫码等待用户
        /// </summary>
        class User
        {
            public long GroupID;
            public long QQID;
        }
        static Dictionary<User, Scan> WaitLogin = new Dictionary<User, Scan>();
        /// <summary>
        /// 获取对话内容结果
        /// </summary>
        /// <param name="Group">来源群号 来自私聊写-1</param>
        /// <param name="qq">来源QQ号</param>
        /// <param name="msg">指令文本</param>
        public string ScanResult(long Group, long qq, string msg)
        {
            try
            {
                //获取等待列表内是否有此用户 若有说明之前触发过登录指令校验信息是否为二维码图片
                if (WaitLogin.Any(x => x.Key.GroupID == Group && x.Key.QQID == qq))
                {
                    //进行二维码登录
                    User user = WaitLogin.First(x => x.Key.GroupID == Group && x.Key.QQID == qq).Key;
                    var c = CQCode.Parse(msg);
                    //查看是否存在图片
                    if (c.Any(x => x.IsImageCQCode))
                    {
                        var img = c.First(x => x.IsImageCQCode);
                        string retMsg = "";
                        //调用登录函数, 进行二维码登录
                        if (WaitLogin[user].QRCodeScan(MainSave.CQApi.ReceiveImage(img)))
                        {
                            retMsg = "扫码完成";
                        }
                        else
                        {
                            retMsg = "扫码登录失败，可能是二维码过期，请查看日志排查问题";
                        }
                        //登录完毕
                        WaitLogin.Remove(user);
                        return retMsg;
                    }
                    else//未在回复中找到图片
                    {
                        WaitLogin.Remove(user);
                        return "无效图片回复，请重新输入登录指令；注：现已经登录了一次，短时间多次登录账号可能会出问题";
                    }
                }
                //之前未触发过 新建等待
                //匹配指令
                if (msg.StartsWith(GetOrderStr()) is false)
                    return string.Empty;
                KeyValuePair<string, string> account = new KeyValuePair<string, string>();
                //查询此QQ是否绑定了账号
                if (AccountSave.Accounts.ContainsKey(qq))
                {
                    //获取此QQ绑定的账号列表
                    var accountList = AccountSave.Accounts[qq];
                    if (accountList.Count == 0)
                        return $"请先绑定账号，输入 {OrderText.SetAccount}";
                    else if (accountList.Count == 1)//为一个账号, 默认登录
                    {
                        foreach (var item in accountList)//?
                            account = new KeyValuePair<string, string>(item.Key, item.Value.ToString());
                    }
                    else
                    {
                        //绑定多个账号, 判定是否存在指令参数
                        var b = msg.Trim().Split(' ');
                        if (b.Length > 1)//存在指令参数
                        {
                            //判定参数是账号还是序号
                            if (accountList.ContainsKey(b[1]))//是账号
                            {
                                account = new KeyValuePair<string, string>(b[1], accountList[b[1]].ToString());
                            }
                            else//是序号
                            {
                                //从1开始递增, 直到序号与指令相同
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
                                //循环完成但未找到对应序号
                                if (account.Key == null)
                                {
                                    return $"未找到对应账号，请输入序号或是 已绑定的手机号或是邮箱";
                                }
                            }
                        }
                        else//多个账号且未指定参数
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
                else//未绑定账号
                {
                    return $"请先绑定账号，输入 {OrderText.SetAccount}";
                }
                var s = new Scan(account.Key, account.Value);
                MainSave.CQLog.Info("扫描二维码", $"QQ={qq} account={account.Key}, 账号获取成功，开始登录");
                MainSave.CQApi.SendGroupMessage(Group, qq, "账号获取成功，开始登录....");
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
                MainSave.CQLog.Info("Error", $"{e.Message}\n{e.StackTrace}");
                return $"扫码登录失败，{e.Message}请稍等后重试，若依旧如此请联系作者获取帮助";
            }
        }
    }
}

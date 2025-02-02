using System.Collections.Generic;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.EventArgs;
using BH3Scanner.PublicInfos;
using System.Threading;
using BH3_QRCodeScanner;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace me.cqp.luohuaming.BH3Scanner.Code.OrderFunctions
{
    public class SetAccount : IOrderModel
    {
        class User
        {
            public long Group;
            public long QQ;
        }
        /// <summary>
        /// 保存用户登记状态机的字典
        /// </summary>
        static Dictionary<User, StateMachine> StateMachines = new Dictionary<User, StateMachine>();
        public bool ImplementFlag { get; set; } = true;
        public string GetOrderStr() => "";
        public int Protity { get; set; } = 100;

        public bool Judge(string destStr) => !destStr.StartsWith(OrderText.QRCodeScan)&&!destStr.Contains("[CQ:image");

        public FunctionResult Progress(CQGroupMessageEventArgs e)
        {
            FunctionResult result = new FunctionResult
            {
                Result = false,
                SendFlag = false,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromGroup,
            };
            User user;
            if (e.Message.Text.Trim() == OrderText.SetAccount)//匹配指令
            {
                result.Result = true;
                result.SendFlag = true;
                if (!StateMachines.Any(x => x.Key.Group == e.FromGroup && x.Key.QQ == e.FromQQ))//状态机数组内不包含此用户
                {
                    user = new User { Group = e.FromGroup, QQ = e.FromQQ };
                    StateMachines.Add(user, new StateMachine(e.FromQQ, e.FromGroup));
                    sendText.MsgToSend.Add(StateMachines[user].GetReply(e.Message.Text));
                }
                else//流程未完成但依旧输入登记指令
                {
                    sendText.MsgToSend.Add($"请参照聊天记录完成当前进度，如需重置，请输入 #扫码重置");
                }
            }
            else if (StateMachines.Any(x => x.Key.Group == e.FromGroup && x.Key.QQ == e.FromQQ))
            {
                user = StateMachines.First(x=> x.Key.Group == e.FromGroup && x.Key.QQ == e.FromQQ).Key;
                string reply = StateMachines[user].GetReply(e.Message.Text);//获取状态机处理结果
                if (reply == "Done" || reply == "Deny" 
                    || (StateMachines[user].NowState == StateMachine.State.Done 
                    || StateMachines[user].NowState == StateMachine.State.Deny))//若最终结果文本或状态为这俩表示已完成登记, 移除用户
                {
                    StateMachines.Remove(user);
                }
                if (string.IsNullOrWhiteSpace(reply) is false)
                {
                    result.Result = true;
                    result.SendFlag = true;
                    sendText.MsgToSend.Add(reply);
                }
            }
            result.SendObject.Add(sendText);
            return result;
        }

        public FunctionResult Progress(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = false,
                SendFlag = false,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromQQ,
            };
            User user;
            if (e.Message.Text.Trim() == OrderText.SetAccount)
            {
                result.Result = true;
                result.SendFlag = true;
                if (!StateMachines.Any(x => x.Key.Group == -1 && x.Key.QQ == e.FromQQ))
                {
                    user = new User { QQ=e.FromQQ, Group = -1 };
                    StateMachines.Add(user, new StateMachine(e.FromQQ, -1));
                    sendText.MsgToSend.Add(StateMachines[user].GetReply(e.Message.Text));
                }
                else
                {
                    sendText.MsgToSend.Add($"请参照聊天记录完成当前进度，如需重置，请输入 #扫码重置");
                }
            }
            else if (StateMachines.Any(x => x.Key.Group == -1 && x.Key.QQ == e.FromQQ))
            {
                user = StateMachines.First(x => x.Key.Group == -1 && x.Key.QQ == e.FromQQ).Key;
                string reply = StateMachines[user].GetReply(e.Message.Text);
                if (reply == "Done" || reply == "Deny"
                    || (StateMachines[user].NowState == StateMachine.State.Done
                    || StateMachines[user].NowState == StateMachine.State.Deny))
                {
                    StateMachines.Remove(user);
                    reply = string.Empty;
                }
                if (string.IsNullOrWhiteSpace(reply) is false)
                {
                    result.Result = true;
                    result.SendFlag = true;
                    sendText.MsgToSend.Add(reply);
                }
            }
            result.SendObject.Add(sendText);
            return result;
        }
    }
    /// <summary>
    /// 状态机, 描述用户登记进度的类
    /// </summary>
    public class StateMachine
    {
        public long QQ;
        public long Group;

        public StateMachine(long qq, long group)
        {
            QQ = qq;
            Group = group;
        }

        public string Account;
        public string Password;
        public string Captcha_Challenge;
        public string Captcha_UserID;

        private BSGameSDK login;
        public State NowState = State.Non;
        public static string Deny_Text = "感谢你的使用";
        /// <summary>
        /// 进度状态
        /// </summary>
        public enum State
        {
            Non,//起始状态
            Account,//输入账号
            Password,//输入密码
            Verify,//验证账号能否登录
            Captcha,//需要验证码
            Captcha_Error,//验证码错误
            Done,//流程完成
            Deny//拒绝协议
        }
        public void SendMsg(string msg)
        {
            if (Group == -1)
            {
                MainSave.CQApi.SendPrivateMessage(QQ, msg);
            }
            else
            {
                MainSave.CQApi.SendGroupMessage(Group, msg);
            }
        }
        /// <summary>
        /// 获取状态机回复
        /// </summary>
        /// <param name="order">指令文本</param>
        public string GetReply(string order)
        {
            if (order.Trim() == "#拒绝")//拒绝了协议
            {
                NowState = State.Deny;
                SendMsg(Deny_Text);
                return "Deny";
            }
            else if (order.Trim() == "#扫码重置")
            {
                NowState = State.Non;
            }
            else if (order.Trim() == OrderText.CaptchaVerify && NowState == State.Captcha_Error)
            {
                NowState = State.Captcha;
                var captcha = login.Captcha();
                if (captcha.ContainsKey("gt") is false)
                {
                    NowState = State.Done;
                    MainSave.CQLog.Info("验证码获取失败", captcha.ToString());
                    SendMsg("验证码获取失败，请联系作者维护");
                    return "";
                }
                MainSave.CQLog.Info("验证码获取成功", "成功");
                Captcha_Challenge = captcha["challenge"].ToString();
                Captcha_UserID = captcha["gt_user_id"].ToString();
                string url = $"https://help.tencentbot.top/geetest/?captcha_type=1&challenge={captcha["challenge"]}&gt={captcha["gt"]}&userid={captcha["gt_user_id"]}&gs=1";
                return $"请在浏览器打开网址: {url}，完成验证码，之后将 validate= 后的内容粘贴到此处";
            }
            switch (NowState)
            {
                case State.Non:
                    NowState = State.Account;
                    return $"感谢你使用水银扫码机，本插件仅记录账号与密码供扫码使用，请在信任Bot所属者之后提供账号信息，造成个人财产损失插件作者恕不负责。\n若认可上述内容，请输入你的Bilibili账号的手机或邮箱或用户名，反之请输入 #拒绝 来取消这一进程。";
                case State.Account:
                    if (order.Split(' ').Length == 2)//若提供了账号与密码则一步到位
                    {
                        var s = order.Split(' ');
                        NowState = State.Verify;
                        this.Account = s[0];
                        this.Password = s[1];
                        Thread thread_A = new Thread(() => GetReply(""));
                        thread_A.Start();
                        return $"正在对你的账号正在验证，请耐心等待；";
                    }
                    else
                    {
                        NowState = State.Password;
                        this.Account = order;
                        return "请输入你的密码";
                    }
                case State.Password:
                    NowState = State.Verify;
                    this.Password = order;
                    Thread thread_B = new Thread(() => GetReply(""));
                    thread_B.Start();
                    return $"正在对你的账号正在验证，请耐心等待；";
                case State.Verify:
                    login = new BSGameSDK(Account, Password);
                    JObject r = login.Login();
                    if (r["code"].ToString() == "500002")
                    {
                        MainSave.CQLog.Info("账号登录", $"QQ = {QQ}, code = 500002, 账号或密码错误");
                        NowState = State.Account;
                        SendMsg($"账号或密码错误，请重新输入账号");
                        return "";
                    }
                    else if (r.ContainsKey("access_key") is false)
                    {
                        MainSave.CQLog.Info("登录失败", $"可能需要验证码，QQ = {QQ}, code = {r["code"]}, {r["msg"]}");
                        NowState = State.Captcha;
                        var captcha = login.Captcha();
                        if (captcha.ContainsKey("gt") is false)
                        {
                            NowState = State.Captcha_Error;
                            MainSave.CQLog.Info("验证码获取失败", $"QQ = {QQ}, " + captcha.ToString());
                            SendMsg("验证码获取失败，请联系作者维护，也可以尝试输入 #扫码验证码 来重新获取");
                            return "";
                        }
                        MainSave.CQLog.Info("验证码获取成功", $"QQ = {QQ}, 成功");
                        Captcha_Challenge = captcha["challenge"].ToString();
                        Captcha_UserID = captcha["gt_user_id"].ToString();
                        string url = $"https://help.tencentbot.top/geetest/?captcha_type=1&challenge={captcha["challenge"]}&gt={captcha["gt"]}&userid={captcha["gt_user_id"]}&gs=1";
                        SendMsg($"请在浏览器打开网址: {url}，完成验证码，之后将 validate= 后的内容粘贴到此处");
                        return $"";
                    }
                    else
                    {
                        NowState = State.Done;
                        MainSave.CQLog.Info("账号登录成功", $"QQ = {QQ}");
                        SendMsg($"账号验证完成，感谢你的耐心");
                        AccountSave.AddAccount(QQ, Account, Password);
                        return "";
                    }
                case State.Captcha:
                    order = order.Trim().Replace("validate=", "");
                    var login_R = login.Login_WithCaptcha(Captcha_Challenge, Captcha_UserID, order);
                    if (login_R["code"].ToString() != "0")
                    {
                        NowState = State.Captcha_Error;
                        MainSave.CQLog.Info("验证码错误", $"code = {login_R["code"]}, msg = {login_R["msg"]}");
                        return $"验证码有误， 请联系管理员，或输入 #扫码验证码 来重新验证";
                    }
                    else
                    {
                        NowState = State.Done;
                        AccountSave.AddAccount(QQ, Account, Password);
                        return $"账号验证完成，感谢你的耐心";
                    }
                case State.Done:
                    return "Done";
                case State.Deny:
                    return "Deny";
                default:
                    break;
            }
            return "";
        }
    }
}

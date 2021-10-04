using System;
using System.Text;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.EventArgs;
using BH3Scanner.PublicInfos;

namespace me.cqp.luohuaming.BH3Scanner.Code.OrderFunctions
{
    public class Help : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;
        public int Protity { get; set; } = 100;

        public string GetOrderStr() => OrderText.Help;

        public bool Judge(string destStr) => destStr.Equals(GetOrderStr());//这里判断是否能触发指令

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

            sendText.MsgToSend.Add(GetHelp());
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

            sendText.MsgToSend.Add(GetHelp());
            result.SendObject.Add(sendText);
            return result;
        }
        public string GetHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"感谢使用水银扫码机, 群聊以及私聊均可, 以下为指令以及说明：");
            sb.AppendLine($"{OrderText.Help}: 获取扫码机的帮助文档");
            sb.AppendLine($"{OrderText.SetAccount}: 进行哔哩哔哩账号与QQ的绑定");
            sb.AppendLine($"{OrderText.RemoveAccount} 已绑定账号: 取消哔哩哔哩账号与QQ的绑定");
            sb.AppendLine($"{OrderText.QRCodeScan} [可选参数: 绑定序号/账号]: 进行扫码过程, 单一绑定无需附加参数, 若有多个账号, 可在指令后指定序号或者账号");
            sb.AppendLine($"{OrderText.CaptchaVerify}: 登录过程中需要验证码但未获取成功或验证失败, 输入此指令重新获取");
            return sb.ToString();
        }
    }
}

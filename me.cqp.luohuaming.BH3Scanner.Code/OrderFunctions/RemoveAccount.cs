using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.EventArgs;
using BH3Scanner.PublicInfos;

namespace me.cqp.luohuaming.BH3Scanner.Code.OrderFunctions
{
    public class RemoveAccount : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;
        public int Protity { get; set; } = 100;
        public string GetOrderStr() => OrderText.RemoveAccount;

        public bool Judge(string destStr) => destStr.Trim().StartsWith(GetOrderStr());//这里判断是否能触发指令

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
            sendText.MsgToSend.Add(Remove(e.Message.Text, e.FromQQ));
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
            sendText.MsgToSend.Add(Remove(e.Message.Text, e.FromQQ));
            result.SendObject.Add(sendText);
            return result;
        }
        public string Remove(string account, long qq)
        {
            var b = account.Split(' ');
            if (b.Length != 2)
                return $"格式有误，请输入 {OrderText.RemoveAccount} 绑定账号";
            if(AccountSave.Accounts.ContainsKey(qq))
            {
                if (AccountSave.Accounts[qq].ContainsKey(b[1]))
                {
                    AccountSave.RemoveAccount(qq, b[1]);
                    return "删除成功";
                }
                else
                {
                    return "无对应账号需删除";
                }
            }
            else
            {
                return "无对应账号需删除";
            }
        }
    }
}

using System.Collections.Generic;
using System.Text;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.EventArgs;

namespace BH3Scanner.PublicInfos
{
    public interface IOrderModel
    {
        int Protity { get; set; }
        bool ImplementFlag { get; set; }
        string GetOrderStr();
        bool Judge(string destStr);
        FunctionResult Progress(CQGroupMessageEventArgs e);
        FunctionResult Progress(CQPrivateMessageEventArgs e);
    }
}

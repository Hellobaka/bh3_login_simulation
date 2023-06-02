using BH3Scanner.PublicInfos;
using Newtonsoft.Json.Linq;

namespace BH3_QRCodeScanner
{
    public class RoleData
    {
        public string open_id;
        public string open_token;
        public string combo_id;
        public string combo_token;
        public string channel_id;
        public string oaserver;
        public string account_type;
        public string accountType;
        public string oa_req_key;
        public int special_tag;

        public RoleData(string open_id, string open_token, string combo_id, string combo_token, string channel_id, string account_type, string oa_req_key, int special_tag)
        {
            string ver = MainSave.BH3Ver;
            MainSave.CQLog.Info("扫码登录", $"当前配置中崩坏3版本: {ver}");
            if (string.IsNullOrWhiteSpace(ver))
            {
                MainSave.CQLog.Info("无效版本", $"版本号无效，请按照文档填写版本号");
                throw new System.Exception();
            }
            this.open_id = open_id;
            this.open_token = open_token;
            this.combo_id = combo_id;
            this.combo_token = combo_token;
            this.channel_id = channel_id;
            this.account_type = account_type;
            this.accountType = account_type;
            this.oa_req_key = ver + "_gf_android_" + oa_req_key;
            this.special_tag = special_tag;
            MainSave.CQLog.Info("OA服务器获取", "获取OA服务器...");
            this.oaserver = GetOAServer();
            MainSave.CQLog.Info("OA服务器获取", "获取OA服务器成功");
        }
        public string GetOAServer()
        {
            JObject data = new JObject();
            using(var http = Helper.GetCommonHttp(false))
            {
                string url = $"https://dispatch.scanner.hellocraft.xyz/v1/query_dispatch/?version={oa_req_key}&t={Helper.TimeStampMs}";
                return http.UploadString(url, "");
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp;
using me.cqp.luohuaming.BH3Scanner.Tool.IniConfig;

namespace BH3Scanner.PublicInfos
{
    public static class MainSave
    {
        /// <summary>
        /// 保存各种事件的数组
        /// </summary>
        public static List<IOrderModel> Instances { get; set; } = new List<IOrderModel>();
        public static CQLog CQLog { get; set; }
        public static CQApi CQApi { get; set; }
        public static string AppDirectory { get; set; }
        public static string ImageDirectory { get; set; }

        static IniConfig configMain;
        public static bool IniChangeFlag = false;
        public static IniConfig ConfigMain
        {
            get
            {
                if (IniChangeFlag)
                    return configMain;
                configMain = new IniConfig(Path.Combine(AppDirectory, "Config.ini"));
                configMain.Load();
                return configMain;
            }
            set { configMain = value; }
        }
    }
}

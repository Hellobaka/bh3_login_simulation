namespace BH3Scanner.PublicInfos
{
    public static class OrderText
    {
        public static string SetAccount {
            get
            {
                string r = MainSave.ConfigMain.Object["Config"]["SetAccount"]?.ToString();
                if (string.IsNullOrWhiteSpace(r))
                    return "#扫码登记";
                return r;
            }
            set { MainSave.ConfigMain.Object["Config"]["SetAccount"] = value; MainSave.ConfigMain.Save(); }
        }
        public static string RemoveAccount {
            get
            {
                string r = MainSave.ConfigMain.Object["Config"]["RemoveAccount"]?.ToString();
                if (string.IsNullOrWhiteSpace(r))
                    return "#扫码删除";
                return r;
            }
            set { MainSave.ConfigMain.Object["Config"]["RemoveAccount"] = value; MainSave.ConfigMain.Save(); }
        }
        public static string Help
        {
            get
            {
                string r = MainSave.ConfigMain.Object["Config"]["Help"]?.ToString();
                if (string.IsNullOrWhiteSpace(r))
                    return "#扫码帮助";
                return r;
            }
            set { MainSave.ConfigMain.Object["Config"]["Help"] = value; MainSave.ConfigMain.Save(); }
        } 
        public static string CaptchaVerify
        {
            get
            {
                string r = MainSave.ConfigMain.Object["Config"]["CaptchaVerify"]?.ToString();
                if (string.IsNullOrWhiteSpace(r))
                    return "#扫码验证";
                return r;
            }
            set { MainSave.ConfigMain.Object["Config"]["CaptchaVerify"] = value; MainSave.ConfigMain.Save(); }
        }
        public static string QRCodeScan
        {
            get
            {
                string r = MainSave.ConfigMain.Object["Config"]["QRCodeScan"]?.ToString();
                if (string.IsNullOrWhiteSpace(r))
                    return "#扫码登录";
                return r;
            }
            set { MainSave.ConfigMain.Object["Config"]["QRCodeScan"] = value; MainSave.ConfigMain.Save(); }
        }
    }
}

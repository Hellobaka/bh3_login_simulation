using System;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BH3Scanner.Sdk.Cqp.Interface;
using BH3Scanner.PublicInfos;
using System.Reflection;
using System.IO;

namespace me.cqp.luohuaming.BH3Scanner.Code
{
    public class Event_StartUp : ICQStartup
    {
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            MainSave.AppDirectory = e.CQApi.AppDirectory;
            MainSave.CQApi = e.CQApi;
            MainSave.CQLog = e.CQLog;
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();

            AccountSave.LoadAccount();
            foreach (var item in Assembly.GetAssembly(typeof(Event_GroupMessage)).GetTypes())
            {
                if (item.IsInterface)
                    continue;
                foreach (var instance in item.GetInterfaces())
                {
                    if (instance == typeof(IOrderModel))
                    {
                        IOrderModel obj = (IOrderModel)Activator.CreateInstance(item);
                        if (obj.ImplementFlag == false)
                            break;
                        MainSave.Instances.Add(obj);
                    }
                }
            }
            foreach(var item in Directory.GetFiles(Path.Combine(MainSave.ImageDirectory, "tmp")))
            {
                try
                {
                    File.Delete(item);
                }
                catch { }
            }    
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeiBoGiveNotice
{
    public class CfgMgr
    {
        public static Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

        public static string GetValue(string key)
        {
            return config.AppSettings.Settings[key].Value;
        }

        public static void SaveValue(string key, string value)
        {

            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeiBoGiveNotice
{
    public partial class ConfigFrom : Form
    {
        private WeBoUserClient weBoUserClient;
        public ConfigFrom(WeBoUserClient weBoUserClient)
        {
            InitializeComponent();
        }
        private void ConfigFrom_Load(object sender, EventArgs e)
        {
            //赋值
            FansRefreshTime_Begin.Text = ConfigurationManager.AppSettings["FansRefreshTime_Begin"];
            FansRefreshTime_End.Text = ConfigurationManager.AppSettings["FansRefreshTime_End"];
            
        }

        //保存时间相关的配置信息
        private void saveConfig_Click(object sender, EventArgs e)
        {
            string file = Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            config.AppSettings.Settings["FansRefreshTime_Begin"].Value = FansRefreshTime_Begin.Text;
            config.AppSettings.Settings["FansRefreshTime_End"].Value = FansRefreshTime_End.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            this.Close();
        }
    }
}

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
            this.weBoUserClient = weBoUserClient;
        }
        private void ConfigFrom_Load(object sender, EventArgs e)
        {
            //赋值
            NewFansRefresh_Begin.Text = ConfigurationManager.AppSettings["NewFansRefresh_Begin"];
            NewFansRefresh_End.Text = ConfigurationManager.AppSettings["NewFansRefresh_End"];
            NewFansCall_Begin .Text = ConfigurationManager.AppSettings["NewFansCall_Begin"];
            NewFansCall_End.Text = ConfigurationManager.AppSettings["NewFansCall_End"];
            OldRefresh_Begin.Text = ConfigurationManager.AppSettings["OldRefresh_Begin"];
            OldRefresh_End.Text = ConfigurationManager.AppSettings["OldRefresh_End"];
            OldRefresh_Begin.Text = ConfigurationManager.AppSettings["OldRefresh_Begin"];
            OldFansCall_End.Text = ConfigurationManager.AppSettings["OldFansCall_End"];

        }

        //保存时间相关的配置信息
        private void saveConfig_Click(object sender, EventArgs e)
        {
            string file = Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            config.AppSettings.Settings["NewFansRefresh_Begin"].Value = NewFansRefresh_Begin.Text;
            config.AppSettings.Settings["NewFansRefresh_End"].Value = NewFansRefresh_End.Text;
            config.AppSettings.Settings["NewFansCall_Begin"].Value = NewFansCall_Begin.Text;
            config.AppSettings.Settings["NewFansCall_End"].Value = NewFansCall_End.Text;
            config.AppSettings.Settings["OldRefresh_Begin"].Value = OldRefresh_Begin.Text;
            config.AppSettings.Settings["OldRefresh_End"].Value = OldRefresh_End.Text;
            config.AppSettings.Settings["OldFansCall_Begin"].Value = OldFansCall_Begin.Text;
            config.AppSettings.Settings["OldFansCall_End"].Value = OldFansCall_End.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            weBoUserClient.NewFansRefresh_Begin = int.Parse(NewFansRefresh_Begin.Text);
            weBoUserClient.NewFansRefresh_End = int.Parse(NewFansRefresh_End.Text);
            weBoUserClient.NewFansCall_Begin = int.Parse(NewFansCall_Begin.Text);
            weBoUserClient.NewFansCall_End = int.Parse(NewFansCall_End.Text);
            weBoUserClient.OldRefresh_Begin = int.Parse(OldRefresh_Begin.Text);
            weBoUserClient.OldRefresh_End = int.Parse(OldRefresh_End.Text);
            weBoUserClient.OldFansCall_Begin = int.Parse(OldFansCall_Begin.Text);
            weBoUserClient.OldFansCall_End = int.Parse(OldFansCall_End.Text);
            this.Close();
        }
    }
}

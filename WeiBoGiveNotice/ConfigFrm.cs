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
    public partial class ConfigFrm : Form
    {
        private WeiBoUserClient weBoUserClient;
        public ConfigFrm(WeiBoUserClient weBoUserClient)
        {
            InitializeComponent();
            this.weBoUserClient = weBoUserClient;
        }
        private void ConfigFrom_Load(object sender, EventArgs e)
        {
            //赋值
            NewFansRefresh_Begin.Text = CfgMgr.GetValue("NewFansRefresh_Begin");
            NewFansRefresh_End.Text = CfgMgr.GetValue("NewFansRefresh_End");
            NewFansCall_Begin.Text = CfgMgr.GetValue("NewFansCall_Begin");
            NewFansCall_End.Text = CfgMgr.GetValue("NewFansCall_End");
            OldRefresh_Begin.Text = CfgMgr.GetValue("OldRefresh_Begin");
            OldRefresh_End.Text = CfgMgr.GetValue("OldRefresh_End");
            OldFansCall_Begin.Text = CfgMgr.GetValue("OldFansCall_Begin");
            OldFansCall_End.Text = CfgMgr.GetValue("OldFansCall_End");
            moreOffInterTime_begin.Text = CfgMgr.GetValue("moreOffInterTime_begin");
            moreOffInterTime_end.Text = CfgMgr.GetValue("moreOffInterTime_end");
        }

        //保存时间相关的配置信息
        private void saveConfig_Click(object sender, EventArgs e)
        {
            //string file = Application.ExecutablePath;
            //Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            CfgMgr.SaveValue("NewFansRefresh_Begin", NewFansRefresh_Begin.Text);
            CfgMgr.SaveValue("NewFansRefresh_End", NewFansRefresh_End.Text);
            CfgMgr.SaveValue("NewFansCall_Begin", NewFansCall_Begin.Text);
            CfgMgr.SaveValue("NewFansCall_End", NewFansCall_End.Text);
            CfgMgr.SaveValue("OldRefresh_Begin", OldRefresh_Begin.Text);
            CfgMgr.SaveValue("OldRefresh_End", OldRefresh_End.Text);
            CfgMgr.SaveValue("OldFansCall_Begin", OldFansCall_Begin.Text);
            CfgMgr.SaveValue("OldFansCall_End", OldFansCall_End.Text);
            CfgMgr.SaveValue("moreOffInterTime_begin", moreOffInterTime_begin.Text);
            CfgMgr.SaveValue("moreOffInterTime_end", moreOffInterTime_end.Text);
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("appSettings");
            weBoUserClient.NewFansRefresh_Begin = int.Parse(NewFansRefresh_Begin.Text);
            weBoUserClient.NewFansRefresh_End = int.Parse(NewFansRefresh_End.Text);
            weBoUserClient.NewFansCall_Begin = int.Parse(NewFansCall_Begin.Text);
            weBoUserClient.NewFansCall_End = int.Parse(NewFansCall_End.Text);
            weBoUserClient.OldRefresh_Begin = int.Parse(OldRefresh_Begin.Text);
            weBoUserClient.OldRefresh_End = int.Parse(OldRefresh_End.Text);
            weBoUserClient.OldFansCall_Begin = int.Parse(OldFansCall_Begin.Text);
            weBoUserClient.OldFansCall_End = int.Parse(OldFansCall_End.Text);
            weBoUserClient.moreOffInterTime_begin = int.Parse(moreOffInterTime_begin.Text);
            weBoUserClient.moreOffInterTime_end = int.Parse(moreOffInterTime_end.Text);
            this.Close();
        }
    }
}

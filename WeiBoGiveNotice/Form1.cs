using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeiBoGiveNotice
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }


        private WeBoUserClient weBoUserClient;

        private void SetControlText(Control control, string value)
        {
            control.Invoke(new MethodInvoker(delegate () { control.Text = value; }));
        }

        //初始化
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                #region 注册微博用户客户端

                weBoUserClient = new WeBoUserClient();
                weBoUserClient.QrCodeImageChange = delegate (string imageUrl)
                {
                    userPhoto.Invoke(new MethodInvoker(delegate () { userPhoto.Load(imageUrl); }));
                };
                weBoUserClient.UserInfoChange = delegate (WeiBoUser user)
                {
                    userPhoto.Invoke(new MethodInvoker(delegate () { userPhoto.Load(user.avatar_large); }));
                    SetControlText(userName, user.nick);
                };
                weBoUserClient.NumberRunsChange = delegate (int value)
                {
                    SetControlText(runNumber, value.ToString());
                };
                weBoUserClient.LastSendMessageTimeChange = delegate (DateTime? date)
                {
                    SetControlText(label7, date.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                };
                weBoUserClient.LastSendMessageUserChange = delegate (Fans fans)
                {
                    SetControlText(lastUserName, fans.nick);
                };
                weBoUserClient.OldFansSendMessageStartUserChange = delegate (Fans fans)
                {
                    SetControlText(beginSite, fans.nick);
                };
                weBoUserClient.TodaySendMessageCountChange = delegate (int value) { SetControlText(runNumberToday, value.ToString()); };
                weBoUserClient.IsSendMessageNewFansRunChange = delegate (bool value) { SetControlText(BeginDown, value ? "停止" : "开始"); };
                weBoUserClient.IsSendMeesageToOldFansRunChange = delegate (bool value) { SetControlText(button1, value ? "停止" : "开始"); };

                #endregion

                weBoUserClient.StartQrcodeLogin();

                //默认配置赋值，从配置中读取
                weBoUserClient.SetDefalutConfig();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //点击开始(向前打招呼)--给老粉丝发消息
        private void button1_Click(object sender, EventArgs e)
        {
            if (!weBoUserClient.IsSendMeesageToOldFansRun)
            {
                if (string.IsNullOrEmpty(officalToOld.Text))
                {
                    MessageBox.Show("文案不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(CallOldFansNum.Text))
                {
                    MessageBox.Show("向前打招呼数量不能为空");
                    return;
                }
                int CallOldFansNums = int.Parse(CallOldFansNum.Text);
                if (CallOldFansNums <= 0)
                {
                    MessageBox.Show("向前打招呼数量必须大于0");
                    return;
                }
                weBoUserClient.SendMeesageToOldFans(officalToOld.Text, CallOldFansNums, delegate (int value)
                {
                    SetControlText(CallCount, value.ToString());
                });
            }
            else
            {
                weBoUserClient.IsSendMeesageToOldFansRun = false;
            }
        }

        //点击系统配置
        private void setConfig_Click(object sender, EventArgs e)
        {
            ConfigFrom cf = new ConfigFrom(weBoUserClient);
            cf.ShowDialog();
            //可以执行此处代码
        }

        //点击开始(下方按钮)--给新粉丝发消息
        private void BeginDown_Click(object sender, EventArgs e)
        {
            if (!weBoUserClient.IsSendMessageNewFansRun)
            {
                if (string.IsNullOrEmpty(officical.Text))
                {
                    MessageBox.Show("文案不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(CallNewFansNum.Text))
                {
                    MessageBox.Show("向前打招呼数量不能为空");
                    return;
                }
                int CallNewFansNums = int.Parse(CallNewFansNum.Text);
                if (CallNewFansNums <= 0)
                {
                    MessageBox.Show("向前打招呼数量必须大于0");
                    return;
                }
                List<string> officicals = officical.Text.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None).ToList();
                officicals = officicals.Where(s => !string.IsNullOrEmpty(s)).ToList();
                weBoUserClient.ListenNewFans(officicals, CallNewFansNums, delegate (int value)
                {
                    SetControlText(label34, value.ToString());
                });
            }
            else
            {
                weBoUserClient.IsSendMessageNewFansRun = false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}

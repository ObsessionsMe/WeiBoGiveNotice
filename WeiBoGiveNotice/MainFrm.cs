using log4net;
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
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();

        }
        private ILog log = LogManager.GetLogger("weiBoLog");

        private WeiBoUserClient weBoUserClient;

        private void SetControlText(Control control, string value)
        {
            control.Invoke(new MethodInvoker(delegate (){ control.Text = value; }));
        }

        //初始化
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                userPhoto.SizeMode = PictureBoxSizeMode.StretchImage;

                if (!LoginFrm.VerifyCheckCode(MachineCode.GetShortMachineCodeString(), CfgMgr.GetValue("CheckCode")))
                {
                    //登陆校验
                    LoginFrm loginFrm = new LoginFrm();
                    if (loginFrm.ShowDialog() != DialogResult.OK)
                    {
                        this.Close();
                    };
                }

                #region 注册微博用户客户端

                weBoUserClient = new WeiBoUserClient();
                weBoUserClient.ErrorMessagNotice = delegate (Exception ex) { MessageBox.Show($"程序运行异常,请检查网络是否正常!异常信息:{ex.Message}", "错误提示", MessageBoxButtons.OK); };
                weBoUserClient.QrCodeImageChange = delegate (string imageUrl)
                {
                    userPhoto.Invoke(new MethodInvoker(delegate () { userPhoto.Load(imageUrl); }));
                };
                weBoUserClient.UserInfoChange = delegate (WeiBoUser user)
                {
                    userPhoto.Invoke(new MethodInvoker(delegate () { userPhoto.Load(user.avatar_large); }) );
                    SetControlText(userName, user.nick);
                    BeginOn.Invoke(new MethodInvoker(delegate () { BeginOn.Enabled = true; }));
                    BeginDown.Invoke(new MethodInvoker(delegate () { BeginDown.Enabled = true; }));
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
                weBoUserClient.TodaySendMessageCountChange = delegate (int value) { SetControlText(runNumberToday, value.ToString());
                
                };
                weBoUserClient.IsSendMessageNewFansRunChange = delegate (bool value)
                {
                    SetControlText(BeginDown, value ? "停止" : "开始");
                    BeginOn.Invoke(new MethodInvoker(delegate () { BeginOn.Enabled = !value; }));
                    BeginDown.Invoke(new MethodInvoker(delegate () { BeginDown.Enabled = !value; }));
                };
                weBoUserClient.IsSendMeesageToOldFansRunChange = delegate (bool value)
                {
                    SetControlText(BeginOn, value ? "停止" : "开始");
                    BeginOn.Invoke(new MethodInvoker(delegate () { BeginOn.Enabled = !value; }));
                    BeginDown.Invoke(new MethodInvoker(delegate () { BeginDown.Enabled = !value; }));
                };

                #endregion

                weBoUserClient.StartQrcodeLogin();

                //默认给页面的配置赋值，数据从配置中读取
                weBoUserClient.SetDefalutConfig();

                BeginOn.Enabled = false;
                BeginDown.Enabled = false;

                weBoUserClient.SentsMessageListByNew = new List<Fans>();
                weBoUserClient.SentsMessageListByOld = new List<Fans>();
            }
            catch (Exception ex)
            {
                log.Error("启动异常!", ex);
                MessageBox.Show(ex.Message);
            }
        }

        //点击开始(向前打招呼)--给老粉丝发消息
        private void button1_Click(object sender, EventArgs e)
        {
            if (!weBoUserClient.IsSendMeesageToOldFansRun)
            {
                if (string.IsNullOrEmpty(officalToOld.Text.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")))
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
                //打印已发送完消息的老粉丝列表
                foreach (var item in weBoUserClient.SentsMessageListByOld)
                {
                    weBoUserClient.PrintMsg(PrintType.info, "已发送完消息的老粉粉丝" + item.nick);
                }
                weBoUserClient.SentsMessageListByOld = new List<Fans>();
            }
        }

        //点击系统配置
        private void setConfig_Click(object sender, EventArgs e)
        {
            ConfigFrm cf = new ConfigFrm(weBoUserClient);
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
                if (CallNewFansNums > 9999)
                {
                    MessageBox.Show("向前打招呼数量必须小于9999");
                    return;
                }
                List<string> officicals = officical.Text.Split(new string[] { "\r\n\r\n\r\n" }, StringSplitOptions.None).ToList();
                officicals = officicals.Where(s => !string.IsNullOrEmpty(s)).ToList();
                weBoUserClient.ListenNewFans(officicals, CallNewFansNums, delegate (int value)
                {
                    SetControlText(label34, value.ToString());
                });
            }
            else
            {
                weBoUserClient.IsSendMessageNewFansRun = false;
                //打印已发送完消息的新粉丝列表
                foreach (var item in weBoUserClient.SentsMessageListByNew)
                {
                    weBoUserClient.PrintMsg(PrintType.info, "已发送完消息的老粉粉丝" + item.nick);
                }
                weBoUserClient.SentsMessageListByNew = new List<Fans>();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (weBoUserClient != null && weBoUserClient.SentsMessageListByOld.Count > 0 && weBoUserClient.SentsMessageListByNew.Count > 0)
            {
                //打印已发送完消息的老粉丝列表
                foreach (var item in weBoUserClient.SentsMessageListByOld)
                {
                    weBoUserClient.PrintMsg(PrintType.info, "已发送完消息的粉丝" + item.nick);
                }
                weBoUserClient.SentsMessageListByOld = new List<Fans>();
                //打印已发送完消息的新粉丝列表
                foreach (var item in weBoUserClient.SentsMessageListByNew)
                {
                    weBoUserClient.PrintMsg(PrintType.info, "已发送完消息的粉丝" + item.nick);
                }
                weBoUserClient.SentsMessageListByNew = new List<Fans>();
            }
            System.Environment.Exit(0);
        }
    }
}

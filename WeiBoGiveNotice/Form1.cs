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

        //初始化
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //注册微博用户客户端
                weBoUserClient = new WeBoUserClient();
                weBoUserClient.QrCodeImageChange=new WeBoUserClient.QrCodeLoginImageChange(QrCodeLoginImageChange);

                //显示图片
                //Image img = Image.FromFile(@"E:\PerTerProject\WeiBoGiveNotice\WeiBoGiveNotice\timg.jpg");
                //userPhoto.Image = img;
                //第一步: 监听二维码是否有扫码操作，获取到微博账号基础信息后在窗体上展示
                weBoUserClient.StartQrcodeLogin();
                //第二步: 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private  void QrCodeLoginImageChange(string imageUrl)
        {
            userPhoto.Load(imageUrl);
        }


        //点击开始(向前打招呼)--给老粉丝发消息
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(officical.Text))
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
            weBoUserClient.SendMeesageToOldFans(officical.Text, CallOldFansNums);
            //this.Enabled = true;
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
            weBoUserClient.ListenNewFans(officical.Text, CallNewFansNums);
        }
    }
}

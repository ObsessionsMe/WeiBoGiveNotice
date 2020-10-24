using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WeiBoGiveNotice
{
    /// <summary>
    /// 微博用户客户端（一个用户对应一个客户端实例，客户端提供该用户数据的访问入口）
    /// </summary>
    public class WeBoUserClient
    {
        public WeBoUserClient()
        {
            HttpHelper = new HttpHelper();
            loadCookie();
        }

        #region 委托类
        /// <summary>
        /// 二维码图片变更委托方法
        /// </summary>
        /// <param name="imageUrl"></param>
        public delegate void QrCodeLoginImageChange(string imageUrl);

        #endregion

        #region 公有属性

        /// <summary>
        /// 二维码获取与变更回调方法
        /// </summary>
        public QrCodeLoginImageChange QrCodeImageChange { get; set; }

        /// <summary>
        /// 当前用户对象
        /// </summary>
        public WeiBoUser WeiBoUser { get; set; }

        /// <summary>
        /// 最新粉丝列表
        /// </summary>
        public List<Fans> LatestFans { get; set; }

        /// <summary>
        /// 运行以来次数
        /// </summary>
        public int NumberRuns { get; set; }
        /// <summary>
        /// 最新发送消息的时间
        /// </summary>
        public DateTime LastSendMessageTime { get; set; }
        /// <summary>
        /// 最新发送消息的用户
        /// </summary>
        public string LastSendMessageUser { get; set; }
        /// <summary>
        /// 今日发送消息次数
        /// </summary>
        public int TodaySendMessageCount { get; set; }

        /// <summary>
        /// 是否在发消息给老粉丝
        /// </summary>
        public bool IsSendMeesageToOldFansRun = false;

        /// <summary>
        /// 是否在发消息给新粉丝
        /// </summary>
        public bool IsSendMessageNewFansRun = false;

        #endregion
        #region 私有属性
        /// <summary>
        /// 时间戳
        /// </summary>
        private long TimeStamp
        {
            get { return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000; }
        }

        /// <summary>
        /// 请求帮助类
        /// </summary>
        private HttpHelper HttpHelper { get; set; }

        private HttpItem HttpItem = new HttpItem();

        /// <summary>
        /// 二维码信息获取接口
        /// </summary>
        private const string QrCodeImageApi = "https://login.sina.com.cn/sso/qrcode/image?entry=weibo&size=180&callback=SKT_{0}";

        /// <summary>
        /// 二维码使用情况监听接口
        /// </summary>
        private const string QrCodeCheckApi = "https://login.sina.com.cn/sso/qrcode/check?entry=weibo&qrid={0}&callback=STK_{1}";

        /// <summary>
        /// 用户认证中心登陆接口
        /// </summary>
        private const string SSOLoginApi = "https://login.sina.com.cn/sso/login.php?entry=weibo&returntype=TEXT&crossdomain=1&cdult=3&domain=weibo.com&alt={0}&savestate=30&callback=STK_{1}";

        /// <summary>
        /// 粉丝列表页
        /// </summary>
        private const string SearchFansPage = "https://weibo.com/{0}/fans?Pl_Official_RelationFans__88_page={1}";

        /// <summary>
        /// 主页
        /// </summary>
        private const string HomePage = "https://weibo.com/";


        #endregion



        private void PrintMsg(PrintType printType, string message, Exception exception = null)
        {
            switch (printType)
            {
                case PrintType.info:
                    break;
                case PrintType.error:
                    break;
                default:
                    break;
            }
        }

        private void loadCookie()
        {
            HttpItem.Method = "GET";
            HttpItem.URL = HomePage;
            var HomePageHttpResult = HttpHelper.GetHtml(HttpItem);
        }

        private void SetCookie(string cookieStr)
        {

            //HttpItem.Cookie+= 
        }



        /// <summary>
        /// 开始扫码登陆
        /// </summary>
        public void StartQrcodeLogin()
        {

            //二维码更新委托方法注册检查
            if (QrCodeImageChange == null)
            {
                throw new Exception("QrCodeImageChange 未绑定!");
            }

            Thread qrcodeImageThread = new Thread(new ThreadStart(QrcodeImageThread));
            qrcodeImageThread.Start();
        }


        /// <summary>
        /// 粉丝查询
        /// </summary>
        /// <param name="pageNum">页号</param>
        public void SearchFans(int pageNum)
        {
            //粉丝查询
            HttpItem.Method = "GET";
            HttpItem.URL = string.Format(SearchFansPage, WeiBoUser.uid, TimeStamp);
            var SearchFansPageHttpResult = HttpHelper.GetHtml(HttpItem);

        }

        /// <summary>
        /// 侦听新粉丝
        /// </summary>
        /// <param name="message">发送的消息内容</param>
        /// <param name="maxUserCount">最大用户数量</param>
        public void ListenNewFans(string message,int maxUserCount)
        {
            //循环
            //判断是否继续发送
            if (IsSendMessageNewFansRun)
            {

            }
        }

        /// <summary>
        /// 发送消息给老粉丝
        /// </summary>
        /// <param name="message"></param>
        /// <param name="maxUserCount"></param>
        public void SendMeesageToOldFans(string message, int maxUserCount)
        {
            //循环
            //判断是否继续发送
            if (IsSendMeesageToOldFansRun)
            {

            }
        }

        private void SendMessage(string uid, string message)
        {

        }
        

        private void SetWeiBoUser(string HttpContent)
        {

        }

        private bool QrcodeImageSuccess { get; set; }

        private void QrcodeImageThread()
        {
            QrcodeImageSuccess = false;
            HttpItem.Method = "GET";
            while (!QrcodeImageSuccess)
            {
                try
                {
                    //获取二维码
                    HttpItem.URL = string.Format(QrCodeImageApi, TimeStamp);
                    var QrCodeImageApiHttpResult = HttpHelper.GetHtml(HttpItem);
                    if (QrCodeImageApiHttpResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var qrCodeApiRes = QrCodeImageApiHttpResult.Html.ToWeiBoJsonResult<QrImage>();
                        if (qrCodeApiRes.retcode == 20000000)
                        {
                            var imageUrl = qrCodeApiRes.data.image;
                            if (imageUrl.IndexOf("http") != 0)
                            {
                                imageUrl = "http:" + imageUrl;
                            }
                            QrCodeImageChange(imageUrl);
                        }
                        var startTime = DateTime.Now;
                        //超时后重新获取新的二维码
                        while ((DateTime.Now - startTime).Seconds < 50)
                        {
                            //检测扫码成功获取数据
                            HttpItem.URL = string.Format(QrCodeCheckApi, qrCodeApiRes.data.qrid, TimeStamp);
                            var QrCodeCheckApiHttpResult = HttpHelper.GetHtml(HttpItem);
                            var QrCodeCheckApiRes = QrCodeCheckApiHttpResult.Html.ToWeiBoJsonResult<QrImage>();
                            if (QrCodeCheckApiRes.retcode == 20000000)
                            {
                                //扫码成功
                                QrcodeImageSuccess = true;

                                //登陆用户认证中心
                                HttpItem.URL = string.Format(SSOLoginApi, QrCodeCheckApiRes.data.alt, TimeStamp);
                                var SSOLoginApiHttpResult = HttpHelper.GetHtml(HttpItem);
                                //保存用户cookie
                                SetCookie(SSOLoginApiHttpResult.Cookie);
                                var SSOLoginApiRes = SSOLoginApiHttpResult.Html.ToWeiBoJsonResult<string>();
                                if (SSOLoginApiRes.retcode == 0)
                                {
                                    //登陆成功
                                    WeiBoUser = new WeiBoUser();
                                    WeiBoUser.uid = SSOLoginApiRes.uid;
                                    //初始化粉丝列表
                                    SearchFans(1);
                                }

                                break;
                            }
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintMsg(PrintType.error, "扫码登陆异常!", ex);
                }
            }
        }


    }

    public enum PrintType
    {
        info = 0,
        error = 1
    }


    public class WeiBoUser
    {
        /// <summary>
        /// 
        /// </summary>
        public string islogin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string oid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string page_id { get; set; }
        /// <summary>
        /// 会看脸的俏小姐
        /// </summary>
        public string onick { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string skin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string background { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string scheme { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string colors_type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 会看脸的俏小姐
        /// </summary>
        public string nick { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string watermark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string domain { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string lang { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string avatar_large { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string servertime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string location { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pageid { get; set; }
        /// <summary>
        /// 会看脸的俏小姐的微博_微博
        /// </summary>
        public string title_value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string webim { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string miyou { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string brand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bigpipe { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string bpType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pid { get; set; }
    }

    public class JsonResult<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public int retcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public T data { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string nick { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> crossDomainUrlList { get; set; }
    }

    /// <summary>
    /// 粉丝对象
    /// </summary>
    public class Fans
    {

    }

    public class QrImage
    {
        /// <summary>
        /// 
        /// </summary>
        public string qrid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string image { get; set; }

        public string alt { get; set; }
    }

    public static class StringExtension
    {
        public static JsonResult<T> ToWeiBoJsonResult<T>(this string str)
        {

            Regex regex = new Regex("({.*})");
            var jsonConten = regex.Match(str).Value;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JsonResult<T>>(jsonConten);
        }
    }
}

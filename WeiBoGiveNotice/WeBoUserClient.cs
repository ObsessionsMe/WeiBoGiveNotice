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

        private HttpItem GetHttpItem = new HttpItem();

        private HttpItem PostHttpItem = new HttpItem();

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

        /// <summary>
        ///  发消息api
        /// </summary>
        private const string SendMessageApi = "https://api.weibo.com/webim/2/direct_messages/new.json";

        private Dictionary<string, string> Cookies = new Dictionary<string, string>();

        /// <summary>
        /// 是否初始化中
        /// </summary>
        private bool IsInit = true;

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
            GetHttpItem.Method = "GET";
            GetHttpItem.URL = HomePage;
            var HomePageHttpResult = HttpHelper.GetHtml(GetHttpItem);
        }

        private void SetCookie(string cookieStr)
        {
            cookieStr += ";";
            cookieStr = Regex.Replace(cookieStr, "(path=.*?,)|(expires=.*?;)|(path=.*?;)|(domain=.*?;)|(Max-Age=.*?;)", "").Trim();
            var filterKeys = new List<string>() { "path", "domain", "SameSite", "httponly", "expires" };
            var cookies = cookieStr.Split(';');
            foreach (string cookie in cookies)
            {
                if (cookie.Contains("="))
                {
                    var ck_keyVal = cookie.Split('=');
                    var key = ck_keyVal[0].Trim();
                    var value = cookie.Substring(cookie.IndexOf(key) + key.Length + 1).Trim();
                    if (!filterKeys.Contains(key))
                    {
                        if (Cookies.Keys.Contains(key))
                        {
                            Cookies.Remove(key);
                        }
                        if (value != "deleted")
                        {
                            Cookies.Add(key, value);
                        }
                    }
                }
            }
            GetHttpItem.Cookie = string.Join(";", Cookies.Select(s => s.Key + "=" + s.Value));
            PostHttpItem.Cookie = string.Join(";", Cookies.Select(s => s.Key + "=" + s.Value));
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
        public List<Fans> SearchFans(int pageNum)
        {
            var Res = new List<Fans>();
            //粉丝查询
            GetHttpItem.Method = "GET";
            GetHttpItem.URL = string.Format(SearchFansPage, WeiBoUser.uid, pageNum);
            var SearchFansPageHttpResult = HttpHelper.GetHtml(GetHttpItem);

            var fansList = Regex.Matches(SearchFansPageHttpResult.Html, "<img usercard=\\\\\"id=(.*?)&refer_flag=1005050005_\\\\\" width=\\\\\"50\\\\\" height=\\\\\"50\\\\\" alt=\\\\\"(.*?)\\\\\" src=\\\\\"(.*?)\\\\\">");

            //解析粉丝列表
            Fans fans = null;
            LatestFans = new List<Fans>();
            foreach (Match match in fansList)
            {
                fans = new Fans();
                fans.uid = match.Groups[1].Value;
                fans.nick = match.Groups[2].Value;
                fans.image = match.Groups[3].Value;
                Res.Add(fans);
            }

            if (pageNum == 1 && IsInit)
            {
                SetWeiBoUser(SearchFansPageHttpResult.Html);
                LatestFans = Res;
            }
            return Res;
        }

        /// <summary>
        /// 侦听新粉丝
        /// </summary>
        /// <param name="message">发送的消息内容</param>
        /// <param name="maxUserCount">最大用户数量</param>
        public void ListenNewFans(string message, int maxUserCount)
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

        public void SendMessage(string uid, string message)
        {
            PostHttpItem.Method = "post";
            PostHttpItem.URL = SendMessageApi;
            PostHttpItem.ContentType = "application/x-www-form-urlencoded";
            PostHttpItem.Header.Add("Origin", "https://api.weibo.com");
            PostHttpItem.Referer = "https://api.weibo.com/chat/ ";
            PostHttpItem.Postdata = "text=hello&uid=6823169570&extensions={\"clientid\":\"ioum121csoxafeztq1x6wymifkx37z\"}&is_encoded=0&decodetime=1&source=209678993";
            HttpResult result = HttpHelper.GetHtml(PostHttpItem);
            var code = result.Html.ToWeiBoJsonResult<object>();
            if (code.error_code > 0)
            {
                PrintMsg(PrintType.error, "方法:SendMessage 出错" + code.error);
            }
        }


        private void SetWeiBoUser(string HttpContent)
        {
            string values = string.Empty;
            var Matches = Regex.Matches(HttpContent, "\\['.*?'\\]='.*?';");
            foreach (Match Match in Matches)
            {
                values += Match.Value;
            }
            string jsonString = "{" + values.Replace("'", "\"").Replace("[", "").Replace("]=", ":").Replace(";", ",").TrimEnd(',') + "}";
            WeiBoUser = Newtonsoft.Json.JsonConvert.DeserializeObject<WeiBoUser>(jsonString);
        }

        private bool QrcodeImageSuccess { get; set; }

        private void QrcodeImageThread()
        {
            QrcodeImageSuccess = false;
            GetHttpItem.Method = "GET";
            while (!QrcodeImageSuccess)
            {
                try
                {
                    //获取二维码
                    GetHttpItem.URL = string.Format(QrCodeImageApi, TimeStamp);
                    var QrCodeImageApiHttpResult = HttpHelper.GetHtml(GetHttpItem);
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
                            GetHttpItem.URL = string.Format(QrCodeCheckApi, qrCodeApiRes.data.qrid, TimeStamp);
                            var QrCodeCheckApiHttpResult = HttpHelper.GetHtml(GetHttpItem);
                            var QrCodeCheckApiRes = QrCodeCheckApiHttpResult.Html.ToWeiBoJsonResult<QrImage>();
                            if (QrCodeCheckApiRes.retcode == 20000000)
                            {
                                //登陆用户认证中心
                                GetHttpItem.URL = string.Format(SSOLoginApi, QrCodeCheckApiRes.data.alt, TimeStamp);
                                var SSOLoginApiHttpResult = HttpHelper.GetHtml(GetHttpItem);
                                //保存用户cookie
                                SetCookie(SSOLoginApiHttpResult.Cookie);
                                var SSOLoginApiRes = SSOLoginApiHttpResult.Html.ToWeiBoJsonResult<string>();
                                if (SSOLoginApiRes.retcode == 0)
                                {
                                    foreach (string crossDomainUrl in SSOLoginApiRes.crossDomainUrlList)
                                    {
                                        //设置核心cookie
                                        GetHttpItem.URL = crossDomainUrl;
                                        var crossDomainUrl_HttpResult = HttpHelper.GetHtml(GetHttpItem);
                                        SetCookie(crossDomainUrl_HttpResult.Cookie);
                                    }

                                    //登陆成功
                                    WeiBoUser = new WeiBoUser();
                                    WeiBoUser.uid = SSOLoginApiRes.uid;
                                    //初始化粉丝列表
                                    SearchFans(1);
                                    //设置二维码图片区显示用户头像
                                    QrCodeImageChange(WeiBoUser.avatar_large);

                                    //扫码登陆成功
                                    QrcodeImageSuccess = true;
                                    //初始化结束
                                    IsInit = false;
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

        public string error { get; set; }
        public string request { get; set; }
        public int error_code { get; set; }
    }

    /// <summary>
    /// 粉丝对象
    /// </summary>
    public class Fans
    {
        public string uid { get; set; }

        public string nick { get; set; }

        public string image { get; set; }
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

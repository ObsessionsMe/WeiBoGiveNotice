using log4net;
using Newtonsoft.Json.Linq;
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
        /// 客户端版本
        /// </summary>
        public VersionLevel VersionLevel { get; set; }

        /// <summary>
        /// 粉丝总页数
        /// </summary>
        public int FansPageCount { get; set; }

        /// <summary>
        /// 最新粉丝
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

        //系统配置信息
        public int NewFansRefresh_Begin { get; set; }
        public int NewFansRefresh_End { get; set; }
        public int NewFansCall_Begin { get; set; }
        public int NewFansCall_End { get; set; }

        public int OldRefresh_Begin { get; set; }
        public int OldRefresh_End { get; set; }
        public int OldFansCall_Begin { get; set; }
        public int OldFansCall_End { get; set; }
        #endregion
        #region 私有属性
        /// <summary>
        /// 时间戳
        /// </summary>
        private long TimeStamp
        {
            get { return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000; }
        }


        private ILog log = LogManager.GetLogger("weiBoLog");

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
        /// 粉丝接口(一版本)
        /// </summary>
        private const string SearchFansApiLv1 = "https://weibo.com/{0}/fans?Pl_Official_RelationFans__88_page={1}";

        /// <summary>
        /// 粉丝接口（二版本）
        /// </summary>
        private const string SearchFansApiLv2 = "https://weibo.com/ajax/friendships/friends?relate=fans&page={0}&uid={1}&type=all";

        private const string isExistFansPage = "https://weibo.com/{1}/fans?search={0}";


        /// <summary>
        ///  发消息api
        /// </summary>
        private const string SendMessageApi = "https://api.weibo.com/webim/2/direct_messages/new.json";

        private Dictionary<string, string> Cookies = new Dictionary<string, string>();


        #endregion


        private void PrintMsg(PrintType printType, string message, Exception exception = null)
        {
            switch (printType)
            {
                case PrintType.info:
                    log.Debug(message);
                    break;
                case PrintType.error:
                    log.Error(message, exception);
                    break;
                default:
                    break;
            }
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
            PrintMsg(PrintType.info, $"StartQrcodeLogin 开始扫码登陆!");

        }

        private void InitLv1Fans(string HtmlContent)
        {
            var Res = new List<Fans>();
            //解析粉丝列表
            var fansList = Regex.Matches(HtmlContent, "<img usercard=\\\\\"id=(.*?)&refer_flag=1005050005_\\\\\" width=\\\\\"50\\\\\" height=\\\\\"50\\\\\" alt=\\\\\"(.*?)\\\\\" src=\\\\\"(.*?)\\\\\">");

            //解析粉丝总页数
            var pageNumMatches = Regex.Matches(HtmlContent, "Pl_Official_RelationFans__88_page=(\\d+)#");
            if (pageNumMatches.Count > 0)
            {
                 FansPageCount = pageNumMatches.Cast<Match>().Select(s => Convert.ToInt32(s.Groups[1].Value)).Max();
            }
            else
            {
                FansPageCount = 1;
            }

            Res = fansList.Cast<Match>().Select(s => new Fans()
            {
                uid = s.Groups[1].Value,
                nick = s.Groups[2].Value,
                image = s.Groups[3].Value
            }).ToList();

            LatestFans = Res;
        }

        /// <summary>
        /// 初始化用户相关数据
        /// </summary>
        public void InitWeiBoUser()
        {
            GetHttpItem.Method = "GET";
            GetHttpItem.URL = string.Format(SearchFansApiLv1, WeiBoUser.uid, 1);
            var SearchFansPageHttpResult = HttpHelper.GetHtml(GetHttpItem);

            SetWeiBoUser(SearchFansPageHttpResult.Html);

            switch (VersionLevel)
            {
                case VersionLevel.None:
                    break;
                case VersionLevel.Lv1:
                    InitLv1Fans(SearchFansPageHttpResult.Html);
                    break;
                case VersionLevel.Lv2:
                    InitLv2Fans();
                    break;
                default:
                    break;
            }


            PrintMsg(PrintType.info, $"InitWeiBoUser 初始化成功");
        }

        private void InitLv2Fans()
        {
            LatestFans = SearchFansLv2(1);
        }


        /// <summary>
        /// 粉丝查询(二版本)
        /// </summary>
        /// <param name="pageNum">页号</param>
        public List<Fans> SearchFansLv2(int pageNum)
        {
            var Res = new List<Fans>();
            GetHttpItem.Method = "GET";
            GetHttpItem.URL = string.Format(SearchFansApiLv2, 1, WeiBoUser.uid);
            var SearchFansPageHttpResult = HttpHelper.GetHtml(GetHttpItem);
            var JsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(SearchFansPageHttpResult.Html);
            Res = JsonObject["users"].Value<JArray>().Select(s => new Fans
            {
                uid = s.Value<string>("id"),
                nick = s.Value<string>("screen_name"),
                image = s.Value<string>("avatar_large"),
            }).ToList();
            return Res;
        }

        /// <summary>
        /// 粉丝查询(一版本)
        /// </summary>
        /// <param name="pageNum">页号</param>
        public List<Fans> SearchFansLv1(int pageNum)
        {
            var Res = new List<Fans>();
            //粉丝查询
            GetHttpItem.Method = "GET";
            GetHttpItem.URL = string.Format(SearchFansApiLv1, WeiBoUser.uid, pageNum);
            var SearchFansPageHttpResult = HttpHelper.GetHtml(GetHttpItem);

            var fansList = Regex.Matches(SearchFansPageHttpResult.Html, "<img usercard=\\\\\"id=(.*?)&refer_flag=1005050005_\\\\\" width=\\\\\"50\\\\\" height=\\\\\"50\\\\\" alt=\\\\\"(.*?)\\\\\" src=\\\\\"(.*?)\\\\\">");

            //解析粉丝列表
            Fans fans = null;
            foreach (Match match in fansList)
            {
                fans = new Fans();
                fans.uid = match.Groups[1].Value;
                fans.nick = match.Groups[2].Value;
                fans.image = match.Groups[3].Value;
                Res.Add(fans);
            }

            PrintMsg(PrintType.info, $"SearchFansLv1 查询成功 pageNum:{pageNum}!");
            return Res;
        }

        /// <summary>
        /// 侦听新粉丝
        /// </summary>
        /// <param name="message">发送的消息内容</param>
        /// <param name="maxUserCount">最大用户数量</param>
        public void ListenNewFans(string message, int maxUserCount)
        {
            Thread thread = new Thread(() => SendMeesageToNewFans(message, maxUserCount));
            thread.Start();
        }

        private void SendMeesageToNewFans(string message, int maxUserCount)
        {
            try
            {
                IsSendMessageNewFansRun = true;
                var sentFans = new List<Fans>();
                int sentFansNum = 0;
                //判断是否继续发送
                while (IsSendMessageNewFansRun)
                {
                    //1从第一页的粉丝列表开始找哪些是新粉丝,依次类推
                    if (LatestFans == null)
                    {
                        return;
                    }
                    int pageNum = 0;
                    var fansList = new List<Fans>();
                    bool isExistlastFans = true;
                    bool bo = true;
                    sentFans = new List<Fans>();
                    while (isExistlastFans)
                    {
                        pageNum++;
                        //fansList = SearchFansLv1(pageNum);
                        fansList = SearchFnas(pageNum);
                        if (fansList.Count == 0)
                        {
                            return;
                        }
                        //找第一页的粉丝
                        while (!isExistFans(LatestFans[0].nick) && pageNum == 1)
                        {
                            LatestFans.RemoveAt(0);
                            if (fansList.Count == 0)
                            {
                                LatestFans = fansList;
                                bo = false;
                                break;
                            }
                        }
                        if (bo)
                        {
                            //发消息
                            for (int i = 0; i < fansList.Count; i++)
                            {
                                if (fansList[i].uid == LatestFans[0].uid)
                                {
                                    break;
                                }
                                SendMessage(fansList[i].uid, message);
                                sentFansNum++;
                                if (maxUserCount == sentFansNum)
                                {
                                    //达到打招呼上线后，退出循环
                                    IsSendMessageNewFansRun = false;
                                }
                                Thread.Sleep(5000);
                            }
                            //2如果找到上次最后一个粉丝就停止循环
                            isExistlastFans = fansList.SingleOrDefault(x => x.uid == LatestFans[0].uid) != null ? false : true;
                        }
                        sentFans.AddRange(fansList);
                        Thread.Sleep(5000);
                    }
                    LatestFans = sentFans;
                }
            }
            catch (Exception ex)
            {
                PrintMsg(PrintType.info, "方法:ListenNewFans: 正在给新粉丝发消息" + ex.ToString());
            }
        }

        //判断是否粉是否取关了
        public bool isExistFans(string nick)
        {
            GetHttpItem.URL = string.Format(isExistFansPage, nick, WeiBoUser.uid);
            var httpResult = HttpHelper.GetHtml(GetHttpItem);
            var reg = new Regex("共搜索到(\\d+)个关于");
            Match mc = reg.Match(httpResult.Html);
            return int.Parse(mc.Groups[1].Value) > 0 ? true : false;
        }

        /// <summary>
        /// 发送消息给老粉丝
        /// </summary>
        /// <param name="message"></param>
        /// <param name="maxUserCount"></param>
        public void SendMeesageToOldFans(string message, int maxUserCount)
        {
            Thread thread = new Thread(() => SendMeesageToOldFun(message, maxUserCount));
            thread.Start();
        }

        private void SendMeesageToOldFun(string message, int maxUserCount)
        {
            //判断是否继续发送
            //判断是否继续发送
            var fansList = new List<Fans>();
            int sentCount = 0;
            int pageNum = 1;
            IsSendMeesageToOldFansRun = true;
            while (IsSendMeesageToOldFansRun)
            {
                fansList = SearchFnas(pageNum);
                pageNum++;
                if (fansList.Count == 0)
                {
                    IsSendMeesageToOldFansRun = false;
                    return;
                }
                foreach (var item in fansList)
                {
                    sentCount++;
                    SendMessage(item.uid, message);
                    PrintMsg(PrintType.info, "方法:SendMeesageToOldFun: 正在给老粉丝发消息" + item.nick);
                    Thread.Sleep(RandomNumber());
                    PrintMsg(PrintType.info, "方法:SendMeesageToOldFun: 正在给新粉丝发消息，睡眠毫秒数为：" + RandomNumber());
                    if (sentCount >= maxUserCount)
                    {
                        IsSendMeesageToOldFansRun = false;
                        return;
                    }
                }
                Thread.Sleep(RandomNumber(10, 20));
                PrintMsg(PrintType.info, "方法:SendMeesageToOldFun: 刷新粉丝时间，睡眠毫秒数为：" + RandomNumber(10, 20));
            }
        }

        private List<Fans> SearchFnas(int pageNum)
        {
            var fans = new List<Fans>();
            switch (VersionLevel)
            {
                case VersionLevel.None:
                    break;
                case VersionLevel.Lv1:
                    //老版
                    fans = SearchFansLv1(pageNum);
                    break;
                case VersionLevel.Lv2:
                    fans = SearchFansLv2(pageNum);
                    break;
                default:
                    break;
            }
            return fans;
        }

        public void SendMessage(string uid, string message)
        {
            PostHttpItem.Method = "post";
            PostHttpItem.URL = SendMessageApi;
            PostHttpItem.ContentType = "application/x-www-form-urlencoded";
            PostHttpItem.Header.Add("Origin", "https://api.weibo.com");
            PostHttpItem.Referer = "https://api.weibo.com/chat/ ";
            //PostHttpItem.Encoding = Encoding.UTF8;
            message = System.Web.HttpUtility.UrlEncode(message);
            PostHttpItem.Postdata = $"text={message}&uid={uid}&extensions={{\"clientid\":\"ioum121csoxafeztq1x6wymifkx37z\"}}&is_encoded=0&decodetime=1&source=209678993";
            HttpResult result = HttpHelper.GetHtml(PostHttpItem);
            var code = result.Html.ToWeiBoJsonResult<object>();
            //判断是否发送失败
            if (code.error_code > 0)
            {
                PrintMsg(PrintType.error, "方法:SendMessage 出错" + code.error);
            }
        }

        /// <summary>
        ///  获取两者间随机数
        /// </summary>
        /// <param name="minVal">最小值，默认20</param>
        /// <param name="maxVal">最大值，默认40</param>
        /// <returns></returns>
        public int RandomNumber(int minVal = 20, int maxVal = 40)
        {
            int a = new Random().Next(minVal * 1000, maxVal * 1000);
            return new Random().Next(minVal * 1000, maxVal * 1000);
        }

        private void SetWeiBoUser(string HttpContent)
        {
            var version = VersionLevel.None;

            var versionMatch = Regex.Match(HttpContent, "CLIENT: '(.*?)'");
            if (string.IsNullOrEmpty(versionMatch.Value))
            {
                version = VersionLevel.Lv1;
                string values = string.Empty;
                var Matches = Regex.Matches(HttpContent, "\\['.*?'\\]='.*?';");
                foreach (Match Match in Matches)
                {
                    values += Match.Value;
                }
                string jsonString = "{" + values.Replace("'", "\"").Replace("[", "").Replace("]=", ":").Replace(";", ",").TrimEnd(',') + "}";
                WeiBoUser = Newtonsoft.Json.JsonConvert.DeserializeObject<WeiBoUser>(jsonString);
            }
            else
            {
                version = VersionLevel.Lv2;
                PrintMsg(PrintType.info, $"SetWeiBoUser 当前版本号 :{versionMatch.Groups[1].Value}!");
                var cfgMatche = Regex.Match(HttpContent, "window.\\$CONFIG = (\\{.*?\\});\\}catch");
                var JsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(cfgMatche.Groups[1].Value);
                WeiBoUser = new WeiBoUser()
                {
                    uid = JsonObject.Value<string>("uid"),
                    nick = JsonObject["user"].Value<string>("screen_name"),
                    avatar_large = JsonObject["user"].Value<string>("avatar_large")
                };
            }
            PrintMsg(PrintType.info, "SetWeiBoUser 用户信息更新成功!");

            VersionLevel = version;
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
                            PrintMsg(PrintType.info, "QrcodeImageThread_QrCodeImageApiHttpResult 获取二维码成功!");
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
                                PrintMsg(PrintType.info, "QrcodeImageThread_QrCodeCheckApiRes_扫码成功!");
                                //登陆用户认证中心
                                GetHttpItem.URL = string.Format(SSOLoginApi, QrCodeCheckApiRes.data.alt, TimeStamp);
                                var SSOLoginApiHttpResult = HttpHelper.GetHtml(GetHttpItem);
                                //保存用户cookie
                                SetCookie(SSOLoginApiHttpResult.Cookie);
                                var SSOLoginApiRes = SSOLoginApiHttpResult.Html.ToWeiBoJsonResult<string>();
                                if (SSOLoginApiRes.retcode == 0)
                                {
                                    PrintMsg(PrintType.info, "QrcodeImageThread_SSOLoginApiRes_用户认证中心通过!");
                                    //foreach (string crossDomainUrl in SSOLoginApiRes.crossDomainUrlList)
                                    //{
                                    //    //设置核心cookie
                                    //    GetHttpItem.URL = crossDomainUrl;
                                    //    var crossDomainUrl_HttpResult = HttpHelper.GetHtml(GetHttpItem);
                                    //    SetCookie(crossDomainUrl_HttpResult.Cookie);
                                    //}
                                    //设置核心cookie
                                    GetHttpItem.URL = SSOLoginApiRes.crossDomainUrlList[SSOLoginApiRes.crossDomainUrlList.Count - 1];
                                    var crossDomainUrl_HttpResult = HttpHelper.GetHtml(GetHttpItem);
                                    SetCookie(crossDomainUrl_HttpResult.Cookie);
                                    PrintMsg(PrintType.info, "QrcodeImageThread_SSOLoginApiRes_登陆成功!");

                                    //登陆成功
                                    WeiBoUser = new WeiBoUser();
                                    WeiBoUser.uid = SSOLoginApiRes.uid;
                                    //初始化用户信息
                                    InitWeiBoUser();
                                    //设置二维码图片区显示用户头像
                                    QrCodeImageChange(WeiBoUser.avatar_large);

                                    //扫码登陆成功
                                    QrcodeImageSuccess = true;
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


    public enum VersionLevel
    {
        None = 0,
        Lv1 = 1,
        Lv2 = 2
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

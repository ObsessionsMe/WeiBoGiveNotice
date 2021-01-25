using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class WeiBoUserClient
    {
        public WeiBoUserClient()
        {
            HttpHelper = new HttpHelper();
        }

        #region 委托类

        /// <summary>
        /// 通用值变更回调委托类
        /// </summary>
        /// <param name="value"></param>
        public delegate void ValueChange<T>(T value);

        #endregion


        #region 委托属性

        /// <summary>
        /// 全局异常通知
        /// </summary>
        public ValueChange<Exception> ErrorMessagNotice { get; set; }

        /// <summary>
        /// 二维码获取与变更回调方法
        /// </summary>
        public ValueChange<string> QrCodeImageChange { get; set; }

        /// <summary>
        /// 用户信息变更回调方法
        /// </summary>
        public ValueChange<WeiBoUser> UserInfoChange { get; set; }


        /// <summary>
        /// 运行以来次数变更回调
        /// </summary>
        public ValueChange<int> NumberRunsChange { get; set; }

        /// <summary>
        /// 最新发送消息的时间变更回调
        /// </summary>
        public ValueChange<DateTime?> LastSendMessageTimeChange { get; set; }

        /// <summary>
        /// 最新发送消息的用户变更回调
        /// </summary>
        public ValueChange<Fans> LastSendMessageUserChange { get; set; }

        /// <summary>
        /// 老粉丝发消息开始位置用户变更回调
        /// </summary>
        public ValueChange<Fans> OldFansSendMessageStartUserChange { get; set; }

        /// <summary>
        /// 今日发送消息次数变更回调
        /// </summary>
        public ValueChange<int> TodaySendMessageCountChange { get; set; }

        /// <summary>
        /// 是否在发消息给老粉丝标识变更回调
        /// </summary>
        public ValueChange<bool> IsSendMeesageToOldFansRunChange { get; set; }

        /// <summary>
        /// 是否在发消息给新粉丝标识变更回调
        /// </summary>
        public ValueChange<bool> IsSendMessageNewFansRunChange { get; set; }

        #endregion

        #region 公有属性


        private int _NumberRuns = 0;
        /// <summary>
        /// 运行以来次数
        /// </summary>
        public int NumberRuns
        {
            get { return _NumberRuns; }
            set
            {
                if (NumberRunsChange != null)
                {
                    NumberRunsChange(value);
                }
                _NumberRuns = value;
            }
        }



        private DateTime? _LastSendMessageTime = null;
        /// <summary>
        /// 最新发送消息的时间
        /// </summary>
        public DateTime? LastSendMessageTime
        {
            get { return _LastSendMessageTime; }
            set
            {
                if (LastSendMessageTimeChange != null)
                {
                    LastSendMessageTimeChange(value);
                }
                _LastSendMessageTime = value;
            }
        }


        private Fans _LastSendMessageUser = null;
        /// <summary>
        /// 最新发送消息的用户
        /// </summary>
        public Fans LastSendMessageUser
        {
            get { return _LastSendMessageUser; }
            set
            {
                if (LastSendMessageUserChange != null)
                {
                    LastSendMessageUserChange(value);
                }
                _LastSendMessageUser = value;
            }
        }



        public int _TodaySendMessageCount = 0;
        /// <summary>
        /// 今日发送消息次数
        /// </summary>
        public int TodaySendMessageCount
        {
            get { return _TodaySendMessageCount; }
            set
            {
                if (TodaySendMessageCountChange != null)
                {
                    TodaySendMessageCountChange(value);
                }
                _TodaySendMessageCount = value;
            }
        }

        private bool _IsSendMeesageToOldFansRun = false;
        /// <summary>
        /// 是否在发消息给老粉丝
        /// </summary>
        public bool IsSendMeesageToOldFansRun
        {
            get { return _IsSendMeesageToOldFansRun; }
            set
            {
                if (IsSendMeesageToOldFansRunChange != null)
                {
                    IsSendMeesageToOldFansRunChange(value);
                }
                _IsSendMeesageToOldFansRun = value;
            }
        }


        public bool _IsSendMessageNewFansRun = false;
        /// <summary>
        /// 是否在发消息给新粉丝
        /// </summary>
        public bool IsSendMessageNewFansRun
        {
            get { return _IsSendMessageNewFansRun; }
            set
            {
                if (IsSendMessageNewFansRunChange != null)
                {
                    IsSendMessageNewFansRunChange(value);
                }
                _IsSendMessageNewFansRun = value;
            }
        }


        private WeiBoUser _WeiBoUser = null;
        /// <summary>
        /// 当前用户对象
        /// </summary>
        public WeiBoUser WeiBoUser
        {
            get { return _WeiBoUser; }
            set
            {
                if (UserInfoChange != null)
                {
                    UserInfoChange(value);
                }
                _WeiBoUser = value;
            }
        }

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

        //系统配置信息
        public int NewFansRefresh_Begin { get; set; }
        public int NewFansRefresh_End { get; set; }
        public int NewFansCall_Begin { get; set; }
        public int NewFansCall_End { get; set; }

        public int OldRefresh_Begin { get; set; }
        public int OldRefresh_End { get; set; }
        public int OldFansCall_Begin { get; set; }
        public int OldFansCall_End { get; set; }
        public int moreOffInterTime_begin { get; set; }
        public int moreOffInterTime_end { get; set; }
        //已发送列表
        public List<Fans> SentsMessageListByOld { get; set; }

        public List<Fans> SentsMessageListByNew { get; set; }
        #endregion

        #region 私有属性

        private string uid { get; set; }


        private string _QrImageUrl;
        /// <summary>
        /// 二维码图像属性
        /// </summary>
        private string QrImageUrl
        {
            get { return _QrImageUrl; }
            set
            {
                if (QrCodeImageChange != null)
                {
                    QrCodeImageChange(value);
                }
                _QrImageUrl = value;
            }
        }


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

        private const string ChangeVersionApi = "https://weibo.com/ajax/changeversion?status={0}";

        /// <summary>
        /// 用户标识cookie
        /// </summary>
        private Dictionary<string, string> Cookies = new Dictionary<string, string>();
        #endregion


        /// <summary>
        /// 统一消息处理
        /// </summary>
        /// <param name="printType"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void PrintMsg(PrintType printType, string message, Exception exception = null)
        {
            switch (printType)
            {
                case PrintType.info:
                    log.Debug(message);
                    break;
                case PrintType.error:
                    log.Error(message, exception);
                    if (ErrorMessagNotice != null)
                    {
                        ErrorMessagNotice(exception);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 更新cookie
        /// </summary>
        /// <param name="cookieStr"></param>
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
            //GetHttpItem.Cookie = );
            //PostHttpItem.Cookie = string.Join(";", Cookies.Select(s => s.Key + "=" + s.Value));
        }

        private HttpItem CreateHttpItem()
        {
            return new HttpItem() { RequestRetryNumber = 5, Cookie = string.Join(";", Cookies.Select(s => s.Key + "=" + s.Value)),Referer= "https://weibo.com/" };
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

            Thread qrcodeImageThread = new Thread(() => QrcodeImageThread());
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
            var GetHttpItem = CreateHttpItem();
            GetHttpItem.Method = "GET";
            GetHttpItem.URL = string.Format(SearchFansApiLv1, uid, 1);
            var SearchFansPageHttpResult = HttpHelper.GetHtml(GetHttpItem);

            if (string.IsNullOrEmpty(SearchFansPageHttpResult.Html))
            {
                string errorMsg = "账号异常，请登录微博PC端解除!";
                PrintMsg(PrintType.error, errorMsg);
                throw new Exception(errorMsg);
            }

            SetWeiBoUser(SearchFansPageHttpResult.Html);

            LatestFans = SearchFnas(1);

            PrintMsg(PrintType.info, $"InitWeiBoUser 初始化成功");
            PrintMsg(PrintType.info, $"InitWeiBoUser 初始化成功后:LatestFans数据: {JsonConvert.SerializeObject(LatestFans)}_fansList:{JsonConvert.SerializeObject(LatestFans)}");
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
            var GetHttpItem = CreateHttpItem();
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
            var GetHttpItem = CreateHttpItem();
            GetHttpItem.Method = "GET";
            GetHttpItem.URL = string.Format(SearchFansApiLv1, WeiBoUser.uid, pageNum);
            //xp-修改
            HttpResult SearchFansPageHttpResult = new HttpResult();
            while (!SearchFansPageHttpResult.Html.Contains("粉丝列表"))
            {
                SearchFansPageHttpResult = HttpHelper.GetHtml(GetHttpItem);
                Thread.Sleep(RandomNumber(2, 5));
            }
            var fansList = Regex.Matches(SearchFansPageHttpResult.Html, "<img usercard=\\\\\"id=(.*?)&refer_flag=.*?\\\\\" width=\\\\\"50\\\\\" height=\\\\\"50\\\\\" alt=\\\\\"(.*?)\\\\\" src=\\\\\"(.*?)\\\\\">");
            //解析粉丝列表
            Fans fans = null;
            if (fansList.Count > 0)
            {
                foreach (Match match in fansList)
                {
                    fans = new Fans();
                    fans.uid = match.Groups[1].Value;
                    fans.nick = match.Groups[2].Value;
                    fans.image = match.Groups[3].Value;
                    Res.Add(fans);
                }
            }
            else
            {
                PrintMsg(PrintType.error, $"SearchFansLv1 查询失败 SearchFansPageHttpResult:{JsonConvert.SerializeObject(SearchFansPageHttpResult)}!");

            }

            PrintMsg(PrintType.info, $"SearchFansLv1 查询成功 pageNum:{pageNum}!");
            return Res;
        }

        /// <summary>
        /// 侦听新粉丝
        /// </summary>
        /// <param name="message">发送的消息内容</param>
        /// <param name="maxUserCount">最大用户数量</param>
        public void ListenNewFans(List<string> message, int maxUserCount, ValueChange<int> valueChange)
        {
            Thread thread = new Thread(() => SendMeesageToNewFans(message, maxUserCount, valueChange));
            thread.Start();
        }

        private void SendMeesageToNewFans(List<string> message, int maxUserCount, ValueChange<int> valueChange)
        {
            try
            {
                IsSendMessageNewFansRun = true;
                if (valueChange != null)
                {
                    valueChange(maxUserCount);
                }
                var sentFans = new List<Fans>();
                int sentFansNum = 0;
                //判断是否继续发送
                while (IsSendMessageNewFansRun)
                {
                    //1从第一页的粉丝列表开始找哪些是新粉丝,依次类推
                    if (LatestFans == null)
                    {
                        IsSendMessageNewFansRun = false;
                        return;
                    }
                    int pageNum = 0;
                    var fansList = new List<Fans>();
                    bool isExistlastFans = true;
                    bool bo = true;
                    sentFans = new List<Fans>();
                    while (isExistlastFans && IsSendMessageNewFansRun)
                    {
                        pageNum++;
                        //fansList = SearchFansLv1(pageNum);
                        fansList = SearchFnas(pageNum);
                        if (fansList.Count == 0)
                        {
                            break;
                        }
                        //找第一页的粉丝
                        while (!isExistFans(LatestFans[0].nick) && pageNum == 1)
                        {
                            LatestFans.RemoveAt(0);
                            PrintMsg(PrintType.info, "方法:SendMeesageToNewFans:移除的粉丝" + LatestFans[0].nick);
                            if (LatestFans.Count == 0)
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
                                if (fansList[i].uid == LatestFans[0].uid || !IsSendMessageNewFansRun)
                                {
                                    break;
                                }
                                //SendMessage(fansList[i], message);
                                if (SentsMessageListByNew.Where(x => x.uid == fansList[i].uid).Count() == 0)
                                {
                                    SendMoreMessageByNew(fansList[i], message);
                                    SentsMessageListByNew.Add(fansList[i]);
                                    sentFansNum++;
                                    PrintMsg(PrintType.info, "方法:SendMeesageToNewFans: 正在给新粉丝发消息: " + fansList[i].nick + " 睡眠毫秒数为:" + RandomNumber(OldFansCall_Begin, OldFansCall_End));
                                    PrintMsg(PrintType.info, $"问题追踪_LatestFans:{JsonConvert.SerializeObject(LatestFans)}_fansList:{JsonConvert.SerializeObject(fansList)}");
                                    if (valueChange != null)
                                    {
                                        valueChange(maxUserCount - sentFansNum);
                                    }
                                    if (maxUserCount == sentFansNum)
                                    {
                                        //达到打招呼上线后，退出循环
                                        IsSendMessageNewFansRun = false;
                                    }
                                    Thread.Sleep(RandomNumber(NewFansCall_Begin, NewFansCall_End));//发多人的间隔时间
                                }
                            }
                            //2如果找到上次最后一个粉丝就停止循环
                            isExistlastFans = fansList.Where(x => x.uid == LatestFans[0].uid).Count() > 0 ? false : true;
                        }
                        sentFans.AddRange(fansList);
                        Thread.Sleep(RandomNumber(NewFansRefresh_Begin, NewFansRefresh_End));
                    }
                    LatestFans = sentFans;
                }
            }
            catch (Exception ex)
            {
                IsSendMessageNewFansRun = false;
                PrintMsg(PrintType.error, "方法:SendMeesageToNewFans: 正在给新粉丝发消息异常", ex);
            }
        }

        /// <summary>
        ///  给用户发多个消息线程; 处理逻辑: 给所有粉丝发一个消息瞬时发送，发第二个消息间隔1小时左右
        /// </summary>
        public void SendMoreMessageByNew(Fans fans, List<string> message)
        {
            for (int i = 0; i < message.Count; i++)
            {
                string msg = message[i].Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                if (i == 0)
                {
                    SendMessage(fans, msg);
                }
                else
                {
                    //发送第二个，第三个消息..
                    Task.Factory.StartNew(() =>
                    {
                        //一人发个多消息的间隔时间
                        Thread.Sleep(RandomNumber(moreOffInterTime_begin, moreOffInterTime_end));
                        //Task.Delay(RandomNumber(moreOffInterTime_begin, moreOffInterTime_end));
                        SendMessage(fans, msg);
                    });
                }
            }
        }

        //判断是否粉是否取关了
        public bool isExistFans(string nick)
        {
            var res = false;
            var GetHttpItem = CreateHttpItem();
            GetHttpItem.URL = string.Format(isExistFansPage, nick, WeiBoUser.uid);
            Match mc = Match.Empty;
            HttpResult httpResult = null;
            var ResetQueryNumber = 3;


            while (!res && ResetQueryNumber > 0)
            {
                while (string.IsNullOrEmpty(mc.Value))
                {
                    httpResult = HttpHelper.GetHtml(GetHttpItem);
                    var reg = new Regex("共搜索到(\\d+)个关于");
                    mc = reg.Match(httpResult.Html);
                    Thread.Sleep(RandomNumber(3, 7));
                }
                res = int.Parse(mc.Groups[1].Value) > 0 ? true : false;
                if (!res)
                {
                    ResetQueryNumber--;
                    mc = Match.Empty;
                }
            }

            return res;
        }

        /// <summary>
        /// 发送消息给老粉丝
        /// </summary>
        /// <param name="message"></param>
        /// <param name="maxUserCount"></param>
        public void SendMeesageToOldFans(string message, int maxUserCount, ValueChange<int> valueChange)
        {
            Thread thread = new Thread(() => SendMeesageToOldFun(message, maxUserCount, valueChange));
            thread.Start();
        }

        private void SendMeesageToOldFun(string message, int maxUserCount, ValueChange<int> valueChange)
        {
            try
            {
                //判断是否继续发送
                //判断是否继续发送
                var fansList = new List<Fans>();
                int sentCount = 0;
                int pageNum = 1;
                IsSendMeesageToOldFansRun = true;
                if (valueChange != null)
                {
                    valueChange(maxUserCount);
                }
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
                        if (SentsMessageListByOld.Where(x => x.uid == item.uid).Count() == 0)
                        {
                            SendMessage(item, message);
                            //将已发送的粉丝存储答已发送列表中；避免重复发送
                            SentsMessageListByOld.Add(item);
                            sentCount++;
                            if (valueChange != null)
                            {
                                valueChange(maxUserCount - sentCount);
                            }

                            if (sentCount == 1)
                            {
                                if (OldFansSendMessageStartUserChange != null)
                                {
                                    OldFansSendMessageStartUserChange(item);
                                }
                            }
                            PrintMsg(PrintType.info, "方法:SendMeesageToOldFun: 正在给老粉丝发消息: " + item.nick + " 睡眠毫秒数为:" + RandomNumber(OldFansCall_Begin, OldFansCall_End));
                            if (sentCount >= maxUserCount)
                            {
                                IsSendMeesageToOldFansRun = false;
                                return;
                            }
                            else if (!IsSendMeesageToOldFansRun)
                            {
                                break;
                            }
                            Thread.Sleep(RandomNumber(OldFansCall_Begin, OldFansCall_End));
                        }
                    }
                    Thread.Sleep(RandomNumber(OldRefresh_Begin, OldRefresh_End));
                    PrintMsg(PrintType.info, "方法:SendMeesageToOldFun: 刷新粉丝时间，睡眠毫秒数为：" + RandomNumber(OldRefresh_Begin, OldRefresh_End));
                }
            }
            catch (Exception ex)
            {
                IsSendMeesageToOldFansRun = false;
                PrintMsg(PrintType.error, "方法:SendMeesageToOldFun异常：", ex);
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

        public void SendMessage(Fans fans, string message)
        {
            var PostHttpItem = CreateHttpItem();
            PostHttpItem.Method = "post";
            PostHttpItem.RequestRetryNumber = 1;
            PostHttpItem.URL = SendMessageApi;
            PostHttpItem.ContentType = "application/x-www-form-urlencoded";
            PostHttpItem.Header.Add("Origin", "https://api.weibo.com");
            PostHttpItem.Referer = "https://api.weibo.com/chat/ ";
            //PostHttpItem.Encoding = Encoding.UTF8;
            message = System.Web.HttpUtility.UrlEncode(message);
            PostHttpItem.Postdata = $"text={message}&uid={fans.uid}&extensions={{\"clientid\":\"ioum121csoxafeztq1x6wymifkx37z\"}}&is_encoded=0&decodetime=1&source=209678993";
            HttpResult result = HttpHelper.GetHtml(PostHttpItem);
            var code = result.Html.ToWeiBoJsonResult<object>();
            //判断是否发送失败
            if (code.error_code > 0)
            {
                PrintMsg(PrintType.info, "方法:SendMessage 出错" + code.error);
            }
            else
            {
                this.NumberRuns++;
                this.TodaySendMessageCount++;
                this.LastSendMessageTime = DateTime.Now;
                this.LastSendMessageUser = fans;
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
            var GetHttpItem = CreateHttpItem();
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
                            QrImageUrl = imageUrl;
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
                                    this.uid = SSOLoginApiRes.uid;

                                    //设置使用接口的版本
                                    GetHttpItem = CreateHttpItem();
                                    GetHttpItem.URL = string.Format(ChangeVersionApi, (int)VersionLevel.Lv1);
                                    var ChangeVersionApiRes = HttpHelper.GetHtml(GetHttpItem);

                                    //初始化用户信息
                                    InitWeiBoUser();

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

        public void SetDefalutConfig()
        {
            //赋值
            NewFansRefresh_Begin = int.Parse(CfgMgr.GetValue("NewFansRefresh_Begin"));
            NewFansRefresh_End = int.Parse(CfgMgr.GetValue("NewFansRefresh_End"));
            NewFansCall_Begin = int.Parse(CfgMgr.GetValue("NewFansCall_Begin"));
            NewFansCall_End = int.Parse(CfgMgr.GetValue("NewFansCall_End"));
            OldRefresh_Begin = int.Parse(CfgMgr.GetValue("OldRefresh_Begin"));
            OldRefresh_End = int.Parse(CfgMgr.GetValue("OldRefresh_End"));
            OldFansCall_Begin = int.Parse(CfgMgr.GetValue("OldFansCall_Begin"));
            OldFansCall_End = int.Parse(CfgMgr.GetValue("OldFansCall_End"));
            moreOffInterTime_begin = int.Parse(CfgMgr.GetValue("moreOffInterTime_begin"));
            moreOffInterTime_end = int.Parse(CfgMgr.GetValue("moreOffInterTime_end"));
            PrintMsg(PrintType.info, "SetDefalutConfig 初始化配置完成!");
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
        Lv1 = 6,
        Lv2 = 7
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

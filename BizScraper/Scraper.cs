using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.IO.Compression;
using System.Net.Configuration;
using System.Diagnostics;
using NLog;

namespace BizScraper
{
    public class Scraper
    {
        public string Html;
        public Stream Stream;


        public string Accept;
        public Encoding Encoding;
        public string Referer;
        public string ContentType;
        public CookieCollection Cookies;
        public HttpStatusCode StatusCode;
        public WebHeaderCollection ResponseHeader;

        private WebHeaderCollection Headers;
        public Boolean AllowAutoRedirect;

        public string UserAgent;
        public Boolean KeepAlive;
        public string CookieString;
        private const string GET = "GET";
        private const string POST = "POST";
        private const string PUT = "PUT";
        private const string PATCH = "PATCH";
        private const int BufferSize = 100;

        private int BadGateway502ErrCnt = 0;
        public int requestTimeOut = 5000;
        public int retryCount = 6;


        protected bool IsRetry = true;
        public bool isRetry { get { return IsRetry; } set { IsRetry = value; } }

        public Scraper()
        {
            this.Cookies = new CookieCollection();
            this.Headers = new WebHeaderCollection();
            this.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
            
            this.Accept = "image/gif, "
                        + "image/x-xbitmap, "
                        + "image/jpeg, "
                        + "image/pjpeg, "
                        + "application/vnd.ms-excel, "
                        + "application/vnd.ms-powerpoint, "
                        + "application/msword, "
                        + "application/x-ms-application, "
                        + "application/x-ms-xbap, "
                        + "application/vnd.ms-xpsdocument, "
                        + "application/xaml+xml, "
                        + "application/x-shockwave-flash, "
                        + "*/*";

            this.AllowAutoRedirect = false;
            this.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            this.KeepAlive = true;
            this.Referer = "about:blank";
            this.Encoding = Encoding.UTF8;

            this.SetHeader("Accept-Language", "ko");
            this.Html = string.Empty;
            this.Stream = null;
        }
        public void Go(string targetUrl)
        {
            this.Go(targetUrl, string.Empty);
        }
        public void Go(string targetUrl, string postData)
        {
            byte[] buffer = this.Encoding.GetBytes(postData);
            string method = "";
            if (buffer.Length == 0)
            {
                //postData가 없다면 GET방식
                method = GET;
            }
            else
            {
                //postData가 있다면 POST방식으로 접근
                method = POST;
            }

            this.Go(targetUrl, buffer, method);
        }
        public void Go(string targetUrl, byte[] buffer)
        {
            string method = "";
            if (buffer.Length == 0)
            {
                //postData가 없다면 GET방식
                method = GET;
            }
            else
            {
                //postData가 있다면 POST방식으로 접근
                method = POST;
            }

            this.Go(targetUrl, buffer, method);
        }
        public void Go(string targetUrl, string postData, string method)
        {
            byte[] buffer = this.Encoding.GetBytes(postData);
            this.Go(targetUrl, buffer, method);
        }
        public void Go(string targetUrl, byte[] buffer, string method)
        {
            this.Html = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                       SecurityProtocolType.Tls11 |
                                       SecurityProtocolType.Tls12;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetUrl);
                request.ContinueTimeout = 700;
                request.Timeout = requestTimeOut;
                request.CookieContainer = new CookieContainer();

                int i;
                string headerName;
                string headerValue;

                for (i = 0; i < this.Headers.Count; i++)
                {
                    headerName = this.Headers.GetKey(i);
                    headerValue = this.Headers.GetValues(i)[0];

                    request.Headers.Add(headerName, headerValue);
                }

                request.UserAgent = this.UserAgent;

                request.Accept = this.Accept;

                request.ContentType = this.ContentType;

                request.KeepAlive = this.KeepAlive;

                request.Referer = this.Referer;

                request.CookieContainer.Add(this.Cookies);

                request.AllowAutoRedirect = this.AllowAutoRedirect;

                request.ServicePoint.Expect100Continue = false;

                request.Method = method;

                if (request.Method == POST || request.Method == PUT || request.Method == PATCH)
                {
                    //POST방식일때만 sendStream을 작성해준다.
                    request.ContentLength = buffer.Length;

                    Stream sendStream = request.GetRequestStream();

                    sendStream.Write(buffer, 0, buffer.Length);

                    sendStream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                this.StatusCode = response.StatusCode;
                this.ResponseHeader = response.Headers;
                Stream stream = response.GetResponseStream();
                StreamReader sr;

                if (response.ContentType.ToLower() == "text/html; charset=utf-16")
                {
                    sr = new StreamReader(stream, Encoding.Unicode);
                }
                else
                {
                    if (response.ContentType.ToLower() == "text/html; charset=utf-8")
                    {
                        sr = new StreamReader(stream, Encoding.UTF8);
                    }
                    else if (response.ContentType.ToLower() == "text/xml;charset=euc-kr")
                    {
                        sr = new StreamReader(stream, Encoding.GetEncoding("euc-kr"));

                    }
                    else
                    {
                        sr = new StreamReader(stream, this.Encoding);
                    }
                }

                bool isAttachement = false;
                if (response.Headers["Content-Disposition"] != null)
                {
                    if (response.Headers["Content-Disposition"].ToString().ToLower().Contains("attachment"))
                    {
                        if (targetUrl.Contains("https://sell.storefarm.naver.com/o/sale/delivery"))
                        {
                            isAttachement = true;
                        }
                        if (targetUrl.Contains("https://sell.storefarm.naver.com/api/products/list/downloadExcel"))
                        {
                            isAttachement = true;
                        }
                        if (targetUrl.Contains("https://soffice.11st.co.kr/product/SellProductAjaxAction.tmall?method=getSellProductListJSON"))
                        {
                            isAttachement = true;
                        }
                        if (targetUrl.Contains("http://mng.halfclub.com/DefaultMng/ExcelZipPwd.aspx?target=scm"))
                        {
                            isAttachement = true;
                        }
                        if (targetUrl.Contains("https://scm.dongwonmall.com/order/excel.do"))
                        {
                            isAttachement = true;
                        }
                        if (targetUrl.Contains("https://spc.tmon.co.kr/delivery/downloadDeliveryListExcel"))
                        {
                            isAttachement = true;
                        }
                    }
                }
                if (response.ContentType == "application/vnd.ms-excel"
                    || response.ContentType == "application/octet-stream"
                    || response.ContentType == "image/jpeg"
                    || response.ContentType == "image/gif"
                    || response.ContentType == "image/png"
                    || response.ContentType == "\"application/octet-stream\""
                    || response.ContentType == "application/vnd.ms-excel; charset=UTF-8"
                    || response.ContentType == "application/vnd.ms-excel;charset=UTF-8"
                    || response.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    || response.ContentType == "application/vnd.ms-excel;charset=ms949"
                    || response.ContentType == "application/vnd.ms-excel; charset=EUC-KR"
                    || response.ContentType == "image/gif;charset=UTF-8"
                    || response.ContentType == "application/x-msexcel;charset=UTF8"
                    || response.ContentType == "application/vnd.ms-excel; charset=euc-kr"
                    || response.ContentType == "application/ms-excel; charset=utf-8"
                    || response.ContentType == "text/html;charset=ISO-8859-1"
                    || response.ContentType == "application/octet-stream;charset=EUC-KR"
                    || response.ContentType == "application/x-msexcel"
                    || isAttachement)
                {
                    if (response.ContentEncoding == "gzip")
                    {
                        GZipStream zipStream = new GZipStream(sr.BaseStream, CompressionMode.Decompress);
                        this.Html = response.ContentType;
                        this.Stream = zipStream;
                    }
                    else
                    {
                        this.Html = response.ContentType;
                        this.Stream = sr.BaseStream;
                    }
                }
                else
                {

                    if (response.ContentEncoding == "gzip")
                    {
                        GZipStream zipStream = new GZipStream(sr.BaseStream, CompressionMode.Decompress);

                        Encoding encoding;

                        if (response.ContentType == "text/html; charset=utf-16")
                        {
                            encoding = Encoding.Unicode;
                        }
                        else
                        {
                            if (response.ContentType.ToLower() == "text/html; charset=utf-8")
                            {
                                encoding = Encoding.UTF8;
                            }
                            else
                            {
                                encoding = this.Encoding;
                            }
                        }

                        this.Html = this.ReadAllBytesFromStream(zipStream, encoding);
                    }
                    else
                    {
                        this.Html = sr.ReadToEnd();
                    }
                }

                this.Referer = targetUrl;

                if (targetUrl.Contains("sell.storefarm.naver.com") && this.Html.Contains("302 Found") && this.Html.Contains("nginx"))
                {
                    throw new Exception("스토어팜에 접속 시, 서비스에 접속 할 수 없습니다. 동시 접속자 수가 많거나 네트워크 불안정으로 일시적 서비스 접속이 불가합니다. 10분후 재시도 해 주십시오.");
                }

                for (i = 0; i < response.Cookies.Count; i++)
                {
                    this.Cookies.Add(response.Cookies[i]);
                }

                this.CookieString = response.Headers["Set-Cookie"];

                if (this.CookieString != null)
                {

                    string[] split = null;

                    string delimStr = ";,";

                    char[] delimiter = delimStr.ToCharArray();

                    split = this.CookieString.Split(delimiter);

                    int equalStrPos;

                    string cookieNmVal;
                    string cookieNm;
                    string cookieValue;

                    for (i = 0; i < split.Length; i++)
                    {
                        if (split[i].Trim().IndexOf("=") > 0
                        && split[i].Trim().IndexOf("path") < 0
                        && split[i].Trim().IndexOf("domain") < 0
                        && split[i].Trim().IndexOf("expires") < 0)
                        {
                            cookieNmVal = split[i].Trim();
                            equalStrPos = cookieNmVal.IndexOf("=", 1);

                            cookieNm = cookieNmVal.Substring(0, equalStrPos);
                            cookieValue = cookieNmVal.Substring
                                (equalStrPos + 1,
                                 cookieNmVal.Length - equalStrPos - 1);
                        }
                    }
                }
                this.BadGateway502ErrCnt = 0;
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("원격 서버에서 (502) 잘못된 게이트웨이 오류를 반환했습니다.") ||
                    ex.Message.Contains("요청이 중단되었습니다. SSL/TLS 보안 채널을 만들 수 없습니다.") ||
                    ex.Message.Contains("URI 형식을 확인할 수 없습니다.") ||
                    ex.Message.Contains("URI가 비어 있습니다.") ||
                    ex.Message.Contains("기본 연결이 닫혔습니다. 유지되어야 할 연결을 서버에서 끊었습니다."))
                {
                    Logger logger = LogManager.GetCurrentClassLogger();

                    this.BadGateway502ErrCnt++;
                    if (BadGateway502ErrCnt < retryCount && IsRetry) //다시시도는 5회만
                    {
                        if (targetUrl == "https://soffice.11st.co.kr/escrow/OrderingLogisticsAction.tmall")
                        {
                            logger.Error("재시도 횟수(scraper.cs)" + "\n" + " target url : " + targetUrl + "\n" + ex.Message + "\n" + BadGateway502ErrCnt);
                        }
                        //일시적오류발생시 잠시쉬고 다시시도,
                        System.Threading.Thread.Sleep(1000);
                        this.Go(targetUrl, buffer, method);

                        return;
                    }
                    logger.Error("재시도 횟수(scraper.cs)" + "\n" + " target url : " + targetUrl + "\n" + ex.Message + "\n" + BadGateway502ErrCnt);
                }
                else if(ex.Message.Contains("작업 시간이 초과되었습니다."))
                {
                    Logger logger = LogManager.GetCurrentClassLogger();

                    this.BadGateway502ErrCnt++;
                    if (BadGateway502ErrCnt < retryCount && IsRetry) //다시시도는 5회만
                    {
                        if (targetUrl == "https://soffice.11st.co.kr/escrow/OrderingLogisticsAction.tmall")
                        {
                            logger.Error("재시도 횟수(scraper.cs)" + "\n" + " target url : " + targetUrl + "\n" + ex.Message + "\n" + BadGateway502ErrCnt);
                        }

                        System.Threading.Thread.Sleep(1000);
                        this.requestTimeOut = this.requestTimeOut + 100000;
                        this.Go(targetUrl, buffer, method);
                        return;
                    }
                    //일시적오류발생시 잠시쉬고 다시시도,
                    logger.Error("재시도 횟수(scraper.cs)" + "\n" + " target url : " + targetUrl + "\n" + ex.Message + "\n" + BadGateway502ErrCnt);
                }
                this.Html = string.Empty;
                this.StatusCode = HttpStatusCode.InternalServerError;
                this.Stream = null;

                if (!targetUrl.Contains("cafe24api"))
                {
                    this.ResponseHeader = null;
                }

                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    StreamReader sr = new StreamReader(stream, this.Encoding);
                    try { 
                    if (ex.Response.Headers["Content-Encoding"].ToString() == "gzip")
                    {
                        GZipStream zipStream = new GZipStream(sr.BaseStream, CompressionMode.Decompress);
                        this.Html = ex.Response.ContentType;
                        this.Stream = zipStream;
                    }
                    else
                    {
                        this.Html = ex.Response.ContentType;
                        this.Stream = sr.BaseStream;
                    }
                    }
                    catch
                    {
                        this.Html = ex.Response.ContentType;
                        this.Stream = sr.BaseStream;
                    }
                    StreamReader sr2 = new StreamReader(this.Stream, this.Encoding);
                    this.Html = sr2.ReadToEnd();
                    StatusCode = ((HttpWebResponse)ex.Response).StatusCode;
                }
                if (ex.Message.Contains("원격 서버에서 (500) 내부 서버 오류 오류를 반환했습니다."))
                {
                    if (Html.Contains("위메프 접속이 원활하지 않습니다."))
                    {
                        throw new Exception("현재 위메프 서버에 접속이 지연되고 있습니다. 위메프측 서비스에 일시적 장애입니다. 재시도 바랍니다.");
                    }
                }
                if (ex.Message.Contains("원격 서버에서 (500) 내부 서버 오류 오류를 반환했습니다."))
                {
                    if (Html.Contains("대표번호를 인증하여 주시기 바랍니다.") && Html.Contains("ESM+를 통한 판매활동이 제한된 상태입니다."))
                    {
                        var 메시지 = string.Empty;
                        메시지 = "ESM PLUS 대표번호 인증 기간 만료" 
                              + "<a href='https://faq.bizesm.com/wordpress/?p=3396' target='_blank'>[해결방법 클릭]</a>"
                              + "<a href='https://www.esmplus.com/Member/SignIn/LogOn' target='_blank'>[ESMPLUS 로그인 페이지]</a>";

                        throw new Exception(메시지);                        
                    }
                }
                if (ex.Message.Contains("원격 서버에서 (502) 잘못된 게이트웨이 오류를 반환했습니다."))
                {
                    if (Html.Contains("스마트 스토어센터 서비스를 점검중입니다."))
                    {
                        throw new Exception("스마트 스토어센터 서비스를 점검중입니다. 스토어팜센터 서비스에 일시적 장애입니다. 재시도 바랍니다.");
                    }
                }

                if (ex.Message.Contains("원격 서버에서 (500) 내부 서버 오류 오류를 반환했습니다."))
                {
                    if (targetUrl.Contains("GetSTKLIST"))
                    {
                        throw new Exception(Html);
                    }
                }

                if (ex.Message.Contains("원격 서버에서 (500) 내부 서버 오류 오류를 반환했습니다."))
                {
                    if (ex.Response.ResponseUri.ToString().Contains("http://eapi.ssgadm.com"))
                    {
                        return;
                    }
                    if (ex.Response.ResponseUri.ToString().Contains("gsshop"))
                    {
                        
                        return;
                    }
                    throw new Exception("연동 진행 쇼핑몰내에 장애가 발생하였습니다.(500)");
                }
                if (ex.Response.ResponseUri.ToString().Contains("https://w-api.wemakeprice.com/"))
                {
                    return;
                }
                if (ex.Response.ResponseUri.ToString().Contains("https://api-gateway.coupang.com"))
                {
                    return;
                }
                if (ex.Response.ResponseUri.ToString().Contains("https://kapi.kakao.com"))
                {
                    return;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetCookieStr()
        {
            string cookieStr = "";

            for (int i = 0; i < this.Cookies.Count; i++)
            {
                cookieStr =
                    cookieStr + this.Cookies[i].Name + "=" + this.Cookies[i].Value + ";";
            }

            return cookieStr;
        }
        public string GetViewstate()
        {
            string benchMarkStr = "id=\"__VIEWSTATE\"";
            string headStr = "value=\"";
            string tailStr = "\" />";
            return GetMidStr(this.Html, benchMarkStr, headStr, tailStr);
        }
        public int SetHeader(string name, string value)
        {
            try
            {
                this.Headers.Remove(name);
                if (value != "")
                {
                    this.Headers.Add(name, value);
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }
        public int RemoveCookie(string name)
        {
            try
            {
                CookieCollection cookieCollection = new CookieCollection();
                for (int i = 0; i < this.Cookies.Count; i++)
                {
                    if (Cookies[i].Name != name)
                    {
                        cookieCollection.Add(Cookies[i]);
                    }
                }

                this.Cookies = cookieCollection;
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        private string ReadAllBytesFromStream(Stream stream, Encoding encoding)
        {
            byte[] buffer = new byte[BufferSize];

            MemoryStream ms = new MemoryStream();

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, BufferSize);

                if (bytesRead == 0)
                {
                    break;
                }

                ms.Write(buffer, 0, bytesRead);
            }

            ms.Position = 0;

            StreamReader sr = new StreamReader(ms, encoding);

            string result = sr.ReadToEnd();

            return result;
        }
        public string GetMidStr(string allStr, string benchmarkStr, string headStr, string tailStr)
        {
            try
            {
                int benchmarkStart = allStr.IndexOf(benchmarkStr, 1); //찾을문자열주소

                if (benchmarkStart < 0) { benchmarkStart = 0; }

                int headStart = allStr.IndexOf(headStr, benchmarkStart);//시작할문자열

                if (headStart < 0) { return ""; }

                int tailStart = allStr.IndexOf(tailStr, headStart + 1); //끝문자열

                int midStrStart = headStart + headStr.Length;
                int midStrEnd = tailStart;
                int midStrLength = midStrEnd - midStrStart;

                string midStr = allStr.Substring(midStrStart, midStrLength);

                return midStr;
            }
            catch
            {
                return string.Empty;
            }
        }
        private static int GetPosFromEnd(string allStr, string targetStr, int start, int end)
        {
            int startIndex;
            int len;
            string curChar;

            for (int i = end; i >= start; i--)
            {
                startIndex = i;

                if ((startIndex + targetStr.Length) > (end + 1))
                {
                    len = (end + 1) - startIndex;
                }
                else
                {
                    len = targetStr.Length;
                }

                curChar = allStr.Substring(startIndex, len);

                if (curChar == targetStr)
                {
                    return i;
                }
            }

            return -1;
        }
        private static int GetPosFromStart(string allStr, string targetStr, int start, int end)
        {
            int startIndex;
            string curChar;

            for (int i = start; i <= end; i++)
            {
                startIndex = i;

                curChar = allStr.Substring(startIndex, targetStr.Length);

                if (curChar == targetStr)
                {
                    return i;
                }
            }
            return -1;
        }
        private static string UrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }
        private static string UrlDecode(string str)
        {
            return HttpUtility.UrlDecode(str);
        }
    }
}
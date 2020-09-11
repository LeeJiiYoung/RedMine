using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BizScraper;
using System.Web.Http;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Cookie = System.Net.Cookie;

namespace RedMine.Hubs
{
    public class chatHub : Hub
    {
        Scraper scraper = new Scraper();
        string csrfToken = "";

        public void hhhh()
        {
            Clients.All.aaaa();
        }

        public void getVersionCombo(object data)
        {
            JObject result = new JObject();
            JArray jarr = new JArray();
            JObject request = new JObject();
            request = JsonConvert.DeserializeObject<JObject>(data.ToString());
            string id = request["id"].ToString();
            string pw = request["pw"].ToString();
            Login(id, pw);
            scraper.Go("http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030/roadmap");
            HtmlDocument hDoc = new HtmlDocument();
            hDoc.LoadHtml(scraper.Html);
            HtmlNodeCollection hnc = hDoc.DocumentNode.SelectNodes("//h3[@class='version']");
            foreach (HtmlNode hn in hnc)
            {
                result = new JObject();
                if (Regex.Match(hn.InnerHtml, "<a.*?>(?<title>.*?)</a>").Groups["title"].Value.Contains("정기업데이트"))
                {
                    result["version"] = Regex.Match(hn.InnerHtml, "href=\"/redmine/versions/(?<version>.*?)\"").Groups["version"].Value;
                    result["title"] = Regex.Match(hn.InnerHtml, "<a.*?>(?<title>.*?)</a>").Groups["title"].Value;
                    jarr.Add(result);
                }
            }
            Clients.All.drawCombo(jarr);
        }

        [HttpPost]
        public string Login(string id, string pw)
        {
            try
            {
                string url = "";
                string message = "";
                CookieContainer container = new CookieContainer();
                url = "http://redmine.ebizway.co.kr:8081/redmine/login?back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2F";
                scraper.Go(url);
                Cookie cookie = scraper.Cookies["_redmine_session"];
                scraper.Cookies.Add(cookie);
                csrfToken = Regex.Match(scraper.Html, "<meta name=\\\"csrf-token\\\" content=\\\"(?<csrfToken>.*)\\\"").Groups["csrfToken"].Value;
                url = "http://redmine.ebizway.co.kr:8081/redmine/login";
                string postData = "utf8=%E2%9C%93"
                    + "&authenticity_token=" + HttpUtility.UrlEncode(csrfToken)
                    + "&back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2F"
                    + "&username=" + id
                    + "&password=" + pw
                    + "&login=%EB%A1%9C%EA%B7%B8%EC%9D%B8+%C2%BB";
                scraper.Go(url, postData);


                if (scraper.Html.Contains("주-이비즈웨이-레드마인-관리시스템입니다"))
                {
                    message = "로그인 성공";
                }
                else
                {
                    message = "로그인 실패";
                }
                return message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
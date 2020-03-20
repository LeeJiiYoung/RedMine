using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using BizScraper;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
namespace RedMine.Controllers
{
    public class RedMineController : ApiController
    {
        Scraper scraper = new Scraper();

        // GET api/<controller>
        [ActionName("d")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        [HttpPost]
        public string Login(string id, string pw)
        {
            try
            {
                string url = "";
                CookieContainer container = new CookieContainer();
                url = "http://redmine.ebizway.co.kr:8081/redmine/login?back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2F";
                scraper.Go(url);
                Cookie cookie = scraper.Cookies["_redmine_session"];
                scraper.Cookies.Add(cookie);
                string csrfToken = Regex.Match(scraper.Html, "<meta name=\\\"csrf-token\\\" content=\\\"(?<csrfToken>.*)\\\"").Groups["csrfToken"].Value;
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
                    return "로그인 성공";
                }
                else
                {
                    return "로그인 실패";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        [HttpPost]
        public string updateDownTxt()
        {
            try
            {
                HttpContext req = HttpContext.Current;
                string id = req.Request.Form["id"];
                string pw = req.Request.Form["pw"];
                string date = req.Request.Form["datetimepicker1"];
                JObject 지원 = new JObject();
                JObject 새기능 = new JObject();
                JObject 결함 = new JObject();
				JObject 기타 = new JObject();
                JArray 지원어레이 = new JArray();
                JArray 새기능어레이 = new JArray();
                JArray 결함어레이 = new JArray();
				JArray 기타어레이 = new JArray();
                Login(id, pw);
                scraper.Go("http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030/roadmap");
                string version = Regex.Match(scraper.Html, "비젬_" + date + "_정기업데이트\" href=\"/redmine/versions/(?<version>.*)\"").Groups["version"].Value;

                string url = "";
				bool isLastPage = false;
				int page = 1;

				//url = "http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030/issues?utf8=%E2%9C%93"
				//					   + "&set_filter=1"
				//					   + "&f%5B%5D=status_id"
				//					   + "&op%5Bstatus_id%5D=c"
				//					   + "&f%5B%5D=fixed_version_id"
				//					   + "&op%5Bfixed_version_id%5D=%3D"
				//					   + "&v%5Bfixed_version_id%5D%5B%5D=" + version
				//					   + "&f%5B%5D="
				//					   + "&c%5B%5D=project"
				//					   + "&c%5B%5D=tracker"
				//					   + "&c%5B%5D=status"
				//					   + "&c%5B%5D=priority"
				//					   + "&c%5B%5D=subject"
				//					   + "&c%5B%5D=author"
				//					   + "&c%5B%5D=assigned_to"
				//					   + "&c%5B%5D=start_date"
				//					   + "&c%5B%5D=due_date"
				//					   + "&c%5B%5D=updated_on"
				//					   + "&c%5B%5D=done_ratio"
				//					   + "&group_by="
				//					   + "&t%5B%5D=estimated_hours"
				//					   + "&t%5B%5D=spent_hours"
				//					   + "&t%5B%5D=";

				version = "552";
				do
				{
					url = "http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030/issues" +
						"?c%5B%5D=project" +
						"&c%5B%5D=tracker" +
						"&c%5B%5D=status" +
						"&c%5B%5D=priority" +
						"&c%5B%5D=subject" +
						"&c%5B%5D=author" +
						"&c%5B%5D=assigned_to" +
						"&c%5B%5D=start_date" +
						"&c%5B%5D=due_date" +
						"&c%5B%5D=updated_on" +
						"&c%5B%5D=done_ratio" +
						"&f%5B%5D=status_id" +
						"&f%5B%5D=fixed_version_id" +
						"&f%5B%5D=" +
						"&group_by=" +
						"&op%5Bfixed_version_id%5D=%3D" +
						"&op%5Bstatus_id%5D=c" +
						"&page=" + page +
						"&set_filter=1" +
						"&t%5B%5D=estimated_hours" +
						"&t%5B%5D=spent_hours" +
						"&t%5B%5D=" +
						"&utf8=%E2%9C%93" +
						"&v%5Bfixed_version_id%5D%5B%5D=" + version;
					scraper.Encoding = Encoding.GetEncoding("UTF-8");
					scraper.Go(url);
					HtmlDocument hDoc = new HtmlDocument();
					hDoc.LoadHtml(scraper.Html);
					HtmlNodeCollection hnc = hDoc.DocumentNode.SelectNodes("//table[@class='list issues sort-by-id sort-desc']/tbody/tr");
					if (hnc == null)
					{
						isLastPage = true;
						break;
					}
					foreach (HtmlNode hn in hnc)
					{
						string tracker = hn.SelectSingleNode("td[@class='tracker']").InnerHtml;
						if (tracker == "새기능")
						{
							새기능 = new JObject();
							새기능["id"] = hn.Id.Split('-')[1];
							새기능["tracker"] = hn.SelectSingleNode("td[@class='tracker']").InnerHtml;
							새기능["subject"] = hn.SelectSingleNode("td[@class='subject']/a").InnerHtml;
							새기능어레이.Add(새기능);
						}
						else if (tracker == "지원")
						{
							지원 = new JObject();
							지원["id"] = hn.Id.Split('-')[1];
							지원["tracker"] = hn.SelectSingleNode("td[@class='tracker']").InnerHtml;
							지원["subject"] = hn.SelectSingleNode("td[@class='subject']/a").InnerHtml;
							지원어레이.Add(지원);
						}
						else if (tracker == "결함")
						{
							결함 = new JObject();
							결함["id"] = hn.Id.Split('-')[1];
							결함["tracker"] = hn.SelectSingleNode("td[@class='tracker']").InnerHtml;
							결함["subject"] = hn.SelectSingleNode("td[@class='subject']/a").InnerHtml;
							결함어레이.Add(결함);
						}
						else
						{
							기타 = new JObject();
							기타["id"] = hn.Id.Split('-')[1];
							기타["tracker"] = hn.SelectSingleNode("td[@class='tracker']").InnerHtml;
							기타["subject"] = hn.SelectSingleNode("td[@class='subject']/a").InnerHtml;
							기타어레이.Add(기타);
						}
					}
					page++;
				}
				while (isLastPage == false);
				
				
				
				url = "";
                
                string textValue = "";
                string gbn = "";
                for (int 새기능개수 = 0; 새기능개수 < 새기능어레이.Count; 새기능개수++)
                {
                    textValue += gbn + 새기능어레이[새기능개수]["tracker"] + " #" + 새기능어레이[새기능개수]["id"] + ": " + HttpUtility.HtmlDecode(새기능어레이[새기능개수]["subject"].ToString());
                    gbn = "\r\n\r\n";
                }
                for (int 지원개수 = 0; 지원개수 < 지원어레이.Count; 지원개수++)
                {
                    textValue += gbn + 지원어레이[지원개수]["tracker"] + " #" + 지원어레이[지원개수]["id"] + ": " + HttpUtility.HtmlDecode(지원어레이[지원개수]["subject"].ToString());
                    gbn = "\r\n\r\n";
                }
                for (int 결함개수 = 0; 결함개수 < 결함어레이.Count; 결함개수++)
                {
                    textValue += gbn + 결함어레이[결함개수]["tracker"] + " #" + 결함어레이[결함개수]["id"] + ": " + HttpUtility.HtmlDecode(결함어레이[결함개수]["subject"].ToString());
                    gbn = "\r\n\r\n";
                }
				for (int 기타개수 = 0; 기타개수 < 기타어레이.Count; 기타개수++)
				{
					textValue += gbn + 기타어레이[기타개수]["tracker"] + " #" + 기타어레이[기타개수]["id"] + ": " + HttpUtility.HtmlDecode(기타어레이[기타개수]["subject"].ToString());
					gbn = "\r\n\r\n";
				}
				string path = @"D:\text.txt";

                System.IO.File.WriteAllText(path, textValue, Encoding.Default);

                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

				//url = "http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030/issues?" +
				//    "utf8=%E2%9C%93" +
	   //            "&set_filter=1" +
				//   "&f%5B%5D=status_id" +
				//   "&op%5Bstatus_id%5D=o" +
				//   "&f%5B%5D=fixed_version_id" +
				//   "&op%5Bfixed_version_id%5D=%3D" +
				//   "&v%5Bfixed_version_id%5D%5B%5D=" + version +
				//   "&f%5B%5D=" +
				//   "&c%5B%5D=project" +
				//   "&c%5B%5D=tracker" +
				//   "&c%5B%5D=status" +
				//   "&c%5B%5D=priority" +
				//   "&c%5B%5D=subject" +
				//   "&c%5B%5D=author" +
				//   "&c%5B%5D=assigned_to" +
				//   "&c%5B%5D=start_date" +
				//   "&c%5B%5D=due_date" +
				//   "&c%5B%5D=updated_on" +
				//   "&c%5B%5D=done_ratio" +
				//   "&group_by=" +
				//   "&t%5B%5D=estimated_hours" +
				//   "&t%5B%5D=spent_hours" +
				//   "&t%5B%5D=";

				//scraper.Go(url);
				//HtmlDocument hDoc2 = new HtmlDocument();
				//hDoc2.LoadHtml(scraper.Html);
				//HtmlNodeCollection hnc2 = hDoc2.DocumentNode.SelectNodes("//table[@class='list issues sort-by-id sort-desc']/tbody/tr");

				//List<string> issue = new List<string>();
				//foreach (HtmlNode hn in hnc2)
				//{
				//	//다음 목표 버전으로 넘길 일감 번호 담기
				//	issue.Add(hn.Id.Split('-')[1]);
				//}
				//string csrfToken = Regex.Match(scraper.Html, "<meta name=\\\"csrf-token\\\" content=\\\"(?<csrfToken>.*)\\\"").Groups["csrfToken"].Value;

				//scraper.Go("http://redmine.ebizway.co.kr:8081/redmine/issues/context_menu?utf8=%E2%9C%93&authenticity_token=" + csrfToken + "&back_url=%2Fredmine%2Fprojects%2Fbf-erp-20131030%2Fissues%3Fc%255B%255D%3Dproject%26c%255B%255D%3Dtracker%26c%255B%255D%3Dstatus%26c%255B%255D%3Dpriority%26c%255B%255D%3Dsubject%26c%255B%255D%3Dauthor%26c%255B%255D%3Dassigned_to%26c%255B%255D%3Dstart_date%26c%255B%255D%3Ddue_date%26c%255B%255D%3Dupdated_on%26c%255B%255D%3Ddone_ratio%26f%255B%255D%3Dstatus_id%26f%255B%255D%3Dfixed_version_id%26f%255B%255D%3D%26group_by%3D%26op%255Bfixed_version_id%255D%3D%253D%26op%255Bstatus_id%255D%3Do%26set_filter%3D1%26t%255B%255D%3Destimated_hours%26t%255B%255D%3Dspent_hours%26t%255B%255D%3D%26utf8%3D%25E2%259C%2593%26v%255Bfixed_version_id%255D%255B%255D%3D548&ids%5B%5D=35114");


				//url = "http://redmine.ebizway.co.kr:8081/redmine/issues/bulk_update?back_url=%2Fredmine%2Fprojects%2Fbf-erp-20131030%2Fissues%3Fc%255B%255D%3Dproject%26c%255B%255D%3Dtracker%26c%255B%255D%3Dstatus%26c%255B%255D%3Dpriority%26c%255B%255D%3Dsubject%26c%255B%255D%3Dauthor%26c%255B%255D%3Dassigned_to%26c%255B%255D%3Dstart_date%26c%255B%255D%3Ddue_date%26c%255B%255D%3Dupdated_on%26c%255B%255D%3Ddone_ratio%26f%255B%255D%3Dstatus_id%26f%255B%255D%3Dfixed_version_id%26f%255B%255D%3D%26group_by%3D%26op%255Bfixed_version_id%255D%3D%253D%26op%255Bstatus_id%255D%3Do%26set_filter%3D1%26t%255B%255D%3Destimated_hours%26t%255B%255D%3Dspent_hours%26t%255B%255D%3D%26utf8%3D%25E2%259C%2593%26v%255Bfixed_version_id%255D%255B%255D%3D548"
				//	+"&ids%5B%5D=" + "35114"
				//	+ "&issue%5Bfixed_version_id%5D=549";

				//string postData = "";
				//postData = "_method=post" +
				//		  "&authenticity_token=" + csrfToken;
				//scraper.Go(url, postData);

					return "다운로드완료";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

		[HttpPost]
		public string CloseUpdate()
		{
			try
			{
				//Login();
				HttpContext req = HttpContext.Current;
				string id = req.Request.Form["id"];
				string pw = req.Request.Form["pw"];
				string date = req.Request.Form["datetimepicker1"];
				
				return "";
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

		}
	}
}
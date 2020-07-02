using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using BizScraper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace RedMine.Controllers
{
    public class RedMineController : ApiController
    {
        Scraper scraper = new Scraper();
		string csrfToken = "";

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
        public HttpResponseMessage updateDownTxt()
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
                version = req.Request.Form["version"];
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

				//version = "552"; 테스트용
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

                //* A,B,C 순서대로 정렬하는 부분 *//
                새기능어레이 = new JArray(새기능어레이.OrderBy(obj => (string)obj["subject"]));
                지원어레이 = new JArray(지원어레이.OrderBy(obj => (string)obj["subject"]));
                결함어레이 = new JArray(결함어레이.OrderBy(obj => (string)obj["subject"]));
                기타어레이 = new JArray(기타어레이.OrderBy(obj => (string)obj["subject"]));
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
				string path = @"C:\Text\text.txt";

                System.IO.File.WriteAllText(path, textValue, Encoding.Default);

                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
				return GetFile();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

		[HttpPost]
		public string CloseUpdate(object data)
		{
			try
			{
				JObject request = new JObject();
				request = JsonConvert.DeserializeObject<JObject>(data.ToString());
				string id = request["id"].ToString();
                string pw = request["pw"].ToString();
                string version = request["version"].ToString();
                string version2 = request["version2"].ToString();
                Login(id, pw);

                string url = "http://redmine.ebizway.co.kr:8081/redmine/versions/" + version + "/edit";
                scraper.Go(url);
                csrfToken = Regex.Match(scraper.Html, "<meta name=\\\"csrf-token\\\" content=\\\"(?<csrfToken>.*)\\\"").Groups["csrfToken"].Value;
                HtmlDocument hDoc = new HtmlDocument();
                hDoc.LoadHtml(scraper.Html);
				string name = hDoc.DocumentNode.SelectSingleNode("//input[@name='version[name]']").Attributes["value"].Value;
				string description = hDoc.DocumentNode.SelectSingleNode("//input[@name='version[description]']").Attributes["value"].Value;
				string effective_date = hDoc.DocumentNode.SelectSingleNode("//input[@name='version[effective_date]']").Attributes["value"].Value;

				url = "http://redmine.ebizway.co.kr:8081/redmine/versions/" + version;
				string postData = "utf8=%E2%9C%93" +
					"&_method=patch" +
					"&authenticity_token=" + Uri.EscapeDataString(csrfToken) +
					"&back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2Fversions%2F" + version +
					"&version%5Bname%5D=" + Uri.EscapeDataString(name) +
					"&version%5Bdescription%5D=" + Uri.EscapeDataString(description) +
					"&version%5Bstatus%5D=closed" +
					"&version%5Bwiki_page_title%5D=" +
					"&version%5Beffective_date%5D=" + effective_date +
					"&version%5Bsharing%5D=none" +
					"&commit=%EC%A0%80%EC%9E%A5";

				scraper.Go(url, postData);
				if(scraper.StatusCode == HttpStatusCode.Redirect)
                {
					return "완료";
				}
                else
                {
					return "실패";
                }
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

		}
		public HttpResponseMessage GetFile()
		{
			string fileName = "text.txt";
			string localFilePath = @"C:\Text\text.txt";


			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
			response.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
			response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
			response.Content.Headers.ContentDisposition.FileName = fileName;
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/txt");

			return response;
		}

		[HttpPost]
		public JArray getVersionCombo(object data)
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
			foreach(HtmlNode hn in hnc)
            {
                result = new JObject();
                if (Regex.Match(hn.InnerHtml, "<a.*?>(?<title>.*?)</a>").Groups["title"].Value.Contains("정기업데이트"))
                {
                    result["version"] = Regex.Match(hn.InnerHtml, "href=\"/redmine/versions/(?<version>.*?)\"").Groups["version"].Value;
                    result["title"] = Regex.Match(hn.InnerHtml, "<a.*?>(?<title>.*?)</a>").Groups["title"].Value;
                    jarr.Add(result);
                }
            }
			return jarr;
		}

		[HttpPost]
		public void versionUp(object data)
        {
            JObject request = new JObject();
            request = JsonConvert.DeserializeObject<JObject>(data.ToString());
            string id = request["id"].ToString();
            string pw = request["pw"].ToString();
            string version = request["version"].ToString();
            string version2 = request["version2"].ToString();
            Login(id, pw);


            bool isLastPage = false;
            do
            {
                string url = "http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030/issues?" +
					"utf8=%E2%9C%93" +
					"&set_filter=1" +
					"&f%5B%5D=status_id" +
					"&op%5Bstatus_id%5D=o" +
					"&f%5B%5D=fixed_version_id" +
					"&op%5Bfixed_version_id%5D=%3D" +
					"&v%5Bfixed_version_id%5D%5B%5D=" + version + 
					"&f%5B%5D=" +
					"&c%5B%5D=project" +
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
					"&group_by=" +
					"&t%5B%5D=estimated_hours" +
					"&t%5B%5D=spent_hours" +
					"&t%5B%5D=";
                scraper.Go(url);

                csrfToken = Regex.Match(scraper.Html, "<meta name=\\\"csrf-token\\\" content=\\\"(?<csrfToken>.*)\\\"").Groups["csrfToken"].Value;

                List<string> ids = new List<string>();
                HtmlDocument hDoc = new HtmlDocument();
                hDoc.LoadHtml(scraper.Html);
                HtmlNodeCollection hnc = hDoc.DocumentNode.SelectNodes("//table[@class='list issues sort-by-id sort-desc']/tbody/tr");
                if (hnc == null || hnc.Count == 0)
                {
                    isLastPage = true;
					break;
                }

                foreach (HtmlNode hn in hnc)
                {
                    ids.Add(hn.Attributes["id"].Value.ToString().Split('-')[1]);
                }
                url = "http://redmine.ebizway.co.kr:8081/redmine/issues/bulk_update?back_url=%2Fredmine%2Fprojects%2Fbf-erp-20131030%2Fissues%3Fc%255B%255D%3Dproject%26c%255B%255D%3Dtracker%26c%255B%255D%3Dstatus%26c%255B%255D%3Dpriority%26c%255B%255D%3Dsubject%26c%255B%255D%3Dauthor%26c%255B%255D%3Dassigned_to%26c%255B%255D%3Dstart_date%26c%255B%255D%3Ddue_date%26c%255B%255D%3Dupdated_on%26c%255B%255D%3Ddone_ratio%26f%255B%255D%3Dstatus_id%26f%255B%255D%3Dfixed_version_id%26f%255B%255D%3D%26group_by%3D%26op%255Bfixed_version_id%255D%3D%253D%26op%255Bstatus_id%255D%3Do%26set_filter%3D1%26t%255B%255D%3Destimated_hours%26t%255B%255D%3Dspent_hours%26t%255B%255D%3D%26utf8%3D%25E2%259C%2593%26v%255Bfixed_version_id%255D%255B%255D%3D" + version;

                string ids_url = "";
                string post = "";
                for (int i = 0; i < ids.Count; i++)
                {
                    ids_url += "&ids%5B%5D=" + ids[i];
                }
                ids_url += "&issue%5Bfixed_version_id%5D=" + version2;

                
                string p = url + ids_url;
				post += "_method=post"
					+ "&authenticity_token=" + Uri.EscapeDataString(csrfToken);
                try
                {
					scraper.requestTimeOut = 500000;
                    scraper.Go(p, post);
					System.Threading.Thread.Sleep(20000);
					if(scraper.StatusCode == HttpStatusCode.Found)
                    {

                    }
                }
                catch(Exception ex) { throw new Exception(ex.Message); }
            } while (isLastPage == false);
        }

    }
}
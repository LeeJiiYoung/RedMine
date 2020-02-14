using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using BizScraper;
namespace RedMine.Controllers
{
    public class RedMineController : ApiController
    {
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

        [HttpGet]
        public string Login()
        {
            try
            {
                Scraper scraper = new Scraper();
                string url = "";
                url = "http://redmine.ebizway.co.kr:8081/redmine/login?back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2F";
                scraper.Go(url);
                Cookie cookie = scraper.Cookies["_redmine_session"];
                string csrfToken = Regex.Match(scraper.Html, "<meta name=\\\"csrf-token\\\" content=\\\"(?<csrfToken>.*)\\\"").Groups["csrfToken"].Value;
                url = "http://redmine.ebizway.co.kr:8081/redmine/login";
                string postData = "utf8=%E2%9C%93&authenticity_token=" + HttpUtility.UrlEncode(csrfToken) + "&back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2F&username=jylee&password=8282&login=%EB%A1%9C%EA%B7%B8%EC%9D%B8+%C2%BB";

                scraper.Go(url, postData);

                scraper.Go("http://redmine.ebizway.co.kr:8081/redmine/");
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
        [HttpGet]
        public void updateDownTxt()
        {
            Login();
            string url = "http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030/roadmap";
            Scraper scraper = new Scraper();
            scraper.Referer= "http://redmine.ebizway.co.kr:8081/redmine/projects/bf-erp-20131030?jump=welcome";
            
            scraper.Go(url);

            return;
        }
    }
}
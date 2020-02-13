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
    public class JobController : ApiController
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
        public void Login()
        {
            Scraper scraper = new Scraper();
            string url = "";
            url = "http://redmine.ebizway.co.kr:8081/redmine/login?back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2F";
           scraper.Go(url);
            Cookie cookie = scraper.Cookies["_redmine_session"];
            string csrfToken = Regex.Match(scraper.Html, "<meta name=\\\"csrf-token\\\" content=\\\"(?<csrfToken>.*)\\\"").Groups["csrfToken"].Value;
            url = "http://redmine.ebizway.co.kr:8081/redmine/login";
            string postData = "utf8=%E2%9C%93&authenticity_token="+ HttpUtility.UrlEncode(csrfToken)+"&back_url=http%3A%2F%2Fredmine.ebizway.co.kr%3A8081%2Fredmine%2F&username=jylee&password=8282&login=%EB%A1%9C%EA%B7%B8%EC%9D%B8+%C2%BB";
            
            scraper.Go(url, postData);
        }
        public void updateDownTxt(string date)
        {
            string url = "http://redmine.ebizway.co.kr:8081/redmine/login";
            string postData = "utf8=%E2%9C%93&authenticity_token=JWhuPr1m97JAE%2F%2B7gPn%2FiMWQObWuRjugTBNIgkDldivrTmuaQD56wYi7pDB3Dnhl1qtQHqj615caqUGJSlWQAw%3D%3D&username=jylee&password=8282&login=%EB%A1%9C%EA%B7%B8%EC%9D%B8+%C2%BB";
            Scraper Scraper = new Scraper();
            Scraper.Go(url, postData);
            return;
        }
    }
}
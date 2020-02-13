using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RedMine.Controllers
{
    public class RedMineController : Controller
    {

        [HttpPost]

		//public string Welcome()
		//{
		//	return "This is the Welcome action method...";
		//}

		public ActionResult Welcome(string name, int numTimes = 1)
		{
			ViewBag.Message = "Hello " + name;
			ViewBag.NumTimes = numTimes;

			return View();
		}

		public ActionResult updateDown()
		{
			return View();
		}

        public DataTable updateDownTxt(string date)
        {
            DataTable dt = new DataTable();
            string url = "http://redmine.ebizway.co.kr:8081/redmine/login";
            string postData = "utf8=%E2%9C%93&authenticity_token=JWhuPr1m97JAE%2F%2B7gPn%2FiMWQObWuRjugTBNIgkDldivrTmuaQD56wYi7pDB3Dnhl1qtQHqj615caqUGJSlWQAw%3D%3D&username=jylee&password=8282&login=%EB%A1%9C%EA%B7%B8%EC%9D%B8+%C2%BB";
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url, postData);
            
            return dt;
        }
    }
}
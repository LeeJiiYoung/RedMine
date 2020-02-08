using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedMine.Controllers
{
    public class RedMineController : Controller
    {
        // GET: RedMine
        //public ActionResult Index()
        //{
        //    return View();
        //}

		public string Index()
		{
			return "This is my default action...";
		}

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
    }
}
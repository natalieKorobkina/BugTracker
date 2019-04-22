using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HistoryController : Controller
    {
        // GET: History
        public ActionResult ShowHistory()
        {
            return View();
        }
    }
}
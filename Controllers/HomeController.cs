using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using dbshub.Models;

namespace dbshub.Controllers
{
    [LoginFilter]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Tables");
        }

        public ActionResult Tables()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Tables( TableSchema table)
        {
            if (!ModelState.IsValid)
            {
                return View(table);
            }
            else {
                var db = l.core.MongoDBHelper.GetMongoDB();
                var acc = SiteAccount.Get(HttpContext, db);
                table.Account = acc._id;
                db.GetCollection("TableSchema").Save(table);
                return Redirect("/home/tables");
            }
        }

        public ActionResult Table()
        {
            return View();
        }
    }
}

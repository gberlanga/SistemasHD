using SistemasHD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace SistemasHD.Controllers
{
    public class HomeController : Controller
    {
        private SistemasHDContext _db = new SistemasHDContext(); 

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public string GetName()
        {
            string name = User.Identity.Name;
            int index = name.IndexOf('\\');
            name = name.Substring(index + 1);
            return _db.Users.SingleOrDefault(u => u.Email.Contains(name)).Name;
        }
    }
}
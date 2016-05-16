using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RealEstate.App_Start;
using RealEstate.Properties;

namespace RealEstate.Controllers
{
    public class HomeController : Controller
    {

        public static RealEstateContext Context;

        public HomeController()
        {
            Context = new RealEstateContext();
        }

        public async Task<ActionResult> Index()
        {
            var buildInfoCommand = new BsonDocument("buildinfo", 1);
            var buildInfo = await Context.Database.RunCommandAsync<BsonDocument>(buildInfoCommand);
            return Content(buildInfo.ToJson(), "application/json");
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
    }
}
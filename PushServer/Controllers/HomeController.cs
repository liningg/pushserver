using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushServer.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PushServer.Controllers
{
    public class HomeController : Controller
    {
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
        public ActionResult Chat()
        {
            return View();
        }
        public ActionResult PushDetails()
        {
            ServerManager server = new ServerManager();
            ViewBag.listServer = ServerManager.listServer;
            ViewBag.listProject = ServerManager.listProject;
            return View();
        }
        public string GetMessage(string data)
        {
            int i = 1;
            string str = "";
            List<JObject> list =  ServerManager.listServer.FirstOrDefault(x => x.ServerId == data).listMessage;
            foreach (var item in list)
            {
                str =  str + "\n" + i++ +" : " +JsonConvert.SerializeObject(item);
            }
            return str;
        }

    }
}
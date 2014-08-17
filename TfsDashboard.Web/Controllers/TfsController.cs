using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TfsDashboard.Library;

namespace TfsDashboard.Web.Controllers
{
    public class TfsController : Controller
    {
        public ActionResult Index(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var settings = TfsDashboardSettingsLoader.Load();
            var manager = new TfsManager(settings.First());
            var img = manager.GetImage(username);
            return File(img, "image");
        }
    }
}
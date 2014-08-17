using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Mvc;
using System.Threading.Tasks;
using TfsDashboard.Web.App_Start;
using TfsDashboard.Library;

namespace TfsDashboard.Web.Controllers
{
    public class TfsDashboardController : ApiController
    {
        [WebApiOutputCache(3, "TfsSummary")]
        public TfsDashboardSummary Get()
        {
            var watch = Stopwatch.StartNew();
            var summary = new TfsDashboardSummary();

            var settings = TfsDashboardSettingsLoader.Load();
            var manager = new TfsManager(settings.First(x => x.IsPrimary));

            manager.GetBuildInformation(summary);
            manager.GetChangeset(summary);


            Trace.WriteLine(watch.Elapsed);
            return summary;
        }

    }
}

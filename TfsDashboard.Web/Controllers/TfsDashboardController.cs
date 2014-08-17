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
        public TfsDashboardSummary[] Get()
        {
            var watch = Stopwatch.StartNew();

            var settings = TfsDashboardSettingsLoader.Load().ToArray();

            var summaries = new TfsDashboardSummary[settings.Length];

            Parallel.ForEach(settings, (setting, state, idx) =>
            {
                var manager = new TfsManager(setting);

                var summary = new TfsDashboardSummary();
                manager.GetBuildInformation(summary);
                manager.GetChangeset(summary);
                summary.Name = setting.Name;
                summaries[idx] = summary;
            });

            Trace.WriteLine("TfsDashboardSummary Get: " + watch.Elapsed);
            return summaries;
        }

    }
}

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
        private static readonly object _lock = new object();
        private static Tuple<DateTime, TfsDashboardSummary[]> _cache;

        public TfsDashboardSummary[] Get()
        {
            TfsDashboardSummary[] result;
            if (TryGetCache(out result))
                return result;

            lock (_lock)
            {
                if (TryGetCache(out result))
                    return result;

                var watch = Stopwatch.StartNew();

                var settings = TfsDashboardSettingsLoader.Load().ToArray();

                result = new TfsDashboardSummary[settings.Length];

                Parallel.ForEach(settings, (setting, state, idx) =>
                {
                    var manager = new TfsManager(setting);
                    var summary = manager.CreateSummary();
                    summary.Name = setting.Name;
                    result[idx] = summary;
                });

                _cache = new Tuple<DateTime, TfsDashboardSummary[]>(DateTime.Now, result);
                Trace.WriteLine("TfsDashboardSummary Get: " + watch.Elapsed);
                return result;
            }
        }

        private bool TryGetCache(out TfsDashboardSummary[] summary)
        {
            summary = null;

            if (_cache != null)
            {
                var time = DateTime.Now - _cache.Item1;
                if (time.TotalSeconds < 3)
                {
                    summary = _cache.Item2;
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsDashboard.Library
{
    public class TfsDashboardSummary
    {
        public IEnumerable<TfsBuildSummary> LastBuilds { get; set; }

        public dynamic LastCheckins { get; set; }

        public TfsBuildSummary LastBuild { get; set; }

        public int CheckinsToday { get; set; }

        public IEnumerable<TfsCheckinStatistic> CheckinStatistic { get; set; }

        public int? LastWarningCount { get; set; }

        public string Name { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsDashboard.Library
{
    public class TfsDashboardSummary
    {
        public dynamic LastBuilds { get; set; }

        public dynamic LastCheckins { get; set; }

        public dynamic LastBuild { get; set; }

        public int CheckinsToday { get; set; }

        public dynamic CheckinStatistic { get; set; }

        public int? LastWarningCount { get; set; }

        public string Name { get; set; }
    }
}

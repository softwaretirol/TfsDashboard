using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsDashboard.Library
{
    public class TfsBuildSummary
    {
        public string Who { get; set; }
        public int Duration { get; set; }

        public string Username { get; set; }

        public string BuildNumber { get; set; }

        public string DropLocation { get; set; }

        public DateTime StartTime { get; set; }

        public Microsoft.TeamFoundation.Build.Client.BuildPhaseStatus TestStatus { get; set; }

        public string CompilationStatus { get; set; }

        public string Status { get; set; }

        public string SourceGetVersion { get; set; }

        public dynamic TestCoverage { get; set; }

        public Uri Uri { get; set; }
    }
}

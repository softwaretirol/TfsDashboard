using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsDashboard.Library
{
    public class TfsDashboardSettings
    {
        public bool IsPrimary { get; set; }
        public string Url { get; set; }
        public string Collection { get; set; }
        public string Password { get; set; }
        public string User { get; set; }
        public string Project { get; set; }
        public string BuildDefinition { get; set; }

        public TfsDashboardSettings()
        {
            IsPrimary = false;
        }
    }
}

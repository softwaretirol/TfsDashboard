using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsDashboard.Library
{
    public class TfsDashboardSettings
    {
        public string Url { get; set; }
        public string Collection { get; set; }
        public string Password { get; set; }
        public string User { get; set; }
        public string Project { get; set; }
        public string BuildDefinition { get; set; }
        public string Name { get; set; }
    }
}

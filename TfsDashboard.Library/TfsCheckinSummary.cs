using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsDashboard.Library
{
    class TfsCheckinSummary
    {
        public string Comment { get; set; }

        public string Committer { get; set; }

        public string Username { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime TimeElapsed { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker
{
    public static class ProjectConstants
    {
        public static readonly List<string> AllowedTicketTypes =
            new List<string> { "Bug", "Feature", "Database", "Support"};

        public static readonly List<string> AllowedTicketPriorities =
            new List<string> { "Low", "Medium", "High"};

        public static readonly List<string> AllowedTicketStatuses =
            new List<string> { "Open", "Resolved ", "Rejected"};
    }
}
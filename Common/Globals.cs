using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Common
{
    public static class Values
    {
        public static Stopwatch ActiveTime;
        public static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}

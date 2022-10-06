using LiveSplit.SourceSplit.ComponentHandling;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    public class SourceSplitUtils
    {
        public static Stopwatch ActiveTime = new Stopwatch();

        /// <summary>
        /// Checks if the process is associated with a VAC protected game.
        /// We don't want to touch them. Even though reading a VAC process'
        /// memory is said to be perfectly fine and only writing is bad.
        /// </summary>
        public static bool IsVACProtectedProcess(Process p)
        {
            // http://forums.steampowered.com/forums/showthread.php?t=2465755
            // http://en.wikipedia.org/wiki/Valve_Anti-Cheat#Games_that_support_VAC
            string[] badExes = { "csgo", "dota2", "swarm", "left4dead",
                "left4dead2", "dinodday", "insurgency", "nucleardawn", "ship" };
            string[] badMods = { "cstrike", "dods", "hl2mp", "insurgency", "tf", "zps" };
            string[] badRootDirs = { "Dark Messiah of Might and Magic Multi-Player" };
            string[] hl2SurvivorDirs = { "hl2mp_japanese", "hl2_japanese" };

            if (badExes.Contains(p.ProcessName.ToLower()))
                return true;

            if (p.ProcessName.ToLower() == "hl2" || p.ProcessName.ToLower() == "mm")
            {
                // it's too difficult to get another process' start arguments, so let's scan the dir
                // http://stackoverflow.com/questions/440932/reading-command-line-arguments-of-another-process-win32-c-code

                try
                {
                    string dir = Path.GetDirectoryName(p.MainModule.FileName);
                    if (dir == null)
                        return true;

                    var directories = new DirectoryInfo(dir).GetDirectories();
                    if (directories.Any(di => badMods.Contains(di.Name.ToLower()))
                         && !directories.Any(di => hl2SurvivorDirs.Contains(di.Name.ToLower())))
                        return true;

                    string root = new DirectoryInfo(dir).Name.ToLower();
                    if (badRootDirs.Any(badRoot => badRoot.ToLower() == root))
                        return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    return true;
                }
            }

            return false;
        }
    }

    // prepends update count and tick count to every Debug.WriteLine
    public class TimedTraceListener : DefaultTraceListener
    {
        private static TimedTraceListener _instance;
        public static TimedTraceListener Instance => _instance ?? (_instance = new TimedTraceListener());


        public int TickCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }

        public int UpdateCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }

        public override void WriteLine(string message)
        {
#if false
            DebugOutputForm.Instance?.Add(_sw.Elapsed, UpdateCount, TickCount, message);
#endif
            message = $"SourceSplit {SourceSplitUtils.ActiveTime.Elapsed.ToStringCustom(4)}|{this.UpdateCount}|{this.TickCount} : {message}";
            base.WriteLine(message);

#if DEBUG 
            // flat out doesnt work on some machines, TODO figure out a way to debug stuff
            // be careful with multiple instances, if you don't want the hassle, #if false me away
            Task e = WriteFileAsync("sourcesplit_log.txt", message + "\r\n");
#endif
        }

        private static async Task WriteFileAsync(string file, string content)
        {
            using (StreamWriter outputFile = new StreamWriter(file, append: true))
            {
                await outputFile.WriteAsync(content);
            }
        }
    }
}

using LiveSplit.SourceSplit.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using LiveSplit.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class SourceSplitUtils
    {
        public static Stopwatch ActiveTime = new Stopwatch();
        private static Dictionary<string, bool> _vacQueriedExecs = new Dictionary<string, bool>();
        
        public static bool IsVACProtectedProcess(Process p)
        {
            var mainProcPath = p.MainModule.FileName;

            if (_vacQueriedExecs.TryGetValue(mainProcPath, out var res) && res)
                return true;

            try
            {
                if (IsVACProtectedProcessName(p.ProcessName) && IsVACProtectedGame(mainProcPath))
                    return true;
            }
            catch (Exception e)
            {
                Logging.WriteLine($"Couldn't check process for VAC support: {e}");
                return true;
            }

            try
            {
                res = IsVACProtectedSteamProduct(Path.GetDirectoryName(mainProcPath));
                _vacQueriedExecs[mainProcPath] = res;

                if (res) return true;
            }
            catch (Exception e)
            {
                Logging.WriteLine($"Couldn't check Steam API for VAC support: {e}");
            }

            return false;
        }

        public static bool IsVACProtectedProcessName(string p)
        {
            string[] badExes =
            {
                "csgo", "dota2", "swarm", "left4dead",
                "left4dead2",  "dinodday",  "insurgency",
                "nucleardawn", "ship"
            };

            return badExes.Contains(p.ToLower());
        }

        public static bool IsVACProtectedGame(string path)
        {
            string[] badMods = { "cstrike", "dods", "hl2mp", "insurgency", "tf", "zps" };
            string[] badRootDirs = { "Dark Messiah of Might and Magic Multi-Player" };
            string[] hl2SurvivorDirs = { "hl2mp_japanese", "hl2_japanese" };

            var dirInfo = new DirectoryInfo(path);

            var dirs = dirInfo.GetDirectories();
            if (dirs.Any(x => badMods.Contains(x.Name.ToLower())) && !dirs.Any(x => hl2SurvivorDirs.Contains(x.Name.ToLower())))
                return true;

            string root = dirInfo.Name.ToLower();
            if (badRootDirs.Any(badRoot => badRoot.ToLower() == root))
                return true;

            return false;
        }

        public static bool IsVACProtectedSteamProduct(string path)
        {
            bool result = false;

            CancellationTokenSource cts = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                int id = int.Parse(File.ReadAllText(Path.Combine(path, "steam_appid.txt").Trim(' ', '\t', '\r', '\n')));
                var req = HttpWebRequest.Create($"https://store.steampowered.com/api/appdetails?appids=" + id);
                var res = new StreamReader(Task.Run(() => req.GetResponseAsync(), cts.Token).Result.GetResponseStream()).ReadToEnd();

                if (res.Contains(@"{""success"":false}")) throw new Exception("API request failed.");

                res = res.Replace("\\\\", "//"); // help with managing escape sequences
                var category = Regex.Match(res, @"""categories"":\[(\\]|[^\]])+\]");
                if (!category.Success) throw new Exception("Couldn't find category information");

                result = category.Value.Contains(@"{""id"":8");
            }, cts.Token);

            cts.CancelAfter(2000);
            task.Wait();

            if (task.Status == TaskStatus.Faulted) throw task.Exception;

            return result;
        }
    }

    public static class Logging
    {
        private struct Message
        {
            public string Content;
            public TimeSpan ActiveTime;
            public DateTime TimeOfCreation;

            public Message(string msg)
            {
                Content = msg;
                ActiveTime = SourceSplitUtils.ActiveTime.Elapsed;
                TimeOfCreation = DateTime.Now;
            }
        }
        private static ConcurrentQueue<Message> _messages = new ConcurrentQueue<Message>();
        private static CancellationTokenSource _cts;
        private static Thread _writeThread = new Thread(() =>
        {
            while (true)
            {
                if (_cts.IsCancellationRequested) return;

                while (_messages.Count > 0 && _messages.TryDequeue(out Message msg))
                {
                    if (_cts.IsCancellationRequested) return;
                    WriteLineInternal(msg);
                }

                Thread.Sleep(10);
            }
        });

        public static int TickCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }
        public static int UpdateCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }
        private static bool _writeFile = true;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void StartLogging()
        {
            TickCount = UpdateCount = 0;
            _writeFile = true;

            _cts = new CancellationTokenSource();
            _writeThread.Start();

            WriteLine("Logging started");
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void StopLogging()
        {
            _cts.Cancel();
            _writeThread.Abort();

            WriteLineInternal(new Message("Logging stopped"));
        }

        private static void WriteLineInternal(Message msg)
        {
            Debug.WriteLine
            (
                $"SourceSplit " +
                $"{SourceSplitUtils.ActiveTime.Elapsed} | " +
                $"{UpdateCount} | " +
                $"{TickCount} : " +
                $"{msg.Content}"
            );

            if (!_writeFile) return;

            var str =
                $"[ " +
                $"{msg.TimeOfCreation:yyyy/MM/dd @ HH:mm:ss.ffffff} | " +
                $"{msg.ActiveTime} | " +
                $"{UpdateCount} | " +
                $"{TickCount} " +
                $"] " +
                $"{msg.Content}";

            List<Exception> errors = new List<Exception>();
            for (int tries = 10; tries > 0; tries--)
            {
                try
                {
                    File.AppendAllText("sourcesplit_log.txt", str + Environment.NewLine);
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"SourceSplit Logging: Fatal exception while trying to log: {e.Message} ({tries} tries left)");

                    errors.Add(e);
                    if (!(e is IOException)) break;
                    continue;
                }
            }

            new ErrorDialog
            (
                "Encountered one or more problems while writing to log file. Logging will be disabled for this session of SourceSplit!",
                false,
                errors.GroupBy(x => x.Message).Select(x => x.First()).ToArray()
            );
            _writeFile = false;
        }
        public static void WriteLine(string msg = "")
        {
            _messages.Enqueue(new Message(msg));
        }
        public static void WriteLine(object obj = null) => WriteLine(obj?.ToString() ?? "");

        public static void WriteLineIf(bool cond, string msg)
        {
            if (cond) WriteLine(msg);
        }
        public static void WriteLineIf(bool cond, object obj) => WriteLineIf(cond, obj?.ToString() ?? "");
    }
}

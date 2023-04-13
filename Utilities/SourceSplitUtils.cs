using LiveSplit.SourceSplit.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class SourceSplitUtils
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
            string[] badExes = 
            { 
                "csgo", "dota2", "swarm", "left4dead", 
                "left4dead2",  "dinodday",  "insurgency", 
                "nucleardawn", "ship" 
            };
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
                    Logging.WriteLine(ex.ToString());
                    return true;
                }
            }

            return false;
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

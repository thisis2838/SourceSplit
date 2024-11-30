﻿using LiveSplit.SourceSplit.Utilities.Forms;
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
using LiveSplit.SourceSplit.GameSpecific;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class SourceSplitUtils
    {
        public static Stopwatch ActiveTime = new Stopwatch();
        
        public static bool IsVACProtectedProcess(Process p)
        {
            var mainProcPath = p.MainModule?.FileName;
            if (mainProcPath == null)
            {
                return false;
            }

            try
            {
                if (IsVACProtectedProcessName(p.ProcessName) || IsVACProtectedGameDir(Path.GetDirectoryName(mainProcPath)))
                    return true;
            }
            catch (Exception e)
            {
                Logging.WriteLine($"Couldn't check process for VAC support: {e}");
                return true;
            }

            try
            {
                if (IsVACProtectedSteamProduct(Path.GetDirectoryName(mainProcPath)))
                    return true;
            }
            catch (Exception e)
            {
                // checking steam api only ensures that a game has vac support. if it fails, we assume it doesn't.
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

        public static bool IsVACProtectedGameDir(string path)
        {
            // since anniversary update hl2mp has been bundled in next to hl2 and the episodes...
            string[] badMods = { "cstrike", "dods", /*"hl2mp",*/ "insurgency", "tf", "zps" };
            string[] badRootDirs = { "Dark Messiah of Might and Magic Multi-Player" };
            // hl2 survivor has a game folder of "hl2mp", but isn't covered by vac.
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

        // this should be comprehensive enough...
        // false is without VAC, true is with VAC
        private static Dictionary<int, bool> _appIDVACStatuses = new Dictionary<int, bool>()
        {
            { 211, false },
            { 215, false },
            { 218, false },
            { 219, false },
            { 220, false },
            { 240, false }, // hls is exempt
            { 280, false },
            { 340, false },
            { 380, false },
            { 400, false },
            { 420, false },
            { 1300, false },
            { 2600, false },
            { 10220, false },
            { 17520, false }, // synergy is exempt
            { 91700, false },
            { 203810, false },
            { 221910, false },
            { 243730, false },
            { 247750, false },
            { 251110, false },
            { 261820, false },
            { 290930, false },
            { 303210, false },
            { 362890, false },
            { 365300, false },
            { 399120, false },
            { 587650, false },
            { 714070, false },
            { 723390, false },
            { 747250, false },
            { 1154130, false },
            { 1583720, false },
            { 5438926, false },

            //{ 240, true }, hls identifies as this for some reason...
            { 320, true },
            { 360, true },
            { 440, true },
            { 500, true },
            { 550, true },
            { 570, true },
            { 620, true },
            { 630, true },
            { 730, true },
            { 1800, true },
            { 2130, true },
            { 4000, true },
            { 17500, true},
            { 17700, true },
            { 17710, true },
            { 222880, true },
        };

        public static bool IsVACProtectedSteamProduct(string path)
        {
            bool result = false;

            CancellationTokenSource cts = new CancellationTokenSource();
            var task = Task.Run
            (
                () =>
                {
                    var appIdFile = Path.Combine(path, "steam_appid.txt").Trim(' ', '\t', '\r', '\n');
                    // we're most likely running some old distribution of a source game, not running on steam,
                    // or not a source game at all, both do not fall within vac purview 
                    if (!File.Exists(appIdFile))
                        return;

                    // find if game has an appid associated with it
                    int id = int.Parse(File.ReadAllText(appIdFile));
                    // check if we have this already
                    if (_appIDVACStatuses.TryGetValue(id, out var cached))
                    {
                        result = cached;
                        return;
                    }

                    // if found, query for its information from the steam store api
                    var req = HttpWebRequest.Create($"https://store.steampowered.com/api/appdetails?appids=" + id);

                    // get a response, read to end. it should be in json format
                    var response = Task.Run(() => req.GetResponseAsync(), cts.Token).Result;
                    var responseStream = new StreamReader(response.GetResponseStream());
                    var responseContent = Task.Run(() => responseStream.ReadToEndAsync(), cts.Token).Result;

                    // this indicates if the api request has failed, maybe because the game doesn't exist or has been delisted
                    if (responseContent.Contains(@"{""success"":false}")) throw new Exception("API request failed.");

                    // remove double escaped backslashes to remove edge cases in regex detection
                    responseContent = responseContent.Replace("\\\\", "//");
                    // ideally, we'd wanna parse the json, but that'd require extra work or a dependancy
                    // so let's just regex search for the right node.
                    // find the category array
                    var category = Regex.Match(responseContent, @"""categories"":\[(\\]|[^\]])+\]");
                    if (!category.Success) throw new Exception("Couldn't find category information");

                    // vac being supported has a category of 8
                    result = category.Value.Contains(@"{""id"":8");
                    _appIDVACStatuses[id] = result;
                }, 
                cts.Token
            );
            cts.CancelAfter(2000); // don't wait for more than 2s
            task.Wait();

            if (task.Status == TaskStatus.Faulted) throw task.Exception;
            return result;
        }
    }

    public static class Logging
    {
        public static string LogFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SourceSplit", "logs", _logFileName);

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
        private static Thread _writeThread = null;

        private static string _logFileName = DateTime.Now.ToString(@"yyyy-MM-dd") + ".csv";

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
        private static bool _enabled = true;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void StartLogging()
        {
            TickCount = UpdateCount = 0;
            _enabled = true;

            _cts = new CancellationTokenSource();

            if (_writeThread != null)
            {
                try { _writeThread.Abort(); } 
                catch { }
            }
            _writeThread = new Thread(() =>
            {
                while (true)
                {
                    SpinWait.SpinUntil(() => _cts.Token.IsCancellationRequested || _messages.Count > 0);
                    if (_cts.IsCancellationRequested) return;

                    while (_messages.TryDequeue(out Message msg))
                    {
                        if (_cts.IsCancellationRequested) return;
                        WriteToFile(msg);
                    }
                }
            });
            _writeThread.IsBackground = true;
            _writeThread.Start();

            WriteLine("Logging started");
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void StopLogging()
        {
            _cts.Cancel();
            _writeThread.Abort();

            WriteToFile(new Message("Logging stopped"));
        }

        private static void WriteToFile(Message msg)
        {
            Debug.WriteLine($"SourceSplit @ {SourceSplitUtils.ActiveTime.Elapsed}, {UpdateCount}, {TickCount} : {msg.Content}");

            if (!_enabled)
            {
                return;
            }

            var line = new List<string>()
            {
                msg.TimeOfCreation.ToString(@"yyyy/MM/dd"),
                msg.TimeOfCreation.ToString(@"HH:mm:ss.ffffff"),
                msg.ActiveTime.ToString(),
                UpdateCount.ToString(),
                TickCount.ToString(),
                msg.Content
            };
            string written = string.Join(",", line.Select(x => "\"" + x.Replace("\"", "\"\"") + "\""));

            List<Exception> errors = new List<Exception>();
            for (int tries = 10; tries > 0; tries--)
            {
                try
                {
                    var saveFolder = Path.GetDirectoryName(LogFilePath);
                    if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
                    File.AppendAllText(LogFilePath, written + Environment.NewLine);
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"SourceSplit Logging: Fatal exception while trying to log: {e.Message} ({tries} tries left)");
                    errors.Add(e);
                    if (e is IOException)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    break;
                }
            }

            new ErrorDialog
            (
                "Encountered one or more problems while writing to log file. Logging will be disabled for this session of SourceSplit.",
                false,
                errors.GroupBy(x => x.Message).Select(x => x.First()).ToArray()
            );
            _enabled = false;
        }
        public static void WriteLine(string msg = "")
        {
            _messages.Enqueue(new Message(msg));
        }
        public static void WriteLine(object obj = null) => WriteLine(obj?.ToString() ?? "<null>");

        public static void WriteLineIf(bool cond, string msg)
        {
            if (cond) WriteLine(msg);
        }
        public static void WriteLineIf(bool cond, object obj) => WriteLineIf(cond, obj?.ToString() ?? "<null>");
    }
}

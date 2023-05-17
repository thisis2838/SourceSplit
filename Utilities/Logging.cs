using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class Logging
    {
        public struct Message
        {
            public string Content;
            public TimeSpan ActiveTime;
            public DateTime TimeOfCreation;

            public Message(string msg)
            {
                Content = msg.Replace(Environment.NewLine, "\n");
                ActiveTime = Common.Globals.ActiveTime.Elapsed;
                TimeOfCreation = DateTime.Now;
            }
        }
        private static ConcurrentQueue<Message> _messages = new ConcurrentQueue<Message>();
        private static CancellationTokenSource _cts = null;
        private static Thread _writeThread = null;

        public static EventHandler<Message> MessageLogged;

        public static int TickCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)] get;
            [MethodImpl(MethodImplOptions.Synchronized)] set;
        }
        public static int UpdateCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)] get;
            [MethodImpl(MethodImplOptions.Synchronized)] set;
        }
        private static bool _enabled = true;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Start()
        {
            TickCount = UpdateCount = 0;
            _enabled = true;

            _cts = new CancellationTokenSource();
            _writeThread = new Thread(() =>
            {
                while (true)
                {
                    if (_cts.IsCancellationRequested) return;

                    while (_messages.Count > 0 && _messages.TryDequeue(out Message msg))
                    {
                        MessageLogged?.BeginInvoke(null, msg, null, null);
                        if (_cts.IsCancellationRequested) return;
                        WriteToFile(msg);
                    }

                    Thread.Sleep(10);
                }
            });
            _writeThread.Start();

            WriteLine("Logging started");
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Stop()
        {
            _cts.Cancel();
            _writeThread.Abort();

            WriteToFile(new Message("Logging stopped"));
        }

        private static void WriteToFile(Message msg)
        {
            Debug.WriteLine
            (
                $"SourceSplit " +
                $"{Common.Globals.ActiveTime.Elapsed} | " +
                $"{UpdateCount} | " +
                $"{TickCount} : " +
                $"{msg.Content}"
            );

            if (!_enabled) return;

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

            ErrorWindow.Show
            (
                "Encountered one or more problems while writing to log file. Logging will be disabled for this session of SourceSplit!",
                false,
                errors.GroupBy(x => x.Message).Select(x => x.First()).ToArray()
            );
            _enabled = false;
        }

        public static void WriteLine(object msg = null)
        {
            string str = msg is null ? "" : msg.ToString();
            _messages.Enqueue(new Message(str));
        }
        private static void WriteLineIf(bool cond, object msg)
        {
            if (cond) WriteLine(msg);
        }
        public static void ErrorLine(string errorMsg)
        {
            WriteLine("!!! ERROR !!! " + errorMsg);
        }
        public static void WarningLine(string warningMsg)
        {
            WriteLine("!! WARNING !! " + warningMsg);
        }
    }
}

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.ComponentHandling;
using LiveSplit.SourceSplit.Utilities;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.ComponentModel;
using LiveSplit.SourceSplit.GameHandling;
using WinUtils = LiveSplit.SourceSplit.Utilities.WinUtils;
using System.Text.RegularExpressions;
using static LiveSplit.SourceSplit.ComponentHandling.SourceSplitComponent;

namespace LiveSplit.SourceSplit.DemoHandling
{
    class DemoMonitor
    {
        public bool Functional { get; private set; } = false;

        public class DemoTickUpdateArgs : EventArgs
        {
            public GameState State { get; internal set; }
            public long CurrentTick { get; internal set; }
            public long LastTick { get; internal set; }
            public string DemoName { get; internal set; }
        }
        public EventHandler<DemoTickUpdateArgs> DemoTickUpdate;

        public class DemoStopRecordingArgs : EventArgs
        {
            public GameState State { get; internal set; }
            public DemoFile Demo { get; internal set; }
            public int FinalDifference { get; internal set; }
        }
        public EventHandler<DemoStopRecordingArgs> DemoStopRecording;

        public class DemoStartRecordingArgs : EventArgs
        {
            public GameState State { get; internal set; }
            public string DemoName { get; internal set; }
        }
        public EventHandler<DemoStartRecordingArgs> DemoStartRecording;


        private bool _first = true;

        private string _gameDir;

        private SigScanTarget _demoRecorderTarget;
        private int _startTickOffset = -1;

        private MemoryWatcherList _watch = new MemoryWatcherList();
        private MemoryWatcher<bool> _demoIsRecording;
        //private MemoryWatcher<bool> _demoIsHeader;
        private MemoryWatcher<int> _demoIndex;
        private MemoryWatcher<int> _demoFrame;

        private ValueWatcher<string> _demoName;
        private IntPtr _demoNamePtr;
        private StringWatcher _demoNameBase;

        public ValueWatcher<long> _index = new ValueWatcher<long>(0);
        private MemoryWatcher<int> _hostTick;
        private MemoryWatcher<int> _startTick;

        public DemoMonitor()
        {
            _demoRecorderTarget = new SigScanTarget(0, "416c7265616479207265636f7264696e672e");
            _demoRecorderTarget.OnFound = (proc, scanner, ptr) =>
            {
                byte[] b = BitConverter.GetBytes(ptr.ToInt32());
                var target = new SigScanTarget(-95, $"68 {b[0]:X02} {b[1]:X02} {b[2]:X02} {b[3]:X02}");

                IntPtr byteArrayPtr = scanner.Scan(target);
                if (byteArrayPtr == IntPtr.Zero)
                    return IntPtr.Zero;

                byte[] bytes = new byte[100];
                proc.ReadBytes(scanner.Scan(target), 100).CopyTo(bytes, 0);
                for (int i = 98; i >= 0; i--)
                {
                    if (bytes[i] == 0x8B && bytes[i + 1] == 0x0D)
                        return proc.ReadPointer(proc.ReadPointer(byteArrayPtr + i + 2));
                }

                return IntPtr.Zero;
            };
        }

        public bool Scan(Process process, string gameDir)
        {
            Reset();

            ProcessModuleWow64Safe engine = process.ModulesWow64SafeNoCache().FirstOrDefault(x => x.ModuleName.ToLower() == "engine.dll");
            if (engine == null)
                return false;

            _gameDir = gameDir;

            SignatureScanner scanner = new SignatureScanner(process, engine.BaseAddress, engine.ModuleMemorySize);
            IntPtr demoRecorderPtr = IntPtr.Zero, gameDirPtr = IntPtr.Zero, hostTickPtr = IntPtr.Zero;

            #region DEMO RECORDER
            demoRecorderPtr = scanner.Scan(_demoRecorderTarget);
            if (demoRecorderPtr == IntPtr.Zero)
                return false;
            #endregion

            #region HOST TICK & START TICK
            for (int i = 10; i > 0; i--)
            {
                SignatureScanner tmpScanner = new SignatureScanner(process, process.ReadPointer(process.ReadPointer(demoRecorderPtr) + i * 4), 0x100); 
                SigScanTarget startTickAccess = new SigScanTarget(2, $"2B ?? ?? ?? 00 00");
                SigScanTarget hostTickAccess = new SigScanTarget(1, "A1");
                hostTickAccess.OnFound = (f_proc, f_scanner, f_ptr) => f_proc.ReadPointer(f_ptr);
                startTickAccess.OnFound = (f_proc, f_scanner, f_ptr) =>
                {
                    IntPtr hostTickOffPtr = f_scanner.ScanGetAll(hostTickAccess)
                        .FirstOrDefault(x => scanner.IsWithin(x.OnFoundResult)).OnFoundResult;

                    if (hostTickOffPtr != IntPtr.Zero)
                    {
                        _startTickOffset = f_proc.ReadValue<int>(f_ptr);
                        return hostTickOffPtr;
                    }
                    return IntPtr.Zero;
                };

                IntPtr ptr = tmpScanner.Scan(startTickAccess);
                if (ptr == IntPtr.Zero)
                    continue;
                else
                {
                    hostTickPtr = ptr;
                    break;
                }
            }

            if (hostTickPtr == IntPtr.Zero || _startTickOffset == 0)
                return false;
            #endregion

            _demoIndex = new MemoryWatcher<int>(demoRecorderPtr + _startTickOffset + 4 + 260 + 1 + 1 + 2);
            _demoFrame = new MemoryWatcher<int>(demoRecorderPtr + _startTickOffset + 4 + 260 + 4 + 4);
            _demoIsRecording = new MemoryWatcher<bool>(demoRecorderPtr + _startTickOffset + 4 + 260 + 2);
            //_demoIsHeader = new MemoryWatcher<bool>(demoRecorderPtr + _startTickOffset + 4 + 260);
            _demoNameBase = new StringWatcher(demoRecorderPtr + _startTickOffset + 4, 260);

            _demoNamePtr = demoRecorderPtr + 4;
            _demoName = new ValueWatcher<string>(process.ReadString(_demoNamePtr, 260));

            _hostTick = new MemoryWatcher<int>(hostTickPtr);
            _startTick = new MemoryWatcher<int>(demoRecorderPtr + _startTickOffset);

            IntPtr isRecPtr = demoRecorderPtr + _startTickOffset + 4 + 260 + 2;
            IntPtr framePtr = isRecPtr + 2 + 4;
            if (process.ReadValue<bool>(isRecPtr) == false &&
                process.ReadValue<int>(framePtr) == 0)
            {
                process.WriteValue<int>(framePtr, 1);
            }

            _watch = new MemoryWatcherList()
            {
                _demoIndex,
                _demoIsRecording,
                //_demoIsHeader,
                _demoNameBase,
                _demoFrame,
                _hostTick,
                _startTick,
            };
            _watch.UpdateAll(process);

            Functional = true;
            return true;
        }

        public void Reset()
        {
            Functional = false;
            _index.Current = _index.Current = 0;
            _first = true;
        }

        private string GetCurDemoName()
        {
            string name = _demoName.Old;
            string nameBase = _demoNameBase.Old;

            //_demoNamePtr is just a rough guess, could change based on compilers used and have extra data preceeding 
            // name data, so we'll base it off another name pointer we know.
            if (Regex.Match(name, $@"{Regex.Escape(nameBase)}(?:_[0-9]+)?\.dem") is var match && match.Success)
                return name.Substring(match.Index);

            return nameBase + "(?)";
        }


        public void Update(GameState state)
        {
            if (!Functional || state.GameProcess.HasExited)
                return;

            _watch.UpdateAll(state.GameProcess);

            string curName = _demoName.Current;
            if (!(curName = state.GameProcess.ReadString(_demoNamePtr, 260 + 4 * 3) ?? "").Contains("demoheader.tmp") && 
                !string.IsNullOrWhiteSpace(curName))
                _demoName.Current = curName;
            else _demoName.Current = _demoName.Current;

            bool switched = _demoIsRecording.Current && (_demoIndex.Changed && _demoIndex.Current > 1);

            //Debug.WriteLine($"cur {_index.Current} host {_hostTick.Current} start {_startTick.Current} rec {_demoIsRecording.Current} ind {_demoIndex.Current} swc {switched} frm {_demoFrame.Current}");

            if (switched || (!_demoIsRecording.Current && _demoIsRecording.Old))
            {
                string name = GetCurDemoName();
                string path = Path.Combine(_gameDir, name);

                if (File.Exists(path) && DemoFile.FromFilePath(path, out var demo))
                {
                    _index.Current = (int)demo.TotalTicks;
                    Debug.WriteLine($"Demo finished: {demo.Name} ({demo.TotalTicks} ticks), differ by {_index.Current - _index.Old} ticks");

                    DemoStopRecording.Invoke(null, new DemoStopRecordingArgs()
                    {
                        Demo = demo,
                        FinalDifference = (int)(_index.Current - _index.Old),
                        State = state
                    });

                    new Thread(new ThreadStart(() => PrintInfo(demo, state))).Start();
                }
            }

            if (_demoIsRecording.Current && (_demoFrame.Current < _demoFrame.Old))
            {
                _index.Current = _index.Current = 0;

                DemoStartRecording.Invoke(null, new DemoStartRecordingArgs()
                {
                    DemoName = GetCurDemoName(),
                    State = state
                });

                Debug.WriteLine($"Demo recording start");
            }

            if (_demoIsRecording.Current && state.SignOnState.Current == SignOnState.Full)
            {
                if (_hostTick.Current >= _startTick.Current)
                    _index.Current = _hostTick.Current - _startTick.Current;
                else _index.Current = _index.Current;

                DemoTickUpdate.Invoke(null, new DemoTickUpdateArgs()
                {
                    State = state,
                    CurrentTick = _index.Current,
                    LastTick = _index.Old,
                    DemoName = GetCurDemoName()
                }); ;
            }
            else _index.Current = _index.Current;

            if (_first)
                _index.Current = _index.Current;
        }

        private void PrintInfo(DemoFile file, GameState state)
        {
            if (!Settings.PrintDemoInfo.Value)
                return;

            string path = Settings.DemoParserPath.Value;

            void sendMsg(string msg)
            {
                List<string> msgs = new List<string>
                {
                    " ",
                    $"Info for: {file.Name}.dem",
                    " "
                };

                msg = msg.Replace("\r\n", "\n").Trim(' ', '\n');
                msg.Split('\n').ToList().ForEach(x => msgs.Add(x));

                msgs.Add(" ");

                state.GameProcess.SendMessage("echo \"" + String.Join("\";echo \"", msgs) + " \"");
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                sendMsg(@$"
Path:   {file.FilePath}
Index:  {file.Index}
Map:    {file.MapName}
Player: {file.PlayerName}
Game:   {file.GameName}

Ticks:  {file.TotalTicks + 1}
 ");
                return;
            }
            else
            {
                Process listdemo = new Process();
                listdemo.StartInfo.FileName = "cmd.exe";
                listdemo.StartInfo.Arguments =
                    $"/C " +
                    $"{Path.GetPathRoot(path).Replace("\\", "")} " +
                    $"&\"{path}\" " +
                    $"\"{file.FilePath}\"" +
                    $"& exit";
                listdemo.StartInfo.UseShellExecute = false;
                listdemo.StartInfo.CreateNoWindow = true;
                listdemo.StartInfo.RedirectStandardOutput = true;
                string output = "";
                listdemo.OutputDataReceived += (s, e) =>
                {
                    output += e.Data + "\n";
                };
                listdemo.Start();
                listdemo.BeginOutputReadLine();
                listdemo.WaitForExit();
                
                sendMsg(output);
            }
        }
    }
}

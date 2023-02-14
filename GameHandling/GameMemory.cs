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
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.DemoHandling;
using System.Collections.Generic;
using System.ComponentModel;
using LiveSplit.SourceSplit.ComponentHandling;
using LiveSplit.SourceSplit.Utilities.Forms;
using System.Management;
using System.Diagnostics.Eventing.Reader;

namespace LiveSplit.SourceSplit.GameHandling
{
    /// <summary>
    /// Class that handles game process acquisition, memory updating and Auto-splitting
    /// </summary>
    partial class GameMemory
    {
        public TimerActions TimerActions = new TimerActions();
        public GameState _state;

        private Task _thread;
        private SynchronizationContext _uiThread;
        private CancellationTokenSource _cancelSource;

        private SigScanTarget _gameDirTarget;
        private IntPtr _gameDirPtr;

        private bool _gotTickRate;

        private int _timesOver;
        private int _timeOverSpent;

        private SourceSplitSettings _settings;

        private ValueWatcher<long> _hostUpdateCount;
        private IntPtr _hostUpdateCountPtr;

        private DemoMonitor _demoMonitor;

        private readonly string[] _targetProccesNames = new string[]
        {
            "hl2.exe",
            "portal2.exe",
            "dearesther.exe",
            "mm.exe",
            "EYE.exe",
            "bms.exe",
            "infra.exe",
            "stanley.exe",
            "hdtf.exe",
            "beginnersguide.exe",
            "synergy.exe",
            "sinepisodes.exe",
        };

        // TODO: match tickrate as closely as possible without going over
        // otherwise we will most likely read when the game isn't sleeping
        // must also account for variance of windows scheduler
        private const int TARGET_UPDATE_RATE = 13;

        public GameMemory(SourceSplitComponent component)
        {
            _settings = component.SettingControl;

            component.TimerOnReset += (s, e) => 
            {
                _state?.MainSupport?.OnTimerReset(e.TimerStarted);
            };

            // TODO: find better way to do this. multiple sigs instead?
            _gameDirTarget = new SigScanTarget(0, "25732F736176652F25732E736176"); // "%s/save/%s.sav"
            _gameDirTarget.OnFound = (proc, scanner, ptr) => 
            {
                byte[] b = BitConverter.GetBytes(ptr.ToInt32());
                var target = new SigScanTarget(-4,
                    // push    offset aSMapsS_sav
                    $"68 {b[0]:X02} {b[1]:X02} {b[2]:X02} {b[3]:X02}");
                IntPtr ptrPtr = scanner.Scan(target);
                if (ptrPtr == IntPtr.Zero)
                    return IntPtr.Zero;
                return proc.ReadPointer(ptrPtr);
            };

            _demoMonitor = new DemoMonitor();
            _demoMonitor.DemoTickUpdate += DemoRecorder_TickUpdate;
            _demoMonitor.DemoStartRecording += DemoRecorder_StartRecording;
            _demoMonitor.DemoStopRecording += DemoRecorder_StopRecording;
        }

#if DEBUG
        ~GameMemory()
        {
            // sometimes this throws a dud cannot access disposed object exception
            try { Debug.WriteLine("GameMemory finalizer"); }
            catch { }
        }
#endif

        public void StartReading()
        {
            if (_thread != null && _thread.Status == TaskStatus.Running)
                throw ErrorDialog.Exception("Memory reading thread is null or isn't running!");
            if (!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))
                throw ErrorDialog.Exception("SynchronizationContext.Current is not a UI thread.");

            _cancelSource = new CancellationTokenSource();
            _uiThread = SynchronizationContext.Current;
            TimerActions.Init();
            _thread = Task.Factory.StartNew(() => MemoryReadThread(_cancelSource));
        }
        public void StopReading()
        {
            if (_cancelSource == null || _thread == null || _thread.Status != TaskStatus.Running)
                return;

            _cancelSource.Cancel();
            _thread.Wait();
        }
        void MemoryReadThread(CancellationTokenSource cts)
        {
            // force windows timer resolution to 1ms. it probably already is though, from the game.
            WinUtils.timeBeginPeriod(1);
            // we do a lot of timing critical stuff so this may help out
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

            while (true)
            {
                if (_cancelSource?.IsCancellationRequested ?? false)
                    break;

                try
                {
                    Debug.WriteLine("Waiting for process");

                    GameState state;
                    while (!this.TryGetGameProcess(out state))
                    {
                        Thread.Sleep(750);

                        SendGameStatusEvent(false);

                        if (cts.IsCancellationRequested)
                            goto ret;
                    }

                    this.HandleProcess(state, cts);

                    SendSetTimingSpecificsEvent(new TimingSpecifics());
                    SourceSplitComponent.Settings.GetUIRepresented().ForEach(x => x.Unlock());

                    Debug.WriteLine($"Process exited");

                    if (cts.IsCancellationRequested)
                        goto ret;
                }
                catch (Exception ex) 
                {
                    Debug.WriteLine(ex.ToString());
#if DEBUG
#else
                    if (!(ex is InvalidOperationException || ex is Win32Exception))
                    {
                        new ErrorDialog("Encountered unhandled exception when reading game memory", false, ex);
                    }
#endif
                    Thread.Sleep(1000);
                }


                SendGameStatusEvent(false);
            }

            ret:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            WinUtils.timeEndPeriod(1);
        }
        bool GetGameInfo(GameState state, Process p)
        {
            // special case for half-life 2 survivor, scan the subdirectories and find specifically-named folder.
            string[] hl2SurvivorDirs = { "hl2mp_japanese", "hl2_japanese" };
            string dir = Path.GetDirectoryName(p.MainModule.FileName);
            var subdir = new DirectoryInfo(dir).GetDirectories();
            if (dir != null && subdir.Any(di => hl2SurvivorDirs.Contains(di.Name.ToLower())))
                state.GameDir = "survivor";
            else
            {
                p.ReadString(_gameDirPtr, ReadStringType.UTF8, 260, out string absoluteGameDir);
                if (string.IsNullOrWhiteSpace(absoluteGameDir))
                    return false;

                if (!Directory.Exists(absoluteGameDir))
                {
                    Debug.WriteLine($"Bogus absolute game directory: {absoluteGameDir}");
                    try
                    {
                        ManagementClass mngmtClass = new ManagementClass("Win32_Process");
                        foreach (ManagementObject mngmtObject in mngmtClass.GetInstances())
                        {
                            // get the executable path for this object
                            var pathObject = mngmtObject["ExecutablePath"];
                            if (pathObject is null || !(pathObject is string)) 
                                continue;
                            var pathString = (pathObject as string).ToLower();
                            if (pathString !=  p.MainModule.FileName.ToLower())
                                continue;

                            // get the command line parameters for this object
                            var launchParamObject = mngmtObject["CommandLine"];
                            if (launchParamObject is null || !(launchParamObject is string))
                                continue;
                            var launchParam = (launchParamObject as string).ToLower();

                            // we'll need to parse the command line parameters, accounting for quotes
                            var elements = new List<string>();
                            string curElement = "";
                            for (int i = 0; i < launchParam.Length; i++)
                            {
                                if (launchParam[i] == '"')
                                {
                                    i++;
                                    while (i < launchParam.Length && launchParam[i] != '\"')
                                        curElement += launchParam[i++];
                                    elements.Add(curElement);
                                    curElement = "";
                                    continue;
                                }

                                if (launchParam[i] == ' ')
                                {
                                    elements.Add(curElement);
                                    curElement = "";
                                    continue;
                                }

                                curElement += launchParam[i];
                            }
                            elements = elements.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToLower()).ToList();
                            // apart from process name, it must contain at least 1 launch parameter
                            if (elements.Count < 2) 
                                continue;

                            // skip the executable argument, then reverse the list to find the last one, as mods by defualt
                            // sets game to 'steam', and then sets game to their directory down the line.
                            elements = elements.Skip(1).Reverse().ToList();
                            string baseGameFolder = Path.GetDirectoryName(p.MainModule.FileName);
                            for (int i = 0; i < elements.Count(); i++)
                            {
                                if (elements[i] == "-game")
                                {
                                    if (i - 1 < 0) 
                                        continue;

                                    i--;

                                    // -game can be relative to the game's executable
                                    string possible = elements[i];  
                                    if (Directory.Exists(possible) || 
                                        Directory.Exists(possible = Path.Combine(baseGameFolder, possible)))
                                    {
                                        absoluteGameDir = possible;
                                        Debug.WriteLine($"Found absolute game directory through WMI: {possible}");
                                        goto success;
                                    }

                                    i++;
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error while trying to retrieve absolute file path: " + ex.ToString());
                    }

                    // don't return false as we can still function without absolute game directory.
                    Debug.WriteLine("Couldn't retrieve absolute game directory!");
                    success:;
                }

                state.GameDir = new DirectoryInfo(absoluteGameDir).Name.ToLower();
                state.AbsoluteGameDir = absoluteGameDir;
            }

            state.MainSupport = GameSupport.Get(state);
            state.GameEngine = state.MainSupport.GetEngine();

            state.AllSupport.Clear();
            state.AllSupport.Add(state.MainSupport);
            void add(List<GameSupport> list)
            {
                if (list.Count > 0)
                {
                    list.ForEach(x => add(x.AdditionalGameSupport));
                    state.AllSupport.AddRange(list);
                }
            }
            add(state.MainSupport.AdditionalGameSupport);

            return true;
        }

        // TODO: log fails
        bool TryGetGameProcess(out GameState state)
        {
            bool error(string reason)
            {
                Debug.WriteLine($"Couldn't get game process: {reason}");
                return false;
            }

            var sw = Stopwatch.StartNew();

            _state = state = null;

            string[] procs = _targetProccesNames.Select(x => x.ToLower().Replace(".exe", "")).ToArray();
            var p = Process.GetProcesses().FirstOrDefault(x =>
            {
                return
                    Utilities.WinUtils.FindWindow("Valve001", x.MainWindowTitle) != IntPtr.Zero ||
                    // todo: test on edge cases like hl2 survivor to see if this is still needed
                    procs.Contains(x.ProcessName.ToLower()); 
            });
            if (p == null || p.HasExited || SourceSplitUtils.IsVACProtectedProcess(p))
                return false;

            ProcessModuleWow64Safe engine = p.ModulesWow64SafeNoCache().FirstOrDefault(x => x.ModuleName.ToLower() == "engine.dll");
            if (engine == null) return error("Engine hasn't loaded");

            SendGameStatusEvent(true);

            var scanner = new SignatureScanner(p, engine.BaseAddress, engine.ModuleMemorySize);
            _gameDirPtr = scanner.Scan(_gameDirTarget);
            if (_gameDirPtr == IntPtr.Zero)
                return error("Couldn't find game directory path pointer");

            state = new GameState();
            if (!GetGameInfo(state, p))
                return error("Failed to retrieve game information");
            try
            {
                if (!state.GameEngine.Init(p)) return error("Coudln't initialize game engine");
                if (!state.GameEngine.Scan()) return error("Couldn't find one or more necessary values");
            }
            catch (Exception e)
            {
                return error($"Encountered exception while initializing and scanning: {e}");
            }

            state.GameEngine.GameDirPtr = _gameDirPtr;

            bool demoGood = (_demoMonitor.Scan(p, state.GameProcess.ReadString(_gameDirPtr, 260)));
            _gamePausedMidDemoRec = !_demoMonitor.Functional;
            if (!demoGood)
            {
                SourceSplitComponent.Settings.CountDemoInterop.Lock(false);
                SourceSplitComponent.Settings.PrintDemoInfo.Lock(false);
                SourceSplitComponent.Settings.ShowCurDemo.Lock(false);
            }

            bool scanForPtr(ref IntPtr ptr, SigScanTarget target, SignatureScanner scanner, string ptrName, string sigName = " ")
            {
                bool found = (ptr = scanner.Scan(target)) != IntPtr.Zero;
                Debug.WriteLine($"{ptrName} = {(found ? $"0x{ptr.ToString("X")}" : "NOT FOUND")} through sig {sigName}");
                return found;
            }

            #region HOST_RUNFRAME TICK COUNT

            // find the beginning of _host_runframe
            // find string pointer and reference
            SigScanTarget _hrfStringTarg = new SigScanTarget("_Host_RunFrame (top):  _heapchk() != _HEAPOK\n".ConvertToHex() + "00");
            IntPtr _hrfStringPtr = IntPtr.Zero;
            if (!scanForPtr(ref _hrfStringPtr, _hrfStringTarg, scanner, "host_runframe string pointer")
                || !scanForPtr(ref _hrfStringPtr, new SigScanTarget("68" + _hrfStringPtr.GetByteString()), scanner, "host_runframe string reference")
                // then find the target jl of the update loop
                || !scanForPtr(ref _hrfStringPtr,
                    new SigScanTarget("0F 8C ?? ?? ?? FF"),
                    new SignatureScanner(p, _hrfStringPtr, 0x700),
                    "host_runframe target jump"))
                return error("Couldn't find host_runframe target jump");

            // find out where the jl goes to, which should be the top of the update loop
            IntPtr loopTo = _hrfStringPtr + p.ReadValue<int>(_hrfStringPtr + 0x2) + 0x6;

            while ((long)loopTo <= (long)_hrfStringPtr)
            {
                loopTo = loopTo + 1;
                uint candidateHostFrameCount = p.ReadValue<uint>(loopTo);

                if (scanner.IsWithin(candidateHostFrameCount))
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        uint candidateNextPtr = p.ReadValue<uint>(loopTo + 4 + i);
                        if (scanner.IsWithin(candidateNextPtr) && candidateNextPtr - candidateHostFrameCount <= 0x8)
                        {
                            _hostUpdateCountPtr = (IntPtr)candidateHostFrameCount;
                            Debug.WriteLine($"host_runframe host_tickcount ptr is 0x{_hostUpdateCountPtr.ToString("X")}");
                            goto foundTimeHooks;
                        }
                    }
                }

            }
            return error("Can't find host_tickcount pointer");

            foundTimeHooks:
            _hostUpdateCount = new ValueWatcher<long>(state.GameProcess.ReadValue<int>(_hostUpdateCountPtr));
            #endregion

            Debug.WriteLine("TryGetGameProcess took: " + sw.Elapsed);

            _state = state;
            state.MainSupport?.OnGameAttached(_state, TimerActions);
            Debug.WriteLine($"EntInfoSize = {state.GameEngine.EntInfoSize}");

            _settings.InvokeWithTimeout(10000, () =>
            {
                _settings.Invalidate();
                _settings.Update();
            });

            return true;
        }
        void HandleProcess(GameState state, CancellationTokenSource cts)
        {
            var game = state.GameProcess;
            Debug.WriteLine("HandleProcess " + game.ProcessName);

            this.InitGameState(state);
            _gotTickRate = false;

            bool forceExit = false;

            CancellationTokenSource cancelForceExit = new CancellationTokenSource();
            /*
            Thread checkGameProcess = new Thread( new ThreadStart(() => 
            {
                // REALLY make sure the game has exited, as sometimes the splitter stops functioning
                // after game crash for some people
                while (!cancelForceExit.IsCancellationRequested)
                {
                    forceExit = !Process.GetProcesses().Any(x => x.ProcessName == game.ProcessName);
                    if (forceExit && !game.HasExited)
                        Debug.WriteLine("HasExited was wrong!!!!");
                    Thread.Sleep(7500);
                }
            }));
            checkGameProcess.Start();
            */

            var profiler = Stopwatch.StartNew();
            while (!game.HasExited && !forceExit && !cts.IsCancellationRequested)
            {
                // iteration must never take longer than 1 tick
                this.UpdateGameState(state);
                state.AllSupport.ForEach(x => x.OnGenericUpdate(state, TimerActions));
                this.CheckGameState(state);

                state.UpdateCount++;
                TimedTraceListener.Instance.UpdateCount = state.UpdateCount;

                if (profiler.ElapsedMilliseconds >= TARGET_UPDATE_RATE)
                {
                    _timesOver++;
                    _timeOverSpent += (int)profiler.ElapsedMilliseconds - TARGET_UPDATE_RATE;
                    Debug.WriteLine($"**** PERFORAMCE WARNING: update took {profiler.ElapsedMilliseconds - TARGET_UPDATE_RATE}ms too long");
                    Debug.WriteLine($"**** exceeded limit: {_timesOver} times, totalling {_timeOverSpent}ms");
                }

                Thread.Sleep(Math.Max(TARGET_UPDATE_RATE - (int)profiler.ElapsedMilliseconds, 1));
                profiler.Restart();
            }

            cancelForceExit.Cancel();
            forceExit = false;

            // if the game crashed, make sure session ends
            if (state.HostState.Current == HostState.Run)
                this.SendSessionEndedEvent();

            this.SendGameStatusEvent(false);
            this.SendGameAttachedEvent(null);
        }
        void InitGameState(GameState state)
        {
            state.Map.Current = (String.Empty);
            Debug.WriteLine($"running game-specific code for: {state.GameDir} : {state.MainSupport}");
            this.SendSetTimingSpecificsEvent(state.MainSupport.TimingSpecifics);
            this.SendGameAttachedEvent(state.MainSupport.GetType());
        }

        // also works for anything derived from CBaseEntity (player etc) (no multiple inheritance)
        // var must be included by one of the DEFINE_FIELD macros
        /// <summary>
        /// Gets the offset of a member from the base address of the entity's class
        /// </summary>
        /// <param name="member">The name of the member</param>
        /// <param name="game">The game process</param>
        /// <param name="scanner">The scanner which encompasses the module that contains instances of this class</param>
        /// <param name="offset">The offset value</param>
        /// <returns>Whether the operation was successful</returns>
        public static bool GetBaseEntityMemberOffset(string member, Process game, SignatureScanner scanner, out int offset)
        {
            offset = -1;

            IntPtr stringPtr = scanner.Scan(new SigScanTarget(0, Encoding.ASCII.GetBytes(member)));

            if (stringPtr == IntPtr.Zero)
                goto end;

            var target = new SigScanTarget(10,
                $"C7 05 ?? ?? ?? ?? {stringPtr.GetByteString()}"); // mov     dword_15E2BF1C, offset aM_fflags ; "m_fFlags"
            target.OnFound = (proc, s, ptr) => {
                // this instruction is almost always directly after above one, but there are a few cases where it isn't
                // so we have to scan down and find it
                var proximityScanner = new SignatureScanner(proc, ptr, 256);
                return proximityScanner.Scan(new SigScanTarget(6, "C7 05 ?? ?? ?? ?? ?? ?? 00 00"));         // mov     dword_15E2BF20, 0CCh
            };

            IntPtr addr = scanner.Scan(target);
            if (addr == IntPtr.Zero)
            {
                // seen in Black Mesa Source (legacy version)
                var target2 = new SigScanTarget(1,
                 "68 ?? ?? ?? ??",                                  // push    256
                $"68 {stringPtr.GetByteString()}"); // push    offset aM_fflags ; "m_fFlags"
                addr = scanner.Scan(target2);

                if (addr == IntPtr.Zero)
                    goto end;
            }

            offset = game.ReadValue<int>(addr);

            end:

            if (offset == -1)
            {
                Debug.WriteLine($"Couldn't find {member} offset.");
                return false;
            }
            else
            {
                Debug.WriteLine($"{member} offset is 0x{offset.ToString("X")}");
                return true;
            }
        }

        /// <summary>
        /// Gets the offset of a member from the base address of the entity's class
        /// </summary>
        /// <param name="member">The name of the member</param>
        /// <param name="state">The game's state</param>
        /// <param name="module">The module that contains instances of this class</param>
        /// <param name="offset">The offset value</param>
        /// <returns>Whether the operation was successful</returns>
        public static bool GetBaseEntityMemberOffset(string member, GameState state, ProcessModuleWow64Safe module, out int offset)
        {
            Trace.Assert(module != null);
            var scanner = new SignatureScanner(state.GameProcess, module.BaseAddress, module.ModuleMemorySize);

            return GetBaseEntityMemberOffset(member, state.GameProcess, scanner, out offset);
        }

#if DEBUG
        private void DebugPlayerState(GameState state)
        {
            if (state.PlayerFlags.Changed)
            {
                string addedList = String.Empty;
                string removedList = String.Empty;
                foreach (FL flag in Enum.GetValues(typeof(FL)))
                {
                    if (state.PlayerFlags.Current.HasFlag(flag) && !state.PlayerFlags.Old.HasFlag(flag))
                        addedList += Enum.GetName(typeof(FL), flag) + " ";
                    else if (!state.PlayerFlags.Current.HasFlag(flag) && state.PlayerFlags.Old.HasFlag(flag))
                        removedList += Enum.GetName(typeof(FL), flag) + " ";
                }
                if (addedList.Length > 0)
                    Debug.WriteLine("player flags added: " + addedList);
                if (removedList.Length > 0)
                    Debug.WriteLine("player flags removed: " + removedList);
            }

            if (state.PlayerViewEntityIndex.Current != state.PlayerViewEntityIndex.Old)
            {
                Debug.WriteLine("player view entity changed: " + state.PlayerViewEntityIndex.Current);
            }

            if (state.PlayerParentEntityHandle.Current != state.PlayerParentEntityHandle.Old)
            {
                Debug.WriteLine("player parent entity changed: " + state.PlayerParentEntityHandle.Current.ToString("X"));
            }

#if false
            if (!state.PlayerPosition.Current.BitEquals(state.PlayerPosition.Old))
            {
                Debug.WriteLine("player pos changed: " + state.PlayerParentEntityHandle.Current);
            }
#endif
        }
#endif
    }

    public class TimerActionArgs : EventArgs
    {
        public float MillisecondOffset = 0;
        public TimerActionArgs(float msOffset = 0)
        {
            MillisecondOffset = msOffset;
        }
    }

    public class TimerActions
    {
        private SynchronizationContext _uiThread = new SynchronizationContext();

        public TimerActions()
        {
            Init();
        }

        public void Init()
        {
            _uiThread = SynchronizationContext.Current;
        }

        public event EventHandler<TimerActionArgs> OnPlayerGainedControl;
        public event EventHandler<TimerActionArgs> OnPlayerLostControl;
        public event EventHandler<TimerActionArgs> ManualSplit;

        public void Start(float msOffset = 0)
        {
            _uiThread.Post(d => {
                this.OnPlayerGainedControl?.Invoke(this, new TimerActionArgs(msOffset));
            }, null);
        }

        public void End(float msOffset = 0)
        {
            _uiThread.Post(d => {
                this.OnPlayerLostControl?.Invoke(this, new TimerActionArgs(msOffset));
            }, null);
        }

        public void Split(float msOffset = 0)
        {
            _uiThread.Post(d => {
                this.ManualSplit?.Invoke(this, new TimerActionArgs(msOffset));
            }, null);
        }
    }

    public class GameTimingMethod
    {
        public bool EngineTicks = true;
        public bool Pauses = true;
        public bool Disconnects = false;
        public bool Inactive = false;
        public GameTimingMethod(bool engineTicks = true, bool pauses = true, bool disconnects = false, bool inactive = false) 
        {
            EngineTicks = engineTicks;
            Pauses = pauses;
            Disconnects = disconnects;
            Inactive = inactive;
        }
    }

    public enum SignOnState
    {
        None = 0,
        Challenge = 1,
        Connected = 2,
        New = 3,
        PreSpawn = 4,
        Spawn = 5,
        Full = 6,
        ChangeLevel = 7
    }

    // HOSTSTATES
    public enum HostState
    {
        NewGame = 0,
        LoadGame = 1,
        ChangeLevelSP = 2,
        ChangeLevelMP = 3,
        Run = 4,
        GameShutdown = 5,
        Shutdown = 6,
        Restart = 7
    }

    // server_state_t
    public enum ServerState
    {
        Dead,
        Loading,
        Active,
        Paused
    }


    [Flags]
    public enum FL
    {
        //ONGROUND = (1<<0),
        //DUCKING = (1<<1),
        //WATERJUMP = (1<<2),
        ONTRAIN = (1 << 3),
        INRAIN = (1 << 4),
        FROZEN = (1 << 5),
        ATCONTROLS = (1 << 6),
        CLIENT = (1 << 7),
        FAKECLIENT = (1 << 8),
        //INWATER = (1<<9),
        FLY = (1 << 10),
        SWIM = (1 << 11),
        CONVEYOR = (1 << 12),
        NPC = (1 << 13),
        GODMODE = (1 << 14),
        NOTARGET = (1 << 15),
        AIMTARGET = (1 << 16),
        PARTIALGROUND = (1 << 17),
        STATICPROP = (1 << 18),
        GRAPHED = (1 << 19),
        GRENADE = (1 << 20),
        STEPMOVEMENT = (1 << 21),
        DONTTOUCH = (1 << 22),
        BASEVELOCITY = (1 << 23),
        WORLDBRUSH = (1 << 24),
        OBJECT = (1 << 25),
        KILLME = (1 << 26),
        ONFIRE = (1 << 27),
        DISSOLVING = (1 << 28),
        TRANSRAGDOLL = (1 << 29),
        UNBLOCKABLE_BY_PLAYER = (1 << 30)
    }
}

using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Speech.Synthesis;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using WinUtils = LiveSplit.SourceSplit.Utilities.WinUtils;
using System.Threading;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class BMSRetail : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // xen start: when view entity changes back to the player's
        // ending: first tick nihilanth's health is zero
        // earthbound ending: when view entity changes to the ending camera's

        private bool _onceFlag;

        // offsets and binary sizes
        private int _baseEntityHealthOffset = -1;
        private const int _serverModernModuleSize = 0x9D6000;
        private const int _serverModModuleSize = 0x81B000;
        private const int _nihiPhaseCounterOffset = 0x1a6e4;
        private int _nextThinkTickOffset = -1;

        // earthbound start
        private string _ebEndMap = "bm_c3a2i";
        private CustomCommand _ebEndCommand = new CustomCommand("ebend", "0", "Split on Lambda Core teleport");
        private int _ebCamIndex;

        // xen start & run end
        private const string _xenStartMap = "bm_c4a1a";
        private CustomCommand _xenStartCommand = new CustomCommand(
            "xenstart",
            "1",
            "Start upon gaining control in Xen\n\t- 0 disables this function\n\t- 1 starts the timer at 2493 ticks\n\t- 2 starts the timer with no offset.", 
            archived: true);
        private CustomCommand _xenSplitCommand = new CustomCommand("xensplit", "0", "Split upon gaining control in Xen");
        private CustomCommand _nihiSplitCommand = new CustomCommand("nihisplit", "0", "Split per phases of Nihilanth's fight");
        private CustomCommand _susClipTimeLeft = new CustomCommand("susclip_timeleft", "0", "Shows the time left for doing susclip");
        private MemoryWatcher<int> _nihiHP;
        private MemoryWatcher<int> _nihiPhaseCounter;
        private int _xenCamIndex;
        private IntPtr _getGlobalNameFuncPtr = IntPtr.Zero;
        private const int _xenStartTick = 2493; // 38.953125s

        private MemoryWatcher<int> _susNextThink = null;

        private CustomCommandHandler _cmdHandler;

        private BMSMods_HazardCourse _hazardCourse = new BMSMods_HazardCourse();
        private BMSMods_FurtherData _furtherData = new BMSMods_FurtherData();

        public BMSRetail()
        {  
            this.StartOnFirstLoadMaps.Add("bm_c1a0a");
            this.AddFirstMap("bm_c1a0a");
            this.AddLastMap("bm_c4a4a");
             
            this.AdditionalGameSupport.AddRange(new GameSupport[] { _hazardCourse, _furtherData });
            _cmdHandler = new CustomCommandHandler( 
                _xenSplitCommand, 
                _xenStartCommand, 
                _nihiSplitCommand, 
                _ebEndCommand, 
                _susClipTimeLeft);
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            base.OnGameAttached(state, actions);
            _cmdHandler.Init(state);

            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            _getGlobalNameFuncPtr = scanner.Scan(new SigScanTarget("55 8B EC 51 FF 75 ?? 8D 45 ??"));

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));

            if (server.ModuleMemorySize < _serverModernModuleSize)
            {
                _ebEndCommand.BValue = true;
                // for mod, eb's final map name is different
                if (server.ModuleMemorySize <= _serverModModuleSize)
                    _ebEndMap = "bm_c3a2h";
            }

            GameMemory.GetBaseEntityMemberOffset("m_nNextThinkTick", state.GameProcess, scanner, out _nextThinkTickOffset);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            _onceFlag = false;

            string curMap = state.Map.Current.ToLower();

            if (curMap == _ebEndMap)
            {
                _ebCamIndex = state.GameEngine.GetEntIndexByName("locked_in");
            }
            else if (curMap == _xenStartMap)
            {
                _xenCamIndex = state.GameEngine.GetEntIndexByName("stand_viewcontrol");
            }
            else if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                IntPtr nihiPtr = state.GameEngine.GetEntityByName("nihilanth");
                Debug.WriteLine("Nihilanth pointer = 0x" + nihiPtr.ToString("X"));

                _nihiHP = new MemoryWatcher<int>(nihiPtr + _baseEntityHealthOffset);
                _nihiPhaseCounter = new MemoryWatcher<int>(nihiPtr + _nihiPhaseCounterOffset);
            }
            else if (curMap == "bm_c4a3d" && _nextThinkTickOffset != -1)
                _susNextThink = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("first_vent") + _nextThinkTickOffset);
        }

        public void DefaultEnd(string endingname, TimerActions actions)
        {
            _onceFlag = true;
            Debug.WriteLine(endingname);
            actions.End(StartOffsetTicks);
        }

        public override void OnGenericUpdate(GameState state, TimerActions actions)
        {
            base.OnGenericUpdate(state, actions);
            _cmdHandler.Update(state);
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            StartOffsetTicks = EndOffsetTicks = 0;

            if (_onceFlag)
                return;

            if (this.IsLastMap)
            {
                _nihiHP.Update(state.GameProcess);

                if (_nihiHP.Current <= 0 && _nihiHP.Old > 0)
                    DefaultEnd("black mesa end", actions);
                    
                if (_nihiSplitCommand.BValue)
                {
                    _nihiPhaseCounter.Update(state.GameProcess);

                    if (_nihiPhaseCounter.Current - _nihiPhaseCounter.Old == 1 && _nihiPhaseCounter.Old != 0)
                    {
                        Debug.WriteLine("black mesa nihilanth phase " + _nihiPhaseCounter.Old + " end");
                        actions.Split();
                    }
                }
            }
            else if (_ebEndCommand.BValue && state.Map.Current.ToLower() == _ebEndMap)
            {
                if (state.PlayerViewEntityIndex.Current == _ebCamIndex && state.PlayerViewEntityIndex.Old == 1)
                    DefaultEnd("bms eb end", actions);
            }
            else if ((_xenStartCommand.BValue || _xenSplitCommand.BValue) && state.Map.Current.ToLower() == _xenStartMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _xenCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("bms xen start");

                    if (_xenStartCommand.BValue && _getGlobalNameFuncPtr != IntPtr.Zero)
                    {
                        if (state.GameProcess.CallFunctionString("LC2XEN", _getGlobalNameFuncPtr) == uint.MaxValue)
                        {
                            if (_xenStartCommand.IValue == 1) StartOffsetTicks = -_xenStartTick;
                            actions.Start(StartOffsetTicks);
                            return;
                        }
                    }

                    if (_xenSplitCommand.BValue) actions.Split();
                }
            }
            else if (state.Map.Current.ToLower() == "bm_c4a3d")
            {
                if (!_susClipTimeLeft.BValue) 
                    return;

                _susNextThink.Update(state.GameProcess);

                var left = _susNextThink.Current - state.RawTickCount.Current;
                if (left < 0 || !_susNextThink.Changed) 
                    return;

                WinUtils.SendMessage(state.GameProcess,
                $"echo \"sus clip time left: {left} ticks, {left * state.IntervalPerTick:0.000000} seconds\"");
            }

            return;
        }
    }
}
using LiveSplit.ComponentUtil;
using System;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.ComponentHandling;
using System.Collections.Generic;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific
{
    abstract class BMSBase : GameSupport
    {
        public BMSBase()
        {
            SourceSplitComponent.Settings.SLPenalty.Lock(1);
        }
    }

    class BMSRetail : BMSBase
    {
        // start: on map load
        // xen start: when view entity changes back to the player's
        // ending: first tick nihilanth's health is zero
        // earthbound ending: when view entity changes to the ending camera's

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
            "Start upon gaining control in Xen\n\t" +
                "- 0 disables this function\n\t" +
                "- 1 starts the timer at 2493 ticks\n\t" +
                "- 2 starts the timer with no offset.", 
            archived: true);
        private CustomCommand _xenSplitCommand = new CustomCommand("xensplit", "0", "Split upon gaining control in Xen");
        private CustomCommand _nihiSplitCommand = new CustomCommand("nihisplit", "0", "Split per phases of Nihilanth's fight");
        private CustomCommand _susClipTimeLeft = new CustomCommand("susclip_timeleft", "0", "Shows the time left for doing susclip");
        private MemoryWatcher<int> _nihiHP;
        private MemoryWatcher<int> _nihiPhaseCounter;
        private int _xenCamIndex;
        private IntPtr _getGlobalNameFuncPtr = IntPtr.Zero;
        private const float _xenStartMS = 38953.125f; // 38.953125s

        private List<string> _startSaves =
        [
            "6c536d2af38c38eadd9649bc451807ab", // MOD
            "72ae5d4cb2fc818b329774c16d4158be" // 0.9
        ];

        private MemoryWatcher<int> _susNextThink = null;

        private BMSMods.HazardCourse _hazardCourse = new BMSMods.HazardCourse();
        private BMSMods.FurtherData _furtherData = new BMSMods.FurtherData();

        public BMSRetail()
        {  
            this.StartOnFirstLoadMaps.Add("bm_c1a0a");
            this.AddFirstMap("bm_c1a0a");
            this.AddLastMap("bm_c4a4a");
             
            this.AdditionalGameSupport.AddRange(_hazardCourse, _furtherData);
            CommandHandler.Commands.AddRange
            (
                _xenSplitCommand, 
                _xenStartCommand, 
                _nihiSplitCommand, 
                _ebEndCommand, 
                _susClipTimeLeft
            );
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            _getGlobalNameFuncPtr = scanner.Scan(new SigScanTarget("55 8B EC 51 FF 75 ?? 8D 45 ??"));

            GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset);

            if (server.ModuleMemorySize < _serverModernModuleSize)
            {
                _ebEndCommand.Boolean = true;
                // for mod, eb's final map name is different
                if (server.ModuleMemorySize <= _serverModModuleSize)
                    _ebEndMap = "bm_c3a2h";
            }

            GameMemory.GetBaseEntityMemberOffset("m_nNextThinkTick", state.GameProcess, scanner, out _nextThinkTickOffset);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            string curMap = state.Map.Current;

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
                Logging.WriteLine("Nihilanth pointer = 0x" + nihiPtr.ToString("X"));

                _nihiHP = new MemoryWatcher<int>(nihiPtr + _baseEntityHealthOffset);
                _nihiPhaseCounter = new MemoryWatcher<int>(nihiPtr + _nihiPhaseCounterOffset);
            }
            else if (curMap == "bm_c4a3d" && _nextThinkTickOffset != -1)
                _susNextThink = new MemoryWatcher<int>(state.GameEngine.GetEntityByName("first_vent") + _nextThinkTickOffset);
        }

        public void DefaultEnd(string endingname, TimerActions actions)
        {
            OnceFlag = true;
            Logging.WriteLine(endingname);
            actions.End();
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            StartOffsetMilliseconds = EndOffsetMilliseconds = 0;

            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                _nihiHP.Update(state.GameProcess);

                if (_nihiHP.Current <= 0 && _nihiHP.Old > 0)
                    DefaultEnd("black mesa end", actions);
                    
                if (_nihiSplitCommand.Boolean)
                {
                    _nihiPhaseCounter.Update(state.GameProcess);

                    if (_nihiPhaseCounter.Current - _nihiPhaseCounter.Old == 1 && _nihiPhaseCounter.Old != 0)
                    {
                        Logging.WriteLine("black mesa nihilanth phase " + _nihiPhaseCounter.Old + " end");
                        actions.Split();
                    }
                }
            }
            else if (_ebEndCommand.Boolean && state.Map.Current == _ebEndMap)
            {
                if (state.PlayerViewEntityIndex.Current == _ebCamIndex && state.PlayerViewEntityIndex.Old == 1)
                    DefaultEnd("bms eb end", actions);
            }
            else if ((_xenStartCommand.Boolean || _xenSplitCommand.Boolean) && state.Map.Current == _xenStartMap)
            {
                if (state.PlayerViewEntityIndex.Current == 1 && state.PlayerViewEntityIndex.Old == _xenCamIndex)
                {
                    OnceFlag = true;
                    Logging.WriteLine("bms xen start");

                    if (_xenStartCommand.Boolean && _getGlobalNameFuncPtr != IntPtr.Zero)
                    {
                        if (state.GameProcess.CallFunctionString("LC2XEN", _getGlobalNameFuncPtr) == uint.MaxValue)
                        {
                            if (_xenStartCommand.Integer == 1) StartOffsetMilliseconds = -_xenStartMS;
                            actions.Start(StartOffsetMilliseconds);
                            return;
                        }
                    }

                    if (_xenSplitCommand.Boolean) actions.Split();
                }
            }
            else if (state.Map.Current == "bm_c4a3d")
            {
                if (!_susClipTimeLeft.Boolean) 
                    return;

                _susNextThink.Update(state.GameProcess);

                var left = _susNextThink.Current - state.RawTickCount.Current;
                if (left < 0 || !_susNextThink.Changed) 
                    return;

                state.GameProcess.SendMessage($"echo \"sus clip time left: {left} ticks, {left * state.IntervalPerTick:0.000000} seconds\"");
            }

            return;
        }

        protected override void OnSaveLoadedInternal(GameState state, TimerActions actions, string saveName)
        {
            string path = Path.Combine(state.AbsoluteGameDir, "save", saveName + ".sav");
            string md5 = FileUtils.GetMD5(path);

            if (_startSaves.Contains(md5))
            {
                // add 1 tick to timer to account for the skipped 1st tick saveload
                actions.Start(-(state.IntervalPerTick * 1000));
                OnceFlag = true;
                Logging.WriteLine($"black mesa premade save start");
                return;
            }
        }
    }
}
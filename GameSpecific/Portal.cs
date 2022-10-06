using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.GameHandling;
using System.IO;
using System.Security.Cryptography;
using LiveSplit.SourceSplit.ComponentHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalBase : GameSupport
    {
        public PortalBase()
        {
            SourceSplitComponent.Settings.SLPenalty.Lock(1);
            SourceSplitComponent.Settings.CountDemoInterop.Lock(true);
        }
    }

    class Portal : PortalBase
    {
        // how to match this timing with demos:
        // start: 
            // portal: crosshair appear
            // portal tfv map pack: on first map

        private bool _onceFlag;
        private int _baseEntityHealthOffset = -1;
        private const int VAULT_SAVE_TICK = 4261;
        private float _splitTime = 0;
        private float _elevSplitTime = 0;
        private MemoryWatcher<int> _playerHP;

        private List<string> _vaultHashes = new List<string>()
        {
            "8fb11971775314ac2135013d8887f875",
            "b39051d47b23ca9bfbfc19d3366f16f3",
        };

        private CustomCommand _newStart = new CustomCommand("newstart", "0", "Start the timer upon portal open");
        private CustomCommand _elevSplit = new CustomCommand("elevsplit", "0", "Split when the elevator starts moving (limited)");
        private CustomCommand _deathSplit = new CustomCommand("deathsplit", "0", "Death category extension ending");
#if DEBUG
        private CustomCommand _enduranceTesting = new CustomCommand("endurancetesting", "", "Do endurance testing");
#endif
        private CustomCommandHandler _ccHandler;

        public Portal() : base()
        {
            this.AddFirstMap("testchmb_a_00");
            this.AddLastMap("escape_02");        
             
            this.AdditionalGameSupport.Add(new PortalMods_TheFlashVersion());
            _ccHandler = new CustomCommandHandler(
                _newStart, 
                _elevSplit, 
                _deathSplit
#if DEBUG
                , _enduranceTesting
#endif      
                );
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));

            _ccHandler.Init(state);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (IsFirstMap)
                _splitTime = state.GameEngine.GetOutputFireTime("scene_*", "PitchShift", "2.0", 50);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                _splitTime = state.GameEngine.GetOutputFireTime("cable_detach_04", 50);

            if (_elevSplit.BValue)
            {
                _elevSplitTime = state.GameEngine.GetOutputFireTime("*elev_start", 15);
                Debug.WriteLine("Elevator split time is " + _elevSplitTime);
            }

            _playerHP = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset);

            _onceFlag = false;
        }

        public override void OnGenericUpdate(GameState state, TimerActions actions)
        {
            base.OnGenericUpdate(state, actions);
            _ccHandler.Update(state);

        }

        public override void OnSaveLoaded(GameState state, TimerActions actions, string name)
        {
            base.OnSaveLoaded(state, actions, name);

            var path = Path.Combine(state.AbsoluteGameDir, "SAVE", name + ".sav");
            string md5 = FileUtils.GetMD5(path);

            if (_vaultHashes.Contains(md5))
            {
                actions.Start(state.Retick(-3534, 0.015f) + 1);
                _onceFlag = true;
                Debug.WriteLine($"portal vault save start");
                return;
            }
        }

#if DEBUG
        // time testing code.
        string[] _commands = new string[]
        {
            "load trans-00-01.sav",
            "load trans-01-02.sav",
            "load trans-02-03.sav",
            "load trans-03-04.sav",/*
            "load trans-04-05.sav",
            "load trans-05-06.sav",
            "load trans-06-07.sav",
            "load trans-07-08.sav",
            "load trans-08-09.sav",
            "load trans-09-10.sav",
            "load trans-10-11.sav",
            "load trans-11-12.sav",
            "load trans-13-14.sav",
            "load trans-14-15.sav",
            "load trans2.sav",*/
            "map testchmb_a_01.bsp",
            "map testchmb_a_02.bsp",
            "map testchmb_a_03.bsp",
            "map testchmb_a_04.bsp",/*
            "map testchmb_a_05.bsp",
            "map testchmb_a_06.bsp",
            "map testchmb_a_07.bsp",
            "map testchmb_a_08.bsp",
            "map testchmb_a_09.bsp",
            "map testchmb_a_10.bsp",
            "map testchmb_a_11.bsp",
            "map testchmb_a_13.bsp",
            "map testchmb_a_14.bsp",
            "map testchmb_a_15.bsp",
            "load norm1",
            "load norm2",
            "load norm3",
            "load norm4",
            "load norm5",*/
        };
        Random rand = new Random();
#endif

        public override void OnUpdate(GameState state, TimerActions actions)
        {
#if DEBUG
            if (_enduranceTesting.BValue)
            {
                var e = _enduranceTesting.Value.Split('|');
                int wait = rand.Next(int.Parse(e[0]), int.Parse(e[1]));
                if (state.TickCount.Current > wait)
                {
                    Utilities.WinUtils.SendMessage(state.GameProcess, _commands[rand.Next(0, _commands.Length - 1)]);
                }
            }
#endif
            _playerHP.Update(state.GameProcess);

            if (_elevSplit.BValue)
            {
                float splitTime = 0;

                TryMany findSplitTime = new TryMany(
                    () => splitTime == 0,
                    () => splitTime = state.GameEngine.GetOutputFireTime("*elevator_start", 15),
                    () => splitTime = state.GameEngine.GetOutputFireTime("*elevator_door_model_close", 15));
                findSplitTime.Begin();

                try
                {
                    if (splitTime == 0 && _elevSplitTime != 0)
                    {
                        Debug.WriteLine("Elevator began moving!");
                        actions.Split(); return;
                    }
                }
                finally { _elevSplitTime = splitTime; }
            }

            if (IsFirstMap && _deathSplit.BValue)
            {
                if (_playerHP.Old > 0 && _playerHP.Current <= 0)
                {
                    Debug.WriteLine("Death% end");
                    actions.Split(); return;
                }
            }

            if (_onceFlag)
                return;

            if (IsFirstMap)
            {
                bool isInside = state.PlayerPosition.Current.InsideBox(-636, -452, -412, -228, 383, 158);

                if (_newStart.BValue)
                {
                    float splitTime = state.GameEngine.GetOutputFireTime("relay_portal_cancel_room1", 50);
                    try
                    {
                        if (_splitTime != 0 && splitTime == 0 && isInside)
                        {
                            StartOffsetTicks = state.Retick(-3801, 0.015f);
                            Debug.WriteLine("portal portal open start");
                            _onceFlag = true;
                            actions.Start(StartOffsetTicks);
                        }
                    }
                    finally { _splitTime = splitTime; }
                }
                /*else
                {
                    // vault save starts at tick 4261, but update interval may miss it so be a little lenient
                    // player must be somewhere within the vault as well due to new vault skip
                    var saveTick = Util.RecalcTickTime(VAULT_SAVE_TICK, 0.015f, state.IntervalPerTick);
                    if (isInside &&
                        state.TickBase >= saveTick && state.TickBase <= saveTick + 4.RecalcTickTime(0.015f, state.IntervalPerTick))
                    {
                        _onceFlag = true;
                        int ticksSinceVaultSaveTick = state.TickBase - saveTick; // account for missing ticks if update interval missed it
                        StartOffsetTicks = -(3534.RecalcTickTime(0.015f, state.IntervalPerTick) + 1) - ticksSinceVaultSaveTick; // 53.01 seconds + 1 tick
                        actions.Start(StartOffsetTicks); return;
                    }
                }*/

                if (isInside && state.PlayerViewEntityIndex.ChangedTo(1))
                {
                    _onceFlag = true;
                    Debug.WriteLine("portal bed start");
                    actions.Start(); return;
                }
            }
            else if (IsLastMap)
            {
                var splitTime = state.GameEngine.GetOutputFireTime("cable_detach_04", 50);
                if (splitTime > 0 && _splitTime == 0)
                {
                    Debug.WriteLine("portal delayed end");
                    _onceFlag = true;
                    actions.End(-1); // -1 for unknown reasons
                }
                _splitTime = splitTime;
            }

            return;
        }
    }
}
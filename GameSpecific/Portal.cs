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

        private int _baseEntityHealthOffset = -1;
        private const int VAULT_SAVE_TICK = 4261;
        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();
        private float _elevSplitTime = 0;
        private MemoryWatcher<int> _playerHP;

        private List<string> _vaultHashes = new List<string>()
        {
            "8fb11971775314ac2135013d8887f875",
            "b39051d47b23ca9bfbfc19d3366f16f3",
            "6a4ff6f22deebb0c095218ace1a9ea19"
        };

        private CustomCommand _newStart = new CustomCommand("newstart", "0", "Start the timer upon portal open");
        private CustomCommand _elevSplit = new CustomCommand("elevsplit", "0", "Split when the elevator starts moving (limited)");
        private CustomCommand _deathSplit = new CustomCommand("deathsplit", "0", "Death category extension ending");
#if DEBUG
        private CustomCommand _enduranceTesting = new CustomCommand("endurancetesting", "", "Do endurance testing");
#endif

        public Portal() : base()
        {
            this.AddFirstMap("testchmb_a_00");
            this.AddLastMap("escape_02");        
             
            this.AdditionalGameSupport.Add(new PortalMods.TheFlashVersion());
            CommandHandler.Commands.AddRange
            (
                _newStart, 
                _elevSplit, 
                _deathSplit
#if DEBUG
                , _enduranceTesting
#endif      
            );
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));

        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("scene_*", "PitchShift", "2.0");

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("cable_detach_04");

            if (_elevSplit.Boolean)
            {
                _elevSplitTime = state.GameEngine.GetOutputFireTime("*elev_start");
                Debug.WriteLine("Elevator split time is " + _elevSplitTime);
            }

            _playerHP = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset);
        }

        protected override void OnSaveLoadedInternal(GameState state, TimerActions actions, string name)
        {
            if (_newStart.Boolean) return;

            var path = Path.Combine(state.AbsoluteGameDir, "SAVE", name + ".sav");
            string md5 = FileUtils.GetMD5(path);

            if (_vaultHashes.Contains(md5))
            {
                actions.Start(-(53010 + 15));
                OnceFlag = true;
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

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
#if DEBUG
            if (_enduranceTesting.Boolean)
            {
                var e = _enduranceTesting.String.Split('|');
                int wait = rand.Next(int.Parse(e[0]), int.Parse(e[1]));
                if (state.TickCount.Current > wait)
                {
                    state.GameProcess.SendMessage(_commands[rand.Next(0, _commands.Length - 1)]);
                }
            }
#endif
            _playerHP.Update(state.GameProcess);

            if (_elevSplit.Boolean)
            {
                float splitTime = 0;

                TryMany findSplitTime = new TryMany(
                    () => splitTime == 0,
                    () => splitTime = state.GameEngine.GetOutputFireTime("*elevator_start"),
                    () => splitTime = state.GameEngine.GetOutputFireTime("*elevator_door_model_close"));
                findSplitTime.Begin();

                if (splitTime == 0 && _elevSplitTime != 0)
                {
                    Debug.WriteLine("Elevator began moving!");
                    actions.Split();
                }
                _elevSplitTime = splitTime;
            }

            if (IsFirstMap && _deathSplit.Boolean)
            {
                if (_playerHP.Old > 0 && _playerHP.Current <= 0)
                {
                    Debug.WriteLine("Death% end");
                    actions.Split();
                }
            }

            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                bool isInside = state.PlayerPosition.Current.InsideBox(-636, -452, -412, -228, 383, 158);

                if (_newStart.Boolean)
                {
                    _splitTime.Current = state.GameEngine.GetOutputFireTime("relay_portal_cancel_room1");
                    if (_splitTime.ChangedTo(0) && isInside)
                    {
                        Debug.WriteLine("portal portal open start");
                        OnceFlag = true;
                        actions.Start(-57045);
                    }
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
                    OnceFlag = true;
                    Debug.WriteLine("portal bed start");
                    actions.Start(); 
                    return;
                }
            }
            else if (IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("cable_detach_04");
                if (_splitTime.ChangedFrom(0))
                {
                    Debug.WriteLine("portal delayed end");
                    OnceFlag = true;
                    actions.End(-state.IntervalPerTick * 1000); // -1 for unknown reasons
                }
            }

            return;
        }
    }
}
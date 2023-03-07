using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.GameHandling;
using System.IO;
using System.Security.Cryptography;
using LiveSplit.SourceSplit.ComponentHandling;
using System.Drawing.Printing;
using System.Linq;
using System.CodeDom;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

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
        private MemoryWatcher<int> _playerHP;

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        private struct Elevator
        {
            public MemoryWatcher<Vector3f> Speed;
            public MemoryWatcher<Vector3f> Position;
            public void Update(Process proc)
            {
                Speed.Update(proc);
                Position.Update(proc);
            }
        }
        private struct Blocker
        {
            public IntPtr EntityPointer;
            public int Chamber;
        }
        private readonly Dictionary<int, int[]> _chamberMaps = new Dictionary<int, int[]>()
        {
            { 00, new int[] { 00, 01 } },
            { 01, new int[] { 02, 03 } },
            { 02, new int[] { 04, 05 } },
            { 03, new int[] { 06, 07 } },
            { 04, new int[] { 08 } },
            { 05, new int[] { 09 } },
            { 06, new int[] { 10 } },
            { 07, new int[] { 11, 12 } },
            { 08, new int[] { 13 } },
            { 09, new int[] { 14 } },
            { 10, new int[] { 15 } },
            { 11, new int[] { 16 } },
            { 13, new int[] { 17 } },
            { 14, new int[] { 18 } },
            { 15, new int[] { 19 } },
        };
        private int _baseModelNameOffset = -1;
        private int _baseVelocityOffset = -1;
        private int _baseSolidFlagsOffset = -1;
        private List<Elevator> _elevatorSpeeds = new List<Elevator>();
        private List<Blocker> _elevatorBlockers = new List<Blocker>();

        private List<string> _vaultHashes = new List<string>()
        {
            "8fb11971775314ac2135013d8887f875",
            "b39051d47b23ca9bfbfc19d3366f16f3",
            "6a4ff6f22deebb0c095218ace1a9ea19"
        };

        private CustomCommand _newStart = new CustomCommand("newstart", "0", "Start the timer upon portal open");
        private CustomCommand _elevSplit = new CustomCommand("elevsplit", "0", "Split when an elevator starts moving from the ends of the shaft. Also check out the SourceSplit command 'elevsplit_on'.");
        private CustomCommand _elevSplitWhitelist = new CustomCommand
        (
            "elevsplit_only",
            "*",
@"This option is only effective when Elevator Splitting is enabled. 
ONLY when an elevator is at the end of one of the specified chambers, and it begins moving from the ends of its shaft, an Auto-Split will be triggered. Entries are separated by a comma.
For example: 
- Entering '1' will only Auto-Split when the elevator at the end of chamber 01 begins moving from the ends of its shaft.
- Entering '1,2,03' will only Auto-Split when the elevators at the end of chamber 01, 02, and 03 begins moving from the ends of their shafts.
- Entering '*' will Auto-Split on any valid elevator movement."
        );
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
                _elevSplitWhitelist,
                _deathSplit
#if DEBUG
                , _enduranceTesting
#endif      
            );

            _deathSplit.Callback = (s) =>
            {
                if (_deathSplit.Boolean)
                    CommandHandler.SendConsoleMessage("Please reload a save to enable Death Splitting.");
            };
            _elevSplit.Callback = (s) =>
            {
                if (_elevSplit.Boolean)
                    CommandHandler.SendConsoleMessage("Please reload a save to enable Elevator Splitting.");
            };
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            GameMemory.GetBaseEntityMemberOffset("m_iHealth", state, state.GameEngine.ServerModule, out _baseEntityHealthOffset);
            GameMemory.GetBaseEntityMemberOffset("m_ModelName", state, state.GameEngine.ServerModule, out _baseModelNameOffset);
            GameMemory.GetBaseEntityMemberOffset("m_vecAbsVelocity", state, state.GameEngine.ServerModule, out _baseVelocityOffset);

            // find brush solidity flag offset (for elevator splitting)
            {
                if (!GameMemory.GetBaseEntityMemberOffset("m_Collision\0", state, state.GameEngine.ServerModule, out var collisionPropOffset)) goto skip;

                var scanner = new SignatureScanner(state.GameProcess, state.GameEngine.ServerModule.BaseAddress, state.GameEngine.ServerModule.ModuleMemorySize);
                
                var strPtr = scanner.Scan(new SigScanTarget(Encoding.UTF8.GetBytes("m_usSolidFlags\0")));
                if (strPtr == IntPtr.Zero) goto skip;

                var strRef = scanner.Scan(new SigScanTarget(1, $"6a ?? 68 {strPtr.GetByteString()} 68 ?? ?? ?? ?? e8 ?? ?? ?? 00"));
                if (strRef == IntPtr.Zero) goto skip;

                _baseSolidFlagsOffset = collisionPropOffset + state.GameProcess.ReadValue<byte>(strRef);
                Debug.WriteLine("Found CCollisionProperty::m_usSolidFlags offset = 0x" + _baseSolidFlagsOffset.ToString("X"));

                skip:;
            }
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            _elevatorSpeeds.Clear();
            _elevatorBlockers.Clear();

            if (_elevSplit.Boolean && _baseModelNameOffset != -1 && _baseVelocityOffset != -1 && _baseSolidFlagsOffset != -1 && _baseSolidFlagsOffset != -1)
            {
                var engine = state.GameEngine;
                var proc = state.GameProcess;

                foreach (var ent in engine.GetEntities())
                {
                    // is this an elevator blocker? 
                    {
                        if (!state.GameProcess.ReadString(proc.ReadPointer(ent + state.GameEngine.BaseEntityTargetNameOffset), 255, out string targetName)) goto skip;
                        if (!targetName.Contains("block_crazy_player")) goto skip;

                        Debug.WriteLine($"Found potential blocking entity @ 0x{ent.ToString("X")}");

                        List<string> names = new List<string>();
                        foreach (var close in engine.GetEntitiesByPos(engine.GetEntityPos(ent), 100))
                        {
                            var name = state.GameProcess.ReadString(proc.ReadPointer(close + state.GameEngine.BaseEntityTargetNameOffset), 255);
                            if (string.IsNullOrWhiteSpace(name)) continue;

                            Debug.WriteLine("\t near: " + name);
                            names.Add(name);
                        }

                        int chamber = -1;
                        var curChamberMatch = Regex.Match(state.Map.Current, @"^testchmb_a_([0-9]+)");
                        if (curChamberMatch.Success)
                        {
                            var curChamber = int.Parse(curChamberMatch.Groups[1].Value);
                            // only 1 chamber...
                            if (_chamberMaps.ContainsKey(curChamber) && _chamberMaps[curChamber].Length == 1)
                            {
                                chamber = _chamberMaps[curChamber][0];
                                goto skip_blocker_owner;
                            }
                        }
                        var processedNames = names
                            .Select(x => (name: x, match: Regex.Match(x, "^a([0-9]{2})(?:-|_)(?:a([0-9]{2}))?")))
                            .Where(x => x.match.Success)
                            .Select(x => 
                            (
                                name: x.name,
                                match: x.match,
                                map: int.Parse(x.match.Groups[1].Value)
                            ))
                            .Where(x => _chamberMaps.ContainsKey(x.map));
                        if (processedNames.Any())
                        {
                            if (!processedNames.Any(x =>
                            {
                                if (x.match.Groups[2].Success)
                                {
                                    chamber = _chamberMaps[x.map].Last();
                                    return true;
                                }
                                return false;
                            }))
                            {
                                chamber = _chamberMaps[processedNames.First().map][0];
                            }
                        }

                        skip_blocker_owner:

                        Debug.WriteLine($"Blocker belongs to chamber {chamber}");
                        _elevatorBlockers.Add(new Blocker()
                        {
                            Chamber = chamber,
                            EntityPointer = ent
                        });

                        continue;

                        skip:;     
                    }

                    // is this an elevator body?
                    {
                        if (!state.GameProcess.ReadString(proc.ReadPointer(ent + _baseModelNameOffset), 255, out string modelName)) goto skip;
                        if (modelName is null) continue;
                        if (!modelName.ToLower().EndsWith("round_elevator_body.mdl")) goto skip;

                        var parent = engine.GetEntityByIndex(engine.GetEntIndexFromHandle(proc.ReadValue<uint>(ent + engine.BaseEntityParentHandleOffset)));
                        if (parent == IntPtr.Zero) goto skip;

                        Debug.WriteLine($"Found potential elevator @ 0x{ent.ToString("X")}, parent @ 0x{parent.ToString("X")}");
                        var w = new Elevator()
                        {
                            Speed = new MemoryWatcher<Vector3f>(parent + _baseVelocityOffset),
                            Position = new MemoryWatcher<Vector3f>(ent + engine.BaseEntityAbsOriginOffset)
                        };
                        _elevatorSpeeds.Add(w);
                        w.Update(proc);
                        continue;

                        skip:;
                    }
                }
            }

            if (IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("scene_*", "PitchShift", "2.0");

                if (_deathSplit.Boolean)
                    _playerHP = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset);
            }

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("cable_detach_04");
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

            if (_elevSplit.Boolean)
            {
                bool splitAlready = false;
                foreach (var elevator in _elevatorSpeeds)
                {
                    elevator.Update(state.GameProcess);
                    var pos = elevator.Position.Current;
                    if (!splitAlready && elevator.Speed.Old.Z == 0 && elevator.Speed.Current.Z != 0)
                    {
                        foreach (var blocker in _elevatorBlockers)
                        {
                            var blockerPos = state.GameEngine.GetEntityPos(blocker.EntityPointer);
                            if (blockerPos.Distance(pos) > 200f) continue;

                            // is the blocker enabled? (i.e is it solid)
                            if ((state.GameProcess.ReadValue<ushort>(blocker.EntityPointer + _baseSolidFlagsOffset) & 0x0004) != 0) continue;
                            
                            // this is the correct elevator that the player's going on, don't bother doing this check again...
                            splitAlready = true;

                            // is this a correct chamber?
                            string whitelist = _elevSplitWhitelist.String;
                            if (whitelist != "*" && 
                                !whitelist.Split(',').Any(x => int.TryParse(x.ToString().Trim(), out int chamber) && blocker.Chamber == chamber))
                            {
                                Debug.WriteLine($"Ignoring elevator split due to incorrect chamber (current: {blocker.Chamber}, target: {whitelist})");
                                splitAlready = true;
                                continue;
                            }

                            Debug.WriteLine($"Elevator split (@ {pos})");
                            actions.Split();
                        }
                    }
                }
            }

            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                if (_deathSplit.Boolean && _playerHP != null)
                {
                    _playerHP.Update(state.GameProcess);

                    if (_playerHP.Old > 0 && _playerHP.Current <= 0)
                    {
                        Debug.WriteLine("Death% end");
                        actions.Split();
                    }
                }

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
/*
 *  this file contains games / mods which follow similar patterns for auto start and stopping:
 *  auto start on loading first map, and auto ending when disconnecting or when an output is queued or fired
 */

using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;
using System.Runtime.CompilerServices;
using LiveSplit.SourceSplit.Utilities;
using System.Security.Cryptography;
using static LiveSplit.SourceSplit.ComponentHandling.SourceSplitComponent;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class FirstMapAutoStart : GameSupport
    {
        public FirstMapAutoStart(string firstMap)
        {
            AddFirstMap(firstMap.ToLower());
            StartOnFirstLoadMaps.AddRange(firstMap.ToLower());
        }
    }

    class OutputBasedAutoEnd : FirstMapAutoStart
    {
        protected string TargetName;
        protected string Command = null;
        protected string Param = null;

        protected ValueWatcher<float> SplitTime = new ValueWatcher<float>();

        public OutputBasedAutoEnd(string firstMap, string lastMap) : base(firstMap)
        {
            AddLastMap(lastMap.ToLower());
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap)
            {
                SplitTime.Current = state.GameEngine.GetOutputFireTime(TargetName, Command, Param);
            }

            base.OnSessionStartInternal(state, actions);
        }
    }

    class DisconnectAutoEnd : OutputBasedAutoEnd
    {
        public DisconnectAutoEnd(string firstMap, string lastMap) : base(firstMap, lastMap)
        {
        }

        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap && state.HostState.Current == HostState.GameShutdown)
                OnUpdateInternal(state, actions);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime(TargetName, Command, Param);
                SplitTime.Current = (splitTime == 0f) ? SplitTime.Current : splitTime;
                if (state.CompareToInternalTimer(SplitTime.Current, GameState.IO_EPSILON, false, true))
                {
                    OnceFlag = true;
                    Debug.WriteLine($"{this.GetType().Name} end");
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class OutputQueuedAutoEnd : OutputBasedAutoEnd
    {
        public OutputQueuedAutoEnd(string firstMap, string lastMap) : base(firstMap, lastMap)
        {
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                SplitTime.Current = state.GameEngine.GetOutputFireTime(TargetName, Command, Param);
                if (SplitTime.ChangedFrom(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine($"{this.GetType().Name} end");
                    actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class OutputFiredAutoEnd : OutputBasedAutoEnd
    {
        public OutputFiredAutoEnd(string firstMap, string lastMap) : base(firstMap, lastMap)
        {
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                SplitTime.Current = state.GameEngine.GetOutputFireTime(TargetName, Command, Param);
                if (SplitTime.ChangedTo(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine($"{this.GetType().Name} end");
                    actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class ThinkTank : DisconnectAutoEnd
    {
        public ThinkTank() : base("ml04_ascend", "ml04_crown_bonus")
        {
            TargetName = "servercommand";
        }
    }

    class Gnome : DisconnectAutoEnd
    {
        public Gnome() : base("at03_findthegnome", "at03_nev_no_gnomes_land")
        {
            TargetName = "cmd_end";
        }
    }

    class BackwardsMod : FirstMapAutoStart
    {
        public BackwardsMod() : base("backward_d3_breen_01") { }
    }

    class Reject : DisconnectAutoEnd
    {
        public Reject() : base("reject", "reject")
        {
            TargetName = "komenda";
        }
    }

    class TrapVille : DisconnectAutoEnd
    {
        public TrapVille() : base("aquickdrivethrough_thc16c4", "makeearthgreatagain_thc16c4")
        {
            TargetName = "game_end";
        }

        private Vector3f _endSector = new Vector3f(7953f, -11413f, 2515f);

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            // todo: probably should use the helicopter's position?
            if (IsLastMap && state.PlayerPosition.Current.Distance(_endSector) <= 300f)
            {
                base.OnUpdateInternal(state, actions);
            }
            return;
        }
    }

    class RTSLVille : DisconnectAutoEnd
    {
        public RTSLVille() : base("from_ashes_map1_rtslv", "terminal_rtslv")
        {
            TargetName = "clientcommand";
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap && state.PlayerViewEntityIndex.Current != GameState.ENT_INDEX_PLAYER)
            {
                base.OnUpdateInternal(state, actions);
            }
            return;
        }
    }

    class Abridged : DisconnectAutoEnd
    {
        public Abridged() : base("ml05_training_facilitea", "ml05_shortcut17")
        {
            TargetName = "end_disconnect";
            Command = "command";
            Param = "disconnect; map_background background_ml05";
        }
    }

    class EpisodeOne : DisconnectAutoEnd
    {
        public EpisodeOne() : base("direwolf", "outland_resistance")
        {
            TargetName = "point_clientcommand2";
        }
    }

    class CombinationVille : DisconnectAutoEnd
    {
        public CombinationVille() : base("canal_flight_ppmc_cv", "cvbonus_ppmc_cv")
        {
            TargetName = "pcc";
            Command = "command";
            Param = "startupmenu force";
        }

        private Vector3f _tramEndPos = new Vector3f(2624f, -1856f, 250f);
        private int _tramPtr;

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap)
                _tramPtr = state.GameEngine.GetEntIndexByName("tram");

            base.OnSessionStartInternal(state, actions);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap && state.GameEngine.GetEntityPos(_tramPtr).Distance(_tramEndPos) <= 100)
            {
                base.OnUpdateInternal(state, actions);
            }
            return;
        }
    }

    class PhaseVille : DisconnectAutoEnd
    {
        public PhaseVille() : base("rtsl_mlc", "hospitalisation_tlc18_c4")
        {
            TargetName = "clientcommand";
        }
    }

    class CompanionPiece : DisconnectAutoEnd
    {
        public CompanionPiece() : base("tg_wrd_carnival", "maplab_jan_cp")
        {
            TargetName = "piss_off_egg_head";
        }
    }

    class TheCitizen : FirstMapAutoStart
    {
        public TheCitizen() : base("TheCitizen_part1") { }
    }

    class DarkIntervention : DisconnectAutoEnd
    {
        public DarkIntervention() : base("dark_intervention", "dark_intervention")
        {
            TargetName = "command_ending";
        }
    }

    class HellsMines : OutputQueuedAutoEnd
    {
        public HellsMines() : base("hells_mines", "hells_mines")
        {
            TargetName = "command";
        }
    }

    class UpmineStruggle : OutputQueuedAutoEnd
    {
        public UpmineStruggle() : base("twhl_upmine_struggle", "twhl_upmine_struggle")
        {
            TargetName = "no_vo";
        }
    }

    class Offshore : DisconnectAutoEnd
    {
        public Offshore() : base("islandescape", "islandcitytrain")
        {
            TargetName = "launchQuit";
        }
    }

    class VeryHardMod : DisconnectAutoEnd
    {
        public VeryHardMod() : base("vhm_chapter", "vhm_chapter")
        {
            TargetName = "end_game";
        }
    }

    class CloneMachine : DisconnectAutoEnd
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public CloneMachine() : base("the2", "the5")
        {
            TargetName = "cmd";
            Command = "command";
            Param = "disconnect";
        }
    }

    class JollysHardcoreMod : DisconnectAutoEnd
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public JollysHardcoreMod() : base("hardcore_01", "hardcore_01")
        {
            TargetName = "clientcommand";
            Command = "Command";
            Param = "disconnect; startupmenu";
        }
    }

    class The72SecondExperiment : DisconnectAutoEnd
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public The72SecondExperiment() : base("prison_break_72s-emc", "INEVITABLE_72s-emc")
        {
            TargetName = "pointcc";
            Command = "Command";
            Param = "disconnect";
        }
    }
}

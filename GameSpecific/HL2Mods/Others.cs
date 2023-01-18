// what is this?
// to cut down on the number of files included in this project, this file was created
// the mods included in this file have either no or similar splitting behavior (start on map load, end on game disconnect)

// mods included: think tank, gnome, hl2 backwards mod, hl2 reject, trapville, rtslville, 
// hl abridged, episode one, combination ville, phaseville, companion piece, school adventures, the citizen 1
// hells mines, dark intervention, upmine struggle, offshore, very hard mod, clone machine

using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Misc : GameSupport
    {
        protected float _splitTime;

        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap && state.HostState.Current == HostState.GameShutdown)
                OnUpdateInternal(state, actions);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            _splitTime = 0f;
        }
    }

    class ThinkTank : Misc
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        public ThinkTank()
        {
            this.AddFirstMap("ml04_ascend");
            this.AddLastMap("ml04_crown_bonus");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("servercommand");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    OnceFlag = true;
                    Debug.WriteLine("think tank end");
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class Gnome : Misc
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        public Gnome()
        {
            
            this.AddFirstMap("at03_findthegnome");
            this.AddLastMap("at03_nev_no_gnomes_land");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("cmd_end");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("gnome end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class BackwardsMod : GameSupport
    {
        // start: on first map
        public BackwardsMod()
        {
            this.AddFirstMap("backward_d3_breen_01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }
    }

    class Reject : Misc
    {
        // start: on first map
        // end: on final output
        public Reject()
        {
            this.StartOnFirstLoadMaps.Add("reject");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            float splitTime = state.GameEngine.GetOutputFireTime("komenda");
            _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
            if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
            {
                Debug.WriteLine("hl2 reject end");
                OnceFlag = true;
                state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
            }
            return;
        }
    }

    class TrapVille : Misc
    {
        // start: on first map
        // end: on final output
        public TrapVille()
        {
            this.AddFirstMap("aquickdrivethrough_thc16c4");
            this.AddLastMap("makeearthgreatagain_thc16c4");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }


        private Vector3f _endSector = new Vector3f(7953f, -11413f, 2515f);

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            // todo: probably should use the helicopter's position?
            if (IsLastMap && state.PlayerPosition.Current.Distance(_endSector) <= 300f)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("game_end");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("trapville end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class RTSLVille : Misc
    {
        // start: on first map
        // end: on final output
        public RTSLVille()
        {
            this.AddFirstMap("from_ashes_map1_rtslv");
            this.AddLastMap("terminal_rtslv");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap && state.PlayerViewEntityIndex.Current != GameState.ENT_INDEX_PLAYER)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("clientcommand");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("rtslville end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class Abridged : Misc
    {
        // start: on first map
        // end: on final output
        public Abridged()
        {
            this.AddFirstMap("ml05_training_facilitea");
            this.AddLastMap("ml05_shortcut17");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("end_disconnect", "command", "disconnect; map_background background_ml05");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("hl abridged end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class EpisodeOne : Misc
    {
        // start: on first map
        // end: on final output
        public EpisodeOne()
        {
            this.AddFirstMap("direwolf");
            this.AddLastMap("outland_resistance");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("point_clientcommand2");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("episode one end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class CombinationVille : Misc
    {
        // start: on first map
        // end: on final output
        public CombinationVille()
        {
            this.AddFirstMap("canal_flight_ppmc_cv");
            this.AddLastMap("cvbonus_ppmc_cv");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        private Vector3f _tramEndPos = new Vector3f(2624f, -1856f, 250f);
        private int _tramPtr;

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap)
                _tramPtr = state.GameEngine.GetEntIndexByName("tram");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap && state.GameEngine.GetEntityPos(_tramPtr).Distance(_tramEndPos) <= 100)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("pcc", "command", "startupmenu force");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("combination ville end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class PhaseVille : Misc
    {
        // start: on first map
        // end: on final output
        public PhaseVille()
        {
            this.AddFirstMap("rtsl_mlc");
            this.AddLastMap("hospitalisation_tlc18_c4");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("clientcommand");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("phaseville end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class CompanionPiece : Misc
    {
        // start: on first map
        // end: on final output
        public CompanionPiece()
        {
            this.AddFirstMap("tg_wrd_carnival");
            this.AddLastMap("maplab_jan_cp");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("piss_off_egg_head");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("companion piece end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class TheCitizen : GameSupport
    {
        // start: on first map
        public TheCitizen()
        {
            this.AddFirstMap("TheCitizen_part1");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }
    }

    class SchoolAdventures : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public SchoolAdventures()
        {
            this.AddFirstMap("sa_01");
            this.AddLastMap("sa_04");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
             
        }

        private int _endCameraIndex = -1;

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap)
            {
                _endCameraIndex = state.GameEngine.GetEntIndexByName("viewcontrol_credits");
                //Debug.WriteLine($"Found end camera index at {_endCameraIndex}");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap && _endCameraIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Old == 1 &&
                    state.PlayerViewEntityIndex.Current == _endCameraIndex)
                {
                    OnceFlag = true;
                    Debug.WriteLine("school_adventures end");
                    actions.End(EndOffsetMilliseconds); return;
                }
            }
            return;
        }
    }

    class DarkIntervention : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public DarkIntervention()
        {
            this.AddFirstMap("dark_intervention");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("command_ending");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    Debug.WriteLine("dark intervention end");
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class HellsMines : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public HellsMines()
        {
            this.AddFirstMap("hells_mines");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            _splitTime = state.GameEngine.GetOutputFireTime("command");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("command");
                if (splitTime != 0 && _splitTime == 0)
                {
                    Debug.WriteLine("hells mines end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
                _splitTime = splitTime;
            }
            return;
        }
    }

    class UpmineStruggle : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public UpmineStruggle()
        {
            this.AddFirstMap("twhl_upmine_struggle");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("no_vo");
                if (_splitTime == 0 && splitTime != 0)
                {
                    Debug.WriteLine("upmine struggle end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
                _splitTime = splitTime;
            }
            return;
        }
    }

    class Offshore : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public Offshore()
        {
            this.AddFirstMap("islandescape");
            this.AddLastMap("islandcitytrain");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("launchQuit");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class VeryHardMod : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public VeryHardMod()
        {
            this.AddFirstMap("vhm_chapter");
            this.AddLastMap("vhm_chapter");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("end_game");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class CloneMachine : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public CloneMachine()
        {
            this.AddFirstMap("the2");
            this.AddLastMap("the5");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("cmd", "Command", "disconnect");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }

    class JollysHardcoreMod : Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public JollysHardcoreMod()
        {
            this.AddFirstMap("hardcore_01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                float splitTime = state.GameEngine.GetOutputFireTime("clientcommand", "Command", "disconnect; startupmenu");
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, GameState.IO_EPSILON, false, true))
                {
                    OnceFlag = true;
                    state.QueueOnNextSessionEnd = () => actions.End(EndOffsetMilliseconds);
                }
            }
            return;
        }
    }
}

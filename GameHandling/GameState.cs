using System;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.Utilities;
using System.Collections.Generic;


namespace LiveSplit.SourceSplit.GameHandling
{
    // change back to struct if we ever need to give a copy of the state
    // to the ui thread

    /// <summary>
    /// Class that holds the state of some game values
    /// </summary>
    class GameState
    {
        public const int ENT_INDEX_PLAYER = 1;
        public const float IO_EPSILON = 0.05f; // precision of about 4 ticks, could be lowered?

        public Process GameProcess 
        {
            get { return GameEngine.GameProcess; } 
            set { GameEngine.GameProcess = value; } 
        }

        public ValueWatcher<HostState> HostState = new ValueWatcher<HostState>(GameHandling.HostState.NewGame);
        public ValueWatcher<SignOnState> SignOnState = new ValueWatcher<SignOnState>(GameHandling.SignOnState.None);
        public ValueWatcher<ServerState> ServerState = new ValueWatcher<ServerState>(GameHandling.ServerState.Dead);

        public ValueWatcher<string> Map = new ValueWatcher<string>(String.Empty);
        public string GameDir;
        public string AbsoluteGameDir;

        public float IntervalPerTick;
        public float FrameTime;
        public int TickBase;
        public ValueWatcher<int> RawTickCount = new ValueWatcher<int>(0);
        public ValueWatcher<int> TickCount = new ValueWatcher<int>(0);
        public float TickTime;

        public ValueWatcher<FL> PlayerFlags = new ValueWatcher<FL>(new FL());
        public ValueWatcher<Vector3f> PlayerPosition = new ValueWatcher<Vector3f>(new Vector3f());
        public ValueWatcher<int> PlayerViewEntityIndex = new ValueWatcher<int>(0);
        public ValueWatcher<int> PlayerParentEntityHandle = new ValueWatcher<int>(0);
        public CEntInfoV2 PlayerEntInfo;

        public GameEngine GameEngine;
        public GameSupport MainSupport;
        public List<GameSupport> AllSupport = new List<GameSupport>();
        public int UpdateCount;

        /// <summary>
        /// Timer behavior on the next session end (game disconnect / map change)
        /// </summary>
        public Action QueueOnNextSessionEnd = null;

        public GameState() {; }

        // fixme: this *could* probably return true twice if the player save/loads on an exact tick
        // precision notice: will always be too early by at most 2 ticks using the standard 0.03 epsilon
        /// <summary>
        /// Compares the inputted time to the internal timer.
        /// </summary>
        /// <param name="time">The time to compare with internal time</param>
        /// <param name="epsilon">The maximum allowed distance between inputted time and internal time</param>
        /// <param name="checkBefore">Whether to check if the internal timer has just gone past inputted time</param>
        /// <param name="adjustFrameTime">Whether to account for frametime (lagginess / alt-tabbing)</param>
        /// <returns>Whether the inputted time is past the internal timer</returns>
        public bool CompareToInternalTimer(float time, float epsilon = IO_EPSILON, bool checkBefore = false, bool adjustFrameTime = false)
        {
            if (time == 0f) return false;

            // adjust for lagginess for example at very low fps or if the game's alt-tabbed
            // could be exploitable but not enough to be concerning
            // max frametime without cheats should be 0.05, so leniency is ~<3 ticks
            time -= adjustFrameTime ? (FrameTime > IntervalPerTick ? FrameTime / 1.15f : 0f) : 0f;

            float curRawTime = RawTickCount.Current * IntervalPerTick;
            float oldRawTime = RawTickCount.Old * IntervalPerTick;
            //Debug.WriteLine($"{curRawTime} {oldRawTime} {splitTime} {epsilon}");

            if (epsilon == 0f)
            {
                return curRawTime >= time &&
                    (!checkBefore || RawTickCount.Old * IntervalPerTick < time);
            }
            else return Math.Abs(time - curRawTime) <= epsilon && 
                    (!checkBefore || Math.Abs(time - oldRawTime) >= epsilon);
        }

        /*
        // currently unused, useful in getting the entity index of a caller in an input/output 
        public int GetIndexOfEHANDLE(uint EHANDLE)
        {
            // FIXME: this mask is actually version dependent, newer ones use 0x1fff!!!
            // possible sig to identify: 8b ?? ?? ?? ?? ?? 8b ?? 81 e1 ff ff 00 00
            int mask = 0xFFF;
            return (EHANDLE & mask) == mask ? -1 : (int)(EHANDLE & mask);
        }
        */

        /// <summary>
        /// Gets a module with the specified file name
        /// </summary>
        /// <param name="name">The name of the module</param>
        /// <returns>The module with the specified file name</returns>
        public ProcessModuleWow64Safe GetModule(string name)
        {
            var proc = GameProcess.ModulesWow64SafeNoCache().FirstOrDefault(x => x.ModuleName.ToLower() == name.ToLower());
            Trace.Assert(proc != null);
            return proc;
        }
    }
}

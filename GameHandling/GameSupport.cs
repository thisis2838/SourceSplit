using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.ComponentHandling;

namespace LiveSplit.SourceSplit.GameHandling
{
    /// <summary>
    /// Class that handles game Auto-Start and Stop
    /// </summary>
    abstract partial class GameSupport
    {
        /// <summary>
        /// The first maps of the mod / game
        /// </summary>
        public List<string> FirstMaps { get; protected set; } = new List<string>();
        internal void AddFirstMap(params string[] maps)
        {
            FirstMaps.AddRange(maps);
        }
        /// <summary>
        /// The last maps of the mod / game
        /// </summary>
        public List<string> LastMaps { get; protected set; } = new List<string>();
        internal void AddLastMap(params string[] maps)
        {
            LastMaps.AddRange(maps);
        }
        /// <summary>
        /// The list of maps on a new session of which the timer should auto-start
        /// </summary>
        public List<string> StartOnFirstLoadMaps { get; internal set; } = new List<string>();
        /// <summary>
        /// The list of additional games and mods to do checks for
        /// </summary>
        public List<GameSupport> AdditionalGameSupport { get; internal set; } = new List<GameSupport>();
        // ticks to subtract
        /// <summary>
        /// Tick offset when starting the timer
        /// </summary>
        public int StartOffsetTicks { get; protected set; }
        /// <summary>
        /// Tick offset when ending the timer
        /// </summary>
        public int EndOffsetTicks { get; protected set; }
        /// <summary>
        /// Info about the game's timing methods and etc...
        /// </summary>
        public TimingSpecifics TimingSpecifics { get; protected set; } = new TimingSpecifics();

        // what kind of generic auto-start detection to use
        // must call base.OnUpdate
        protected AutoStart AutoStartType;
        protected enum AutoStart
        {
            None,
            Unfrozen,
            ViewEntityChanged,
            ParentEntityChanged
        }

        protected bool IsFirstMap { get; private set; }
        protected bool IsLastMap { get; private set; }
        private bool _onceFlag;

    }

    /// <summary>
    /// Games' / mods' default timing settings, 
    /// </summary>
    public class TimingSpecifics
    {
        /// <summary>
        /// Default Timing method SourceSplit should switch to when the "Let SourceSplit Decide" option is enabled
        /// </summary>
        public GameTimingMethod DefaultTimingMethod = new GameTimingMethod();

    }
}

using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.ComponentHandling;
using System.Diagnostics.Eventing;

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
        protected void AddFirstMap(params string[] maps)
        {
            FirstMaps.AddRange(maps.Select(x => x.ToLower()));
        }
        /// <summary>
        /// The last maps of the mod / game
        /// </summary>
        public List<string> LastMaps { get; protected set; } = new List<string>();
        protected void AddLastMap(params string[] maps)
        {
            LastMaps.AddRange(maps.Select(x => x.ToLower()));
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
        /// Millisecond offset when starting the timer
        /// </summary>
        public float StartOffsetMilliseconds { get; protected set; }
        /// <summary>
        /// Millisecond offset when ending the timer
        /// </summary>
        public float EndOffsetMilliseconds { get; protected set; }
        /// <summary>
        /// Info about the game's timing methods and etc...
        /// </summary>
        public TimingSpecifics TimingSpecifics { get; protected set; } = new TimingSpecifics();

        protected bool IsFirstMap { get; private set; }
        protected bool IsLastMap { get; private set; }
        protected bool OnceFlag;

        /// <summary>
        /// Game's command handler
        /// </summary>
        protected CustomCommandHandler CommandHandler = new CustomCommandHandler();

        /*
        /// <summary>
        /// List of output fire time value watchers and their corresponding details for tracking
        /// </summary>
        protected class OutputFireTimeWatcher : ValueWatcher<float>
        {
            public string TargetName;
            public string Command = null;
            public string Param = null;
            public int Clamp = 100;
            public Func<GameState, bool> QualifyingGameState = null;

            public OutputFireTimeWatcher() : base(0) { }
        }

        protected List<OutputFireTimeWatcher> OutputFireTimeWatchers = new List<OutputFireTimeWatcher>();
        protected void AddOutputFireTimeWatcher(params OutputFireTimeWatcher[] watcher)
        {
            OutputFireTimeWatchers.AddRange(watcher);
        }
        private void UpdateOutputFireTimeWatchers(GameState state)
        {
            if (!OutputFireTimeWatchers.Any()) return;

            foreach (var w in OutputFireTimeWatchers)
            {
                if (w.QualifyingGameState != null && !w.QualifyingGameState.Invoke(state))
                    continue;

                if (w.Command == null && w.Command == null)
                    w.Current = state.GameEngine.GetOutputFireTime(w.TargetName, w.Clamp);
                else
                    w.Current = state.GameEngine.GetOutputFireTime
                    (
                        w.TargetName, 
                        w.Command,
                        w.Param,
                        w.Clamp
                    );
            }
        }
        */
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

using LiveSplit.SourceSplit.Utilities;
using System.Linq;


namespace LiveSplit.SourceSplit.GameHandling
{
    abstract partial class GameSupport
    {
        /// <summary>
        /// Returns the corresponding function this mod / game is based on.
        /// </summary>
        public virtual GameEngine GetEngine() { return new GenericEngine(); }


        public void OnGameAttached(GameState state, TimerActions actions)
        {
            CommandHandler.Init(state);

            OnGameAttachedInternal(state, actions);
            Templates?.ForEach(x => x.OnGameAttached(state, actions));
            AdditionalGameSupport?.ForEach(x => x.OnGameAttached(state, actions));
        }
        /// <summary>
        /// Actions to do when SourceSplit has successfully acquired a game process.
        /// Called at the end of TryGetGameProcess().
        /// </summary>
        protected virtual void OnGameAttachedInternal(GameState state, TimerActions actions) { }


        public virtual void OnTimerReset(bool resetFlagTo)
        {
            OnTimerResetInternal(resetFlagTo);
            Templates?.ForEach(x => x.OnTimerReset(resetFlagTo));
            AdditionalGameSupport?.ForEach(x => x.OnTimerReset(resetFlagTo));
        }
        /// <summary>
        /// Actions to do when the timer is manually reset.
        /// Called when Livesplit state's OnReset() is called .
        /// </summary>
        protected virtual void OnTimerResetInternal(bool resetFlagTo) { }


        public void OnSessionStart(GameState state, TimerActions actions)
        {
            OnceFlag = false;

            string map = state.Map.Current;
            this.IsFirstMap = FirstMaps.ConvertAll(x => x.ToLower()).Contains(map);
            this.IsLastMap = LastMaps.ConvertAll(x => x.ToLower()).Contains(map);

            OnSessionStartInternal(state, actions);
            Templates?.ForEach(x => x.OnSessionStart(state, actions));
            AdditionalGameSupport?.ForEach(x => x.OnSessionStart(state, actions));
        }
        /// <summary>
        /// Actions to do when a new session starts and the player is fully in the game. 
        /// Called when SignOnState changes to Full and HostState is Run.
        /// </summary>
        protected virtual void OnSessionStartInternal(GameState state, TimerActions actions) { }


        public void OnSessionEnd(GameState state, TimerActions actions)
        {
            OnSessionEndInternal(state, actions);
            Templates?.ForEach(x => x.OnSessionEnd(state, actions));
            AdditionalGameSupport?.ForEach(x => x.OnSessionEnd(state, actions));
        }
        /// <summary>
        /// Actions to do when a session ends and the player is no longer fully in-game. 
        /// Called when HostState changes away from Run.
        /// </summary>
        protected virtual void OnSessionEndInternal(GameState state, TimerActions actions) { }


        public void OnGenericUpdate(GameState state, TimerActions actions)
        {
            CommandHandler.Update(state);

            OnGenericUpdateInternal(state, actions);
            Templates?.ForEach(x => x.OnGenericUpdate(state, actions));
            AdditionalGameSupport?.ForEach(x => x.OnGenericUpdate(state, actions));
        }
        /// <summary>
        /// Actions to do on every SourceSplit internal update loop.
        /// Called after UpdateGameState() and before CheckGameState().
        /// </summary>
        protected virtual void OnGenericUpdateInternal(GameState state, TimerActions actions) { }


        public void OnUpdate(GameState state, TimerActions actions)
        {
            OnUpdateInternal(state, actions);
            Templates?.ForEach(x => x.OnUpdate(state, actions));
            AdditionalGameSupport?.ForEach(x => x.OnUpdate(state, actions));
        }
        /// <summary>
        /// Actions to do when the timer updates and the player is fully in the game. 
        /// Called when SignOnState is Full and HostState is Run.
        /// </summary>
        protected virtual void OnUpdateInternal(GameState state, TimerActions actions) { }


        public void OnSaveLoaded(GameState state, TimerActions actions, string saveName)
        {
            OnSaveLoadedInternal(state, actions, saveName);
            Templates?.ForEach(x => x.OnSaveLoaded(state, actions, saveName));
            AdditionalGameSupport?.ForEach(x => x.OnSaveLoaded(state, actions, saveName));
        }
        /// <summary>.
        /// Actions to do when a save is QUEUED to be loaded.
        /// Called when HostState changes to LoadGame.
        /// </summary>
        protected virtual void OnSaveLoadedInternal(GameState state, TimerActions actions, string saveName) { }


        public bool OnNewGame(GameState state, TimerActions actions, string newMapName)
        {
            if (StartOnFirstLoadMaps.Contains(newMapName))
            {
                Logging.WriteLine(state.GameDir + " new game start on " + newMapName);
                actions.Start();
                return false;
            }

            if (OnNewGameInternal(state, actions, newMapName) &&
                Templates.All(x => x.OnNewGame(state, actions, newMapName)) &&
                AdditionalGameSupport.All(x => x.OnNewGame(state, actions, newMapName)))
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Actions to do when a New Game load is detected. 
        /// Called when HostState changes to NewGame.
        /// </summary>
        /// <returns>Whether GameMemory should signal an Autosplit to the new map</returns>
        protected virtual bool OnNewGameInternal(GameState state, TimerActions actions, string newMapName) { return true; }


        public bool OnChangelevel(GameState state, TimerActions actions, string newMapName)
        {
            if (OnChangelevelInternal(state, actions, newMapName) &&
                Templates.All(x => x.OnChangelevel(state, actions, newMapName)) &&
                AdditionalGameSupport.All(x => x.OnChangelevel(state, actions, newMapName)))
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Actions to do when a changelevel is detected. 
        /// Called when HostState changes to ChangeLevelSP or ChangeLevelMP.
        /// </summary>
        /// <returns>Whether GameMemory should signal an Autosplit to the new map</returns>        
        protected virtual bool OnChangelevelInternal(GameState state, TimerActions actions, string newMapName) { return true; }
    }
}

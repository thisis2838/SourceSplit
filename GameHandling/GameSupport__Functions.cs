using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.ComponentHandling;


namespace LiveSplit.SourceSplit.GameHandling
{
    abstract partial class GameSupport
    {
        /// <summary>
        /// Returns the corresponding function this mod / game is based on
        /// </summary>
        /// <returns></returns>
        public virtual GameEngine GetEngine() { return new GenericEngine(); }
        /// <summary>
        /// Actions to do when the game process is found and game-specific code is initialized. 
        /// Called when attached to a new game process
        /// </summary>
        /// <param name="name">The name of the entity</param>
        public virtual void OnGameAttached(GameState state, TimerActions actions) { }
        /// <summary>
        /// Actions to do when the timer is manually reset
        /// Called when the timer is reset
        /// </summary>
        /// <param name="resetFlagTo">Value of corresponding reset flag</param>
        public virtual void OnTimerReset(bool resetFlagTo) { }
        /// <summary>
        /// Actions to do when a new session starts and the player is fully in the game. 
        /// Called on the first tick when player is fully in the game (according to demos)
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual void OnSessionStart(GameState state, TimerActions actions)
        {
            _onceFlag = false;

            string map = state.Map.Current.ToLower();

            this.IsFirstMap = FirstMaps.ConvertAll(x => x.ToLowerInvariant()).Contains(map);
            this.IsLastMap = LastMaps.ConvertAll(x => x.ToLowerInvariant()).Contains(map);
        }
        /// <summary>
        /// Actions to do when a session ends and the player is no longer fully in-game. 
        /// Called when player no longer fully in the game (map changed, load started)
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual void OnSessionEnd(GameState state, TimerActions actions) { }
        /// <summary>
        /// Actions to do when the timer updates, regardless of game states. 
        /// Called every update loop, regardless if the player is fully in-game
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual void OnGenericUpdate(GameState state, TimerActions actions) { }
        /// <summary>
        /// Actions to do when the timer updates and the player is fully in the game. 
        /// Called once per tick when player is fully in the game
        /// </summary>
        /// <param name="state">GameState</param>
        public virtual void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.AutoStartType == AutoStart.Unfrozen
                && !state.PlayerFlags.Current.HasFlag(FL.FROZEN)
                && state.PlayerFlags.Old.HasFlag(FL.FROZEN))
            {
                Debug.WriteLine("FL_FROZEN removed from player");
                _onceFlag = true;
                actions.Start(StartOffsetTicks);
            }
            else if (this.AutoStartType == AutoStart.ViewEntityChanged
                && state.PlayerViewEntityIndex.Old != GameState.ENT_INDEX_PLAYER
                && state.PlayerViewEntityIndex.Current == GameState.ENT_INDEX_PLAYER)
            {
                Debug.WriteLine("view entity changed to player");
                _onceFlag = true;
                actions.Start(StartOffsetTicks);
            }
            else if (this.AutoStartType == AutoStart.ParentEntityChanged
                && state.PlayerParentEntityHandle.Old != -1
                && state.PlayerParentEntityHandle.Current == -1)
            {
                Debug.WriteLine("player no longer parented");
                _onceFlag = true;
                actions.Start(StartOffsetTicks);
            }

            return;
        }
        /// <summary>
        /// Actions to do when a save is loaded
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        public virtual void OnSaveLoaded(GameState state, TimerActions actions, string saveName) { }

    }
}

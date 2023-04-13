using System;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Synergy : GameSupport
    {
        // start: on map load

        private CustomCommand _autosplitIL = new CustomCommand("ilstart", "0");
        private const FL _dead = FL.ATCONTROLS | FL.NOTARGET | FL.AIMTARGET;

        private HL2 _hl2 = new HL2();
        private HL2Ep1 _ep1 = new HL2Ep1();
        private HL2Ep2 _ep2 = new HL2Ep2();

        public Synergy()
        {
            CommandHandler.Commands.Add(_autosplitIL);
            this.AdditionalGameSupport.AddRange(new GameSupport[] { _hl2 , _ep1 , _ep2 });
        }

        protected override void OnGenericUpdateInternal(GameState state, TimerActions actions)
        {
            // HACKHACK: when the player dies and respawn, the map is also lightly reloaded,
            // potentially causing all the entity indicies to change
            // players when killed also have these flags applied to them and removed when respawning
            // so lets fire onsessionstart then
            if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                if (!state.PlayerFlags.Current.HasFlag(_dead) &&
                    state.PlayerFlags.Old.HasFlag(_dead))
                {
                    this.OnSessionStart(state, actions);
                    this.AdditionalGameSupport.ForEach(x => x.OnSessionStart(state, actions));
                    Logging.WriteLine("synergy session start");
                }    
            }

            if (_autosplitIL.Boolean)
            {
                if (!StartOnFirstLoadMaps.Contains(state.Map.Current))
                {
                    StartOnFirstLoadMaps.Clear();
                    StartOnFirstLoadMaps.Add(state.Map.Current);
                }
            }
            else
                StartOnFirstLoadMaps.Clear();
        }

    }
}
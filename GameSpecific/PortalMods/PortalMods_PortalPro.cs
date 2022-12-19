using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_PortalPro : Portal
    {
        // how to match this timing with demos:
        // start: on view entity changing from start camera's to the player's
        // ending: on view entity changing from the player's to final camera's

        private int _startCamIndex;
        private int _endCamIndex;

        public PortalMods_PortalPro() : base()
        {
            this.AddFirstMap("start");
            this.AddLastMap("boss");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("wub_viewcontrol");
                //Debug.WriteLine("found start cam index at " + _startCamIndex);
            }

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("end_game_camera");
                //Debug.WriteLine("found end cam index at " + _endCamIndex);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.Old == _startCamIndex && state.PlayerViewEntityIndex.Current == 1)
                {
                    Debug.WriteLine("portal pro start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex.Old == 1 && state.PlayerViewEntityIndex.Current == _endCamIndex)
                {
                    Debug.WriteLine("portal pro end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }

    }
}

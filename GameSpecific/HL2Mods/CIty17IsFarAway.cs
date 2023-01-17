using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;
using System;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class City17IsFarAway : GameSupport
    {
        // start: when the view entity index switches from the start camera to the player's
        // ending: when the view entity index switches from the player's to the end camera

        private int _startCamIndex = -1;
        private int _endCamIndex = -1;

        public City17IsFarAway()
        {
            AddFirstMap("station");
            AddLastMap("finale");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("start_camera");
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("game_end_point_view");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag) 
                return;

            if (IsFirstMap && _startCamIndex != -1)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    OnceFlag = true;
                    Debug.WriteLine("c17 is far away start");
                    actions.Start();
                }
            }
            else if (IsLastMap && _endCamIndex != -1)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _endCamIndex))
                {
                    OnceFlag = true;
                    Debug.WriteLine("c17 is far away end");
                    actions.End();
                }
            }
        }
    }
}

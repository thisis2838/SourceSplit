using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.IO;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Amalgam : GameSupport
    {
        // start:
        //      if no intro camera is found, start on game load
        //      else when the view index switches from start camera to the player's
        // ending: when the view index switches from the player's to the end camera

        private int _startCamIndex = -1;
        private int _endCamIndex = -1;

        private bool _newGame = false;

        public Amalgam()
        {
            AddFirstMap("intro_1");
            AddLastMap("beacon_1");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                if (_newGame)
                {
                    if (state.GameEngine.GetEntIndexByName("introcamera2") == -1)
                    {
                        Debug.WriteLine("amalgam cutsceneless start");
                        actions.Start();
                    }
                }
                
                _startCamIndex = state.GameEngine.GetEntIndexByName("camera1");
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("outrocam12");
            }

            _newGame = false;
        }

        protected override bool OnNewGameInternal(GameState state, TimerActions actions, string newMapName)
        {
            _newGame = true;
            return true;
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
                    Debug.WriteLine("amalgam start");
                    actions.Start();
                }
            }
            else if (IsLastMap && _endCamIndex != -1)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _endCamIndex))
                {
                    OnceFlag = true;
                    Debug.WriteLine("amalgam end");
                    actions.End();
                }
            }
        }
    }
}

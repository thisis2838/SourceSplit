using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class Amalgam : GameSupport
    {
        // start:
        //      if no intro camera is found, start on game load
        //      else when the view index switches from start camera to the player's
        // ending: when the view index switches from the player's to the end camera

        private bool _newGame = false;

        public Amalgam()
        {
            AddFirstMap("intro_1");
            AddLastMap("beacon_1");

            WhenCameraSwitchesToPlayer(ActionType.AutoStart, "camera1");
            WhenCameraSwitchesFromPlayer(ActionType.AutoEnd, "outrocam12");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                if (_newGame)
                {
                    if (state.GameEngine.GetEntIndexByName("introcamera2") == -1)
                    {
                        Logging.WriteLine("amalgam cutsceneless start");
                        actions.Start();
                    }
                }
                
            }

            _newGame = false;
        }

        protected override bool OnNewGameInternal(GameState state, TimerActions actions, string newMapName)
        {
            _newGame = true;
            return true;
        }
    }
}

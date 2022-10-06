using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DeepDown : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private bool _onceFlag;

        private int _introIndex;

        private float _splitTime;

        public HL2Mods_DeepDown()
        {
            
            this.AddFirstMap("ep2_deepdown_1");
            this.AddLastMap("ep2_deepdown_5");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._introIndex = state.GameEngine.GetEntIndexByName("IntroCredits1");
                //Debug.WriteLine("intro index is " + this._introIndex);
            }

            if (this.IsLastMap)
            {
                _splitTime = state.GameEngine.GetOutputFireTime("Titles_music1", 17);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (FirstMaps.Contains(state.Map.Current.ToLower()) && this._introIndex != -1)
            {
                var newIntro = state.GameEngine.GetEntInfoByIndex(_introIndex);

                if (newIntro.EntityPtr == IntPtr.Zero)
                {
                    _introIndex = -1;
                    _onceFlag = true;
                    Debug.WriteLine("deepdown start");
                    actions.Start(StartOffsetTicks); return;
                }
            }
            else if (LastMaps.Contains(state.Map.Current.ToLower()))
            {
                float splitTime = state.GameEngine.GetOutputFireTime("AlyxWakeUp1", 7);

                if (_splitTime == 0f && splitTime != 0f)
                {
                    Debug.WriteLine("deepdown end");
                    _onceFlag = true;
                    _splitTime = splitTime;
                    actions.End(EndOffsetTicks); return;
                }

                _splitTime = splitTime;
            }
            return;
        }
    }
}

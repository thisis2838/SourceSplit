using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class DeepDown : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private int _introIndex;
        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public DeepDown()
        {
            this.AddFirstMap("ep2_deepdown_1");
            this.AddLastMap("ep2_deepdown_5");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._introIndex = state.GameEngine.GetEntIndexByName("IntroCredits1");
                //Debug.WriteLine("intro index is " + this._introIndex);
            }

            if (this.IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("Titles_music1");
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (FirstMaps.Contains(state.Map.Current) && this._introIndex != -1)
            {
                var newIntro = state.GameEngine.GetEntInfoByIndex(_introIndex);

                if (newIntro.EntityPtr == IntPtr.Zero)
                {
                    _introIndex = -1;
                    OnceFlag = true;
                    Debug.WriteLine("deepdown start");
                    actions.Start(StartOffsetMilliseconds); 
                }
            }
            else if (LastMaps.Contains(state.Map.Current))
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("AlyxWakeUp1");

                if (_splitTime.ChangedFrom(0))
                {
                    Debug.WriteLine("deepdown end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }
}

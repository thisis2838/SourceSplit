using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_MImp : GameSupport
    {
        // how to match with demos:
        // start: when cave_giveitems_equipper is called
        // ending: when player's view entity changes

        private int _camIndex;
        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        public HL2Mods_MImp()
        {
            this.AddFirstMap("mimp1");
            this.AddLastMap("mimp3");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsFirstMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("cave_giveitems_equipper");
            }
            else if (this.IsLastMap)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("outro.camera");
                //Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap)
            {
                float newSplitTime = state.GameEngine.GetOutputFireTime("cave_giveitems_equipper");
                if (_splitTime.ChangedTo(0))
                {
                    OnceFlag = true;
                    Debug.WriteLine("mimp start");
                    actions.Start(StartOffsetMilliseconds);
                }
            }
            else if (this.IsLastMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Current == _camIndex && state.PlayerViewEntityIndex.Old != _camIndex)
                {
                    Debug.WriteLine("mimp end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}

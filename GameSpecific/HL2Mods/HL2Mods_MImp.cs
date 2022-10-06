using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_MImp : GameSupport
    {
        // how to match with demos:
        // start: when cave_giveitems_equipper is called
        // ending: when player's view entity changes

        private bool _onceFlag;

        private int _camIndex;
        private float _splitTime;

        public HL2Mods_MImp()
        {
            this.AddFirstMap("mimp1");
            this.AddLastMap("mimp3");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);

            if (this.IsFirstMap)
            {
                _splitTime = state.GameEngine.GetOutputFireTime("cave_giveitems_equipper", 5);
            }
            else if (this.IsLastMap)
            {
                this._camIndex = state.GameEngine.GetEntIndexByName("outro.camera");
                //Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                float newSplitTime = state.GameEngine.GetOutputFireTime("cave_giveitems_equipper", 5);
                if (_splitTime != 0f && newSplitTime == 0f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("mimp start");
                    actions.Start(StartOffsetTicks); return;
                }
                _splitTime = newSplitTime;
            }
            else if (this.IsLastMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex.Current == _camIndex && state.PlayerViewEntityIndex.Old != _camIndex)
                {
                    Debug.WriteLine("mimp end");
                    _onceFlag = true;
                    actions.End(EndOffsetTicks); return;
                }
            }

            return;
        }
    }
}

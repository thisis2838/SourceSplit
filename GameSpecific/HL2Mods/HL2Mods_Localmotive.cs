using LiveSplit.ComponentUtil;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Localmotive : GameSupport
    {
        // start: on first map
        // ending: when the end text model's skin code is 10 and player view entity switches to the final camera

        private bool _onceFlag;
        private int _camIndex;
        private int _spriteIndex = -1;

        public HL2Mods_Localmotive()
        {
            this.AddFirstMap("eli_final");
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            if (IsFirstMap)
            {
                _camIndex = state.GameEngine.GetEntIndexByName("car_view");
                _spriteIndex = state.GameEngine.GetEntIndexByName("exit_button_sprite");
            }
            _onceFlag = false;
        }


        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_camIndex, 1))
                {
                    actions.Start();
                    Debug.WriteLine("localmotive start");
                    return;
                }
                if (_spriteIndex != -1)
                {
                    if (state.GameEngine.GetEntInfoByIndex(_spriteIndex).EntityPtr == System.IntPtr.Zero)
                    {
                        actions.End();
                        Debug.WriteLine($"localmotive end");
                        _spriteIndex = -1;
                        _onceFlag = true;
                        return;
                    }
                }
            }

            return;
        }
    }
}

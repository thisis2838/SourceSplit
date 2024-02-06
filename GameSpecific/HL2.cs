using System.Collections.Generic;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.ComponentHandling;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2 : GameSupport
    {
        // start: first tick when your position is at -9419 -2483 22 (cl_showpos 1)
        // ending: first tick when screen flashes white

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();

        private Vector3f _startPos = new Vector3f(-9419f, -2483f, 22f);

        public HL2()
        {
            this.AddFirstMap("d1_trainstation_01");
            this.AddLastMap("d3_breen_01");
            AdditionalGameSupport = new List<GameSupport>()
            {
                new HL2Mods.TheLostCity(),
                new HL2Mods.Tinje(),
                new HL2Mods.ExperimentalFuel(),
                new HL2Mods.NightmareHouse()
            };
            SourceSplitComponent.Settings.SLPenalty.Lock(1);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (this.IsLastMap)
                _splitTime.Current = state.GameEngine.GetOutputFireTime("sprite_end_final_explosion_1", "ShowSprite", "");
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap) 
            {
                // "OnTrigger" "point_teleport_destination,Teleport,,0.1,-1"

                // first tick player is moveable and on the train
                if (state.PlayerPosition.Current.DistanceXY(_startPos) <= 1.0)
                {
                    Logging.WriteLine("hl2 start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds); return;
                }
            }
            else if (this.IsLastMap)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("sprite_end_final_explosion_1", "ShowSprite", "");
                if (_splitTime.Current > 0 && _splitTime.Old == 0)
                {
                    Logging.WriteLine("hl2 end");
                    OnceFlag = true;
                    actions.End(EndOffsetMilliseconds); 
                }
            }

            return;
        }
    }
}

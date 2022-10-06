using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Infra : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // endings: all on fades

        private bool _onceFlag = false;

        public Infra()
        {
            this.AddFirstMap("infra_c1_m1_office");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }

        public override GameEngine GetEngine()
        {
            return new InfraEngine();
        }

        public bool DefaultEnd(GameState state, float fadeSpeed, string ending, TimerActions actions)
        {
            float splitTime = state.GameEngine.GetFadeEndTime(fadeSpeed);
            // this is how the game actually knows when a fade has finished as well
            if (state.CompareToInternalTimer(splitTime, 0.05f))
            {
                _onceFlag = true;
                Debug.WriteLine("infra " + ending + " ending");
                actions.Split();
                return true;
            }
            return false;
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            _onceFlag = false;
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            switch (state.Map.Current.ToLower())
            {
                default:
                    return;

                case "infra_c5_m2b_sewer2":
                    {
                        DefaultEnd(state, -2560f, "part 1", actions);
                        return;
                    }
                case "infra_c7_m5_powerstation":
                    {
                        // v2 and v3 have different start and end durations since v2 ends in a credits sequence 
                        var test = DefaultEnd(state, -85f, "part 2", actions);
                        if (!test)
                            DefaultEnd(state, -2560f, "part 2", actions);
                        return;
                    }
                case "infra_c11_ending_1":
                case "infra_c11_ending_2":
                case "infra_c11_ending_3":
                    {
                        DefaultEnd(state, -25.5f, "part 3", actions);
                        return;
                    }
            }
        }
    }
}

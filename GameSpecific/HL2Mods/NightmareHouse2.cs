using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class NightmareHouse2 : GameSupport
    {
        // start: when view entity index switches from view controller to the player's
        // ending:
        //      secret: when the view entity indes switches from the player's to the end camera
        //      normal: when the output to disable hud is fired AND the output to trigger the ending relay is queued.

        private int _startCamIndex = -1;
        private int _endCamIndex = -1;

        private ValueWatcher<float> _endSplitTime = new ValueWatcher<float>();

        public NightmareHouse2()
        {
            AdditionalGameSupport.AddRange
            (
                new NightmareHouse1_Remake()
            );

            AddFirstMap("nh2c1_v2");
            AddFirstMap("nh2c1");
            AddLastMap("nh2c7_v2");
            AddLastMap("nh2c7");
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("blackout_viewcontroller");
            }

            if (IsLastMap)
            {
                _endCamIndex = state.GameEngine.GetEntIndexByName("credits_camera_main");
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("nohud", "ModifySpeed", "0.9");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    actions.Start();
                    OnceFlag = true;
                    Debug.WriteLine("nightmare house 2 start");
                }
            }

            if (IsLastMap)
            {
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("nohud", "ModifySpeed", "0.9");
                if (_endSplitTime.ChangedTo(0))
                {
                    // just to be sure, may not be needed...
                    var endRelayTime = state.GameEngine.GetOutputFireTime("core_destroyed_r", "trigger", null);
                    if (endRelayTime != 0)
                    {
                        actions.End();
                        OnceFlag = true;
                        Debug.WriteLine("nightmare house 2 end");
                    }
                }

                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _endCamIndex))
                {
                    actions.End();
                    OnceFlag = true;
                    Debug.WriteLine("nightmare house 2 secret end");
                }
            }
        }
    }

    class NightmareHouse1_Remake : GameSupport
    {
        private int _startCamIndex = -1;
        private ValueWatcher<float> _endSplitTime = new ValueWatcher<float>();

        public NightmareHouse1_Remake()
        {
            AddFirstMap("nh1remake1_v2");
            AddFirstMap("nh1remake1_fixed");

            AddLastMap(FirstMaps.ToArray());
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startCamIndex = state.GameEngine.GetEntIndexByName("blackout_viewcontroller");
            }

            if (IsLastMap)
            {
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("teleport2");
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startCamIndex, 1))
                {
                    actions.Start();
                    Debug.WriteLine("nightmare house 1 (remake) start");
                }
            }

            if (IsLastMap)
            {
                _endSplitTime.Current = state.GameEngine.GetOutputFireTime("teleport2");
                if (_endSplitTime.ChangedTo(0))
                {
                    actions.End();
                    Debug.WriteLine("nightmare house 1 (remake) end");
                }
            }
        }
    }
}

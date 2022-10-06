using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static LiveSplit.SourceSplit.GameSpecific.TBGFireTimeWatcher;

namespace LiveSplit.SourceSplit.GameSpecific
{
    partial class TheBeginnersGuide : GameSupport
    {
        // start: 2:27.50 before map load 
        // ending: when player move speed is modified

        private bool _onceFlag = false;

        private MemoryWatcher<float> _playerMoveSpeed;

        private const int _pointMsgEnabledOffset = 0x366;

        private CustomCommandHandler _ccHandler = new CustomCommandHandler();
        private CustomCommand _enableExtraSplits = new CustomCommand(
            "esplits", 
            "0", 
            "Enables/Disables extra split points", 
            archived: true);

        private Dictionary<string, List<ITBGExtraSplit>> _points = new Dictionary<string, List<ITBGExtraSplit>>();

        private List<string> _maps = new List<string>()
        {
            "whisper",
            "backwards",
            "entering",
            "stairs",
            "puzzle",
            "exiting",
            "down",
            "notes",
            "escape",
            "house",
            "lecture",
            "theater",
            "mobius",
            "presence",
            "machine",
            "tower",
            "nomansland1",
            "nomansland2"
        };

        public TheBeginnersGuide()
        {
            StartOffsetTicks = -8850;
            this.AddFirstMap("whisper");
            this.AddLastMap("nomansland2");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);

            typeof(TheBeginnersGuide).GetFields(BindingFlags.NonPublic | BindingFlags.Instance) 
                .Select (x => x.GetValue(this)).Where(x => x is ITBGExtraSplit).Select(x => x as ITBGExtraSplit)
                .GroupBy(x => x.Map).ToList().ForEach(x => _points.Add(x.Key, x.ToList()));

            _enableExtraSplits.Description = 
                $"Enables the following split points:\n" + 
                string.Join("\n---------------\n", _points.Select(x => 
                    "\t- " + x.Key + "\n\t\t" + string.Join("\n\t\t", x.Value.Select(y => "\"\"\"\"" + y.Description))));

            _ccHandler = new CustomCommandHandler(_enableExtraSplits);
        }

        public override void OnGameAttached(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server;
            server = state.GetModule("server.dll");
            _playerMoveSpeed = new MemoryWatcher<float>(server.BaseAddress + 0x761310);

            _ccHandler.Init(state);
        }

        public override void OnSessionStart(GameState state, TimerActions actions)
        {
            base.OnSessionStart(state, actions);
            _onceFlag = false;

            if (_points.ContainsKey(state.Map.Current.ToLower()))
                _points[state.Map.Current.ToLower()].ForEach(x => x.Reset(state));
        }

        public override void OnGenericUpdate(GameState state, TimerActions actions)
        {
            base.OnGenericUpdate(state, actions);
            _ccHandler.Update(state);
        }

        public override void OnUpdate(GameState state, TimerActions actions)
        {
            if (_onceFlag)
                return;

            if (_enableExtraSplits.BValue)
            {
                string map = state.Map.Current.ToLower();

                if (_points.ContainsKey(map))
                {
                    _points[map].ForEach(x => x.Update(state));
                    if (_points[map].FirstOrDefault(x => x.CheckSplit(state)) is var done && done != null)
                    {
                        Debug.WriteLine($"split on extra split: {done.Description}");
                        actions.Split();
                    }
                }

            }

            if (this.IsLastMap)
            {
                _playerMoveSpeed.Update(state.GameProcess);
                if (_playerMoveSpeed.Old != 0 && _playerMoveSpeed.Current == 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("tbg end");
                    actions.End(EndOffsetTicks); return;
                }
            }
            return;
        }
    }

}

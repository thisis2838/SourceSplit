using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiveSplit.SourceSplit.ComponentHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    partial class TheBeginnersGuide : GameSupport
    {
        // start: 2:27.50 before map load 
        // ending: when player move speed is modified

        private MemoryWatcher<float> _playerMoveSpeed;

        private const int _pointMsgEnabledOffset = 0x366;

        private CustomCommand _enableExtraSplits = new CustomCommand(
            "esplits", 
            "0", 
            "Enables/Disables extra split points", 
            archived: true);

        private Dictionary<string, List<ITBGExtraSplit>> _points = new Dictionary<string, List<ITBGExtraSplit>>();

        private List<string> _maps = new List<string>()
        {
            "intro",
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
            this.AddFirstMap("intro");
            this.StartOnFirstLoadMaps.AddRange(FirstMaps);
            this.AddLastMap("nomansland2");

            typeof(TheBeginnersGuide).GetFields(BindingFlags.NonPublic | BindingFlags.Instance) 
                .Select (x => x.GetValue(this)).Where(x => x is ITBGExtraSplit).Select(x => x as ITBGExtraSplit)
                .GroupBy(x => x.Map).ToList().ForEach(x => _points.Add(x.Key, x.ToList()));

            _enableExtraSplits.LongDescription =
                $"Enables the following split points:\n" + 
                string.Join("\n\n", _points.Select(x => 
                    "\t- " + x.Key + "\n\t\t" + string.Join("\n\t\t", x.Value.Select(y => "\t" + y.Description))));

            CommandHandler.Commands.Add(_enableExtraSplits);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            _playerMoveSpeed = new MemoryWatcher<float>(server.BaseAddress + 0x761310);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (_points.ContainsKey(state.Map.Current))
                _points[state.Map.Current].ForEach(x => x.Reset(state));
        }

        protected override bool OnNewGameInternal(GameState state, TimerActions actions, string newMapName)
        {
            if (newMapName.StartsWith("menu")) return false;

            switch (newMapName)
            {
                case "whisper": actions.Start(-147500f); break;
                default:
                    {
                        if (SourceSplitComponent.Settings.AutoSplitOnGenericMap.Value)
                            return true;

                        var current = _maps.IndexOf(state.Map.Current);
                        var next = _maps.IndexOf(newMapName);

                        if (current >= 0 && next >= 0 && next - current == 1)
                            actions.Split();

                        break;
                    }
            }
            return false;
        }

        protected override bool OnChangelevelInternal(GameState state, TimerActions actions, string newMapName)
        {
            if (newMapName == "whisper") actions.Start(-147500f); 
            return false;
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (_enableExtraSplits.Boolean)
            {
                string map = state.Map.Current;

                if (_points.ContainsKey(map))
                {
                    _points[map].ForEach(x => x.Update(state));
                    if (_points[map].FirstOrDefault(x => x.CheckSplit(state)) is var done && done != null)
                    {
                        Logging.WriteLine($"split on extra split: {done.Description}");
                        actions.Split();
                    }
                }

            }

            if (this.IsLastMap)
            {
                _playerMoveSpeed.Update(state.GameProcess);
                if (_playerMoveSpeed.Old != 0 && _playerMoveSpeed.Current == 0)
                {
                    OnceFlag = true;
                    Logging.WriteLine("tbg end");
                    actions.End(EndOffsetMilliseconds);
                }
            }

            return;
        }
    }

}

using LiveSplit.UI.Components;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    partial class SourceSplitComponent : IComponent
    {
        public enum SplitType
        {
            User,
            AutoSplitter
        }

        class SplitOperations
        {
            internal struct SplitEvent
            {
                public SplitType Type;

                public string PreviousMap;
                public string NextMap;

                public SplitEvent(SplitType type, string prev, string next)
                {
                    Type = type;
                    PreviousMap = prev;
                    NextMap = next;
                }

                public bool CompareTo(SplitEvent other)
                {
                    return other.PreviousMap == PreviousMap && other.NextMap == NextMap;
                }

                public override string ToString()
                {
                    return $"{Type} : {PreviousMap} -> {NextMap}";
                }
            }

            public List<SplitEvent> SplitEvents = new List<SplitEvent>();
            public bool HasJustAutoSplit = false;

            public void AddSplit(SplitType type, string previous = "", string next = "")
            {
                var split = new SplitEvent(type, previous, next);
                SplitEvents.Add(split);
                if (type == SplitType.AutoSplitter)
                    HasJustAutoSplit = true;
            }

            public bool ExistsTransition(string previous, string next, SplitType type)
            {
                return SplitEvents.Any(x => x.Type == type && x.PreviousMap == previous && x.NextMap == next);
            }

            public void UndoLast()
            {
                if (SplitEvents.Any()) 
                    SplitEvents.RemoveAt(SplitEvents.Count() - 1);
            }

            public void Clear()
            {
                SplitEvents.Clear();
            }
        }
    }
}

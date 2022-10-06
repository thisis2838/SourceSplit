using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.SourceSplit.GameSpecific;
using static LiveSplit.SourceSplit.GameHandling.GameMemory;
using LiveSplit.SourceSplit.GameHandling;
using System.Reflection;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.ComponentHandling;

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
                SplitEvents.RemoveAt(SplitEvents.Count() - 1);
            }

            public void Clear()
            {
                SplitEvents.Clear();
            }
        }
    }
}

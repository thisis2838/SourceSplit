using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public class Session
    {
        public long ActiveTicks { get; set; }
        public TimeSpan ActiveTime => TimeSpanUtils.TimeFromTicks(ActiveTicks, TickRate);
        public long PausedTicks { get; set; }
        public TimeSpan PausedTime => TimeSpanUtils.TimeFromTicks(PausedTicks, TickRate);
        public long StartTick { get; set; }
        public TimeSpan StartTime => TimeSpanUtils.TimeFromTicks(StartTick, TickRate);
        public long OffsetTicks { get; set; } = 0;
        public TimeSpan OffsetTime => TimeSpanUtils.TimeFromTicks(OffsetTicks, TickRate);
        public long TotalTicks => ActiveTicks + PausedTicks + OffsetTicks - StartTick;
        public TimeSpan TotalTime => TimeSpanUtils.TimeFromTicks(TotalTicks, TickRate);

        public float TickRate { get; set; } = 0.015f;

        public bool Ended { get; set; } = false;

        public string MapName { get; private set; }

        public Session(string map, float rate)
        {
            TickRate = rate;
            MapName = map;
        }

    }

    public class SessionList : List<Session>
    {
        // cache this or else we easily get performance problems
        private long _totalTicks = 0;
        public long TotalTicks 
        { 
            get 
            {
                switch (Count)
                {
                    case 0: return 0;
                    case 1: return this[0].TotalTicks;
                    default: return _totalTicks + this[Count - 1].TotalTicks;
                }
            } 
        }

        public Session Current => this.LastOrDefault();

        public new void Add(Session session)
        {
            if (Count > 0)
            {
                _totalTicks = this.Sum(x => x.TotalTicks);
                SessionsForm.Instance.Add(this);
            }
            base.Add(session);
        }

        public new void Clear()
        {
            base.Clear();
            _totalTicks = 0;
            SessionsForm.Instance.Clear();
        }
    }
}

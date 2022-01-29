using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utils
{
    // simple value watcher keeping track of a pair of values.
    class ValueWatcher<T>
    {
        private T _current;
        public bool Changed = false;
        public T Current 
        {
            get { return _current; }
            set
            {
                Old = _current;
                _current = value;
                Changed = !Old.Equals(_current);
            }
        }
        public T Old { get; protected set; }
        public ValueWatcher(T t)
        {
            Current = t;
            Old = t;
        }

        public bool ChangedFromTo(T from, T to)
        {
            return Old.Equals(from) && Current.Equals(to);
        }

        public bool ChangedTo(T to)
        {
            return !Old.Equals(to) && Current.Equals(to);
        }

        public bool ChangedFrom(T from)
        {
            return Old.Equals(from) && !Current.Equals(from);
        }
    }
}

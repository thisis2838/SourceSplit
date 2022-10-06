using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    // simple value watcher keeping track of values, one feature away from being an undo-able value class
    public class ValueWatcher<T>
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
                Changed = (Old == null) ? _current == null : !Old.Equals(_current);
            }
        }
        public T Old { get; protected set; }

        public ValueWatcher(T t)
        {
            _current = t;
            Old = t;
        }

        public void Reset(T newVal)
        {
            Old = _current = newVal;
        } 

        public bool ChangedFromTo(T from, T to)
        {
            return 
                (Old == null ? from == null : Old.Equals(from)) && 
                (Current == null ? to == null : Current.Equals(to));
        }

        public bool ChangedTo(T to)
        {
            return
                !(Old == null ? to == null : Old.Equals(to)) &&
                (Current == null ? to == null : Current.Equals(to));
        }

        public bool ChangedFrom(T from)
        {
            return
                (Old == null ? from == null : Old.Equals(from)) &&
                !(Current == null ? from == null : Current.Equals(from));
        }

        public override string ToString()
        {
            return Current.ToString();
        }
    }
}

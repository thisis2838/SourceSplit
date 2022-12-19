using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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

        public ValueWatcher()
        {
            _current = default;
            Old = default;
        }

        public void Reset(T newVal)
        {
            Old = _current = newVal;
        } 

        public bool ChangedFromTo(T from, T to)
        {
            return 
                (Old is null ? from is null : Old.Equals(from)) && 
                (Current is null ? to is null : Current.Equals(to));
        }

        public bool ChangedTo(T to)
        {
            return
                !(Old is null ? to is null : Old.Equals(to)) &&
                (Current is null ? to is null : Current.Equals(to));
        }

        public bool ChangedFrom(T from)
        {
            return
                (Old is null ? from is null : Old.Equals(from)) &&
                !(Current is null ? from is null : Current.Equals(from));
        }

        public override string ToString()
        {
            return Current.ToString();
        }
    }
}
